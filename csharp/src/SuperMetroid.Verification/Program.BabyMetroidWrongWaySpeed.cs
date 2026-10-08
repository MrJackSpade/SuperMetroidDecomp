using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Game;

internal static partial class Program
{
    // #1269: the Baby's stare-down, fly-off and final-charge-staging legs call
    // $A9:F466, which stores a $10 wrong-way off-screen X extra before the accelerator.
    // The port passed zero, so in the 100% movie the off-screen Baby reversing toward X
    // $131 slowed by $0A instead of $1A ($015F -> $0145 natively).
    private static void VerifyBabyMetroidWrongWaySpeed()
    {
        var bus = CartridgeImportAddressSpaceTooling.LoadRetailRom(Path.GetFullPath("Super Metroid.smc"));
        var samus = new SamusState { Health = 99, XPosition = 0x0080, YPosition = 0x0080 };
        var motherBrain = new MotherBrainRainbowBeamAttackSequence();
        var baby = new BabyMetroidCutsceneState();
        baby.Initialize(inheritedXSubposition: 0x1400, inheritedYSubposition: 0x3100);
        void Set(string name, object value) => typeof(BabyMetroidCutsceneState).GetProperty(name)!.SetValue(baby, value);
        Set(nameof(baby.Phase), BabyMetroidCutscenePhase.MoveToFinalChargeStart);
        Set(nameof(baby.XPosition), (ushort)0x0133);
        Set(nameof(baby.YPosition), (ushort)0x004d);
        Set(nameof(baby.XVelocity), (ushort)0x015f);
        Set(nameof(baby.YVelocity), (ushort)0x012b);

        // Layer-1 X zero puts X $133 beyond the $120-wide vaguely-on-screen band.
        baby.Step(bus, samus, motherBrain, HeadOf(motherBrain), layer1X: 0, layer1Y: 0);
        AssertEqual((ushort)0x0145, baby.XVelocity,
            "the wrong-way off-screen reversal subtracts the $10 extra, $8 and two $1 steps");
        Console.WriteLine("Baby Metroid wrong-way speed: $A9:F466 legs apply the $10 off-screen extra.");
    }
}
