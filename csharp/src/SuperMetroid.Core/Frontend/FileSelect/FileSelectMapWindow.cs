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
    private WrappingCountdown timer;

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
        if ((uint)area >= FileSelectMapDefinitions.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        var installedLabels = labels ?? throw new InvalidOperationException(
            "Map-window positions require installed world-map labels.");
        ushort x = installedLabels.Get(area).X;
        ushort y = installedLabels.Get(area).Y;
        edges[0] = edges[1] = SnesSixteenSixteen.FromWhole((uint)x);
        edges[2] = edges[3] = SnesSixteenSixteen.FromWhole((uint)y);
        timer = WrappingCountdown.Start(motion.Timer);
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
        window.timer = WrappingCountdown.FromRaw(unchecked((ushort)(window.timer.RawValue - FileSelectMapDefinitions.ReturnWindowTimerReduction)));
        int inset = FileSelectMapDefinitions.ReturnWindowInset;
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
            RaiseWholeWord(0, 1);
            LowerWholeWord(1, 255);
            RaiseWholeWord(2, 1);
            LowerWholeWord(3, 224);
        }
        timer = timer.Decrement().Remaining;
        IsComplete = timer.IsNegative;
        return IsComplete;
    }

    /// <summary>Raises one edge's signed whole-pixel word to <paramref name="minimum"/>, retaining its fractional position.</summary>
    private void RaiseWholeWord(int edge, ushort minimum)
    {
        if (WholeWord(edge) < minimum)
            SetWholeWord(edge, minimum);
    }

    /// <summary>Lowers one edge's signed whole-pixel word to <paramref name="maximum"/>, retaining its fractional position.</summary>
    private void LowerWholeWord(int edge, ushort maximum)
    {
        if (WholeWord(edge) > maximum)
            SetWholeWord(edge, maximum);
    }

    /// <summary>One edge's whole-pixel word, read as signed.</summary>
    private short WholeWord(int edge) => unchecked((short)SnesSixteenSixteen.WholeOf(edges[edge]));

    /// <summary>Replaces one edge's whole-pixel word, keeping its fraction.</summary>
    private void SetWholeWord(int edge, ushort whole) =>
        edges[edge] = SnesSixteenSixteen.Compose(whole, (ushort)SnesSixteenSixteen.FractionOf(edges[edge]));
}
