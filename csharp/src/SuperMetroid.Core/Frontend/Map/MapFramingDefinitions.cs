namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The map screens' framing (#1975): how file select and the pause screen each frame the area map, and the rules both
/// share, in map and screen pixels.
/// </summary>
public static partial class MapFramingDefinitions
{
    /// <summary>
    /// File select's room map: the revealed rows centred higher on the screen than the pause screen's, and the up arrow's
    /// limit nearer the screen's top.
    /// </summary>
    public static MapFraming FileSelect { get; } =
        new(CentreLine: 112, LeftEdge: 24, RightEdge: 232, TopEdge: 40, BottomEdge: 145, ArrowDrop: 0);

    /// <summary>
    /// The pause screen's map page: the revealed rows centred on the screen, the arrows' limits inside its frame, and the
    /// arrows drawn below file select's.
    /// </summary>
    public static MapFraming Pause { get; } =
        new(CentreLine: 128, LeftEdge: 24, RightEdge: 232, TopEdge: 56, BottomEdge: 177, ArrowDrop: 24);

    /// <summary>The leftmost revealed column a map takes when none of its columns is revealed.</summary>
    public const int UnrevealedLeftColumn = 26;

    /// <summary>The rightmost revealed column a map takes when none of its columns is revealed.</summary>
    public const int UnrevealedRightColumn = 28;

    /// <summary>The top revealed row a map takes when none of its rows is revealed.</summary>
    public const int UnrevealedTopRow = 1;

    /// <summary>The bottom revealed row a map takes when none of its rows is revealed.</summary>
    public const int UnrevealedBottomRow = 11;

    /// <summary>The screen column the middle of the revealed columns is centred on.</summary>
    public const int CentreColumn = 128;

    /// <summary>Map pixels the middle of the revealed rows lies above the line the framing centres.</summary>
    public const int RowMiddleAboveCentredLine = 16;

    /// <summary>The framing moves the map so the player's marker lies at or right of this screen column.</summary>
    public const int MarkerLeftmostColumn = 32;

    /// <summary>The framing moves the map so the player's marker lies at or left of this screen column.</summary>
    public const int MarkerRightmostColumn = 224;

    /// <summary>The framing moves the map so the player's marker lies at or below this screen line, unless that would scroll the map's top further down than <see cref="LowestMapTop"/>.</summary>
    public const int MarkerTopLine = 64;

    /// <summary>The farthest the framing scrolls the map's top below the screen's top, in map pixels.</summary>
    public const int LowestMapTop = 40;
}
