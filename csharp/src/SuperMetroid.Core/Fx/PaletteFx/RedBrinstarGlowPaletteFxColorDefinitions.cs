namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable control words for Red Brinstar's background-glow palette loop.</summary>
/// <remarks>
/// Palette-FX definition <c>$F77D</c> owns fourteen timed records at $8D:EED7-$EFF6.
/// Its 112 BGR555 colors remain live presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// </remarks>
public static class RedBrinstarGlowPaletteFxColorDefinitions
{

    /// <summary>First timed record, <c>InstList_PaletteFXObject_Brinstar2_1</c>.</summary>
    public const ushort FirstFramePointer = 0xeedb;

    /// <summary>Fourteen ten-frame records form one complete glow cycle.</summary>
    public const int FrameCount = 14;

    /// <summary>Eight BGR555 colors are presentation-owned by each timed record.</summary>
    public const int ColorsPerFrame = 8;

    /// <summary>Bytes from one duration word through its terminal wait command.</summary>
    public const int FrameByteCount = 20;

    /// <summary>Returns the timed-record pointer for one zero-based glow frame.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Returns one contiguous presentation-color address within a frame.</summary>
    /// <remarks>
    /// For frame f=0..13 and color c=0..7, the live BGR555 word at
    /// $8D:EEDD + 20*f + 2*c is max(0, base[c] - $0401*min(f, 14-f)),
    /// where base[0..7] is $4C17, $280F, $2409, $1C07, $1405,
    /// $0C03, $0802, $0401. This word arithmetic exactly matches all
    /// 112 colors in the pinned NTSC J/U v1.0 ROM; records 8..13 mirror
    /// fade depths 6..1, and frame fourteen reaches the loop command.
    /// Colors remain live presentation data read by the palette-FX caller.
    /// </remarks>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
