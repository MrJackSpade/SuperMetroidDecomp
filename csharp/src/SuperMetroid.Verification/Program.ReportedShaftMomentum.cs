using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    /// <summary>Verifies the recorded neutral left-spin sequence retains native carry-set momentum and moves Samus eleven pixels over eight frames.</summary>
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
            // Whole and fractional X as one 16.16 position, so the accepted displacement
            // is the exact difference across the movement step.
            int xBefore = samus.XPosition << 16 | samus.Kinematics.XSubposition;
            SamusAerialMovement.StepSpinJump(memory, level, samus, 0, frame);
            int xAfter = samus.XPosition << 16 | samus.Kinematics.XSubposition;
            AssertEqual(-0x00016000, xAfter - xBefore,
                $"recorded neutral spin carry at frame {frame} matches cartridge carry-set branch");
            AssertEqual((ushort)1, samus.HorizontalSpeed.BaseSpeed, "retained spin whole speed");
            AssertEqual((ushort)0x6000, samus.HorizontalSpeed.BaseSubspeed, "retained spin fractional speed");
        }
        AssertEqual((ushort)117, samus.XPosition, "eight recorded neutral frames carry eleven pixels left");
        Console.WriteLine("#1158: recorded no-Left drift matches native spin momentum; no movement alteration required.");
    }
}
