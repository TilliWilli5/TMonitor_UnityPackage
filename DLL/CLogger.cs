using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace TMonitor
{
    //
    //Базовый класс для различных Архиваторов, систем локального хранения данных
    //
    abstract public class CArchiveService
    {
        public abstract bool ArchiveOfNewsFeed(string pNewsFeed);
        public abstract bool ArchiveOfNewsFeed(CNewsBit[] pNewsFeed);
        public abstract bool ArchiveOfNewsFeed(CMessageEnvelope pMessageEnvelope);
        public abstract Dictionary<string, string> ExtractAllArchives();
    }
    public class CLogger : CArchiveService
    {
        //Дописать кастомизацию к следующим переменным
        public string logDirectory;
        public string defaultFileName = "log";
        public string prefixToFileName;
        public string postfixToFileName;
        public string timeFormatString = "[yyyy-MM-ddTHH]";
        public string fileExtension = "nfl";//.nfl - news feed log
        //public string
        //public string
        public CLogger() { }
        public bool Initialize(Dictionary<string, string> pConfiguration)
        {
            try
            {
                logDirectory = pConfiguration["logDirectory"];
                //defaultFileName = pConfiguration["defaultFileName"];
                //prefixToFileName = pConfiguration["prefixToFileName"];
                //postfixToFileName = pConfiguration["postfixToFileName"];
                //timeFormatString = pConfiguration["timeFormatString"];
                //fileExtension = pConfiguration["fileExtension"];
                return true;
            }
            catch
            {
                return false;
            }
        }
        public override bool ArchiveOfNewsFeed(string pNewsFeed)//pNewsFeed - это JsonConvert.SerializeObject(newsFeed). json строка от массива CNewsBit[]
        {
            //Проверяем создана ли папка с логами, если нет то создаем
            if (!Directory.Exists(logDirectory))
                Directory.CreateDirectory(logDirectory);
            //Создаем текущий таймстамп
            DateTime now = DateTime.Now;
            //Составляем имя-путь к необходимому файлу логов с учетом созданного времени
            string fileName = logDirectory + defaultFileName + now.ToString(timeFormatString) + "." + fileExtension;
            //Проверяем существует ли файл с таким именем уже - если да то мы дописываем в конец файла если нет то мы создаем новый файл
            if(File.Exists(fileName))
            {
                try
                {
                    string appendData = "," + pNewsFeed.Substring(1);
                    UTF8Encoding utf8 = new UTF8Encoding();
                    FileStream fs = File.OpenWrite(fileName);
                    fs.Seek(-1, SeekOrigin.End);
                    try
                    {
                        fs.Write(utf8.GetBytes(appendData), 0, appendData.Length);
                    }
                    finally
                    {
                        fs.Close();
                        fs.Dispose();
                    }
                    return true;
                }
                catch
                {
                    return false;
                }
            }
            else
            {
                File.WriteAllText(fileName, pNewsFeed);
            }
            return true;
        }
        public override bool ArchiveOfNewsFeed(CNewsBit[] pNewsFeed)
        {
            return ArchiveOfNewsFeed(JsonConvert.SerializeObject(pNewsFeed));
            //throw new NotImplementedException("ArchiveOfNewsFeed");
        }
        public override bool ArchiveOfNewsFeed(CMessageEnvelope pMessageEnvelope)
        {
            return ArchiveOfNewsFeed(pMessageEnvelope.message);
            //throw new NotImplementedException("ArchiveOfNewsFeed");
        }
        public override Dictionary<string, string> ExtractAllArchives()
        {
            //Console.WriteLine("[log2serv]:[ExtractAllArchives]:started");
            Dictionary<string, string> _result = new Dictionary<string, string>();
            if (Directory.Exists(logDirectory))
            {
                string[] files = Directory.GetFiles(logDirectory);
                for (int iX = 0; iX < files.Length; ++iX)
                {
                    //Console.WriteLine("[log2serv]:[ExtractAllArchives]:file" + iX + ": " + files[iX]);
                    _result.Add(files[iX], File.ReadAllText(files[iX]));
                }
            }
            //Console.WriteLine("[log2serv]:[ExtractAllArchives]:finished-successfully");
            return _result;
        }
    }
}
