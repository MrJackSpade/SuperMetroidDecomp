using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using static SuperMetroid.Verification.Harness.RetailRuntimeFixtureVerification;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms;

/// <summary>Checks that a directionless jump skips horizontal movement and its slope alignment.</summary>
internal static class JumpNoXMovementVerification
{
    /// <summary>
    /// Reproduces issue #1269 after the Red Fish ceiling hit: with no held direction and
    /// acceleration mode zero, jumping skips $90:902B and preserves Samus's Y position
    /// instead of aligning her two pixels downward to a slope.
    /// </summary>
    internal static void VerifyJumpNoXMovement()
    {
        var runtime = CreateRetailRuntimeFixture();
        runtime.HudSetup.InitializeHud(HudSnapshot.CeresDebug);
        runtime.RoomLoader.InitializeStartingCeresRoom();
        runtime.RoomLoader.InitializeCeresStartSamus();
        runtime.LoadCartridgeRoomForDebug(RoomHeaderDefinitions.RedFish);
        var samus = runtime.Player.IntroducedSamus;
        samus.Pose = SamusPoseId.NormalJumpAimDownLeftPose;
        samus.Position = samus.Position.WithWhole(new(0x0225, 0x004a));
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        samus.Kinematics.Position = samus.Kinematics.Position.WithFractionX(0).WithFractionY(0);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.Kinematics.YSpeed = default;
        samus.Kinematics.YAcceleration = samus.Kinematics.YAcceleration.WithFraction(0x1c00);
        samus.HorizontalSpeed.AccelerationMode = SamusAccelerationMode.Accelerating;

        SamusAerialMovement.StepNormalJump(runtime.Room.LoadedLevelData, samus,
            (SnesButton.A | SnesButton.B), nmiFrameCounter: 0);
        AssertEqual((ushort)0x004a, samus.Position.Y.Whole, "no horizontal move means no slope alignment");
        AssertEqual((ushort)0x0225, samus.Position.X.Whole, "X is unchanged");
        Console.WriteLine("Jump without X movement: Samus_Jumping_Movement skips the horizontal mover and its slope alignment.");
    }

    /// <summary>Runs the focused Red Fish jump-without-horizontal-motion check.</summary>
    [VerificationCommand("--jump-no-x-movement")]
    internal static int JumpNoXMovementCommand()
    {
        VerifyJumpNoXMovement();
        return 0;
    }
}
