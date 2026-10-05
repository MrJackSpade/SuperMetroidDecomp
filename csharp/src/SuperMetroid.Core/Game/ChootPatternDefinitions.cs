namespace SuperMetroid.Core.Game;

/// <summary>One Choot falling stream and its per-loop vertical advance.</summary>
internal readonly record struct ChootPatternDefinition(
    ushort FallingPatternPointer,
    ushort FallingPatternYDistance);

/// <summary>Named motion selection and loop-origin advance for Choot falling patterns.</summary>
internal static class ChootPatternDefinitions
{
    /// <summary>
    /// $A2:DF5E-$DF67 selects normal, wide, very wide, slow and very slow motion.
    /// $A2:DF6A-$DF73 selects the corresponding loop Y advances. Slow variants
    /// repeat normal-path plateau frames and use the same vertical advance.
    /// The following alias words are outside the five supported selectors.
    /// </summary>
    internal static ChootPatternDefinition ForIndex(ushort patternIndex) => patternIndex switch
    {
        0 => new(ChootFallingPathDefinitions.NormalPointer, 30),
        1 => new(ChootFallingPathDefinitions.WidePointer, 28),
        2 => new(ChootFallingPathDefinitions.VeryWidePointer, 32),
        3 => new(ChootFallingPathDefinitions.SlowPointer, 30),
        4 => new(ChootFallingPathDefinitions.VerySlowPointer, 30),
        _ => throw new InvalidDataException(
            $"Choot falling-pattern index {patternIndex} exceeds its five real streams."),
    };
}