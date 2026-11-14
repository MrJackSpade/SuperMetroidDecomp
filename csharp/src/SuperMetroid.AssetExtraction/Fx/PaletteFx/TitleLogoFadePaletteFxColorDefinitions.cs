using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for fading in the Super Metroid title logo.</summary>
/// <remarks>
/// Issue #852 / #625: the logo fade's definition
/// runs <c>SetColorIndex(322)</c>, eight 34-byte
/// records of <c>3, colors[15], Wait</c>, then <c>Delete</c> at
/// its end after 24 frames. All 19 control words match. Let
/// <c>final[c]</c> be color column <c>c</c> (0..14) of the eighth record.
/// For record <c>f</c> (0..7), each five-bit BGR555 channel <c>q</c> is
/// <c>floor(q(final[c]) * f / 7)</c>. This exact channel rule matches all
/// 120 original colors; frame 0 is black and frame 7 is the authored final
/// palette. The presentation compiler still supplies all live colors while
/// this catalog supplies only controls to the palette-FX runtime.
/// </remarks>
public static class TitleLogoFadePaletteFxColorDefinitions
{

    /// <summary>Each record writes fifteen live BGR555 colors.</summary>
    public const int ColorsPerFrame = 15;

    /// <summary>The family's records, consecutive from the first its program shows, and the colours each writes.</summary>
    public static PaletteFxColorFrames ColorFrames { get; } = PaletteFxColorFrames.Of(ColourEffectAnimation.FadeInSuperMetroidTitleLogo, ColorsPerFrame);
}
