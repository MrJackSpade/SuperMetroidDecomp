namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's attack selection after a hover.</summary>
internal static class CeresRidleyAttackDefinitions
{
    /// <summary>
    /// The attack the room's current random number chooses after a hover (#2510): runs of rolls choose a fireball route, a
    /// lunge, a fireball route, a swoop and a fireball route in turn, so the fireballs are a little likelier than the lunge
    /// or the swoop. The choice reads the current number; it draws no other.
    /// </summary>
    internal static readonly WeightedRandomChoice<CeresRidleyPhase> Attack = new(new RandomSampleField(0, 4),
        new(4, CeresRidleyPhase.FireballMoveToPosition),
        new(5, CeresRidleyPhase.LungeSetup),
        new(1, CeresRidleyPhase.FireballMoveToPosition),
        new(5, CeresRidleyPhase.SwoopSetup),
        new(1, CeresRidleyPhase.FireballMoveToPosition));
}
