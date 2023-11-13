using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public static class JsonSaveHandler
    {
        private const string SavePathFormat = "{0}/{1}.jsav";

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
            } catch (Exception e)
            {
                Debug.Log(e);
            }

            return result;
        }

        public static void SaveToFile<T>(T data, string fileName, string encryptionKey = null)
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            try
            {
                var json = JsonConvert.SerializeObject(data);
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    json = XorEncryption.EncryptDecrypt(json, encryptionKey);
                }
                File.WriteAllText(path, json);
            } catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}