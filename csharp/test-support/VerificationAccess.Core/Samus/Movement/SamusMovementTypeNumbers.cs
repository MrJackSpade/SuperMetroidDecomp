namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The checks' reading of native movement bytes (#1481), over the tools' table
/// <see cref="SamusMovementTypeNativeWords"/>: which movement type a byte names, and how many bytes the per-movement
/// tables cover.
/// </summary>
internal static class SamusMovementTypeNumbers
{
    /// <summary>
    /// The number of movement bytes the per-movement tables cover: one per movement type, and the two after knockback
    /// that no pose selects.
    /// </summary>
    internal static int Count => SamusMovementTypeNativeWords.Types.Word(SamusMovementType.Special) + 1;

    /// <summary>
    /// The movement bytes the tables cover that no retail pose selects (#1554), so the game has no movement type for
    /// them; the pose dispatch check confirms every pose's movement byte against the cartridge.
    /// </summary>
    internal static IReadOnlyList<int> Unselected { get; } = [11, 12];

    /// <summary>The movement bytes that name a movement type, in table order.</summary>
    internal static IEnumerable<int> Named => Enumerable.Range(0, Count).Except(Unselected);

    /// <summary>The movement type <paramref name="number"/> names.</summary>
    /// <param name="number">The cartridge's movement byte.</param>
    internal static SamusMovementType Of(int number) => SamusMovementTypeNativeWords.Types.Decode(number, "Samus movement byte");
}
