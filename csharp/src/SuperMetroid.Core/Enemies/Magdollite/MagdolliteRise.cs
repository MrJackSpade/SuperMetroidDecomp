using static SuperMetroid.Core.Enemies.Magdollite.MagdollitePhaseDefinitions;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>
/// Magdollite's rising body: its phase at each rise. Whether an arm grows a final phase at full height is
/// <see cref="MagdolliteApexGrowth"/>'s.
/// </summary>
internal static class MagdolliteRise
{
    /// <summary>The distance threshold, body animation and overlay offset of phase <paramref name="index"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is outside the nine authored phases.</exception>
    internal static MagdollitePhaseDefinition Phase(int index) =>
        (uint)index < (uint)Phases.Length
            ? Phases[index]
            : throw new InvalidDataException(
                $"Magdollite phase {index} is outside the nine authored phases.");
}
