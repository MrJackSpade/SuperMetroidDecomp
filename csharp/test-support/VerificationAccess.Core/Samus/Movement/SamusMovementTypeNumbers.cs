namespace SuperMetroid.Core.Samus.Movement;

/// <summary>
/// The checks' reading of native movement bytes (#1481), over the tools' table
/// <see cref="SamusMovementTypeNativeWords"/>: which movement type a byte names, and how many bytes the per-movement
/// tables cover.
/// </summary>
internal static class SamusMovementTypeNumbers
{
    /// <summary>The number of movement bytes the per-movement tables cover: one per movement type.</summary>
    internal static int Count => SamusMovementTypeNativeWords.Types.Word(SamusMovementType.Special) + 1;

    /// <summary>The movement type <paramref name="number"/> names.</summary>
    /// <param name="number">The cartridge's movement byte.</param>
    internal static SamusMovementType Of(int number) => SamusMovementTypeNativeWords.Types.Decode(number, "Samus movement byte");
}
