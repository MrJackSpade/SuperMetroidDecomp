using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifySamusHurtColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus hurt colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        SamusHurtColorCatalog catalog = SamusHurtColorCatalog.Load(
            new MemoryStream(SamusHurtColorExtractor.Extract(rom)));
        foreach (SamusHurtColorVariant variant in Enum.GetValues<SamusHurtColorVariant>())
        {
            int address = variant == SamusHurtColorVariant.Hurt
                ? SamusHurtColorFormat.HurtSourceAddress
                : SamusHurtColorFormat.IntroSourceAddress;
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
            {
                int wordAddress = address + index * sizeof(ushort);
                ushort native = unchecked((ushort)(rom.ReadByte(wordAddress) |
                    rom.ReadByte(wordAddress + 1) << 8));
                AssertEqual(native, catalog.Resolve(variant, index),
                    $"native {variant} color {index}");
            }
        }

        CompareHurtCycle(rom, catalog, cinematic: false);
        CompareHurtCycle(rom, catalog, cinematic: true);
        AssertThrows<InvalidOperationException>(() => SamusHurtFlashPalette.Update(
            rom, new SnesCgram(), new SamusState { HurtFlashCounter = 1 }, 0),
            "hurt flash requires installed color artwork instead of a cartridge fallback");
        AssertThrows<InvalidDataException>(() => SamusHurtColorCatalog.Load(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Samus hurt JSON property fails loudly");
        AssertThrows<InvalidDataException>(() => SamusHurtColorCatalog.Load(
            new MemoryStream([1, 2, 3])), "corrupt Samus hurt JSON fails loudly");
        Console.WriteLine("  Samus hurt colors: both native palettes and guarded ordinary/cinematic cycles pass.");
    }

    private static void CompareHurtCycle(ISnesAddressSpace rom,
        SamusHurtColorCatalog catalog, bool cinematic)
    {
        var samus = new SamusState { HurtFlashCounter = 1 };
        samus.SuitColors = SamusSuitColorCatalog.Load(new MemoryStream(
            SamusSuitColorExtractor.Extract(rom)));
        samus.LiquidPhysics.CinematicFunctionActive = cinematic;
        var cgram = new SnesCgram();
        var guarded = new ForbiddenHurtColorBus(rom);
        // Each call starts from a sentinel palette, so CGRAM shows whether that call wrote
        // colors and, when it did, which native source it selected.
        const ushort sentinel = 0x5a5a;
        for (int call = 1; call <= 7; call++)
        {
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                cgram.SetColor(SamusPaletteRomData.Common.SamusObjPaletteStart + index, Bgr555.FromWord(checked((ushort)(sentinel))));
            SamusHurtFlashPalette.Update(
                guarded, cgram, samus, 0, catalog);
            AssertEqual((ushort)(call + 1), samus.HurtFlashCounter,
                $"{(cinematic ? "intro" : "ordinary")} hurt call {call} counter");
            if (call == 7)
            {
                for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                    AssertEqual(sentinel,
                        cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + index],
                        $"{(cinematic ? "intro" : "ordinary")} hurt call {call} leaves CGRAM {index} unchanged");
                continue;
            }
            int expectedPaletteAddress = (call & 1) != 0
                ? SamusHurtColorFormat.HurtSourceAddress
                : cinematic
                    ? SamusHurtColorFormat.IntroSourceAddress
                    : SamusRenderingRomData.Body.PowerSuitPalette;
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                AssertEqual((ushort)(SuperMetroid.Core.Rom.RomDataReader.ReadWordFixedBank(
                        CartridgeImportSource.Require(rom),
                        expectedPaletteAddress + index * sizeof(ushort)) & 0x7fff),
                    cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + index],
                    $"{(cinematic ? "intro" : "ordinary")} hurt call {call} source CGRAM {index}");
        }
        AssertEqual(0, guarded.ForbiddenReads,
            $"installed {(cinematic ? "intro" : "ordinary")} hurt cycle does not read native art");
    }

    private static void VerifySamusHurtColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline, ISnesAddressSpace rom)
    {
        string path = Path.Combine(overrides, SamusHurtColorFormat.FileName);
        SamusHurtColorDocument document = JsonSerializer.Deserialize<SamusHurtColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusHurtColorFormat.FileName)),
            MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Stock Samus hurt color document is null.");
        PaletteRgb5 hurtOriginal = document.Hurt[5];
        PaletteRgb5 introOriginal = document.Intro[5];
        document.Hurt[5] = hurtOriginal with { Red = (hurtOriginal.Red + 1) % 32 };
        document.Intro[5] = introOriginal with { Blue = (introOriginal.Blue + 1) % 32 };
        File.WriteAllBytes(path, SamusHurtColorCatalog.Write(document));
        AreaMapPresentationCatalog edited = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "hurt color override changes installed content identity");
        foreach (bool cinematic in new[] { false, true })
        {
            var samus = new SamusState { HurtFlashCounter = (ushort)(cinematic ? 2 : 1) };
            samus.LiquidPhysics.CinematicFunctionActive = cinematic;
            var cgram = new SnesCgram();
            var guarded = new ForbiddenHurtColorBus(rom);
            SamusHurtFlashPalette.Update(
                guarded, cgram, samus, 0, edited.SamusHurtColors);
            SamusHurtColorVariant variant = cinematic
                ? SamusHurtColorVariant.Intro : SamusHurtColorVariant.Hurt;
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                AssertEqual(edited.SamusHurtColors.Resolve(variant, index),
                    cgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + index],
                    $"edited {variant} path remains selected by native counter (CGRAM {index})");
            AssertEqual(edited.SamusHurtColors.Resolve(variant, 5), cgram.Colors[197],
                $"edited {variant} color reaches CGRAM");
            AssertEqual(0, guarded.ForbiddenReads, $"edited {variant} avoids native art");
        }
        AssertThrows<InvalidDataException>(() => SamusHurtColorCatalog.Write(document with
        {
            Hurt = [.. document.Hurt.Take(5), hurtOriginal with { Red = 32 }, .. document.Hurt.Skip(6)],
        }), "out-of-range Samus hurt RGB5 fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = AreaMapPresentationCatalog.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing hurt override restores stock content identity");
        Console.WriteLine("Samus hurt override: edited flash/intro colors reach CGRAM and stock restores.");
    }

    private sealed class ForbiddenHurtColorBus(ISnesAddressSpace inner) : ISnesAddressSpace, IImportCartridgeSource
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadCartridgeByte(int address) => ReadByte(address);

        public byte ReadByte(int address)
        {
            if (address is >= SamusHurtColorFormat.HurtSourceAddress and
                < (SamusHurtColorFormat.IntroSourceAddress +
                    SamusHurtColorFormat.ColorsPerPalette * sizeof(ushort)))
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed hurt handler read native artwork ${address:X6}.");
            }
            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
