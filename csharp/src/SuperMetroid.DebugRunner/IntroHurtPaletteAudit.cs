using System.Reflection;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

/// <summary>Checks the real Rinka hit restores the intro palette, not a gameplay suit.</summary>
internal static class IntroHurtPaletteAudit
{
    public static int Run(string romPath, string outputDirectory)
    {
        var bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        var intro = new IntroCinematicState(bus);
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var samusField = typeof(IntroCinematicState).GetField("flashbackSamus", flags)!;
        var cgramField = typeof(IntroCinematicState).GetField("cgram", flags)!;
        for (int frame = 0; frame < 8192 && intro.Phase != IntroCinematicPhase.PageOneAwaitingInput; frame++)
            intro.Step(0);
        if (intro.Phase != IntroCinematicPhase.PageOneAwaitingInput)
            throw new InvalidDataException("Intro did not reach its first page.");
        intro.Step((ushort)SnesButton.A);
        int restores = 0;
        for (int frame = 0; frame < 800; frame++)
        {
            var samus = (SamusState?)samusField.GetValue(intro);
            ushort before = samus?.HurtFlashCounter ?? 0;
            intro.Step(0);
            if (before is not (2 or 4 or 6)) continue;
            Directory.CreateDirectory(outputDirectory);
            PngWriter.WriteRgba(Path.Combine(outputDirectory, $"restore-{before}.png"), 256, 224, intro.Render());
            var cgram = (SnesCgram)cgramField.GetValue(intro)!;
            for (int color = 0; color < SamusPaletteRomData.Common.ColorsPerObjPalette; color++)
            {
                int address = SamusPaletteRomData.HurtFlash.IntroColors + color * 2;
                ushort expected = (ushort)(bus.ReadByte(address) | bus.ReadByte(address + 1) << 8);
                ushort actual = cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + color];
                if (actual != expected)
                    throw new InvalidDataException($"Intro hurt restore {before}, color {color}: ${actual:X4}, expected ${expected:X4}.");
            }
            restores++;
        }
        if (restores != 3) throw new InvalidDataException($"Expected three intro restores, observed {restores}.");
        Console.WriteLine("Real intro Rinka hit: all three restores match all sixteen cartridge intro colors.");
        return 0;
    }
}
