using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Samus.Poses;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;

namespace SuperMetroid.Verification.Samus.Poses;

/// <summary>
/// Reproduces #2516: Up in the Draygon-held ball the Baby's Hyper Beam freeze can install (#1535) chooses the stand-up,
/// whose own setup does nothing, so the pose update only makes room for the larger body and starts the stand-up's
/// animation. The port threw instead.
/// </summary>
internal static class DraygonHoldStandUpVerification
{
    /// <summary>The block column Samus stands in, inside the cleared part of the room.</summary>
    private const int Column = 32;

    /// <summary>The block row of the floor the floor cases rest the ball on.</summary>
    private const int FloorRow = 30;

    /// <summary>
    /// Up stands the ball up in open air without moving it, stands it up off a floor with its feet kept where they were,
    /// and keeps it a ball where a ceiling just above it leaves no room; each stand-up starts its animation.
    /// </summary>
    internal static void VerifyDraygonHoldStandUp()
    {
        SuperMetroidRuntime runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.NewGame);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus(runtime.Room.Loaded);
        runtime.LoadCartridgeRoomForDebug(RoomDefinitions.LandingSite);
        // No enemy's solid body takes part in the stand-up's collision.
        foreach (var enemy in runtime.Enemies.Places.InUpdateOrder)
            enemy.Clear();
        SamusState samus = runtime.Player.Samus;
        samus.Control.Unlock();
        int ballRadius = SamusPoseMetadata.ReadPoseYRadius(SamusPoseId.DraygonHeldBallLeftPose);
        int standUpRadius = SamusPoseMetadata.ReadPoseYRadius(SamusPoseId.DraygonHeldStandingUpLeftPose);
        int floorTop = FloorRow * RoomGridConversions.BlockPixels;

        // Open air: no probe meets anything, so she stands up where she is.
        PressUp(floor: false, ceiling: false, centreY: floorTop - 80);
        AssertStandUp(floorTop - 80, "in open air");

        // On a floor: the larger body moves up off it by the radius difference, so her feet stay on it.
        PressUp(floor: true, ceiling: false, centreY: floorTop - ballRadius);
        AssertStandUp(floorTop - standUpRadius, "on a floor, keeping her feet on it");

        // A ceiling one block above the floor: both probes meet terrain, and a body shorter than an upright one keeps its pose.
        PressUp(floor: true, ceiling: true, centreY: floorTop - ballRadius);
        AssertEqual(SamusPoseId.DraygonHeldBallLeftPose, samus.Pose, "boxed in, Up keeps the Draygon-held ball");
        AssertEqual(floorTop - ballRadius, samus.Position.Y.Whole, "boxed in, the ball does not move");
        Console.WriteLine("Draygon-held stand-up: Up stands the ball up through the larger-pose collision and starts the stand-up's animation.");

        void PressUp(bool floor, bool ceiling, int centreY)
        {
            RoomLevelData level = runtime.Room.LoadedLevelData;
            for (int y = 16; y < 36; y++)
            {
                for (int x = 16; x < 48; x++)
                {
                    RoomCollisionType collision = (floor && y == FloorRow) || (ceiling && y == FloorRow - 2)
                        ? RoomCollisionType.SolidBlock
                        : RoomCollisionType.Air;
                    int index = y * level.Size.X + x;
                    level.SetTerrain(index, RoomTerrainBlock.Of(0, BlockMirroring.None, collision));
                    level.SetBehavior(index, RoomBlockBehavior.None);
                }
            }

            samus.Pose = SamusPoseId.DraygonHeldBallLeftPose;
            samus.Position = samus.Position.WithWhole(new(Column * RoomGridConversions.BlockPixels, (ushort)centreY));
            SamusPoseMetadata.RefreshCollisionRadii(samus);
            SamusPresentation.InitializeAnimation(samus);
            samus.CommitPoseHistory();
            runtime.PlayerActions.Latch(0);
            runtime.StepFrame(ControllerButton.Up);
        }

        void AssertStandUp(int expectedCentreY, string where)
        {
            AssertEqual(SamusPoseId.DraygonHeldStandingUpLeftPose, samus.Pose, $"{where}, Up stands the Draygon-held ball up");
            AssertEqual(expectedCentreY, samus.Position.Y.Whole, $"{where}, the stand-up's collision places her");
            AssertEqual(SamusPoseId.DraygonHeldStandingUpLeftPose, samus.AnimatedPose, $"{where}, the stand-up's animation runs");
            AssertEqual(SamusAnimationDelayDefinitions.DraygonHeldStandingUpLeftAnimation.Start, samus.AnimationFrame,
                $"{where}, the stand-up's animation starts on its first frame");
            if (samus.AnimationStep is not SamusAnimationDelay firstDelay)
                throw new InvalidOperationException($"{where}, the stand-up's first frame is not a delay.");
            AssertEqual(WrappingCountdown.Start(samus.ExtraAnimationDelay).AddFrames(firstDelay.Ticks), samus.AnimationFrameTimer,
                $"{where}, the stand-up's animation timer starts from its first delay");
        }
    }
}
