namespace Pepersistance
{
    public class LocalSaveSource : ISaveSource
    {
        private readonly string savePath;

        public LocalSaveSource() : this("save") { }

        public LocalSaveSource(string savePath)
        {
            this.savePath = savePath;
        }

        public SaveObject Load()
        {
            return JsonSaveFileManager.LoadFromFile<SaveObject>(savePath);
        }

        public void Save(SaveObject save)
        {
            JsonSaveFileManager.SaveToFile(save, savePath);
        }
    }
}