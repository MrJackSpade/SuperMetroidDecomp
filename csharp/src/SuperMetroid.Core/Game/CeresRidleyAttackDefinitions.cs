namespace SuperMetroid.Core.Game;

/// <summary>Ceres Ridley's attack selection after a hover.</summary>
internal static class CeresRidleyAttackDefinitions
{
    /// <summary>
    /// $A6:A743: sixteen function pointers indexed by the low nibble of RandomNumberSeed
    /// ($A6:A72D reads the seed; it does not generate another number). Six entries start a
    /// fireball route, five a lunge and five a swoop.
    /// </summary>
    private static ReadOnlySpan<RidleyAiFunction> AttackByRandomNibble =>
    [
        RidleyAiFunction.CeresFireballMoveToPosition, RidleyAiFunction.CeresFireballMoveToPosition,
        RidleyAiFunction.CeresFireballMoveToPosition, RidleyAiFunction.CeresFireballMoveToPosition,
        RidleyAiFunction.CeresLungeSetup, RidleyAiFunction.CeresLungeSetup,
        RidleyAiFunction.CeresLungeSetup, RidleyAiFunction.CeresLungeSetup,
        RidleyAiFunction.CeresLungeSetup, RidleyAiFunction.CeresFireballMoveToPosition,
        RidleyAiFunction.CeresSwoopSetup, RidleyAiFunction.CeresSwoopSetup,
        RidleyAiFunction.CeresSwoopSetup, RidleyAiFunction.CeresSwoopSetup,
        RidleyAiFunction.CeresSwoopSetup, RidleyAiFunction.CeresFireballMoveToPosition,
    ];

    /// <summary>The attack the table selects for a random word; only its low nibble indexes the table.</summary>
    internal static RidleyAiFunction SelectAttack(ushort random) => AttackByRandomNibble[random & 0x0f];
}
