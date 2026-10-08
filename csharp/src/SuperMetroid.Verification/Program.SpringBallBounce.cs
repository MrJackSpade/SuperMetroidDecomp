using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: Spring Ball's in-air handler ($90:A6F1) runs the morphed bouncing routine
    // ($90:91D1) while MorphBallBounceState is nonzero, not the powered jump. The jump's
    // released-button cutoff therefore cannot cancel a rebound: in the 100% movie the
    // bounce's 1.0000 upward speed carries Samus up a pixel before gravity reduces it.
    private static void VerifySpringBallBounce()
    {
        var bus = CartridgeImportAddressSpace.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var room = CreateRoom(16, 32, new ushort[16 * 32], new byte[16 * 32]);
        foreach (ushort bounceState in new ushort[] { 0x0601, 0 })
        {
            var samus = new SamusState
            {
                Pose = SamusPoseIds.SpringBallJumpRightPose,
                XPosition = 0x80,
                YPosition = 0x169,
                MorphBallBounceState = bounceState,
            };
            samus.RefreshCollisionRadii(bus);
            samus.Kinematics.YSubposition = 0xffff;
            samus.Kinematics.YDirection = 1;
            samus.Kinematics.YSpeed = 1;
            samus.Kinematics.YSubspeed = 0;
            samus.Kinematics.YSubacceleration = 0x1c00;

            SamusMorphBallMovement.StepSpringBallInAir(bus, room, samus, (ushort)SuperMetroid.Core.Input.SnesButton.Right, 0);
            string context = $"Spring Ball in air with bounce state ${bounceState:X4}";
            if (bounceState != 0)
            {
                AssertEqual((ushort)0x168, samus.YPosition, $"{context} rises by the rebound speed");
                AssertEqual((ushort)0xffff, samus.Kinematics.YSubposition, $"{context} Y fraction");
                AssertEqual((ushort)1, samus.Kinematics.YDirection, $"{context} keeps rising");
                AssertEqual((ushort)0, samus.Kinematics.YSpeed, $"{context} Y speed");
                AssertEqual((ushort)0xe400, samus.Kinematics.YSubspeed, $"{context} Y subspeed after gravity");
            }
            else
            {
                AssertEqual((ushort)2, samus.Kinematics.YDirection, $"{context} released Jump cuts the rise");
            }
        }
        Console.WriteLine("Spring Ball bounce: a rebound in the air uses the bouncing routine, not the jump cutoff.");
    }
}
