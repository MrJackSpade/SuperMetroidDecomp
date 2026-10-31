using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Samus.Palettes;
using System.Text.Json;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Botwoon palette art, live health transitions, and override validation.</summary>
internal static class EnemyTileArtworkBotwoonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 128 stock colors with the ROM and checks edited colors flow through live health-band updates.</summary>
    internal static void VerifyInstalledBotwoonColors(
        ISnesAddressSpace rom, string stockDirectory, InstalledEnemyArtwork stock)
    {
        BotwoonColorCatalog native = stock.Bosses.BotwoonColors ??
            throw new InvalidDataException("Installed enemy art has no Botwoon colors.");
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        for (int color = 0; color < BotwoonHealthPaletteDefinitions.ColorsPerPalette; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BotwoonSourceDefinitions.HealthColors +
                    (band * BotwoonHealthPaletteDefinitions.ColorsPerPalette + color) * 2),
                native.HealthColor(band, color),
                $"installed Botwoon health band {band} color {color}");

        string file = Path.Combine(stockDirectory, BotwoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BotwoonColorDocument visual = JsonSerializer.Deserialize<BotwoonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Botwoon color JSON is null.");
        foreach (PaletteRgb5[] band in visual.Health)
            band[5] = ChangeRed(band[5]);
        string overrides = Path.Combine(stockDirectory, "botwoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, BotwoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, BotwoonColorFile.Write(visual));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        BotwoonColorCatalog colors = edited.Bosses.BotwoonColors ??
            throw new InvalidDataException("Edited enemy art has no Botwoon colors.");
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        {
            AssertTrue(native.HealthColor(band, 5) != colors.HealthColor(band, 5),
                $"Botwoon health band {band} edit takes effect");
            AssertEqual(native.HealthColor(band, 4), colors.HealthColor(band, 4),
                $"Botwoon health band {band} adjacent color remains native");
        }
        AssertEqual(colors.HealthColor(7, 5),
            (EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .Bosses.BotwoonColors ?? throw new InvalidOperationException("Verification requires EnemyTileArtworkFiles.Load(stockDirectory, overrides) .BotwoonColors.")).HealthColor(7, 5),
            "Botwoon color override survives catalog reload");

        var guard = NewBotwoonColorReadGuard(rom);
        var cgram = new SamusColourRig();
        var enemies = new RoomEnemySystem(new EnemySession(new SuperMetroid.Core.Compatibility.UnauthoredBoostCadence()), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = edited.Enemies, BossArtwork = edited.Bosses, AreaArtwork = edited.Areas };
        enemies.Presentation.BindColours(cgram.Gameplay);
        Action<RoomEnemySlot, BotwoonEnemyState> update = enemies.BotwoonBehavior.UpdateBotwoonHealthPalette;
        RoomEnemySlot head = enemies.SlotTable.Slots[0];
        var state = new BotwoonEnemyState()
        {
            HealthColouring = new SuperMetroid.Core.Enemies.Drawing.EncounterSpriteColouring(SuperMetroid.Core.Bosses.Artwork.BossColourDefinitions.BotwoonBody),
        };
        cgram.SetColor(BotwoonHealthPaletteDefinitions.FirstColour - 1, RgbColor.FromWord(0x1234));
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        {
            ushort threshold = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + band * sizeof(ushort));
            state.PalettePhase = band;
            head.Health = threshold - 1;
            update(head, state);
            AssertEqual(band + 1, state.PalettePhase,
                $"Botwoon health band {band} preserves native threshold progression");
            for (int color = 0; color < BotwoonHealthPaletteDefinitions.ColorsPerPalette; color++)
                AssertEqual(colors.HealthColor(band, color),
                    cgram.Colors[BotwoonHealthPaletteDefinitions.FirstColour + color],
                    $"live Botwoon health band {band} color {color}");
            AssertEqual(0x1234,
                cgram.Colors[BotwoonHealthPaletteDefinitions.FirstColour - 1],
                $"Botwoon health band {band} preserves adjacent palette");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Botwoon health palette avoids migrated RGB5 ROM reads");
        state.PalettePhase = 0;
        // Over the set before the body's, the cartridge's copy would run on past the authored band.
        state.HealthColouring = SpriteColouring.InFixtureSet(SuperMetroid.Core.Bosses.Artwork.BossColourDefinitions.BotwoonBody.Store.Set - 1);
        head.Health = 0;
        AssertThrows<InvalidDataException>(() => update(head, state),
            "installed Botwoon rejects health colours other than its body's");

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Botwoon color override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Botwoon color property fails loudly");
        visual.Health[0][5] = visual.Health[0][5] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => BotwoonColorFile.Write(visual),
            "Botwoon RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Botwoon colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Botwoon colors: 128 native RGB5 words, eight live health bands, preserved threshold progression, persistent override, ROM guard and strict failures pass.");

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };
    }

    /// <summary>Guards the migrated Botwoon palette range against live cartridge reads.</summary>
    internal static CartridgeReadGuard NewBotwoonColorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BotwoonSourceDefinitions.HealthColors,
            BotwoonHealthPaletteDefinitions.PaletteCount *
                BotwoonHealthPaletteDefinitions.ColorsPerPalette * sizeof(ushort),
            address => $"Live Botwoon read migrated color ROM ${address:X6}.");
}
