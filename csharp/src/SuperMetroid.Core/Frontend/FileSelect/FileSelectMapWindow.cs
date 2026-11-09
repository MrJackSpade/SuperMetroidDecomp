using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// The file-select map's reveal between the area map and a room map (#1555): a rectangle of screen pixels, the room scene
/// showing inside it and the area scene outside. Opening, its four edges start together on the selected area's label and
/// move outward by the area's speeds on each update, each stopping at the screen's edge; returning, they start just inside
/// the screen and move back inward by the same speeds, for fewer updates. The rectangle is not a scale of either scene.
/// </summary>
public sealed class FileSelectMapWindow
{
    /// <summary>The area's edge speeds and the updates the edges move on.</summary>
    private readonly FileSelectMapWindowMotion motion;
    /// <summary>The left edge: the first screen column inside the rectangle, in pixels and fractions of one from the screen's left.</summary>
    private SignedSixteenSixteen left;
    /// <summary>The right edge: the first screen column past the rectangle, in pixels and fractions of one from the screen's left.</summary>
    private SignedSixteenSixteen right;
    /// <summary>The top edge: the first screen line inside the rectangle, in pixels and fractions of one from the screen's top.</summary>
    private SignedSixteenSixteen top;
    /// <summary>The bottom edge: the first screen line past the rectangle, in pixels and fractions of one from the screen's top.</summary>
    private SignedSixteenSixteen bottom;
    /// <summary>The updates the edges still move on; the reveal completes on the last.</summary>
    private int movesLeft;

    /// <summary>Starts the area-to-room reveal at the installed label of <paramref name="area"/>, with the area's compiled motion; editable labels move the rectangle's start, not its motion.</summary>
    /// <param name="area">Game-area identity 0..5 for the six Zebes areas, not the file-select display-order ordinal.</param>
    /// <param name="labels">Required installed world-map label anchors; a missing catalog is rejected even though the parameter defaults to null.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area has no supported transition.</exception>
    /// <exception cref="InvalidOperationException">World-map labels are not installed.</exception>
    public FileSelectMapWindow(int area, WorldMapLabelPositions? labels = null)
        : this(area, FileSelectMapWindowMotions.Get(area), labels) { }

    /// <summary>Explicit motion injection for bounded arithmetic/render fixtures; production uses the compiled catalog.</summary>
    internal FileSelectMapWindow(int area, FileSelectMapWindowMotion motion, WorldMapLabelPositions? labels = null)
    {
        if ((uint)area >= FileSelectMapDefinitions.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        WorldMapLabelPositions installedLabels = labels ?? throw new InvalidOperationException(
            "Map-window positions require installed world-map labels.");
        Position16<SpritePixels> label = installedLabels.Get(area);
        this.motion = motion;
        left = right = SignedSixteenSixteen.FromWholePixels(label.X);
        top = bottom = SignedSixteenSixteen.FromWholePixels(label.Y);
        movesLeft = Ensure.BetweenInclusive(1, int.MaxValue, motion.MovingUpdates);
    }

    /// <summary>The first screen column inside the rectangle.</summary>
    public int Left => left.WholePixels;

    /// <summary>The first screen column past the rectangle; at or left of <see cref="Left"/>, the rectangle holds no column.</summary>
    public int Right => right.WholePixels;

    /// <summary>The first screen line inside the rectangle.</summary>
    public int Top => top.WholePixels;

    /// <summary>The first screen line past the rectangle; at or above <see cref="Top"/>, the rectangle holds no line.</summary>
    public int Bottom => bottom.WholePixels;

    /// <summary>Whether the rectangle holds no screen pixel: its edges meet or have crossed.</summary>
    public bool IsEmpty => Right <= Left || Bottom <= Top;

    /// <summary>Whether the edges have made their last move; latched until this reveal is discarded.</summary>
    public bool IsComplete { get; private set; }

    /// <summary>Whether this is the shortened room-to-area return: the edges move inward, and the area scene shows when it completes.</summary>
    public bool IsReturning { get; private set; }

    /// <summary>
    /// Begins the room-to-area return: the rectangle starts <see cref="FileSelectMapDefinitions.ReturnWindowInset"/> inside
    /// each of the screen's edges and its edges move back inward by the area's speeds, for
    /// <see cref="FileSelectMapDefinitions.ReturnWindowTimerReduction"/> fewer updates than the opening.
    /// </summary>
    public static FileSelectMapWindow CreateReturn(int area, WorldMapLabelPositions? labels = null)
    {
        var window = new FileSelectMapWindow(area, labels) { IsReturning = true };
        window.movesLeft = Ensure.BetweenInclusive(1, int.MaxValue, window.movesLeft - FileSelectMapDefinitions.ReturnWindowTimerReduction);
        int inset = FileSelectMapDefinitions.ReturnWindowInset;
        window.left = window.top = SignedSixteenSixteen.FromWholePixels(inset);
        window.right = SignedSixteenSixteen.FromWholePixels(ScreenGeometry.ScreenWidthPixels - inset);
        window.bottom = SignedSixteenSixteen.FromWholePixels(ScreenGeometry.ScreenHeightPixels - inset);
        return window;
    }

    /// <summary>
    /// Moves every edge by its speed: outward while opening, each stopping at the screen's edge, inward while returning.
    /// The reveal completes on its last move.
    /// </summary>
    /// <returns>Whether the reveal is complete.</returns>
    public bool Step()
    {
        if (IsComplete) return true;
        if (IsReturning)
        {
            left -= motion.Left;
            right -= motion.Right;
            top -= motion.Top;
            bottom -= motion.Bottom;
        }
        else
        {
            left = OnScreen(left + motion.Left, ScreenGeometry.ScreenWidthPixels);
            right = OnScreen(right + motion.Right, ScreenGeometry.ScreenWidthPixels);
            top = OnScreen(top + motion.Top, ScreenGeometry.ScreenHeightPixels);
            bottom = OnScreen(bottom + motion.Bottom, ScreenGeometry.ScreenHeightPixels);
        }
        IsComplete = --movesLeft == 0;
        return IsComplete;
    }

    /// <summary>
    /// <paramref name="edge"/> kept on the screen, whose side along the edge's axis is <paramref name="side"/> pixels: an
    /// edge moved past the screen's first or last pixel stops at that edge of the screen.
    /// </summary>
    private static SignedSixteenSixteen OnScreen(SignedSixteenSixteen edge, int side) =>
        SignedSixteenSixteen.FromRaw(Math.Clamp(edge.RawValue, 0, SignedSixteenSixteen.FromWholePixels(side).RawValue));
}
