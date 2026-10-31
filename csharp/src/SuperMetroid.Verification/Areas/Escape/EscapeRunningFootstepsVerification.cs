using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Bosses.Torizo.BombTorizoHandArtworkVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Gameplay;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Areas.Escape;

using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

/// <summary>Checks the escape-timer handoff from drained Samus to running animation.</summary>
internal static class EscapeRunningFootstepsVerification
{
    /// <summary>Exercises both stand-up outcomes and facings through the reported acid footstep path.</summary>
    internal static void VerifyEscapeRunningFootsteps()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        foreach (bool able in new[] { false, true })
        foreach (SamusPoseId pose in new[] { SamusPoseId.MovingRightNormalPose, SamusPoseId.MovingLeftNormalPose })
        {
            var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            if (able) samus.Drained.SetupForRainbowBeamAbleToStand(samus);
            else samus.Drained.SetupForRainbowBeamUnableToStand(samus);
            samus.Drained.PutCrouchingOrFalling(samus);
            if (!able)
            {
                AssertTrue(samus.Drained.StepGetUpHandler(samus, ControllerButton.Up), "cutscene still accepts failed stand-up");
                AssertEqual(18, samus.AnimationFrame, "native failed stand-up still selects frame eighteen");
            }

            // Begin at the identified native timer-handoff phase, not a boss playthrough.
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.State.Phase = MotherBrainBodyFunction.ThirdPhaseDeathDoorExplodingStartTimer;
            var request = sequence.Step(samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0, NextRandomNumber = () => 0 });
            AssertTrue(request.TimerHandlingEnableRequested && request.MotherBrainEscapeTimerStartRequested,
                "native death phase emits command F and timer start together");
            var runtime = SuperMetroid.Core.Runtime.SuperMetroidRuntime.Create(HostGameplayOverrides.None, InstalledFixtures.SamusOverreads, initialPaletteArt: BombTorizoHandFixturePalette());
            runtime.Player.Samus = samus;
            var motherBrain = new MotherBrainEnemyState(runtime.Enemies.SlotTable.Slots[0]);
            runtime.Enemies.MotherBrainEncounter.MotherBrain = motherBrain;
            // Area flags are outside this fixture. Preserve the actual timer requests.
            runtime.EnemyRoomEffects.ApplyPendingMotherBrainPlms(EnemyRoomEffectRequestsConstants.None with
            {
                MotherBrainDeathStep = request with { MotherBrainBossBitRequested = false, ZebesTimebombEventRequested = false },
            }, runtime.Plms, runtime.RoomLayer3Fx, runtime.RoomPaletteFx);

            samus.Pose = pose;
            samus.Control.Unlock();
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus, initialFrame: 8);
            SamusPresentation.SetAnimationFrameFromSpecialHandler(samus, 8, 1);
            samus.LiquidPhysics.FxType = RoomFxType.Acid;
            samus.LiquidPhysics.LavaAcidYPosition = 0;
            samus.Drained.StepGetUpHandler(samus, (ControllerButton.Up | ControllerButton.A));
            // Before the fix, the stale failed-stand callback selects 18/1 here and the
            // real animation/footstep path throws the exact player-reported exception.
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { Random = new SharedRandom(), ControllerInput = (ControllerButton.Up | ControllerButton.A) });
            AssertEqual(DrainedGetUpHandler.Inactive, samus.Drained.GetUpHandler,
                "escape timer replaces the drained hack handler");
            AssertEqual(9, samus.AnimationFrame, "Up edge in acid advances the running animation normally");
            AssertTrue(!samus.AnimationFrameTimer.IsZero, "running cadence receives its real next-frame delay");
        }
        Console.WriteLine("Escape running footsteps: native timer handoff displaces both drained handlers; Up/jump in acid preserves both running animations and the cutscene failed-stand branch.");
    }

    /// <summary>Runs the focused drained-handler and running-cadence regression.</summary>
    [VerificationCommand("--escape-running-footsteps")]
    internal static int EscapeRunningFootstepsCommand()
    {
        Suite(nameof(VerifyEscapeRunningFootsteps), () => VerifyEscapeRunningFootsteps());
        return 0;
    }
}
