using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Frontend.Title.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the frontend's turning floors once (#1467): the title's floor with each Baby Metroid page shown, the Ceres
/// cinematics' floor in each of their shots, and each ending shot's floor. Each is the whole floor the cartridge's map and
/// character transfers leave for that shot, pictured under the palette of the screen that shows it; the screens select
/// the pictures by scene state.
/// </summary>
internal static class FrontendFloorSheetComposer
{
    /// <summary>Composes floor sheet <paramref name="sheet"/> from the stock artwork <paramref name="sources"/> read.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no frontend floor.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetId sheet, ScreenTileSources sources)
    {
        Ensure.NotNull(sources);
        return Composers.TryGetValue(sheet, out Func<ScreenTileSources, ComposedSpriteSheet>? compose) ? compose(sources)
            : throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no frontend floor.");
    }

    /// <summary>How each frontend floor sheet composes from the stock artwork.</summary>
    private static readonly FrozenDictionary<SpriteSheetId, Func<ScreenTileSources, ComposedSpriteSheet>> Composers =
        new Dictionary<SpriteSheetId, Func<ScreenTileSources, ComposedSpriteSheet>>
        {
            [SpriteSheetId.TitleFloor] = sources => Title(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            [SpriteSheetId.CeresFloor] = sources => Ceres(sources.Intro),
            [SpriteSheetId.EndingEscapeAFloor] = sources => EndingScene(sources, EndingFloorSceneId.EscapeA, EndingPaletteId.Escape),
            [SpriteSheetId.EndingEscapeBFloor] = sources => EndingScene(sources, EndingFloorSceneId.EscapeB, EndingPaletteId.Escape),
            [SpriteSheetId.EndingPlanetExplosionFloor] = sources => EndingScene(sources, EndingFloorSceneId.PlanetExplosion, EndingPaletteId.Explosion),
            [SpriteSheetId.EndingFlyawayFloor] = sources => Flyaway(sources.Intro.CeresFlight, sources.EndingPalettes),
            [SpriteSheetId.EndingRewardIconFloor] = sources => RewardIcon(sources.EndingMode7, sources.EndingPalettes),
        }.ToFrozenDictionary();

    /// <summary>
    /// The title's floor with each Baby Metroid page shown: the title's map over the start of a floor whose other cells
    /// keep the initial tile, as the title's setup fills the map and loads the installed map over its start; and its
    /// characters with the page loaded over the Baby's tiles.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed map or characters are shorter than the title loads.</exception>
    internal static ComposedSpriteSheet Title(TitleGraphicsPresentation graphics, TitlePalettePresentation palette)
    {
        TitleGraphicsCharacters title = TitleGraphicsCharacters.Of(Ensure.NotNull(graphics));
        if (title.FloorMap.Length < TitleFloorTileDefinitions.FloorMapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than the map the title loads.");
        if (title.FloorCharacters.Length < TitleFloorTileDefinitions.FloorCharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than the characters the title loads.");
        if (title.BabyCharacters.Length < TitleFloorTileDefinitions.BabyPageCount * TitleFloorTileDefinitions.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");
        var map = new byte[FloorSheetComposer.CellCount];
        map.AsSpan().Fill(TitleFloorTileDefinitions.FloorInitialMapByte);
        title.FloorMap[..TitleFloorTileDefinitions.FloorMapByteCount].CopyTo(map);
        var pictures = new List<FloorSheetComposer.FloorPicture>();
        for (int page = 0; page < TitleFloorTileDefinitions.BabyPageCount; page++)
        {
            var pixels = new byte[FloorSheetComposer.TileSetBytes];
            title.FloorCharacters[..TitleFloorTileDefinitions.FloorCharacterByteCount]
                .CopyTo(pixels.AsSpan(TitleFloorTileDefinitions.FloorFirstTile * FloorSheetComposer.TileBytes));
            title.BabyCharacters.Slice(page * TitleFloorTileDefinitions.BabyCharacterPageByteCount, TitleFloorTileDefinitions.BabyCharacterPageByteCount)
                .CopyTo(pixels.AsSpan(TitleFloorTileDefinitions.BabyFirstTile * FloorSheetComposer.TileBytes));
            pictures.Add(FloorSheetComposer.Whole(TitleFloors.FrameName(page), map, pixels));
        }
        return FloorSheetComposer.Compose(pictures, ScreenPaletteColours.TitleColours(palette));
    }

    /// <summary>
    /// The Ceres cinematics' floor in each shot, under the flight's palette: each shot's views over the start of a floor of
    /// the blank tile, as the flight and the destruction load them, showing the flight's characters.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(IntroCinematicArtwork intro) => Ceres(Ensure.NotNull(intro).CeresFlight, intro.CeresDestruction);

    /// <summary>The Ceres cinematics' floor in each shot from the flight's and the destruction's installed artwork.</summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(CeresFlightArtwork flight, CeresDestructionArtwork destruction)
    {
        Ensure.NotNull(flight);
        Ensure.NotNull(destruction);
        ReadOnlySpan<byte> characters = CeresFlightCharacters.Of(flight).FloorCharacters.Span;
        if (characters.Length < CeresFlightArtworkFormat.FloorByteCount)
            throw new InvalidDataException("The Ceres flight's Mode 7 characters are shorter than the scenes load.");
        if (flight.FloorMaps.Length + destruction.CeresMaps.Length < CeresFloorShotDefinitions.MapCells)
            throw new InvalidDataException("The Ceres cinematics' floor views are shorter than the scenes load.");
        var views = new byte[CeresFloorShotDefinitions.MapCells];
        flight.FloorMaps.Span.CopyTo(views);
        destruction.CeresMaps.Span[..(CeresFloorShotDefinitions.MapCells - flight.FloorMaps.Length)]
            .CopyTo(views.AsSpan(flight.FloorMaps.Length));
        byte[] pixels = characters[..CeresFlightArtworkFormat.FloorByteCount].ToArray();
        byte[] station = Blank();
        views.AsSpan(CeresFloorShotDefinitions.StationStart, CeresFloorShotDefinitions.StationCells).CopyTo(station);
        // The final explosion shows the front gunship in the upper view, or a blank view with Samus still aboard, over a
        // blank lower view; the approach to Zebes then shows the rear gunship in the upper view.
        byte[] explosion = Exploded(views, station, CeresFloorShotDefinitions.FrontViewStart);
        byte[] explosionWithoutGunship = Exploded(views, station, CeresFloorShotDefinitions.BlankViewStart);
        byte[] zebes = (byte[])explosion.Clone();
        views.AsSpan(CeresFloorShotDefinitions.RearViewStart, CeresFloorShotDefinitions.ViewCells).CopyTo(zebes);
        return FloorSheetComposer.Compose(
        [
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightFront), View(views, CeresFloorShotDefinitions.FrontViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightRear), View(views, CeresFloorShotDefinitions.RearViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Station), station, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Explosion), explosion, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.ExplosionWithoutGunship), explosionWithoutGunship, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.ZebesApproach), zebes, pixels),
        ], ScreenPaletteColours.FlightColours(flight));

