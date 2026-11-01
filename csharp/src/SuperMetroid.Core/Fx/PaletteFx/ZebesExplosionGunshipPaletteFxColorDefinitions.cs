namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the gunship emerging from the Zebes explosion.</summary>
/// <remarks>
/// Issue #845 / #625: the gunship definition
/// runs <c>SetColorIndex(160)</c>, sixteen 36-byte
/// records of <c>24, colors[16], Wait</c>, then <c>Delete</c> at
/// its end after 384 frames. All 35 control words match. For every
/// BGR555 component and column, frames 0..7 interpolate from frame 0 to
/// frame 7 using <c>floor(((7 - f)*q0 + f*q7)/7)</c>. Frame 8 repeats frame
/// 7 except that its last color changes from a dim grey to black. Frames
/// 8..15 interpolate from frame 8 to frame 15 with <c>t = f - 8</c> and
/// nearest rounding: <c>floor(((7 - t)*q8 + t*q15)/7 + 0.5)</c>. These two
/// rules and the boundary change match all 256 original colors exactly. The
/// bright endpoint colors and frame7's final color remain presentation inputs;
/// EndingGunshipPaletteColors calculates matching white/dim and intermediate
/// colors while preserving independent edits. This catalog supplies controls to the
/// palette-FX runtime.
/// </remarks>
public static class ZebesExplosionGunshipPaletteFxColorDefinitions
{

    /// <summary>The one-shot reveal contains sixteen timed records.</summary>
    public static int FrameCount => ColorFrames.FrameCount;

    /// <summary>Each record writes sixteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxProgram.GunshipEmergingFromZebesExplosion, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => ColorFrames.Color(frame, color);
}
