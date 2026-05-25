//using UnityEngine;
//using UnityEngine.InputSystem;
//using System;

//public class ScreenshotTaker : MonoBehaviour
//{
//    void Update()
//    {
//        if (Keyboard.current.pKey.wasPressedThisFrame)
//        {
//            string fileName = "screenshot_" +
//                              DateTime.Now.ToString("yyyy-MM-dd_HH-mm-ss") +
//                              ".png";

//            ScreenCapture.CaptureScreenshot(fileName);

//            Debug.Log("Saved: " + fileName);
//        }
//    }
//}