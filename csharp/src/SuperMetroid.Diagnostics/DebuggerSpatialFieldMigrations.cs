using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// One typed spatial field (#628) that replaced parallel scalar fields. An older capture names
/// the scalar fields: each is drained as a retired field, and the typed value is rebuilt from all
/// of them once the instance's saved fields are restored. A capture must carry every legacy word
/// unless the typed value's state was itself added after some published layouts
/// (<see cref="LegacyMayOmit"/>), in which case a capture with none of them keeps the default.
/// </summary>
/// <param name="TypeName">The owning type's full name.</param>
/// <param name="Field">The typed field that replaced <paramref name="LegacyFields"/>.</param>
/// <param name="LegacyFields">The retired scalar fields, in the order <paramref name="Rebuild"/> reads them.</param>
/// <param name="Rebuild">Builds the typed value from the restored instance and the legacy values.</param>
/// <param name="LegacyTypes">The exact payload type an older layout wrote for each legacy field.</param>
/// <param name="LegacyMayOmit">True when published layouts predate the legacy fields themselves.</param>
/// <param name="OmittedWarning">Reported once when a capture predating the legacy fields keeps the default.</param>
internal sealed record DebuggerSpatialFieldMigration(
    string TypeName,
    string Field,
    string[] LegacyFields,
    Type[] LegacyTypes,
    Func<object, object?[], object> Rebuild,
    bool LegacyMayOmit = false,
    string? OmittedWarning = null);

/// <summary>Exact migrations from parallel scalar coordinate fields to typed spatial values (#628).</summary>
internal static class DebuggerSpatialFieldMigrations
{
    private const string DoorOpeningScrollState = "SuperMetroid.Core.Runtime.DoorOpeningScrollState";
    private const string LoaderSamusPlacement = "SuperMetroid.Core.Runtime.LoaderSamusPlacement";
    private const string DoorOpeningPpuScroll = "SuperMetroid.Core.Runtime.DoorOpeningPpuScroll";
    private const string DoorTransitionState = "SuperMetroid.Core.Frontend.DoorTransitionState";

