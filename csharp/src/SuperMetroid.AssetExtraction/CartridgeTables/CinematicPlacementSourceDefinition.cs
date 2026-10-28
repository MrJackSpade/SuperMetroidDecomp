using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where a cinematic actor's placement is read from: its asset id and the bank-$8B word of its initializer's first
/// immediate. Every placement initializer loads the horizontal word, stores it, then loads the vertical word, so the
/// second immediate always lies <see cref="OperandDistance"/> bytes after the first.
/// </summary>
/// <param name="Id">The actor's asset id.</param>
/// <param name="FirstOperand">The bank-$8B address of its horizontal immediate.</param>
internal readonly record struct CinematicPlacementSourceDefinition(string Id, int FirstOperand)
{
    /// <summary>The bank-$8B address of its horizontal immediate, an offset within the bank.</summary>
    public int FirstOperand { get; init; } = Ensure.BetweenInclusive(0, ushort.MaxValue - OperandDistance, FirstOperand);

    /// <summary>The bytes from the first immediate to the second: the first immediate's word and the store between them.</summary>
    internal const int OperandDistance = 6;

    /// <summary>The bank-$8B address of its vertical immediate.</summary>
    internal int SecondOperand => FirstOperand + OperandDistance;
}
