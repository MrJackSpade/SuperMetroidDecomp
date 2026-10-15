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
    public const int FrameCount = 8;

    /// <summary>$8D:C914..C918: three text colors per record.</summary>
    public const int ColorsPerFrame = 3;

    /// <summary>Bytes from one duration through its terminal wait command.</summary>
    public const int FrameByteCount = sizeof(ushort) * (ColorsPerFrame + 2);
    /// <summary>$8D:C90E: native fade-in setup; fade-out follows its terminal delete.</summary>
    internal const ushort FadeInProgram = 0xC90E;

    /// <summary>The fade-in and fade-out programs in native order without cached definition records.</summary>
    public static IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition> All { get; } = new DefinitionSequence();

    /// <summary>Indexable fade-in and fade-out programs without cached definition records.</summary>
    private sealed class DefinitionSequence : IReadOnlyList<PlanetZebesTextPaletteFxColorDefinition>
    {
        /// <summary>One program for each direction of the PLANET ZEBES text fade.</summary>
        public int Count => 2;
        /// <summary>Constructs the fade program at its native definition ordinal.</summary>
        public PlanetZebesTextPaletteFxColorDefinition this[int index] => index switch
        {
            0 => new(PlanetZebesTextPaletteFxProgramOwner.FadeIn),
            1 => new(PlanetZebesTextPaletteFxProgramOwner.FadeOut),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Yields fade-in before fade-out.</summary>
        public IEnumerator<PlanetZebesTextPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Uses the typed fade sequence for nongeneric callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
