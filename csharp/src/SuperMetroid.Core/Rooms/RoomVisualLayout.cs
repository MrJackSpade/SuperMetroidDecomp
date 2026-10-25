using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Replaceable initial BG1/BG2 visual references for one room level.
/// Each word contains only the native ten-bit block index and two parent flip bits;
/// collision type and BTS remain in the application's unmodified level allocation.
/// </summary>
public sealed class RoomVisualLayout
{
    /// <summary>Copied BG1 visual words independent of the room's collision allocation.</summary>
    private readonly ushort[] foreground;
    /// <summary>Copied BG2 visual words independent of subsequent scripted room edits.</summary>
    private readonly ushort[] background;

    /// <summary>Copies and validates both initial visual planes for one room level, rejecting mismatched dimensions and any collision/type bits in the artwork words.</summary>
    /// <param name="level">The level the room states share, used for artwork lookup rather than runtime cartridge access.</param>
    /// <param name="size">Positive plane size in native 16-pixel blocks: the width is the row stride and each plane must contain width times height words.</param>
    /// <param name="foregroundVisualWords">Row-major BG1 words: block selector bits 0..9 and parent horizontal/vertical flip bits 10..11 only.</param>
    /// <param name="backgroundVisualWords">Row-major BG2 words with the same permitted bit fields; collision/type and BTS data are not supplied here.</param>
    /// <exception cref="ArgumentOutOfRangeException">A dimension is nonpositive.</exception>
    /// <exception cref="InvalidDataException">A plane has the wrong word count or includes bits 12..15.</exception>
    public RoomVisualLayout(RoomLevelId level, Extent16<RoomBlocks> size,
        ReadOnlySpan<ushort> foregroundVisualWords,
        ReadOnlySpan<ushort> backgroundVisualWords)
    {
        ArgumentNullException.ThrowIfNull(level.Name, nameof(level));
        if (size.X <= 0 || size.Y <= 0)
            throw new ArgumentOutOfRangeException(nameof(size), "Room layout dimensions must be positive.");
        int expected = size.Area;
        if (foregroundVisualWords.Length != expected || backgroundVisualWords.Length != expected)
            throw new InvalidDataException(
                $"Room layout {level} needs {expected} visual words in each plane.");
        if (HasCollisionBits(foregroundVisualWords) || HasCollisionBits(backgroundVisualWords))
            throw new InvalidDataException(
                $"Room layout {level} contains a collision/type bit in visual artwork.");

        Level = level;
        Size = size;
        foreground = foregroundVisualWords.ToArray();
        background = backgroundVisualWords.ToArray();
    }

    /// <summary>The level the room states using the same initial artwork allocation share.</summary>
    public RoomLevelId Level { get; }
    /// <summary>
    /// Size of each visual plane in native 16-pixel blocks rather than 8-pixel SNES characters: the width
    /// is the row stride; the height does not resize the compiled collision allocation.
    /// </summary>
    public Extent16<RoomBlocks> Size { get; }
    /// <summary>Copied initial BG1 block selectors and parent flips in row-major order, with collision/type bits excluded; runtime streaming combines them with the unmodified native type bits.</summary>
    public ReadOnlyMemory<ushort> ForegroundVisualWords => foreground;
    /// <summary>Copied initial BG2 block selectors and parent flips in row-major order, independent of native type bits, BTS, and subsequent scripted room changes.</summary>
    public ReadOnlyMemory<ushort> BackgroundVisualWords => background;

    /// <summary>Rejects artwork words carrying any high-nibble physical block type bits.</summary>
    private static bool HasCollisionBits(ReadOnlySpan<ushort> words)
    {
        foreach (ushort word in words)
            if ((word & 0xf000) != 0) return true;
        return false;
    }
}
