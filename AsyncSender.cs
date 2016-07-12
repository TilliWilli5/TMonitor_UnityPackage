using UnityEngine;
using System.Collections;
using System.Collections.Generic;

public class AsyncSender : MonoBehaviour {

	// Use this for initialization
	void Start () {
	
	}
	
	// Update is called once per frame
	void Update () {
	
	}
    public void SendAsyc(string pURL, byte[] pData, Dictionary<string, string> pHeaders)
    {
        WWW www = new WWW(pURL, pData, pHeaders);
        StartCoroutine(WaitWWW(www));
        //while (!www.isDone) { };
        //return www.text;
    }
    IEnumerator WaitWWW(WWW www)
    {
        //WWW www = new WWW(pURL, pData, pHeaders);
        yield return www;
    }
}
