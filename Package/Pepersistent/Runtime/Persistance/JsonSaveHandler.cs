using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public static class JsonSaveHandler
    {
        private const string SavePathFormat = "{0}/{1}.jsav";
        private const string TempSavePathFormat = "{0}/{1}.jsav.tmp";

        public static T LoadFromFile<T>(string fileName, string encryptionKey = null) where T : class
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            T result = null;
            try
            {
                var json = File.ReadAllText(path);
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    json = XorEncryption.EncryptDecrypt(json, encryptionKey);
                }

                result = JsonConvert.DeserializeObject<T>(json);
            }
            catch (FileNotFoundException foundException)
            {
                Debug.Log(foundException);
            } 
            catch (Exception e)
            {
                Debug.LogException(e);
            }

            return result;
        }

        public static void SaveToFile<T>(T data, string fileName, string encryptionKey = null)
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            var tempPath = string.Format(TempSavePathFormat, Application.persistentDataPath, fileName);
            try
            {
                var json = JsonConvert.SerializeObject(data);
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    json = XorEncryption.EncryptDecrypt(json, encryptionKey);
                }
                
                // Write to a temporary file first to prevent save corruption if the disk is full
                File.WriteAllText(tempPath, json);
                
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
                
                File.Move(tempPath, path);
            } 
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}