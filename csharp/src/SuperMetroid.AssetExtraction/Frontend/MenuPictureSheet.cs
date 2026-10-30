using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.FileSelect.Artwork;
using SuperMetroid.AssetExtraction.Frontend.GameOver.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;
using SuperMetroid.AssetExtraction.Frontend.Options.Artwork;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Menus;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// The menus' picture sheet (#1467): each picture the shared menu screen shows, cut from the page the cartridge lays it out
/// on and pre-coloured under the file-select palette over the world foreground and map characters the menus load. The game
/// places each by its frame name; no menu keeps a tilemap.
/// </summary>
internal static class MenuPictureSheet
{
    /// <summary>Every picture the sheet holds, in sheet order.</summary>
    internal static IReadOnlyList<ScreenPictureSubject> Subjects(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        return [GameOverSourcePresentation.Of(maps.GameOver).PageSubject(), .. GameOptionsSourcePresentation.Of(maps.GameOptions).Subjects(),
            .. FileSelectSourcePresentation.Of(maps.FileSelect).Subjects()];
    }

    /// <summary>The characters the menus' pages name: the world foreground's, then the map's after them.</summary>
    internal static ScreenCharacters Characters(AreaMapPresentation maps)
    {
        Ensure.NotNull(maps);
        int mapBytes = MapScreenDefinitions.MapTileCount * ScreenTileSheet.CharacterWordsOf(TileInkDepth.SixteenColours) * sizeof(ushort);
        return new ScreenCharacters(TileInkDepth.SixteenColours)
            .Place(MenuTileSetDefinitions.WorldForegroundFirstTile, WorldMapCharacters.Of(maps.WorldArtwork).ForegroundCharacters())
            .Place(MenuTileSetDefinitions.MenuMapFirstTile, MapCharacterArtwork.Of(maps).Map.Characters[..mapBytes].ToArray());
    }

    /// <summary>Composes the sheet.</summary>
    internal static ComposedSpriteSheet Compose(AreaMapPresentation maps) =>
        ScreenPictureSheetComposer.Compose(Subjects(maps), Characters(maps), ScreenColourImage.Encode(Ensure.NotNull(maps).Palettes.FileSelect));

    /// <summary>The frames the sheet must hold: one per picture.</summary>
    internal static IEnumerable<string> FrameNames(AreaMapPresentation maps) => Subjects(maps).Select(subject => subject.Name);
}
