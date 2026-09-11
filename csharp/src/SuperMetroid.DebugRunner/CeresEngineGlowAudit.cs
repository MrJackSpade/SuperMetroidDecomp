using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Desktop;
using System.Reflection;

/// <summary>Checks the visible engine-color cycle throughout the real post-Ceres cinematic.</summary>
internal static class CeresEngineGlowAudit
{
    public static int Run(string romPath, string directory)
    {
        Directory.CreateDirectory(directory);
        var fields = typeof(CeresDestructionCinematicState)
            .GetFields(BindingFlags.Instance | BindingFlags.NonPublic | BindingFlags.Public)
            .OrderBy(field => field.MetadataToken).ToArray();
        var legacy = DebuggerStateFieldMigrations.SelectSerializedFields(
            typeof(CeresDestructionCinematicState), fields, fields.Length - 1);
        if (!legacy.SequenceEqual(fields.Where(field => field.Name != "paletteFx")))
            throw new InvalidDataException("Legacy engine migration changed an existing cinematic field.");
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort Word(int address) => (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
        int program = CeresEngineGlowFixtureData.Bank | Word(CeresEngineGlowFixtureData.Definition + 2);
        int colorIndex = Word(program + 2) / 2;
        ushort bright = Word(program + 6), dark = Word(program + 12);
        if (Word(program + 4) != 1 || Word(program + 10) != 1 || bright == dark)
            throw new InvalidDataException("Engine reference requires the pinned alternating one-frame palette program.");
        var cinematic = new CeresDestructionCinematicState(bus);
        int age = -1, frames = 0, visibleFrames = 0, enginePixels = 0;
        while (!cinematic.Finished && frames++ < 4000)
        {
            cinematic.Step();
            if (cinematic.Phase < CeresDestructionPhase.WaitForZebesMusicQueue) continue;
            age++;
            var packet = cinematic.CaptureRenderSnapshot();
            var actual = SoftwareLayeredSnapshotRenderer.Render(packet);
            if (!actual.AsSpan().SequenceEqual(cinematic.Render()))
                throw new InvalidDataException("Engine capture differs from direct cinematic rendering.");
            Rgba32[] Reference(ushort color)
            {
                ushort[] colors = packet.Memory.Cgram.ToArray();
                colors[colorIndex] = color;
                var memory = new PpuMemorySnapshot(packet.Memory.Vram, colors,
                    packet.Memory.Oam, packet.Memory.ModeledSpriteCount);
                return SoftwareLayeredSnapshotRenderer.Render(new(memory, packet.Layers, packet.ObjectSelection, packet.Brightness));
            }
            var lightImage = Reference(bright);
            var darkImage = Reference(dark);
            int footprint = lightImage.Zip(darkImage).Count(pair => pair.First != pair.Second);
            if (footprint != 0) { visibleFrames++; enginePixels += footprint; }
            if (visibleFrames == 100 && footprint != 0)
            {
                PngWriter.WriteRgba(Path.Combine(directory, "visible-engine-light.png"), 256, 224, lightImage);
                PngWriter.WriteRgba(Path.Combine(directory, "visible-engine-dark.png"), 256, 224, darkImage);
            }
            var expected = (age & 1) == 0 ? lightImage : darkImage;
            if (!actual.AsSpan().SequenceEqual(expected))
            {
                string name = $"frame-{frames}-{cinematic.Phase}";
                PngWriter.WriteRgba(Path.Combine(directory, name + "-actual.png"), 256, 224, actual);
                PngWriter.WriteRgba(Path.Combine(directory, name + "-expected.png"), 256, 224, expected);
                throw new InvalidDataException($"{name}: engine pixels disagree with palette-program age {age}; visible footprint={footprint} pixels.");
            }
            // Exercise both phases of the live interpreter, not merely initial
            // construction. The following frames must retain the same pixel sequence.
            if (visibleFrames is 10 or 11 && footprint != 0)
            {
                using var state = new MemoryStream();
                DebuggerObjectGraphSerializer.Serialize(state, cinematic);
                state.Position = 0;
                cinematic = DebuggerObjectGraphSerializer.Deserialize<CeresDestructionCinematicState>(state);
            }
        }
        if (!cinematic.Finished || visibleFrames < 10)
            throw new InvalidDataException("Engine audit did not cover visible glow through the completed approach.");
        Console.WriteLine($"Engine glow: {age + 1} palette frames, {visibleFrames} visible frames and {enginePixels} engine-pixel observations pass through cinematic completion.");
        return 0;
    }
}

/// <summary>Independent ROM definition for the engine flicker program.</summary>
internal static class CeresEngineGlowFixtureData
{
    /// <summary>PaletteFXObjects_CutsceneGunshipEngineFlicker at $8D:E1A8.</summary>
    public const int Definition = 0x8de1a8;
    /// <summary>Instruction-list bank for the palette-FX interpreter.</summary>
    public const int Bank = 0x8d0000;
}
