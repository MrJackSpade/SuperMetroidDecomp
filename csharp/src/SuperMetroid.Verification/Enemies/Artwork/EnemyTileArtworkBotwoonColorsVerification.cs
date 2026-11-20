using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using System.Text.Json;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.TestSupport.CpuBusTestDoubles;
using SuperMetroid.Core.World;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Bosses.Botwoon.Artwork;
using SuperMetroid.Verification.Bosses;
using SuperMetroid.Core.Bosses.Botwoon.Artwork;
using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Verification.Harness;

namespace SuperMetroid.Verification.Enemies.Artwork;

/// <summary>Checks installed Botwoon palette art, live health transitions, and document validation.</summary>
internal static class EnemyTileArtworkBotwoonColorsVerification
{
    /// <summary>JSON options: Case-insensitive property matching.</summary>
    private static readonly JsonSerializerOptions CaseInsensitiveJson = new() { PropertyNameCaseInsensitive = true };

    /// <summary>Compares 128 stock colors with the ROM and checks installed colors flow through live health-band updates.</summary>
    internal static void VerifyInstalledBotwoonColors(
        ISnesAddressSpace rom, string stockDirectory, ImportEnemyArtwork stock)
    {
        BotwoonColorCatalog native = stock.Bosses.BotwoonColors ??
            throw new InvalidDataException("Installed enemy art has no Botwoon colors.");
        for (int band = 0; band < BotwoonColorFormat.HealthBandCount; band++)
        for (int color = 0; color < BotwoonColorFormat.ColorsPerPalette; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                    BotwoonSourceDefinitions.HealthColors +
                    (band * BotwoonColorFormat.ColorsPerPalette + color) * 2),
                native.HealthColor(BotwoonHealthBandDefinitions.InOrderShown[band], color),
                $"installed Botwoon health band {band} color {color}");

        string file = Path.Combine(stockDirectory, BotwoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BotwoonColorDocument visual = JsonSerializer.Deserialize<BotwoonColorDocument>(
            stockJson, CaseInsensitiveJson) ??
            throw new InvalidDataException("Stock Botwoon color JSON is null.");
        BotwoonColorCatalog colors = native;

        var guard = NewBotwoonColorReadGuard(rom);
        var enemies = EnemySystemFixtures.New(EnemySessionFixtures.WithInstalledServices(stock.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance, installedBosses: stock.Bosses);
        // Botwoon's sprites are pre-coloured in each band's colours over his row, the seventh (#1512).
        IReadOnlyList<SuperMetroid.AssetExtraction.Artwork.SheetShade> shades =
            SuperMetroid.AssetExtraction.Bosses.BossSheetShades.OwnRow(stock.Bosses, EnemyDefinitionId.Botwoon, 7 * 16);
        Action<RoomEnemyPlace, BotwoonEnemyState> update = BotwoonBehavior.UpdateBotwoonHealthBand;
        RoomEnemyPlace head = enemies.Places.InUpdateOrder[0];
        var state = new BotwoonEnemyState(body: []);
        IReadOnlyList<BotwoonHealthBand> bands = BotwoonHealthBandDefinitions.InOrderShown;
        for (int band = 0; band < bands.Count; band++)
        {
            ushort threshold = RomDataReader.ReadWordFixedBank(CartridgeImportSource.Require(rom),
                BotwoonHealthBandDefinitions.NativeThresholdAddress + band * sizeof(ushort));
            state.HealthBand = band == 0 ? null : bands[band - 1];
            head.Health = threshold - 1;
            update(head, state);
            AssertEqual(bands[band], state.HealthBand,
                $"Botwoon health band {band} preserves native threshold progression");
            AssertEqual(PictureShading.Shaded(BotwoonShades.Health(bands[band])), state.BodyShading,
                $"live Botwoon draws in health band {band}");
            ushort[] cgram = BossShadeColours.Laid([BotwoonShades.Health(bands[band])], shades, BossShadeColours.Sprites, 0x1234);
            for (int color = 0; color < BotwoonColorFormat.ColorsPerPalette; color++)
                AssertEqual(colors.HealthColor(BotwoonHealthBandDefinitions.InOrderShown[band], color).ToWord(),
                    cgram[BotwoonHealthBandDefinitions.FirstColour + color],
                    $"live Botwoon health band {band} color {color}");
            AssertEqual(0x1234,
                cgram[BotwoonHealthBandDefinitions.FirstColour - 1],
                $"Botwoon health band {band} preserves adjacent palette");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Botwoon health palette avoids migrated RGB5 ROM reads");

        visual.Health[0][5] = visual.Health[0][5] with { Green = 32 };
        AssertThrows<InvalidDataException>(() => BotwoonColorFile.Read(new MemoryStream(BotwoonColorFile.Write(visual), writable: false)),
            "reading Botwoon colors rejects an RGB5 channel outside five-bit precision");
        Console.WriteLine("  Botwoon colors: 128 native RGB5 words, eight live health bands, preserved threshold progression, ROM guard and strict failures pass.");
    }

    /// <summary>Guards the migrated Botwoon palette range against live cartridge reads.</summary>
    internal static CartridgeReadGuard NewBotwoonColorReadGuard(ISnesAddressSpace source) =>
        new CartridgeReadGuard(source).ForbidRange(BotwoonSourceDefinitions.HealthColors,
            BotwoonColorFormat.HealthBandCount *
                BotwoonColorFormat.ColorsPerPalette * sizeof(ushort),
            address => $"Live Botwoon read migrated color ROM ${address:X6}.");
}
