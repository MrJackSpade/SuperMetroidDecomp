using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Verification.Rooms.Doors;

/// <summary>
/// The native bank-$84 lists of the door PLMs typed by #1296, for the ROM oracles that walk them beside the typed
/// programs. Only verification names these lists.
/// </summary>
internal static class DoorPlmNativeListAddresses
{
    /// <summary>The blue caps' lists (<c>InstList_PLM_BlueDoorFacing*</c>, $84:C489..C54C).</summary>
    internal static IReadOnlyList<(BlueDoorProgram Program, ushort Entry)> BlueEntries { get; } =
    [
        (BlueDoorProgram.LeftOpening, 0xc489),
        (BlueDoorProgram.LeftClosing, 0xc49e),
        (BlueDoorProgram.LeftClosed, 0xc4b1),
        (BlueDoorProgram.RightOpening, 0xc4ba),
        (BlueDoorProgram.RightClosing, 0xc4cf),
        (BlueDoorProgram.RightClosed, 0xc4e2),
        (BlueDoorProgram.UpOpening, 0xc4eb),
        (BlueDoorProgram.UpClosing, 0xc500),
        (BlueDoorProgram.UpClosed, 0xc513),
        (BlueDoorProgram.DownOpening, 0xc51c),
        (BlueDoorProgram.DownClosing, 0xc531),
        (BlueDoorProgram.DownClosed, 0xc544),
    ];

    /// <summary>The yellow cap facing left's lists (<c>InstList_PLM_YellowDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> YellowLeftEntries =
    [
        (ColoredDoorProgram.Closing, 0xbffd),
        (ColoredDoorProgram.Waiting, 0xc014),
        (ColoredDoorProgram.Sleep, 0xc024),
        (ColoredDoorProgram.Hit, 0xc02a),
        (ColoredDoorProgram.Open, 0xc04b),
    ];

    /// <summary>The yellow cap facing right's lists (<c>InstList_PLM_YellowDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> YellowRightEntries =
    [
        (ColoredDoorProgram.Closing, 0xc060),
        (ColoredDoorProgram.Waiting, 0xc077),
        (ColoredDoorProgram.Sleep, 0xc087),
        (ColoredDoorProgram.Hit, 0xc08d),
        (ColoredDoorProgram.Open, 0xc0ae),
    ];

    /// <summary>The yellow cap facing up's lists (<c>InstList_PLM_YellowDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> YellowUpEntries =
    [
        (ColoredDoorProgram.Closing, 0xc0c3),
        (ColoredDoorProgram.Waiting, 0xc0da),
        (ColoredDoorProgram.Sleep, 0xc0ea),
        (ColoredDoorProgram.Hit, 0xc0ec),
        (ColoredDoorProgram.Open, 0xc10d),
    ];

    /// <summary>The yellow cap facing down's lists (<c>InstList_PLM_YellowDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> YellowDownEntries =
    [
        (ColoredDoorProgram.Closing, 0xc122),
        (ColoredDoorProgram.Waiting, 0xc139),
        (ColoredDoorProgram.Sleep, 0xc14d),
        (ColoredDoorProgram.Hit, 0xc14f),
        (ColoredDoorProgram.Open, 0xc170),
    ];

    /// <summary>The green cap facing left's lists (<c>InstList_PLM_GreenDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> GreenLeftEntries =
    [
        (ColoredDoorProgram.Closing, 0xc185),
        (ColoredDoorProgram.Waiting, 0xc19c),
        (ColoredDoorProgram.Sleep, 0xc1ac),
        (ColoredDoorProgram.Hit, 0xc1ae),
        (ColoredDoorProgram.Open, 0xc1cf),
    ];

    /// <summary>The green cap facing right's lists (<c>InstList_PLM_GreenDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> GreenRightEntries =
    [
        (ColoredDoorProgram.Closing, 0xc1e4),
        (ColoredDoorProgram.Waiting, 0xc1fb),
        (ColoredDoorProgram.Sleep, 0xc20b),
        (ColoredDoorProgram.Hit, 0xc20d),
        (ColoredDoorProgram.Open, 0xc22e),
    ];

    /// <summary>The green cap facing up's lists (<c>InstList_PLM_GreenDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> GreenUpEntries =
    [
        (ColoredDoorProgram.Closing, 0xc243),
        (ColoredDoorProgram.Waiting, 0xc25a),
        (ColoredDoorProgram.Sleep, 0xc26a),
        (ColoredDoorProgram.Hit, 0xc26c),
        (ColoredDoorProgram.Open, 0xc28d),
    ];

