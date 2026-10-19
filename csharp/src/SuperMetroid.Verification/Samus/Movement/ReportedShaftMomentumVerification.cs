using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Poses;
namespace SuperMetroid.Verification.Samus.Movement;
using SuperMetroid.Verification.Harness;

/// <summary>Checks the reported neutral-input shaft drift against native spin-jump carry behavior.</summary>
internal static class ReportedShaftMomentumVerification
{
    /// <summary>Confirms recorded frames 5842..5849 retain 1.6000 leftward speed without held Left input.</summary>
    /// <remarks>#1158: native $90:906C accepts carry from the speed calculation in a left spin pose and mode two, even with a neutral controller.</remarks>
    internal static void VerifyReportedShaftMomentum()
    {
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
            SamusAerialMovement.StepSpinJump(level, samus, 0, frame);
            int xAfter = SnesSixteenSixteen.FromWhole(samus.Position.X.Whole) | samus.Kinematics.Position.X.Fraction;
            AssertEqual(-0x00016000, xAfter - xBefore,
                $"recorded neutral spin carry at frame {frame} matches cartridge carry-set branch");
            AssertEqual((ushort)1, samus.HorizontalSpeed.BaseSpeed, "retained spin whole speed");
            AssertEqual((ushort)0x6000, samus.HorizontalSpeed.BaseSubspeed, "retained spin fractional speed");
        }
        AssertEqual((ushort)117, samus.Position.X.Whole, "eight recorded neutral frames carry eleven pixels left");
        Console.WriteLine("#1158: recorded no-Left drift matches native spin momentum; no movement alteration required.");
    }

    /// <summary>Runs the focused shaft-momentum verification command.</summary>
    [VerificationCommand("--reported-shaft-momentum")]
    internal static int ReportedShaftMomentumCommand()
    {
        Suite(nameof(VerifyReportedShaftMomentum), () => VerifyReportedShaftMomentum());
        return 0;
    }
}
