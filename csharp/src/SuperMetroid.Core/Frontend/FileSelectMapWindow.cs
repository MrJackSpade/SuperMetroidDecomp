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

    public FileSelectMapWindow(ISnesAddressSpace bus, int area)
    {
        ArgumentNullException.ThrowIfNull(bus);
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        ushort x = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelPositions + area * 4);
        ushort y = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.LabelPositions + area * 4 + 2);
        edges[0] = edges[1] = (uint)x << 16;
        edges[2] = edges[3] = (uint)y << 16;
        timer = RomDataReader.ReadWordFixedBank(bus, FileSelectMapRomData.WindowTimers + area * 2);
        if ((short)timer < 0)
            throw new InvalidDataException("Map-window timer must begin before signed underflow.");
        for (int edge = 0; edge < edges.Length; edge++)
        {
            int address = FileSelectMapRomData.WindowVelocities + area * FileSelectMapRomData.VelocityRecordBytes + edge * 4;
            velocities[edge] = RomDataReader.ReadWordFixedBank(bus, address) |
                ((uint)RomDataReader.ReadWordFixedBank(bus, address + 2) << 16);
        }
    }

    public ushort Left => (ushort)(edges[0] >> 16);
    public ushort Right => (ushort)(edges[1] >> 16);
    public ushort Top => (ushort)(edges[2] >> 16);
    public ushort Bottom => (ushort)(edges[3] >> 16);
    public bool IsComplete { get; private set; }

    /// <summary>
    /// Advances all four 16.16 positions, clamps only their whole words, and then
    /// decrements the timer. The zero-timer frame still moves; completion is at -1.
    /// </summary>
    public bool Step()
    {
        if (IsComplete) return true;
        for (int edge = 0; edge < edges.Length; edge++)
            edges[edge] = unchecked(edges[edge] + velocities[edge]);
        ClampWholeWord(0, 1, lowerBound: true);
        ClampWholeWord(1, 255, lowerBound: false);
        ClampWholeWord(2, 1, lowerBound: true);
        ClampWholeWord(3, 224, lowerBound: false);
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
