using SuperMetroid.AssetExtraction.Areas.Escape;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction.Rooms.Artwork;

namespace SuperMetroid.TestSupport.RepositoryInstallation.Verification;

/// <summary>
/// Verification access (#1463): the installed characters a runtime's shown sprite sources name, resolved the way the
/// import pipeline resolves them, from the enemy, boss, area and map artwork bound into the runtime and the installed
/// standard OBJ sheet. The runtime itself never resolves characters; it draws pre-coloured sheets.
/// </summary>
/// <param name="runtime">The runtime whose bound artwork is resolved.</param>
/// <param name="standardObjects">The standard OBJ sheet the runtime's room loads show first.</param>
internal sealed class RuntimeArtworkContent(SuperMetroidRuntime runtime, RoomCharacterAtlas standardObjects)
    : IInstalledArtworkContent, IInstalledSpriteCharacterArtwork, IInstalledBossSpriteCharacterArtwork, IInstalledAreaSpriteCharacterArtwork
{
    /// <summary>The artwork bound into <paramref name="runtime"/>, with the installation's standard OBJ sheet.</summary>
    internal static RuntimeArtworkContent Of(SuperMetroidRuntime runtime) => new(runtime, InstalledFixtures.StandardObjects);

    /// <inheritdoc/>
    public EnemyTileSourceArtwork EnemyTiles =>
        (EnemyTileSourceArtwork?)runtime.EnemySession.TileArtwork ?? throw new InvalidOperationException("No enemy artwork is installed.");

    /// <inheritdoc/>
    public RoomFxAnimatedTileAtlas AnimatedTiles =>
        runtime.Artwork.MapPresentation?.RoomFxAnimatedTiles ?? throw new InvalidOperationException("No map presentation is installed.");

    /// <inheritdoc/>
    public BossSourceArtwork BossTiles =>
        (BossSourceArtwork?)runtime.BossArtwork.Artwork ?? throw new InvalidOperationException("No boss artwork is installed.");

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
            ? EscapeTimerTilePageTransfers.Resolve(InstalledAreaMapPresentation.EscapeTimerTilesOf(runtime.Artwork.MapPresentation as AreaMapPresentation
                ?? throw new InvalidOperationException("Map presentation artwork is not bound.")), asset)
            : throw new ArgumentOutOfRangeException(nameof(asset), asset, "The runtime binds no characters for this asset.");

    /// <summary>The gameplay OBJ character page enemy-format sprites draw from: the table's uploads over the bound artwork.</summary>
    /// <param name="uploads">The OBJ character table's uploads.</param>
    internal SpriteArtwork GameplayObjectPage(SpriteArtworkPlacements uploads) => new ObjCharacterTablePage().Page(uploads, this);
}