    internal static readonly DebuggerSpatialFieldMigration[] All =
    [
        // The camera's layer-one 16.16 position, ideal position and 16.16 speed words.
        Fixed(typeof(ScrollBoundaryCamera).FullName!, "<Position>k__BackingField",
            "<XPosition>k__BackingField", "<XSubposition>k__BackingField",
            "<YPosition>k__BackingField", "<YSubposition>k__BackingField"),
        Whole<RoomPixels>(typeof(ScrollBoundaryCamera).FullName!, "<IdealPosition>k__BackingField",
            "<IdealXPosition>k__BackingField", "<IdealYPosition>k__BackingField"),
        new(typeof(ScrollBoundaryCamera).FullName!, "<Speed>k__BackingField",
            ["<CameraXSpeed>k__BackingField", "<CameraXSubspeed>k__BackingField",
             "<CameraYSpeed>k__BackingField", "<CameraYSubspeed>k__BackingField"], Types<ushort>(4),
            (_, v) => FixedVector<RoomPixels>.FromWords(Word(v[0]), Word(v[1]), Word(v[2]), Word(v[3]))),

        // Bank-$80 layer positions, offsets, PPU mirrors and streaming blocks.
        Whole<RoomPixels>(typeof(BackgroundScrollState).FullName!, "<Layer1Position>k__BackingField",
            "<Layer1XPosition>k__BackingField", "<Layer1YPosition>k__BackingField"),
        Whole<Bg2Pixels>(typeof(BackgroundScrollState).FullName!, "<Layer2Position>k__BackingField",
            "<Layer2XPosition>k__BackingField", "<Layer2YPosition>k__BackingField"),
        new(typeof(BackgroundScrollState).FullName!, "<Layer2Scroll>k__BackingField",
            ["<Layer2ScrollX>k__BackingField", "<Layer2ScrollY>k__BackingField"], Types<byte>(2),
            (_, v) => new Layer2ScrollModes(Byte(v[0]), Byte(v[1]))),
        Offset<RoomPixels>(typeof(BackgroundScrollState).FullName!, "<Bg1Offset>k__BackingField",
            "<Bg1XOffset>k__BackingField", "<Bg1YOffset>k__BackingField"),
        Offset<Bg2Pixels>(typeof(BackgroundScrollState).FullName!, "<Bg2Offset>k__BackingField",
            "<Bg2XOffset>k__BackingField", "<Bg2YOffset>k__BackingField"),
        Whole<RoomPixels>(typeof(BackgroundScrollState).FullName!, "<Bg1Scroll>k__BackingField",
            "<Bg1HorizontalScroll>k__BackingField", "<Bg1VerticalScroll>k__BackingField"),
        Whole<Bg2Pixels>(typeof(BackgroundScrollState).FullName!, "<Bg2Scroll>k__BackingField",
            "<Bg2HorizontalScroll>k__BackingField", "<Bg2VerticalScroll>k__BackingField"),
        Whole<TilemapBlocks>(typeof(BackgroundScrollState).FullName!, "<Bg1Block>k__BackingField",
            "<Bg1XBlock>k__BackingField", "<Bg1YBlock>k__BackingField"),
        Whole<TilemapBlocks>(typeof(BackgroundScrollState).FullName!, "<Bg2Block>k__BackingField",
            "<Bg2XBlock>k__BackingField", "<Bg2YBlock>k__BackingField"),
        Whole<RoomBlocks>(typeof(BackgroundScrollState).FullName!, "<Layer1Block>k__BackingField",
            "<Layer1XBlock>k__BackingField", "<Layer1YBlock>k__BackingField"),
        Whole<RoomBlocks>(typeof(BackgroundScrollState).FullName!, "<Layer2Block>k__BackingField",
            "<Layer2XBlock>k__BackingField", "<Layer2YBlock>k__BackingField"),
        Whole<RoomBlocks>(typeof(BackgroundScrollState).FullName!, "<PreviousLayer1Block>k__BackingField",
            "<PreviousLayer1XBlock>k__BackingField", "<PreviousLayer1YBlock>k__BackingField"),
        Whole<RoomBlocks>(typeof(BackgroundScrollState).FullName!, "<PreviousLayer2Block>k__BackingField",
            "<PreviousLayer2XBlock>k__BackingField", "<PreviousLayer2YBlock>k__BackingField"),

        // The door-opening IRQ trajectory. The old magnitude-only step takes its direction from
        // the restored Direction field.
        new(DoorOpeningScrollState, "<SamusIrqStep>k__BackingField", ["<SamusStep>k__BackingField"], Types<uint>(1),
            (state, v) => OrientedStep(state, Raw(v[0]))),
        Whole<RoomPixels>(DoorOpeningScrollState, "<CameraPosition>k__BackingField",
            "<CameraX>k__BackingField", "<CameraY>k__BackingField"),
        Whole<Bg2Pixels>(DoorOpeningScrollState, "<Layer2Position>k__BackingField",
            "<Layer2X>k__BackingField", "<Layer2Y>k__BackingField"),
        RawFixed(DoorOpeningScrollState, "<SamusPosition>k__BackingField",
            "<SamusXFixed>k__BackingField", "<SamusYFixed>k__BackingField"),
        Whole<RoomPixels>(DoorOpeningScrollState, "<FinalCameraPosition>k__BackingField",
            "<FinalCameraX>k__BackingField", "<FinalCameraY>k__BackingField"),
        Whole<Bg2Pixels>(DoorOpeningScrollState, "<FinalLayer2Position>k__BackingField",
            "<FinalLayer2X>k__BackingField", "<FinalLayer2Y>k__BackingField"),
        RawFixed(DoorOpeningScrollState, "<FinalSamusPosition>k__BackingField",
            "<FinalSamusXFixed>k__BackingField", "<FinalSamusYFixed>k__BackingField"),
        Whole<RoomPixels>(LoaderSamusPlacement, "<Position>k__BackingField",
            "<XPosition>k__BackingField", "<YPosition>k__BackingField"),
        Whole<RoomPixels>(DoorOpeningPpuScroll, "<Bg1Scroll>k__BackingField",
            "<Bg1Horizontal>k__BackingField", "<Bg1Vertical>k__BackingField"),
        RawFixed(DoorTransitionState, "sourceSamus", "sourceSamusXFixed", "sourceSamusYFixed"),

        // The PPU words latched by one accepted gameplay NMI.
        Whole<RoomPixels>(typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot).FullName!, "<Layer1Position>k__BackingField",
            "<Layer1XPosition>k__BackingField", "<Layer1YPosition>k__BackingField"),
        Whole<RoomPixels>(typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot).FullName!, "<Bg1Scroll>k__BackingField",
            "<Bg1HorizontalScroll>k__BackingField", "<Bg1VerticalScroll>k__BackingField"),
        Whole<Bg2Pixels>(typeof(SuperMetroid.Core.Runtime.GameplayPpuRenderSnapshot).FullName!, "<Bg2Scroll>k__BackingField",
            "<Bg2HorizontalScroll>k__BackingField", "<Bg2VerticalScroll>k__BackingField"),

        // #1275 layer-one fractions that outlive a released runtime. Published layouts predate
        // them; older builds cleared the fractions, which the zero default reproduces.
        Fractions(typeof(SuperMetroid.Core.Runtime.SuperMetroidRuntime).FullName!, "<InheritedLayer1Position>k__BackingField",
            "<InheritedLayer1XSubposition>k__BackingField", "<InheritedLayer1YSubposition>k__BackingField"),
        Fractions(typeof(SuperMetroidGame).FullName!, "retainedLayer1Position",
            "retainedLayer1XSubposition", "retainedLayer1YSubposition"),

        // The scroll-zone grid's room size, the BG3 room-FX scroll state and the Ceres shaft pivot.
        Size<RoomScreens>(typeof(RoomScrollGrid).FullName!, "<Size>k__BackingField",
            "<WidthInScreens>k__BackingField", "<HeightInScreens>k__BackingField"),
        Whole<Bg3Pixels>(typeof(RoomLayer3FxState).FullName!, "<Scroll>k__BackingField",
            "<HorizontalScroll>k__BackingField", "<VerticalScroll>k__BackingField"),
        Offset<Bg3Subpixels>(typeof(RoomLayer3FxState).FullName!, "drift",
            "horizontalAccumulator", "verticalAccumulator"),
        Whole<RoomPixels>(typeof(RoomLayer3FxState).FullName!, "previousCamera",
            "previousCameraX", "previousCameraY"),
        Whole<Bg3Pixels>(typeof(RoomLayer3FxRenderSnapshot).FullName!, "<Scroll>k__BackingField",
            "<HorizontalScroll>k__BackingField", "<VerticalScroll>k__BackingField"),
        Whole<RoomPixels>(typeof(SamusMode7Transform).FullName!, "<Center>k__BackingField",
            "<CenterX>k__BackingField", "<CenterY>k__BackingField"),
        Signed<RoomPixels>(typeof(RoomShakeFrameResult).FullName!, "<Bg1>k__BackingField",
            "<Bg1X>k__BackingField", "<Bg1Y>k__BackingField"),
        Signed<Bg2Pixels>(typeof(RoomShakeFrameResult).FullName!, "<Bg2>k__BackingField",
            "<Bg2X>k__BackingField", "<Bg2Y>k__BackingField"),

        // Captured display packets retained by the frontend: PPU register pairs and map sizes.
        Size<ScreenPixels>(typeof(SuperMetroid.Core.Rendering.RenderFrameSnapshot).FullName!, "<Size>k__BackingField",
            "<Width>k__BackingField", "<Height>k__BackingField"),
        Whole<BgPlanePixels>(typeof(SuperMetroid.Core.Rendering.Bg4BppRenderLayer).FullName!, "<Scroll>k__BackingField",
            "<HorizontalScroll>k__BackingField", "<VerticalScroll>k__BackingField"),
        Size<TilemapCells>(typeof(SuperMetroid.Core.Rendering.Bg4BppRenderLayer).FullName!, "<MapSize>k__BackingField",
            "<MapWidthTiles>k__BackingField", "<MapHeightTiles>k__BackingField"),
        Whole<Bg2Pixels>(typeof(SuperMetroid.Core.Rendering.Mode1FloorBand).FullName!, "<Scroll>k__BackingField",
            "<HorizontalScroll>k__BackingField", "<VerticalScroll>k__BackingField"),
        Size<TilemapCells>(typeof(SuperMetroid.Core.Rendering.Mode1FloorBand).FullName!, "<MapSize>k__BackingField",
            "<MapWidthTiles>k__BackingField", "<MapHeightTiles>k__BackingField"),
        Signed<Mode7Pixels>(typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters).FullName!, "<Center>k__BackingField",
            "<CenterX>k__BackingField", "<CenterY>k__BackingField"),
        Signed<Mode7Pixels>(typeof(SuperMetroid.Core.Rendering.Mode7RenderRegisters).FullName!, "<Scroll>k__BackingField",
            "<HorizontalOffset>k__BackingField", "<VerticalOffset>k__BackingField"),
        Whole<RoomPixels>(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, "<Bg1Scroll>k__BackingField",
            "<Bg1X>k__BackingField", "<Bg1Y>k__BackingField"),
        Whole<Bg2Pixels>(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, "<Bg2Scroll>k__BackingField",
            "<Bg2X>k__BackingField", "<Bg2Y>k__BackingField"),
        Size<TilemapCells>(typeof(SuperMetroid.Core.Rendering.OrdinaryGameplayRegisters).FullName!, "<Bg2MapSize>k__BackingField",
            "<Bg2WidthTiles>k__BackingField", "<Bg2HeightTiles>k__BackingField"),

        // Room, door and load-station catalog records retained in captured room state.
        BytePair<RoomBlocks>(typeof(CartridgeDoorHeader).FullName!, BackingField("PlmBlock"), BackingField("PlmX"), BackingField("PlmY")),
        BytePair<RoomScreens>(typeof(CartridgeDoorHeader).FullName!, BackingField("DestinationScreen"),
            BackingField("DestinationScreenX"), BackingField("DestinationScreenY")),
        BytePair<MapTiles>(typeof(CartridgeRoomHeader).FullName!, BackingField("MapPosition"), BackingField("MapX"), BackingField("MapY")),
        new(typeof(CartridgeRoomHeader).FullName!, BackingField("Size"),
            [BackingField("WidthInScreens"), BackingField("HeightInScreens")], Types<byte>(2),
            (_, v) => new Extent16<RoomScreens>(Byte(v[0]), Byte(v[1]))),
        new(typeof(CartridgeRoomState).FullName!, BackingField("Layer2Scroll"),
            [BackingField("Layer2ScrollX"), BackingField("Layer2ScrollY")], Types<byte>(2),
            (_, v) => new Layer2ScrollModes(Byte(v[0]), Byte(v[1]))),
        BytePair<MapTiles>(typeof(LandingSiteEntryState).FullName!, BackingField("RoomMapPosition"),
            BackingField("RoomMapX"), BackingField("RoomMapY")),
        Whole<RoomPixels>(typeof(LoadStationEntry).FullName!, BackingField("CameraPosition"), BackingField("CameraX"), BackingField("CameraY")),
        // The native table stores both offsets as words; X is a two's-complement displacement.
        Offset<RoomPixels>(typeof(LoadStationEntry).FullName!, BackingField("SamusOffset"),
            BackingField("SamusXOffset"), BackingField("SamusYOffset")),
        Whole<RoomPixels>(typeof(AttractDemoScene).FullName!, BackingField("CameraPosition"), BackingField("CameraX"), BackingField("CameraY")),
        new(typeof(AttractDemoScene).FullName!, BackingField("SamusOffset"),
            [BackingField("SamusXFromCenter"), BackingField("SamusYFromTop")], [typeof(short), typeof(ushort)],
            (_, v) => new Offset16<RoomPixels>(Signed(v[0]), unchecked((short)Word(v[1])))),
        Size<RoomBlocks>(typeof(RoomLevelData).FullName!, BackingField("Size"), BackingField("WidthInBlocks"), BackingField("HeightInBlocks")),
        Size<RoomBlocks>(typeof(RoomVisualLayout).FullName!, BackingField("Size"), BackingField("WidthInBlocks"), BackingField("HeightInBlocks")),

        // Area-map views.
        Whole<MapPixels>(typeof(FileSelectMapScroll).FullName!, BackingField("Scroll"), BackingField("Horizontal"), BackingField("Vertical")),
        Whole<MapPixels>(typeof(FileSelectMapScroll).FullName!, BackingField("Minimum"), BackingField("MinimumX"), BackingField("MinimumY")),
        Whole<MapPixels>(typeof(FileSelectMapScroll).FullName!, BackingField("Maximum"), BackingField("MaximumX"), BackingField("MaximumY")),
        Whole<MapPixels>(typeof(FileSelectStationMarker).FullName!, BackingField("MapPosition"), BackingField("MapX"), BackingField("MapY")),
        BytePair<MapTiles>(PauseMenuState, "roomMapPosition", "roomMapX", "roomMapY"),
        Whole<MapPixels>(PauseMenuState, "mapScrollPosition", "mapHorizontalScroll", "mapVerticalScroll"),
        Whole<OamPixels>(PauseMenuState, "lastIndicatorOrigin", "lastIndicatorOriginX", "lastIndicatorOriginY"),
        // Captured primary-constructor parameters serialize as their compiler-named fields.
        Whole<MapPixels>(PauseMapScroll, "<minimum>P", "<minimumX>P", "<minimumY>P"),
        Whole<MapPixels>(PauseMapScroll, "<maximum>P", "<maximumX>P", "<maximumY>P"),
        Whole<OamPixels>(MapArrow, "Position", "X", "Y"),

        // PLM blocks and projectile requests. 571b9a42 implemented Samus Eater capture; earlier
        // builds did not execute it, and a fresh plant setup supplies the coordinates if caught.
        Whole<RoomPixels>(PlmSlot, BackingField("PlantHeldPosition"), BackingField("PlantHeldX"), BackingField("PlantHeldY")) with
        {
            LegacyMayOmit = true,
            OmittedWarning = "Legacy PLM slot lacks plant-held coordinates; restoring zero until a new capture setup.",
        },
        BytePair<RoomBlocks>(typeof(BombTorizoStatueProjectileRequest).FullName!, BackingField("PlmBlock"),
            BackingField("PlmBlockX"), BackingField("PlmBlockY")),
        BytePair<RoomBlocks>(typeof(MotherBrainGlassProjectileRequest).FullName!, BackingField("PlmBlock"),
            BackingField("PlmBlockX"), BackingField("PlmBlockY")),

        // Cinematic sprites and backgrounds: whole and subpixel words of both axes.
        FixedWords<ScreenPixels>(Frontend + "IntroDiscoverySprite", BackingField("Position"),
            BackingField("XPosition"), BackingField("XSubPosition"), BackingField("YPosition"), BackingField("YSubPosition")),
        FixedWords<Mode7Pixels>(Frontend + "CeresDestructionCinematicState", "background",
            "backgroundX", "backgroundXSubPosition", "backgroundY", "backgroundYSubPosition"),
        FixedWords<Mode7Pixels>(Frontend + "IntroCeresFlightState", "background",
            "backgroundX", "backgroundXSubPosition", "backgroundY", "backgroundYSubPosition"),
        FixedWords<ScreenPixels>(Frontend + "EndingShootingStar", "Position", "X", "XSubposition", "Y", "YSubposition"),
        // The ending's Mode 7 Y never had a subpixel word; its restored fraction is zero.
        new(Frontend + "EndingCreditsState", "mode7", ["mode7X", "mode7XSubposition", "mode7Y"], Types<ushort>(3),
            (_, v) => FixedPosition<Mode7Pixels>.FromWords(Word(v[0]), Word(v[1]), Word(v[2]), 0)),
        // The title state kept sign-extended whole words and zero-extended fractions in ints.
        new(Frontend + "TitleSequenceState", "mode7", ["mode7X", "mode7XSubposition", "mode7Y", "mode7YSubposition"], Types<int>(4),
            (_, v) => FixedPosition<Mode7Pixels>.FromWords(
                unchecked((ushort)Int(v[0])), checked((ushort)Int(v[1])), unchecked((ushort)Int(v[2])), checked((ushort)Int(v[3])))),
        Whole<ScreenPixels>(Frontend + "TitleSequenceState", "activeOrigin", "activeOriginX", "activeOriginY"),
        Whole<RoomPixels>(Frontend + "IntroScientistCutsceneState", BackingField("Background"), BackingField("BackgroundX"), BackingField("BackgroundY")),
        Whole<ScreenPixels>(Frontend + "IntroCinematicObjectSystem", BackingField("Caret"), BackingField("CaretX"), BackingField("CaretY")),
        Whole<ScreenPixels>(Frontend + "IntroMotherBrainExplosionSystem+ExplosionActor", BackingField("Position"),
            BackingField("XPosition"), BackingField("YPosition")),
        new(Frontend + "CinematicTextGlowSystem+GlowSlot", "Cell", ["X", "Y"], Types<int>(2),
            (_, v) => new Position16<TilemapCells>(checked((ushort)Int(v[0])), checked((ushort)Int(v[1])))),
        Size<TilemapCells>(Frontend + "CinematicTextGlowSystem+GlowSlot", "Size", "Width", "Height"),
        // Native signed 8.8 velocity words, kept bit for bit.
        new(Frontend + "IntroBabyDiscoveryState", BackingField("BabyVelocity"),
            [BackingField("BabyXVelocity"), BackingField("BabyYVelocity")], Types<ushort>(2), (_, v) => EightEight(v)),
        new(Frontend + "EndingShootingStar", "Velocity", ["XVelocity", "YVelocity"], Types<ushort>(2), (_, v) => EightEight(v)),
    ];

    private static readonly Dictionary<(string Type, string Field), DebuggerSpatialFieldMigration> ByLegacyField =
        All.SelectMany(migration => migration.LegacyFields.Select(field => (Key: (migration.TypeName, field), migration)))
            .ToDictionary(entry => entry.Key, entry => entry.migration);

    /// <summary>One introduction per typed field, rebuilding it from its drained legacy fields.</summary>
    internal static IEnumerable<DebuggerFieldIntroduction> Introductions =>
        All.Select(migration => new DebuggerFieldIntroduction(migration.TypeName, [migration.Field], null,
            instance => Rebuild(instance, migration)));

    /// <summary>True when <paramref name="field"/> of <paramref name="declaringType"/> is a retired spatial component.</summary>
    internal static bool IsLegacyField(Type declaringType, string field) =>
        ByLegacyField.ContainsKey((declaringType.FullName!, field));

    private static void Rebuild(object instance, DebuggerSpatialFieldMigration migration)
    {
        var values = new object?[migration.LegacyFields.Length];
        int present = 0;
        for (int index = 0; index < values.Length; index++)
        {
            if (DebuggerRetiredFieldDefinitions.TryGetLegacyValue(instance, migration.LegacyFields[index], out values[index]))
                present++;
        }
        if (present == 0 && migration.LegacyMayOmit)
        {
            if (migration.OmittedWarning is { } warning && ReportedWarnings.TryAdd(warning, true))
                Console.Error.WriteLine($"WARNING: {warning}");
            return;
        }
        if (present != values.Length)
            throw new InvalidDataException(
                $"Legacy {migration.TypeName} names only {present} of the {values.Length} components of {migration.Field}.");
        System.Reflection.FieldInfo field = FindField(instance.GetType(), migration.Field);
        field.SetValue(instance, migration.Rebuild(instance, values));
    }

    private const string PauseMenuState = "SuperMetroid.Core.Frontend.PauseMenuState";
    private const string PauseMapScroll = "SuperMetroid.Core.Frontend.PauseMapScroll";
    private const string MapArrow = "SuperMetroid.Core.Frontend.FileSelectMapAnimations+Arrow";
    private const string PlmSlot = "SuperMetroid.Core.Rooms.RoomPlmSystem+PlmSlot";
    private const string Frontend = "SuperMetroid.Core.Frontend.";

    // Warnings for captures predating a field's legacy components, each reported once per process.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> ReportedWarnings = new();

    // An automatic property's backing field carries the property's name.
    private static string BackingField(string property) => $"<{property}>k__BackingField";

    private static Type[] Types<T>(int count) => Enumerable.Repeat(typeof(T), count).ToArray();

    private static DebuggerSpatialFieldMigration Whole<TSpace>(string type, string field, string x, string y)
        where TSpace : ISpatialSpace =>
        new(type, field, [x, y], Types<ushort>(2), (_, v) => new Position16<TSpace>(Word(v[0]), Word(v[1])));

    private static DebuggerSpatialFieldMigration BytePair<TSpace>(string type, string field, string x, string y)
        where TSpace : ISpatialSpace =>
        new(type, field, [x, y], Types<byte>(2), (_, v) => new Position16<TSpace>(Byte(v[0]), Byte(v[1])));

    private static DebuggerSpatialFieldMigration Offset<TSpace>(string type, string field, string x, string y)
        where TSpace : ISpatialSpace =>
        new(type, field, [x, y], Types<ushort>(2), (_, v) => new Offset16<TSpace>(unchecked((short)Word(v[0])), unchecked((short)Word(v[1]))));

    private static DebuggerSpatialFieldMigration Signed<TSpace>(string type, string field, string x, string y)
        where TSpace : ISpatialSpace =>
        new(type, field, [x, y], Types<short>(2), (_, v) => new Offset16<TSpace>(Signed(v[0]), Signed(v[1])));

    private static DebuggerSpatialFieldMigration Size<TSpace>(string type, string field, string width, string height)
        where TSpace : ISpatialSpace =>
        new(type, field, [width, height], Types<int>(2), (_, v) => new Extent16<TSpace>(Length(v[0]), Length(v[1])));

    private static DebuggerSpatialFieldMigration Fixed(string type, string field, string x, string xFraction, string y, string yFraction) =>
        FixedWords<RoomPixels>(type, field, x, xFraction, y, yFraction);

    // Legacy layouts stored X whole, X subpixel, Y whole and Y subpixel words, in that order.
    private static DebuggerSpatialFieldMigration FixedWords<TSpace>(string type, string field,
        string x, string xFraction, string y, string yFraction)
        where TSpace : ISpatialSpace =>
        new(type, field, [x, xFraction, y, yFraction], Types<ushort>(4),
            (_, v) => FixedPosition<TSpace>.FromWords(Word(v[0]), Word(v[1]), Word(v[2]), Word(v[3])));

    private static DebuggerSpatialFieldMigration RawFixed(string type, string field, string x, string y) =>
        new(type, field, [x, y], Types<uint>(2), (_, v) => new FixedPosition<RoomPixels>(
            SnesFixedPosition.FromRaw(Raw(v[0])), SnesFixedPosition.FromRaw(Raw(v[1]))));

    private static DebuggerSpatialFieldMigration Fractions(string type, string field, string xFraction, string yFraction) =>
        new(type, field, [xFraction, yFraction], Types<ushort>(2),
            (_, v) => FixedPosition<RoomPixels>.FromWords(0, Word(v[0]), 0, Word(v[1])), LegacyMayOmit: true);

    private static EightEightVector<ScreenPixels> EightEight(object?[] v) =>
        new(new SnesSignedEightEight(Word(v[0])), new SnesSignedEightEight(Word(v[1])));

    private static int Int(object? value) => value as int? ??
        throw new InvalidDataException($"Legacy spatial component {value?.GetType().Name ?? "null"} is not an int.");

    private static FixedVector<RoomPixels> OrientedStep(object state, uint magnitude)
    {
        var direction = (DoorDirection)FindField(state.GetType(), "<Direction>k__BackingField").GetValue(state)!;
        var forward = SnesSignedSixteenSixteen.FromRaw(unchecked((int)magnitude));
        var back = SnesSignedSixteenSixteen.FromRaw(unchecked(-(int)magnitude));
        return direction switch
        {
            DoorDirection.Right => new(forward, SnesSignedSixteenSixteen.Zero),
            DoorDirection.Left => new(back, SnesSignedSixteenSixteen.Zero),
            DoorDirection.Down => new(SnesSignedSixteenSixteen.Zero, forward),
            DoorDirection.Up => new(SnesSignedSixteenSixteen.Zero, back),
            _ => throw new InvalidDataException($"Legacy door-opening scroll has undefined direction {direction}."),
        };
    }

    private static ushort Word(object? value) => value as ushort? ??
        throw new InvalidDataException($"Legacy spatial component {value?.GetType().Name ?? "null"} is not a word.");

    private static short Signed(object? value) => value as short? ??
        throw new InvalidDataException($"Legacy signed register {value?.GetType().Name ?? "null"} is not a signed word.");

    private static ushort Length(object? value) => value is int length and >= 0 and <= ushort.MaxValue
        ? (ushort)length
        : throw new InvalidDataException($"Legacy size component {value ?? "null"} is not a 16-bit length.");

    private static uint Raw(object? value) => value as uint? ??
        throw new InvalidDataException($"Legacy 16.16 component {value?.GetType().Name ?? "null"} is not a 32-bit word.");

    private static byte Byte(object? value) => value as byte? ??
        throw new InvalidDataException($"Legacy scroll mode {value?.GetType().Name ?? "null"} is not a byte.");

    private static System.Reflection.FieldInfo FindField(Type type, string field) =>
        type.GetField(field, System.Reflection.BindingFlags.Instance | System.Reflection.BindingFlags.Public |
            System.Reflection.BindingFlags.NonPublic)
        ?? throw new InvalidOperationException($"{type.FullName} has no field {field}.");
}

/// <summary>
/// The retired four-word <c>SamusCameraPoint</c> layout. Older captures name that type for the
/// camera's and enemies' Samus scrolling checkpoints; it restores only to be converted to the
/// typed 16.16 position that replaced it.
/// </summary>
[SpatialComponents("Debugger codec: the exact four serialized words of a retired checkpoint layout.")]
internal readonly record struct LegacySamusCameraPoint(
    ushort XPosition,
    ushort XSubposition,
    ushort YPosition,
    ushort YSubposition)
{
    /// <summary>The serialized identity of the retired Core type.</summary>
    internal const string RetiredTypeName = "SuperMetroid.Core.Game.SamusCameraPoint";

    /// <summary>The typed 16.16 position with the same four words.</summary>
    internal FixedPosition<RoomPixels> ToFixedPosition() =>
        FixedPosition<RoomPixels>.FromWords(XPosition, XSubposition, YPosition, YSubposition);
}
