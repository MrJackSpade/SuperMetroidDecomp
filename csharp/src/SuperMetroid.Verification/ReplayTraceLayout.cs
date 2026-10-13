using System.Globalization;
using System.Security.Cryptography;
using System.Text;

/// <summary>One contiguous span of native work RAM, as a byte offset into the 128 KiB <c>$7E:0000-$7F:FFFF</c> image.</summary>
/// <param name="Start">Offset of the first byte.</param>
/// <param name="Length">Number of bytes; positive.</param>
internal readonly record struct WorkRamRange(int Start, int Length)
{
    /// <summary>The offset just past the range.</summary>
    public int End => Start + Length;

    /// <summary>The range holding <paramref name="count"/> consecutive little-endian words from <paramref name="start"/>.</summary>
    /// <param name="start">Offset of the first word.</param>
    /// <param name="count">Number of words.</param>
    public static WorkRamRange Words(int start, int count = 1) => new(start, count * sizeof(ushort));

    /// <summary>The range from <paramref name="start"/> up to, but excluding, <paramref name="end"/>.</summary>
    /// <param name="start">Offset of the first byte.</param>
    /// <param name="end">Offset just past the last byte.</param>
    public static WorkRamRange Between(int start, int end) => new(start, end - start);
}

/// <summary>
/// The fixed set of WRAM ranges one replay verifier reads from each native checkpoint. A compact
/// checkpoint payload holds exactly these bytes, ranges in ascending address order, so a payload
/// offset follows from the layout alone. The layout's identity (<see cref="Sha256"/>) is recorded
/// in every compact trace; changing the ranges requires recompacting the trace.
/// </summary>
internal sealed class ReplayTraceLayout
{
    private readonly WorkRamRange[] ranges;
    private readonly int[] payloadOffsets;

    /// <summary>Normalizes <paramref name="ranges"/> (sorted, overlapping or adjacent ranges merged).</summary>
    /// <param name="name">Stable layout name recorded in compact manifests.</param>
    /// <param name="ranges">The WRAM ranges the verifier reads; any order, overlaps allowed.</param>
    public ReplayTraceLayout(string name, IEnumerable<WorkRamRange> ranges)
    {
        Name = name;
        var merged = new List<WorkRamRange>();
        foreach (WorkRamRange range in ranges.OrderBy(range => range.Start))
        {
            if (range.Length <= 0 || range.Start < 0 || range.End > ReplayTraceMemoryCatalog.WorkRamByteCount)
                throw new ArgumentOutOfRangeException(nameof(ranges), $"WRAM range {range} is outside the 128 KiB image.");
            if (merged.Count != 0 && range.Start <= merged[^1].End)
                merged[^1] = WorkRamRange.Between(merged[^1].Start, Math.Max(merged[^1].End, range.End));
            else
                merged.Add(range);
        }
        this.ranges = merged.ToArray();
        payloadOffsets = new int[this.ranges.Length];
        int offset = 0;
        for (int index = 0; index < this.ranges.Length; index++)
        {
            payloadOffsets[index] = offset;
            offset += this.ranges[index].Length;
        }
        PayloadByteCount = offset;
        var description = new StringBuilder(name).Append('\n');
        foreach (WorkRamRange range in this.ranges)
            description.Append(CultureInfo.InvariantCulture, $"{range.Start}:{range.Length}\n");
        Sha256 = Convert.ToHexString(SHA256.HashData(Encoding.UTF8.GetBytes(description.ToString())));
    }

    /// <summary>The stable layout name recorded in compact manifests.</summary>
    public string Name { get; }

    /// <summary>The normalized ranges, ascending and disjoint, in payload order.</summary>
    public IReadOnlyList<WorkRamRange> Ranges => ranges;

    /// <summary>Bytes of WRAM in one compact checkpoint payload.</summary>
    public int PayloadByteCount { get; }

    /// <summary>SHA-256 of the layout name and normalized ranges; identifies the payload layout.</summary>
    public string Sha256 { get; }

    /// <summary>Copies the layout's bytes of a full WRAM image into <paramref name="payload"/>.</summary>
    /// <param name="workRam">The complete 128 KiB native WRAM image.</param>
    /// <param name="payload">Destination of <see cref="PayloadByteCount"/> bytes.</param>
    public void Pack(ReadOnlySpan<byte> workRam, Span<byte> payload)
    {
        if (workRam.Length != ReplayTraceMemoryCatalog.WorkRamByteCount || payload.Length != PayloadByteCount)
            throw new ArgumentException("Packing requires a complete WRAM image and a payload of the layout's size.");
        for (int index = 0; index < ranges.Length; index++)
            workRam.Slice(ranges[index].Start, ranges[index].Length).CopyTo(payload[payloadOffsets[index]..]);
    }

    /// <summary>
    /// Finds the payload offset of <paramref name="length"/> WRAM bytes at <paramref name="address"/>;
    /// false when any of them lies outside the layout.
    /// </summary>
    /// <param name="address">WRAM offset of the first byte.</param>
    /// <param name="length">Number of bytes read.</param>
    /// <param name="payloadOffset">The payload offset of <paramref name="address"/> when found.</param>
    public bool TryLocate(int address, int length, out int payloadOffset)
    {
        int low = 0, high = ranges.Length - 1;
        while (low <= high)
        {
            int middle = (low + high) / 2;
            WorkRamRange range = ranges[middle];
            if (address < range.Start)
                high = middle - 1;
            else if (address >= range.End)
                low = middle + 1;
            else
            {
                payloadOffset = payloadOffsets[middle] + (address - range.Start);
                return address + length <= range.End;
            }
        }
        payloadOffset = 0;
        return false;
    }
}
