using SuperMetroid.AssetExtraction.Frontend.Map;
using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.Areas.Ceres;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.CeresCinematics.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Ending;
using SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Intro.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Title;
using SuperMetroid.AssetExtraction.Frontend.Title.Artwork;
using SuperMetroid.AssetExtraction.Fx;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
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
    /// <summary>The installed sheets of frontend turning floors' pictures (#1392, #1467).</summary>
    internal static IReadOnlyList<SpriteSheetId> Sheets { get; } =
    [
        SpriteSheetId.TitleFloor, SpriteSheetId.CeresFloor, SpriteSheetId.EndingEscapeAFloor,
        SpriteSheetId.EndingEscapeBFloor, SpriteSheetId.EndingPlanetExplosionFloor, SpriteSheetId.EndingFlyawayFloor,
        SpriteSheetId.EndingRewardIconFloor,
    ];

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
            [SpriteSheetId.TitleFloor] = sources => Title(sources.Maps.TitleGraphics, InstalledAreaMapPresentation.TitlePaletteOf(sources.Maps)),
            [SpriteSheetId.CeresFloor] = sources => Ceres(sources.Intro, sources.Maps.RoomPaletteFx),
            [SpriteSheetId.EndingEscapeAFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.EscapeA, EndingPaletteId.Escape), sources),
            [SpriteSheetId.EndingEscapeBFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.EscapeB, EndingPaletteId.Escape), sources),
            [SpriteSheetId.EndingPlanetExplosionFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.PlanetExplosion, EndingPaletteId.Explosion), sources),
            [SpriteSheetId.EndingFlyawayFloor] = sources => EndingShaded(Flyaway(sources.Intro.CeresFlight, sources.EndingPalettes), sources),
            [SpriteSheetId.EndingRewardIconFloor] = sources => EndingShaded(RewardIcon(sources.EndingMode7, sources.EndingPalettes), sources),
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
        ComposedSpriteSheet floor = FloorSheetComposer.Compose(pictures, ScreenPaletteColours.TitleColours(palette));
        // The floor shows each record the console lights show (#1512), over the inks each light recolours.
        var shades = new List<SheetShade>();
        foreach (PaletteFxColours light in TitleSheetInks.FloorLights)
        {
            (int first, int end) = TitleSheetInks.FloorLight(light);
            shades.AddRange(PaletteFxScreenShades.Of(
                [TitleSequenceDefinitions.ConsolePaletteFx.SlowLights, TitleSequenceDefinitions.ConsolePaletteFx.FastLights],
                light, first, end, palette));
        }
        return floor with { Manifest = floor.Manifest with { Shades = SheetShade.Documents(shades) } };
    }

    /// <summary>
    /// The Ceres cinematics' floor in each shot, under the flight's palette: each shot's views over the start of a floor of
    /// the blank tile, as the flight and the destruction load them, showing the flight's characters, with a shade for each
    /// record the gunship engine's flicker shows, coloured from <paramref name="paletteFx"/> (#1512).
    /// </summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(IntroCinematicArtwork intro, IPaletteFxColorSource paletteFx) =>
        Ceres(Ensure.NotNull(intro).CeresFlight, IntroCinematicSourceArtwork.Of(intro).CeresDestructionMaps, paletteFx);

    /// <summary>
    /// The Ceres cinematics' floor in each shot from the flight's installed artwork and the destruction's maps, with the
    /// engine flicker's shades coloured from <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(CeresFlightArtwork flight, CeresDestructionMaps destruction, IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(flight);
        Ensure.NotNull(destruction);
        Ensure.NotNull(paletteFx);
        ReadOnlySpan<byte> characters = CeresFlightCharacters.Of(flight).FloorCharacters.Span;
        if (characters.Length < CeresFlightArtworkFileFormat.FloorByteCount)
            throw new InvalidDataException("The Ceres flight's Mode 7 characters are shorter than the scenes load.");
        if (CeresFlightCharacters.Of(flight).FloorMaps.Length + destruction.Views.Length < CeresFloorShotDefinitions.MapCells)
            throw new InvalidDataException("The Ceres cinematics' floor views are shorter than the scenes load.");
        var views = new byte[CeresFloorShotDefinitions.MapCells];
        CeresFlightCharacters.Of(flight).FloorMaps.Span.CopyTo(views);
        destruction.Views.Span[..(CeresFloorShotDefinitions.MapCells - CeresFlightCharacters.Of(flight).FloorMaps.Length)]
            .CopyTo(views.AsSpan(CeresFlightCharacters.Of(flight).FloorMaps.Length));
        byte[] pixels = characters[..CeresFlightArtworkFileFormat.FloorByteCount].ToArray();
        byte[] station = Blank();
        views.AsSpan(CeresFloorShotDefinitions.StationStart, CeresFloorShotDefinitions.StationCells).CopyTo(station);
        // The final explosion shows the front gunship in the upper view, or a blank view with Samus still aboard, over a
        // blank lower view; the approach to Zebes then shows the rear gunship in the upper view.
        byte[] explosion = Exploded(views, station, CeresFloorShotDefinitions.FrontViewStart);
        byte[] explosionWithoutGunship = Exploded(views, station, CeresFloorShotDefinitions.BlankViewStart);
        byte[] zebes = (byte[])explosion.Clone();
        views.AsSpan(CeresFloorShotDefinitions.RearViewStart, CeresFloorShotDefinitions.ViewCells).CopyTo(zebes);
        ComposedSpriteSheet floor = FloorSheetComposer.Compose(
        [
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightFront), View(views, CeresFloorShotDefinitions.FrontViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightRear), View(views, CeresFloorShotDefinitions.RearViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Station), station, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Explosion), explosion, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.ExplosionWithoutGunship), explosionWithoutGunship, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.ZebesApproach), zebes, pixels),
        ], ScreenPaletteColours.FlightColours(flight));
        // The floor shows each record the gunship engine's flicker shows over the engine's colour (#1512).
        return floor with { Manifest = floor.Manifest with { Shades = SheetShade.Documents(CeresSheetInks.EngineShades(paletteFx)) } };

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

    /// <summary>
    /// An ending floor shaded in every ending shade (#1512): a floor's inks are the screen's colours, so each shade covers
    /// the floor's colours it writes.
    /// </summary>
    private static ComposedSpriteSheet EndingShaded(ComposedSpriteSheet floor, ScreenTileSources sources) => floor with
    {
        Manifest = floor.Manifest with
        {
            Shades = SheetShade.Documents(EndingScreenShades.Screen(sources.EndingPalettes, sources.Maps.RoomPaletteFx)),
        },
    };

    /// <summary>An ending escape or explosion scene's floor: its map laid out twice, one copy after the other, over its characters.</summary>
    private static ComposedSpriteSheet EndingScene(ScreenTileSources sources, EndingFloorSceneId scene, EndingPaletteId palette)
    {
        ReadOnlySpan<byte> map = EndingFloorCharacters.SceneOf(sources.EndingMode7, scene).Map.Span;
        var cells = new byte[FloorSheetComposer.CellCount];
        map.CopyTo(cells);
        map.CopyTo(cells.AsSpan(EndingFloorArtworkFormat.MapByteCount));
        return FloorSheetComposer.Compose(
            [FloorSheetComposer.Whole(EndingFloorDefinitions.FrameName, cells, EndingFloorSceneCharacters.Of(EndingFloorCharacters.SceneOf(sources.EndingMode7, scene)).Characters.Span)],
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
        if (characters.Length < EndingArtworkExtentDefinitions.FloorBytes || CeresFlightCharacters.Of(flight).FloorMaps.Length < EndingArtworkExtentDefinitions.FlyawayMapDataBytes)
            throw new InvalidDataException("The flight's floor is shorter than the flyaway loads.");
        var cells = new byte[FloorSheetComposer.CellCount];
        cells.AsSpan().Fill(EndingArtworkExtentDefinitions.FlyawayBlankTile);
        CeresFlightCharacters.Of(flight).FloorMaps.Span[..EndingArtworkExtentDefinitions.FlyawayMapDataBytes].CopyTo(cells);
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
        ReadOnlySpan<byte> words = EndingRewardIconCharacters.Of(EndingFloorCharacters.RewardIconOf(mode7)).Bytes.Span;
        ReadOnlySpan<byte> map = EndingFloorCharacters.RewardIconOf(mode7).Map.Span;
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
