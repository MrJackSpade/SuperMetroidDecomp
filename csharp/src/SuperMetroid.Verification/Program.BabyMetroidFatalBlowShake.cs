using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: each fatal-blow call runs ShakeBabyMetroidCutscene ($A9:CEDB), which adds $FFFF
    // to the Baby's Y velocity and places it at the saved origin plus the undoubled shaking
    // offsets. The port doubled the offsets and left the velocity at zero, so in the 100%
    // movie the Baby's Y fraction stayed $C700 where native drifted to $C600.
    /// <summary>Checks that fatal-blow shake calls decrement Y velocity and apply the native undoubled offsets.</summary>
    private static void VerifyBabyMetroidFatalBlowShake()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 99, XPosition = 0x0080, YPosition = 0x0080 };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(inheritedXSubposition: 0, inheritedYSubposition: 0);
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.FinalCharge);
        Set(nameof(baby.XPosition), (ushort)0x00c8);
        Set(nameof(baby.YPosition), (ushort)0x0057);
        Set(nameof(baby.Health), (ushort)0);
        var head = new MotherBrainHeadPosition(0x00c8, 0x0077);

        // Frame-counter index 0: zero offsets around the origin; velocity zeroed then decremented.
        baby.Step(bus, samus, motherBrain, head, enemyFrameCounter: 0);
        AssertEqual(BabyMetroidCutscenePhase.TakeFinalBlow, baby.Phase, "zero health starts the fatal blow");
        AssertEqual((ushort)0xffff, baby.YVelocity, "the first shake call decrements the cleared Y velocity");

        // Index 1: offsets (-1,+1), undoubled.
        baby.Step(bus, samus, motherBrain, head, enemyFrameCounter: 2);
        AssertEqual((ushort)0xfffe, baby.YVelocity, "every shake call decrements the Y velocity");
        AssertEqual((ushort)0x00c7, baby.XPosition, "the shake adds the undoubled X offset");
        Console.WriteLine("Baby Metroid fatal-blow shake: $A9:CEDB decrements Y velocity and adds undoubled offsets.");
    }
}
