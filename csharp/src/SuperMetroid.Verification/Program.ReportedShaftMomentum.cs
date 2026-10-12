using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using static SuiteTimingVerification;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Spatial;

internal static class ReportedShaftMomentumVerification
{
    internal static void VerifyReportedShaftMomentum()
    {
        // #1158, recording inputs 5842..5849: neutral controller, left spin pose,
        // mode two, base speed 1.6000. $90:906C retains horizontal movement when
        // the native speed calculation returns carry even without forward input.
        var memory = new SuperMetroidAddressSpace();
        var level = new RoomLevelData(new Extent16<RoomBlocks>(32, 32), new RoomLevelPlanes(new ushort[32 * 32], new byte[32 * 32], new ushort[32 * 32]), new byte[0x400 * 8]);
        var samus = new SamusState
        {
            Pose = SamusPoseId.SpinJumpLeftPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(128, 128)),
        };
        samus.Kinematics.Radius = new(5, 12);
        samus.Kinematics.YDirection = SamusVerticalDirection.Down;
        samus.Kinematics.YSpeed = 2;
        samus.HorizontalSpeed.BaseSpeed = 1;
        samus.HorizontalSpeed.BaseSubspeed = 0x6000;
        samus.HorizontalSpeed.AccelerationMode = 2;
        for (ushort frame = 0; frame < 8; frame++)
        {
            // Whole and fractional X as one 16.16 position, so the accepted displacement
            // is the exact difference across the movement step.
            int xBefore = SnesSixteenSixteen.FromWhole(samus.Position.X.Whole) | samus.Kinematics.Position.X.Fraction;
            SamusAerialMovement.StepSpinJump(memory, level, samus, 0, frame);
            int xAfter = SnesSixteenSixteen.FromWhole(samus.Position.X.Whole) | samus.Kinematics.Position.X.Fraction;
            AssertEqual(-0x00016000, xAfter - xBefore,
                $"recorded neutral spin carry at frame {frame} matches cartridge carry-set branch");
            AssertEqual((ushort)1, samus.HorizontalSpeed.BaseSpeed, "retained spin whole speed");
            AssertEqual((ushort)0x6000, samus.HorizontalSpeed.BaseSubspeed, "retained spin fractional speed");
        }
        AssertEqual((ushort)117, samus.Position.X.Whole, "eight recorded neutral frames carry eleven pixels left");
        Console.WriteLine("#1158: recorded no-Left drift matches native spin momentum; no movement alteration required.");
    }

    [VerificationCommand("--reported-shaft-momentum")]
    internal static int ReportedShaftMomentumCommand()
    {
        Suite(nameof(VerifyReportedShaftMomentum), () => VerifyReportedShaftMomentum());
        return 0;
    }
}
