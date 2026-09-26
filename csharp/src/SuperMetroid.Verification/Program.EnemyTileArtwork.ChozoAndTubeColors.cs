using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

internal static partial class Program
{
    private static void VerifyInstalledChozoAndTubeColors(
        ISnesAddressSpace rom, string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        ChozoAndTubeColorCatalog native = stock.ChozoAndTubeColors ??
            throw new InvalidDataException("Installed enemy art has no Chozo/tube colors.");
        VerifyBand(ChozoAndTubeColorRomData.TubeCracksSource,
            native.ResolveTubeCracks, "tube cracks");
        VerifyBand(ChozoAndTubeColorRomData.WreckedShipSource,
            native.ResolveWreckedShip, "Wrecked Ship Chozo");
        VerifyBand(ChozoAndTubeColorRomData.LowerNorfairSource,
            native.ResolveLowerNorfair, "Lower Norfair Chozo");

        string file = Path.Combine(stockDirectory, ChozoAndTubeColorFormat.FileName);
        byte[] stockJson = File.ReadAllBytes(file);
        ChozoAndTubeColorDocument visual =
            JsonSerializer.Deserialize<ChozoAndTubeColorDocument>(stockJson,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true }) ??
            throw new InvalidDataException("Stock Chozo/tube color JSON is null.");
        visual.TubeCracks[5] = ChangeRed(visual.TubeCracks[5]);
        visual.WreckedShip[5] = ChangeRed(visual.WreckedShip[5]);
        visual.LowerNorfair[5] = ChangeRed(visual.LowerNorfair[5]);
        string overrides = Path.Combine(stockDirectory, "chozo-tube-color-overrides");
        Directory.CreateDirectory(overrides);
        string overrideFile = Path.Combine(overrides, ChozoAndTubeColorFormat.FileName);
        File.WriteAllBytes(overrideFile, ChozoAndTubeColorCatalog.Write(visual));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        ChozoAndTubeColorCatalog colors = edited.ChozoAndTubeColors ??
            throw new InvalidDataException("Edited enemy art has no Chozo/tube colors.");
        CheckEditedBand(native.ResolveTubeCracks, colors.ResolveTubeCracks, "tube cracks");
        CheckEditedBand(native.ResolveWreckedShip, colors.ResolveWreckedShip,
            "Wrecked Ship Chozo");
        CheckEditedBand(native.ResolveLowerNorfair, colors.ResolveLowerNorfair,
            "Lower Norfair Chozo");
        AssertEqual(colors.ResolveWreckedShip(5),
            EnemyTileArtworkFiles.Load(stockDirectory, overrides)
                .ChozoAndTubeColors!.ResolveWreckedShip(5),
            "Chozo/tube override survives catalog reload");

        // Every live initializer must use the installed data, while variant selection,
        // instruction identity and native PLM requests remain engine-owned.
        BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        Type type = typeof(RoomEnemySystem);
        var guarded = new ChozoAndTubePaletteReadGuard(rom);
        VerifyLive((ushort)0, colors.ResolveTubeCracks,
            "n00b-tube cracks", "InitializeN00bTubeCracks", null);
        VerifyLive((ushort)0, colors.ResolveWreckedShip,
            "Wrecked Ship Chozo", "InitializeChozoStatue",
            ChozoStatueInstructionProgramDefinitions.WreckedShipInitial);
        VerifyLive((ushort)2, colors.ResolveLowerNorfair,
            "Lower Norfair Chozo", "InitializeChozoStatue",
            ChozoStatueInstructionProgramDefinitions.LowerNorfairInitial);

        File.WriteAllBytes(overrideFile, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "invalid Chozo/tube override fails loudly");
        File.WriteAllBytes(overrideFile,
            "{\"version\":1,\"version\":1}"u8.ToArray());
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "duplicate Chozo/tube color property fails loudly");
        visual.TubeCracks[5] = visual.TubeCracks[5] with { Blue = 32 };
        AssertThrows<InvalidDataException>(() => ChozoAndTubeColorCatalog.Write(visual),
            "Chozo/tube RGB5 channel outside five-bit precision is rejected");
        File.Delete(overrideFile);
        File.WriteAllBytes(file, [0]);
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "corrupt stock Chozo/tube colors fail manifest hash validation");
        File.WriteAllBytes(file, stockJson);
        Console.WriteLine("  Chozo/tube colors: 96 native RGB5 words, all three live initializers, persistent overrides, ROM guard, and strict failures pass.");

        void VerifyBand(int source, Func<int, ushort> resolve, string name)
        {
            for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
                AssertEqual(RomDataReader.ReadWordFixedBank(rom,
                    source + color * sizeof(ushort)), resolve(color),
                    $"installed {name} color {color} preserves native RGB5");
        }

        void VerifyLive(ushort variant, Func<int, ushort> resolve, string name,
            string initializer, ushort? expectedInstruction)
        {
            var cgram = new SnesCgram();
            var enemies = new RoomEnemySystem { TileArtwork = edited };
            type.GetField("_bus", flags)!.SetValue(enemies, guarded);
            type.GetField("_cgram", flags)!.SetValue(enemies, cgram);
            RoomEnemySlot statue = enemies.Slots[0];
            statue.Parameter2 = variant;
            type.GetMethod(initializer, flags)!.Invoke(enemies,
                expectedInstruction is null ? null : [statue]);
            for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
                AssertEqual(resolve(color),
                    cgram.Colors[ChozoAndTubeColorRomData.Destination + color],
                    $"live {name} CGRAM color {color}");
            if (expectedInstruction is not null)
            {
                AssertEqual(expectedInstruction.Value, statue.CurrentInstruction,
                    $"{name} keeps native instruction selection");
                AssertTrue(enemies.ChozoStatuePlmRequests.Count != 0,
                    $"{name} still publishes its native PLM request");
            }
        }

        static PaletteRgb5 ChangeRed(PaletteRgb5 rgb) => rgb with
        {
            Red = rgb.Red == 31 ? 30 : rgb.Red + 1,
        };

        static void CheckEditedBand(Func<int, ushort> original,
            Func<int, ushort> changed, string name)
        {
            AssertTrue(original(5) != changed(5),
                $"{name} edit changes selected RGB5 word");
            for (int color = 0; color < ChozoAndTubeColorRomData.ColorCount; color++)
                if (color != 5)
                    AssertEqual(original(color), changed(color),
                        $"{name} edit leaves color {color} unchanged");
        }
    }

    private sealed class ChozoAndTubePaletteReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        public byte ReadByte(int address) =>
            address >= ChozoAndTubeColorRomData.TubeCracksSource &&
            address < ChozoAndTubeColorRomData.LowerNorfairSource +
                ChozoAndTubeColorRomData.ColorCount * sizeof(ushort)
                ? throw new InvalidOperationException(
                    $"Chozo/tube accessed migrated palette ROM ${address:X6}.")
                : source.ReadByte(address);

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
