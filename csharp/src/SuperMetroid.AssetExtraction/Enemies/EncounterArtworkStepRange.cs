using SuperMetroid.Core;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>
/// Consecutive steps of an encounter's artwork sequence at which frames are drawn (#1513): from <paramref name="First"/>
/// through <paramref name="Last"/>, or through the sequence's last step when <paramref name="Last"/> is none.
/// </summary>
/// <param name="First">The first step.</param>
/// <param name="Last">The last step, or none for the sequence's last.</param>
internal sealed record EncounterArtworkStepRange(int First, int? Last)
{
    /// <summary>The first step.</summary>
    public int First { get; } = Ensure.AtLeastZero(First);

    /// <summary>The last step, or none for the sequence's last.</summary>
    public int? Last { get; } = Last is int last && last < First
        ? throw new InvalidDataException($"An artwork step range ends at step {last}, before its first step {First}.") : Last;

    /// <summary>Whether <paramref name="step"/> lies in the range.</summary>
    internal bool Contains(int step) => step >= First && (Last is not int last || step <= last);
}
