using static SuperMetroid.Core.Enemies.Magdollite.MagdollitePhaseDefinitions;

namespace SuperMetroid.Core.Enemies.Magdollite;

/// <summary>Magdollite's rising body: its phase at each rise and whether an arm grows a final phase at the apex.</summary>
internal static class MagdolliteRise
{
    /// <summary>Whether an arm in <paramref name="enemySlot"/> grows one final phase on reaching the maximum rise.</summary>
    internal static bool ApexGrowsFinalPhase(int enemySlot)
    {
        if ((uint)enemySlot >= (uint)s_apexGrowsByEnemySlot.Length)
            throw new InvalidDataException(
                $"Magdollite enemy slot {enemySlot} is outside the {s_apexGrowsByEnemySlot.Length} enemy slots.");
        return s_apexGrowsByEnemySlot[enemySlot];
    }

    /// <summary>The distance threshold, body animation and overlay offset of phase <paramref name="index"/>.</summary>
    /// <exception cref="InvalidDataException"><paramref name="index"/> is outside the nine authored records.</exception>
    internal static MagdollitePhaseDefinition Phase(int index) =>
        (uint)index < (uint)Phases.Length
            ? Phases[index]
            : throw new InvalidDataException(
                $"Magdollite phase {index} is outside the nine authored records.");
}
