using SuperMetroid.AssetExtraction;
using SuperMetroid.Desktop;

/// <summary>Loads immutable ticket fixtures through the production state reader without touching live slots.</summary>
internal static class DebuggerFixtureLoader
{
    /// <summary>
    /// Restores a fixture the way an installed host does: the repository installation supplies the
    /// verified content identity, and its catalogs are rebound because states omit external artwork.
    /// </summary>
    public static DebuggerSaveStateLoadResult Load(string fixtureName, int slot)
    {
        var installation = RepositoryInstallation.Installation;
        var identity = GameContentIdentity.Create(installation.LoadAudio(), RepositoryInstallation.Maps,
            RepositoryInstallation.Projectiles);
        string directory = Path.Combine(Path.GetTempPath(), $"SuperMetroid-fixture-{Guid.NewGuid():N}");
        Directory.CreateDirectory(directory);
        var store = DebuggerSaveStateStore.ForInstalledGame(installation.Root, null, identity, directory);
        string destination = store.GetSlotPath(slot);
        try
        {
            File.Copy(Path.Combine("csharp", "test-fixtures", fixtureName, $"slot-{slot}-named.smstate"), destination);
            DebuggerSaveStateLoadResult loaded = store.Load(slot);
            RepositoryInstallation.BindGame(loaded.Game);
            return loaded;
        }
        finally
        {
            if (File.Exists(destination)) File.Delete(destination);
            Directory.Delete(directory);
        }
    }
}
