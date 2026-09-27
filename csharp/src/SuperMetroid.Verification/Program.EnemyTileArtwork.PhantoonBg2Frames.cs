using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledPhantoonBg2Frames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.PhantoonBg2Frames is not null,
            "installed enemy art includes Phantoon BG2 frames");
        var identities = new HashSet<ushort>();
        foreach (PhantoonBg2FrameDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            AssertTrue(identities.Add(frame.Pointer),
                $"Phantoon frame {frame.Name} has a distinct physical selector");
            AssertTrue(stock.PhantoonBg2Frames!.TryGet(frame.Pointer,
                    out ReadOnlyMemory<PhantoonBg2TilemapWrite> installed),
                $"installed Phantoon BG2 frame {frame.Name} exists");
            var native = new List<(ushort Destination, ushort[] Tiles)>();
            int root = (PhantoonBg2FrameDefinitions.Bank << 16) | frame.Pointer;
            int componentCount = rom.ReadByte(root);
            for (int component = 0; component < componentCount; component++)
            {
                ushort stream = ReadWord(root + 2 + component * 8 + 4);
                int cursor = (PhantoonBg2FrameDefinitions.Bank << 16) |
                    unchecked((ushort)(stream + 2));
                for (int command = 0; command < 128; command++)
                {
                    ushort destination = ReadWord(cursor);
                    if (destination == 0xffff)
                        break;
                    int count = ReadWord(cursor + 2);
                    var tiles = new ushort[count];
                    for (int tile = 0; tile < count; tile++)
                        tiles[tile] = ReadWord(cursor + 4 + tile * 2);
                    native.Add((destination, tiles));
                    cursor += 4 + count * 2;
                }
            }
            AssertEqual(native.Count, installed.Length,
                $"Phantoon {frame.Name} preserves native BG2 write order");
            for (int index = 0; index < native.Count; index++)
            {
                PhantoonBg2TilemapWrite write = installed.Span[index];
                AssertEqual(native[index].Destination,
                    checked((ushort)(PhantoonBg2FrameDefinitions.WorkingRamBase +
                        write.DestinationWord * 2)),
                    $"Phantoon {frame.Name} write {index} preserves BG2 destination");
                AssertTrue(write.Tiles.Span.SequenceEqual(native[index].Tiles),
                    $"Phantoon {frame.Name} write {index} preserves native tiles");
            }
        }

        string stockPath = Path.Combine(stockDirectory, PhantoonBg2FrameDefinitions.FileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock Phantoon BG2 frames are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        PhantoonBg2FrameDocument document =
            JsonSerializer.Deserialize<PhantoonBg2FrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        const string frameName = "body_invulnerable";
        PhantoonBg2WriteDocument first = document.Frames[frameName][0];
        int originalTile = first.Tiles[0];
        int[] editedTiles = (int[])first.Tiles.Clone();
        editedTiles[0] ^= 1;
        document.Frames[frameName][0] = first with { Tiles = editedTiles };
        string overrideDirectory = Path.Combine(stockDirectory, "phantoon-bg2-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory,
            PhantoonBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath,
            JsonSerializer.SerializeToUtf8Bytes(document,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(edited.PhantoonBg2Frames!.TryGet(
            PhantoonBg2FrameDefinitions.Frames[0].Pointer,
            out ReadOnlyMemory<PhantoonBg2TilemapWrite> editedFrame),
            "Phantoon BG2 edited frame is installed");
        AssertEqual((ushort)(originalTile ^ 1), editedFrame.Span[0].Tiles.Span[0],
            "Phantoon BG2 override changes the selected tile reference");
        AssertEqual((ushort)originalTile,
            stock.PhantoonBg2Frames!.TryGet(
                PhantoonBg2FrameDefinitions.Frames[0].Pointer,
                out ReadOnlyMemory<PhantoonBg2TilemapWrite> stockFrame)
                ? stockFrame.Span[0].Tiles.Span[0] : (ushort)0,
            "Phantoon BG2 override leaves stock frame intact");
        ushort firstPointer = PhantoonBg2FrameDefinitions.Frames[0].Pointer;
        byte[] nativeVram = DrawPhantoonBg2(null, rom, firstPointer,
            newInstructionFrame: true);
        var noCartridge = new FrontendCartridgeReadGuard(rom);
        byte[] stockVram = DrawPhantoonBg2(stock, noCartridge,
            firstPointer, newInstructionFrame: true);
        byte[] editedVram = DrawPhantoonBg2(edited, noCartridge,
            firstPointer, newInstructionFrame: true);
        AssertTrue(nativeVram.SequenceEqual(stockVram),
            "installed Phantoon frame draws native BG2 without cartridge reads");
        int changedVramByte = (PhantoonBg2FrameDefinitions.VramBase +
            editedFrame.Span[0].DestinationWord) * 2;
        AssertEqual((ushort)(originalTile ^ 1),
            BitConverter.ToUInt16(editedVram, changedVramByte),
            "Phantoon BG2 override changes the live rendered tile word");
        AssertTrue(DrawPhantoonBg2(edited, noCartridge,
                firstPointer, newInstructionFrame: false).All(value => value == 0),
            "Phantoon BG2 override retains the native new-frame producer gate");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(reloaded.PhantoonBg2Frames!.TryGet(
                PhantoonBg2FrameDefinitions.Frames[0].Pointer,
                out ReadOnlyMemory<PhantoonBg2TilemapWrite> reloadedFrame) &&
            reloadedFrame.Span[0].Tiles.Span[0] == (ushort)(originalTile ^ 1),
            "Phantoon BG2 override survives catalog reload");

        document.Frames[frameName][0] = first with { X = 32 };
        using var invalid = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(document,
                new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => PhantoonBg2FrameCatalog.Load(invalid),
            "Phantoon BG2 frame outside tilemap fails loudly");
        Console.WriteLine("  Phantoon BG2 frames: all 22 selectors preserve ordered native writes; override, reload and invalid-data checks passed.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private static byte[] DrawPhantoonBg2(EnemyTileArtworkCatalog? art,
        ISnesAddressSpace bus, ushort pointer, bool newInstructionFrame)
    {
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        var enemies = new RoomEnemySystem { TileArtwork = art };
        var vram = new SnesVram();
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies, bus);
        typeof(RoomEnemySystem).GetField("_vram", flags)!.SetValue(enemies, vram);
        var queues = (List<ushort>[])typeof(RoomEnemySystem)
            .GetField("_drawQueues", flags)!.GetValue(enemies)!;
        queues[0].Add(0);
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = 0xe4bf;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = PhantoonBg2FrameDefinitions.Bank,
        };
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        if (newInstructionFrame)
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.NewInstructionFrame);
        slot.SpritemapPointer = pointer;
        slot.XPosition = 0x0080;
        slot.YPosition = 0x0080;
        enemies.DrawLayers(new OamBuffer(), 0, 0, 0, 0);
        return vram.Bytes.ToArray();
    }
}
