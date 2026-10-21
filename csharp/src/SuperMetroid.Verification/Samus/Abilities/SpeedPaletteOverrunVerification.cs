using SuperMetroid.Core;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Samus.Abilities;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
namespace SuperMetroid.Verification.Samus.Abilities;
using SuperMetroid.Verification.Harness;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Progression;

/// <summary>Reproduces Speed Booster palette table overrun after Screw Attack phase handoff.</summary>
internal static class SpeedPaletteOverrunVerification
{
    /// <summary>Checks phases eight and ten against a 65816 trace, including four-frame hold and recovery.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    internal static void VerifySpeedPaletteOverrun()
    {
        var rom = CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        var cart = CartridgeImportSource.Require(rom);
        var colors = SamusFullBodyCycleColorCatalog.Load(new MemoryStream(SamusFullBodyCycleColorExtractor.Extract(rom)));
        var memory = SuperMetroidAddressSpace.CreateWithoutCartridge();
        string[] nativeTrace = File.ReadAllLines("csharp/test-fixtures/issue-1254-speed-palette/native.csv");
        AssertEqual(3, nativeTrace.Length, "bounded native CPU palette trace contains both overrun cases");
        AssertEqual("phase,nextPhase,timer,colors", nativeTrace[0], "native trace schema");
        int traceRow = 1;
        foreach (ushort phase in new ushort[] { 8, 10 })
        {
            var speed = new SamusHorizontalSpeedState { SpeedBoostCounter = new SamusSpeedBoostCounter(0x0400), SpecialPaletteTimer = WrappingCountdown.Start(1) };
            var cgram = new SnesCgram();
            ushort equipped = ((SamusEquipmentFlags.GravitySuit | SamusEquipmentFlags.ScrewAttack | SamusEquipmentFlags.SpeedBooster).ToNativeWord());
            for (int call = 0; call < phase / 2; call++)
                speed.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.SpinJumping, AnimationFrame = 1, EquippedItems = (SamusEquipmentFlags)equipped }, cycleColors: colors, overreads: InstalledFixtures.SamusOverreads);
            AssertEqual(phase, speed.SpecialPaletteFrame, "Screw Attack leaves its shared phase for the next movement handler");
            AssertTrue(speed.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = (SamusEquipmentFlags)equipped }, cycleColors: colors, overreads: InstalledFixtures.SamusOverreads),
                "leaving Screw Attack while boosted copies the native overrun palette without a crash");
            ushort pointer = Read(0x910000 | (Read(0x91daad) + phase));
            for (int color = 0; color < 16; color++)
            {
                // $91:DD64..DDD0 uses LDA $0000..001E,X. At the $68AD expansion
                // pointer each high operand byte drives zero onto the undriven bus.
                ushort expected = pointer == 0x68ad ? (ushort)0 : (ushort)(Read(0x9b0000 | (pointer + color * 2)) & 0x7fff);
                AssertEqual(expected, cgram.Colors[192 + color], $"phase {phase} native overrun color {color}");
            }
            AssertEqual(6, speed.SpecialPaletteFrame, "native clamps the phase only after the overrun read");
            AssertEqual((ushort)4, speed.SpecialPaletteTimer.RawValue, "native reloads the four-frame timer");
            string actualTrace = $"{phase},{speed.SpecialPaletteFrame},{speed.SpecialPaletteTimer.RawValue}," +
                string.Concat(cgram.Colors.Slice(192, 16).SelectToArray(color => color.ToWord().ToString("X4")));
            AssertEqual(nativeTrace[traceRow++], actualTrace, "port colors/phase/timer match execution of original ROM on the 65816 core");
            var first = cgram.Colors.ToArray();
            for (int call = 0; call < 3; call++)
            {
                AssertTrue(!speed.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = (SamusEquipmentFlags)equipped }, cycleColors: colors, overreads: InstalledFixtures.SamusOverreads), "overrun palette remains until timer expires");
                AssertTrue(first.SequenceEqual(cgram.Colors), "palette holds for four frames");
            }
            speed.UpdateSpeedBoosterPalette(cgram, new SpeedBoosterPaletteFrame { MovementType = SamusMovementType.NormalJumping, AnimationFrame = 0, EquippedItems = (SamusEquipmentFlags)equipped }, cycleColors: colors, overreads: InstalledFixtures.SamusOverreads);
            for (int color = 0; color < 16; color++)
                AssertEqual((ushort)(Read(0x9b9f80 + color * 2) & 0x7fff), cgram.Colors[192 + color], "next tick restores native final Gravity boost shade");
        }
        Console.WriteLine("Speed palette overrun: Screw Attack phases 8/10, exact native colors, four-frame hold and next-tick recovery pass without gameplay ROM access.");
        ushort Read(int address) => ReadNativeSpeedOverrunPaletteWord(cart, address);
    }

    /// <summary>Reads a fixed-bank retail palette word for the trace comparison.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for Speed Booster overrun palette words")]
    private static ushort ReadNativeSpeedOverrunPaletteWord(IImportCartridgeSource cart, int address) =>
        RomDataReader.ReadWordFixedBank(cart, address);

    /// <summary>Runs the focused Speed Booster palette overrun check.</summary>
    [VerificationCommand("--speed-palette-overrun")]
    internal static int SpeedPaletteOverrunCommand()
    {
        Suite(nameof(VerifySpeedPaletteOverrun), () => VerifySpeedPaletteOverrun());
        return 0;
    }
}
