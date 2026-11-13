using SuperMetroid.AssetExtraction.Samus.Artwork;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Samus;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// The installed stock artwork the enemy sheets compose from (#1392), loaded once for all of them. It resolves the
/// artwork an encounter uploads over the characters after its room loads, as the game's installed artwork does.
/// </summary>
/// <param name="Enemies">The enemies' tiles, palettes, compositions and extended frames.</param>
/// <param name="Bosses">The bosses' artwork, which their encounters upload.</param>
/// <param name="Areas">The areas' artwork, which their encounters upload.</param>
/// <param name="StandardObjects">The standard OBJ characters a room load uploads first.</param>
/// <param name="Palettes">The starting and common sprite colours a room load copies.</param>
/// <param name="EscapeTimer">The escape timer's characters, which escapes upload.</param>
internal sealed record EnemySheetInputs(EnemyTileSourceArtwork Enemies, BossSourceArtwork Bosses, AreaSourceArtwork Areas,
    ReadOnlyMemory<byte> StandardObjects, GameplayBasePalette Palettes, EscapeTimerCharacters EscapeTimer)
    : IInstalledArtworkContent, IInstalledSpriteCharacterArtwork, IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static EnemySheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string enemyTiles = Path.Combine(content, GameInstallationLayout.EnemyTileDirectoryName);
        string standardObjects = Path.Combine(content, GameInstallationLayout.StandardObjectDirectoryName);
        string maps = Path.Combine(content, GameInstallationLayout.MapDirectoryName);
        ImportEnemyArtwork installed = EnemyTileArtworkFiles.LoadForImport(ImportDataDirectory.Of(enemyTiles));
        return new(
            installed.Enemies,
            installed.Bosses,
            installed.Areas,
            StandardObjectArtworkFiles.LoadForImport(standardObjects, ImportDataDirectory.Of(standardObjects)).Characters,
            GameplayBasePaletteFiles.LoadForImport(ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.GameplayBasePaletteDirectoryName))),
            LoadEscapeTimer(Path.Combine(ImportDataDirectory.Of(maps), EscapeTimerTileAtlasFormat.FileName)))
        {
            BeamPalettes = LoadBeamPalettes(Path.Combine(ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.ProjectileDirectoryName)), BeamPaletteFile.FileName)),
            Maps = MapPresentationFiles.LoadForImport(maps, ImportDataDirectory.Of(maps)),
        };
    }

    /// <summary>
    /// The palette row each graphics set's records load their members' colours into, by set name (#1512): the retail
    /// catalogue for the installation's sets; a fixture composing a set of its own names that set's rows, as its records'
    /// placement words name them.
    /// </summary>
    internal IReadOnlyDictionary<string, int[]> SetPaletteRows { get; init; } = EnemyGraphicsSetPaletteRowDefinitions.Rows;

    /// <summary>
    /// The members each graphics set loads, by set name (#1527): the retail catalogue for the installation's sets; a
    /// fixture composing a set of its own names that set's members.
    /// </summary>
    internal IReadOnlyDictionary<string, RoomEnemyGraphicsSetMembers> SetMembers { get; init; } = RoomEnemyGraphicsSetLists.Members;

    /// <summary>The beam colours, which a frozen enemy's shade is coloured from (#1512); null for fixtures that draw no frozen enemy.</summary>
    internal InstalledBeamPalettes? BeamPalettes { get; init; }

    /// <summary>
    /// The installed map presentation, which holds the colours the Ceres Ridley and Mother Brain encounters load over
    /// their sprites (#1512); null for fixtures that draw neither encounter.
    /// </summary>
    internal ImportAreaMapPresentation? Maps { get; init; }

    /// <summary>The beam colours the import extracted at <paramref name="path"/> (#1637).</summary>
    private static InstalledBeamPalettes LoadBeamPalettes(string path)
    {
        using FileStream json = File.OpenRead(path);
        return BeamPaletteFile.Read(json);
    }

    /// <summary>The escape timer characters the import extracted at <paramref name="path"/>.</summary>
    private static EscapeTimerCharacters LoadEscapeTimer(string path)
    {
        using FileStream png = File.OpenRead(path);
        return EscapeTimerCharacters.Read(png);
    }

    /// <inheritdoc/>
    EnemyTileSourceArtwork IInstalledSpriteCharacterArtwork.EnemyTiles => Enemies;

    /// <inheritdoc/>
    BossSourceArtwork IInstalledBossSpriteCharacterArtwork.BossTiles => Bosses;

    /// <inheritdoc/>
    AreaSourceArtwork IInstalledAreaSpriteCharacterArtwork.AreaTiles => Areas;

    /// <inheritdoc/>
    /// <exception cref="InvalidDataException">The enemy artwork holds no gunship takeoff pictures.</exception>
    /// <exception cref="ArgumentOutOfRangeException">No catalogued upload names the asset.</exception>
    ReadOnlyMemory<byte> IInstalledArtworkContent.Resolve(InstalledArtworkId asset) =>
        asset is InstalledArtworkId.EscapeTimerFirstTiles or InstalledArtworkId.EscapeTimerSecondTiles ? EscapeTimerTilePageTransfers.Resolve(EscapeTimer, asset)
            : (Enemies.GunshipLiftoff ?? throw new InvalidDataException("Enemy sheets require the installed gunship takeoff pictures.")).Resolve(asset);
}
