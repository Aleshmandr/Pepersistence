using System;
using System.IO;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public static class JsonSaveFileManager
    {
        private const string SavePathFormat = "{0}/{1}.jsav";

        public static T LoadFromFile<T>(string fileName) where T : class
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            T result = null;
            try
            {
                var json = File.ReadAllText(path);
                result = JsonConvert.DeserializeObject<T>(json);
            } catch (Exception e)
            {
                Debug.Log(e);
            }

            return result;
        }

        public static void SaveToFile<T>(T data, string fileName)
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            try
            {
                var json = JsonConvert.SerializeObject(data);
                File.WriteAllText(path, json);
            } catch (Exception e)
            {
                Debug.Log(e);
            }
        }
    }
}