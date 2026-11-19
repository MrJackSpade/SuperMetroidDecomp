using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.World;

/// <summary>The attract demo hooks over Kraid.</summary>
/// <param name="kraid">Kraid.</param>
internal sealed class EnemyAttractDemoHooks(KraidBehavior kraid) : IEnemyAttractDemoHooks
{
    /// <inheritdoc/>
    /// <remarks>The demo reaches the encounter Kraid's body began through his parts, whose body holds it (#2218).</remarks>
    public void StartKraidPhaseWait(int updates) =>
        kraid.Parts.Encounter.BodyState.PhaseWait.LastFor(updates);
}
