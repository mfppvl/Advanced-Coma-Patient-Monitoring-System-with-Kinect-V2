using UnityEngine;
using System;
using System.IO;
using System.Collections.Generic;

public class GuiCamera : MonoBehaviour {

   
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
    Rect rctWindow2 = new Rect();
    public GUISkin blueSkin;

    void Awake()
    {
        int width = Screen.width / 3;
        int height = Screen.height / 2;

        rctWindow = new Rect(0, 0, width, height);
        rctWindow2 = new Rect(0, height, width, height);

    }
    void OnGUI()
    {


      

        GUI.skin = blueSkin;

        rctWindow = GUI.Window(0, rctWindow, AlertWindow, "Alerts", GUI.skin.GetStyle("window"));
        rctWindow2 = GUI.Window(1, rctWindow2, VideoWindows, "Alert video", GUI.skin.GetStyle("window"));
    }
    Vector2 scrollpos = new Vector2();
    Vector2 scrollpos2 = new Vector2();


    void AlertWindow(int windowID)
    {


        GUILayout.BeginVertical();
        scrollpos = GUILayout.BeginScrollView(scrollpos, GUI.skin.GetStyle("Box"));        
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
        //GUI.DragWindow();


    }

    void VideoWindows(int windowID)
    {
        GUILayout.BeginVertical();
        scrollpos2 = GUILayout.BeginScrollView(scrollpos2, GUI.skin.GetStyle("Box"));
        //scrollpos = GUILayout.BeginScrollView(scrollpos, GUILayout.Width(width - 10), GUILayout.Height(height-10));
        //scrollpos = GUILayout.BeginScrollView(scrollpos,Gui)
        string text = "";
        if (KinectManager.videoRecorder != null)
        {
            GUILayout.Label("Video recordering ...");
        }
        foreach (var s in KinectManager.VideoFiles)
        {
            if (GUILayout.Button(s.Key))
            {
                Debug.Log("Try to play video "+s.Value);
                System.Diagnostics.Process.Start(s.Value);
            }
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
