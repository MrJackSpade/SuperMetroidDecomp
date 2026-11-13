using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>
/// Immutable mechanics for the shared wide-background and space-whiteout palette ramp.
/// </summary>
/// <remarks>
/// Issue #848 / #625: the wide-background definition
/// sets color index 34 and branches
/// to the shared ramp. The space-whiteout definition
/// sets index 0 and falls through to that same ramp. It has
/// fifteen six-byte records of <c>14, color, Wait</c>, then <c>Delete</c>
/// after 210 frames; all 37 control words match. For frame
/// <c>f</c> (0..14), grayscale channel <c>g</c> is
/// <c>floor((31*f + 6)/14)</c>, except frame 12 uses <c>g = 26</c> rather
/// than 27. The BGR555 color is <c>g</c> in each of red, green and blue. This rule matches all
/// fifteen original colors exactly, including the authored one-unit step. The
/// presentation compiler still supplies those live colors while this catalog
/// supplies only controls to the palette-FX runtime.
/// </remarks>
public static class ZebesExplosionWhiteoutPaletteFxColorDefinitions
{

    /// <summary>Each record writes one live BGR555 color.</summary>
    public const int ColorsPerFrame = 1;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxSegment.CommonZebesExplosion, ColorsPerFrame);
}
