using System;
using System.Collections.Generic;
using System.Text;
using System.Timers;
using Newtonsoft.Json;
using System.IO;


namespace TMonitor
{
    using SSDictionary = Dictionary<string, string>;
    public delegate void IncomingOrderDelegate();
    //
    //the Boss class. Один Инстпектор на одну интерактивную инсталяцию. Реализовать singleton - данный класс должен быть один в инсталяции
    //Класс должен заниматься упаковкой писем, подписыванием ит.д. Он должен с некоторой переодичностью ходить и собирать различную инфу с класса NewsCollector
    //Этот классом является связующим звеном со всеми остальными, является ИНИЦИАТОРОМ всех процессов.
    //Письма пишет этот класс, т.е. он является инициатором создания КонвертовПисьма
    //
    [JsonObject(MemberSerialization.OptIn)]
    public class CClientInspector
    {
        public static string console = "";
        [JsonProperty]
        public string installationToken;//Токен инсталяции полученный на сервере-авторизации заранее например через Веб-интерфейс или просто сгенерированный и переданный уполномеченным человеком
        [JsonProperty]
        public string installationDescription;//Текстовое описание инсталяции для того чтобы человеку понимать с какой инсталяции приходят сообщения
        public CPostalService postalService;
        public CArchiveService archiveService;

        [JsonProperty]
        Guid sessionID; // ИД сессии будет отправляться каждый раз со всеми сообщениями - таким образом сравнивая на стороне сессии с последним ГУИ мы можем понимать когда происходили перезагрузки
        DateTime inspectorCreatingTime;

        /*
        public CClientInspector()
        {
            inspectorCreatingTime = DateTime.Now;
            sessionID = Guid.NewGuid();
        }
        */
        public CClientInspector(string pToken, string pDescription = "")
        {
            installationToken = pToken;
            installationDescription = pDescription;
            inspectorCreatingTime = DateTime.Now;
            sessionID = Guid.NewGuid();
        }
        public bool Initialize(Dictionary<string, string> pConfiguration)
        {
            try
            {
                installationToken = pConfiguration["installationToken"];
                installationDescription = pConfiguration["installationDescription"];
                return true;
            }
            catch
            {
                return false;
            }
        }
        public bool CheckInstallationToken(bool pExceptionMode = false)
        {
            string response = SendCommandToServer("checkToken");
            if(pExceptionMode)
            {
                if(response == "valid")
                {
                    return true;
                }
                else
                {
                    throw new Exception("Invalid installation token");
                }
            }
            else
            {
                if (response == "valid")
                {
                    return true;
                }
                else
                {
                    return false;
                }
            }
            
            /*
            if(pExceptionMode)
            {
                throw new Exception("Installation token is wrong. Revalidate and get new token.");
            }
            else
            {
                return true;
            }
            */
        }
        public void MessageReturn(CMessageEnvelope pMessageEnvelope)
        {
            archiveService.ArchiveOfNewsFeed(pMessageEnvelope.message);
        }
        public void SendPingSignalToServer()
        {
            //Console.WriteLine("[tilli]: SendPingSignalToServer start");
            string response = postalService.SendPing("ping", JsonConvert.SerializeObject(this));
            //if (response != "pong")
            //    _OnIncomingOrder(response);
            //Console.WriteLine("[tilli]: SendPingSignalToServer start");
        }
        public void SendLogsToServer()
        {
            //Console.WriteLine("[log2serv]:[SendLogsToServer]:started");
            //Воспользовавшись функционалом Архиватора мы получим словарь с ключами ввиде имен файлов и значениями ввиде содержимого файлов
            Dictionary<string, string> theFileCollection = archiveService.ExtractAllArchives();
            foreach (KeyValuePair<string, string> file in theFileCollection)
            {
                bool requestResult = postalService.SendLogs(file.Value, JsonConvert.SerializeObject(this));
                if(requestResult)
                {
                    //Console.WriteLine("[log2serv]:[SendLogsToServer]:before-delete-file");
                    File.Delete(file.Key);
                }
            }
            //Console.WriteLine("[log2serv]:[SendLogsToServer]:finished-successfully");
        }
        public string SendCommandToServer(string pCommand)
        {
            string response = postalService.SendCommand(pCommand, JsonConvert.SerializeObject(this));
            return response;
        }
        public void NewsFeedToLogs()
        {
            archiveService.ArchiveOfNewsFeed(CNewsCollector.newsFeed.ToArray());
            CNewsCollector.ClearNewsFeed();
        }
        public void WindowDressing()
        {
            CNewsCollector.WindowDressing();
            //CNewsCollector.ButtonPressed("unknown button");
        }
        public event IncomingOrderDelegate QuitOrder;
        public void _OnIncomingOrder(string pResponse)
        {
            SSDictionary response = JsonConvert.DeserializeObject<SSDictionary>(pResponse);
            if (response.ContainsKey("order"))
            {
                if (response["order"] == "quit")
                {
                    QuitOrder?.Invoke();
                }
            }
        }
    }
}
