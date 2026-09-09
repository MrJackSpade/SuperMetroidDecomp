using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyEndingLogo(ISnesAddressSpace bus)
    {
        var cgram = new SnesCgram();
        int landings = 0;
        var logo = new EndingLogo(bus, cgram, () => landings++);
        int frame = 0, fadeStart = 0;
        var poses = new HashSet<string>();
        while (!logo.Completed && frame < 300)
        {
            logo.Step(cgram); frame++;
            if (logo.CrossfadeStarted && fadeStart == 0) fadeStart = frame;
            if (logo.PaletteStep > 0)
                for (int p = 0; p < 2; p++)
                {
                    int pointer = RomDataReader.ReadWordFixedBank(bus, 0x8be5e7 + (logo.PaletteStep - 1) * 4 + p * 2);
                    for (int i = 0; i < 16; i++)
                        AssertEqual(RomDataReader.ReadWordFixedBank(bus, 0x8c0000 | (pointer - 30 + i * 2)),
                            cgram.Colors[(p == 0 ? 16 : 240) + i], "native logo crossfade palette table entry");
                }
            poses.Add(Convert.ToHexString(logo.Draw().LowTable));
        }
        AssertTrue(logo.Completed, "logo actors reach the final palette handoff");
        AssertEqual(171, fadeStart, "native circle list waits 96+5+5+64 frames before grey-out instruction");
        AssertEqual(187, frame, "logo palette handoff follows sixteen function calls");
        AssertEqual(1, landings, "upper logo half spawns its palette FX exactly once on landing");
        AssertTrue(poses.Count >= 10, "logo approach and circle animation produce changing OAM positions/maps");
        Console.WriteLine($"  Logo: {fadeStart} actor frames, sixteen exact palette pairs, {poses.Count} OAM poses.");
    }
}
