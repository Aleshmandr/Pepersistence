namespace Pepersistence
{
    public interface ISaveDataMigration
    {
        public string MigrateToVersion { get; }
        public string Migrate(string saveData);
    }
}