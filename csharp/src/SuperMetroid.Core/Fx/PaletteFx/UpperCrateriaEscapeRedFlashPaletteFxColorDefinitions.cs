namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for upper Crateria's escape red-flash loop.</summary>
/// <remarks>
/// Definition <c>$FFE5</c> enters one fourteen-record program. Its 98 BGR555 words
/// remain live presentation data; this catalog owns palette placement, timing, waits,
/// and loop control.
/// </remarks>
public static class UpperCrateriaEscapeRedFlashPaletteFxColorDefinitions
{
    /// <summary>The first timed record at <c>$8D:FD01</c>.</summary>
    public const ushort FirstFramePointer = 0xfd01;
    /// <summary>The loop contains fourteen timed records.</summary>
    public const int FrameCount = 14;
    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;
    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 18;

    /// <summary>Returns one timed-record pointer.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one live BGR555 color word in a timed record.</summary>
    /// <remarks>
    /// For frame 0..13 and color 0..6, the word is at
    /// $8D:FD03 + 18 * frame + 2 * color. Eight distinct authored rows
    /// appear: frames 0..7 use rows 0..7, and frames 8..13 mirror rows
    /// 6..1, so the source row is frame when frame is at most seven and
    /// 14 - frame afterward. All 98 words match the pinned NTSC J/U v1.0
    /// ROM and bank-$8D annotation. Independent seven-color BGR555 values
    /// are still supplied presentation payloads; their derivation remains
    /// pending under issue #1165.
    /// </remarks>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
