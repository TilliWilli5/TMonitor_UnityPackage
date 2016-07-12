using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TMonitor
{
    using MessageEnvelopeBatch = List<CMessageEnvelope>;
    public enum EMessageType
    {
        GENERAL,
        UPTIME,
        NEWS,//Сообщения создающиеся CClientInspector'om на основе данных от CNewsCollector'a 
    }
    //
    //Класс конвертов. Конверты создаются и с ними работает класс CClientPostamn'om. 
    //
    [JsonObject(MemberSerialization.OptIn)]
    public class CMessageEnvelope
    {
        static uint currentTick = 0;
        [JsonProperty]
        uint tick;
        [JsonProperty]
        DateTime sendingTime;
        [JsonProperty("signature")]
        string _signature;
        public string signature { get { return _signature; } }
        [JsonProperty("message")]
        string _message;//JSON'ed мессадж
        public string message { get { return _message; } }

        public CMessageEnvelope(string pCorrespondence, string pSignature)
        {
            tick = currentTick++;
            sendingTime = DateTime.Now;
            _message = pCorrespondence;
            _signature = pSignature;
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    //
    //Класс писем. Письма создаются(aka пишуться) CCLientInspector'om
    //
    /*
    public class CMessage
    {
        [JsonProperty]
        DateTime messageCreatingTime;
        [JsonProperty]
        EMessageType messageType;//Тип сообщения для того что бы сервер знал что за сообщение ему пришло и какой обработчик использовать
        [JsonProperty]
        public string installationToken;//aka токен инсталяции - идентифицирующий инсталяцию
        [JsonProperty]
        public string installationDescription;//Текстовое описание инсталяции для того чтобы человеку понимать с какой инсталяции приходят сообщения
        [JsonProperty]
        public string content;
        public CMessage(EMessageType pMessageType, string pInstallationToken, string pInstallationDescription, string pContent)
        {
            messageCreatingTime = DateTime.Now;
            messageType = pMessageType;
            installationToken = pInstallationToken;
            installationDescription = pInstallationDescription;
            content = pContent;
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
    */
}
