using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Progression;

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
            var game = new SuperMetroidGame(new SuperMetroid.Core.Saves.SaveSlotStore(), new SuperMetroidGameOptions { Invincibility = true }, SuperMetroidGameHostServices.Default with { GameplayFrames = RetainedGameplayFrames.Instance }, new SharedRandom());
            PrepareRomFreeBindings(installation)(game, false);
            game.InitializeDirectRoomVerification();
            var runtime = game.GameplayRuntimeForVerification;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.WestOcean);
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
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            samus.LiquidPhysics.ConfigureFor(runtime.RoomLayer3Fx);
            AssertEqual(SamusLiquidMedium.Water, samus.LiquidPhysics.DetermineMovementMedium(samus), "fixture is underwater without Gravity Suit");
            SamusPresentation.InitializeAnimation(samus, 3);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = new SnesUnsignedSixteenSixteen(1, 0x8000);
            for (int jump = 0; jump < 2; jump++)
            {
                Resume(true);
                SamusPoseId springGround = left ? SamusPoseId.SpringBallGroundLeftPose : SamusPoseId.SpringBallGroundRightPose;
                AssertEqual(springGround, samus.Pose, "unpause equips jump-enabled Spring Ball even while rising");
                AssertEqual(0, samus.AnimationFrame, "cross-family equipment conversion restarts ball animation");
                ushort beforeY = samus.Position.Y.Whole;
                runtime.Controller1.Latch(0);
                runtime.StepFrame(ControllerButton.A);
                AssertEqual(left ? SamusPoseId.SpringBallJumpLeftPose : SamusPoseId.SpringBallJumpRightPose,
                    samus.Pose, "new jump input enters powered Spring Ball jump");
                AssertEqual(2, samus.Kinematics.YSpeed.Whole, "native Hi-Jump water launch whole speed");
                AssertEqual(0x8000, samus.Kinematics.YSpeed.Fraction, "native Hi-Jump water launch fractional speed");
                runtime.StepFrame(ControllerButton.A);
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
                int y = samus.Position.Y.Whole, speed = samus.Kinematics.YSpeed.Whole, sub = samus.Kinematics.YSpeed.Fraction;
                SamusVerticalDirection direction = samus.Kinematics.YDirection;
                if (equipped) samus.EquippedItems |= SamusEquipmentFlags.SpringBall;
                else samus.EquippedItems &= ~SamusEquipmentFlags.SpringBall;
                // The equipment screen writes this live word. Exercise its real forced-blank
                // teardown, where cartridge command $0C must reconcile the pose.
                game.GameState = SuperMetroidGameState.UnpausingB;
                game.Step(0);
                AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "actual frontend teardown completes");
                AssertEqual(y, samus.Position.Y.Whole, "equipment switch does not move Samus");
                AssertEqual(speed, samus.Kinematics.YSpeed.Whole, "equipment switch preserves vertical whole speed");
                AssertEqual(sub, samus.Kinematics.YSpeed.Fraction, "equipment switch preserves vertical fractional speed");
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
