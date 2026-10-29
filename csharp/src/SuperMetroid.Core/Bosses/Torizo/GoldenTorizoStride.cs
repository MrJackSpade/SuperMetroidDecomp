using static SuperMetroid.Core.Bosses.Torizo.GoldenTorizoStrideDefinitions;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Golden Torizo's planted-foot stride: how far each walking phase advances the body.</summary>
internal static class GoldenTorizoStride
{
    /// <summary>
    /// $AA:D59A-D5C1: each phase transition cancels the existing planted foot's local displacement. At the double-plant
    /// phase opening each half, the outgoing foot moves to the departing origin; otherwise the current support strip
    /// remains fixed. No independent speed is stored.
    /// </summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="phase"/> is not a walking phase.</exception>
    internal static int HorizontalAdvance(int phase)
    {
        int phases = SupportFootXs.Length;
        if ((uint)phase >= (uint)phases) throw new ArgumentOutOfRangeException(nameof(phase));
        int next = phase % PhasesPerHalf == 0 ? DepartingFootX : SupportFootXs[phase];
        return next - SupportFootXs[(phase + phases - 1) % phases];
    }
}
