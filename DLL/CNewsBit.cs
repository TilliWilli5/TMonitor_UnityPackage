using System;
using System.Collections.Generic;
using System.Text;
using Newtonsoft.Json;

namespace TMonitor
{
    public enum ENewsBitType
    {
        general,//(unstandardized) Для нестандартных ивентов для кых еще не придумано своего АПИ
        buttonPressed,//При нажатие кнопки
        textInput,//Пользователь ввёл какуюто важную инфу типа: свой e-mail, номер телефона, ФИО ит.д. Важность инфу определяет разработчик само собой
        splashScreenStart,//Экран начальной загрузки - инсталяция проигрывает первую заставку
        splashScreenEnd,//Экран начальной загрузки - инсталяция вошла в активный (дееспособный) режим после первой заставки
        screenSaverStart,//Инсталяцией никто не пользовался и она начала проигрывать заставку
        screenSaverEnd,//Произошло активное воздействие на инсталяцию и она вышла из заставки
        activityStart,
        activityEnd,
        eventOccur,
        //Технические ивенты как то: приложение стартануло, приложение заканчивает работу
        applicationStart,
        applicationEnd,
        windowDressing,//События кое служит для подтверждения работы приложения (автогенерируя эту новость приложение создает видимость работы)
        test,
        saveData,
        log
    }

    [JsonObject(MemberSerialization.OptIn)]
    public class CNewsBit
    {
        static uint currentID = 0;
        uint id;
        [JsonProperty]
        public ENewsBitType type;
        [JsonProperty]
        DateTime creatingTime;
        [JsonProperty]
        public string point;
        public string description;
        public CNewsBit(ENewsBitType pType, string pTitle = "", string pDescription = "")
        {
            id = currentID++;
            type = pType;
            creatingTime = DateTime.Now;
            point = pTitle;
            description = pDescription;
        }
        public string ToJSON()
        {
            return JsonConvert.SerializeObject(this);
        }
    }
}
