namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Replaceable initial BG1/BG2 visual references for one decompressed room source.
/// Each word contains only the native ten-bit block index and two parent flip bits;
/// collision type and BTS remain in the application's unmodified level allocation.
/// </summary>
public sealed class RoomVisualLayout
{
    private readonly ushort[] foreground;
    private readonly ushort[] background;

    public RoomVisualLayout(int sourceAddress, int widthInBlocks, int heightInBlocks,
        ReadOnlySpan<ushort> foregroundVisualWords,
        ReadOnlySpan<ushort> backgroundVisualWords)
    {
        if ((uint)sourceAddress > 0xffffff)
            throw new ArgumentOutOfRangeException(nameof(sourceAddress));
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(widthInBlocks);
        ArgumentOutOfRangeException.ThrowIfNegativeOrZero(heightInBlocks);
        int expected = checked(widthInBlocks * heightInBlocks);
        if (foregroundVisualWords.Length != expected || backgroundVisualWords.Length != expected)
            throw new InvalidDataException(
                $"Room layout ${sourceAddress:X6} needs {expected} visual words in each plane.");
        if (HasCollisionBits(foregroundVisualWords) || HasCollisionBits(backgroundVisualWords))
            throw new InvalidDataException(
                $"Room layout ${sourceAddress:X6} contains a collision/type bit in visual artwork.");

        SourceAddress = sourceAddress;
        WidthInBlocks = widthInBlocks;
        HeightInBlocks = heightInBlocks;
        foreground = foregroundVisualWords.ToArray();
        background = backgroundVisualWords.ToArray();
    }

    public int SourceAddress { get; }
    public int WidthInBlocks { get; }
    public int HeightInBlocks { get; }
    public ReadOnlyMemory<ushort> ForegroundVisualWords => foreground;
    public ReadOnlyMemory<ushort> BackgroundVisualWords => background;

    private static bool HasCollisionBits(ReadOnlySpan<ushort> words)
    {
        foreach (ushort word in words)
            if ((word & 0xf000) != 0) return true;
        return false;
    }
}

/// <summary>Complete installed room-layout artwork selected by the room state's source.</summary>
public sealed class RoomVisualLayoutCatalog
{
    private readonly IReadOnlyDictionary<int, RoomVisualLayout> layouts;

    public RoomVisualLayoutCatalog(IReadOnlyDictionary<int, RoomVisualLayout> layouts)
    {
        ArgumentNullException.ThrowIfNull(layouts);
        this.layouts = new Dictionary<int, RoomVisualLayout>(layouts);
    }

    public RoomVisualLayout Get(int sourceAddress) =>
        layouts.TryGetValue(sourceAddress, out RoomVisualLayout? layout)
            ? layout
            : throw new InvalidDataException(
                $"Installed room visual layouts lack source ${sourceAddress:X6}.");
}
