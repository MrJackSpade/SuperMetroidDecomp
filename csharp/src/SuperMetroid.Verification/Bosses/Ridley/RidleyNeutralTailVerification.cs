using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.TestSupport.MovieReplay;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Verification.Bosses.Ridley;

/// <summary>Checks Ridley's Power Bomb tail reaction and the fields it must leave untouched.</summary>
internal static class RidleyNeutralTailVerification
{
    /// <summary>Confirms an armed Power Bomb resets tail function and angle delta, preserves animated foot displacement, and leaves inactive or death modes unchanged.</summary>
    internal static void VerifyRidleyPowerBombNeutralTail()
    {
        Action<RidleyEnemyState, SamusBombProjectileSystem> prepare = NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame;
        var armed = new SamusBombProjectileSystem();
        armed.PowerBombExplosion.Arm();
        var unarmed = new SamusBombProjectileSystem();
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var state = new RidleyEnemyState
            {
                FightMode = RidleyFightMode.Fighting, GrabState = RidleyGrabState.Released,
                TailAction = RidleyTailAction.Pogo, TailAngleDelta = (ushort)raw,
                ClawHeight = RidleyClawHeightPlaces.TableOrder[raw % 3],
            };
            prepare(state, armed);
            AssertEqual(RidleyTailAction.Neutral, state.TailAction, "Power Bomb sets neutral tail function");
            AssertEqual(1, state.TailAngleDelta, "Power Bomb resets native tail angle delta");
            AssertEqual(RidleyClawHeightPlaces.TableOrder[raw % 3], state.ClawHeight, "Power Bomb preserves animated foot displacement");
            AssertEqual(RidleyPhase.NorfairGrabApproach, state.Phase, "Power Bomb lunge handoff");
        }
        foreach (RidleyFightMode fight in Enum.GetValues<RidleyFightMode>())
        foreach (RidleyGrabState grab in Enum.GetValues<RidleyGrabState>())
        foreach (bool flag in new[] { false, true })
        {
            if (fight == RidleyFightMode.Fighting && grab == RidleyGrabState.Released && flag) continue;
            var state = new RidleyEnemyState
            {
                FightMode = fight, GrabState = grab,
                TailAction = RidleyTailAction.Pogo, TailAngleDelta = 8, ClawHeight = RidleyClawHeight.LegsExtended,
                Phase = RidleyPhase.NorfairHover,
            };
            prepare(state, flag ? armed : unarmed);
            AssertEqual(RidleyTailAction.Pogo, state.TailAction, "Inactive reaction preserves tail function");
            AssertEqual(8, state.TailAngleDelta, "Inactive reaction preserves angle delta");
            AssertEqual(RidleyClawHeight.LegsExtended, state.ClawHeight, "Inactive reaction preserves feet");
            AssertEqual(RidleyPhase.NorfairHover, state.Phase, "Inactive reaction preserves phase");
        }
        Console.WriteLine("Ridley Power Bomb neutral tail: 65536 field-preservation cases and inactive gates match native write ownership.");
    }
}
