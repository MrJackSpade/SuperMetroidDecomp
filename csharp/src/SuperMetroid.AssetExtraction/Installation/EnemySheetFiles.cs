using System.Text.Json;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.AssetExtraction.Installation;

/// <summary>
/// Installs the pre-coloured enemy sheets (#1392): each distinct sheet's image, palette map and frames, the manifest
/// naming each room graphics set's sheet for each of its enemies, and the provenance of their hashes. A player override
/// of a sheet's file replaces the stock one.
/// </summary>
public static class EnemySheetFiles
{
    /// <summary>The provenance manifest's file name.</summary>
    public const string ProvenanceFileName = "enemy-sheets-provenance.json";

    /// <summary>Composes and writes every enemy sheet and the set manifest, then validates the result.</summary>
    /// <param name="content">Installed stock content holding the tiles, compositions and colours the sheets compose from.</param>
    /// <param name="directory">Destination directory, created if absent.</param>
    /// <param name="sourceCartridgeSha256">Identity written into the provenance manifest.</param>
    public static void Extract(string content, string directory, string sourceCartridgeSha256)
    {
        EnemySheetComposition composition = EnemySheetComposer.Compose(EnemySheetInputs.Load(content));
        SpriteSheetStore store = SpriteSheetStore.Create(directory, ProvenanceFileName);
        foreach ((string name, ComposedSpriteSheet sheet) in composition.Sheets)
            store.Write(name, sheet);
        store.Finish(sourceCartridgeSha256);
        File.WriteAllBytes(Path.Combine(directory, EnemySheetDefinitions.ManifestFileName),
            JsonSerializer.SerializeToUtf8Bytes(composition.Manifest, JsonOptions));
        _ = Load(directory, null);
    }

    /// <summary>
    /// Loads the set manifest and every sheet it names, preferring a player override of each file, and checks each
    /// sheet holds exactly the frames its enemy's behaviour shows.
    /// </summary>
    /// <exception cref="InvalidDataException">The manifest, a stock hash, an image or a frame manifest is invalid.</exception>
    public static InstalledEnemySheets Load(string stockDirectory, string? overrideDirectory)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(stockDirectory);
        string manifestPath = Path.Combine(stockDirectory, EnemySheetDefinitions.ManifestFileName);
        EnemySheetManifestDocument manifest = JsonSerializer.Deserialize<EnemySheetManifestDocument>(File.ReadAllBytes(manifestPath), JsonOptions)
            ?? throw new InvalidDataException($"Enemy sheet manifest {manifestPath} is empty.");
        if (manifest.Version != EnemySheetDefinitions.ManifestVersion)
            throw new InvalidDataException($"Enemy sheet manifest {manifestPath} has revision {manifest.Version}, not {EnemySheetDefinitions.ManifestVersion}.");
        string[] names = [.. manifest.Sets.Values.SelectMany(members => members.Values).Distinct(StringComparer.Ordinal)];
        SpriteSheetStore store = SpriteSheetStore.Open(stockDirectory, overrideDirectory, ProvenanceFileName, names.Length);
        var loaded = names.ToDictionary(name => name, store.Read, StringComparer.Ordinal);
        var sheets = new Dictionary<EnemySheetKey, SpriteSheet>();
        // The sheets' frames are named from the stock enemy compositions installed beside them.
        var enemies = new Lazy<EnemyTileArtworkCatalog>(() => EnemyTileArtworkFiles.Load(
            Path.Combine(Path.GetDirectoryName(Path.GetFullPath(stockDirectory))!, GameInstallationLayout.EnemyTileDirectoryName), null).Enemies);
        foreach ((string set, Dictionary<string, string> members) in manifest.Sets)
        {
            foreach ((string enemyName, string name) in members)
            {
                if (!Enum.TryParse(enemyName, out EnemyDefinitionId enemy) || !Enum.IsDefined(enemy))
                    throw new InvalidDataException($"Enemy sheet manifest set {set} names unknown enemy {enemyName}.");
                SpriteSheet sheet = loaded[name];
                if (!sheet.HoldsExactly(EnemySheetComposer.FrameNames(enemies.Value, enemy)))
                    throw new InvalidDataException($"Enemy sheet {name} does not hold exactly the frames {enemy} shows.");
                sheets.Add(new EnemySheetKey(set, enemy), sheet);
            }
        }
        return new InstalledEnemySheets(sheets);
    }

    /// <summary>Used by installer completeness checks; overrides do not affect stock provenance.</summary>
    public static void ValidateStock(string stockDirectory) => _ = Load(stockDirectory, null);

    /// <summary>Stable camel-case manifest fields.</summary>
    private static readonly JsonSerializerOptions JsonOptions = new()
    {
        PropertyNamingPolicy = JsonNamingPolicy.CamelCase,
        PropertyNameCaseInsensitive = true,
        WriteIndented = true,
    };
}
