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
        Action<NorfairRidleyState, SamusBombProjectileSystem> prepare = NorfairRidleyCombat.PrepareNorfairRidleyCombatFrame;
        var armed = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps.PowerBombGlowColors, global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        armed.PowerBombExplosion.Arm();
        var unarmed = new SamusBombProjectileSystem(GameplayAudioRequests.Standalone(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.Maps.PowerBombGlowColors, global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork());
        for (int raw = 0; raw <= ushort.MaxValue; raw++)
        {
            var state = new NorfairRidleyState { FightMode = RidleyFightMode.Fighting, GrabState = RidleyGrabState.Released, Flight = { ClawHeight = RidleyClawHeightPlaces.TableOrder[raw % 3] }, Tail = { Action = RidleyTailAction.Pogo, SwingStep = RidleyTailTurn.OfSteps(raw) } };
            prepare(state, armed);
            AssertEqual(RidleyTailAction.Neutral, state.Tail.Action, "Power Bomb sets neutral tail function");
            AssertEqual(RidleyTailTurn.OfSteps(1), state.Tail.SwingStep, "Power Bomb resets native tail angle delta");
            AssertEqual(RidleyClawHeightPlaces.TableOrder[raw % 3], state.Flight.ClawHeight, "Power Bomb preserves animated foot displacement");
            AssertEqual(NorfairRidleyPhase.GrabApproach, state.Phase, "Power Bomb lunge handoff");
        }
        foreach (RidleyFightMode fight in Enum.GetValues<RidleyFightMode>())
        foreach (RidleyGrabState grab in Enum.GetValues<RidleyGrabState>())
        foreach (bool flag in new[] { false, true })
        {
            if (fight == RidleyFightMode.Fighting && grab == RidleyGrabState.Released && flag) continue;
            var state = new NorfairRidleyState { FightMode = fight, GrabState = grab, Phase = NorfairRidleyPhase.Hover, Flight = { ClawHeight = RidleyClawHeight.LegsExtended }, Tail = { Action = RidleyTailAction.Pogo, SwingStep = RidleyTailTurn.OfSteps(8) } };
            prepare(state, flag ? armed : unarmed);
            AssertEqual(RidleyTailAction.Pogo, state.Tail.Action, "Inactive reaction preserves tail function");
            AssertEqual(RidleyTailTurn.OfSteps(8), state.Tail.SwingStep, "Inactive reaction preserves angle delta");
            AssertEqual(RidleyClawHeight.LegsExtended, state.Flight.ClawHeight, "Inactive reaction preserves feet");
            AssertEqual(NorfairRidleyPhase.Hover, state.Phase, "Inactive reaction preserves phase");
        }
        Console.WriteLine("Ridley Power Bomb neutral tail: 65536 field-preservation cases and inactive gates match native write ownership.");
    }
}
