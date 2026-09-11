using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

internal static partial class Program
{
    private static void VerifyCrystalWindowNative(string rom, string path)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(rom);
        AssertEqual("282BFF5EBE3C4EB99ECC2CCD0214A9FB544FB0E18A437FD01065909BED57A9C4",
            Convert.ToHexString(System.Security.Cryptography.SHA256.HashData(File.ReadAllBytes(path))),
            "accepted native window and ordinary afterglow trace");
        var effect = new SamusPowerBombExplosionState();
        effect.BeginCrystalFlash(128, 112);
        using var trace = File.OpenText(path);
        AssertEqual("frame,phase,radius,speed,r,g,b,left192,right192", trace.ReadLine(), "native Crystal Flash window schema");
        int frames = 0;
        while (trace.ReadLine() is { } line)
        {
            string[] fields = line.Split(',');
            effect.StepFrame(bus);
            int phase = effect.Phase == PowerBombExplosionPhase.Inactive ? 0 :
                effect.Phase == PowerBombExplosionPhase.CrystalFlashExplosion ? 1 : 2;
            AssertEqual(int.Parse(fields[0]), ++frames, "contiguous native window frames");
            AssertEqual(int.Parse(fields[1]), phase, "native window phase");
            AssertEqual(Hex(fields[2]), effect.ExplosionRadius, "native window radius");
            if (phase != 0) AssertEqual(Hex(fields[3]), effect.RadiusSpeed, "native window radius speed");
            AssertEqual(Hex(fields[4]), effect.FixedColorRed, "native window red");
            AssertEqual(Hex(fields[5]), effect.FixedColorGreen, "native window green");
            AssertEqual(Hex(fields[6]), effect.FixedColorBlue, "native window blue");
            var window = SnesGameplayFrameRenderer.CapturePowerBombColorMath(bus, effect, 0, 0);
            if (phase == 0) { AssertTrue(window is null, "native cleanup removes window"); break; }
            byte[] left = Convert.FromHexString(fields[7]), right = Convert.FromHexString(fields[8]);
            for (int y = 32; y < 224; y++)
            {
                int distance = Math.Abs(y - 112);
                var actual = window!.Windows[y];
                // The packet uses 1/0 for an empty interval; hardware uses FF/00.
                // Both reject every pixel. Compare membership, not sentinel spelling.
                if (left[distance] > right[distance])
                {
                    AssertTrue(actual.Left > actual.Right, "native empty window row");
                    continue;
                }
                AssertEqual(left[distance], actual.Left, $"native left profile frame {frames}, row {y}");
                AssertEqual(right[distance], actual.Right, $"native right profile frame {frames}, row {y}");
            }
        }
        AssertTrue(effect.Phase == PowerBombExplosionPhase.Inactive, "native trace reaches cleanup");
        AssertEqual("ordinary-frame,wake,r,g,b", trace.ReadLine(), "ordinary afterglow control schema");
        effect.Spawn(128, 112);
        int setupFrames = 0;
        while (effect.Phase != PowerBombExplosionPhase.Afterglow)
        {
            effect.StepFrame(bus);
            if (++setupFrames > 500) throw new InvalidDataException("Ordinary Power Bomb failed to reach afterglow.");
        }
        // Match the native control's constructed COLDATA input. Saturated channels
        // make every fade observable; this is not a claim about preceding stock colors.
        foreach (string property in new[] { nameof(effect.FixedColorRed), nameof(effect.FixedColorGreen), nameof(effect.FixedColorBlue) })
            typeof(SamusPowerBombExplosionState).GetProperty(property)!.SetValue(effect, (byte)31);
        for (int frame = 1; frame <= 32; frame++)
        {
            string[] fields = trace.ReadLine()!.Split(',');
            bool cleanedUp = effect.StepFrame(bus);
            AssertEqual(frame, int.Parse(fields[0]), "native ordinary afterglow frame");
            AssertEqual(int.Parse(fields[1]) != 0, cleanedUp, "native ordinary afterglow wake");
            AssertEqual(Hex(fields[2]), effect.FixedColorRed, "native ordinary afterglow red");
            AssertEqual(Hex(fields[3]), effect.FixedColorGreen, "native ordinary afterglow green");
            AssertEqual(Hex(fields[4]), effect.FixedColorBlue, "native ordinary afterglow blue");
        }
        AssertTrue(trace.ReadLine() is null, "native window trace fully consumed");
        Console.WriteLine($"Crystal Flash window: {frames} native radius/color/profile frames match.");
        Console.WriteLine("Ordinary Power Bomb afterglow: 32 native color/wake frames match.");
        static int Hex(string text) => Convert.ToUInt16(text, 16);
    }
}
