using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresExplosionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;
using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>Checks fixed RGB5 colors through Power Bomb and Crystal Flash lifecycles against retail data.</summary>
internal static class PowerBombFixedColorsVerification
{
    /// <summary>Compares every fixed-color RGB5 triplet and guarded Power Bomb/Crystal Flash lifecycle with cartridge data.</summary>
    internal static void VerifyPowerBombFixedColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Power Bomb fixed colors: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        InstalledPowerBombFixedColors catalog = PowerBombFixedColorFile.Read(
            new MemoryStream(PowerBombFixedColorExtractor.Extract(rom)));
        int compared = 0;
        foreach (PowerBombFixedColorSequence sequence in Enum.GetValues<PowerBombFixedColorSequence>())
        {
            for (int index = 0; index < PowerBombFixedColorFormat.Count(sequence); index++)
            {
                int address = PowerBombColorSources.Start(sequence) +
                    index * SamusSourceDefinitions.PowerBombBytesPerColor;
                RgbColor color = catalog.Resolve(sequence, index);
                byte red = (byte)color.Red;
                byte green = (byte)color.Green;
                byte blue = (byte)color.Blue;
                AssertEqual((byte)(rom.ReadCartridgeByte(address) & SamusPaletteDefinitions.PowerBomb.ComponentMask),
                    red, $"{sequence} native red {index}");
                AssertEqual((byte)(rom.ReadCartridgeByte(address + 1) & SamusPaletteDefinitions.PowerBomb.ComponentMask),
                    green, $"{sequence} native green {index}");
                AssertEqual((byte)(rom.ReadCartridgeByte(address + 2) & SamusPaletteDefinitions.PowerBomb.ComponentMask),
                    blue, $"{sequence} native blue {index}");
                compared++;
            }
        }

