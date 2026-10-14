using static SuiteTimingVerification;
using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static BombTorizoHandArtworkVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;

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
            var samus = new SamusState { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus);
            if (able) samus.Drained.SetupForRainbowBeamAbleToStand(bus, samus);
            else samus.Drained.SetupForRainbowBeamUnableToStand(bus, samus);
            samus.Drained.PutCrouchingOrFalling(bus, samus);
            if (!able)
            {
                AssertTrue(samus.Drained.StepGetUpHandler(samus, (ushort)SnesButton.Up), "cutscene still accepts failed stand-up");
                AssertEqual((ushort)18, samus.AnimationFrame, "native failed stand-up still selects frame eighteen");
            }

            // Begin at the identified native timer-handoff phase, not a boss playthrough.
            var sequence = new MotherBrainRainbowBeamAttackSequence();
            sequence.Phase = MotherBrainRainbowBeamAttackPhase.Phase3DeathSequenceDoorExplodingStartTimer;
            var request = sequence.Step(bus, samus, new RainbowBeamAttackFrame { EnemyFrameCounter = 0, MainEnemyExecutionCounter = 0, NextRandomNumber = () => 0 });
            AssertTrue(request.TimerHandlingEnableRequested && request.MotherBrainEscapeTimerStartRequested,
                "native death phase emits command F and timer start together");
            var runtime = new SuperMetroidRuntime(bus, HostGameplayOverrides.None, initialPaletteArt: BombTorizoHandFixturePalette());
            runtime.Player.Samus = samus;
            var motherBrain = new MotherBrainEnemyState(runtime.Enemies.SlotTable.Slots[0])
            {
                // Area flags are outside this fixture. Preserve the actual timer requests.
                LastRainbowBeamStep = request with { MotherBrainBossBitRequested = false, ZebesTimebombEventRequested = false },
            };
            const BindingFlags flags = BindingFlags.NonPublic | BindingFlags.Instance;
            runtime.Enemies.MotherBrainEncounter.MotherBrain = motherBrain;
            runtime.EnemyRoomEffects.ApplyPendingMotherBrainPlms();

            samus.Pose = pose;
            samus.InputLocked = false;
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Presentation.InitializeAnimation(bus, initialFrame: 8);
            samus.Presentation.SetAnimationFrameFromSpecialHandler(8, 1);
            samus.LiquidPhysics.FxType = RoomFxType.Acid;
            samus.LiquidPhysics.LavaAcidYPosition = 0;
            samus.Drained.StepGetUpHandler(samus, (ushort)(SnesButton.Up | SnesButton.A));
            // Before the fix, the stale failed-stand callback selects 18/1 here and the
            // real animation/footstep path throws the exact player-reported exception.
            samus.Presentation.AnimateNoFx(bus, new SamusAnimationStep { ControllerInput = (ushort)(SnesButton.Up | SnesButton.A) });
            AssertEqual(DrainedGetUpHandler.Inactive, samus.Drained.GetUpHandler,
                "escape timer replaces the drained hack handler");
            AssertEqual((ushort)9, samus.AnimationFrame, "Up edge in acid advances the running animation normally");
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
