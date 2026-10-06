namespace SuperMetroid.Core.Game;

/// <summary>One Choot falling stream and its per-loop vertical advance.</summary>
internal readonly record struct ChootPatternDefinition(
    ushort FallingPatternPointer,
    ushort FallingPatternYDistance);

/// <summary>Named motion selection and loop-origin advance for Choot falling patterns.</summary>
internal static class ChootPatternDefinitions
{
    /// <summary>$A2:D974, also DD42/DF5C: selected normal/slow loop-origin advance30; its magnitude remains unresolved.</summary>
    private const ushort UnresolvedNormalLoopYDistance = 30;
    /// <summary>$A2:DA9E: selected wide loop-origin advance28; its magnitude remains unresolved.</summary>
    private const ushort UnresolvedWideLoopYDistance = 28;
    /// <summary>$A2:DBC8: selected very-wide loop-origin advance32; its magnitude remains unresolved.</summary>
    private const ushort UnresolvedVeryWideLoopYDistance = 32;

    /// <summary>
    /// $A2:DF5E-$DF67 selects normal, wide, very wide, slow and very slow motion.
    /// $A2:DF6A-$DF73 selects the corresponding loop Y advances. Slow variants
    /// repeat normal-path plateau frames and use the same vertical advance.
    /// The following alias words are outside the five supported selectors.
    /// </summary>
    internal static ChootPatternDefinition ForIndex(ushort patternIndex) => patternIndex switch
    {
        0 => new(ChootFallingPathDefinitions.NormalPointer, UnresolvedNormalLoopYDistance),
        1 => new(ChootFallingPathDefinitions.WidePointer, UnresolvedWideLoopYDistance),
        2 => new(ChootFallingPathDefinitions.VeryWidePointer, UnresolvedVeryWideLoopYDistance),
        3 => new(ChootFallingPathDefinitions.SlowPointer, UnresolvedNormalLoopYDistance),
        4 => new(ChootFallingPathDefinitions.VerySlowPointer, UnresolvedNormalLoopYDistance),
        _ => throw new InvalidDataException(
            $"Choot falling-pattern index {patternIndex} exceeds its five real streams."),
    };
}
