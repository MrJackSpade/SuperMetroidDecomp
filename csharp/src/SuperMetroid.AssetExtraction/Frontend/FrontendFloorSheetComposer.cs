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
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.AssetExtraction.Fx.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend;

/// <summary>
/// Composes the frontend's turning floors once (#1467): the title's floor and the Baby Metroid's poses drawn over it, the Ceres
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

    /// <summary>
    /// Composes floor sheet <paramref name="sheet"/> from the stock artwork <paramref name="sources"/> read, shown whole where
    /// its scene repeats the floor beyond its edges (<see cref="RepeatingFloors"/>, #1646).
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException">The sheet holds no frontend floor.</exception>
    internal static ComposedSpriteSheet Compose(SpriteSheetId sheet, ScreenTileSources sources)
    {
        Ensure.NotNull(sources);
        return Composers.TryGetValue(sheet, out Func<ScreenTileSources, ComposedSpriteSheet>? compose)
            ? compose(sources) with { ShownWhole = RepeatingFloors.Repeats(sheet) }
            : throw new ArgumentOutOfRangeException(nameof(sheet), sheet, "The sheet holds no frontend floor.");
    }

    /// <summary>How each frontend floor sheet composes from the stock artwork.</summary>
    private static readonly FrozenDictionary<SpriteSheetId, Func<ScreenTileSources, ComposedSpriteSheet>> Composers =
        new Dictionary<SpriteSheetId, Func<ScreenTileSources, ComposedSpriteSheet>>
        {
            [SpriteSheetId.TitleFloor] = sources => Title(sources.Maps.TitleGraphics, sources.Maps.TitlePalette),
            [SpriteSheetId.CeresFloor] = sources => Ceres(sources.Intro, sources.Maps.RoomPaletteFx),
            [SpriteSheetId.EndingEscapeAFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.EscapeA, EndingPaletteId.Escape), sources,
                EndingFloorShot.EscapeA),
            [SpriteSheetId.EndingEscapeBFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.EscapeB, EndingPaletteId.Escape), sources,
                EndingFloorShot.EscapeB),
            [SpriteSheetId.EndingPlanetExplosionFloor] = sources => EndingShaded(EndingScene(sources, EndingFloorSceneId.PlanetExplosion, EndingPaletteId.Explosion), sources,
                EndingFloorShot.PlanetExplosion),
            [SpriteSheetId.EndingFlyawayFloor] = sources => EndingShaded(Flyaway(sources.Intro.FlightCharacters, sources.EndingPalettes), sources, EndingFloorShot.Flyaway),
            [SpriteSheetId.EndingRewardIconFloor] = sources => EndingShaded(RewardIcon(sources.EndingMode7, sources.EndingPalettes), sources, EndingFloorShot.RewardIcon),
        }.ToFrozenDictionary();

    /// <summary>
    /// The title's floor (#1538): the whole floor without the Baby Metroid, the title's map over the start of a floor whose
    /// other cells keep the initial tile, as the title's setup fills the map and loads the installed map over its start,
    /// showing the floor's characters with the Baby's tiles blank; and the Baby in each of its poses, the cells of the map
    /// that show the Baby's tiles, showing the pose's characters over them, drawn over the floor where the Baby lies.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The installed map or characters are shorter than the title loads, or the map shows no cell of the Baby.
    /// </exception>
    internal static ComposedSpriteSheet Title(TitleGraphicsCharacters graphics, TitlePalettePresentation palette)
    {
        TitleGraphicsCharacters title = Ensure.NotNull(graphics);
        if (title.FloorMap.Length < TitleFloorTileDefinitions.FloorMapByteCount)
            throw new InvalidDataException("Title Mode 7 map stream is shorter than the map the title loads.");
        if (title.FloorCharacters.Length < TitleFloorTileDefinitions.FloorCharacterByteCount)
            throw new InvalidDataException("Title Mode 7 character stream is shorter than the characters the title loads.");
        if (title.BabyCharacters.Length < TitleFloorTileDefinitions.BabyPageCount * TitleFloorTileDefinitions.BabyCharacterPageByteCount)
            throw new InvalidDataException("Title baby-Metroid stream is missing an animation page.");
        var map = new byte[FloorSheetComposer.CellCount];
        map.AsSpan().Fill(TitleFloorTileDefinitions.FloorInitialMapByte);
        title.FloorMap[..TitleFloorTileDefinitions.FloorMapByteCount].CopyTo(map);
        var floorPixels = new byte[FloorSheetComposer.TileSetBytes];
        title.FloorCharacters[..TitleFloorTileDefinitions.FloorCharacterByteCount]
            .CopyTo(floorPixels.AsSpan(TitleFloorTileDefinitions.FloorFirstTile * FloorSheetComposer.TileBytes));
        floorPixels.AsSpan(TitleFloorTileDefinitions.BabyFirstTile * FloorSheetComposer.TileBytes, TitleFloorTileDefinitions.BabyCharacterPageByteCount).Clear();
        BabyCellRectangle baby = BabyCells(map);
        byte[] babyCells = new byte[baby.Columns * baby.Rows];
        for (int row = 0; row < baby.Rows; row++)
            map.AsSpan((baby.Top + row) * FloorSheetComposer.Side + baby.Left, baby.Columns).CopyTo(babyCells.AsSpan(row * baby.Columns));
        var pictures = new List<FloorSheetComposer.FloorPicture> { FloorSheetComposer.Whole(TitleFloors.FloorFrameName, map, floorPixels) };
        foreach (TitleBabyPose pose in Enum.GetValues<TitleBabyPose>())
        {
            byte[] posePixels = [.. floorPixels];
            title.BabyCharacters.Slice(TitleFloorTileDefinitions.BabyPages[pose] * TitleFloorTileDefinitions.BabyCharacterPageByteCount,
                    TitleFloorTileDefinitions.BabyCharacterPageByteCount)
                .CopyTo(posePixels.AsSpan(TitleFloorTileDefinitions.BabyFirstTile * FloorSheetComposer.TileBytes));
            pictures.Add(new FloorSheetComposer.FloorPicture(TitleFloors.BabyFrameName(pose), babyCells, baby.Columns, baby.Left, baby.Top, posePixels));
        }
        ComposedSpriteSheet floor = FloorSheetComposer.Compose(pictures, ScreenPaletteColours.TitleColours(palette));
        // The floor shows each record the console lights show (#1512), over the inks each light recolours.
        var shades = new List<SheetShade>();
        foreach (PaletteFxColours light in TitleSheetInks.FloorLights)
        {
            shades.AddRange(PaletteFxScreenShades.Of(
                [TitleSequenceDefinitions.ConsolePaletteFx.SlowLights, TitleSequenceDefinitions.ConsolePaletteFx.FastLights],
                light, TitleSheetInks.FloorLight(light), palette));
        }
        return floor with { Manifest = floor.Manifest with { Shades = SheetShade.Documents(shades) } };
    }

    /// <summary>
    /// The rectangle of the title floor's <paramref name="map"/> the Baby Metroid lies in: the left column, top row, columns
    /// and rows of the cells that show the Baby's tiles.
    /// </summary>
    /// <exception cref="InvalidDataException">No cell of the map shows the Baby's tiles.</exception>
    private static BabyCellRectangle BabyCells(byte[] map)
    {
        int left = FloorSheetComposer.Side, top = FloorSheetComposer.Side, right = -1, bottom = -1;
        for (int cell = 0; cell < map.Length; cell++)
        {
            if (map[cell] is < TitleFloorTileDefinitions.BabyFirstTile or >= TitleFloorTileDefinitions.BabyFirstTile + TitleFloorTileDefinitions.BabyTileCount)
                continue;
            int column = cell % FloorSheetComposer.Side, row = cell / FloorSheetComposer.Side;
            left = Math.Min(left, column);
            top = Math.Min(top, row);
            right = Math.Max(right, column);
            bottom = Math.Max(bottom, row);
        }
        return right >= 0 ? new BabyCellRectangle(left, top, right - left + 1, bottom - top + 1)
            : throw new InvalidDataException("The title floor's map shows no cell of the Baby Metroid.");
    }

    /// <summary>The cells of the title floor's map the Baby Metroid lies in.</summary>
    /// <param name="Left">The leftmost cell's column.</param>
    /// <param name="Top">The topmost cell's row.</param>
    /// <param name="Columns">The rectangle's width in cells.</param>
    /// <param name="Rows">The rectangle's height in cells.</param>
    private readonly record struct BabyCellRectangle(int Left, int Top, int Columns, int Rows);

    /// <summary>
    /// The Ceres cinematics' floor in each shot, under the flight's palette: each shot's views over the start of a floor of
    /// the blank tile, as the flight and the destruction load them, showing the flight's characters, with a shade for each
    /// record the gunship engine's flicker (#1512) and the station's navigation lights (#1733) show, coloured from
    /// <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(ImportIntroCinematicArtwork intro, IPaletteFxColorSource paletteFx) =>
        Ceres(Ensure.NotNull(intro).FlightCharacters, intro.CeresDestructionMaps,
            intro.FlightPalette, paletteFx);

    /// <summary>
    /// The Ceres cinematics' floor in each shot from the flight's installed artwork and the destruction's maps, coloured
    /// in the flight's colours <paramref name="colours"/>, with the engine flicker's and the navigation lights' shades coloured
    /// from <paramref name="paletteFx"/>.
    /// </summary>
    /// <exception cref="InvalidDataException">The installed views or characters are shorter than the scenes load.</exception>
    internal static ComposedSpriteSheet Ceres(CeresFlightCharacters flight, CeresDestructionMaps destruction, CeresFlightPalette colours,
        IPaletteFxColorSource paletteFx)
    {
        Ensure.NotNull(flight);
        Ensure.NotNull(destruction);
        Ensure.NotNull(colours);
        Ensure.NotNull(paletteFx);
        ReadOnlySpan<byte> characters = flight.FloorCharacters.Span;
        if (characters.Length < CeresFlightArtworkFileFormat.FloorByteCount)
            throw new InvalidDataException("The Ceres flight's Mode 7 characters are shorter than the scenes load.");
        if (flight.FloorMaps.Length + destruction.Views.Length < CeresFloorShotDefinitions.MapCells)
            throw new InvalidDataException("The Ceres cinematics' floor views are shorter than the scenes load.");
        var views = new byte[CeresFloorShotDefinitions.MapCells];
        flight.FloorMaps.Span.CopyTo(views);
        destruction.Views.Span[..(CeresFloorShotDefinitions.MapCells - flight.FloorMaps.Length)]
            .CopyTo(views.AsSpan(flight.FloorMaps.Length));
        byte[] pixels = characters[..CeresFlightArtworkFileFormat.FloorByteCount].ToArray();
        byte[] station = Blank();
        views.AsSpan(CeresFloorShotDefinitions.StationStart, CeresFloorShotDefinitions.StationCells).CopyTo(station);
        // The final explosion shows the front gunship in the upper view over a blank lower view; with Samus still aboard
        // both views are blank and the scene shows no floor (#1513). The approach to Zebes then shows the rear gunship in
        // the upper view.
        byte[] explosion = Exploded(views, station, CeresFloorShotDefinitions.FrontViewStart);
        byte[] zebes = (byte[])explosion.Clone();
        views.AsSpan(CeresFloorShotDefinitions.RearViewStart, CeresFloorShotDefinitions.ViewCells).CopyTo(zebes);
        ComposedSpriteSheet floor = FloorSheetComposer.Compose(
        [
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightFront), View(views, CeresFloorShotDefinitions.FrontViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.FlightRear), View(views, CeresFloorShotDefinitions.RearViewStart), pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Station), station, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.Explosion), explosion, pixels),
            FloorSheetComposer.Whole(CeresFloors.FrameName(CeresFloorShot.ZebesApproach), zebes, pixels),
        ], colours.Colours.AsSpan());
        // The floor shows each record the gunship engine's flicker shows over the engine's colour (#1512), and each record the
        // station's navigation lights show over their colours (#1733).
        return floor with
        {
            Manifest = floor.Manifest with
            {
                Shades = SheetShade.Documents([.. CeresSheetInks.EngineShades(paletteFx), .. CeresSheetInks.FloorNavigationLightShades(paletteFx)]),
            },
        };

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
    /// The floor of <paramref name="shot"/> shaded in every ending shade authored for it (#1512, #1544): a floor's inks are
    /// the screen's colours, so each shade covers the floor's colours it writes.
    /// </summary>
    private static ComposedSpriteSheet EndingShaded(ComposedSpriteSheet floor, ScreenTileSources sources, EndingFloorShot shot) => floor with
    {
        Manifest = floor.Manifest with
        {
            Shades = SheetShade.Documents(EndingScreenShades.Scenery(EndingScreenShades.Screen(sources.EndingPalettes, sources.Maps.RoomPaletteFx),
                [EndingFloorDefinitions.Scenery[shot]])),
        },
    };

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
    private static ComposedSpriteSheet Flyaway(CeresFlightCharacters flight, EndingPaletteSet palettes)
    {
        ReadOnlySpan<byte> characters = Ensure.NotNull(flight).FloorCharacters.Span;
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
    private static ComposedSpriteSheet RewardIcon(EndingFloorCharacters mode7, EndingPaletteSet palettes)
    {
        ReadOnlySpan<byte> words = EndingRewardIconCharacters.Of(mode7.RewardIcon).Bytes.Span;
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
