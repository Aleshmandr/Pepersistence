using System;
using System.IO;
using System.Text.RegularExpressions;
using UnityEditor;
using UnityEngine;

namespace Pepersistence.Editor
{
    public static class EditorSaveManager
    {
        private const string SaveFilesFilter = @"^.*sav$";

        [MenuItem("Edit/Pepersistence/Open Persistent Data Path")]
        public static void OpenPersistentDataPath()
        {
            EditorUtility.RevealInFinder(Application.persistentDataPath);
        }

        [MenuItem("Edit/Pepersistence/Clear All Saves")]
        public static void ClearLocalSavedData()
        {
            // Get the path to the persistent data directory
            string dataPath = Application.persistentDataPath;

            // Check if the directory exists
            if (Directory.Exists(dataPath))
            {
                // Get all files in the directory
                string[] files = Directory.GetFiles(dataPath);
                if (files.Length == 0)
                {
                    Debug.Log("No saves found");
                    return;
                }

                // Delete files that match the pattern
                foreach (var file in files)
                {
                    if (Regex.IsMatch(Path.GetFileName(file), SaveFilesFilter))
                    {
                        try
                        {
                            File.Delete(file);
                            Debug.Log($"Save deleted: {file}");
                        }
                        catch (Exception ex)
                        {
                            Debug.LogError($"Error deleting save file {file}: {ex.Message}");
                        }
                    }
                }
            }
            else
            {
                Debug.LogError("Persistent data directory does not exist.");
            }
        }
    }
}