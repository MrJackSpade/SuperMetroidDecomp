using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledBoyonSpritemaps(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertTrue(stock.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue(stock.Spritemaps!.TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed Boyon frame {frame.Name} exists");
            foreach ((ushort x, ushort y, ushort palette, ushort baseTile) in
                     new (ushort, ushort, ushort, ushort)[]
                     {
                         (0x0010, 0x0020, 0, 0),
                         (0x01f8, 0x00fc, 0x0c00, 0x01ff),
                         (0x0000, 0x0000, 0x0400, 0x0010),
                     })
            {
                var native = new OamBuffer();
                var installed = new OamBuffer();
                native.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                    x, y, palette, baseTile);
                installed.AddEnemySpritemap(parts.Span, x, y, palette, baseTile);
                AssertTrue(native.LowTable.SequenceEqual(installed.LowTable) &&
                           native.HighTable.SequenceEqual(installed.HighTable) &&
                           native.NextByteOffset == installed.NextByteOffset,
                    $"installed {frame.Name} OAM matches native at {x:X4},{y:X4}");
            }
        }

        string fileName = EnemySpritemapDefinitions.FileName;
        string stockPath = Path.Combine(stockDirectory, fileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock enemy compositions are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemySpritemapDocument document = JsonSerializer.Deserialize<EnemySpritemapDocument>(
            original, new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        SpriteVisualPart originalPart = document.Frames["boyon_idle_0"][0];
        document.Frames["boyon_idle_0"][0] = originalPart with
        {
            OffsetX = originalPart.OffsetX + 1,
            TileColumn = originalPart.TileColumn + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory, "spritemap-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort framePointer = EnemySpritemapDefinitions.BoyonFrameAt(0x86ad);
        var stockOam = DrawBoyon(stock, new FrameReadGuard(rom), framePointer);
        var editedOam = DrawBoyon(edited, new FrameReadGuard(rom), framePointer);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0], "authored Boyon X offset changes live room OAM");
        AssertEqual(unchecked((byte)(stockOam.LowTable[2] + 1)),
            editedOam.LowTable[2], "authored Boyon tile changes live room OAM");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "visual override leaves Boyon Y unchanged");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGet(EnemySpritemapDefinitions.BoyonBank, framePointer, out _),
            "enemy composition override survives catalog reload");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1,\"frames\":{}}");
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "duplicate enemy composition keys fail loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed enemy composition override fails loudly");

        static OamBuffer DrawBoyon(EnemyTileArtworkCatalog art,
            ISnesAddressSpace guard, ushort pointer)
        {
            var enemies = new RoomEnemySystem { TileArtwork = art };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var queues = (List<ushort>[])typeof(RoomEnemySystem)
                .GetField("_drawQueues", flags)!.GetValue(enemies)!;
            queues[0].Add(0);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = RoomEnemySystem.BoyonDefinition;
            slot.Definition = default(RoomEnemyDefinition) with
                { Bank = EnemySpritemapDefinitions.BoyonBank };
            slot.SpritemapPointer = pointer;
            slot.XPosition = 0x0040;
            slot.YPosition = 0x0080;
            var oam = new OamBuffer();
            enemies.DrawLayers(oam, 0, 0, 0, 0);
            return oam;
        }
    }

    private sealed class FrameReadGuard(ISnesAddressSpace source) : ISnesAddressSpace
    {
        public byte ReadByte(int address)
        {
            if (address is >= 0xa288da and < 0xa2890b)
                throw new InvalidOperationException(
                    $"Installed Boyon draw read native visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
