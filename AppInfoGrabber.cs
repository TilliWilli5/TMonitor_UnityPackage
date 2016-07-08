using UnityEngine;
using System.Collections;
using Newtonsoft.Json;
using System.Collections.Generic;
using System.ComponentModel;
using System;
using System.Reflection;
using UnityEngine.UI;

public class AppInfoGrabber : MonoBehaviour {
	public bool toDebug;
	public bool toConsole;
	public bool toFile;
	public bool toObject;

	public GameObject display;
	// Use this for initialization
	void Start () {
        var info = ApplicationInfo();
		string log = "";
		foreach(KeyValuePair<string, string> kvp in info)
		{
			log += kvp.Key + ": " + kvp.Value + "\n";
		}
		Debug.Log(log);
		display.GetComponent<Text>().text = log;
	}
	
	// Update is called once per frame
	void Update () {
	
	}
	
	public static Dictionary<string, string> ApplicationInfo()
	{
		Dictionary<string, string> _result = new Dictionary<string, string>();
		Type type = typeof(Application);
		foreach(PropertyInfo property in type.GetProperties())
		{
			_result.Add(property.Name, property.GetValue(null, null).ToString());
		}
		return _result;
	}
}
