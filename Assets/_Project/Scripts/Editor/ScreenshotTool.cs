using System.IO;
using UnityEditor;
using UnityEngine;

namespace MiniFactory.Editor
{
    public static class ScreenshotTool
    {
        [MenuItem("Tools/Take Screenshot _F12")]
        public static void CaptureScreenshot()
        {
            Directory.CreateDirectory("Screenshots");
            string path = "Screenshots/gameplay.png";
            ScreenCapture.CaptureScreenshot(path);
            Debug.Log($"[ScreenshotTool] Скриншот сохранен: {Path.GetFullPath(path)}");
        }
    }
}
