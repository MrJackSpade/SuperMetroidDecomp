namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's attack selection after a hover.</summary>
internal static class CeresRidleyAttackDefinitions
{
    /// <summary>
    /// Sixteen attack functions indexed by the room's random number modulo 16
    /// (the attack choice reads the seed; it does not generate another number). Six entries start a
    /// fireball route, five a lunge and five a swoop.
    /// </summary>
    private static ReadOnlySpan<RidleyPhase> AttackByRandomNibble =>
    [
        RidleyPhase.CeresFireballMoveToPosition, RidleyPhase.CeresFireballMoveToPosition,
        RidleyPhase.CeresFireballMoveToPosition, RidleyPhase.CeresFireballMoveToPosition,
        RidleyPhase.CeresLungeSetup, RidleyPhase.CeresLungeSetup,
        RidleyPhase.CeresLungeSetup, RidleyPhase.CeresLungeSetup,
        RidleyPhase.CeresLungeSetup, RidleyPhase.CeresFireballMoveToPosition,
        RidleyPhase.CeresSwoopSetup, RidleyPhase.CeresSwoopSetup,
        RidleyPhase.CeresSwoopSetup, RidleyPhase.CeresSwoopSetup,
        RidleyPhase.CeresSwoopSetup, RidleyPhase.CeresFireballMoveToPosition,
    ];

    /// <summary>The attack choice, one of sixteen: the sample modulo 16.</summary>
    private static readonly RandomSampleField AttackChoice = new(0, 4);

    /// <summary>The attack the table selects for a random number; only its <see cref="AttackChoice"/> indexes the table.</summary>
    internal static RidleyPhase SelectAttack(int random) => AttackByRandomNibble[AttackChoice.Of(random)];
}
