using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Text;
using TMonitor;
using Newtonsoft.Json;
using System.IO;

using UnityEngine.UI;

namespace TMonitor
{
    using SSDictionary = Dictionary<string, string>;
    public class AutoLoader : MonoBehaviour {
        //Unity specific
        public TextAsset configFile;
        //Для класса CAutoLoader

        //Необходимо добавить следующие параметры всюду
        string logDirectory;
        [HideInInspector]
        public string logsFolder = "Logs/";
        CInitiative thePingSignalToServer;
        CInitiative theLogsToServer;
        CInitiative theNewsFeedToLogs;
        CInitiative theWindowDressing;

        CClientInspector inspector;
        bool needToQuit;
        public string highPriorityConf = "storage/emulated/0/conf.json";
        public bool checkTokenExceptionMode = false;

        #region AutoLoaderEditorParameters
        //Настройки для AutoLoaderEditor отрисовщика
        [HideInInspector]
        public SSDictionary configuration;
        [HideInInspector]
        public bool useFile = false;
        [HideInInspector]
        public string fileContent;
        [HideInInspector]
        public bool errorLoadingFile = false;
        [HideInInspector]
        public string confPath = "TMonitor/conf.json";
        [HideInInspector]
        public string fullPath;
        [HideInInspector]
        public bool showConfFileGroup = true;
        [HideInInspector]
        public bool showBaseGroup = true;
        [HideInInspector]
        public int dataChangedCounter = 0;
        #endregion
        // Use this for initialization
        void Start () {
            //Text memo = GameObject.Find("DebugMemo").GetComponent<Text>();
            //Как только приложение стартует добавляем первую новость о том что приложение запустилось
            CNewsCollector.ApplicationStart("mark!AutoLoader");
            CNewsCollector.SaveData(JsonConvert.SerializeObject(AppInfoGrabber.ApplicationInfo()));
            //Debug.Log("Application.dataPath: " + Application.dataPath);
            //Debug.Log("Application.dataPath: " + Application.persistentDataPath);
            //Составляем полный путь к файлу настроек
            //string pathNameToConf = Application.dataPath + "/TMonitor/" + configurationFileName;
            //Составляем полный путь к директории куда будут сохраняться файлы логов
            logDirectory = Application.persistentDataPath + "/" + logsFolder;//Необходимо дописать кастомизацию
            //Небольшой хак для случая когда скрипт открывается в эдиторе на виндоус
            //ClearLogsHack(logDirectory);
                //Меняем путь на / только для Windows платформы. Файл должен находиться в корне системы, по умолчанию это C:\
            #if (UNITY_EDITOR_WIN || UNITY_STANDALONE_WIN)
                highPriorityConf = "/conf.json";
            #endif
            //Используем класс автозагрузки для получения всех интересующих нас параметров для дальнейшей работы остальных модулей
            if (File.Exists(highPriorityConf))
            {
                //memo.text += "conf.json from root" + "\n";
                CNewsCollector.SaveData("configuration will be loaded from /conf.json file");
                if (CAutoLoader.LoadFromFile(highPriorityConf) == false)
                {
                    Debug.Log("Error loading config file.");
                    //memo.text += "Error loading root file." + "\n";
                }
            }
            else
            {
                //memo.text += "conf.json from TextAsset" + "\n";
                if (CAutoLoader.LoadFromText(configFile.text) == false)
                {
                    Debug.Log("Error loading config file.");
                }
            }
            //Входные параметры от скрипта имеют больший приоритет посему делаем подмену там где это надо
            //Дальнейшая работа должна происходить исключительно с параметрами из словаря setup
            Dictionary<string, string> setup = CAutoLoader.GetClonedConfiguration();
            if (logDirectory != null)
                setup["logDirectory"] = logDirectory;
            //memo.text += "address: " + setup["serverAddress"] + "\n";
            //Debug.Log(setup["serverAddress"]);
            //Дальше создаём моего любимого инспектора (ака "the Boss")
            //Инициализируем с помощью словаря setup
            inspector = new CClientInspector(setup["installationToken"], setup["installationDescription"]);
            inspector.Initialize(setup);
            //Создаем и настраиваем Почтальона кый берет на себе весь гемор работы с сетью
            CClientPostman thePostman = new CClientPostman();
            thePostman.Initialize(setup);
            //Создаем и настраиваем класс Архиватора кый отвечает за локальное хранение данных
            CLogger theLogger = new CLogger();
            theLogger.Initialize(setup);
            //Добавляем почтальона как дефолтную почтовую службу к Инспектору.
            //Добавляем Архиватора как дефлотную службу локального хранения данных
            inspector.postalService = thePostman;
            inspector.archiveService = theLogger;
            //Если передача данных не удалась что делать? Вешаем обработчики для такой ситуации
            thePostman.ErrorDelivery += new ErrorDeliveryHandler(inspector.MessageReturn);//Необходимо продумать ситуацию в кой данные передаются из уже записанного лог файла и следовательно повторно архивировать в случае не удачи их не нужно
            //Если приходит сообщение о закрытии выходим из приложения
            inspector.QuitOrder += new IncomingOrderDelegate(() =>{
                needToQuit = true;
            });
            //Проверяем заданный токен инсталяции, есть ли такой в БД на сервере - чтоб удостовериться что Телеметрия бдует принематься по данной инсталяции.
            //Если такой ключ не зарегестрирован то кидаем исключение в качестве напоминания разработчику что надо обновить ключ епт
            inspector.CheckInstallationToken(checkTokenExceptionMode);
            //Далее создаются инициативы кые будут испольняться через промежутки времени
            int pingInterval = Convert.ToInt32(setup["initPingInterval"]);
            int logsToServerInterval = Convert.ToInt32(setup["initLogsToServerInterval"]);
            int newsFeedToLogsInterval = Convert.ToInt32(setup["initNewsFeedToLogsInterval"]);
            int windowDressingInterval = Convert.ToInt32(setup["initWindowDressingInterval"]);
            thePingSignalToServer = new CInitiative(inspector.SendPingSignalToServer, pingInterval * 1000, true, true, "Ping 1m", "Каждую минуту отсылает Пинг-сигнал на сервер");
            theLogsToServer = new CInitiative(inspector.SendLogsToServer, logsToServerInterval * 1000, true, true, "SendingLogs 1h", "Отсылка статистики из логов на сервер");
            theNewsFeedToLogs = new CInitiative(inspector.NewsFeedToLogs, newsFeedToLogsInterval * 1000, true, true, "NewsFeedToLogs 15m", "Забираем данные из ленты новостей и помещаем в логи, после чего очищаем ленту новостей");
            theWindowDressing = new CInitiative(inspector.WindowDressing, windowDressingInterval * 1000, true, true, "WindowDressing 1m", "Заполняем Ленту новостей чем-нибудь. Зондирующие сигналы");
        }
	    public void OnDestroy()
        {
            if(thePingSignalToServer != null)
                thePingSignalToServer.Destroy();
            if(theLogsToServer != null)
                theLogsToServer.Destroy();
            if(theNewsFeedToLogs != null)
                theNewsFeedToLogs.Destroy();
            if(theWindowDressing != null)
                theWindowDressing.Destroy();
            //Приложение заканчивает свою работу. Генерируем новость об этом. Эта строчка должна оставаться последней
            CNewsCollector.ApplicationEnd("mark!AutoLoader");
            if (inspector != null)
            {
                inspector.NewsFeedToLogs();
                inspector.SendLogsToServer();
            }
            //ClearLogsHack(logDirectory);
        }
	    // Update is called once per frame
	    void Update () {
            if(needToQuit)
            {
                #if UNITY_EDITOR
                    UnityEditor.EditorApplication.isPlaying = false;
                #else
                    Application.Quit();
                #endif
            }
        }
        //
        //Public interface to CNewsCollector Methods
        //
        public void ButtonPressed(string pPoint)
        {
            CNewsCollector.ButtonPressed(pPoint);
        }
        //Самые первые действия
        void ClearLogsHack(string pLogDirectory)
        {
            #if UNITY_EDITOR
            if(Directory.Exists(pLogDirectory))
            {
                string[] files = Directory.GetFiles(pLogDirectory);
                foreach(var file in files)
                {
                    File.Delete(file);
                }
            }
            #endif
        }
    }

}

