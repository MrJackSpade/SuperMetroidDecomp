namespace SuperMetroid.Core.Samus;

/// <summary>
/// Taking one from a count of Samus's whose caller has established it is positive (#1323). The range of her counts, the
/// unchecked decrement that runs a count below zero and how the HUD shows a count below zero are the compatibility
/// layer's (<see cref="Compatibility.CountsRunBelowZero"/>).
/// </summary>
internal static class SamusQuantity
{
    /// <summary>
    /// Takes one from a count whose caller has established it is positive. The game decrements without a check, so a
    /// violated guard fails here instead of wrapping.
    /// </summary>
    internal static int DecrementPositive(int count) => count > 0
        ? count - 1
        : throw new InvalidOperationException($"Decrementing Samus count {count} breaks its caller's positive-count guard.");
}
