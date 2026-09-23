using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyPowerBombFixedColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Power Bomb fixed colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
        PowerBombFixedColorCatalog catalog = PowerBombFixedColorCatalog.Load(
            new MemoryStream(PowerBombFixedColorExtractor.Extract(rom)));
        int compared = 0;
        foreach (PowerBombFixedColorSequence sequence in Enum.GetValues<PowerBombFixedColorSequence>())
        {
            for (int index = 0; index < PowerBombFixedColorFormat.Count(sequence); index++)
            {
                int address = PowerBombFixedColorFormat.SourceAddress(sequence) +
                    index * SamusPaletteRomData.PowerBomb.BytesPerColor;
                (byte red, byte green, byte blue) = catalog.Resolve(sequence, index);
                AssertEqual((byte)(rom.ReadByte(address) & SamusPaletteRomData.PowerBomb.ComponentMask),
                    red, $"{sequence} native red {index}");
                AssertEqual((byte)(rom.ReadByte(address + 1) & SamusPaletteRomData.PowerBomb.ComponentMask),
                    green, $"{sequence} native green {index}");
                AssertEqual((byte)(rom.ReadByte(address + 2) & SamusPaletteRomData.PowerBomb.ComponentMask),
                    blue, $"{sequence} native blue {index}");
                compared++;
            }
        }

        VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: false);
        VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: true);
        VerifyCeresExplosionTimeline(catalog);
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt Power Bomb colors fail loudly");
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Power Bomb color field fails loudly");
        Console.WriteLine($"  Power Bomb fixed colors: {compared} native RGB5 triplets and guarded Power Bomb/Crystal Flash lifecycles pass.");
    }

    private static void VerifyPowerBombColorAnimation(ISnesAddressSpace rom,
        PowerBombFixedColorCatalog catalog, bool crystalFlash)
    {
        var native = new SamusPowerBombExplosionState();
        var installed = new SamusPowerBombExplosionState { PresentationColors = catalog };
        var guarded = new ForbiddenPowerBombColorBus(rom);
        if (crystalFlash)
        {
            native.BeginCrystalFlash(128, 112);
            installed.BeginCrystalFlash(128, 112);
        }
        else
        {
            native.Arm();
            installed.Arm();
            native.Spawn(128, 112);
            installed.Spawn(128, 112);
        }

        int frames = 0;
        do
        {
            bool nativeComplete = native.StepFrame(rom);
            bool installedComplete = installed.StepFrame(guarded);
            string label = $"{(crystalFlash ? "Crystal Flash" : "Power Bomb")} frame {frames}";
            AssertEqual(nativeComplete, installedComplete, $"{label} completion");
            AssertEqual(native.Phase, installed.Phase, $"{label} next phase");
            AssertEqual(native.RenderedPhase, installed.RenderedPhase, $"{label} rendered phase");
            AssertEqual(native.PreExplosionRadius, installed.PreExplosionRadius, $"{label} pre-radius");
            AssertEqual(native.ExplosionRadius, installed.ExplosionRadius, $"{label} radius");
            AssertEqual(native.FixedColorRed, installed.FixedColorRed, $"{label} fixed red");
            AssertEqual(native.FixedColorGreen, installed.FixedColorGreen, $"{label} fixed green");
            AssertEqual(native.FixedColorBlue, installed.FixedColorBlue, $"{label} fixed blue");
            frames++;
            AssertTrue(frames < 1000, $"{label} completes in bounded time");
        } while (native.IsActive);
        AssertTrue(!installed.IsActive, "installed explosion completes with native state");
        AssertEqual(0, guarded.ForbiddenReads, "installed explosion never rereads fixed-color ROM tables");
    }

    private static void VerifyPowerBombFixedColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, PowerBombFixedColorFormat.FileName);
        PowerBombFixedColorDocument document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, PowerBombFixedColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Power Bomb color document is null.");
        PaletteRgb5 original = document.PreExplosion[0];
        document.PreExplosion[0] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, PowerBombFixedColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "Power Bomb color edit changes installed content identity");
        AssertTrue(edited.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0) !=
            baseline.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0),
            "Power Bomb color override changes the selected component");
        var state = new SamusPowerBombExplosionState { PresentationColors = edited.PowerBombFixedColors };
        state.Arm();
        state.Spawn(128, 112);
        var forbidden = new ForbiddenPowerBombColorBus(new TestAddressSpace());
        _ = state.StepFrame(forbidden); // Native setup precedes the first colored frame.
        _ = state.StepFrame(forbidden);
        AssertEqual((byte)document.PreExplosion[0].Red, state.FixedColorRed,
            "Power Bomb color override reaches the live explosion frame");
        AssertEqual(0, forbidden.ForbiddenReads,
            "edited Power Bomb color does not read the native table");
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorCatalog.Write(document with
        {
            PreExplosion = [original with { Red = 32 }, .. document.PreExplosion.Skip(1)],
        }), "out-of-range Power Bomb color fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing Power Bomb color override restores stock identity");
        Console.WriteLine("Power Bomb color override: edited component reaches explosion and removal restores stock.");
    }

    private sealed class ForbiddenPowerBombColorBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (PowerBombFixedColorSequence sequence in Enum.GetValues<PowerBombFixedColorSequence>())
            {
                int start = PowerBombFixedColorFormat.SourceAddress(sequence);
                int length = PowerBombFixedColorFormat.Count(sequence) *
                    SamusPaletteRomData.PowerBomb.BytesPerColor;
                if (address >= start && address < start + length)
                {
                    ForbiddenReads++;
                    throw new InvalidOperationException($"Installed explosion read color ROM ${address:X6}.");
                }
            }
            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
