namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Immutable mechanics for the cinematic PLANET ZEBES text fades.</summary>
/// <remarks>
/// Issue #840 / #625: the pinned NTSC J/U v1.0 ROM encodes both definitions as
/// <c>SetColorIndex($0102)</c>, eight ten-byte records of
/// <c>3, colors[3], Wait</c>, then <c>Delete</c>. Fade-in starts at
/// <c>$8D:C90E</c> (first record <c>$C912</c>, delete <c>$C962</c>);
/// fade-out starts at <c>$8D:C964</c> (first record <c>$C968</c>, delete
/// <c>$C9B8</c>). Each one-shot lasts 24 frames. All 38 control words match
/// the ROM. For frame <c>i</c> and color <c>c</c>, the ROM's fade-out color
/// equals fade-in color <c>(7 - i, c)</c> at all 24 positions. The eight
/// color rows and their reverse form48 native presentation identities. The
/// presentation view calculates rounded RGB5 interpolation from three required
/// endpoints and preserves independent supplied edits. This catalog supplies
/// control words; the text endpoints, cadence and palette selection are authored inputs.
/// ROM SHA-256:
/// <c>12B77C4BC9C1832CEE8881244659065EE1D84C70C3D29E6EAF92E6798CC2CA72</c>.
/// </remarks>
public static class PlanetZebesTextPaletteFxColorDefinitions
{
    /// <summary>$8D:C912..C960: eight brightness records per fade.</summary>
    public static int FrameCount => All[0].Rows.FrameCount;

    /// <summary>$8D:C914..C918: three text colors per record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>The fade-in and fade-out programs in native order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } =
        [
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeIn,
                Rows = PaletteFxColorRows.Of(PaletteFxProgram.FadeInPlanetZebesText, ColorsPerFrame),
            },
            new()
            {
                Owner = PlanetZebesTextPaletteFxProgramOwner.FadeOut,
                Rows = PaletteFxColorRows.Of(PaletteFxProgram.FadeOutPlanetZebesText, ColorsPerFrame),
            },
        ];

}
