namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the cinematic PLANET ZEBES text fades.</summary>
/// <remarks>
/// Issue #840 / #625: both definitions are
/// <c>SetColorIndex(258)</c>, eight ten-byte records of
/// <c>3, colors[3], Wait</c>, then <c>Delete</c>. Fade-in starts at
/// the fade-in entry;
/// fade-out starts at the fade-out entry.
/// Each one-shot lasts 24 frames. All 38 control words match
/// the original programs. For frame <c>i</c> and color <c>c</c>, the fade-out color
/// equals fade-in color <c>(7 - i, c)</c> at all 24 positions. The eight
/// colour frames and their reverse form 48 presentation identities. The
/// presentation view calculates rounded level interpolation from three required
/// endpoints and preserves independent supplied edits. This catalog supplies
/// control words; the text endpoints, cadence and palette selection are authored inputs.
/// </remarks>
public static class PlanetZebesTextPaletteFxColorDefinitions
{
    /// <summary>Eight brightness records per fade.</summary>
    public static int FrameCount => All[0].ColorFrames.FrameCount;

    /// <summary>Three text colors per record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The fade-in and fade-out programs in definition order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } =
        [
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeIn,
                ColorFrames = PaletteFxColorFrames.Of(PaletteFxProgram.FadeInPlanetZebesText, ColorsPerFrame),
            },
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeOut,
                ColorFrames = PaletteFxColorFrames.Of(PaletteFxProgram.FadeOutPlanetZebesText, ColorsPerFrame),
            },
        ];

}
