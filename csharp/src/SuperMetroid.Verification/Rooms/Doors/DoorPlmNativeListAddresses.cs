using SuperMetroid.Verification.Bosses.MotherBrain;
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
    internal static IReadOnlyList<NativeProgramEntry<BlueDoorProgram>> BlueEntries { get; } =
    [
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.LeftOpening, 0xc489),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.LeftClosing, 0xc49e),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.LeftClosed, 0xc4b1),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.RightOpening, 0xc4ba),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.RightClosing, 0xc4cf),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.RightClosed, 0xc4e2),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.UpOpening, 0xc4eb),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.UpClosing, 0xc500),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.UpClosed, 0xc513),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.DownOpening, 0xc51c),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.DownClosing, 0xc531),
        new NativeProgramEntry<BlueDoorProgram>(BlueDoorProgram.DownClosed, 0xc544),
    ];

    /// <summary>The yellow cap facing left's lists (<c>InstList_PLM_YellowDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> YellowLeftEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xbffd),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc014),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc024),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc02a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc04b),
    ];

    /// <summary>The yellow cap facing right's lists (<c>InstList_PLM_YellowDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> YellowRightEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc060),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc077),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc087),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc08d),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc0ae),
    ];

    /// <summary>The yellow cap facing up's lists (<c>InstList_PLM_YellowDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> YellowUpEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc0c3),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc0da),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc0ea),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc0ec),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc10d),
    ];

    /// <summary>The yellow cap facing down's lists (<c>InstList_PLM_YellowDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> YellowDownEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc122),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc139),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc14d),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc14f),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc170),
    ];

    /// <summary>The green cap facing left's lists (<c>InstList_PLM_GreenDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> GreenLeftEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc185),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc19c),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc1ac),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc1ae),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc1cf),
    ];

    /// <summary>The green cap facing right's lists (<c>InstList_PLM_GreenDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> GreenRightEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc1e4),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc1fb),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc20b),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc20d),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc22e),
    ];

    /// <summary>The green cap facing up's lists (<c>InstList_PLM_GreenDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> GreenUpEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc243),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc25a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc26a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc26c),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc28d),
    ];

    /// <summary>The green cap facing down's lists (<c>InstList_PLM_GreenDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> GreenDownEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc2a2),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc2b9),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc2c9),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc2cb),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc2ec),
    ];

    /// <summary>The red cap facing left's lists (<c>InstList_PLM_RedDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> RedLeftEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc301),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc318),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc328),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc32a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc34e),
    ];

    /// <summary>The red cap facing right's lists (<c>InstList_PLM_RedDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> RedRightEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc363),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc37a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc38a),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc38c),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc3b0),
    ];

    /// <summary>The red cap facing up's lists (<c>InstList_PLM_RedDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> RedUpEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc3c5),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc3dc),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc3ec),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc3ee),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc412),
    ];

    /// <summary>The red cap facing down's lists (<c>InstList_PLM_RedDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> RedDownEntries =
    [
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Closing, 0xc427),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Waiting, 0xc43e),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Sleep, 0xc44e),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Hit, 0xc450),
        new NativeProgramEntry<ColoredDoorProgram>(ColoredDoorProgram.Open, 0xc474),
    ];

    /// <summary>The grey cap facing left's lists (<c>InstList_PLM_GreyDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> GreyLeftEntries =
    [
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Closing, 0xbe59),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Waiting, 0xbe70),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Sleep, 0xbe7e),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Activate, 0xbe84),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Flash, 0xbe8c),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Hit, 0xbea8),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Open, 0xbead),
    ];

    /// <summary>The grey cap facing right's lists (<c>InstList_PLM_GreyDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> GreyRightEntries =
    [
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Closing, 0xbec2),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Waiting, 0xbed9),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Sleep, 0xbee7),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Activate, 0xbeed),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Flash, 0xbef5),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Hit, 0xbf11),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Open, 0xbf16),
    ];

    /// <summary>The grey cap facing up's lists (<c>InstList_PLM_GreyDoorFacingUp_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> GreyUpEntries =
    [
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Closing, 0xbf2b),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Waiting, 0xbf42),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Sleep, 0xbf50),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Activate, 0xbf56),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Flash, 0xbf5e),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Hit, 0xbf7a),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Open, 0xbf7f),
    ];

    /// <summary>The grey cap facing down's lists (<c>InstList_PLM_GreyDoorFacingDown_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> GreyDownEntries =
    [
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Closing, 0xbf94),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Waiting, 0xbfab),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Sleep, 0xbfb9),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Activate, 0xbfbf),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Flash, 0xbfc7),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Hit, 0xbfe3),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Open, 0xbfe8),
    ];

    /// <summary>Bomb Torizo's grey door's lists (<c>InstList_PLM_BombTorizoGreyDoor*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> BombTorizoEntries =
    [
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Closing, 0xba4c),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Waiting, 0xba7f),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Sleep, 0xba8d),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Activate, 0xba93),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Flash, 0xba9b),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Hit, 0xbab7),
        new NativeProgramEntry<GreyDoorProgram>(GreyDoorProgram.Open, 0xbabc),
    ];

    /// <summary>The left-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeLeftEyeEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeStart, 0xd81e),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeIdle, 0xd822),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeActivate, 0xd830),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeWatch, 0xd83c),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeClose, 0xd878),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeHit, 0xd880),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeDestroyed, 0xd8c4),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeOpening, 0xd8d3),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeOpened, 0xd8e3),
    ];

    /// <summary>The left-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeLeftDoorEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentStart, 0xd8e9),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentIdle, 0xd8ed),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentActivate, 0xd8fb),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentCycle, 0xd903),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentDelete, 0xd91d),
    ];

    /// <summary>The left-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingLeft_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeLeftBottomEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentStart, 0xd91f),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentIdle, 0xd923),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentActivate, 0xd931),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentCycle, 0xd939),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentDelete, 0xd953),
    ];

    /// <summary>The right-facing eye door eye's lists (<c>InstList_PLM_EyeDoorEyeFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeRightEyeEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeStart, 0xd955),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeIdle, 0xd959),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeActivate, 0xd967),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeWatch, 0xd973),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeClose, 0xd9af),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeHit, 0xd9b7),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeDestroyed, 0xd9fb),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeOpening, 0xda0a),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.EyeOpened, 0xda1a),
    ];

    /// <summary>The right-facing eye door door's lists (<c>InstList_PLM_EyeDoorFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeRightDoorEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentStart, 0xda20),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentIdle, 0xda24),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentActivate, 0xda32),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentCycle, 0xda3a),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentDelete, 0xda54),
    ];

    /// <summary>The right-facing eye door bottom's lists (<c>InstList_PLM_EyeDoorBottomFacingRight_*</c>).</summary>
    private static readonly IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeRightBottomEntries =
    [
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentStart, 0xda56),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentIdle, 0xda5a),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentActivate, 0xda68),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentCycle, 0xda70),
        new NativeProgramEntry<EyeDoorPlmProgram>(EyeDoorPlmProgram.ComponentDelete, 0xda8a),
    ];

    /// <summary>The <paramref name="color"/> cap facing <paramref name="orientation"/>'s lists.</summary>
    internal static IReadOnlyList<NativeProgramEntry<ColoredDoorProgram>> ColoredEntries(ColoredDoorColor color, ColoredDoorOrientation orientation) =>
        new ColoredEntriesCase(color, orientation) switch
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
    internal static IReadOnlyList<NativeProgramEntry<GreyDoorProgram>> GreyEntries(GreyDoorVariant variant) =>
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
    internal static IReadOnlyList<NativeProgramEntry<EyeDoorPlmProgram>> EyeDoorEntries(EyeDoorOrientation orientation, EyeDoorComponent component) =>
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
