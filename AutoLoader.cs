using UnityEngine;
using System.Collections;

using System;
using System.Collections.Generic;
using System.Text;
using TMonitor;
using Newtonsoft.Json;
using System.IO;

public class AutoLoader : MonoBehaviour {
    //Unity specific
    public TextAsset configFile;
    //Для класса CAutoLoader
    public string configurationFileName = "conf.json";
    //
    public string serverAddress = "192.168.1.4";
    public string httpMethod = "POST";
    public string httpContentType = "application/json";
    public int correspondenceFrequency;
    public string installationToken;
    public string installationDescription;
    //Необходимо добавить следующие параметры всюду
    public string scheme = "http://";
    public uint port = 80;
    public uint requestTimeout = 1000;
    //Дописать кастомизацию (из скрипта CLogger)
    public string logDirectory = "Logs/";
    public string defaultFileName = "log";
    public string prefixToFileName;
    public string postfixToFileName;
    public string timeFormatString = "[yyyy-MM-ddTHH]";
    public string fileExtension = "nfl";//.nfl - news feed log
    CInitiative thePingSignalToServer;
    CInitiative theLogsToServer;
    CInitiative theNewsFeedToLogs;
    CInitiative theWindowDressing;

    CClientInspector inspector;
    // Use this for initialization
    void Start () {
        //Как только приложение стартует добавляем первую новость о том что приложение запустилось
        CNewsCollector.ApplicationStart("mark!AutoLoader");
        Debug.Log("Application.dataPath: " + Application.dataPath);
        Debug.Log("Application.dataPath: " + Application.persistentDataPath);
        //Составляем полный путь к файлу настроек
        string pathNameToConf = Application.dataPath + "/TMonitor/" + configurationFileName;
        //Составляем полный путь к директории куда будут сохраняться файлы логов
        logDirectory = Application.persistentDataPath + "/Logs/";//Необходимо дописать кастомизацию
        //Используем класс автозагрузки для получения всех интересующих нас параметров для дальнейшей работы остальных модулей
        if (CAutoLoader.LoadFromText(configFile.text) == false)
        {
            //Console.BackgroundColor = ConsoleColor.DarkRed;
            Debug.Log("Error loading config file.");
            //Console.BackgroundColor = ConsoleColor.Black;
        }
        //Входные параметры от скрипта имеют больший приоритет посему делаем подмену там где это надо
        //Дальнейшая работа должна происходить исключительно с параметрами из словаря setup
        Dictionary<string, string> setup = CAutoLoader.GetClonedConfiguration();
        if (serverAddress != null)
            setup["serverAddress"] = serverAddress;
        if (httpMethod != null)
            setup["httpMethod"] = httpMethod;
        if (httpContentType != null)
            setup["httpContentType"] = httpContentType;
        if (correspondenceFrequency != 0)
            setup["correspondenceFrequency"] = correspondenceFrequency.ToString();
        if (installationToken != null)
            setup["installationToken"] = installationToken;
        if (installationDescription != null)
            setup["installationDescription"] = installationDescription;
        if (logDirectory != null)
            setup["logDirectory"] = logDirectory;
        //Дальше создаём моего любимого инспектора (ака "the Boss")
        //Инициализируем с помощью словаря setup
        inspector = new CClientInspector(setup["installationToken"], setup["installationDescription"]);
        inspector.Initialize(setup);
        //Проверяем заданный токен инсталяции, есть ли такой в БД на сервере - чтоб удостовериться что Телеметрия бдует принематься по данной инсталяции.
        //Если такой ключ не зарегестрирован то кидаем исключение в качестве напоминания разработчику что надо обновить ключ епт
        inspector.CheckInstallationToken();
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
                                                                                         //Создаем инициативу кая будет каждую минуту слать Пинг-покет на сервер
        thePingSignalToServer = new CInitiative(inspector.SendPingSignalToServer, 1 * 3 * 1000, true, true, "Ping 1m", "Каждую минуту отсылает Пинг-сигнал на сервер");
        theLogsToServer = new CInitiative(inspector.SendLogsToServer, 1 * 7 * 1000, true, true, "SendingLogs 1h", "Отсылка статистики из логов на сервер");
        theNewsFeedToLogs = new CInitiative(inspector.NewsFeedToLogs, 1 * 3 * 1000, true, true, "NewsFeedToLogs 15m", "Забираем данные из ленты новостей и помещаем в логи, после чего очищаем ленту новостей");
        theWindowDressing = new CInitiative(inspector.WindowDressing, 1 * 1 * 1000, true, true, "WindowDressing 1m", "Заполняем Ленту новостей чем-нибудь. Зондирующие сигналы");
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
        inspector.NewsFeedToLogs();
        inspector.SendLogsToServer();
    }
	// Update is called once per frame
	void Update () {

    }
    //
    //Public interface to CNewsCollector Methods
    //
    public void ButtonPressed(string pPoint)
    {
        CNewsCollector.ButtonPressed(pPoint);
    }
}
