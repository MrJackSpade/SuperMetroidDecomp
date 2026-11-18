namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's attack selection after a hover.</summary>
internal static class CeresRidleyAttackDefinitions
{
    /// <summary>
    /// The attack the room's current random number chooses after a hover, of sixteen rolls (#2510): four start a fireball
    /// route, five a lunge, one a fireball route, five a swoop and the last a fireball route, so six choose the fireballs,
    /// five the lunge and five the swoop. The choice reads the current number; it draws no other.
    /// </summary>
    internal static readonly WeightedRandomChoice<CeresRidleyPhase> Attack = new(new RandomSampleField(0, 4),
        new(4, CeresRidleyPhase.FireballMoveToPosition),
        new(5, CeresRidleyPhase.LungeSetup),
        new(1, CeresRidleyPhase.FireballMoveToPosition),
        new(5, CeresRidleyPhase.SwoopSetup),
        new(1, CeresRidleyPhase.FireballMoveToPosition));
}