        // A floor of the blank tile.
        static byte[] Blank()
        {
            var cells = new byte[FloorSheetComposer.CellCount];
            cells.AsSpan().Fill(CeresFloorShotDefinitions.BlankTile);
            return cells;
        }

        // A floor of the blank tile with the view starting at installed cell `start` over its start.
        static byte[] View(ReadOnlySpan<byte> views, int start)
        {
            byte[] cells = Blank();
            views.Slice(start, CeresFloorShotDefinitions.ViewCells).CopyTo(cells);
            return cells;
        }

        // The station's floor with the view starting at installed cell `upper` over its upper view and the blank view below.
        static byte[] Exploded(ReadOnlySpan<byte> views, byte[] station, int upper)
        {
            var cells = (byte[])station.Clone();
            views.Slice(upper, CeresFloorShotDefinitions.ViewCells).CopyTo(cells);
            views.Slice(CeresFloorShotDefinitions.BlankViewStart, CeresFloorShotDefinitions.ViewCells)
                .CopyTo(cells.AsSpan(CeresFloorShotDefinitions.LowerViewFirstCell));
            return cells;
        }
    }

    /// <summary>An ending escape or explosion scene's floor: its map laid out twice, one copy after the other, over its characters.</summary>
    private static ComposedSpriteSheet EndingScene(ScreenTileSources sources, EndingFloorSceneId scene, EndingPaletteId palette)
    {
        ReadOnlySpan<byte> map = sources.EndingMode7[scene].Map.Span;
        var cells = new byte[FloorSheetComposer.CellCount];
        map.CopyTo(cells);
        map.CopyTo(cells.AsSpan(EndingFloorArtworkFormat.MapByteCount));
        return FloorSheetComposer.Compose(
            [FloorSheetComposer.Whole(EndingFloorDefinitions.FrameName, cells, EndingFloorSceneCharacters.Of(sources.EndingMode7[scene]).Characters.Span)],
            ScreenPaletteColours.EndingColours(sources.EndingPalettes, palette));
    }

    /// <summary>
    /// The flyaway's floor once all its loads are made: the flight's map data padded with the blank tile to the whole floor,
    /// over the flight's characters, under the escape palette.
    /// </summary>
    /// <exception cref="InvalidDataException">The flight's characters or map are shorter than the flyaway loads.</exception>
    private static ComposedSpriteSheet Flyaway(CeresFlightArtwork flight, EndingPaletteSet palettes)
    {
        ReadOnlySpan<byte> characters = CeresFlightCharacters.Of(Ensure.NotNull(flight)).FloorCharacters.Span;
        if (characters.Length < EndingArtworkExtentDefinitions.FloorBytes || flight.FloorMaps.Length < EndingArtworkExtentDefinitions.FlyawayMapDataBytes)
            throw new InvalidDataException("The flight's floor is shorter than the flyaway loads.");
        var cells = new byte[FloorSheetComposer.CellCount];
        cells.AsSpan().Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        flight.FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(cells);
        return FloorSheetComposer.Compose(
            [FloorSheetComposer.Whole(EndingFloorDefinitions.FrameName, cells, characters[..EndingArtworkExtentDefinitions.FloorBytes])],
            ScreenPaletteColours.EndingColours(palettes, EndingPaletteId.Escape));
    }

    /// <summary>
    /// The reward icon's floor once all its chunks are loaded: each word of its artwork holds a floor cell in its low byte
    /// and a tile pixel in its high byte; under the post-credits palette.
    /// </summary>
    /// <exception cref="InvalidDataException">The icon's artwork is shorter than its chunks load.</exception>
    private static ComposedSpriteSheet RewardIcon(EndingFloorArtwork mode7, EndingPaletteSet palettes)
    {
        ReadOnlySpan<byte> words = EndingRewardIconCharacters.Of(Ensure.NotNull(mode7).RewardIcon).Bytes.Span;
        ReadOnlySpan<byte> map = mode7.RewardIcon.Map.Span;
        if (map.Length < FloorSheetComposer.CellCount || words.Length < FloorSheetComposer.TileSetBytes * sizeof(ushort))
            throw new InvalidDataException("The reward icon's artwork is shorter than its chunks load.");
        var pixels = new byte[FloorSheetComposer.TileSetBytes];
        for (int pixel = 0; pixel < pixels.Length; pixel++)
            pixels[pixel] = words[pixel * sizeof(ushort) + 1];
        return FloorSheetComposer.Compose(
            [FloorSheetComposer.Whole(EndingFloorDefinitions.FrameName, map[..FloorSheetComposer.CellCount], pixels)],
            ScreenPaletteColours.EndingColours(palettes, EndingPaletteId.PostCredits));
    }
}
