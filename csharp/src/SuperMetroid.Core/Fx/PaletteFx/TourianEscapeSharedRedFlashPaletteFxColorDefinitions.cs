namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for Tourian's shared general-level red-flash loop.</summary>
/// <remarks>
/// Definitions <c>$FFD1</c> and <c>$FFD5</c> select different CGRAM destinations before
/// converging at <c>$F94D</c>. The 98 BGR555 words remain live presentation data; this
/// catalog owns entry routing, timing, the inline CGRAM skip, waits, and loop control.
/// </remarks>
public static class TourianEscapeSharedRedFlashPaletteFxColorDefinitions
{
    /// <summary><c>PalFxDef_Tourian20</c> at <c>$8D:FFD1</c>.</summary>
    public const ushort GeneralLevelDefinitionPointer = 0xffd1;

    /// <summary><c>PalFxDef_Tourian40</c> at <c>$8D:FFD5</c>.</summary>
    public const ushort ArkanoidDefinitionPointer = 0xffd5;

    /// <summary>The shared timed-record loop at <c>$8D:F94D</c>.</summary>
    public const ushort FirstFramePointer = 0xf94d;

    /// <summary>The shared loop contains fourteen records.</summary>
    public const int FrameCount = 14;

    /// <summary>Each record writes seven live BGR555 colors.</summary>
    public const int ColorsPerFrame = 7;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>Calculates a timed-record address from the first frame and its 20-byte stride, rejecting out-of-range frames.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Locates a live color word, accounting for the inline CGRAM-index instruction before the seventh color.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        int offset = color < 6 ? sizeof(ushort) + color * sizeof(ushort) : 16;
        return unchecked((ushort)(FramePointer(frame) + offset));
    }
}
