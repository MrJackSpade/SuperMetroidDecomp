using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledBotwoonColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        BotwoonColorCatalog native = stock.BotwoonColors ??
            throw new InvalidDataException("Installed enemy art has no Botwoon colors.");
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        for (int color = 0; color < BotwoonHealthPaletteDefinitions.ColorsPerPalette; color++)
            AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    BotwoonHealthPaletteDefinitions.NativePaletteAddress +
                    (band * BotwoonHealthPaletteDefinitions.ColorsPerPalette + color) * 2),
                native.HealthColor(band, color),
                $"installed Botwoon health band {band} color {color}");

        string file = Path.Combine(stockDirectory, BotwoonColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        BotwoonColorDocument visual = JsonSerializer.Deserialize<BotwoonColorDocument>(
            stockJson, new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Botwoon color JSON is null.");
        foreach (PaletteRgb5[] band in visual.Health)
            band[5] = ChangeRed(band[5]);
        string overrides = Path.Combine(stockDirectory, "botwoon-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, BotwoonColorFormat.FileName);
        File.WriteAllBytes(overrideFile, BotwoonColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        BotwoonColorCatalog colors = edited.BotwoonColors ??
            throw new InvalidDataException("Edited enemy art has no Botwoon colors.");
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        {
            AssertTrue(native.HealthColor(band, 5) != colors.HealthColor(band, 5),
                $"Botwoon health band {band} edit takes effect");
            AssertEqual(native.HealthColor(band, 4), colors.HealthColor(band, 4),
                $"Botwoon health band {band} adjacent color remains native");
        }
        AssertEqual(colors.HealthColor(7, 5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .BotwoonColors!.HealthColor(7, 5),
            "Botwoon color override survives catalog reload");

        var guard = new BotwoonColorReadGuard(rom);
        var cgram = new SnesCgram();
        var enemies = new RoomEnemySystem { TileArtwork = edited };
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
        typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
        var update = typeof(RoomEnemySystem).GetMethod(
            "UpdateBotwoonHealthPalette", flags)!
            .CreateDelegate<Action<RoomEnemySlot, BotwoonEnemyState>>(enemies);
        RoomEnemySlot head = enemies.Slots[0];
        var state = new BotwoonEnemyState(head)
        {
            PaletteDestinationByteOffset =
                BotwoonHealthPaletteDefinitions.DestinationColor * sizeof(ushort),
        };
        cgram.SetColor(BotwoonHealthPaletteDefinitions.DestinationColor - 1, 0x1234);
        for (int band = 0; band < BotwoonHealthPaletteDefinitions.PaletteCount; band++)
        {
            ushort phase = (ushort)(band * sizeof(ushort));
            ushort threshold = RomDataReader.ReadWordFixedBank(rom,
                BotwoonHealthPaletteDefinitions.NativeThresholdAddress + phase);
            state.PalettePhaseByteOffset = phase;
            head.Health = unchecked((ushort)(threshold - 1));
            update(head, state);
            AssertEqual((ushort)(phase + 2), state.PalettePhaseByteOffset,
                $"Botwoon health band {band} preserves native threshold progression");
            for (int color = 0; color < BotwoonHealthPaletteDefinitions.ColorsPerPalette; color++)
                AssertEqual(colors.HealthColor(band, color),
                    cgram.Colors[BotwoonHealthPaletteDefinitions.DestinationColor + color],
                    $"live Botwoon health band {band} color {color}");
            AssertEqual((ushort)0x1234,
                cgram.Colors[BotwoonHealthPaletteDefinitions.DestinationColor - 1],
                $"Botwoon health band {band} preserves adjacent palette");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "live Botwoon health palette avoids migrated RGB5 ROM reads");
        state.PalettePhaseByteOffset = 0;
        state.PaletteDestinationByteOffset = 0x01c0;
        head.Health = 0;
        AssertThrows<InvalidDataException>(() => update(head, state),
            "installed Botwoon rejects a non-retail palette destination");

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
        AssertThrows<InvalidDataException>(() => BotwoonColorCatalog.Write(visual),
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

    private sealed class BotwoonColorReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= BotwoonHealthPaletteDefinitions.NativePaletteAddress &&
                address < BotwoonHealthPaletteDefinitions.NativePaletteAddress +
                    BotwoonHealthPaletteDefinitions.PaletteCount *
                    BotwoonHealthPaletteDefinitions.ColorsPerPalette * sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Live Botwoon read migrated color ROM ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
