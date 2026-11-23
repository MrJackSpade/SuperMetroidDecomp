using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// Writes the Godot game's assets from an installation: its pictures cut from the installed sheets, and its data
/// files, whose values and timelines come from the old game's own definitions. The Godot project names every file
/// it wants; what is written here is written under those names.
/// </summary>
public sealed partial class EngineAssetWriter
{
    private readonly EngineAssetTree _tree;
    private readonly EngineSheets _sheets;

    private EngineAssetWriter(GameInstallation installation, string assets)
    {
        _tree = new EngineAssetTree(assets);
        _sheets = new EngineSheets(installation.ContentDirectory);
    }

    /// <summary>
    /// Writes the assets of the installation at <paramref name="installationRoot"/> into the directory
    /// <paramref name="assets"/>, which is the Godot project's <c>assets</c> folder.
    /// </summary>
    public static void Write(string installationRoot, string assets)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(installationRoot);
        ArgumentException.ThrowIfNullOrWhiteSpace(assets);
        var writer = new EngineAssetWriter(new GameInstallation(installationRoot), assets);
        writer.WriteEnemies();
    }

    /// <summary>
    /// Starts an enemy's data file: its pictures are cut from the installed sheet named for the old enemy, into a
    /// folder beside the file named as the file is.
    /// </summary>
    /// <param name="file">The data file, as the Godot project names it.</param>
    /// <param name="type">The Godot type that reads the file.</param>
    /// <param name="id">The old game's enemy.</param>
    /// <param name="sheet">The installed sheet its pictures are on; the old enemy's own name when not given.</param>
    internal EngineEnemy Enemy(string file, string type, EnemyDefinitionId id, string? sheet = null)
    {
        string folder = file[..file.LastIndexOf('.')];
        string named = _sheets.SheetNamed(GameInstallationLayout.EnemySheetDirectoryName, sheet ?? id.ToString());
        return new EngineEnemy(_tree, file, type, id, _sheets.Cut(_tree, GameInstallationLayout.EnemySheetDirectoryName, named, folder));
    }
}
