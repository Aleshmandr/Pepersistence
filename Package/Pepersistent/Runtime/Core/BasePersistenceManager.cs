using System;
using System.Collections.Generic;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public abstract class BasePersistenceManager<T> where T : ISaveData, new()
    {
        private readonly List<ISavable<T>> savableObjects;
        private readonly ISaveSource saveSource;
        private T saveData;

        public BasePersistenceManager(ISaveSource saveSource)
        {
            this.saveSource = saveSource;
            savableObjects = new List<ISavable<T>>();
        }

        public void Register(ISavable<T> savable)
        {
            savableObjects.Add(savable);
        }

        public void Save()
        {
            for (int i = 0; i < savableObjects.Count; i++)
            {
                savableObjects[i].Save(ref saveData);
            }

            var saveObject = CreateSaveObject();
            saveSource.Save(saveObject);
        }

        public void Load()
        {
            SaveObject saveObject = saveSource.Load();
            Migrate(saveObject);
            LoadSaveObject(saveObject);

            for (int i = 0; i < savableObjects.Count; i++)
            {
                savableObjects[i].Load(saveData);
            }
        }

        private void Migrate(SaveObject saveObject)
        {
            // TODO: Add save migration feature
        }

        private void LoadSaveObject(SaveObject saveObject)
        {
            try
            {
                saveData = JsonConvert.DeserializeObject<T>(saveObject.Data);
            } catch (Exception e)
            {
                Debug.Log($"Unable to load a save object, create the new one. ({e.Message})");
                saveData = new T();
            }
        }

        private SaveObject CreateSaveObject()
        {
            string version = Application.version;
            return new SaveObject
            {
                Data = JsonConvert.SerializeObject(saveData),
                Version = version
            };
        }
    }
}