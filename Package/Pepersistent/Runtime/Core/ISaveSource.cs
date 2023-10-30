namespace Pepersistance
{
    public interface ISaveSource
    {
        public SaveObject Load();
        public void Save(SaveObject save);
    }
}