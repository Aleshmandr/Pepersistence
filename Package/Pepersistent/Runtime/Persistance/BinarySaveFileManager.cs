using System;
using System.IO;
using System.Runtime.Serialization.Formatters.Binary;
using UnityEngine;

namespace Pepersistence
{
    public static class BinarySaveFileManager
    {
        private const string SavePathFormat = "{0}/{1}.bsav";

        public static T LoadFromFile<T>(string fileName) where T : class
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            T result = null;
            FileStream fs = null;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                fs = new FileStream(path, FileMode.Open);
                result = bf.Deserialize(fs) as T;
            } catch (Exception e)
            {
                Debug.Log(e);
            } finally
            {
                fs?.Close();
            }

            return result;
        }

        public static void SaveToFile<T>(T data, string fileName)
        {
            var path = string.Format(SavePathFormat, Application.persistentDataPath, fileName);
            FileStream fs = null;
            try
            {
                BinaryFormatter bf = new BinaryFormatter();
                fs = new FileStream(path, FileMode.OpenOrCreate);
                bf.Serialize(fs, data);
            } catch (Exception e)
            {
                Debug.Log(e);
            } finally
            {
                fs?.Close();
            }
        }
    }
}