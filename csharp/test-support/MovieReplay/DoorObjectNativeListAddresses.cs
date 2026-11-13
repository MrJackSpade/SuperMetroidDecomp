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
    private static readonly Dictionary<ColoredDoorOrientation, NativeBlueDoorLists> BlueLists = new Dictionary<ColoredDoorOrientation, NativeBlueDoorLists>
    {
        [ColoredDoorOrientation.Left] = new(Opening: 0xc489, Closing: 0xc49e, Closed: 0xc4b1),
        [ColoredDoorOrientation.Right] = new(Opening: 0xc4ba, Closing: 0xc4cf, Closed: 0xc4e2),
        [ColoredDoorOrientation.Up] = new(Opening: 0xc4eb, Closing: 0xc500, Closed: 0xc513),
        [ColoredDoorOrientation.Down] = new(Opening: 0xc51c, Closing: 0xc531, Closed: 0xc544),
    };

    /// <summary>Each colour and facing's coloured cap lists (<c>InstList_PLM_{Yellow,Green,Red}DoorFacing*</c>).</summary>
    private static readonly Dictionary<ColoredDoorVariant, NativeColoredDoorLists> ColoredLists = new Dictionary<ColoredDoorVariant, NativeColoredDoorLists>
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

    /// <summary>Each facing's eye-door eye lists (<c>InstList_PLM_EyeDoorEyeFacing*</c>).</summary>
    private static readonly Dictionary<EyeDoorOrientation, NativeEyeDoorEyeLists> EyeLists = new Dictionary<EyeDoorOrientation, NativeEyeDoorEyeLists>
    {
        [EyeDoorOrientation.Left] = new(0xd81e, 0xd822, 0xd830, 0xd83c, 0xd878, 0xd880, 0xd8c4, 0xd8d3, 0xd8e3),
        [EyeDoorOrientation.Right] = new(0xd955, 0xd959, 0xd967, 0xd973, 0xd9af, 0xd9b7, 0xd9fb, 0xda0a, 0xda1a),
    };

    /// <summary>Each facing's eye-door body and bottom lists (<c>InstList_PLM_EyeDoorFacing*</c>, <c>InstList_PLM_EyeDoorBottomFacing*</c>).</summary>
    private static readonly Dictionary<EyeDoorPart, NativeEyeDoorPartLists> PartLists = new Dictionary<EyeDoorPart, NativeEyeDoorPartLists>
    {
        [new(EyeDoorComponent.Door, EyeDoorOrientation.Left)] = new(0xd8e9, 0xd8ed, 0xd8fb, 0xd903, 0xd91d),
        [new(EyeDoorComponent.Bottom, EyeDoorOrientation.Left)] = new(0xd91f, 0xd923, 0xd931, 0xd939, 0xd953),
        [new(EyeDoorComponent.Door, EyeDoorOrientation.Right)] = new(0xda20, 0xda24, 0xda32, 0xda3a, 0xda54),
        [new(EyeDoorComponent.Bottom, EyeDoorOrientation.Right)] = new(0xda56, 0xda5a, 0xda68, 0xda70, 0xda8a),
    };

    /// <summary>The blue cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeBlueDoorLists Blue(ColoredDoorOrientation orientation) => BlueLists[orientation];

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeColoredDoorLists Colored(ColoredDoorColor color, ColoredDoorOrientation orientation) =>
        ColoredLists[new ColoredDoorVariant(color, orientation)];

    /// <summary>Grey cap <paramref name="variant"/>'s lists.</summary>
    internal static NativeGreyDoorLists Grey(GreyDoorVariant variant) => GreyLists[variant];

    /// <summary>Every grey cap: the four facings and Bomb Torizo's door.</summary>
    internal static IReadOnlyList<GreyDoorVariant> GreyVariants { get; } = [.. GreyLists.Keys];

    /// <summary>The eye-door eye facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeEyeDoorEyeLists Eye(EyeDoorOrientation orientation) => EyeLists[orientation];

    /// <summary>The eye-door <paramref name="component"/> (body or bottom) facing <paramref name="orientation"/>'s lists.</summary>
    internal static NativeEyeDoorPartLists Part(EyeDoorOrientation orientation, EyeDoorComponent component) =>
        PartLists[new EyeDoorPart(component, orientation)];

    /// <summary>The native closing list a coloured or grey cap's header names, or null for a header that is not one.</summary>
    internal static ushort? TypedClosingEntry(RoomObjectKind header)
    {
        if (DoorObjects.TryIdentifyColoredDoor(header, out ColoredDoorColor color, out ColoredDoorOrientation coloredOrientation))
            return Colored(color, coloredOrientation).Closing;
        if (GreyDoorObjects.TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
            return Grey(new GreyDoorVariant(greyOrientation, header == RoomObjectKind.BombTorizoGreyDoor)).Closing;
        return null;
    }
}