        Suite(nameof(VerifyPowerBombColorAnimation), () => VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: false));
        Suite(nameof(VerifyPowerBombColorAnimation), () => VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: true));
        Suite(nameof(VerifyCeresExplosionTimeline), () => VerifyCeresExplosionTimeline(catalog));
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt Power Bomb colors fail loudly");
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Power Bomb color field fails loudly");
        Console.WriteLine($"  Power Bomb fixed colors: {compared} native RGB5 triplets and guarded Power Bomb/Crystal Flash lifecycles pass.");
    }

    /// <summary>Compares native and guarded explosion phases, radii, completion, and fixed colors frame by frame.</summary>
    private static void VerifyPowerBombColorAnimation(ISnesAddressSpace rom,
        InstalledPowerBombFixedColors catalog, bool crystalFlash)
    {
        // The source-color words above are checked against the import-only ROM reader.
        // The runtime no longer has a cartridge color fallback, so both timing owners
        // must receive the installed catalog; the guarded one also rejects any stray
        // gameplay request for those native color bytes.
        var native = new SamusPowerBombExplosionState { PresentationColors = catalog };
        var installed = new SamusPowerBombExplosionState { PresentationColors = catalog };
        var guarded = NewForbiddenPowerBombColorBus(rom);
        if (crystalFlash)
        {
            native.BeginCrystalFlash(new(128, 112));
            installed.BeginCrystalFlash(new(128, 112));
        }
        else
        {
            native.Arm();
            installed.Arm();
            native.Spawn(new(128, 112));
            installed.Spawn(new(128, 112));
        }

        int frames = 0;
        do
        {
            bool nativeComplete = native.StepFrame();
            bool installedComplete = installed.StepFrame();
            string label = $"{(crystalFlash ? "Crystal Flash" : "Power Bomb")} frame {frames}";
            AssertEqual(nativeComplete, installedComplete, $"{label} completion");
            AssertEqual(native.Phase, installed.Phase, $"{label} next phase");
            AssertEqual(native.RenderedPhase, installed.RenderedPhase, $"{label} rendered phase");
            AssertEqual(native.PreExplosionRadius, installed.PreExplosionRadius, $"{label} pre-radius");
            AssertEqual(native.ExplosionRadius, installed.ExplosionRadius, $"{label} radius");
            AssertEqual(native.FixedColor.Red, installed.FixedColor.Red, $"{label} fixed red");
            AssertEqual(native.FixedColor.Green, installed.FixedColor.Green, $"{label} fixed green");
            AssertEqual(native.FixedColor.Blue, installed.FixedColor.Blue, $"{label} fixed blue");
            frames++;
            AssertTrue(frames < 1000, $"{label} completes in bounded time");
        } while (native.IsActive);
        AssertTrue(!installed.IsActive, "installed explosion completes with native state");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed explosion never rereads fixed-color ROM tables");
    }

    /// <summary>Checks an edited pre-explosion color reaches live state and removing the override restores stock identity.</summary>
    internal static void VerifyPowerBombFixedColorOverride(string stock, string overrides,
        AreaMapPresentationCatalog baseline)
    {
        string path = Path.Combine(overrides, PowerBombFixedColorFormat.FileName);
        PowerBombFixedColorDocument document = JsonSerializer.Deserialize<PowerBombFixedColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, PowerBombFixedColorFormat.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Power Bomb color document is null.");
        PaletteRgb5 original = document.PreExplosion[0];
        document.PreExplosion[0] = original with { Red = (original.Red + 1) % 32 };
        File.WriteAllBytes(path, PowerBombFixedColorFile.Write(document));
        AreaMapPresentationCatalog edited = MapPresentationFiles.Load(stock, overrides);
        AssertTrue(edited.ContentIdentity != baseline.ContentIdentity,
            "Power Bomb color edit changes installed content identity");
        AssertTrue(edited.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0) !=
            baseline.PowerBombFixedColors.Resolve(PowerBombFixedColorSequence.PreExplosion, 0),
            "Power Bomb color override changes the selected component");
        var state = new SamusPowerBombExplosionState { PresentationColors = edited.PowerBombFixedColors };
        state.Arm();
        state.Spawn(new(128, 112));
        var forbidden = NewForbiddenPowerBombColorBus(new TestAddressSpace());
        _ = state.StepFrame(); // Native setup precedes the first colored frame.
        _ = state.StepFrame();
        AssertEqual(document.PreExplosion[0].Red, state.FixedColor.Red,
            "Power Bomb color override reaches the live explosion frame");
        AssertEqual(0, forbidden.ForbiddenReadAttempts,
            "edited Power Bomb color does not read the native table");
        AssertThrows<InvalidDataException>(() => PowerBombFixedColorFile.Write(document with
        {
            PreExplosion = [original with { Red = 32 }, .. document.PreExplosion.Skip(1)],
        }), "out-of-range Power Bomb color fails loudly");
        File.Delete(path);
        AreaMapPresentationCatalog restored = MapPresentationFiles.Load(stock, overrides);
        AssertEqual(baseline.ContentIdentity, restored.ContentIdentity,
            "removing Power Bomb color override restores stock identity");
        Console.WriteLine("Power Bomb color override: edited component reaches explosion and removal restores stock.");
    }

    /// <summary>Forbids all authored fixed-color source ranges during installed explosion updates.</summary>
    internal static CartridgeReadGuard NewForbiddenPowerBombColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => Enum.GetValues<PowerBombFixedColorSequence>().Any(sequence =>
                    address >= PowerBombColorSources.Start(sequence) &&
                    address < PowerBombColorSources.Start(sequence) +
                        PowerBombFixedColorFormat.Count(sequence) * SamusSourceDefinitions.PowerBombBytesPerColor),
                address => $"Installed explosion read color ROM ${address:X6}.");

    /// <summary>Runs the Power Bomb fixed-color verification suite.</summary>
    [VerificationCommand("--power-bomb-fixed-colors")]
    internal static int PowerBombFixedColorsCommand()
    {
        Suite(nameof(VerifyPowerBombFixedColors), () => VerifyPowerBombFixedColors());
        return 0;
    }
}
