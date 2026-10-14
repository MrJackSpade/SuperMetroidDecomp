namespace SuperMetroid.Core.Game;

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
public static class MaridiaEnvironmentalPaletteFxProgramMechanicsDefinitions
{
    /// <summary>$8D:F4E9: native Maridia1 color-index setup, followed contiguously by sand-fall and waterfall programs.</summary>
    internal const ushort SandPitProgram = 0xF4E9;
    /// <summary>$8D:F4EF/F547: four-color sand rotation group.</summary>
    internal const int SandRotationColors = 4;
    /// <summary>$8D:F4EF: pair of separately rotating sand-pit color bands.</summary>
    internal const int SandPitBandCount = 2;
    /// <summary>$8D:F57F: eight-color waterfall rotation group.</summary>
    internal const int WaterfallRotationColors = 8;
    /// <summary>$8D:F4EB/F543: selected sand background palette slot2.</summary>
    internal const int SandPalette = 2;
    /// <summary>$8D:F57B: selected waterfall background palette slot3.</summary>
    internal const int WaterfallPalette = 3;
    /// <summary>$8D:F4EB: first sand-pit color4 within its palette.</summary>
    internal const int SandPitFirstColor = 4;
    /// <summary>$8D:F543: first sand-fall color8 within its palette.</summary>
    internal const int FallingFirstColor = 8;
    /// <summary>$8D:F57B operand0068: waterfall first color4 in palette3; the native source comment incorrectly labels colors8..F.</summary>
    internal const int WaterfallFirstColor = 4;
    /// <summary>$8D:F4ED/F545: authored ten-frame sand cadence.</summary>
    internal const ushort SandDuration = 10;
    /// <summary>$8D:F57D: selected two-frame waterfall cadence.</summary>
    internal const ushort WaterfallDuration = 2;

    /// <summary>The sand-pit, sand-fall, and waterfall programs in native definition order, calculated without cached records.</summary>
    public static IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition> All { get; } = new DefinitionSequence();

    /// <summary>Creates Maridia owner definitions on demand in native definition order.</summary>
    private sealed class DefinitionSequence : IReadOnlyList<MaridiaEnvironmentalPaletteFxProgramDefinition>
    {
        /// <summary>Sand pits, sand falls, and background waterfalls are the three compiled owners.</summary>
        public int Count => 3;
        /// <summary>Constructs the program definition at its native owner-order index.</summary>
        public MaridiaEnvironmentalPaletteFxProgramDefinition this[int index] => index switch
        {
            0 => new(MaridiaEnvironmentalPaletteOwner.SandPits),
            1 => new(MaridiaEnvironmentalPaletteOwner.SandFalls),
            2 => new(MaridiaEnvironmentalPaletteOwner.BackgroundWaterfalls),
            _ => throw new ArgumentOutOfRangeException(nameof(index)),
        };
        /// <summary>Enumerates the three owner definitions without retaining cached records.</summary>
        public IEnumerator<MaridiaEnvironmentalPaletteFxProgramDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++) yield return this[index];
        }
        /// <summary>Shares the typed owner-order enumerator with non-generic callers.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
    /// <summary>Resolves one compiled mechanics word across all three programs.</summary>
    public static bool TryReadMechanicsWord(ushort pointer, out ushort value)
    {
        foreach (MaridiaEnvironmentalPaletteFxProgramDefinition definition in All)
        {
            if (definition.TryReadMechanicsWord(pointer, out value))
                return true;
        }

        value = 0;
        return false;
    }
}
