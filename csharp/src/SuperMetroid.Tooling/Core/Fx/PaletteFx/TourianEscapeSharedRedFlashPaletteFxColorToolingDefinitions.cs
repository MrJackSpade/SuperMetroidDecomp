namespace SuperMetroid.Core.Fx.PaletteFx;

/// <summary>Development-tool members of <see cref="TourianEscapeSharedRedFlashPaletteFxColorDefinitions"/>; never linked by player hosts.</summary>
internal static class TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions
{
    /// <summary>The two entries into the shared red-flash loop.</summary>
    [AccessedByReflection]
    public static IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxColorDefinition> All =>
        TourianEscapeSharedRedFlashPaletteFxColorToolingDefinitions.Definitions;
    /// <summary><c>PalFxDef_Tourian20</c> at <c>$8D:FFD1</c>.</summary>
    public const ushort GeneralLevelDefinitionPointer = 0xffd1;
    /// <summary><c>PalFxDef_Tourian40</c> at <c>$8D:FFD5</c>.</summary>
    public const ushort ArkanoidDefinitionPointer = 0xffd5;
    /// <summary><c>PalFxInstList_Tourian20</c> at <c>$8D:F941</c>.</summary>
    public const ushort GeneralLevelProgramStart = 0xf941;
    /// <summary><c>PalFxInstList_Tourian40</c> at <c>$8D:F949</c>.</summary>
    public const ushort ArkanoidProgramStart = 0xf949;
    /// <summary>CGRAM byte $A8: general-level red-flash destination.</summary>
    internal const ushort GeneralColorByte = 0x00a8;
    /// <summary>CGRAM byte $E8: Arkanoid-block and red-orb flash destination.</summary>
    internal const ushort ArkanoidColorByte = 0x00e8;
    /// <summary>Both palette-FX entries, ordered by their owner value and materialized without a backing array.</summary>
    internal static readonly IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxColorDefinition> Definitions = new ProgramEntries();
    /// <summary>Projects each owner onto its definition pointer, entry list, CGRAM destination, and branch policy.</summary>
    internal sealed class ProgramEntries : IReadOnlyList<TourianEscapeSharedRedFlashPaletteFxColorDefinition>
    {
        /// <summary>Tourian's two definitions converge on the same timed-record loop.</summary>
        public int Count => 2;
        /// <summary>Indexes the general-level entry first and the Arkanoid/red-orb entry second.</summary>
        public TourianEscapeSharedRedFlashPaletteFxColorDefinition this[int index] =>
            (TourianEscapeSharedRedFlashPaletteOwner)index switch
            {
                TourianEscapeSharedRedFlashPaletteOwner.GeneralLevel => new(
                    TourianEscapeSharedRedFlashPaletteOwner.GeneralLevel,
                    GeneralLevelDefinitionPointer, GeneralLevelProgramStart, GeneralColorByte, usesGoto: true),
                TourianEscapeSharedRedFlashPaletteOwner.ArkanoidBlocksAndRedOrbs => new(
                    TourianEscapeSharedRedFlashPaletteOwner.ArkanoidBlocksAndRedOrbs,
                    ArkanoidDefinitionPointer, ArkanoidProgramStart, ArkanoidColorByte, usesGoto: false),
                _ => throw new ArgumentOutOfRangeException(nameof(index)),
            };
        /// <summary>Visits both entries in the same order as the owner values.</summary>
        public IEnumerator<TourianEscapeSharedRedFlashPaletteFxColorDefinition> GetEnumerator()
        {
            for (int index = 0; index < Count; index++)
                yield return this[index];
        }
        /// <summary>Uses the typed entry iteration for callers of the non-generic collection interface.</summary>
        System.Collections.IEnumerator System.Collections.IEnumerable.GetEnumerator() => GetEnumerator();
    }
}
