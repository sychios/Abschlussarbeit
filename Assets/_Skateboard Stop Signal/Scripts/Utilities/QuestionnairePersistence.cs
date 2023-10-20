using System;
using System.IO;
using UnityEngine;

public static class QuestionnairePersistence{

    public static void WriteFile(string path, string filename, string data)
    {
        try
        {
            CreateDirectoryRecursively(path);
            // File exists, change filename and call WriteFile
            if (File.Exists(path + filename))
            {
                // Split file name at '.' to get "real" filename
                var name = filename.Split('.')[0];
                name += " Copy.csv";
                WriteFile(path, name, data);
            }
            else
            {
                File.WriteAllText(path + filename, data);
            }
        }
        catch (Exception e)
        {
            string errorMessage = "Write File error:\n" + e.Message;
            Debug.LogError(errorMessage);
            throw;
        }
    }

    public static void WritePicture(string path, string filename, byte[] imageTexture)
    {
        try
        {
            CreateDirectoryRecursively(path);
            File.WriteAllBytes(path + filename, imageTexture);

            return;
        }
        catch (Exception e)
        {
            string errorMessage = "Write .png file error: " + e.Message;
            Debug.LogError(errorMessage);
            throw;
        }
        
        
    }

    public static void CreateDirectoryRecursively(string path)
    {
        string[] junkedPath = path.Split('\\');

        for (int i = 0; i < junkedPath.Length; i++)
        {
            if (i > 0)
            {
                junkedPath[i] = Path.Combine(junkedPath[i - 1], junkedPath[i]);
            }

            if (!Directory.Exists(junkedPath[i]) && junkedPath[i] != "")
            {
                Directory.CreateDirectory(junkedPath[i]);
            }
        }
    }
}
