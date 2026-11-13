using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1463): the characters a runtime's shown sprite sources name, resolved the way the import pipeline
/// resolves them, from the cartridge-derived extraction's boss, area, map and standard OBJ characters and the enemy
/// artwork the fixture installed it from (#1636). The runtime itself never resolves characters; it draws pre-coloured
/// sheets, and holds none of the import's artwork (#1583).
/// </summary>
/// <param name="runtime">The runtime whose bound artwork is resolved.</param>
/// <param name="standardObjects">The standard OBJ sheet the runtime's room loads show first.</param>
/// <param name="enemyTiles">The import's enemy artwork the runtime's enemy sheets and frame displays were installed from.</param>
internal sealed class RuntimeArtworkContent(SuperMetroidRuntime runtime, RoomCharacterAtlas standardObjects, EnemyTileSourceArtwork enemyTiles)
    : IInstalledArtworkContent, IInstalledSpriteCharacterArtwork, IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork
{
    /// <summary>The artwork bound into <paramref name="runtime"/>, with the installation's standard OBJ sheet and enemy artwork.</summary>
    internal static RuntimeArtworkContent Of(SuperMetroidRuntime runtime) => Of(runtime, InstalledFixtures.EnemyTiles);

    /// <summary>The artwork bound into <paramref name="runtime"/>, installed from <paramref name="enemyTiles"/>, with the installation's standard OBJ sheet.</summary>
    internal static RuntimeArtworkContent Of(SuperMetroidRuntime runtime, EnemyTileSourceArtwork enemyTiles) =>
        new(runtime, InstalledFixtures.StandardObjects, enemyTiles);

    /// <inheritdoc/>
    public EnemyTileSourceArtwork EnemyTiles => enemyTiles;

    /// <summary>The installation's boss artwork, which only the import reads; the runtime binds only Kraid's eye colours (#1583).</summary>
    public BossSourceArtwork BossTiles => InstalledFixtures.BossArtwork;

    /// <summary>The installation's area artwork, which only the import reads; the runtime binds none (#1512).</summary>
    public AreaSourceArtwork AreaTiles => InstalledFixtures.AreaArtwork;

    /// <summary>The characters a typed asset names, from the standard OBJ sheet, the gunship's takeoff pages or the map presentation's escape-timer characters.</summary>
    /// <exception cref="ArgumentOutOfRangeException">No bound catalog holds characters for the asset.</exception>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset) =>
        asset == InstalledArtworkId.StandardObjectTiles
            ? standardObjects.Characters
            : asset is InstalledArtworkId.GunshipLiftoffFirstTiles or InstalledArtworkId.GunshipLiftoffSecondTiles or
                InstalledArtworkId.GunshipLiftoffThirdTiles or InstalledArtworkId.GunshipLiftoffFourthTiles or
                InstalledArtworkId.GunshipLiftoffFifthTiles
            ? (EnemyTiles.GunshipLiftoff ?? throw new InvalidOperationException("Gunship takeoff artwork is not bound.")).Resolve(asset)
            : asset is InstalledArtworkId.EscapeTimerFirstTiles or InstalledArtworkId.EscapeTimerSecondTiles
            ? EscapeTimerTilePageTransfers.Resolve(InstalledFixtures.ImportMaps.EscapeTimerTiles, asset)
            : throw new ArgumentOutOfRangeException(nameof(asset), asset, "The runtime binds no characters for this asset.");

    /// <summary>The gameplay OBJ character page enemy-format sprites draw from: the table's uploads over the bound artwork.</summary>
    /// <param name="uploads">The OBJ character table's uploads.</param>
    internal SpriteArtwork GameplayObjectPage(SpriteArtworkPlacements uploads) => new ObjCharacterTablePage().Page(uploads, this);
}
