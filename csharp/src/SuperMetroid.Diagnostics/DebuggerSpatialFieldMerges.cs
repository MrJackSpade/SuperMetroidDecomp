using System.Reflection;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

// Legacy namespace is persisted in debugger identities; ownership is now platform-neutral.
namespace SuperMetroid.Desktop;

/// <summary>
/// One spatial value whose components an older debugger layout stored as separate fields (#628).
/// The legacy component fields are retired and remembered; once the instance is restored the value
/// field is composed from them exactly, so no information is lost.
/// </summary>
/// <param name="TypeName">Full name of the declaring type.</param>
/// <param name="Field">Serialized name of the current spatial value field.</param>
/// <param name="LegacyFields">Serialized names of the retired component fields, in composition order.</param>
/// <param name="LegacyType">Exact payload type the older layout wrote for every component.</param>
/// <param name="Compose">Builds the current value from the remembered legacy component values.</param>
/// <param name="PredatesComponents">
/// For a field whose components were themselves introduced by a later build: the value restored, with
/// its warning, for an older layout that omits every component. Null when every legacy layout stores them.
/// </param>
internal sealed record DebuggerSpatialFieldMerge(
    string TypeName, string Field, string[] LegacyFields, Type[] LegacyType, Func<object?[], object> Compose,
    DebuggerSpatialFieldOrigin? PredatesComponents = null);

/// <summary>The restored value and warning for a layout older than a merged field's components.</summary>
internal sealed record DebuggerSpatialFieldOrigin(object Value, string Warning);

/// <summary>Explicit inventory of debugger fields merged into spatial values (#628).</summary>
internal static class DebuggerSpatialFieldMerges
{
    private const string PauseMenuState = "SuperMetroid.Core.Frontend.PauseMenuState";
    private const string PauseMapScroll = "SuperMetroid.Core.Frontend.PauseMapScroll";
    private const string MapArrow = "SuperMetroid.Core.Frontend.FileSelectMapAnimations+Arrow";
    private const string PlmSlot = "SuperMetroid.Core.Rooms.RoomPlmSystem+PlmSlot";
    private const string Frontend = "SuperMetroid.Core.Frontend.";

