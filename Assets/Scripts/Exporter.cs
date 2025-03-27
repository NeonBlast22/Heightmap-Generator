using System.IO;
using UnityEngine;
using System;
using TMPro;

public class Exporter : MonoBehaviour
{
    [SerializeField] TMP_Text outputLog;
    string path;

    private void Start()
    {
        string downloadsPath = "";
        #if UNITY_ANDROID && !UNITY_EDITOR
                downloadsPath = "/storage/emulated/0/Download/";
        #elif UNITY_IOS && !UNITY_EDITOR
                downloadsPath = Path.Combine(Application.persistentDataPath, "..", "Documents");
        #else
                downloadsPath = Path.Combine(Environment.GetFolderPath(Environment.SpecialFolder.UserProfile), "Downloads");
        #endif
        path = downloadsPath;
    }
    public void Export(Texture2D texture)
    {
        string date = DateTime.Now.ToShortDateString().Replace('/', '.');

        string filePath = path + $"\\Export {date}.png";
        if (File.Exists(filePath))
        {
            int i = 1;
            while (File.Exists(path + $"\\Export {date} {i}.png"))
            {
                i++;
                if (i > 10000)
                {
                    outputLog.text = $"Failed to Export: Why do you have over 1000 files in your downloads called Export {date}.png";
                    return;
                }
            }
            filePath = path + $"\\Export {date} {i}.png";
        }
        File.WriteAllBytes(filePath, texture.EncodeToPNG());
        outputLog.text = $"Saved Export at {filePath}";
    }
}
