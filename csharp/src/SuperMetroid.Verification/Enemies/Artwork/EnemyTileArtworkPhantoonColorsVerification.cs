using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Phantoon health, fade, and power-on colors against
/// native RGB5 data and live palette transitions.</summary>
internal static class EnemyTileArtworkPhantoonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Checks isolated color edits through boss fade and hurt handlers,
    /// with native source reads forbidden and malformed overrides rejected.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Phantoon palette bands")]
    internal static void VerifyInstalledPhantoonColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        PhantoonColorCatalog native = stock.Bosses.PhantoonColors ??
            throw new InvalidDataException("Installed enemy art has no Phantoon colors.");
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonColorRomData.FadeOutSource,
            PhantoonColorRomData.FadeOutCount, native.ResolveFadeOut, "fade-out"));
        Suite(nameof(VerifyBand), () => VerifyBand(PhantoonColorRomData.PowerOnSource,
            PhantoonColorRomData.PowerOnCount, native.ResolvePowerOn, "power-on"));
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    PhantoonColorRomData.HealthBandsSource +
                    (band * PhantoonColorRomData.HealthBandColorCount + color) *
                        sizeof(ushort)),
                native.ResolveHealth(band, color),
                $"installed Phantoon health palette {band} color {color} preserves RGB5");

        string file = Path.Combine(stockDirectory, PhantoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        PhantoonColorDocument visual = JsonSerializer.Deserialize<PhantoonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Phantoon color JSON is null.");
        visual.HealthBands[2][1] = ChangeRed(visual.HealthBands[2][1]);
        visual.FadeOut[1] = ChangeRed(visual.FadeOut[1]);
        visual.PowerOn[1] = ChangeRed(visual.PowerOn[1]);
        string overrides = Path.Combine(stockDirectory, "phantoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, PhantoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, PhantoonColorCatalog.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        PhantoonColorCatalog colors = edited.Bosses.PhantoonColors ??
            throw new InvalidDataException("Edited enemy art has no Phantoon colors.");
        AssertTrue(colors.ResolveHealth(2, 1) != native.ResolveHealth(2, 1),
            "Phantoon health color edit changes selected RGB5 word");
        AssertTrue(colors.ResolveFadeOut(1) != native.ResolveFadeOut(1),
            "Phantoon fade-out edit changes selected RGB5 word");
        AssertTrue(colors.ResolvePowerOn(1) != native.ResolvePowerOn(1),
            "Phantoon power-on edit changes selected RGB5 word");
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
            if (band != 2 || color != 1)
                AssertEqual(native.ResolveHealth(band, color),
                    colors.ResolveHealth(band, color),
                    $"Phantoon health edit leaves palette {band} color {color} unchanged");
        CheckUnchangedBand(PhantoonColorRomData.FadeOutCount,
            native.ResolveFadeOut, colors.ResolveFadeOut, "fade-out");
        CheckUnchangedBand(PhantoonColorRomData.PowerOnCount,
            native.ResolvePowerOn, colors.ResolvePowerOn, "power-on");
        AssertEqual(colors.ResolvePowerOn(1),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.PhantoonColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .PhantoonColors.")).ResolvePowerOn(1),
            "Phantoon override survives catalog reload");

        // Drive the actual boss palette handlers. The complete authored color range is
        // forbidden on the bus, proving no installed runtime path silently falls back.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= PhantoonColorRomData.FadeOutSource and
                < (PhantoonColorRomData.HealthBandsSource +
                    PhantoonColorRomData.HealthBandCount *
                    PhantoonColorRomData.HealthBandColorCount * sizeof(ushort)),
            address => $"Phantoon accessed migrated palette ROM ${address:X6}.");
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        Type type = typeof(RoomEnemySystem);
        enemies.Presentation.BindCgram(cgram.Gameplay);
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
        };
        enemies.PhantoonBehavior.Combat.Phantoon = state;
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        {
            body.Health = unchecked((ushort)(band * 312 + 1));
            enemies.PhantoonBehavior.CopyPhantoonHealthPalette(body);
            AssertBodyBand(band, $"Phantoon live health restore band {band}");
            for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
                cgram.SetColor(PhantoonColorRomDataConstants.BodyDestination + color, new Bgr555(0, 0, 0));
            state.Parts[eye.SlotIndex].FadeNumerator = 1;
            state.Parts[eye.SlotIndex].FadeComplete = false;
            enemies.PhantoonBehavior.Presentation.AdvancePhantoonFadeIn(body, state, 0);
            AssertBodyBand(band, $"Phantoon live materialization band {band}");
            AssertEqual(2, state.Parts[eye.SlotIndex].FadeNumerator,
                $"Phantoon fade-in band {band} retains native numerator advance");
        }

        body.Health = unchecked(2 * 312 + 1);
        body.FlashTimer = WrappingCountdown.Start(8);
        state.Parts[tentacles.SlotIndex].HitReaction = 1; state.Parts[tentacles.SlotIndex].HurtFlashShown = true;
        enemies.PhantoonBehavior.ApplyPhantoonHurt(body, state);
        AssertBodyBand(2, "Phantoon live hurt restore");
        AssertTrue(state.Parts[tentacles.SlotIndex].HitReaction == 1 && !state.Parts[tentacles.SlotIndex].HurtFlashShown,
            "Phantoon hurt restore clears only the flash-latch byte");

        state.Parts[eye.SlotIndex].FadeNumerator = 1;
        state.Parts[eye.SlotIndex].FadeComplete = false;
        enemies.PhantoonBehavior.Patterns.AdvancePhantoonFadeOut(state, 0);
        for (int color = 0; color < PhantoonColorRomData.FadeOutCount; color++)
            AssertEqual(colors.ResolveFadeOut(color),
                cgram.Colors[PhantoonColorRomDataConstants.BodyDestination + color],
                $"Phantoon live fade-out target {color}");
        AssertEqual(2, state.Parts[eye.SlotIndex].FadeNumerator,
            "Phantoon fade-out retains native numerator advance");

        state.Parts[eye.SlotIndex].FadeDenominator = 0;
        state.Parts[eye.SlotIndex].FadeNumerator = 1;
        bool complete = enemies.PhantoonBehavior.Death.AdvanceWreckedShipPowerPalette(state.Parts[eye.SlotIndex]);
        AssertTrue(!complete, "Phantoon ship power-on remains active on target frame");
        for (int color = 0; color < PhantoonColorRomData.PowerOnCount; color++)
            AssertEqual(colors.ResolvePowerOn(color),
                cgram.Colors[PhantoonColorRomData.PowerOnDestination + color],
                $"Phantoon live ship power-on target {color}");
        AssertEqual(2, state.Parts[eye.SlotIndex].FadeNumerator,
            "Phantoon ship power-on retains native numerator advance");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Phantoon override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Phantoon color property fails loudly");
        visual.HealthBands[2][1] = visual.HealthBands[2][1] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => PhantoonColorCatalog.Write(visual),
            "Phantoon RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Phantoon colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Phantoon colors: 256 native RGB5 targets, all health bands and three live transitions, persistent override, ROM guard, and strict failures pass.");

        void VerifyBand(int source, int count, Func<int, Bgr555> resolve, string name)
        {
            for (int color = 0; color < count; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed Phantoon {name} color {color} preserves native RGB5");
        }

        void AssertBodyBand(int band, string name)
        {
            for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
                AssertEqual(colors.ResolveHealth(band, color),
                    cgram.Colors[PhantoonColorRomDataConstants.BodyDestination + color],
                    $"{name} color {color}");
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckUnchangedBand(int count, Func<int, Bgr555> original,
            Func<int, Bgr555> changed, string name)
        {
            for (int color = 0; color < count; color++)
                if (color != 1)
                    AssertEqual(original(color), changed(color),
                        $"Phantoon {name} edit leaves color {color} unchanged");
        }
    }
}
