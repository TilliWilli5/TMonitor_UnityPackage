using System;
using System.Collections.Generic;
using System.Text;
using System.IO;
using Newtonsoft.Json;

namespace TMonitor
{
    public class CAutoLoader
    {
        static string loadedFileName;
        static Dictionary<string, string> configuration;
        static public bool LoadFromFile(string pFileName)
        {
                try
                {
                    configuration = JsonConvert.DeserializeObject<Dictionary<string, string>>(File.ReadAllText(pFileName));
                    loadedFileName = pFileName;
                    return true;
                }
                catch
                {
                    return false;
                }
        }
        static public bool LoadFromText(string pText)
        {
            try
            {
                configuration = JsonConvert.DeserializeObject<Dictionary<string, string>>(pText);
                return true;
            }
            catch
            {
                return false;
            }
        }
        static public Dictionary<string, string> GetConfiguration()
        {
            return configuration;
        }
        static public Dictionary<string, string> GetClonedConfiguration()
        {
            return new Dictionary<string, string>(configuration);
        }
        static public void Set(string pKey, Object pValue)
        {
            if (configuration.ContainsKey(pKey))
                configuration[pKey] = pValue.ToString();
            else
                configuration.Add(pKey, pValue.ToString());
        }
        static public string Get(string pKey)
        {
            if (configuration.ContainsKey(pKey))
                return configuration[pKey];
            else
                return "";
        }
        static public bool CheckKey(string pKey)
        {
            return configuration.ContainsKey(pKey);
        }
        static public bool CheckValue(string pValue)
        {
            return configuration.ContainsValue(pValue);
        }
        static public bool SaveToFile(string pFileName)
        {
            try
            {
                File.WriteAllText(pFileName, JsonConvert.SerializeObject(configuration));
                return true;
            }
            catch
            {
                return false;
            }
        }
        static public bool SaveToFile()
        {
            try
            {
                File.WriteAllText(loadedFileName, JsonConvert.SerializeObject(configuration));
                return true;
            }
            catch
            {
                return false;
            }
        }
    }
}
