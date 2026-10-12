namespace SuperMetroid.Core.Rooms;

/// <summary>
/// A room's three logical level planes in row-major block order (#633): the BG1 words with their
/// collision nibbles, one BTS byte per block, and the BG2 words.
/// </summary>
public readonly ref struct RoomLevelPlanes
{
    /// <summary>Binds the three planes; <see cref="RoomLevelData"/> validates their lengths against the room size.</summary>
    public RoomLevelPlanes(ReadOnlySpan<ushort> foreground, ReadOnlySpan<byte> behavior, ReadOnlySpan<ushort> background)
    {
        Foreground = foreground;
        Behavior = behavior;
        Background = background;
    }

    /// <summary>Complete native BG1 words, including collision nibble, block selector and parent flips.</summary>
    public ReadOnlySpan<ushort> Foreground { get; }

    /// <summary>One native BTS byte per logical foreground block.</summary>
    public ReadOnlySpan<byte> Behavior { get; }

    /// <summary>Native BG2 block words, independent of foreground collision.</summary>
    public ReadOnlySpan<ushort> Background { get; }
}

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
