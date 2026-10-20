namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>
/// Area-to-room map window motion from <c>$81:AAAC</c> and <c>$81:AC84</c>.
/// This is a clipping-window expansion, not a scale transform of the map pixels.
/// </summary>
public sealed class FileSelectMapWindow
{
    /// <summary>Native 16.16 expansion or contraction velocity for each window edge.</summary>
    private readonly uint[] velocities = new uint[4];
    /// <summary>Current 16.16 left, right, top, and bottom clipping positions.</summary>
    internal readonly uint[] edges = new uint[4];
    /// <summary>Signed-underflow countdown controlling the final moving frame.</summary>
    private NativeCountdown timer;

    /// <summary>Creates the area-to-room expanding window at the installed label anchor, with collapsed edges and native area-specific 16.16 velocities/timer; editable anchors do not change compiled motion.</summary>
    /// <param name="area">Game-area identity 0..5 for the six Zebes areas, not the file-select display-order ordinal.</param>
    /// <param name="labels">Required installed world-map label anchors; a missing catalog is rejected even though the parameter defaults to null.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area has no supported transition.</exception>
    /// <exception cref="InvalidOperationException">World-map labels are not installed.</exception>
    public FileSelectMapWindow(int area, SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelLayout? labels = null)
        : this(area, FileSelectMapWindowMotions.Get(area), labels) { }

    /// <summary>Explicit motion injection for bounded arithmetic/render fixtures; production uses the compiled catalog.</summary>
    internal FileSelectMapWindow(int area, FileSelectMapWindowMotion motion,
        SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelLayout? labels = null)
    {
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        var installedLabels = labels ?? throw new InvalidOperationException(
            "Map-window positions require installed world-map labels.");
        ushort x = installedLabels.Get(area).X;
        ushort y = installedLabels.Get(area).Y;
        edges[0] = edges[1] = SnesSixteenSixteen.FromWhole((uint)x);
        edges[2] = edges[3] = SnesSixteenSixteen.FromWhole((uint)y);
        timer = NativeCountdown.Start(motion.Timer);
        if (timer.IsNegative)
            throw new InvalidDataException("Map-window timer must begin before signed underflow.");
        velocities[0] = motion.Left;
        velocities[1] = motion.Right;
        velocities[2] = motion.Top;
        velocities[3] = motion.Bottom;
    }

    /// <summary>
    /// Whole-pixel upper-left corner of the clipping window. X is the inclusive left endpoint (its low eight bits are
    /// the displayed column) and Y the first covered scanline; expansion clamps both at one.
    /// </summary>
    public Position16<ScreenPixels> TopLeft =>
        new(SnesFixedPosition.WholeOf(unchecked((int)edges[0])), SnesFixedPosition.WholeOf(unchecked((int)edges[2])));
    /// <summary>
    /// Whole-pixel lower-right corner of the clipping window. X is the inclusive right endpoint, clamped at 255 during
    /// expansion; Y bounds the covered scanlines measured from the top rather than naming an inclusive last row, and is
    /// clamped at 224 during expansion.
    /// </summary>
    public Position16<ScreenPixels> BottomRight =>
        new(SnesFixedPosition.WholeOf(unchecked((int)edges[1])), SnesFixedPosition.WholeOf(unchecked((int)edges[3])));
    /// <summary>Whether the transition timer has reached signed underflow after the final edge movement; initially false and latched true until this window is discarded.</summary>
    public bool IsComplete { get; private set; }
    /// <summary>Whether this is $81:AFF6's shortened room-to-area contraction: subtract velocities without whole-word clamps, and reveal the area scene when complete.</summary>
    public bool IsReturning { get; private set; }

    /// <summary>$81:AFF6 begins a shortened, unclamped contraction from the inset room frame.</summary>
    public static FileSelectMapWindow CreateReturn(int area, SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelLayout? labels = null)
    {
        var window = new FileSelectMapWindow(area, labels);
        window.IsReturning = true;
        window.timer = NativeCountdown.FromRaw(unchecked((ushort)(window.timer.RawValue - FileSelectMapRomData.ReturnWindowTimerReduction)));
        int inset = FileSelectMapRomData.ReturnWindowInset;
        window.edges[0] = window.edges[2] = SnesSixteenSixteen.FromWhole((uint)inset);
        window.edges[1] = SnesSixteenSixteen.FromWhole((uint)(FrontendFrame.Width - inset));
        window.edges[3] = SnesSixteenSixteen.FromWhole((uint)(FrontendFrame.Height - inset));
        return window;
    }

    /// <summary>
    /// Advances all four 16.16 positions, clamps only their whole words, and then
    /// decrements the timer. The zero-timer frame still moves; completion is at -1.
    /// </summary>
    public bool Step()
    {
        if (IsComplete) return true;
        for (int edge = 0; edge < edges.Length; edge++)
            edges[edge] = IsReturning
                ? unchecked(edges[edge] - velocities[edge])
                : unchecked(edges[edge] + velocities[edge]);
        if (!IsReturning)
        {
            ClampWholeWord(0, 1, lowerBound: true);
            ClampWholeWord(1, 255, lowerBound: false);
            ClampWholeWord(2, 1, lowerBound: true);
            ClampWholeWord(3, 224, lowerBound: false);
        }
        timer = timer.Decrement().Remaining;
        IsComplete = timer.IsNegative;
        return IsComplete;
    }

    /// <summary>Clamps one edge's whole-pixel word while retaining its fractional position.</summary>
    private void ClampWholeWord(int edge, ushort limit, bool lowerBound)
    {
        short position = unchecked((short)SnesSixteenSixteen.WholeOf(edges[edge]));
        if (lowerBound ? position < limit : position > limit)
            edges[edge] = SnesSixteenSixteen.Compose(limit, (ushort)SnesSixteenSixteen.FractionOf(edges[edge]));
    }
}
