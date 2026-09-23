using System.Text;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifySamusHurtColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus hurt colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom("Super Metroid.smc");
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
        var native = new SamusState { HurtFlashCounter = 1 };
        var installed = new SamusState { HurtFlashCounter = 1 };
        native.LiquidPhysics.CinematicFunctionActive = cinematic;
        installed.LiquidPhysics.CinematicFunctionActive = cinematic;
        var nativeCgram = new SnesCgram();
        var installedCgram = new SnesCgram();
        var guarded = new ForbiddenHurtColorBus(rom);
        for (int call = 1; call <= 7; call++)
        {
            SamusHurtFlashPaletteStepResult expected = SamusHurtFlashPalette.Update(
                rom, nativeCgram, native, 0);
            SamusHurtFlashPaletteStepResult actual = SamusHurtFlashPalette.Update(
                guarded, installedCgram, installed, 0, catalog);
            AssertEqual(expected, actual, $"{(cinematic ? "intro" : "ordinary")} hurt call {call} result");
            AssertEqual(native.HurtFlashCounter, installed.HurtFlashCounter,
                $"{(cinematic ? "intro" : "ordinary")} hurt call {call} counter");
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                AssertEqual(nativeCgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + index],
                    installedCgram.Colors[SamusPaletteRomData.Common.SamusObjPaletteStart + index],
                    $"{(cinematic ? "intro" : "ordinary")} hurt call {call} CGRAM {index}");
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
            SamusHurtFlashPaletteStepResult step = SamusHurtFlashPalette.Update(
                guarded, cgram, samus, 0, edited.SamusHurtColors);
            SamusHurtColorVariant variant = cinematic
                ? SamusHurtColorVariant.Intro : SamusHurtColorVariant.Hurt;
            AssertEqual(cinematic ? SamusHurtFlashPaletteAction.IntroRestore :
                SamusHurtFlashPaletteAction.HurtFlash, step.Action,
                $"edited {variant} path remains selected by native counter");
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

    private sealed class ForbiddenHurtColorBus(ISnesAddressSpace inner) : ISnesAddressSpace
    {
        public int ForbiddenReads { get; private set; }
        public byte ReadByte(int address)
        {
            if (address >= SamusHurtColorFormat.HurtSourceAddress &&
                address < SamusHurtColorFormat.IntroSourceAddress +
                    SamusHurtColorFormat.ColorsPerPalette * sizeof(ushort))
            {
                ForbiddenReads++;
                throw new InvalidOperationException($"Installed hurt handler read native artwork ${address:X6}.");
            }
            return inner.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => inner.WriteByte(address, value);
    }
}
