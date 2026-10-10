using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    /// <summary>Verifies phase-8 and phase-10 speed-palette overrun colors, timing, and recovery against the captured native CPU trace.</summary>
    private static void VerifySpeedPaletteOverrun()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(rom);
        var colors = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorExtractor.Extract(rom)));
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        string tracePath = Path.Combine(SuperMetroid.SourceAudit.ProductionMagicNumberAudit.FindRepositoryRoot(),
            "csharp", "test-fixtures", "issue-1254-speed-palette", "native.csv");
        string[] nativeTrace = File.ReadAllLines(tracePath);
        AssertEqual(3, nativeTrace.Length, "bounded native CPU palette trace contains both overrun cases");
        AssertEqual("phase,nextPhase,timer,colors", nativeTrace[0], "native trace schema");
        int traceRow = 1;
        foreach (ushort phase in new ushort[] { 8, 10 })
        {
            var speed = new SamusHorizontalSpeedState { SpeedBoostCounter = 0x0400, SpecialPaletteTimer = 1 };
            var cgram = new SnesCgram();
            ushort equipped = (ushort)(SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.SpeedBooster);
            for (int call = 0; call < phase / 2; call++)
                speed.UpdateSpeedBoosterPalette(memory, cgram, SamusMovementType.SpinJumping, 1, equipped, cycleColors: colors);
            AssertEqual(phase, speed.SpecialPaletteFrame, "Screw Attack leaves its shared phase for the next movement handler");
            AssertTrue(speed.UpdateSpeedBoosterPalette(memory, cgram, SamusMovementType.NormalJumping, 0, equipped, cycleColors: colors),
                "leaving Screw Attack while boosted copies the native overrun palette without a crash");
            ushort pointer = Read(0x910000 | (Read(0x91daad) + phase));
            for (int color = 0; color < 16; color++)
            {
                // $91:DD64..DDD0 uses LDA $0000..001E,X. At the $68AD expansion
                // pointer each high operand byte drives zero onto the undriven bus.
                ushort expected = pointer == 0x68ad ? (ushort)0 : (ushort)(Read(0x9b0000 | (pointer + color * 2)) & 0x7fff);
                AssertEqual(expected, cgram.Colors[192 + color], $"phase {phase} native overrun color {color}");
            }
            AssertEqual((ushort)6, speed.SpecialPaletteFrame, "native clamps the phase only after the overrun read");
            AssertEqual((ushort)4, speed.SpecialPaletteTimer, "native reloads the four-frame timer");
            string actualTrace = $"{phase},{speed.SpecialPaletteFrame},{speed.SpecialPaletteTimer}," +
                string.Concat(cgram.Colors.Slice(192, 16).ToArray().Select(color => color.ToString("X4")));
            AssertEqual(nativeTrace[traceRow++], actualTrace, "port colors/phase/timer match execution of original ROM on the 65816 core");
            var first = cgram.Colors.ToArray();
            for (int call = 0; call < 3; call++)
            {
                AssertTrue(!speed.UpdateSpeedBoosterPalette(memory, cgram, SamusMovementType.NormalJumping, 0, equipped, cycleColors: colors), "overrun palette remains until timer expires");
                AssertTrue(first.AsSpan().SequenceEqual(cgram.Colors), "palette holds for four frames");
            }
            speed.UpdateSpeedBoosterPalette(memory, cgram, SamusMovementType.NormalJumping, 0, equipped, cycleColors: colors);
            for (int color = 0; color < 16; color++)
                AssertEqual((ushort)(Read(0x9b9f80 + color * 2) & 0x7fff), cgram.Colors[192 + color], "next tick restores native final Gravity boost shade");
        }
        Console.WriteLine("Speed palette overrun: Screw Attack phases 8/10, exact native colors, four-frame hold and next-tick recovery pass without gameplay ROM access.");
        ushort Read(int address) => RomDataReader.ReadWordFixedBank(cart, address);
    }
}
