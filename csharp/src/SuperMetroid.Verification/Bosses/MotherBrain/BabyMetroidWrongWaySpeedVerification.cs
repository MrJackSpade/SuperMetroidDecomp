using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidFixtureHeadVerification;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Samus;
namespace SuperMetroid.Verification.Bosses.MotherBrain;
using SuperMetroid.Verification.Harness;

/// <summary>Reproduces the Baby Metroid's off-screen reversal from issue #1269.</summary>
internal static class BabyMetroidWrongWaySpeedVerification
{
    /// <summary>
    /// Confirms the $A9:F466 movement legs apply a $10 off-screen wrong-way X extra
    /// before acceleration. At the recorded 100% movie position, native velocity
    /// changes from $015F to $0145 rather than slowing by only $0A.
    /// </summary>
    internal static void VerifyBabyMetroidWrongWaySpeed()
    {
        var samus = new SamusState { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)) };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.FromWords(0, 0x1400, 0, 0x3100));
        baby.Phase = BabyMetroidCutscenePhase.MoveToFinalChargeStart;
        baby.Position = FixedPosition<RoomPixels>.FromWhole(new(0x0133, 0x004d)).WithFractionsOf(baby.Position);
        baby.Velocity = new EightEightVector<RoomPixels>(new SnesSignedEightEight(0x015f), new SnesSignedEightEight(0x012b));

        // Layer-1 X zero puts X $133 beyond the $120-wide vaguely-on-screen band.
        baby.Step(samus, motherBrain, HeadOf(motherBrain), layer1: Position16<RoomPixels>.Zero);
        AssertEqual((ushort)0x0145, baby.Velocity.X.RawValue,
            "the wrong-way off-screen reversal subtracts the $10 extra, $8 and two $1 steps");
        Console.WriteLine("Baby Metroid wrong-way speed: $A9:F466 legs apply the $10 off-screen extra.");
    }

    /// <summary>Runs the targeted off-screen reversal regression from the verification host.</summary>
    [VerificationCommand("--baby-metroid-wrong-way-speed")]
    internal static int BabyMetroidWrongWaySpeedCommand()
    {
        VerifyBabyMetroidWrongWaySpeed();
        return 0;
    }
}