    /// <summary>The green cap facing down's lists (<c>InstList_PLM_GreenDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> GreenDownEntries =
    [
        (ColoredDoorProgram.Closing, 0xc2a2),
        (ColoredDoorProgram.Waiting, 0xc2b9),
        (ColoredDoorProgram.Sleep, 0xc2c9),
        (ColoredDoorProgram.Hit, 0xc2cb),
        (ColoredDoorProgram.Open, 0xc2ec),
    ];

    /// <summary>The red cap facing left's lists (<c>InstList_PLM_RedDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> RedLeftEntries =
    [
        (ColoredDoorProgram.Closing, 0xc301),
        (ColoredDoorProgram.Waiting, 0xc318),
        (ColoredDoorProgram.Sleep, 0xc328),
        (ColoredDoorProgram.Hit, 0xc32a),
        (ColoredDoorProgram.Open, 0xc34e),
    ];

    /// <summary>The red cap facing right's lists (<c>InstList_PLM_RedDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> RedRightEntries =
    [
        (ColoredDoorProgram.Closing, 0xc363),
        (ColoredDoorProgram.Waiting, 0xc37a),
        (ColoredDoorProgram.Sleep, 0xc38a),
        (ColoredDoorProgram.Hit, 0xc38c),
        (ColoredDoorProgram.Open, 0xc3b0),
    ];

    /// <summary>The red cap facing up's lists (<c>InstList_PLM_RedDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> RedUpEntries =
    [
        (ColoredDoorProgram.Closing, 0xc3c5),
        (ColoredDoorProgram.Waiting, 0xc3dc),
        (ColoredDoorProgram.Sleep, 0xc3ec),
        (ColoredDoorProgram.Hit, 0xc3ee),
        (ColoredDoorProgram.Open, 0xc412),
    ];

    /// <summary>The red cap facing down's lists (<c>InstList_PLM_RedDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> RedDownEntries =
    [
        (ColoredDoorProgram.Closing, 0xc427),
        (ColoredDoorProgram.Waiting, 0xc43e),
        (ColoredDoorProgram.Sleep, 0xc44e),
        (ColoredDoorProgram.Hit, 0xc450),
        (ColoredDoorProgram.Open, 0xc474),
    ];

    /// <summary>The grey cap facing left's lists (<c>InstList_PLM_GreyDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> GreyLeftEntries =
    [
        (GreyDoorProgram.Closing, 0xbe59),
        (GreyDoorProgram.Waiting, 0xbe70),
        (GreyDoorProgram.Sleep, 0xbe7e),
        (GreyDoorProgram.Activate, 0xbe84),
        (GreyDoorProgram.Flash, 0xbe8c),
        (GreyDoorProgram.Hit, 0xbea8),
        (GreyDoorProgram.Open, 0xbead),
    ];

    /// <summary>The grey cap facing right's lists (<c>InstList_PLM_GreyDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> GreyRightEntries =
    [
        (GreyDoorProgram.Closing, 0xbec2),
        (GreyDoorProgram.Waiting, 0xbed9),
        (GreyDoorProgram.Sleep, 0xbee7),
        (GreyDoorProgram.Activate, 0xbeed),
        (GreyDoorProgram.Flash, 0xbef5),
        (GreyDoorProgram.Hit, 0xbf11),
        (GreyDoorProgram.Open, 0xbf16),
    ];

    /// <summary>The grey cap facing up's lists (<c>InstList_PLM_GreyDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> GreyUpEntries =
    [
        (GreyDoorProgram.Closing, 0xbf2b),
        (GreyDoorProgram.Waiting, 0xbf42),
        (GreyDoorProgram.Sleep, 0xbf50),
        (GreyDoorProgram.Activate, 0xbf56),
        (GreyDoorProgram.Flash, 0xbf5e),
        (GreyDoorProgram.Hit, 0xbf7a),
        (GreyDoorProgram.Open, 0xbf7f),
    ];

