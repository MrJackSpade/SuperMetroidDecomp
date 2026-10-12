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

internal static class BabyMetroidFatalBlowShakeVerification
{
    // #1269: each fatal-blow call runs ShakeBabyMetroidCutscene ($A9:CEDB), which adds $FFFF
    // to the Baby's Y velocity and places it at the saved origin plus the undoubled shaking
    // offsets. The port doubled the offsets and left the velocity at zero, so in the 100%
    // movie the Baby's Y fraction stayed $C700 where native drifted to $C600.
    internal static void VerifyBabyMetroidFatalBlowShake()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 99, Position = FixedPosition<RoomPixels>.FromWhole(new(0x0080, 0x0080)) };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(previousOccupant: FixedPosition<RoomPixels>.Zero);
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.FinalCharge);
        Set(nameof(baby.Position), FixedPosition<RoomPixels>.FromWhole(new(0x00c8, 0x0057)).WithFractionsOf(baby.Position));
        Set(nameof(baby.Health), (ushort)0);
        var head = new Position16<RoomPixels>(0x00c8, 0x0077);

        // Frame-counter index 0: zero offsets around the origin; velocity zeroed then decremented.
        baby.Step(bus, samus, motherBrain, head, enemyFrameCounter: 0);
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, baby.Phase, "zero health starts the fatal blow");
        AssertEqual((ushort)0xffff, baby.Velocity.Y.RawValue, "the first shake call decrements the cleared Y velocity");

        // Index 1: offsets (-1,+1), undoubled.
        baby.Step(bus, samus, motherBrain, head, enemyFrameCounter: 2);
        AssertEqual((ushort)0xfffe, baby.Velocity.Y.RawValue, "every shake call decrements the Y velocity");
        AssertEqual((ushort)0x00c7, baby.Position.X.Whole, "the shake adds the undoubled X offset");
        Console.WriteLine("Baby Metroid fatal-blow shake: $A9:CEDB decrements Y velocity and adds undoubled offsets.");
    }

    [VerificationCommand("--baby-metroid-fatal-blow-shake")]
    internal static int BabyMetroidFatalBlowShakeCommand()
    {
        VerifyBabyMetroidFatalBlowShake();
        return 0;
    }
}
