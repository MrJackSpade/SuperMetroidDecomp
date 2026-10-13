using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;
using static SuiteTimingVerification;

/// <summary>Checks installed Phantoon health, fade, and power-on colors against
/// native RGB5 data and live palette transitions.</summary>
internal static class EnemyTileArtworkPhantoonColorsVerification
{
    /// <summary>Checks isolated color edits through boss fade and hurt handlers,
    /// with native source reads forbidden and malformed overrides rejected.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native CGRAM slots")]
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Phantoon palette bands")]
    internal static void VerifyInstalledPhantoonColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        PhantoonColorCatalog native = stock.PhantoonColors ??
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
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Phantoon color JSON is null.");
        visual.HealthBands[2][1] = ChangeRed(visual.HealthBands[2][1]);
        visual.FadeOut[1] = ChangeRed(visual.FadeOut[1]);
        visual.PowerOn[1] = ChangeRed(visual.PowerOn[1]);
        string overrides = Path.Combine(stockDirectory, "phantoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, PhantoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, PhantoonColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        PhantoonColorCatalog colors = edited.PhantoonColors ??
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
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .PhantoonColors!.ResolvePowerOn(1),
            "Phantoon override survives catalog reload");

        // Drive the actual boss palette handlers. The complete authored color range is
        // forbidden on the bus, proving no installed runtime path silently falls back.
        CartridgeReadGuard guarded = new CartridgeReadGuard(rom).Forbid(
            address => address is >= PhantoonColorRomData.FadeOutSource and
                < (PhantoonColorRomData.HealthBandsSource +
                    PhantoonColorRomData.HealthBandCount *
                    PhantoonColorRomData.HealthBandColorCount * sizeof(ushort)),
            address => $"Phantoon accessed migrated palette ROM ${address:X6}.");
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        enemies.BindFixtureBus(guarded);
        enemies.Presentation._cgram = cgram;
        RoomEnemySlot body = enemies.SlotTable.Slots[0];
        RoomEnemySlot eye = enemies.SlotTable.Slots[1];
        RoomEnemySlot tentacles = enemies.SlotTable.Slots[2];
        var state = new PhantoonEnemyState(body)
        {
            Eye = eye,
            Tentacles = tentacles,
        };
        for (int band = 0; band < PhantoonColorRomData.HealthBandCount; band++)
        {
            body.Health = unchecked((ushort)(band * 312 + 1));
            enemies.PhantoonBehavior.CopyPhantoonHealthPalette(body);
            AssertBodyBand(band, $"Phantoon live health restore band {band}");
            for (int color = 0; color < PhantoonColorRomData.HealthBandColorCount; color++)
                cgram.SetColor(PhantoonColorRomData.BodyDestination + color, new Bgr555(0, 0, 0));
            eye.VariableE = 1;
            eye.VariableF = 0;
            enemies.PhantoonBehavior.AdvancePhantoonFadeIn(body, state, (ushort)0);
            AssertBodyBand(band, $"Phantoon live materialization band {band}");
            AssertEqual((ushort)2, eye.VariableE,
                $"Phantoon fade-in band {band} retains native numerator advance");
        }

        body.Health = unchecked((ushort)(2 * 312 + 1));
        body.FlashTimer = NativeCountdown.Start(8);
        tentacles.Parameter2 = 0x0101;
        enemies.PhantoonBehavior.ApplyPhantoonHurt(body, state);
        AssertBodyBand(2, "Phantoon live hurt restore");
        AssertEqual((ushort)0x0001, tentacles.Parameter2,
            "Phantoon hurt restore clears only the flash-latch byte");

        eye.VariableE = 1;
        eye.VariableF = 0;
        enemies.PhantoonBehavior.AdvancePhantoonFadeOut(state, (ushort)0);
        for (int color = 0; color < PhantoonColorRomData.FadeOutCount; color++)
            AssertEqual(colors.ResolveFadeOut(color),
                cgram.Colors[PhantoonColorRomData.BodyDestination + color],
                $"Phantoon live fade-out target {color}");
        AssertEqual((ushort)2, eye.VariableE,
            "Phantoon fade-out retains native numerator advance");

        eye.VariableD = 0;
        eye.VariableE = 1;
        bool complete = (bool)enemies.PhantoonBehavior.AdvanceWreckedShipPowerPalette(eye);
        AssertTrue(!complete, "Phantoon ship power-on remains active on target frame");
        for (int color = 0; color < PhantoonColorRomData.PowerOnCount; color++)
            AssertEqual(colors.ResolvePowerOn(color),
                cgram.Colors[PhantoonColorRomData.PowerOnDestination + color],
                $"Phantoon live ship power-on target {color}");
        AssertEqual((ushort)2, eye.VariableE,
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
                    cgram.Colors[PhantoonColorRomData.BodyDestination + color],
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
