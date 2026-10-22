using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Bosses.Artwork;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed stock artwork room block pictures are composed from (#1392): the tilesets' characters, block definitions
/// and palettes, and the artwork rooms bind over the characters after loading.
/// </summary>
/// <param name="Characters">The tilesets' and common room elements' characters.</param>
/// <param name="Metatiles">The tilesets' and common room elements' block definitions.</param>
/// <param name="Palettes">The tilesets' palettes.</param>
/// <param name="AnimatedTiles">The animated tiles' frames.</param>
/// <param name="Collectibles">The collectibles' graphics.</param>
/// <param name="Bosses">The bosses' artwork, Kraid's room backdrop among it.</param>
/// <param name="Hud">The HUD's characters, which Kraid's library background moves under BG1.</param>
/// <param name="BackgroundTilemaps">The library backgrounds' tilemaps.</param>
/// <param name="Skies">The scrolling skies' tilemaps, which some library backgrounds transfer.</param>
/// <param name="EscapeTypewriter">The escape typewriter's programs, whose text is typed over a room's background.</param>
internal sealed record RoomBlockSheetInputs(RoomCharacterAtlasCatalog Characters, RoomMetatileCatalog Metatiles,
    RoomStaticPaletteCatalog Palettes, RoomFxAnimatedTileAtlas AnimatedTiles, RoomPlmDynamicCollectibleArtCatalog Collectibles,
    BossArtworkCatalog Bosses, HudTileAtlas Hud, RoomBackgroundTilemapCatalog BackgroundTilemaps, RoomSkyTilemapCatalog Skies,
    EscapeTypewriterPresentation EscapeTypewriter)
{
    /// <summary>Loads the stock artwork from installed content.</summary>
    internal static RoomBlockSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        AreaMapPresentationCatalog maps = AreaMapPresentationCatalog.Load(Path.Combine(content, GameInstallationLayout.MapDirectoryName), null);
        return new(
            RoomCharacterArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.RoomCharacterDirectoryName), null),
            RoomMetatileArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.RoomMetatileDirectoryName), null),
            RoomStaticPaletteArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.RoomPaletteDirectoryName), null),
            maps.RoomFxAnimatedTiles,
            RoomPlmDynamicCollectibleArtFiles.Load(Path.Combine(content, GameInstallationLayout.RoomPlmDynamicCollectibleArtDirectoryName), null),
            EnemyTileArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.EnemyTileDirectoryName), null).Bosses,
            maps.HudTiles,
            RoomBackgroundTilemapArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.RoomBackgroundTilemapDirectoryName), null),
            RoomSkyTilemapArtworkFiles.Load(Path.Combine(content, GameInstallationLayout.RoomBackgroundTilemapDirectoryName), null),
            maps.EscapeTypewriter);
    }
}
