namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The saved-game area and room maps' definitions (#1416): the entry and return windows' geometry and timing, each Zebes
/// area's reveal motion (#2084), the room view's frame offset and the markers' colourings.
/// </summary>
/// <remarks>
/// A reveal opens the window from the area's label towards the screen's edges: each edge has its own authored speed, the
/// edge farthest from the label moving four pixels an update, and the edges stop on the reveal's last moving update.
/// </remarks>
public static class FileSelectMapDefinitions
{
    /// <summary>Where the area map's title is drawn, centred above the areas' labels.</summary>
    internal static Offset16<ScreenPixels> WorldTitlePosition => new(128, 16);

    /// <summary>The width and height of one map square, in map pixels: each of a room's screens fills one square.</summary>
    public const int MapSquarePixels = 8;

    /// <summary>The room view's frame drawn this many pixels lower, aligning it with the visible map viewport.</summary>
    public const int RoomFrameVerticalScroll = 24;

    /// <summary>Updates the return window's area timer is shortened by.</summary>
    public const int ReturnWindowTimerReduction = 12;

    /// <summary>Pixels inside the viewport the return window starts.</summary>
    public const int ReturnWindowInset = 8;

    /// <summary>Updates the load prelude takes before the screen fades.</summary>
    public const int LoadPreludeFrames = 2;

    /// <summary>Updates the load's fade to black takes from full brightness: one brightness step each update.</summary>
    public const int LoadFadeFrames = 15;

    /// <summary>Black updates the load waits after the fade before gameplay setup begins.</summary>
    public const int LoadBlackFrames = 32;

    /// <summary>Updates the return setup takes before the window contracts.</summary>
    public const int ReturnSetupFrames = 6;

    /// <summary>Updates the map's entry holds the screen black before its reveal window opens.</summary>
    public const int EntryBlackHoldUpdates = 20;

    /// <summary>The entry window's upper margin when it opens, in screen rows.</summary>
    public const int EntryWindowTop = 111;

    /// <summary>The entry window's inclusive left edge when it opens.</summary>
    public const int EntryWindowLeft = 127;

    /// <summary>The entry window's inclusive right edge when it opens.</summary>
    public const int EntryWindowRight = 129;

    /// <summary>Pixels each entry-window edge expands per update.</summary>
    public const int EntryWindowSpeed = 4;

    /// <summary>
    /// Crateria's reveal: its four edges move outward on 52 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion CrateriaReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromPixelFraction(-113, 64),
        Right: SignedSixteenSixteen.FromPixelFraction(205, 64),
        Top: SignedSixteenSixteen.FromPixelFraction(-31, 32),
        Bottom: SignedSixteenSixteen.FromWholePixels(4));

    /// <summary>
    /// Brinstar's reveal: its four edges move outward on 54 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion BrinstarReveal { get; } = new(
        MovingUpdates: 54,
        Left: SignedSixteenSixteen.FromPixelFraction(-25, 32),
        Right: SignedSixteenSixteen.FromWholePixels(4),
        Top: SignedSixteenSixteen.FromPixelFraction(-151, 64),
        Bottom: SignedSixteenSixteen.FromPixelFraction(77, 32));

    /// <summary>
    /// Norfair's reveal: its four edges move outward on 46 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion NorfairReveal { get; } = new(
        MovingUpdates: 46,
        Left: SignedSixteenSixteen.FromPixelFraction(-33, 16),
        Right: SignedSixteenSixteen.FromPixelFraction(229, 64),
        Top: SignedSixteenSixteen.FromWholePixels(-4),
        Bottom: SignedSixteenSixteen.FromPixelFraction(53, 32));

    /// <summary>
    /// Wrecked Ship's reveal: its four edges move outward on 52 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion WreckedShipReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromWholePixels(-4),
        Right: SignedSixteenSixteen.FromPixelFraction(31, 32),
        Top: SignedSixteenSixteen.FromPixelFraction(-99, 64),
        Bottom: SignedSixteenSixteen.FromPixelFraction(109, 32));

    /// <summary>
    /// Maridia's reveal: its four edges move outward on 52 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion MaridiaReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromWholePixels(-4),
        Right: SignedSixteenSixteen.FromPixelFraction(31, 32),
        Top: SignedSixteenSixteen.FromPixelFraction(-197, 64),
        Bottom: SignedSixteenSixteen.FromPixelFraction(15, 8));

    /// <summary>
    /// Tourian's reveal: its four edges move outward on 35 updates, each at its own speed per update.
    /// </summary>
    internal static FileSelectMapWindowMotion TourianReveal { get; } = new(
        MovingUpdates: 35,
        Left: SignedSixteenSixteen.FromPixelFraction(-31, 8),
        Right: SignedSixteenSixteen.FromPixelFraction(111, 32),
        Top: SignedSixteenSixteen.FromWholePixels(-4),
        Bottom: SignedSixteenSixteen.FromPixelFraction(215, 64));
}
