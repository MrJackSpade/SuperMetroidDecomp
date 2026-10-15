namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the gunship emerging from the Zebes explosion.</summary>
/// <remarks>
/// Issue #845 / #625: the pinned NTSC J/U v1.0 ROM definition <c>$8D:E1E4</c>
/// enters at <c>$8D:D6BA</c>: <c>SetColorIndex($00A0)</c>, sixteen 36-byte
/// records of <c>24, colors[16], Wait</c>, then <c>Delete</c> at
/// <c>$D8FE</c> after 384 frames. All 35 control words match. For every
/// BGR555 component and column, frames 0..7 interpolate from frame 0 to
/// frame 7 using <c>floor(((7 - f)*q0 + f*q7)/7)</c>. Frame 8 repeats frame
/// 7 except that its last color changes from <c>$0404</c> to black. Frames
/// 8..15 interpolate from frame 8 to frame 15 with <c>t = f - 8</c> and
/// nearest rounding: <c>floor(((7 - t)*q8 + t*q15)/7 + 0.5)</c>. These two
/// rules and the boundary change match all 256 ROM colors exactly. The
/// bright endpoint colors and frame7's final color remain presentation inputs;
/// EndingGunshipPaletteColorDefinitions calculates matching white/dim and intermediate
/// colors while preserving independent edits. This catalog supplies controls to the
/// palette-FX runtime. ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class ZebesExplosionGunshipPaletteFxColorDefinitions
{

    /// <summary>The first timed record at <c>$8D:D6BE</c>.</summary>
    public const ushort FirstFramePointer = 0xd6be;

    /// <summary>The one-shot reveal contains sixteen timed records.</summary>
    public const int FrameCount = 16;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = 36;

    /// <summary>Frames0..15 occupy36-byte records atD6BE: duration,16 colors, wait.</summary>
    public static ushort FramePointer(int frame)
    {
        if ((uint)frame >= FrameCount)
            throw new ArgumentOutOfRangeException(nameof(frame));
        return unchecked((ushort)(FirstFramePointer + frame * FrameByteCount));
    }

    /// <summary>Color0..15 begins two bytes after its frame duration and advances by two.</summary>
    public static ushort ColorPointer(int frame, int color)
    {
        if ((uint)color >= ColorsPerFrame)
            throw new ArgumentOutOfRangeException(nameof(color));
        return unchecked((ushort)(FramePointer(frame) + sizeof(ushort) +
            color * sizeof(ushort)));
    }
}
