namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>
/// Immutable control words for Maridia's sand and background-waterfall palette loops.
/// </summary>
/// <remarks>
/// Definitions <c>$F795</c>, <c>$F799</c>, and <c>$F79D</c> own 112 BGR555 color
/// words. Those colors remain presentation data; this catalog owns only color-index
/// setup, durations, waits, and loop control.
/// Sand pits set CGRAM byte $0048 and enter four ten-frame records at
/// $F4ED + 20*i, i=0..3, then $C61E goto at $F53D: a 40-frame loop.
/// Sand falls set $0050 and enter four ten-frame records at
/// $F545 + 12*i, then goto $F575: another 40-frame loop.
/// Background waterfalls set $0068 and enter eight two-frame records
/// at $F57D + 20*i, i=0..7, then goto $F61D: a 16-frame loop.
/// Each record ends in $C595 wait; each goto targets its first record.
/// The next index in each program reaches control, not presentation data.
/// All 44 mechanics words match the pinned NTSC J/U v1.0 ROM.
/// </remarks>
public static class MaridiaEnvironmentalPaletteFxColorDefinitions
{
    /// <summary>$8D:F4EF/F547: four-color sand rotation group.</summary>
    internal const int SandRotationColors = 4;
    /// <summary>$8D:F4EF: pair of separately rotating sand-pit color bands.</summary>
    internal const int SandPitBandCount = 2;
    /// <summary>$8D:F57F: eight-color waterfall rotation group.</summary>
    internal const int WaterfallRotationColors = 8;

    /// <summary>The sand-pit, sand-fall, and waterfall programs in native definition order, calculated without cached records.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxColorDefinition> All { get; } = new DefinitionSequence();

    /// <summary>Creates Maridia owner definitions on demand in native definition order.</summary>
    private sealed class DefinitionSequence : IReadOnlyList<MaridiaEnvironmentalPaletteFxColorDefinition>
    {
        /// <summary>Sand pits, sand falls, and background waterfalls are the three compiled owners.</summary>
        public int Count => 3;
        /// <summary>Constructs the program definition at its native owner-order index.</summary>
        public MaridiaEnvironmentalPaletteFxColorDefinition this[int index] => index switch
        {
            0 => new(MaridiaEnvironmentalPaletteOwner.SandPits),
            1 => new(MaridiaEnvironmentalPaletteOwner.SandFalls),
            2 => new(MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates the three owner definitions without retaining cached records.</summary>
        public IEnumerator<MaridiaEnvironmentalPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Shares the typed owner-order enumerator with non-generic callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