    /// <summary>The grey cap facing down's lists (<c>InstList_PLM_GreyDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> GreyDownEntries =
    [
        (GreyDoorProgram.Closing, 0xbf94),
        (GreyDoorProgram.Waiting, 0xbfab),
        (GreyDoorProgram.Sleep, 0xbfb9),
        (GreyDoorProgram.Activate, 0xbfbf),
        (GreyDoorProgram.Flash, 0xbfc7),
        (GreyDoorProgram.Hit, 0xbfe3),
        (GreyDoorProgram.Open, 0xbfe8),
    ];

    /// <summary>Bomb Torizo's grey door's lists (<c>InstList_PLM_BombTorizoGreyDoor*</c>).</summary>
    private static readonly IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> BombTorizoEntries =
    [
        (GreyDoorProgram.Closing, 0xba4c),
        (GreyDoorProgram.Waiting, 0xba7f),
        (GreyDoorProgram.Sleep, 0xba8d),
        (GreyDoorProgram.Activate, 0xba93),
        (GreyDoorProgram.Flash, 0xba9b),
        (GreyDoorProgram.Hit, 0xbab7),
        (GreyDoorProgram.Open, 0xbabc),
    ];

    /// <summary>The left-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeLeftEyeEntries =
    [
        (EyeDoorPlmProgram.EyeStart, 0xd81e),
        (EyeDoorPlmProgram.EyeIdle, 0xd822),
        (EyeDoorPlmProgram.EyeActivate, 0xd830),
        (EyeDoorPlmProgram.EyeWatch, 0xd83c),
        (EyeDoorPlmProgram.EyeClose, 0xd878),
        (EyeDoorPlmProgram.EyeHit, 0xd880),
        (EyeDoorPlmProgram.EyeDestroyed, 0xd8c4),
        (EyeDoorPlmProgram.EyeOpening, 0xd8d3),
        (EyeDoorPlmProgram.EyeOpened, 0xd8e3),
    ];

    /// <summary>The left-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeLeftDoorEntries =
    [
        (EyeDoorPlmProgram.ComponentStart, 0xd8e9),
        (EyeDoorPlmProgram.ComponentIdle, 0xd8ed),
        (EyeDoorPlmProgram.ComponentActivate, 0xd8fb),
        (EyeDoorPlmProgram.ComponentCycle, 0xd903),
        (EyeDoorPlmProgram.ComponentDelete, 0xd91d),
    ];

    /// <summary>The left-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeLeftBottomEntries =
    [
        (EyeDoorPlmProgram.ComponentStart, 0xd91f),
        (EyeDoorPlmProgram.ComponentIdle, 0xd923),
        (EyeDoorPlmProgram.ComponentActivate, 0xd931),
        (EyeDoorPlmProgram.ComponentCycle, 0xd939),
        (EyeDoorPlmProgram.ComponentDelete, 0xd953),
    ];

    /// <summary>The right-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeRightEyeEntries =
    [
        (EyeDoorPlmProgram.EyeStart, 0xd955),
        (EyeDoorPlmProgram.EyeIdle, 0xd959),
        (EyeDoorPlmProgram.EyeActivate, 0xd967),
        (EyeDoorPlmProgram.EyeWatch, 0xd973),
        (EyeDoorPlmProgram.EyeClose, 0xd9af),
        (EyeDoorPlmProgram.EyeHit, 0xd9b7),
        (EyeDoorPlmProgram.EyeDestroyed, 0xd9fb),
        (EyeDoorPlmProgram.EyeOpening, 0xda0a),
        (EyeDoorPlmProgram.EyeOpened, 0xda1a),
    ];

    /// <summary>The right-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeRightDoorEntries =
    [
        (EyeDoorPlmProgram.ComponentStart, 0xda20),
        (EyeDoorPlmProgram.ComponentIdle, 0xda24),
        (EyeDoorPlmProgram.ComponentActivate, 0xda32),
        (EyeDoorPlmProgram.ComponentCycle, 0xda3a),
        (EyeDoorPlmProgram.ComponentDelete, 0xda54),
    ];

