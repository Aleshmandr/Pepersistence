namespace Pepersistance
{
    public interface ISavable<T> where T : ISaveData
    {
        void Load(T saveData);
        void Save(ref T saveData);
    }
}