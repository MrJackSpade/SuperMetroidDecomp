using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

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

    public FileSelectMapWindow(ISnesAddressSpace bus, int area, SuperMetroid.Core.Assets.WorldMapLabelLayout? labels = null)
        : this(bus, area, FileSelectMapWindowMotions.Get(area), labels) { }

    /// <summary>Explicit motion injection for bounded arithmetic/render fixtures; production uses the compiled catalog.</summary>
    internal FileSelectMapWindow(ISnesAddressSpace bus, int area, FileSelectMapWindowMotion motion,
        SuperMetroid.Core.Assets.WorldMapLabelLayout? labels = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ushort x = labels is null ? RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelPositions + area * 4) : (ushort)labels.Get(area).X;
        ushort y = labels is null ? RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelPositions + area * 4 + 2) : (ushort)labels.Get(area).Y;
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

    public ushort Left => (ushort)(edges[0] >> 16);
    public ushort Right => (ushort)(edges[1] >> 16);
    public ushort Top => (ushort)(edges[2] >> 16);
    public ushort Bottom => (ushort)(edges[3] >> 16);
    public bool IsComplete { get; private set; }
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
