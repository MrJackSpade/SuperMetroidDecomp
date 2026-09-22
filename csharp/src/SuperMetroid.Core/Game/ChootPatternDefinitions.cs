namespace SuperMetroid.Core.Game;

/// <summary>One authored Choot falling stream and its per-loop vertical advance.</summary>
internal readonly record struct ChootPatternDefinition(
    ushort FallingPatternPointer,
    ushort FallingPatternYDistance);

/// <summary>Compiled selectors and physical loop distances for Choot's five fall patterns.</summary>
internal static class ChootPatternDefinitions
{
    /// <summary>
    /// The five real pointer selections at <c>$A2:DF5E-$DF67</c> and the distances reached
    /// through <c>$A2:DF6A-$DF73</c>. The sixth pointer word at <c>$DF68</c> is table alias
    /// garbage and is deliberately outside the authored domain.
    /// </summary>
    /// <remarks>
    /// Pointer selection: the pinned NTSC J/U v1.0 ROM gives
    /// <c>D84C,D976,DAA0,DBCA,DD44</c> for selectors 0..4. The next word is
    /// <c>DF5E</c>, an alias back into the pointer table, not a sixth path.
    /// Retain these five named path identities: deriving their addresses from
    /// physical stream lengths would encode ROM layout rather than behavior.
    /// Investigation: #625 / #660.
    /// </remarks>
    private static readonly ChootPatternDefinition[] Patterns =
    [
        new(0xd84c, 0x001e),
        new(0xd976, 0x001c),
        new(0xdaa0, 0x0020),
        new(0xdbca, 0x001e),
        new(0xdd44, 0x001e),
    ];

    /// <summary>Returns one of the five genuine Choot falling-pattern definitions.</summary>
    internal static ChootPatternDefinition ForIndex(ushort patternIndex)
    {
        if (patternIndex >= Patterns.Length)
        {
            throw new InvalidDataException(
                $"Choot falling-pattern index {patternIndex} exceeds its five real streams.");
        }

        return Patterns[patternIndex];
    }
}
