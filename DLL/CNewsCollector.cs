using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;


namespace TMonitor
{
    using NewsBatch = List<CNewsBit>;

    public delegate void MessageReadyHandler(string pMessage);//pMessage - это JSON.stringify(от любого объекта кый надо передать как сообщение), по сути мессадж это тело сообщения вокруг кого будет обертываться CMessageEnvelope перед отправкой дальше по сети

    //
    //Класс отвечает только за те события кые производяться пользователем или имеют отношение к пользователю (например показ заставки) - как то нажатие клавиш взаимодействие с какими-то элеменатми презентации ит.д. Класс собирает информацию о UserExpirience
    //
    public class CNewsCollector
    {
        public static NewsBatch newsFeed = new NewsBatch();
        public static uint effectiveMessageCount = 20;
        public CNewsCollector()
        {

        }
        static void NewsHandler(ENewsBitType pType, string pPoint, string pDescription = "")
        {
            newsFeed.Add(new CNewsBit(pType, pPoint));
            //if (newsFeed.Count >= effectiveMessageCount)
            //    OnBasketFillOut();
        }

		#region TeleFunctions
		//
		//Различные телеметрические ивенты кые могут быть интересны. Нижестоящие функции это тот интерфейс кый будет раскрыт стороннему разработчику.
		//
		static public void GeneralNews(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.general, pPoint, pDescription);
		}
		static public void ButtonPressed(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.buttonPressed, pPoint, pDescription);
		}
		static public void SplashScreenStart(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.splashScreenStart, pPoint, pDescription);
		}
		static public void SplashScreenEnd(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.splashScreenEnd, pPoint, pDescription);
		}
		static public void ScreenSaverStart(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.screenSaverStart, pPoint, pDescription);
		}
		static public void ScreenSaverEnd(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.screenSaverEnd, pPoint, pDescription);
		}
		static public void ActivityStart(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.activityStart, pPoint, pDescription);
		}
		static public void ActivityEnd(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.activityEnd, pPoint, pDescription);
		}
		static public void ApplicationStart(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.applicationStart, pPoint, pDescription);
		}
		static public void ApplicationEnd(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.applicationEnd, pPoint, pDescription);
		}
		static public void WindowDressing(string pPoint = "wd", string pDescription = "")
		{
			NewsHandler(ENewsBitType.windowDressing, pPoint, pDescription);
		}
		static public void Test(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.test, pPoint, pDescription);
		}
		static public void SaveData(string pPoint, string pDescription = "")
		{
			NewsHandler(ENewsBitType.saveData, pPoint, pDescription);
		}
        static public void Log(string pPoint, string pDescription = "")
        {
            NewsHandler(ENewsBitType.log, pPoint, pDescription);
        }
        #endregion
        static public void ClearNewsFeed()
        {
            newsFeed.Clear();
        }
        //Ниже события
        static public event MessageReadyHandler BasketFlush;
        static void OnBasketFillOut()
        {
            if (BasketFlush != null)
                BasketFlush(JsonConvert.SerializeObject(newsFeed));
            newsFeed.Clear();
        }
    }
}