    /// <summary>Every merged field, by declaring type.</summary>
    internal static readonly DebuggerSpatialFieldMerge[] Merges =
    [
        Bytes(typeof(CartridgeDoorHeader), "PlmBlock", "PlmX", "PlmY", (x, y) => new Point32<RoomBlocks>(x, y)),
        Bytes(typeof(CartridgeDoorHeader), "DestinationScreen", "DestinationScreenX", "DestinationScreenY",
            (x, y) => new Point32<RoomScreens>(x, y)),
        Bytes(typeof(CartridgeRoomHeader), "MapPosition", "MapX", "MapY", (x, y) => new Point32<MapTiles>(x, y)),
        Bytes(typeof(CartridgeRoomHeader), "Size", "WidthInScreens", "HeightInScreens", (x, y) => new Size32<RoomScreens>(x, y)),
        Bytes(typeof(CartridgeRoomState), "Layer2Scroll", "Layer2ScrollX", "Layer2ScrollY", (x, y) => new Layer2ScrollFactors(x, y)),
        Bytes(typeof(LandingSiteEntryState), "RoomMapPosition", "RoomMapX", "RoomMapY", (x, y) => new Point32<MapTiles>(x, y)),
        Words(typeof(LoadStationEntry), "CameraPosition", "CameraX", "CameraY", (x, y) => new Position16<RoomPixels>(x, y)),
        // The native table stores both offsets as words; X is a two's-complement displacement.
        Words(typeof(LoadStationEntry), "SamusOffset", "SamusXOffset", "SamusYOffset",
            (x, y) => new Offset16<RoomPixels>(unchecked((short)x), unchecked((short)y))),
        Words(typeof(AttractDemoScene), "CameraPosition", "CameraX", "CameraY", (x, y) => new Position16<RoomPixels>(x, y)),
        new(typeof(AttractDemoScene).FullName!, BackingField("SamusOffset"),
            [BackingField("SamusXFromCenter"), BackingField("SamusYFromTop")], [typeof(short), typeof(ushort)],
            v => new Offset16<RoomPixels>((short)v[0]!, unchecked((short)(ushort)v[1]!))),
        Ints(typeof(RoomLevelData), "Size", "WidthInBlocks", "HeightInBlocks", (x, y) => new Size32<RoomBlocks>(x, y)),
        Ints(typeof(RoomVisualLayout), "Size", "WidthInBlocks", "HeightInBlocks", (x, y) => new Size32<RoomBlocks>(x, y)),
        Words(typeof(FileSelectMapScroll), "Scroll", "Horizontal", "Vertical", (x, y) => new Position16<MapPixels>(x, y)),
        Words(typeof(FileSelectMapScroll), "Minimum", "MinimumX", "MinimumY", (x, y) => new Position16<MapPixels>(x, y)),
        Words(typeof(FileSelectMapScroll), "Maximum", "MaximumX", "MaximumY", (x, y) => new Position16<MapPixels>(x, y)),
        Words(typeof(FileSelectStationMarker), "MapPosition", "MapX", "MapY", (x, y) => new Position16<MapPixels>(x, y)),
        Fields(PauseMenuState, "roomMapPosition", "roomMapX", "roomMapY", typeof(byte), v => new Point32<MapTiles>((byte)v[0]!, (byte)v[1]!)),
        Fields(PauseMenuState, "mapScrollPosition", "mapHorizontalScroll", "mapVerticalScroll", typeof(ushort),
            v => new Position16<MapPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(PauseMenuState, "lastIndicatorOrigin", "lastIndicatorOriginX", "lastIndicatorOriginY", typeof(ushort),
            v => new Position16<ScreenPixels>((ushort)v[0]!, (ushort)v[1]!)),
        // Captured primary-constructor parameters serialize as their compiler-named fields.
        Fields(PauseMapScroll, "<minimum>P", "<minimumX>P", "<minimumY>P", typeof(ushort),
            v => new Position16<MapPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(PauseMapScroll, "<maximum>P", "<maximumX>P", "<maximumY>P", typeof(ushort),
            v => new Position16<MapPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(MapArrow, "Position", "X", "Y", typeof(ushort), v => new Position16<ScreenPixels>((ushort)v[0]!, (ushort)v[1]!)),
        // 571b9a42 implemented Samus Eater capture. Earlier builds did not execute that capture;
        // fresh plant setup supplies the coordinates if the player is subsequently caught.
        Fields(PlmSlot, "<PlantHeldPosition>k__BackingField", "<PlantHeldX>k__BackingField", "<PlantHeldY>k__BackingField",
            typeof(ushort), v => new Position16<RoomPixels>((ushort)v[0]!, (ushort)v[1]!)) with
        {
            PredatesComponents = new(new Position16<RoomPixels>(0, 0),
                "Legacy PLM slot lacks plant-held coordinates; restoring zero until a new capture setup."),
        },
        Bytes(typeof(BombTorizoStatueProjectileRequest), "PlmBlock", "PlmBlockX", "PlmBlockY", (x, y) => new Point32<RoomBlocks>(x, y)),
        Bytes(typeof(MotherBrainGlassProjectileRequest), "PlmBlock", "PlmBlockX", "PlmBlockY", (x, y) => new Point32<RoomBlocks>(x, y)),
        // Cinematic sprites and backgrounds: whole and subpixel words of both axes.
        FixedWords<ScreenPixels>(Frontend + "IntroDiscoverySprite", BackingField("Position"),
            BackingField("XPosition"), BackingField("XSubPosition"), BackingField("YPosition"), BackingField("YSubPosition")),
        FixedWords<Mode7Pixels>(Frontend + "CeresDestructionCinematicState", "background",
            "backgroundX", "backgroundXSubPosition", "backgroundY", "backgroundYSubPosition"),
        FixedWords<Mode7Pixels>(Frontend + "IntroCeresFlightState", "background",
            "backgroundX", "backgroundXSubPosition", "backgroundY", "backgroundYSubPosition"),
        FixedWords<ScreenPixels>(Frontend + "EndingShootingStar", "Position", "X", "XSubposition", "Y", "YSubposition"),
        // The ending's Mode 7 Y never had a subpixel word; its restored fraction is zero.
        new(Frontend + "EndingCreditsState", "mode7", ["mode7X", "mode7XSubposition", "mode7Y"],
            [typeof(ushort), typeof(ushort), typeof(ushort)],
            v => new FixedPosition<Mode7Pixels>(new((ushort)v[0]!, (ushort)v[1]!), new((ushort)v[2]!, 0))),
        // The title state kept sign-extended whole words and zero-extended fractions in ints.
        new(Frontend + "TitleSequenceState", "mode7", ["mode7X", "mode7XSubposition", "mode7Y", "mode7YSubposition"],
            [typeof(int), typeof(int), typeof(int), typeof(int)],
            v => new FixedPosition<Mode7Pixels>(
                new(unchecked((ushort)(int)v[0]!), checked((ushort)(int)v[1]!)),
                new(unchecked((ushort)(int)v[2]!), checked((ushort)(int)v[3]!)))),
        Fields(Frontend + "TitleSequenceState", "activeOrigin", "activeOriginX", "activeOriginY", typeof(ushort),
            v => new Position16<ScreenPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(Frontend + "IntroScientistCutsceneState", BackingField("Background"), BackingField("BackgroundX"), BackingField("BackgroundY"),
            typeof(ushort), v => new Position16<RoomPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(Frontend + "IntroCinematicObjectSystem", BackingField("Caret"), BackingField("CaretX"), BackingField("CaretY"),
            typeof(ushort), v => new Position16<ScreenPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(Frontend + "IntroMotherBrainExplosionSystem+ExplosionActor", BackingField("Position"),
            BackingField("XPosition"), BackingField("YPosition"), typeof(ushort),
            v => new Position16<ScreenPixels>((ushort)v[0]!, (ushort)v[1]!)),
        Fields(Frontend + "CinematicTextGlowSystem+GlowSlot", "Cell", "X", "Y", typeof(int),
            v => new Point32<TilemapCells>((int)v[0]!, (int)v[1]!)),
        Fields(Frontend + "CinematicTextGlowSystem+GlowSlot", "Size", "Width", "Height", typeof(int),
            v => new Size32<TilemapCells>((int)v[0]!, (int)v[1]!)),
        // Native signed 8.8 velocity words, kept bit for bit.
        Fields(Frontend + "IntroBabyDiscoveryState", BackingField("BabyVelocity"),
            BackingField("BabyXVelocity"), BackingField("BabyYVelocity"), typeof(ushort), EightEightWords),
        Fields(Frontend + "EndingShootingStar", "Velocity", "XVelocity", "YVelocity", typeof(ushort), EightEightWords),
    ];

    /// <summary>The current spatial fields, introduced for layouts that stored their components.</summary>
    internal static IEnumerable<DebuggerFieldIntroduction> Introductions =>
        Merges.Select(merge => new DebuggerFieldIntroduction(merge.TypeName, [merge.Field], null,
            instance => Restore(instance, merge)));

    /// <summary>(Declaring type full name, field name) of every retired component field.</summary>
    internal static IEnumerable<(string Type, string Field)> RetiredFields =>
        Merges.SelectMany(merge => merge.LegacyFields.Select(legacy => (merge.TypeName, legacy)));

    // An automatic property's backing field carries the property's name.
    private static DebuggerSpatialFieldMerge Bytes(Type type, string property, string x, string y, Func<byte, byte, object> compose) =>
        new(type.FullName!, BackingField(property), [BackingField(x), BackingField(y)], [typeof(byte), typeof(byte)],
            values => compose((byte)values[0]!, (byte)values[1]!));

    private static DebuggerSpatialFieldMerge Words(Type type, string property, string x, string y, Func<ushort, ushort, object> compose) =>
        new(type.FullName!, BackingField(property), [BackingField(x), BackingField(y)], [typeof(ushort), typeof(ushort)],
            values => compose((ushort)values[0]!, (ushort)values[1]!));

    private static DebuggerSpatialFieldMerge Ints(Type type, string property, string x, string y, Func<int, int, object> compose) =>
        new(type.FullName!, BackingField(property), [BackingField(x), BackingField(y)], [typeof(int), typeof(int)],
            values => compose((int)values[0]!, (int)values[1]!));

    private static DebuggerSpatialFieldMerge Fields(string typeName, string field, string x, string y, Type componentType,
        Func<object?[], object> compose) =>
        new(typeName, field, [x, y], [componentType, componentType], compose);

    private static string BackingField(string property) => $"<{property}>k__BackingField";

    // Legacy layouts stored X whole, X subpixel, Y whole and Y subpixel words, in that order.
    private static DebuggerSpatialFieldMerge FixedWords<TSpace>(string typeName, string field,
        string xWhole, string xFraction, string yWhole, string yFraction)
        where TSpace : ISpatialSpace =>
        new(typeName, field, [xWhole, xFraction, yWhole, yFraction],
            [typeof(ushort), typeof(ushort), typeof(ushort), typeof(ushort)],
            v => new FixedPosition<TSpace>(new((ushort)v[0]!, (ushort)v[1]!), new((ushort)v[2]!, (ushort)v[3]!)));

    private static object EightEightWords(object?[] v) =>
        new EightEightVector<ScreenPixels>(
            new SuperMetroid.Core.Hardware.SnesSignedEightEight((ushort)v[0]!),
            new SuperMetroid.Core.Hardware.SnesSignedEightEight((ushort)v[1]!));

    // Warnings for layouts that predate a field's components, each reported once per process.
    private static readonly System.Collections.Concurrent.ConcurrentDictionary<string, bool> ReportedWarnings = new();

    private static void Restore(object instance, DebuggerSpatialFieldMerge merge)
    {
        var values = new object?[merge.LegacyFields.Length];
        var supplied = new bool[values.Length];
        for (int index = 0; index < values.Length; index++)
            supplied[index] = DebuggerRetiredFieldDefinitions.TryGetLegacyValue(instance, merge.LegacyFields[index], out values[index]);
        if (merge.PredatesComponents is { } origin && !supplied.Contains(true))
        {
            if (ReportedWarnings.TryAdd(origin.Warning, true))
                Console.Error.WriteLine($"WARNING: {origin.Warning}");
            FindField(instance.GetType(), merge.Field).SetValue(instance, origin.Value);
            return;
        }
        for (int index = 0; index < values.Length; index++)
        {
            if (!supplied[index])
                throw new InvalidDataException(
                    $"Legacy {merge.TypeName} omits {merge.Field} but does not supply its component {merge.LegacyFields[index]}.");
            if (values[index]?.GetType() != merge.LegacyType[index])
                throw new InvalidDataException(
                    $"Legacy {merge.TypeName}.{merge.LegacyFields[index]} is not a {merge.LegacyType[index].Name}.");
        }
        FindField(instance.GetType(), merge.Field).SetValue(instance, merge.Compose(values));
    }

    private static FieldInfo FindField(Type type, string field)
    {
        for (Type? current = type; current is not null; current = current.BaseType)
            if (current.GetField(field, BindingFlags.Instance | BindingFlags.Public | BindingFlags.NonPublic | BindingFlags.DeclaredOnly) is { } found)
                return found;
        throw new InvalidDataException($"Spatial migration field {type.FullName}.{field} no longer exists.");
    }
}
