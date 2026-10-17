using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
using SuperMetroid.Core.Gameplay;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spring Ball equipment changes and airborne jumps through the real pause teardown.</summary>
internal static class SpringBallEquipmentJumpVerification
{
    /// <summary>Uses an underwater West Ocean fixture to check both facings, preserved ascent, and native launch speeds.</summary>
    internal static void VerifySpringBallEquipmentJump()
    {
        var installation = RepositoryInstallation.Installation;
        foreach (bool left in new[] { false, true })
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var game = new SuperMetroidGame(memory, new SuperMetroid.Core.Saves.SaveSlotStore(), new SuperMetroidGameOptions { Invincibility = true }, renderGameplayFrames: false);
            PrepareRomFreeBindings(installation)(game, false);
            game.InitializeDirectRoomVerification();
            var runtime = game.GameplayRuntimeForVerification;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.WestOcean);
            // A bounded underwater fixture retains the retail FX but removes terrain/enemies,
            // isolating the reported pause/equipment/launch contract from room obstacles.
            foreach (var enemy in runtime.Enemies.SlotTable.Slots) enemy.Clear();
            var level = runtime.Room.LoadedLevelData;
            for (int block = 0; block < level.Size.X * level.Size.Y; block++)
            { level.SetForegroundEntry(block, 0); level.SetBehavior(block, 0); }
            var samus = runtime.Player.IntroducedSamus;
            samus.EquippedItems = (SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots);
            samus.CollectedItems = (SamusEquipmentFlags)((ushort)(samus.EquippedItems | SamusEquipmentFlags.SpringBall));
            samus.Position = samus.Position.WithWhole(new(128, (ushort)(runtime.RoomLayer3Fx.Surface.Y + 96)));
            samus.Pose = left ? SamusPoseId.MorphBallFallingLeftPose : SamusPoseId.MorphBallFallingRightPose;
            samus.PoseMetadata.RefreshCollisionRadii();
            runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
            AssertEqual(SamusLiquidMedium.Water, samus.LiquidPhysics.DetermineMovementMedium(samus), "fixture is underwater without Gravity Suit");
            samus.Presentation.InitializeAnimation(3);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = 1;
            samus.Kinematics.YSubspeed = 0x8000;
            for (int jump = 0; jump < 2; jump++)
            {
                Resume(true);
                SamusPoseId springGround = left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose;
                AssertEqual(springGround, samus.Pose, "unpause equips jump-enabled Spring Ball even while rising");
                AssertEqual((ushort)0, samus.AnimationFrame, "cross-family equipment conversion restarts ball animation");
                ushort beforeY = samus.Position.Y.Whole;
                runtime.Controller1.Latch(0);
                runtime.StepFrame(SnesButton.A);
                AssertEqual(left ? SamusPoseId.SpringBallJumpLeftPose : SamusPoseId.SpringBallJumpRightPose,
                    samus.Pose, "new jump input enters powered Spring Ball jump");
                AssertEqual((ushort)2, samus.Kinematics.YSpeed, "native Hi-Jump water launch whole speed");
                AssertEqual((ushort)0x8000, samus.Kinematics.YSubspeed, "native Hi-Jump water launch fractional speed");
                runtime.StepFrame(SnesButton.A);
                AssertTrue(samus.Position.Y.Whole < beforeY, "mid-air equipment jump gains height");
                if (jump == 0)
                {
                    Resume(false);
                    AssertEqual(left ? SamusPoseId.MorphBallGroundLeftPose : SamusPoseId.MorphBallGroundRightPose,
                        samus.Pose, "disabling Spring Ball restores ordinary ball without cancelling ascent");
                }
            }

            void Resume(bool equipped)
            {
                ushort y = samus.Position.Y.Whole, speed = samus.Kinematics.YSpeed, sub = samus.Kinematics.YSubspeed;
                SamusVerticalDirection direction = samus.Kinematics.YDirection;
                if (equipped) samus.EquippedItems |= SamusEquipmentFlags.SpringBall;
                else samus.EquippedItems &= ~SamusEquipmentFlags.SpringBall;
                // The equipment screen writes this live word. Exercise its real forced-blank
                // teardown, where cartridge command $0C must reconcile the pose.
                game.GameState = SuperMetroidGameState.UnpausingB;
                game.Step(0);
                AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "actual frontend teardown completes");
                AssertEqual(y, samus.Position.Y.Whole, "equipment switch does not move Samus");
                AssertEqual(speed, samus.Kinematics.YSpeed, "equipment switch preserves vertical whole speed");
                AssertEqual(sub, samus.Kinematics.YSubspeed, "equipment switch preserves vertical fractional speed");
                AssertEqual(direction, samus.Kinematics.YDirection, "equipment switch preserves ascent");
            }
        }
        Console.WriteLine("Spring Ball equipment jump: real unpause preserves ascent, enables two airborne launches with native water speeds, and handles both facings.");
    }

    /// <summary>Runs the focused underwater equipment-jump verification command.</summary>
    [VerificationCommand("--spring-ball-equipment-jump")]
    internal static int SpringBallEquipmentJumpCommand()
    {
        Suite(nameof(VerifySpringBallEquipmentJump), () => VerifySpringBallEquipmentJump());
        return 0;
    }
}
