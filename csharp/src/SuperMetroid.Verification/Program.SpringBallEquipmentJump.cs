using System.Reflection;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifySpringBallEquipmentJump()
    {
        var installation = new GameInstallation(GameAssetInstaller.DesktopRoot);
        foreach (bool left in new[] { false, true })
        {
            var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
            var game = new SuperMetroidGame(memory, new SuperMetroidGameOptions { Invincibility = true }, renderGameplayFrames: false);
            PrepareRomFreeBindings(installation)(game, false);
            game.InitializeDirectRoomVerification();
            var runtime = game.RuntimeForVerification!;
            runtime.LoadCartridgeRoomForDebug(RoomHeaderPointers.WestOcean);
            // A bounded underwater fixture retains the retail FX but removes terrain/enemies,
            // isolating the reported pause/equipment/launch contract from room obstacles.
            foreach (var enemy in runtime.Enemies.Slots) enemy.Clear();
            var level = runtime.LevelData!;
            for (int block = 0; block < level.WidthInBlocks * level.HeightInBlocks; block++)
            { level.SetForegroundEntry(block, 0); level.SetBehavior(block, 0); }
            var samus = runtime.Samus!;
            samus.EquippedItems = (ushort)(SamusEquipmentFlags.MorphBall | SamusEquipmentFlags.HiJumpBoots);
            samus.CollectedItems = (ushort)(samus.EquippedItems | (ushort)SamusEquipmentFlags.SpringBall);
            samus.XPosition = 128;
            samus.YPosition = (ushort)(runtime.RoomLayer3Fx.CurrentYPosition + 96);
            samus.Pose = left ? SamusPoseIds.MorphBallFallingLeftPose : SamusPoseIds.MorphBallFallingRightPose;
            samus.RefreshCollisionRadii(memory);
            runtime.RoomLayer3Fx.ApplyToSamusLiquidPhysics(samus.LiquidPhysics);
            AssertEqual(SamusLiquidPhysicsState.Water, samus.LiquidPhysics.DetermineMovementMedium(samus), "fixture is underwater without Gravity Suit");
            samus.InitializeAnimation(memory, 3);
            samus.Kinematics.YDirection = 1;
            samus.Kinematics.YSpeed = 1;
            samus.Kinematics.YSubspeed = 0x8000;
            for (int jump = 0; jump < 2; jump++)
            {
                Resume(true);
                byte springGround = left ? SamusPoseIds.SpringBallGroundLeftPose : SamusPoseIds.SpringBallGroundRightPose;
                AssertEqual(springGround, samus.Pose, "unpause equips jump-enabled Spring Ball even while rising");
                AssertEqual((ushort)0, samus.AnimationFrame, "cross-family equipment conversion restarts ball animation");
                ushort beforeY = samus.YPosition;
                runtime.Controller1.Latch(0);
                runtime.StepFrame((ushort)SnesButton.A);
                AssertEqual(left ? SamusPoseIds.SpringBallJumpLeftPose : SamusPoseIds.SpringBallJumpRightPose,
                    samus.Pose, "new jump input enters powered Spring Ball jump");
                AssertEqual((ushort)2, samus.Kinematics.YSpeed, "native Hi-Jump water launch whole speed");
                AssertEqual((ushort)0x8000, samus.Kinematics.YSubspeed, "native Hi-Jump water launch fractional speed");
                runtime.StepFrame((ushort)SnesButton.A);
                AssertTrue(samus.YPosition < beforeY, "mid-air equipment jump gains height");
                if (jump == 0)
                {
                    Resume(false);
                    AssertEqual(left ? SamusPoseIds.MorphBallGroundLeftPose : SamusPoseIds.MorphBallGroundRightPose,
                        samus.Pose, "disabling Spring Ball restores ordinary ball without cancelling ascent");
                }
            }

            void Resume(bool equipped)
            {
                ushort y = samus.YPosition, speed = samus.Kinematics.YSpeed, sub = samus.Kinematics.YSubspeed;
                ushort direction = samus.Kinematics.YDirection;
                if (equipped) samus.EquippedItems |= (ushort)SamusEquipmentFlags.SpringBall;
                else samus.EquippedItems &= unchecked((ushort)~(ushort)SamusEquipmentFlags.SpringBall);
                // The equipment screen writes this live word. Exercise its real forced-blank
                // teardown, where cartridge command $0C must reconcile the pose.
                typeof(SuperMetroidGame).GetProperty("GameState")!.SetValue(game, SuperMetroidGameState.UnpausingB);
                game.Step(0);
                AssertEqual(SuperMetroidGameState.Unpausing, game.GameState, "actual frontend teardown completes");
                AssertEqual(y, samus.YPosition, "equipment switch does not move Samus");
                AssertEqual(speed, samus.Kinematics.YSpeed, "equipment switch preserves vertical whole speed");
                AssertEqual(sub, samus.Kinematics.YSubspeed, "equipment switch preserves vertical fractional speed");
                AssertEqual(direction, samus.Kinematics.YDirection, "equipment switch preserves ascent");
            }
        }
        Console.WriteLine("Spring Ball equipment jump: real unpause preserves ascent, enables two airborne launches with native water speeds, and handles both facings.");
    }
}
