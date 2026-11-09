using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.TestSupport.MovieReplay;

/// <summary>
/// The native bank-$84 lists of the door room objects (#1296, #1554), for the ROM oracles that walk them beside the typed
/// doors and for the movie checks that read a recorded door's list. Only verification names these lists.
/// </summary>
internal static class DoorObjectNativeListAddresses
{
    /// <summary>Each facing's blue cap lists (<c>InstList_PLM_BlueDoorFacing*</c>, $84:C489..C54C).</summary>
    private static readonly IReadOnlyDictionary<ColoredDoorOrientation, NativeBlueDoorLists> BlueLists = new Dictionary<ColoredDoorOrientation, NativeBlueDoorLists>
    {
        [ColoredDoorOrientation.Left] = new(Opening: 0xc489, Closing: 0xc49e, Closed: 0xc4b1),
        [ColoredDoorOrientation.Right] = new(Opening: 0xc4ba, Closing: 0xc4cf, Closed: 0xc4e2),
        [ColoredDoorOrientation.Up] = new(Opening: 0xc4eb, Closing: 0xc500, Closed: 0xc513),
        [ColoredDoorOrientation.Down] = new(Opening: 0xc51c, Closing: 0xc531, Closed: 0xc544),
    };

    /// <summary>Each colour and facing's coloured cap lists (<c>InstList_PLM_{Yellow,Green,Red}DoorFacing*</c>).</summary>
    private static readonly IReadOnlyDictionary<ColoredDoorVariant, NativeColoredDoorLists> ColoredLists = new Dictionary<ColoredDoorVariant, NativeColoredDoorLists>
    {
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Left)] = new(0xbffd, 0xc014, 0xc024, 0xc02a, 0xc04b),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Right)] = new(0xc060, 0xc077, 0xc087, 0xc08d, 0xc0ae),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Up)] = new(0xc0c3, 0xc0da, 0xc0ea, 0xc0ec, 0xc10d),
        [new(ColoredDoorColor.Yellow, ColoredDoorOrientation.Down)] = new(0xc122, 0xc139, 0xc14d, 0xc14f, 0xc170),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Left)] = new(0xc185, 0xc19c, 0xc1ac, 0xc1ae, 0xc1cf),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Right)] = new(0xc1e4, 0xc1fb, 0xc20b, 0xc20d, 0xc22e),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Up)] = new(0xc243, 0xc25a, 0xc26a, 0xc26c, 0xc28d),
        [new(ColoredDoorColor.Green, ColoredDoorOrientation.Down)] = new(0xc2a2, 0xc2b9, 0xc2c9, 0xc2cb, 0xc2ec),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Left)] = new(0xc301, 0xc318, 0xc328, 0xc32a, 0xc34e),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Right)] = new(0xc363, 0xc37a, 0xc38a, 0xc38c, 0xc3b0),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Up)] = new(0xc3c5, 0xc3dc, 0xc3ec, 0xc3ee, 0xc412),
        [new(ColoredDoorColor.Red, ColoredDoorOrientation.Down)] = new(0xc427, 0xc43e, 0xc44e, 0xc450, 0xc474),
    };

    /// <summary>Each grey cap's lists (<c>InstList_PLM_GreyDoorFacing*</c> and <c>InstList_PLM_BombTorizoGreyDoor*</c>).</summary>
    private static readonly IReadOnlyDictionary<GreyDoorVariant, NativeGreyDoorLists> GreyLists = new Dictionary<GreyDoorVariant, NativeGreyDoorLists>
    {
        [new(ColoredDoorOrientation.Left, BombTorizo: false)] = new(0xbe59, 0xbe70, 0xbe7e, 0xbe84, 0xbe8c, 0xbea8, 0xbead),
        [new(ColoredDoorOrientation.Right, BombTorizo: false)] = new(0xbec2, 0xbed9, 0xbee7, 0xbeed, 0xbef5, 0xbf11, 0xbf16),
        [new(ColoredDoorOrientation.Up, BombTorizo: false)] = new(0xbf2b, 0xbf42, 0xbf50, 0xbf56, 0xbf5e, 0xbf7a, 0xbf7f),
        [new(ColoredDoorOrientation.Down, BombTorizo: false)] = new(0xbf94, 0xbfab, 0xbfb9, 0xbfbf, 0xbfc7, 0xbfe3, 0xbfe8),
        [new(ColoredDoorOrientation.Right, BombTorizo: true)] = new(0xba4c, 0xba7f, 0xba8d, 0xba93, 0xba9b, 0xbab7, 0xbabc),
    };

    /// <summary>Mother Brain's escape gate placed shut (<c>InstList_PLM_MotherBrainsRoomEscapeDoor_0</c>).</summary>
    internal const ushort EscapeGateShut = 0xbb34;

    /// <summary>Mother Brain's escape gate closing behind Samus (<c>InstList_PLM_MotherBrainsRoomEscapeDoor_1</c>).</summary>
    internal const ushort EscapeGateClosing = 0xbb44;

    /// <summary>The left-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeLeftEyeEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeStart, 0xd81e),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeIdle, 0xd822),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeActivate, 0xd830),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeWatch, 0xd83c),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeClose, 0xd878),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeHit, 0xd880),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeDestroyed, 0xd8c4),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeOpening, 0xd8d3),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeOpened, 0xd8e3),
    ];

    /// <summary>The left-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeLeftDoorEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentStart, 0xd8e9),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentIdle, 0xd8ed),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentActivate, 0xd8fb),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentCycle, 0xd903),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentDelete, 0xd91d),
    ];

    /// <summary>The left-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeLeftBottomEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentStart, 0xd91f),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentIdle, 0xd923),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentActivate, 0xd931),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentCycle, 0xd939),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentDelete, 0xd953),
    ];

    /// <summary>The right-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeRightEyeEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeStart, 0xd955),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeIdle, 0xd959),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeActivate, 0xd967),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeWatch, 0xd973),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeClose, 0xd9af),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeHit, 0xd9b7),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeDestroyed, 0xd9fb),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeOpening, 0xda0a),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.EyeOpened, 0xda1a),
    ];

    /// <summary>The right-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeRightDoorEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentStart, 0xda20),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentIdle, 0xda24),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentActivate, 0xda32),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentCycle, 0xda3a),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentDelete, 0xda54),
    ];

    /// <summary>The right-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeRightBottomEntries =
    [
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentStart, 0xda56),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentIdle, 0xda5a),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentActivate, 0xda68),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentCycle, 0xda70),
        new NativeProgramEntry<EyeDoorObjectProgram>(EyeDoorObjectProgram.ComponentDelete, 0xda8a),
    ];

    /// <summary>The blue cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeBlueDoorLists Blue(ColoredDoorOrientation orientation) => BlueLists[orientation];

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeColoredDoorLists Colored(ColoredDoorColor color, ColoredDoorOrientation orientation) =>
        ColoredLists[new ColoredDoorVariant(color, orientation)];

    /// <summary>Grey cap <paramref name="variant"/>'s lists.</summary>
    internal static NativeGreyDoorLists Grey(GreyDoorVariant variant) => GreyLists[variant];

    /// <summary>Every grey cap: the four facings and Bomb Torizo's door.</summary>
    internal static IReadOnlyList<GreyDoorVariant> GreyVariants { get; } = [.. GreyLists.Keys];

    /// <summary>The lists of eye-door <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static IReadOnlyList<NativeProgramEntry<EyeDoorObjectProgram>> EyeDoorEntries(EyeDoorOrientation orientation, EyeDoorComponent component) =>
        new EyeDoorEntriesCase(orientation, component) switch
        {
            (EyeDoorOrientation.Left, EyeDoorComponent.Eye) => EyeLeftEyeEntries,
            (EyeDoorOrientation.Left, EyeDoorComponent.Door) => EyeLeftDoorEntries,
            (EyeDoorOrientation.Left, EyeDoorComponent.Bottom) => EyeLeftBottomEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Eye) => EyeRightEyeEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Door) => EyeRightDoorEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Bottom) => EyeRightBottomEntries,
            _ => throw new InvalidOperationException($"Undefined eye-door component {orientation} {component}."),
        };

    /// <summary>The native closing list a coloured or grey cap's header names, or null for a header that is not one.</summary>
    internal static ushort? TypedClosingEntry(RoomObjectHeaderId header)
    {
        if (DoorObjects.TryIdentifyColoredDoor(header, out ColoredDoorColor color, out ColoredDoorOrientation coloredOrientation))
            return Colored(color, coloredOrientation).Closing;
        if (GreyDoorObjects.TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
            return Grey(new GreyDoorVariant(greyOrientation, header == RoomObjectHeaderId.BombTorizoGreyDoor)).Closing;
        return null;
    }
}
