# Pepersistence

Extensible save/load system package for Unity game engine, designed to simplify the management of game data persistence.

## Installation
You can install Pepersistence in one of the following ways:

**Using Unity Package Manager**:

1. Open your Unity project.
2. Go to the Unity Package Manager via `Window > Package Manager`.
3. Click on the "Add package from git URL" button.
4. Enter the following URL: `https://github.com/Aleshmandr/Pepersistence.git`.

**Manual Download**:

Alternatively, you can download the source files directly from the [Pepersistence GitHub repository](https://github.com/Aleshmandr/Pepersistence) and import them into your Unity project manually.

**Package dependencies**:

This package depends on Json.NET by Newtonsoft (`com.unity.nuget.newtonsoft-json` version `3.1.2`)

## How to use

Follow these steps to integrate Pepersistence into your project:

1. Create a class for your global savable game data that implements the `ISaveData` interface:

    ````c#
    [Serializable]
    public class GameSaveData : ISaveData
    {
        public LevelsSaveData Levels;
    }
    ````

    ````c#
    [Serializable]
    public struct LevelsSaveData
    {
        public int CompletedCount;
    }
    ````

2. Create your own persistence manager class by extending the `BasePersistenceManager<T> where T : ISaveData, new()` class:

    ````c#
    public class PersistenceManager : BasePersistenceManager<GameSaveData>
    {
        public PersistenceManager(ISaveSource saveSource) : base(saveSource) { }
    }
    ````

3. Ensure that all objects you want to save implement `ISavable<T> where T : ISaveData`:

   ````c#
    public class LevelsDataManager : ISavable<GameSaveData>
    {
        private int completedCount;
        
        public void Load(GameSaveData saveData)
        {
            completedCount = saveData.Levels.CompletedCount;
        }

        public void Save(ref GameSaveData saveData)
        {
            saveData.Levels.CompletedCount = completedCount;
        }
    }
   ````


4. Register all objects you want to save with your `PersistenceManager`:
   ````c#
   ...
   persistenceManager.Register(levelsDataManager);
   ...
   ````
   **Alternatively**, if you use dependency injection (DI) in your project, you can register the necessary objects inside your PersistenceManager:

   ````c#
   public class PersistenceManager : BasePersistenceManager<SaveData>
    {
        public PersistenceManager(ISaveSource saveSource, IReadOnlyList<ISavable> savableObjects) : base(saveSource)
        {
            foreach (var savableObject in savableObjects)
            {
                Register(savableObject);
            }
        }
    }
   ````
   Where `LevelsDataManager` implements `ISavable` instead of `ISavable<GameSaveData>`:

   ````c#
   public interface ISavable : ISavable<SaveData> { }
   ````

6. Load/save your game data
   ````c#
   ...
   persistenceManager.Load();
   ...
   persistenceManager.Save();
   ````

7. You can delete all save files in the editor by selecting the `Edit > Pepersistence > Clear All Saves`.
   This action will remove all files with an extension that matches the pattern `^.*sav$`, including the default `.jsav`, 
   together with their `.tmp`, `.bak` and `.corrupt` copies.
   Therefore, if you create your own save file format, make sure to use a file extension that matches this pattern, as it will also be removed when clearing saves.

## Save file safety

`JsonSaveHandler` (used by `LocalSaveSource`) protects local saves from crashes and power loss:

- A save is written to `<name>.jsav.tmp` and flushed to the storage before it replaces the save file.
- The previous save is kept as `<name>.jsav.bak` instead of being deleted, so a complete save file exists at every step.
- If the save file is missing or cannot be parsed, the backup is loaded instead, then a complete temporary file left by an 
  interrupted save. A corrupted save file is moved to `<name>.jsav.corrupt` for investigation, so it can't replace the 
  good backup on the next save.
- `BasePersistenceManager` also validates the save data inside the file. When it is empty or not valid JSON, the manager 
  asks a source implementing `IRecoverableSaveSource` (`LocalSaveSource`, `BackgroundLocalSaveSource`) for the backup 
  before it falls back to new save data.
- The file format is unchanged, so saves written by older versions load as before.

### Background saves

Flushing a save file to the storage takes several milliseconds, which is noticeable on the main thread of a real-time game.
Use `BackgroundLocalSaveSource` instead of `LocalSaveSource` to keep it off the main thread:

````c#
var persistenceManager = new PersistenceManager(new BackgroundLocalSaveSource("save", encryptionKey));
````

- The save data is serialized on the calling thread; the file is written on a background thread. 
  Call it from the main thread.
- A newer save of the same file replaces a queued write that has not started yet, so frequent saves don't pile up.
- Loads, synchronous saves and `DeleteAllSaves` wait for queued writes of the file, and all file operations are serialized, 
  so an older save never overwrites a newer one.
- A failed write is kept and retried by the next save or flush. `JsonSaveHandler.WaitForPendingWrites` returns `false` 
  while a write keeps failing.
- On `Application.quitting` queued writes are completed and later saves are written synchronously. 
  WebGL always writes synchronously.

## Features

1. Contains classes for local saves, supporting both binary and JSON formats.
2. Extensibility: You can implement your custom save source, such as cloud-based saves, to meet your specific needs.
3. Save file encryprion
4. Save data migrations
5. Crash-safe local saves with an automatic backup
