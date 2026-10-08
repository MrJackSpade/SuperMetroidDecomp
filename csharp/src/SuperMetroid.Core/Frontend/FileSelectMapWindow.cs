using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Area-to-room map window motion from <c>$81:AAAC</c> and <c>$81:AC84</c>.
/// This is a clipping-window expansion, not a scale transform of the map pixels.
/// </summary>
public sealed class FileSelectMapWindow
{
    private readonly uint[] velocities = new uint[4];
    private readonly uint[] edges = new uint[4];
    private ushort timer;

    /// <summary>Creates the area-to-room expanding window at the installed label anchor, with collapsed edges and native area-specific 16.16 velocities/timer; editable anchors do not change compiled motion.</summary>
    /// <param name="bus">Non-null runtime address-space dependency; this setup uses compiled motion rather than reading cartridge tables.</param>
    /// <param name="area">Game-area identity 0..5 for the six Zebes areas, not the file-select display-order ordinal.</param>
    /// <param name="labels">Required installed world-map label anchors; a missing catalog is rejected even though the parameter defaults to null.</param>
    /// <exception cref="ArgumentOutOfRangeException">The area has no supported transition.</exception>
    /// <exception cref="InvalidOperationException">World-map labels are not installed.</exception>
    public FileSelectMapWindow(ISnesAddressSpace bus, int area, SuperMetroid.Core.Assets.WorldMapLabelLayout? labels = null)
        : this(bus, area, FileSelectMapWindowMotions.Get(area), labels) { }

    /// <summary>Explicit motion injection for bounded arithmetic/render fixtures; production uses the compiled catalog.</summary>
    internal FileSelectMapWindow(ISnesAddressSpace bus, int area, FileSelectMapWindowMotion motion,
        SuperMetroid.Core.Assets.WorldMapLabelLayout? labels = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        var installedLabels = labels ?? throw new InvalidOperationException(
            "Map-window positions require installed world-map labels.");
        ushort x = (ushort)installedLabels.Get(area).X;
        ushort y = (ushort)installedLabels.Get(area).Y;
        edges[0] = edges[1] = (uint)x << 16;
        edges[2] = edges[3] = (uint)y << 16;
        timer = motion.Timer;
        if ((short)timer < 0)
            throw new InvalidDataException("Map-window timer must begin before signed underflow.");
        velocities[0] = motion.Left;
        velocities[1] = motion.Right;
        velocities[2] = motion.Top;
        velocities[3] = motion.Bottom;
    }

    /// <summary>Raw whole-pixel word of the left 16.16 edge; its low byte is the inclusive horizontal window endpoint, and expansion clamps the whole word at one.</summary>
    public ushort Left => (ushort)(edges[0] >> 16);
    /// <summary>Raw whole-pixel word of the right 16.16 edge; its low byte is the inclusive horizontal endpoint, and expansion clamps the whole word at 255.</summary>
    public ushort Right => (ushort)(edges[1] >> 16);
    /// <summary>Raw whole-pixel word of the top 16.16 edge, used as the initial HDMA scanline count; expansion clamps the whole word at one.</summary>
    public ushort Top => (ushort)(edges[2] >> 16);
    /// <summary>Raw whole-pixel word of the bottom 16.16 edge, defining the window's scanline extent from top rather than an inclusive Y endpoint; expansion clamps the whole word at 224.</summary>
    public ushort Bottom => (ushort)(edges[3] >> 16);
    /// <summary>Whether the transition timer has reached signed underflow after the final edge movement; initially false and latched true until this window is discarded.</summary>
    public bool IsComplete { get; private set; }
    /// <summary>Whether this is $81:AFF6's shortened room-to-area contraction: subtract velocities without whole-word clamps, and reveal the area scene when complete.</summary>
    public bool IsReturning { get; private set; }

    /// <summary>$81:AFF6 begins a shortened, unclamped contraction from the inset room frame.</summary>
    public static FileSelectMapWindow CreateReturn(ISnesAddressSpace bus, int area, SuperMetroid.Core.Assets.WorldMapLabelLayout? labels = null)
    {
        var window = new FileSelectMapWindow(bus, area, labels);
        window.IsReturning = true;
        window.timer = unchecked((ushort)(window.timer - FileSelectMapRomData.ReturnWindowTimerReduction));
        int inset = FileSelectMapRomData.ReturnWindowInset;
        window.edges[0] = window.edges[2] = (uint)inset << 16;
        window.edges[1] = (uint)(FrontendFrame.Width - inset) << 16;
        window.edges[3] = (uint)(FrontendFrame.Height - inset) << 16;
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
        timer = unchecked((ushort)(timer - 1));
        IsComplete = (short)timer < 0;
        return IsComplete;
    }

    private void ClampWholeWord(int edge, ushort limit, bool lowerBound)
    {
        short position = unchecked((short)(edges[edge] >> 16));
        if (lowerBound ? position < limit : position > limit)
            edges[edge] = (edges[edge] & ushort.MaxValue) | ((uint)limit << 16);
    }
}
