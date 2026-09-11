using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;

/// <summary>Checks actual cinematic actor births against the cartridge spawner's waits.</summary>
internal static class CeresExplosionTimingAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var scene = new CeresDestructionCinematicState(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var actorsField = typeof(CeresDestructionCinematicState).GetField("actors", flags)!;
        var clockField = typeof(CeresDestructionCinematicState).GetField("explosionSpawnerFrame", flags)!;
        var actors = (List<IntroDiscoverySprite>)actorsField.GetValue(scene)!;
        var seen = new HashSet<IntroDiscoverySprite>(actors);
        var actual = new List<int>();
        int departureFrame = int.MaxValue;
        int Word(int address) => bus.ReadByte(address) | bus.ReadByte(address + 1) << 8;
        int first = Word(CeresExplosionAuditRomData.InitialWait);
        int second = first + Word(CeresExplosionAuditRomData.InterGroupWait) + 1;
        int final = second - 1 + Word(CeresExplosionAuditRomData.RepeatingWait);
        int period = Word(CeresExplosionAuditRomData.RepeatingPeriodOperand);
        Directory.CreateDirectory(outputDirectory);
        for (int tick = 0; tick < final; tick++)
        {
            scene.Step();
            int frame = (int)clockField.GetValue(scene)!;
            bool departedNow = departureFrame == int.MaxValue &&
                scene.Phase == CeresDestructionPhase.FlyingAwayFromExplosion;
            if (departedNow) departureFrame = frame;
            foreach (var actor in actors)
                // Departure adds a separate final-explosion actor, not a spawner child.
                if (seen.Add(actor) && !departedNow) actual.Add(frame);
            if (frame == first || frame == first + 1 || frame == second || frame == final)
                PngWriter.WriteRgba(Path.Combine(outputDirectory, $"frame-{frame:D4}.png"),
                    FrontendFrame.Width, FrontendFrame.Height, scene.Render());
        }
        var expected = Enumerable.Repeat(first, 5).ToList();
        for (int frame = second; frame < Math.Min(final, departureFrame); frame += period) expected.Add(frame);
        expected.AddRange(Enumerable.Repeat(final, 4));
        Console.WriteLine($"Actual explosion births: {string.Join(",", actual)}");
        Console.WriteLine($"ROM wait schedule:      {string.Join(",", expected)}");
        if (!actual.SequenceEqual(expected))
            throw new InvalidDataException("Ceres explosion groups overlap outside the cartridge spawner schedule.");
        return 0;
    }
}

/// <summary>Independent ROM operands used by the explosion timing regression.</summary>
internal static class CeresExplosionAuditRomData
{
    /// <summary>$8B:CE35, Ceres explosion spawner's initial invisible frame duration.</summary>
    public const int InitialWait = 0x8bce35;
    /// <summary>$8B:CE3B, wait after spawning the five initial explosions.</summary>
    public const int InterGroupWait = 0x8bce3b;
    /// <summary>$8B:CE43, lifetime of the installed repeating-explosion pre-instruction.</summary>
    public const int RepeatingWait = 0x8bce43;
    /// <summary>$8B:C4A9, immediate timer reload operand in the repeating pre-instruction.</summary>
    public const int RepeatingPeriodOperand = 0x8bc4a9;
}
