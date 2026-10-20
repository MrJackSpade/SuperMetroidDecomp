using SuperMetroid.Core.Samus.Drawing;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Enemies.Elevator;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.Rooms.Doors;

/// <summary>Reproduces same-frame elevator contact when Samus stands from a crouch.</summary>
internal static class ElevatorStandUpContactVerification
{
    /// <summary>
    /// <c>$91:F404</c> installs the new pose before <c>$91:FDAE</c> probes the larger body, so
    /// an elevator pseudo-door met by those probes sees the new pose against
    /// <c>$94:938B</c>'s below-$09 gate. In the 13% movie Samus stands up ($28 to $02) on
    /// Kraid's lair elevator; native arms it in that frame and Down boards it next frame.
    /// </summary>
    internal static void VerifyElevatorStandUpContact()
    {
        const int Width = 4, Height = 6, DoorRow = 4;
        var blocks = new ushort[Width * Height];
        var behaviors = new byte[Width * Height];
        for (int x = 0; x < Width; x++)
        {
            // Tourian's elevator list: BTS 2 is the solid pseudo-door that arms the actor.
            blocks[DoorRow * Width + x] = 0x9000;
            behaviors[DoorRow * Width + x] = 2;
        }
        var level = new RoomLevelData(new Extent16<RoomBlocks>(Width, Height), new RoomLevelPlanes(blocks, behaviors, new ushort[Width * Height]), [], doors: RoomDoorListDefinitions.ForListPointer(0xdad5));

        var samus = new SamusState { Pose = SamusPoseId.CrouchingLeftPose, Position = FixedPosition<RoomPixels>.FromWhole(new(24, 0)) };
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        SamusPresentation.InitializeAnimation(samus);
        // Feet on the elevator's top surface; the standing body grows five pixels upward.
        samus.Position = samus.Position.WithWholeY((ushort)(DoorRow * 16 - samus.Kinematics.Radius.Y));
        AssertTrue(level.ConsumeElevatorDoorContact() is false, "no contact before the stand-up");

        AssertTrue(SamusPosturePoseTransitions.TryApplyDirectCrouchToStandingTransition(samus, level, SamusPoseId.FacingLeftNormalPose, 0),
            "the standing body fits above the elevator");
        AssertEqual(SamusPoseId.FacingLeftNormalPose, samus.Pose, "Samus stands");
        AssertTrue(level.ConsumeElevatorDoorContact(),
            "the probe for the new standing pose ($02 < $09) arms the elevator");
        Console.WriteLine("  Elevator stand-up contact: standing up on the elevator arms it in the same frame.");
    }

    /// <summary>Runs the standing-pose elevator contact check from the verification host.</summary>
    [VerificationCommand("--elevator-stand-up-contact")]
    internal static int ElevatorStandUpContactCommand()
    {
        Suite(nameof(VerifyElevatorStandUpContact), () => VerifyElevatorStandUpContact());
        return 0;
    }
}
