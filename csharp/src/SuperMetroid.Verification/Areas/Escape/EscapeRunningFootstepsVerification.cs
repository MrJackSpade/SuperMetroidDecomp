using SuperMetroid.Core.World;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies;
namespace SuperMetroid.Verification.Areas.Escape;

using SuperMetroid.Verification.Harness;

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
            var samus = new SamusState(GameplayAudioRequests.Standalone(), InstalledFixtures.FramesBeyondAnimation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState(), global::SuperMetroid.TestSupport.RepositoryInstallation.Verification.InstalledFixtures.SamusArtwork()) { Pose = SamusPoseId.FacingRightNormalPose, Position = FixedPosition<RoomPixels>.FromWhole(new(100, 100)) };
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            if (able) samus.Drained.SetupForRainbowBeamAbleToStand(samus);
            else samus.Drained.SetupForRainbowBeamUnableToStand(samus);
            samus.Drained.CrouchDrained(samus);
            if (!able)
            {
                AssertTrue(samus.Drained.RunGetUpCheck(samus, GameplayAction.Up), "cutscene still accepts failed stand-up");
                AssertEqual(18, samus.AnimationNumber, "native failed stand-up still selects frame eighteen");
            }

            // Begin at the identified native timer-handoff phase, not a boss playthrough.
            var sequence = MotherBrainLateFightFixtures.NewSequence();
            sequence.Encounter.DeathPhase = MotherBrainDeathPhase.EscapeDoorBursts;
            var request = sequence.Step(samus, MotherBrainLateFightFixtures.QuietFrame(0, 0) with { NextRandomNumber = () => 0 });
            AssertTrue(request.Starts(MotherBrainStepEffect.GetUpRelease) && request.Starts(MotherBrainStepEffect.EscapeTimerStart),
                "native death phase emits command F and timer start together");
            // The death step's requests are applied in Mother Brain's room, whose PLMs, FX and enemies the room load owns.
            var runtime = CreateRetailRuntimeFixture();
            runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
            runtime.RoomLoader.InitializeStartingCeresRoom();
            runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
            runtime.LoadCartridgeRoomForDebug(RoomDefinitions.MotherBrain);
            runtime.Player.Samus = samus;
            // Area flags are outside this fixture. Preserve the actual timer requests.
            runtime.EnemyRoomEffects.ApplyMotherBrainDeathStep(request with
            {
                Effects = [.. request.Effects.Where(effect => effect is not (MotherBrainStepEffect.DefeatRecord or MotherBrainStepEffect.ZebesTimebombEvent))],
            }, SuperMetroid.Core.World.EnemyRoomEffectSite.Of(runtime.Room.LoadedScope, samus));

            samus.Pose = pose;
            samus.Control.Unlock();
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus, samus.PoseFrameNumbered(8));
            SamusPresentation.SetAnimationFrameFromMovementOwner(samus, samus.AnimationFrameNumbered(8), 1);
            PrivateState.Set(samus.LiquidPhysics, nameof(SamusLiquidPhysicsState.FxType), RoomFxType.Acid);
            samus.LiquidPhysics.LavaAcidSurfaceY = 0;
            samus.Drained.RunGetUpCheck(samus, (GameplayAction.Up | GameplayAction.Jump));
            // Before the fix, the stale failed-stand callback selects 18/1 here and the
            // real animation/footstep path throws the exact player-reported exception.
            SamusPresentation.Animate(samus, new SamusAnimationStep { Ground = new SuperMetroid.Core.Samus.Drawing.SamusGround(SuperMetroid.Core.Samus.Drawing.SamusGroundWetness.Dry, DryLandingsRaiseDust: false),  Random = new SharedRandom(), ControllerInput = (GameplayAction.Up | GameplayAction.Jump) });
            AssertEqual(DrainedGetUpCheck.Inactive, samus.Drained.GetUpCheck,
                "escape timer replaces the drained hack handler");
            AssertEqual(9, samus.AnimationNumber, "Up edge in acid advances the running animation normally");
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
