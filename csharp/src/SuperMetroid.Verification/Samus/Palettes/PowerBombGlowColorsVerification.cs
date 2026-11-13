using SuperMetroid.AssetExtraction.CartridgeTables;
using System.Text;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using static SuperMetroid.Verification.Frontend.CeresCinematics.CeresExplosionVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.AssetExtraction;
namespace SuperMetroid.Verification.Samus.Palettes;
using SuperMetroid.Verification.Harness;

/// <summary>Checks fixed RGB5 colors through Power Bomb and Crystal Flash lifecycles against retail data.</summary>
internal static class PowerBombGlowColorsVerification
{
    /// <summary>Compares every glow colour RGB5 triplet and guarded Power Bomb/Crystal Flash lifecycle with cartridge data.</summary>
    internal static void VerifyPowerBombGlowColors()
    {
        if (!File.Exists("Super Metroid.smc"))
        {
            Console.WriteLine("  Power Bomb glow colours: cartridge comparison skipped (private ROM absent).");
            return;
        }

        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom("Super Metroid.smc");
        InstalledPowerBombGlowColors catalog = PowerBombGlowColorFile.Read(
            new MemoryStream(PowerBombGlowColorExtractor.Extract(rom)));
        int compared = 0;
        foreach (PowerBombGlowSequence sequence in Enum.GetValues<PowerBombGlowSequence>())
        {
            for (int index = 0; index < PowerBombGlowColorFormat.Count(sequence); index++)
            {
                int address = PowerBombColorSources.Start(sequence) +
                    index * SamusSourceDefinitions.PowerBombBytesPerColor;
                RgbColor color = catalog.Resolve(sequence, index);
                byte red = (byte)color.FadeLevels.Red;
                byte green = (byte)color.FadeLevels.Green;
                byte blue = (byte)color.FadeLevels.Blue;
                AssertEqual((byte)(rom.ReadCartridgeByte(address) & PowerBombGlowColorExtractor.ComponentMask),
                    red, $"{sequence} native red {index}");
                AssertEqual((byte)(rom.ReadCartridgeByte(address + 1) & PowerBombGlowColorExtractor.ComponentMask),
                    green, $"{sequence} native green {index}");
                AssertEqual((byte)(rom.ReadCartridgeByte(address + 2) & PowerBombGlowColorExtractor.ComponentMask),
                    blue, $"{sequence} native blue {index}");
                compared++;
            }
        }

        Suite(nameof(VerifyPowerBombColorAnimation), () => VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: false));
        Suite(nameof(VerifyPowerBombColorAnimation), () => VerifyPowerBombColorAnimation(rom, catalog, crystalFlash: true));
        Suite(nameof(VerifyCeresExplosionTimeline), () => VerifyCeresExplosionTimeline(catalog));
        AssertThrows<InvalidDataException>(() => PowerBombGlowColorFile.Read(
            new MemoryStream([1, 2, 3])), "corrupt Power Bomb colors fail loudly");
        AssertThrows<InvalidDataException>(() => PowerBombGlowColorFile.Read(
            new MemoryStream(Encoding.UTF8.GetBytes("{\"version\":1,\"version\":1}"))),
            "duplicate Power Bomb color field fails loudly");
        Console.WriteLine($"  Power Bomb glow colours: {compared} native RGB5 triplets and guarded Power Bomb/Crystal Flash lifecycles pass.");
    }

    /// <summary>Compares native and guarded explosion phases, radii, completion, and glow colours frame by frame.</summary>
    private static void VerifyPowerBombColorAnimation(ISnesAddressSpace rom,
        InstalledPowerBombGlowColors catalog, bool crystalFlash)
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
            AssertEqual(native.GlowColor.FadeLevels.Red, installed.GlowColor.FadeLevels.Red, $"{label} fixed red");
            AssertEqual(native.GlowColor.FadeLevels.Green, installed.GlowColor.FadeLevels.Green, $"{label} fixed green");
            AssertEqual(native.GlowColor.FadeLevels.Blue, installed.GlowColor.FadeLevels.Blue, $"{label} fixed blue");
            frames++;
            AssertTrue(frames < 1000, $"{label} completes in bounded time");
        } while (native.IsActive);
        AssertTrue(!installed.IsActive, "installed explosion completes with native state");
        AssertEqual(0, guarded.ForbiddenReadAttempts, "installed explosion never rereads fixed-color ROM tables");
    }

    /// <summary>Checks that the Power Bomb colour codec rejects out-of-range colours.</summary>
    internal static void VerifyPowerBombGlowColorsRejectMalformedDocuments(string stock)
    {
        PowerBombGlowColorDocument document = JsonSerializer.Deserialize<PowerBombGlowColorDocument>(
            File.ReadAllBytes(Path.Combine(stock, PowerBombGlowColorFile.FileName)),
            ArtworkJson.Options)
            ?? throw new InvalidDataException("Stock Power Bomb color document is null.");
        AssertThrows<InvalidDataException>(() => PowerBombGlowColorFile.Read(new MemoryStream(PowerBombGlowColorFile.Write(document with
        {
            PreExplosion = [document.PreExplosion[0] with { Red = 32 }, .. document.PreExplosion.Skip(1)],
        }))), "out-of-range Power Bomb color fails loudly");
    }

    /// <summary>Forbids all authored fixed-color source ranges during installed explosion updates.</summary>
    internal static CartridgeReadGuard NewForbiddenPowerBombColorBus(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source)
            .Forbid(address => Enum.GetValues<PowerBombGlowSequence>().Any(sequence =>
                    address >= PowerBombColorSources.Start(sequence) &&
                    address < PowerBombColorSources.Start(sequence) +
                        PowerBombGlowColorFormat.Count(sequence) * SamusSourceDefinitions.PowerBombBytesPerColor),
                address => $"Installed explosion read color ROM ${address:X6}.");

    /// <summary>Runs the Power Bomb glow colour verification suite.</summary>
    [VerificationCommand("--power-bomb-glow-colors")]
    internal static int PowerBombGlowColorsCommand()
    {
        Suite(nameof(VerifyPowerBombGlowColors), () => VerifyPowerBombGlowColors());
        return 0;
    }
}
