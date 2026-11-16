namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ceres Ridley's attack selection after a hover.</summary>
internal static class CeresRidleyAttackDefinitions
{
    /// <summary>
    /// Sixteen attacks chosen by the room's random number modulo 16
    /// (the attack choice reads the seed; it does not generate another number). Six entries start a
    /// fireball route, five a lunge and five a swoop.
    /// </summary>
    private static ReadOnlySpan<CeresRidleyPhase> AttackByRandomChoice =>
    [
        CeresRidleyPhase.FireballMoveToPosition, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.FireballMoveToPosition, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.LungeSetup,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.LungeSetup,
        CeresRidleyPhase.LungeSetup, CeresRidleyPhase.FireballMoveToPosition,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.SwoopSetup,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.SwoopSetup,
        CeresRidleyPhase.SwoopSetup, CeresRidleyPhase.FireballMoveToPosition,
    ];

    /// <summary>The attack choice, one of sixteen: the sample modulo 16.</summary>
    private static readonly RandomSampleField AttackChoice = new(0, 4);

    /// <summary>The attack the table selects for a random number; only its <see cref="AttackChoice"/> indexes the table.</summary>
    internal static CeresRidleyPhase SelectAttack(int random) => AttackByRandomChoice[AttackChoice.Of(random)];
}
