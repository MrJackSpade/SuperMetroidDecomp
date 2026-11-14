using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.CartridgeTables;

/// <summary>
/// Where a cinematic actor's placement is read from: the role it places and the bank-$8B word of its initializer's first
/// immediate. Every placement initializer loads the horizontal word, stores it, then loads the vertical word, so the
/// second immediate always lies <see cref="CinematicPlacementOperands.Distance"/> bytes after the first.
/// </summary>
/// <typeparam name="TRole">The scene's actor roles.</typeparam>
/// <param name="Role">The actor the placement places.</param>
/// <param name="FirstOperand">The bank-$8B address of its horizontal immediate.</param>
internal readonly record struct CinematicPlacementSourceDefinition<TRole>(TRole Role, int FirstOperand)
    where TRole : struct, Enum
{
    /// <summary>The bank-$8B address of its horizontal immediate, an offset within the bank.</summary>
    public int FirstOperand { get; init; } =
        Ensure.BetweenInclusive(0, ushort.MaxValue - CinematicPlacementOperands.Distance, FirstOperand);

    /// <summary>The bank-$8B address of its vertical immediate.</summary>
    internal int SecondOperand => FirstOperand + CinematicPlacementOperands.Distance;
}
