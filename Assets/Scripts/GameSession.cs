using System.Collections;
using System.Collections.Generic;
using UnityEngine;

public class GameSession : MonoBehaviour
{
    string path;

    private void Start()
    {
        path = Application.dataPath;
    }
    // Update is called once per frame
    void Update()
    {
        if (Input.GetKeyDown(KeyCode.F1))
        {
            Debug.Log("Screenshot taken");
            ScreenCapture.CaptureScreenshot(path + "Screenshot" + Time.renderedFrameCount + ".png",1);
        }
        if (Input.GetKeyDown(KeyCode.F2))
        {
            Debug.Log("Screenshot taken");
            ScreenCapture.CaptureScreenshot(path + "Screenshot" + Time.renderedFrameCount + ".png", 2);
        }
        if (Input.GetKeyDown(KeyCode.F3))
        {
            Debug.Log("Screenshot taken");
            ScreenCapture.CaptureScreenshot(path + "Screenshot" +Time.renderedFrameCount + ".png", 4);
        }
    }
}
