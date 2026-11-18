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
    /// Crateria's reveal: its edges move on 52 updates, the left edge 452 and the right 820 256ths of a pixel per
    /// update, the top edge 248 and the bottom 1024.
    /// </summary>
    internal static FileSelectMapWindowMotion CrateriaReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-452),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(820),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-248),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1024));

    /// <summary>
    /// Brinstar's reveal: its edges move on 54 updates, the left edge 200 and the right 1024 256ths of a pixel per
    /// update, the top edge 604 and the bottom 616.
    /// </summary>
    internal static FileSelectMapWindowMotion BrinstarReveal { get; } = new(
        MovingUpdates: 54,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-200),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(1024),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-604),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(616));

    /// <summary>
    /// Norfair's reveal: its edges move on 46 updates, the left edge 528 and the right 916 256ths of a pixel per
    /// update, the top edge 1024 and the bottom 424.
    /// </summary>
    internal static FileSelectMapWindowMotion NorfairReveal { get; } = new(
        MovingUpdates: 46,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-528),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(916),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1024),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(424));

    /// <summary>
    /// Wrecked Ship's reveal: its edges move on 52 updates, the left edge 1024 and the right 248 256ths of a pixel per
    /// update, the top edge 396 and the bottom 872.
    /// </summary>
    internal static FileSelectMapWindowMotion WreckedShipReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1024),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(248),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-396),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(872));

    /// <summary>
    /// Maridia's reveal: its edges move on 52 updates, the left edge 1024 and the right 248 256ths of a pixel per
    /// update, the top edge 788 and the bottom 480.
    /// </summary>
    internal static FileSelectMapWindowMotion MaridiaReveal { get; } = new(
        MovingUpdates: 52,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1024),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(248),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-788),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(480));

    /// <summary>
    /// Tourian's reveal: its edges move on 35 updates, the left edge 992 and the right 888 256ths of a pixel per
    /// update, the top edge 1024 and the bottom 860.
    /// </summary>
    internal static FileSelectMapWindowMotion TourianReveal { get; } = new(
        MovingUpdates: 35,
        Left: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-992),
        Right: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(888),
        Top: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(-1024),
        Bottom: SignedSixteenSixteen.FromTwoHundredFiftySixthsOfPixel(860));
}
