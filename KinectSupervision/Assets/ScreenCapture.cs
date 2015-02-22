using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class ScreenCapture : MonoBehaviour {

   
	// Use this for initialization
    void Start()
    {
      
	
	}
	
	// Update is called once per frame
	void Update () 
    {
     
	
	}
    #region GUI
    Rect rctWindow = new Rect();
    public GUISkin blueSkin;

    void Awake()
    {
        int width = Screen.width / 3;
        int height = Screen.height / 2;

        rctWindow = new Rect(0, 0, width, height);

    }
    void OnGUI()
    {


        int width = Screen.width / 3;
        int height = Screen.height / 2;

        GUI.skin = blueSkin;

        rctWindow = GUI.Window(0, rctWindow, DoMyWindow, "Alerts", GUI.skin.GetStyle("window"));
    }
    Vector2 scrollpos = new Vector2();


    void DoMyWindow(int windowID)
    {


        GUILayout.BeginVertical();
        scrollpos = GUILayout.BeginScrollView(scrollpos, GUI.skin.GetStyle("Box"));
        //scrollpos = GUILayout.BeginScrollView(scrollpos, GUILayout.Width(width - 10), GUILayout.Height(height-10));
        //scrollpos = GUILayout.BeginScrollView(scrollpos,Gui)
        string text = "";
        foreach (var s in KinectManager.Alerts)
        {
            text += "\n" + s;
        }
        if (text != "")
            GUILayout.TextArea(text);
        GUILayout.EndScrollView();
        //GUILayout.EndScrollView();
        GUILayout.EndVertical();
        GUI.DragWindow();


    }
    #endregion
  
}