    /// <summary>The right-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeRightBottomEntries =
    [
        (EyeDoorPlmProgram.ComponentStart, 0xda56),
        (EyeDoorPlmProgram.ComponentIdle, 0xda5a),
        (EyeDoorPlmProgram.ComponentActivate, 0xda68),
        (EyeDoorPlmProgram.ComponentCycle, 0xda70),
        (EyeDoorPlmProgram.ComponentDelete, 0xda8a),
    ];

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static IReadOnlyList<(ColoredDoorProgram Program, ushort Entry)> ColoredEntries(ColoredDoorColor color, ColoredDoorOrientation orientation) =>
        (color, orientation) switch
        {
            (ColoredDoorColor.Yellow, ColoredDoorOrientation.Left) => YellowLeftEntries,
            (ColoredDoorColor.Yellow, ColoredDoorOrientation.Right) => YellowRightEntries,
            (ColoredDoorColor.Yellow, ColoredDoorOrientation.Up) => YellowUpEntries,
            (ColoredDoorColor.Yellow, ColoredDoorOrientation.Down) => YellowDownEntries,
            (ColoredDoorColor.Green, ColoredDoorOrientation.Left) => GreenLeftEntries,
            (ColoredDoorColor.Green, ColoredDoorOrientation.Right) => GreenRightEntries,
            (ColoredDoorColor.Green, ColoredDoorOrientation.Up) => GreenUpEntries,
            (ColoredDoorColor.Green, ColoredDoorOrientation.Down) => GreenDownEntries,
            (ColoredDoorColor.Red, ColoredDoorOrientation.Left) => RedLeftEntries,
            (ColoredDoorColor.Red, ColoredDoorOrientation.Right) => RedRightEntries,
            (ColoredDoorColor.Red, ColoredDoorOrientation.Up) => RedUpEntries,
            (ColoredDoorColor.Red, ColoredDoorOrientation.Down) => RedDownEntries,
            _ => throw new InvalidOperationException($"Undefined coloured cap {color} {orientation}."),
        };

    /// <summary>Grey cap <paramref name="variant"/>'s lists.</summary>
    internal static IReadOnlyList<(GreyDoorProgram Program, ushort Entry)> GreyEntries(GreyDoorVariant variant) =>
        variant.BombTorizo ? BombTorizoEntries : variant.Orientation switch
        {
            ColoredDoorOrientation.Left => GreyLeftEntries,
            ColoredDoorOrientation.Right => GreyRightEntries,
            ColoredDoorOrientation.Up => GreyUpEntries,
            ColoredDoorOrientation.Down => GreyDownEntries,
            _ => throw new InvalidOperationException($"Undefined {nameof(ColoredDoorOrientation)} {variant.Orientation}."),
        };

    /// <summary>Every grey cap: the four facings and Bomb Torizo's door.</summary>
    internal static IReadOnlyList<GreyDoorVariant> GreyVariants { get; } =
    [
        .. Enum.GetValues<ColoredDoorOrientation>().Select(orientation => new GreyDoorVariant(orientation, BombTorizo: false)),
        new GreyDoorVariant(ColoredDoorOrientation.Right, BombTorizo: true),
    ];

    /// <summary>The lists of eye-door <paramref name="component"/> facing <paramref name="orientation"/>.</summary>
    internal static IReadOnlyList<(EyeDoorPlmProgram Program, ushort Entry)> EyeDoorEntries(EyeDoorOrientation orientation, EyeDoorComponent component) =>
        (orientation, component) switch
        {
            (EyeDoorOrientation.Left, EyeDoorComponent.Eye) => EyeLeftEyeEntries,
            (EyeDoorOrientation.Left, EyeDoorComponent.Door) => EyeLeftDoorEntries,
            (EyeDoorOrientation.Left, EyeDoorComponent.Bottom) => EyeLeftBottomEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Eye) => EyeRightEyeEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Door) => EyeRightDoorEntries,
            (EyeDoorOrientation.Right, EyeDoorComponent.Bottom) => EyeRightBottomEntries,
            _ => throw new InvalidOperationException($"Undefined eye-door component {orientation} {component}."),
        };

    /// <summary>The native closing list a typed resident cap's header names, or null for a header that is not a typed cap.</summary>
    internal static ushort? TypedClosingEntry(PlmHeaderId header)
    {
        if (DoorPlms.TryIdentifyColoredDoor(header, out ColoredDoorColor color, out ColoredDoorOrientation coloredOrientation))
            return ColoredEntries(color, coloredOrientation).Single(entry => entry.Program == ColoredDoorProgram.Closing).Entry;
        if (GreyDoorPlms.TryIdentifyGreyDoor(header, out ColoredDoorOrientation greyOrientation))
            return GreyEntries(new GreyDoorVariant(greyOrientation, header == PlmHeaderId.BombTorizoGreyDoor))
                .Single(entry => entry.Program == GreyDoorProgram.Closing).Entry;
        return null;
    }
}
