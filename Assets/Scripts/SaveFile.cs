using System.Collections;
using System.Collections.Generic;
using System;
using System.IO;
using UnityEngine;

public class SaveFile
{
    [Serializable]
    public class SaveData
    {
        public string[] Progress;
    }
    private static readonly string saveFilePath = Application.persistentDataPath + "/progress.txt";

    public static void SaveProgress(List<string> progress)
    {
        SaveData data = new SaveData();
        data.Progress = progress.ToArray();
        string encodedProgress = JsonUtility.ToJson(data);
        try
        {
            File.WriteAllText(saveFilePath, encodedProgress);
        }
        catch (Exception e)
        {
            Debug.LogError(String.Format("SaveProgress: error occured while saving: {0}", e));
        }
    }
    public static List<string> LoadProgress()
    {
        if (File.Exists(saveFilePath))
        {
            try
            {
                string encodedProgress = File.ReadAllText(saveFilePath);
                SaveData data = JsonUtility.FromJson<SaveData>(encodedProgress);
                return new List<string>(data.Progress);
            }
            catch (Exception e)
            {
                Debug.LogError(String.Format("LoadProgress: error occured while loading progress: {0}", e));
            }
        }
        return new List<string>();
    }
}
