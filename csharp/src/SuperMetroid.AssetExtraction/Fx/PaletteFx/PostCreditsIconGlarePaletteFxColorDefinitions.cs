using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the post-credits Super Metroid icon glare.</summary>
/// <remarks>
/// Issue #851 / #625: the glare shows fourteen records of sixteen colours, each for one
/// frame, and ends after 14 frames. Let
/// <c>base[c]</c> be the final record's sixteen authored colours.
/// For record <c>f</c> (0..13), set <c>t = f + 1</c> for 0..6 and
/// <c>t = 13 - f</c> for 7..13. For each color column <c>c</c> and
/// level <c>q</c> of <c>base[c]</c>, the output channel is
/// <c>floor(((7 - t)*q + 31*t)/7)</c>. Frame 6 is white, frames 7..12
/// mirror frames 5..0, and frame 13 restores the base. This rule matches
/// all 224 original colors exactly. LogoGlarePaletteColors evaluates matching
/// presentation colors from the final supplied frame; independent edits remain explicit.
/// The presentation compiler owns the base colors and overrides.
/// </remarks>
public static class PostCreditsIconGlarePaletteFxColorDefinitions
{

    /// <summary>Each record writes sixteen live colours.</summary>
    public const int ColorsPerFrame = 16;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(PaletteFxProgram.PostCreditsSuperMetroidIconGlare, ColorsPerFrame);

    /// <summary>Colour <paramref name="color"/> of record <paramref name="frame"/>.</summary>
    public static PaletteFxColorKey Color(int frame, int color) => ColorFrames.Color(frame, color);
}
