using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.AssetExtraction.CartridgeTables;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Samus.Artwork;

/// <summary>Checks installed Samus hurt and intro flash palettes against native CGRAM behavior.</summary>
internal static class SamusHurtColorsVerification
{
    /// <summary>Compares extracted colors and ordinary and cinematic flash cycles, including malformed inputs.</summary>
    internal static void VerifySamusHurtColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Samus hurt colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        InstalledSamusHurtColors catalog = SamusHurtColorFile.Read(
            new MemoryStream(SamusHurtColorExtractor.Extract(rom)));
        foreach (SamusHurtColorVariant variant in Enum.GetValues<SamusHurtColorVariant>())
        {
            int address = variant == SamusHurtColorVariant.Hurt
                ? SamusSourceDefinitions.HurtFlashColors.Value
                : SamusSourceDefinitions.IntroHurtFlashColors.Value;
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
            {
                int wordAddress = address + index * sizeof(ushort);
                ushort native = CartridgeImportSource.Require(rom).ReadCartridgeWord(wordAddress);
                AssertEqual(native, catalog.Resolve(variant, index),
                    $"native {variant} color {index}");
            }
        }

        CompareHurtCycle(rom, catalog, cinematic: false);
        CompareHurtCycle(rom, catalog, cinematic: true);
        AssertThrows<InvalidDataException>(() => SamusHurtColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Samus hurt JSON property fails loudly");
        AssertThrows<InvalidDataException>(() => SamusHurtColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt Samus hurt JSON fails loudly");
        Console.WriteLine("  Samus hurt colors: both native palettes and guarded ordinary/cinematic cycles pass.");
    }

    /// <summary>Checks each counter step's selected source palette or intentional no-write frame.</summary>
    private static void CompareHurtCycle(ISnesAddressSpace rom,
        InstalledSamusHurtColors catalog, bool cinematic)
    {
        var samus = new SamusState(InstalledFixtures.AnimationContinuation, new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence(), SuperMetroid.Core.Samus.SamusCompatibility.ForNewSamus(), new SuperMetroid.Core.Samus.Projectiles.BeamChargeState()).WithHurtFlashCount(1);
        // The flash names shades (#1512); the import resolves them from the catalog and the cartridge's suits.
        var sources = InstalledFixtures.SamusShadeSources with
        {
            Hurt = catalog,
            Suits = SamusSuitColorFile.Read(new MemoryStream(SamusSuitColorExtractor.Extract(rom))),
        };
        samus.LiquidPhysics.CinematicActive = cinematic;
        var samusColours = new SamusColours();
        var guarded = NewForbiddenHurtColorBus(rom);
        // Each call starts from a sentinel shade, so the suit's shade shows whether that call changed it and, when it
        // did, which native source its colours come from.
        PictureShade sentinel = SamusShades.ChargedShotGlow;
        for (int call = 1; call <= 7; call++)
        {
            samusColours.ShowSuit(sentinel);
            SamusHurtFlashPalette.Update(
                samusColours, samus, 0);
            AssertEqual((ushort)(call + 1), samus.HurtFlash.Count,
                $"{(cinematic ? "intro" : "ordinary")} hurt call {call} counter");
            if (call == 7)
            {
                AssertEqual(sentinel, samusColours.Suit,
                    $"{(cinematic ? "intro" : "ordinary")} hurt call {call} leaves the suit's shade unchanged");
                continue;
            }
            int expectedPaletteAddress = (call & 1) != 0
                ? SamusSourceDefinitions.HurtFlashColors.Value
                : cinematic
                    ? SamusSourceDefinitions.IntroHurtFlashColors.Value
                    : SamusSourceDefinitions.PowerSuitPalette.Value;
            for (int index = 0; index < SamusHurtColorFormat.ColorsPerPalette; index++)
                AssertEqual((ushort)(ReadNativeHurtCycleColor(rom, expectedPaletteAddress, index) & 0x7fff),
                    samusColours.ShownSuit(sources)[index],
                    $"{(cinematic ? "intro" : "ordinary")} hurt call {call} source colour {index}");
        }
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            $"installed {(cinematic ? "intro" : "ordinary")} hurt cycle does not read native art");
    }

    /// <summary>Checks that the hurt colour codec rejects out-of-range colours.</summary>
    internal static void VerifySamusHurtColorsRejectMalformedDocuments(string stock)
    {
        SamusHurtColorDocument document = JsonSerializer.Deserialize<SamusHurtColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, SamusHurtColorFile.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Samus hurt color document is null.");
        AssertThrows<InvalidDataException>(() => SamusHurtColorFile.Read(new MemoryStream(SamusHurtColorFile.Write(document with
        {
            Hurt = [.. document.Hurt.Take(5), document.Hurt[5] with { Red = 32 }, .. document.Hurt.Skip(6)],
        }))), "out-of-range Samus hurt RGB5 fails loudly");
    }

    /// <summary>Rejects reads of native hurt and intro color tables after installed artwork binding.</summary>
    private static CartridgeReadGuard NewForbiddenHurtColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).Forbid(address => address >= SamusSourceDefinitions.HurtFlashColors.Value &&
                address < SamusSourceDefinitions.IntroHurtFlashColors.Value +
                    SamusHurtColorFormat.ColorsPerPalette * sizeof(ushort),
            address => $"Installed hurt handler read native artwork ${address:X6}.");

    /// <summary>Reads one cartridge BGR555 word for a source-palette comparison.</summary>
    private static ushort ReadNativeHurtCycleColor(ISnesAddressSpace rom, int paletteAddress, int index) =>
        RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom), paletteAddress + index * sizeof(ushort));

    /// <summary>Runs the Samus hurt palette comparison from the command dispatcher.</summary>
    [VerificationCommand("--samus-hurt-colors")]
    internal static int SamusHurtColorsCommand()
    {
        Suite(nameof(VerifySamusHurtColors), () => VerifySamusHurtColors());
        return 0;
    }
}
