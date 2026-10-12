using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using static VerificationHelpersVerification;

internal static class SpringBallBounceVerification
{
    // #1269: Spring Ball's in-air handler ($90:A6F1) runs the morphed bouncing routine
    // ($90:91D1) while MorphBallBounceState is nonzero, not the powered jump. The jump's
    // released-button cutoff therefore cannot cancel a rebound: in the 100% movie the
    // bounce's 1.0000 upward speed carries Samus up a pixel before gravity reduces it.
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
            samus.PoseMetadata.RefreshCollisionRadii(bus);
            samus.Kinematics.Position = samus.Kinematics.Position.WithFractionY(0xffff);
            samus.Kinematics.YDirection = SamusVerticalDirection.Up;
            samus.Kinematics.YSpeed = 1;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YSubacceleration = 0x1c00;

            SamusMorphBallMovement.StepSpringBallInAir(bus, room, samus, (ushort)SuperMetroid.Core.Input.SnesButton.Right, 0);
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

    [VerificationCommand("--spring-ball-bounce")]
    internal static int SpringBallBounceCommand()
    {
        VerifySpringBallBounce();
        return 0;
    }
}
