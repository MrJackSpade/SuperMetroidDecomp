using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Installation;
using SuperMetroid.Core.Escape.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// The installed stock artwork room block pictures are composed from (#1392): the tilesets' characters, block definitions
/// and palettes, and the artwork rooms bind over the characters after loading.
/// </summary>
/// <param name="Characters">The tilesets' and common room elements' characters.</param>
/// <param name="Metatiles">The tilesets' and common room elements' block definitions.</param>
/// <param name="Palettes">The tilesets' palettes.</param>
/// <param name="AnimatedTiles">The animated tiles' frames.</param>
/// <param name="Bosses">The bosses' artwork, Kraid's room backdrop among it.</param>
/// <param name="Hud">The HUD's characters, which Kraid's library background moves under BG1.</param>
/// <param name="BackgroundTilemaps">The library backgrounds' tilemaps.</param>
/// <param name="Skies">The scrolling skies' tilemaps, which some library backgrounds transfer.</param>
/// <param name="EscapeTypewriter">The escape typewriter's programs, whose text is typed over a room's background.</param>
/// <param name="Maps">The map presentation: the HUD, the area maps its minimap shows and the room effects' BG3 pages.</param>
/// <param name="Objects">
/// The artwork uploaded over the OBJ character table, which the Ceres Ridley arena's backgrounds show, and the gameplay base
/// palettes, whose sprite rows every room load restores.
/// </param>
internal sealed record RoomBlockSheetInputs(RoomCharacterAtlases Characters, RoomMetatileCatalog Metatiles,
    RoomStaticPalettes Palettes, RoomFxAnimatedTileAtlas AnimatedTiles,
    BossSourceArtwork Bosses, HudTileAtlas Hud, RoomBackgroundTilemaps BackgroundTilemaps, RoomSkyTilemaps Skies,
    EscapeTypewriterPresentation EscapeTypewriter, ImportAreaMapPresentation Maps, EnemySheetInputs Objects)
{
    /// <summary>Loads the stock artwork from the extracted content <paramref name="content"/> and its import data.</summary>
    internal static RoomBlockSheetInputs Load(string content)
    {
        ArgumentException.ThrowIfNullOrWhiteSpace(content);
        string map = Path.Combine(content, GameInstallationLayout.MapDirectoryName);
        string roomCharacters = Path.Combine(content, GameInstallationLayout.RoomCharacterDirectoryName);
        // The block compositions, background tilemaps and sky pages are import data (#1638), as the characters are (#1636).
        string backgrounds = ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.RoomBackgroundTilemapDirectoryName));
        ImportAreaMapPresentation maps = MapPresentationFiles.LoadForImport(map, ImportDataDirectory.Of(map));
        EnemySheetInputs objects = EnemySheetInputs.Load(content);
        return new(
            RoomCharacterArtworkFiles.LoadForImport(roomCharacters, ImportDataDirectory.Of(roomCharacters)),
            RoomMetatileArtworkFiles.LoadForImport(ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.RoomMetatileDirectoryName))),
            RoomStaticPaletteArtworkFiles.LoadForImport(ImportDataDirectory.Of(Path.Combine(content, GameInstallationLayout.RoomPaletteDirectoryName))),
            maps.RoomFxAnimatedTiles,
            objects.Bosses,
            maps.Characters.Hud,
            RoomBackgroundTilemapArtworkFiles.LoadForImport(backgrounds),
            RoomSkyTilemapArtworkFiles.LoadForImport(backgrounds),
            maps.Game.EscapeTypewriter,
            maps,
            objects);
    }
}
