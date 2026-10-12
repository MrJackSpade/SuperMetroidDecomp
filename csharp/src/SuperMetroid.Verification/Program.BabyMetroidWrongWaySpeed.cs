using System.Buffers.Binary;
using System.Runtime.InteropServices;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Runtime;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using static BabyMetroidFixtureHeadVerification;

internal static class BabyMetroidWrongWaySpeedVerification
{
    // #1269: the Baby's stare-down, fly-off and final-charge-staging legs call
    // $A9:F466, which stores a $10 wrong-way off-screen X extra before the accelerator.
    // The port passed zero, so in the 100% movie the off-screen Baby reversing toward X
    // $131 slowed by $0A instead of $1A ($015F -> $0145 natively).
    internal static void VerifyBabyMetroidWrongWaySpeed()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)) };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.FromWords(0, 0x1400, 0, 0x3100));
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.MoveToFinalChargeStart);
        Set(nameof(baby.Position), FixedPosition<RoomPixels>.FromWhole(new(0x0133, 0x004d)).WithFractionsOf(baby.Position));
        Set(nameof(baby.Velocity), new EightEightVector<RoomPixels>(new SnesSignedEightEight(0x015f), new SnesSignedEightEight(0x012b)));

        // Layer-1 X zero puts X $133 beyond the $120-wide vaguely-on-screen band.
        baby.Step(bus, samus, motherBrain, HeadOf(motherBrain), layer1: Position16<RoomPixels>.Zero);
        AssertEqual((ushort)0x0145, baby.Velocity.X.RawValue,
            "the wrong-way off-screen reversal subtracts the $10 extra, $8 and two $1 steps");
        Console.WriteLine("Baby Metroid wrong-way speed: $A9:F466 legs apply the $10 off-screen extra.");
    }

    [VerificationCommand("--baby-metroid-wrong-way-speed")]
    internal static int BabyMetroidWrongWaySpeedCommand()
    {
        VerifyBabyMetroidWrongWaySpeed();
        return 0;
    }
}
