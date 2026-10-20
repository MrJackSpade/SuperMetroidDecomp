using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ridley's Power Bomb tail reaction and the fields it must leave untouched.</summary>
internal static class RidleyNeutralTailVerification
{
    /// <summary>Confirms an armed Power Bomb resets tail function and angle delta, preserves animated foot displacement, and leaves inactive or death modes unchanged.</summary>
    internal static void VerifyRidleyPowerBombNeutralTail()
    {
        Action<RidleyEnemyState, SamusBombProjectileSystem> prepare = RidleyBehavior.PrepareNorfairRidleyCombatFrame;
        var armed = new SamusBombProjectileSystem();
        armed.PowerBombExplosion.Arm();
        var unarmed = new SamusBombProjectileSystem();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var state = new RidleyEnemyState
            {
                FightMode = RidleyFightMode.Fighting, GrabState = RidleyGrabState.Released,
                TailFunctionIndex = RidleyTailFunction.Pogo, TailAngleDelta = (ushort)raw,
                FeetDistanceIndex = raw % 3,
            };
            prepare(state, armed);
            AssertEqual(RidleyTailFunction.Neutral, state.TailFunctionIndex, "Power Bomb sets neutral tail function");
            AssertEqual(1, state.TailAngleDelta, "Power Bomb resets native tail angle delta");
            AssertEqual(raw % 3, state.FeetDistanceIndex, "Power Bomb preserves animated foot displacement");
            AssertEqual(RidleyAiFunction.NorfairGrabApproach, state.Function, "Power Bomb lunge handoff");
        }
        foreach (RidleyFightMode fight in Enum.GetValues<RidleyFightMode>())
        foreach (RidleyGrabState grab in Enum.GetValues<RidleyGrabState>())
        foreach (bool flag in new[] { false, true })
        {
            if (fight == RidleyFightMode.Fighting && grab == RidleyGrabState.Released && flag) continue;
            var state = new RidleyEnemyState
            {
                FightMode = fight, GrabState = grab,
                TailFunctionIndex = RidleyTailFunction.Pogo, TailAngleDelta = 8, FeetDistanceIndex = 2,
                Function = RidleyAiFunction.NorfairHover,
            };
            prepare(state, flag ? armed : unarmed);
            AssertEqual(RidleyTailFunction.Pogo, state.TailFunctionIndex, "Inactive reaction preserves tail function");
            AssertEqual(8, state.TailAngleDelta, "Inactive reaction preserves angle delta");
            AssertEqual(2, state.FeetDistanceIndex, "Inactive reaction preserves feet");
            AssertEqual(RidleyAiFunction.NorfairHover, state.Function, "Inactive reaction preserves phase");
        }
        Console.WriteLine("Ridley Power Bomb neutral tail: 65536 field-preservation cases and inactive gates match native write ownership.");
    }
}
