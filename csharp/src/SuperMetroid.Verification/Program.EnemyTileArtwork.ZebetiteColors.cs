using System.Reflection;
using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledZebetiteColors(ISnesAddressSpace bus,
        string stockDirectory, EnemyTileArtworkCatalog stock)
    {
        if (stock.ZebetiteColors is null)
            throw new InvalidDataException("Installed enemy artwork lacks Zebetite pulse colors.");
        (RoomEnemySystem native, SnesCgram nativeCgram) = Create(bus, null);
        var guard = new ZebetiteColorReadGuard(bus);
        (RoomEnemySystem installed, SnesCgram installedCgram) = Create(guard, stock);
        for (int call = 0; call < ZebetiteColorFormat.FrameCount * 2; call++)
        {
            Cycle(native);
            Cycle(installed);
            AssertEqual((ushort)((call + 1) & ZebetiteDefinitions.PaletteCycleMask),
                installed.Slots[0].VariableC,
                "Installed Zebetite pulse retains physical-slot-zero counter and wrap");
            AssertEqual(native.Slots[0].VariableC, installed.Slots[0].VariableC,
                "Installed Zebetite pulse keeps native frame selection");
            AssertTrue(nativeCgram.Colors.SequenceEqual(installedCgram.Colors),
                "Installed Zebetite pulse preserves full native CGRAM and neighbors");
        }
        AssertEqual(0, guard.ForbiddenReadAttempts,
            "Installed Zebetite pulse never reads migrated ROM colors");

        string stockPath = Path.Combine(stockDirectory, ZebetiteColorFormat.FileName);
        byte[] stockBytes = File.ReadAllBytes(stockPath);
        string overrides = Path.Combine(stockDirectory, "zebetite-color-overrides");
        Directory.CreateDirectory(overrides);
        string overridePath = Path.Combine(overrides, ZebetiteColorFormat.FileName);
        JsonNode editedDocument = JsonNode.Parse(stockBytes)!;
        JsonNode red = editedDocument["frames"]![3]![1]!["red"]!;
        editedDocument["frames"]![3]![1]!["red"] = red.GetValue<int>() ^ 1;
        byte[] selectedBytes = System.Text.Encoding.UTF8.GetBytes(editedDocument.ToJsonString());
        File.WriteAllBytes(overridePath, selectedBytes);
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        (RoomEnemySystem selected, SnesCgram selectedCgram) = Create(guard, edited);
        (RoomEnemySystem control, SnesCgram controlCgram) = Create(bus, null);
        for (int call = 0; call < 2; call++)
        {
            Cycle(selected);
            Cycle(control);
            AssertTrue(controlCgram.Colors.SequenceEqual(selectedCgram.Colors),
                "Zebetite edit leaves preceding pulse frames unchanged");
        }
        Cycle(selected);
        Cycle(control);
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            AssertEqual((ushort)(controlCgram.Colors[color] ^
                (color == ZebetiteDefinitions.PaletteDestinationColor + 1 ? 1 : 0)),
                selectedCgram.Colors[color],
                "Zebetite visual edit changes only one color channel at the selected frame");
        AssertEqual(control.Slots[0].VariableC, selected.Slots[0].VariableC,
            "Zebetite color edit does not change animation counter");
        selected.PaletteChangeNumber = 1;
        ushort counterBeforeGate = selected.Slots[0].VariableC;
        Cycle(selected);
        AssertEqual(counterBeforeGate, selected.Slots[0].VariableC,
            "Zebetite palette-change gate remains an engine-owned condition");
        selected.PaletteChangeNumber = 0;
        selected.Slots[0].Parameter1 = 1;
        Cycle(selected);
        AssertEqual(counterBeforeGate, selected.Slots[0].VariableC,
            "Zebetite secondary-half gate remains an engine-owned condition");

        EnemyTileArtworkFiles.Extract(bus, stockDirectory, SupportedCartridge.Sha256);
        AssertTrue(selectedBytes.SequenceEqual(File.ReadAllBytes(overridePath)),
            "Stock re-extraction preserves the Zebetite color override");
        var reloaded = EnemyTileArtworkFiles.Load(stockDirectory, overrides);
        var persistedCgram = new SnesCgram();
        reloaded.ZebetiteColors!.Apply(persistedCgram, 3,
            ZebetiteDefinitions.PaletteDestinationColor);
        AssertEqual(selectedCgram.Colors[ZebetiteDefinitions.PaletteDestinationColor + 1],
            persistedCgram.Colors[ZebetiteDefinitions.PaletteDestinationColor + 1],
            "Reload retains edited Zebetite pulse color");

        File.WriteAllText(overridePath, "broken");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Corrupt Zebetite color override fails loudly");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1}");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, overrides),
            "Duplicate Zebetite color property fails loudly");
        var invalid = JsonNode.Parse(stockBytes)!;
        invalid["frames"]![0]![0]!["green"] = 32;
        AssertThrows<InvalidDataException>(() => ZebetiteColorCatalog.Load(
            new MemoryStream(System.Text.Encoding.UTF8.GetBytes(invalid.ToJsonString()))),
            "Out-of-range Zebetite RGB5 component rejected");
        File.Delete(overridePath);
        File.WriteAllText(stockPath, "broken stock");
        AssertThrows<InvalidDataException>(() => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "Corrupt stock Zebetite colors fail manifest validation");
        File.WriteAllBytes(stockPath, stockBytes);
        Console.WriteLine("  Zebetite colors: 16 native RGB5 words, sixteen live pulse calls, full CGRAM, ROM guard, isolated edit, gates, persistence and strict failures pass.");

        static (RoomEnemySystem Enemies, SnesCgram Cgram) Create(
            ISnesAddressSpace source, EnemyTileArtworkCatalog? artwork)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            var enemies = new RoomEnemySystem { TileArtwork = artwork };
            var cgram = new SnesCgram();
            for (int color = 0; color < SnesCgram.ColorCount; color++)
                cgram.SetColor(color, (ushort)(color * 31));
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, source);
            typeof(RoomEnemySystem).GetField("_cgram", flags)!.SetValue(enemies, cgram);
            return (enemies, cgram);
        }

        static void Cycle(RoomEnemySystem enemies)
        {
            const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
            typeof(RoomEnemySystem).GetMethod("CycleZebetitePalette", flags)!
                .Invoke(enemies, [enemies.Slots[0]]);
        }
    }

    private sealed class ZebetiteColorReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            if (address >= ZebetiteDefinitions.PaletteSource &&
                address < ZebetiteDefinitions.PaletteSource +
                    ZebetiteColorFormat.FrameCount * ZebetiteColorFormat.ColorsPerFrame *
                    sizeof(ushort))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Installed Zebetite pulse read migrated ROM color ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
