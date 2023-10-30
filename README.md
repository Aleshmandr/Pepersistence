# Pepersistence

Extensible save/load system package for Unity engine, designed to simplify the management of game data persistence.

## Installation
You can install Pepersistence in one of the following ways:

**Using Unity Package Manager**:

1. Open your Unity project.
2. Go to the Unity Package Manager via `Window > Package Manager`.
3. Click on the "Add package from git URL" button.
4. Enter the following URL: `https://github.com/Aleshmandr/Pepersistence.git`.

**Manual Download**:

Alternatively, you can download the source files directly from the [Pepersistence GitHub repository](https://github.com/Aleshmandr/Pepersistence) and import them into your Unity project manually.

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
   Alternatively, if you use dependency injection (DI) in your project, you can register the necessary objects inside your PersistenceManager after resolving dependencies.
6. Load/save your game data
   ````c#
   ...
   persistenceManager.Load();
   ...
   persistenceManager.Save();
   ````

## Features

1. Contains classes for local saves, supporting both binary and JSON formats.
2. Extensibility: You can implement your custom save source, such as cloud-based saves, to meet your specific needs.

## TODO
1. Clear saves in Editor
2. Migrations support
