using UnityEngine;
using System.Collections;
using UnityEditor;
using System.Collections.Generic;
using System.IO;
using Newtonsoft.Json;

namespace TMonitor
{
    using SSDictionary = Dictionary<string, string>;
    [CustomEditor(typeof(AutoLoader))]
    [System.Serializable]
    [CanEditMultipleObjects]
    public class AutoLoaderEditor : Editor
    {
        AutoLoader targetAL;
        //public Dictionary<string, string> targetAL.configuration;
        //[SerializeField]
        //public SSDictionary targetAL.configuration;
        //[SerializeField]
        //public bool targetAL.useFile = false;
        //[SerializeField]
        //public string targetAL.fileContent;
        //[SerializeField]
        //public bool targetAL.errorLoadingFile = false;
        //[SerializeField]
        //public string targetAL.confPath = "TMonitor/conf.json";
        //[SerializeField]
        //public string targetAL.fullPath;
        //[SerializeField]
        //public bool targetAL.showConfFileGroup = true;
        //[SerializeField]
        //public bool targetAL.showBaseGroup = true;
        //[SerializeField]
        public Texture2D blankTexture;
        //[SerializeField]
        //public int targetAL.dataChangedCounter = 0;
        public override void OnInspectorGUI()
        {
            //Задаем наш таргет
            targetAL = (AutoLoader)target;
            //Продолжаем работу с указанным таргетом
            targetAL.fullPath = Application.dataPath + "/" + targetAL.confPath;
            targetAL.showConfFileGroup = EditorGUILayout.Foldout(targetAL.showConfFileGroup, "-------[Edit conf.json file]-------");
            if (targetAL.showConfFileGroup)
            {
                //Если нажали кнопку Загрузить конфигуровочный файл - мы берем файл conf.json кый должен находиться по стандартному пути TMonitor/conf.json и считываем весь стафф
                DrawLoadButton();
                //В случае ошибки выводим предупреждение
                if (targetAL.errorLoadingFile)
                    EditorGUILayout.HelpBox("error loading file: " + targetAL.fullPath, MessageType.Error);
                //Если файл загрузился то кажем его контент
                if (targetAL.useFile)
                {
                    EditorGUILayout.HelpBox("Loaded from file", MessageType.None);
                    //DrawDictionary возвращает true в том случае если контент в одном из полей был поменян в таком случае мы кажем кнопку сохранить изменения
                    if (DrawDictionary(targetAL.configuration))
                        ++targetAL.dataChangedCounter;
                    //Если targetAL.dataChangedCounter индикатор оказался заплюсован - значит изменения были выводим соответсвующую кнопку
                    if (targetAL.dataChangedCounter > 0)
                        DrawSaveButton();
                }
            }
            //Дальше стандартная прорисовка остальных частей скрипта
            targetAL.showBaseGroup = EditorGUILayout.Foldout(targetAL.showBaseGroup, "-------[Base parameters]-------");
            if(targetAL.showBaseGroup)
            {
                base.OnInspectorGUI();
            }
        }
        //DrawDictionary возвращает true в том случае если контент в одном из полей был поменян
        bool DrawDictionary(SSDictionary pDictionary)
        {
            bool changed = false;
            if (pDictionary != null)
            {
                string[] keys = new string[pDictionary.Count];
                string[] newValues = new string[pDictionary.Count];
                int iIndex = 0;
                foreach (KeyValuePair<string, string> pair in pDictionary)
                {
                    keys[iIndex] = pair.Key;
                    newValues[iIndex] = EditorGUILayout.DelayedTextField(pair.Key, pair.Value);
                    ++iIndex;
                }
                for (int iX = 0; iX < keys.Length; ++iX)
                {
                    if (pDictionary[keys[iX]] != newValues[iX])
                        changed = true;
                    pDictionary[keys[iX]] = newValues[iX];
                }
            }
            return changed;
        }
        void DrawLoadButton()
        {
            if (targetAL.useFile)
            {
                if (GUILayout.Button("Unload"))
                {
                    targetAL.useFile = false;
                    targetAL.dataChangedCounter = 0;
                }
            }
            else
            {
                if (GUILayout.Button("Load"))
                {
                    try
                    {
                        targetAL.fileContent = File.ReadAllText(targetAL.fullPath);
                        targetAL.configuration = JsonConvert.DeserializeObject<SSDictionary>(targetAL.fileContent);
                        targetAL.errorLoadingFile = false;
                        targetAL.useFile = true;
                        targetAL.dataChangedCounter = 0;
                    }
                    catch
                    {
                        targetAL.errorLoadingFile = true;
                    }
                }
            }
            
        }
        void DrawSaveButton()
        {
            if (GUILayout.Button("Save changes"))
            {
                File.WriteAllText(targetAL.fullPath, JsonConvert.SerializeObject(targetAL.configuration));
                Debug.Log("In file: " + targetAL.fullPath + " has been written new data:\n" + JsonConvert.SerializeObject(targetAL.configuration));
                targetAL.dataChangedCounter = 0;
            }
        }
    }

}
