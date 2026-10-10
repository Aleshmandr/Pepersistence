using System;
using System.IO;
using System.Text;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public static class JsonSaveHandler
    {
        private const string SavePathFormat = "{0}/{1}.jsav";
        private const string TempFileSuffix = ".tmp";
        private const string BackupFileSuffix = ".bak";
        private const string CorruptedFileSuffix = ".corrupt";
        private static readonly UTF8Encoding FileEncoding = new UTF8Encoding(false);

        private enum LoadStatus
        {
            Loaded,
            NotFound,
            Unreadable,
            Corrupted
        }

        public static T LoadFromFile<T>(string fileName, string encryptionKey = null) where T : class
        {
            var path = GetSavePath(fileName);
            var status = TryLoad(path, encryptionKey, out T result);
            if (status == LoadStatus.Loaded)
            {
                return result;
            }

            if (status == LoadStatus.Corrupted)
            {
                // Keeps the damaged file for investigation and prevents the next save from rotating it into the backup.
                SetAsideCorrupted(path);
            }

            var backupPath = path + BackupFileSuffix;
            if (TryLoad(backupPath, encryptionKey, out result) == LoadStatus.Loaded)
            {
                Debug.LogWarning($"Save file '{path}' could not be loaded ({status}), restored from '{backupPath}'.");
                return result;
            }

            return null;
        }

        public static void SaveToFile<T>(T data, string fileName, string encryptionKey = null)
        {
            var path = GetSavePath(fileName);
            var tempPath = path + TempFileSuffix;
            var backupPath = path + BackupFileSuffix;
            try
            {
                var json = JsonConvert.SerializeObject(data);
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    json = XorEncryption.EncryptDecrypt(json, encryptionKey);
                }

                // The temporary file must reach the storage before the swap; otherwise a crash or power loss can leave
                // a renamed save file without its contents.
                using (var stream = new FileStream(tempPath, FileMode.Create, FileAccess.Write, FileShare.None))
                {
                    using (var writer = new StreamWriter(stream, FileEncoding))
                    {
                        writer.Write(json);
                        writer.Flush();
                        stream.Flush(true);
                    }
                }

                // The previous save becomes the backup, so a complete save file exists at every step of the swap.
                if (File.Exists(path))
                {
                    if (File.Exists(backupPath))
                    {
                        File.Delete(backupPath);
                    }

                    File.Move(path, backupPath);
                }

                File.Move(tempPath, path);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                TryDelete(tempPath);
            }
        }

        private static string GetSavePath(string fileName)
        {
            return string.Format(SavePathFormat, Application.persistentDataPath, fileName);
        }

        private static LoadStatus TryLoad<T>(string path, string encryptionKey, out T result) where T : class
        {
            result = null;
            if (!File.Exists(path))
            {
                return LoadStatus.NotFound;
            }

            string json;
            try
            {
                json = File.ReadAllText(path);
            }
            catch (FileNotFoundException)
            {
                return LoadStatus.NotFound;
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return LoadStatus.Unreadable;
            }

            try
            {
                if (!string.IsNullOrEmpty(encryptionKey))
                {
                    json = XorEncryption.EncryptDecrypt(json, encryptionKey);
                }

                result = JsonConvert.DeserializeObject<T>(json);
            }
            catch (Exception e)
            {
                Debug.LogException(e);
                return LoadStatus.Corrupted;
            }

            if (result == null)
            {
                // Empty, whitespace-only or zero-filled files deserialize to null without an exception.
                Debug.LogError($"Save file '{path}' contains no data.");
                return LoadStatus.Corrupted;
            }

            return LoadStatus.Loaded;
        }

        private static void SetAsideCorrupted(string path)
        {
            var corruptedPath = path + CorruptedFileSuffix;
            try
            {
                if (File.Exists(corruptedPath))
                {
                    File.Delete(corruptedPath);
                }

                File.Move(path, corruptedPath);
                Debug.LogWarning($"Corrupted save file '{path}' was moved to '{corruptedPath}'.");
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }

        private static void TryDelete(string path)
        {
            try
            {
                if (File.Exists(path))
                {
                    File.Delete(path);
                }
            }
            catch (Exception e)
            {
                Debug.LogException(e);
            }
        }
    }
}