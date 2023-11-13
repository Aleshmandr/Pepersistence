namespace Pepersistence
{
    public class LocalSaveSource : ISaveSource
    {
        private readonly string savePath;
        private readonly string encryptionKey;

        public LocalSaveSource() : this("save", null)
        {}

        public LocalSaveSource(string savePath, string encryptionKey)
        {
            this.savePath = savePath;
            this.encryptionKey = encryptionKey;
        }

        public SaveObject Load()
        {
            return JsonSaveHandler.LoadFromFile<SaveObject>(savePath, encryptionKey);
        }

        public void Save(SaveObject save)
        {
            JsonSaveHandler.SaveToFile(save, savePath, encryptionKey);
        }
    }
}