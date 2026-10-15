namespace SuperMetroid.Core.Rooms.LevelData;

/// <summary>
/// A room's copied native streaming allocations (#633): the decompressed BG1 and BG2 buffers
/// including the overread tails the initial column fill reaches. An empty allocation means the
/// logical plane alone.
/// </summary>
public readonly ref struct RoomLevelStreaming
{
    /// <summary>Binds the native allocations; either may be empty.</summary>
    public RoomLevelStreaming(ReadOnlySpan<ushort> foreground = default, ReadOnlySpan<ushort> background = default)
    {
        Foreground = foreground;
        Background = background;
    }

    /// <summary>The native BG1 allocation, or empty for the logical foreground plane.</summary>
    public ReadOnlySpan<ushort> Foreground { get; }

    /// <summary>The native BG2 allocation, or empty for the logical background plane.</summary>
    public ReadOnlySpan<ushort> Background { get; }
}
