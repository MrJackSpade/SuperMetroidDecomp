using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyReportedShaftMomentum()
    {
        // #1158, recording inputs 5842..5849: neutral controller, left spin pose,
        // mode two, base speed 1.6000. $90:906C retains horizontal movement when
        // the native speed calculation returns carry even without forward input.
        var memory = new SuperMetroidAddressSpace();
        var level = new RoomLevelData(32, 32, new ushort[32 * 32],
            new byte[32 * 32], new ushort[32 * 32], new byte[0x400 * 8]);
        var samus = new SamusState
        {
            Pose = SamusPoseIds.SpinJumpLeftPose,
            XPosition = 128,
            YPosition = 128,
        };
        samus.Kinematics.XRadius = 5;
        samus.Kinematics.YRadius = 12;
        samus.Kinematics.YDirection = 2;
        samus.Kinematics.YSpeed = 2;
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x6000;
        samus.HorizontalSpeed.AccelerationMode = 2;
        for (ushort frame = 0; frame < 8; frame++)
        {
            var result = SamusAerialMovement.StepSpinJump(memory, level, samus, 0, frame);
            AssertEqual(-0x00016000, result.Horizontal.AcceptedDisplacement,
                $"recorded neutral spin carry at frame {frame} matches cartridge carry-set branch");
            AssertEqual((ushort)1, samus.HorizontalSpeed.BaseSpeed, "retained spin whole speed");
            AssertEqual((ushort)0x6000, samus.HorizontalSpeed.BaseSubspeed, "retained spin fractional speed");
        }
        AssertEqual((ushort)117, samus.XPosition, "eight recorded neutral frames carry eleven pixels left");
        Console.WriteLine("#1158: recorded no-Left drift matches native spin momentum; no movement alteration required.");
    }
}
