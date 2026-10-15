namespace SuperMetroid.Core.Rooms.LevelData;

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
