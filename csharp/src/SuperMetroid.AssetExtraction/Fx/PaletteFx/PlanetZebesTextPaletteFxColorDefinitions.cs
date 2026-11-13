using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.AssetExtraction.Fx.PaletteFx;

/// <summary>Immutable mechanics for the cinematic PLANET ZEBES text fades.</summary>
/// <remarks>
/// Issue #840 / #625: both fades show eight records of the text's three colours, each held
/// for three frames, and then end. Fade-in starts at the fade-in entry;
/// fade-out starts at the fade-out entry.
/// Each one-shot lasts 24 frames. For frame <c>i</c> and color <c>c</c>, the fade-out color
/// equals fade-in color <c>(7 - i, c)</c> at all 24 positions. The eight
/// colour frames and their reverse form 48 presentation identities, each kept as
/// installed (#1615). This catalog supplies the timing; the text colours, cadence
/// and palette selection are authored inputs.
/// </remarks>
public static class PlanetZebesTextPaletteFxColorDefinitions
{

    /// <summary>Three text colors per record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The fade-in and fade-out programs in definition order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } =
        [
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeIn,
                ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.FadeInPlanetZebesText, ColorsPerFrame),
            },
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeOut,
                ColorFrames = PaletteFxColorFrames.Of(PaletteFxSegment.FadeOutPlanetZebesText, ColorsPerFrame),
            },
        ];

}
