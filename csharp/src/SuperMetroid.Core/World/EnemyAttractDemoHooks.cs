using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The attract demo hooks over Kraid.</summary>
/// <param name="encounters">The room's bosses' encounters, whose waits on Kraid his body's setup runs.</param>
internal sealed class EnemyAttractDemoHooks(RoomBossEncounters encounters) : IEnemyAttractDemoHooks
{
    /// <inheritdoc/>
    /// <remarks>The wait is composed with the encounter Kraid's body begins, as it finishes setting up (#2218, #2554).</remarks>
    public void StartKraidPhaseWaitAsHeSetsUp(int updates) =>
        encounters.WhenKraidSetsUp(kraid => kraid.BodyState.PhaseWait.LastFor(updates));
}
