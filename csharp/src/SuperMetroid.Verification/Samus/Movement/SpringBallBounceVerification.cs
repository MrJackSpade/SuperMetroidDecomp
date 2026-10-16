using SuperMetroid.Core.Input;
using static SuperMetroid.Verification.Harness.VerificationHelpersVerification;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks Spring Ball's in-air rebound path when Jump is released.</summary>
internal static class SpringBallBounceVerification
{
    /// <summary>Confirms $90:A6F1 chooses morphed bounce at $90:91D1 while the bounce state is nonzero, preserving the one-pixel rise before gravity.</summary>
    internal static void VerifySpringBallBounce()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CreateRoom(new(16, 32), new ushort[16 * 32], new byte[16 * 32]);
        foreach (SamusBallBounceState bounceState in new[] { SamusBallBounceState.SpringBallFirstRebound, SamusBallBounceState.None })
        {
            var samus = new SamusState
            {
                Pose = SamusPoseId.SpringBallJumpRightPose,
                Position = FixedPosition<RoomPixels>.FromWhole(new(0x80, 0x169)),
                MorphBallBounceState = bounceState,
            };
            samus.PoseMetadata.RefreshCollisionRadii();
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xffff);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = 1;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YSubacceleration = 0x1c00;

            SamusMorphBallMovement.StepSpringBallInAir(room, samus, SnesButton.Right, 0);
            string context = $"Spring Ball in air with bounce state ${(int)bounceState:X4}";
            if (bounceState != SamusBallBounceState.None)
            {
                AssertEqual((ushort)0x168, samus.Position.Y.Whole, $"{context} rises by the rebound speed");
                AssertEqual((ushort)0xffff, samus.Kinematics.Position.Y.Fraction, $"{context} Y fraction");
                AssertEqual(SamusVerticalDirection.Up, samus.Kinematics.YDirection, $"{context} keeps rising");
                AssertEqual((ushort)0, samus.Kinematics.YSpeed, $"{context} Y speed");
                AssertEqual((ushort)0xe400, samus.Kinematics.YSubspeed, $"{context} Y subspeed after gravity");
            }
            else
            {
                AssertEqual(SamusVerticalDirection.Down, samus.Kinematics.YDirection, $"{context} released Jump cuts the rise");
            }
        }
        Console.WriteLine("Spring Ball bounce: a rebound in the air uses the bouncing routine, not the jump cutoff.");
    }

    /// <summary>Runs the focused 100% movie Spring Ball rebound check.</summary>
    [VerificationCommand("--spring-ball-bounce")]
    internal static int SpringBallBounceCommand()
    {
        VerifySpringBallBounce();
        return 0;
    }
}
