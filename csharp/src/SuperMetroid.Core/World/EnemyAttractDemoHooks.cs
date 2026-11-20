using SuperMetroid.Core.Bosses;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The attract demo hooks over Kraid.</summary>
/// <param name="encounters">The room's bosses' encounters, Kraid's among them once his body sets up.</param>
internal sealed class EnemyAttractDemoHooks(RoomBossEncounters encounters) : IEnemyAttractDemoHooks
{
    /// <inheritdoc/>
    /// <remarks>
    /// The demo reaches the encounter Kraid's body began, which the room's bosses' encounters hold (#2218, #2554). A room
    /// without Kraid has no phase wait to start.
    /// </remarks>
    public void StartKraidPhaseWait(int updates)
    {
        if (encounters.Kraid is { } kraid)
            kraid.BodyState.PhaseWait.LastFor(updates);
    }
}
