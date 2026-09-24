using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledEnemySpritemaps(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        AssertTrue(stock.Spritemaps is not null,
            "installed enemy catalog contains named OAM compositions");
        AssertTrue(!EnemySpritemapDefinitions.TryFrameAt(0xffff, 0xe312, out _),
            "unknown enemy family keeps the existing cartridge selector path");
        foreach (EnemySpritemapDefinition frame in EnemySpritemapDefinitions.Frames)
        {
            AssertTrue(stock.Spritemaps!.TryGet(frame.Bank, frame.Pointer, out var parts),
                $"installed enemy frame {frame.Name} exists");
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
            var room = DrawEnemy(stock, new FrameReadGuard(rom), frame.Pointer,
                frame.Name.StartsWith("boyon_", StringComparison.Ordinal)
                    ? RoomEnemySystem.BoyonDefinition
                    : frame.Name.StartsWith("cacatac_", StringComparison.Ordinal)
                        ? RoomEnemySystem.CacatacDefinition
                        : frame.Name.StartsWith("boulder_", StringComparison.Ordinal)
                            ? RoomEnemySystem.BoulderDefinition
                            : frame.Name.StartsWith("skultera_", StringComparison.Ordinal)
                                ? RoomEnemySystem.SkulteraDefinition
                                : frame.Name.StartsWith("waver_", StringComparison.Ordinal)
                                    ? RoomEnemySystem.WaverDefinition
                                : RoomEnemySystem.AtomicDefinition);
            var nativeRoom = new OamBuffer();
            nativeRoom.AddEnemySpritemap(rom, frame.Bank, frame.Pointer,
                0x0040, 0x0080, 0, 0);
            AssertTrue(room.LowTable.SequenceEqual(nativeRoom.LowTable) &&
                       room.HighTable.SequenceEqual(nativeRoom.HighTable),
                $"production room draws installed {frame.Name} without visual ROM reads");
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
        SpriteVisualPart cacatacPart = document.Frames["cacatac_upright_idle_0"][0];
        document.Frames["cacatac_upright_idle_0"][0] = cacatacPart with
        {
            OffsetY = cacatacPart.OffsetY + 1,
        };
        SpriteVisualPart boulderPart = document.Frames["boulder_roll_0"][0];
        document.Frames["boulder_roll_0"][0] = boulderPart with
        {
            OffsetY = boulderPart.OffsetY + 1,
        };
        SpriteVisualPart atomicPart = document.Frames["atomic_up_right_0"][0];
        document.Frames["atomic_up_right_0"][0] = atomicPart with
        {
            OffsetY = atomicPart.OffsetY + 1,
        };
        SpriteVisualPart skulteraPart = document.Frames["skultera_swim_left_0"][0];
        document.Frames["skultera_swim_left_0"][0] = skulteraPart with
        {
            OffsetY = skulteraPart.OffsetY + 1,
        };
        SpriteVisualPart waverPart = document.Frames["waver_steady_left"][0];
        document.Frames["waver_steady_left"][0] = waverPart with
        {
            OffsetY = waverPart.OffsetY + 1,
        };
        string overrideDirectory = Path.Combine(stockDirectory, "spritemap-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory, fileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        ushort framePointer = EnemySpritemapDefinitions.BoyonFrameAt(0x86ad);
        var stockOam = DrawEnemy(stock, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        var editedOam = DrawEnemy(edited, new FrameReadGuard(rom), framePointer,
            RoomEnemySystem.BoyonDefinition);
        AssertEqual(unchecked((byte)(stockOam.LowTable[0] + 1)),
            editedOam.LowTable[0], "authored Boyon X offset changes live room OAM");
        AssertEqual(unchecked((byte)(stockOam.LowTable[2] + 1)),
            editedOam.LowTable[2], "authored Boyon tile changes live room OAM");
        AssertEqual(stockOam.LowTable[1], editedOam.LowTable[1],
            "visual override leaves Boyon Y unchanged");
        ushort cacatacPointer = EnemySpritemapDefinitions.CacatacFrameAt(0x9e8e);
        var stockCacatac = DrawEnemy(stock, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        var editedCacatac = DrawEnemy(edited, new FrameReadGuard(rom),
            cacatacPointer, RoomEnemySystem.CacatacDefinition);
        AssertEqual(unchecked((byte)(stockCacatac.LowTable[1] + 1)),
            editedCacatac.LowTable[1],
            "authored Cacatac Y offset changes live room OAM");
        AssertEqual(stockCacatac.LowTable[0], editedCacatac.LowTable[0],
            "Cacatac visual override leaves X unchanged");
        ushort boulderPointer = EnemySpritemapDefinitions.BoulderFrameAt(0x86a9);
        var stockBoulder = DrawEnemy(stock, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        var editedBoulder = DrawEnemy(edited, new FrameReadGuard(rom),
            boulderPointer, RoomEnemySystem.BoulderDefinition);
        AssertEqual(unchecked((byte)(stockBoulder.LowTable[1] + 1)),
            editedBoulder.LowTable[1],
            "authored Boulder Y offset changes live room OAM");
        AssertEqual(stockBoulder.LowTable[0], editedBoulder.LowTable[0],
            "Boulder visual override leaves X unchanged");
        ushort atomicPointer = EnemySpritemapDefinitions.AtomicFrameAt(0xe312);
        var stockAtomic = DrawEnemy(stock, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        var editedAtomic = DrawEnemy(edited, new FrameReadGuard(rom),
            atomicPointer, RoomEnemySystem.AtomicDefinition);
        AssertEqual(unchecked((byte)(stockAtomic.LowTable[1] + 1)),
            editedAtomic.LowTable[1],
            "authored Atomic Y offset changes live room OAM");
        AssertEqual(stockAtomic.LowTable[0], editedAtomic.LowTable[0],
            "Atomic visual override leaves X unchanged");
        ushort skulteraPointer = EnemySpritemapDefinitions.SkulteraFrameAt(0x902e);
        var stockSkultera = DrawEnemy(stock, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        var editedSkultera = DrawEnemy(edited, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(unchecked((byte)(stockSkultera.LowTable[1] + 1)),
            editedSkultera.LowTable[1],
            "authored Skultera Y offset changes live room OAM");
        AssertEqual(stockSkultera.LowTable[0], editedSkultera.LowTable[0],
            "Skultera visual override leaves X unchanged");
        ushort waverPointer = EnemySpritemapDefinitions.WaverFrameAt(0x86a9);
        var stockWaver = DrawEnemy(stock, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        var editedWaver = DrawEnemy(edited, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertEqual(unchecked((byte)(stockWaver.LowTable[1] + 1)),
            editedWaver.LowTable[1],
            "authored Waver Y offset changes live room OAM");
        AssertEqual(stockWaver.LowTable[0], editedWaver.LowTable[0],
            "Waver visual override leaves X unchanged");
        AssertTrue(EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory)
                .Spritemaps!.TryGet(EnemySpritemapDefinitions.BoyonBank, framePointer, out _),
            "enemy composition override survives catalog reload");
        var previousFrames = document.Frames
            .Where(pair => !pair.Key.StartsWith("waver_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.PreviousFrameCount,
            previousFrames.Count, "previous enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.PreviousVersion,
                Frames = previousFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog upgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var upgradedBoyon = DrawEnemy(upgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var upgradedSkultera = DrawEnemy(upgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], upgradedBoyon.LowTable[0],
            "previous-version override retains edited Boyon composition");
        AssertEqual(editedSkultera.LowTable[1], upgradedSkultera.LowTable[1],
            "previous-version override retains edited Skultera composition");
        var upgradedWaver = DrawEnemy(upgraded, new FrameReadGuard(rom),
            waverPointer, RoomEnemySystem.WaverDefinition);
        AssertTrue(stockWaver.LowTable.SequenceEqual(upgradedWaver.LowTable),
            "previous-version override gains stock Waver composition");
        var legacyFrames = previousFrames
            .Where(pair => !pair.Key.StartsWith("skultera_", StringComparison.Ordinal))
            .ToDictionary(pair => pair.Key, pair => pair.Value, StringComparer.Ordinal);
        AssertEqual(EnemySpritemapDefinitions.LegacyFrameCount,
            legacyFrames.Count, "legacy enemy composition schema frame count");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(
            new EnemySpritemapDocument
            {
                Version = EnemySpritemapDefinitions.LegacyVersion,
                Frames = legacyFrames,
            }, new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog legacyUpgraded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        var legacyBoyon = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            framePointer, RoomEnemySystem.BoyonDefinition);
        var legacySkultera = DrawEnemy(legacyUpgraded, new FrameReadGuard(rom),
            skulteraPointer, RoomEnemySystem.SkulteraDefinition);
        AssertEqual(editedOam.LowTable[0], legacyBoyon.LowTable[0],
            "legacy override retains edited Boyon composition");
        AssertTrue(stockSkultera.LowTable.SequenceEqual(legacySkultera.LowTable),
            "legacy override gains stock Skultera composition");
        File.WriteAllText(overridePath, "{\"version\":1,\"version\":1,\"frames\":{}}");
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "duplicate enemy composition keys fail loudly");
        File.WriteAllBytes(overridePath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory),
            "malformed enemy composition override fails loudly");

        static OamBuffer DrawEnemy(EnemyTileArtworkCatalog art,
            ISnesAddressSpace guard, ushort pointer, ushort definition)
        {
            var enemies = new RoomEnemySystem { TileArtwork = art };
            typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, guard);
            var queues = (List<ushort>[])typeof(RoomEnemySystem)
                .GetField("_drawQueues", flags)!.GetValue(enemies)!;
            queues[0].Add(0);
            RoomEnemySlot slot = enemies.Slots[0];
            slot.EnemyDefinitionPointer = definition;
            slot.Definition = default(RoomEnemyDefinition) with
                { Bank = definition == RoomEnemySystem.BoulderDefinition
                    ? EnemySpritemapDefinitions.BoulderBank
                    : definition == RoomEnemySystem.AtomicDefinition
                        ? EnemySpritemapDefinitions.AtomicBank
                        : definition == RoomEnemySystem.SkulteraDefinition ||
                          definition == RoomEnemySystem.WaverDefinition
                            ? EnemySpritemapDefinitions.SkulteraBank
                        : EnemySpritemapDefinitions.BoyonBank };
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
            if (address is >= 0xa288da and < 0xa2890b or
                >= 0xa2a0bb and < 0xa2a377 or
                >= 0xa68a59 and < 0xa68b09 or
                >= 0xa8e489 and < 0xa8e587 or
                >= 0xa3881e and < 0xa388f0 or
                >= 0xa3928a and < 0xa394aa)
                throw new InvalidOperationException(
                    $"Installed enemy draw read native visual byte ${address:X6}.");
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
