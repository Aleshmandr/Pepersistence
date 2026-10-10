namespace Pepersistence
{
    /// <summary>
    /// A save source that can replace a save with valid outer structure but invalid content by its backup.
    /// </summary>
    public interface IRecoverableSaveSource
    {
        /// <summary>
        /// Sets the current save aside as corrupted and loads its backup. Returns null when no backup can be loaded.
        /// </summary>
        SaveObject RecoverFromBackup();
    }
}