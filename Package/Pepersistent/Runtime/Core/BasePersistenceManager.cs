using System;
using System.Collections.Generic;
using System.Globalization;
using Newtonsoft.Json;
using UnityEngine;

namespace Pepersistence
{
    public abstract class BasePersistenceManager<T> where T : ISaveData, new()
    {
        private readonly List<ISavable<T>> savableObjects;
        private readonly ISaveSource saveSource;
        private IReadOnlyList<ISaveDataMigration> migrations;
        private T saveData;
        
        private readonly JsonSerializerSettings serializerSettings = new()
        {
            Error = (sender, args) =>
            {
                Debug.LogError($"Save load error at path '{args.ErrorContext.Path}': {args.ErrorContext.Error.Message}");
                args.ErrorContext.Handled = true;
            }
        };

        public bool IsLoaded { get; private set; }

        public BasePersistenceManager(ISaveSource saveSource)
        {
            this.saveSource = saveSource;
            savableObjects = new List<ISavable<T>>();
        }

        public void Register(ISavable<T> savable)
        {
            savableObjects.Add(savable);
        }

        public void InitializeMigration(IReadOnlyList<ISaveDataMigration> migrations)
        {
            this.migrations = migrations;
        }

        public void Save()
        {
            if (!IsLoaded)
            {
                Debug.LogWarning("Skip a save. Save method must be called after Load to prevent progress loss.");
                return;
            }

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
            saveObject = Migrate(saveObject);
            LoadSaveObject(saveObject);

            for (int i = 0; i < savableObjects.Count; i++)
            {
                savableObjects[i].Load(saveData);
            }

            IsLoaded = true;
        }

        private SaveObject Migrate(SaveObject saveObject)
        {
            if (migrations == null || saveObject == null || string.IsNullOrEmpty(saveObject.Data))
            {
                return saveObject;
            }

            foreach (var migration in migrations)
            {
                try
                {
                    Version version = Version.Parse(saveObject.Version);
                    Version migrateToVersion = Version.Parse(migration.MigrateToVersion);
                    if (version >= migrateToVersion)
                    {
                        continue;
                    }

                    saveObject.Data = migration.Migrate(saveObject.Data);
                    saveObject.Version = migration.MigrateToVersion;
                    Debug.Log($"Migration {version} -> {migrateToVersion} performed");
                }
                catch (Exception e)
                {
                    Debug.LogError($"Migration error {migration}: {e}");
                }
            }

            return saveObject;
        }

        private void LoadSaveObject(SaveObject saveObject)
        {
            try
            {
                saveData = JsonConvert.DeserializeObject<T>(saveObject.Data, serializerSettings);
            }
            catch (NullReferenceException nullReferenceException)
            {
                Debug.Log($"Unable to load a save object, create the new one. ({nullReferenceException.Message})");
                saveData = new T();
            }
            catch (Exception e)
            {
                Debug.LogError($"Unable to load a save object, create the new one. ({e.Message})");
                saveData = new T();
            }
        }

        private SaveObject CreateSaveObject()
        {
            string version = Application.version;
            return new SaveObject
            {
                Data = JsonConvert.SerializeObject(saveData),
                Version = version,
                Date = DateTime.Now.ToString(CultureInfo.InvariantCulture)
            };
        }
    }
}