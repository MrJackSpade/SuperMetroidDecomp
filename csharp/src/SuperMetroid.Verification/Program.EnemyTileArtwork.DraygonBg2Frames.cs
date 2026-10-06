using System.Reflection;
using System.Text.Json;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;

internal static partial class Program
{
    private static void VerifyInstalledDraygonBg2Frames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.DraygonBg2Frames is not null,
            "installed enemy art includes Draygon BG2 frames");
        var noCartridge = new FrontendCartridgeReadGuard(rom);
        int writeCount = 0;
        foreach (EnemyBg2FrameDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            AssertTrue(stock.DraygonBg2Frames!.TryGet(frame.Pointer,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"installed Draygon BG2 frame {frame.Name} exists");
            int root = (DraygonBg2FrameDefinitions.Bank << 16) | frame.Pointer;
            AssertEqual((byte)1, rom.ReadByte(root),
                $"Draygon {frame.Name} has one BG2 component");
            AssertEqual((ushort)0, ReadWord(root + 2),
                $"Draygon {frame.Name} BG2 component X offset");
            AssertEqual((ushort)0, ReadWord(root + 4),
                $"Draygon {frame.Name} BG2 component Y offset");
            ushort stream = ReadWord(root + 6);
            AssertEqual(EnemyBg2FrameLayout.StreamMarker,
                ReadWord((DraygonBg2FrameDefinitions.Bank << 16) | stream),
                $"Draygon {frame.Name} selects a BG2 command stream");
            int cursor = (DraygonBg2FrameDefinitions.Bank << 16) |
                unchecked((ushort)(stream + 2));
            var native = new List<(ushort Destination, ushort[] Tiles)>();
            for (int command = 0;
                 command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
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
            AssertEqual(native.Count, installed.Length,
                $"Draygon {frame.Name} preserves native BG2 write order");
            for (int index = 0; index < native.Count; index++)
            {
                EnemyBg2TilemapWrite write = installed.Span[index];
                AssertEqual(native[index].Destination,
                    checked((ushort)(EnemyBg2FrameLayout.WorkingRamBase +
                        write.DestinationWord * 2)),
                    $"Draygon {frame.Name} write {index} preserves destination");
                AssertTrue(write.Tiles.Span.SequenceEqual(native[index].Tiles),
                    $"Draygon {frame.Name} write {index} preserves tile words");
            }
            writeCount += native.Count;
            AssertTrue(ReadReferenceExtendedBg2Vram(rom, DraygonBg2FrameDefinitions.Bank, frame.Pointer, true)
                    .SequenceEqual(DrawDraygonBg2(stock, noCartridge,
                        frame.Pointer, true)),
                $"Draygon {frame.Name} stock VRAM matches native without cartridge reads");
        }
        AssertEqual(34, DraygonBg2FrameDefinitions.Frames.Length,
            "all selected Draygon BG2 frame identities are installed");
        AssertEqual(102, writeCount, "all Draygon BG2 command writes are installed");
        Suite(nameof(VerifyCompiledDraygonBg2Collision), () => VerifyCompiledDraygonBg2Collision(rom));
        Suite(nameof(VerifyCompiledDraygonOamCollision), () => VerifyCompiledDraygonOamCollision(rom));
        Suite(nameof(VerifyCompiledSporeSpawnCollision), () => VerifyCompiledSporeSpawnCollision(rom));

        string stockPath = Path.Combine(stockDirectory, DraygonBg2FrameDefinitions.FileName);
        byte[] original = File.ReadAllBytes(stockPath);
        File.WriteAllBytes(stockPath, [0]);
        AssertThrows<InvalidDataException>(
            () => EnemyTileArtworkFiles.Load(stockDirectory, null),
            "stock Draygon BG2 frames are manifest-hash checked");
        File.WriteAllBytes(stockPath, original);

        EnemyBg2FrameDocument document =
            JsonSerializer.Deserialize<EnemyBg2FrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true })!;
        EnemyBg2FrameDefinition editedIdentity = DraygonBg2FrameDefinitions.Frames[0];
        EnemyBg2WriteDocument first = document.Frames[editedIdentity.Name][0];
        int changedTile = first.Tiles[0] ^ 1;
        int[] editedTiles = (int[])first.Tiles.Clone();
        editedTiles[0] = changedTile;
        document.Frames[editedIdentity.Name][0] = first with { Tiles = editedTiles };
        string overrideDirectory = Path.Combine(stockDirectory, "draygon-bg2-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory,
            DraygonBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(edited.DraygonBg2Frames!.TryGet(editedIdentity.Pointer,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> editedFrame),
            "edited Draygon BG2 frame is installed");
        int vramByte = (EnemyBg2FrameLayout.VramBase +
            editedFrame.Span[0].DestinationWord) * 2;
        AssertEqual((ushort)changedTile,
            BitConverter.ToUInt16(DrawDraygonBg2(edited, noCartridge,
                editedIdentity.Pointer, true), vramByte),
            "Draygon visual override changes the live BG2 tile word");
        AssertTrue(DrawDraygonBg2(edited, noCartridge,
                editedIdentity.Pointer, false).All(value => value == 0),
            "Draygon override preserves the native new-frame producer gate");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue(reloaded.DraygonBg2Frames!.TryGet(editedIdentity.Pointer,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> reloadedFrame) &&
            reloadedFrame.Span[0].Tiles.Span[0] == (ushort)changedTile,
            "Draygon BG2 override survives reload");
        document.Frames[editedIdentity.Name][0] = first with { X = 32 };
        using var invalid = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            new JsonSerializerOptions { PropertyNamingPolicy = JsonNamingPolicy.CamelCase }));
        AssertThrows<InvalidDataException>(
            () => DraygonBg2FrameCatalog.Load(invalid),
            "Draygon BG2 write outside its tilemap fails loudly");
        Console.WriteLine("  Draygon BG2 frames: 34 selected frames and 102 native writes match installed VRAM; live edit, producer gate, ROM guard, reload, hash and strict validation pass.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private static void VerifyCompiledDraygonBg2Collision(
        SuperMetroidAddressSpace rom)
    {
        var nativeLists = new HashSet<ushort>();
        foreach (EnemyBg2FrameDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            int root = (DraygonBg2FrameDefinitions.Bank << 16) | frame.Pointer;
            var components =
                DraygonCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual((int)rom.ReadByte(root), components.Length,
                $"Draygon {frame.Name} compiled collision component count");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                DraygonCollisionComponent component = components[index];
                AssertEqual(ReadWord(record), unchecked((ushort)component.X),
                    $"Draygon {frame.Name} collision X offset");
                AssertEqual(ReadWord(record + 2), unchecked((ushort)component.Y),
                    $"Draygon {frame.Name} collision Y offset");
                AssertEqual(ReadWord(record + 6), component.HitboxPointer,
                    $"Draygon {frame.Name} hitbox list identity");
                nativeLists.Add(component.HitboxPointer);
            }
        }
        AssertEqual(3, nativeLists.Count,
            "Draygon BG2 frames have three distinct native hitbox lists");
        foreach (ushort pointer in nativeLists)
        {
            int list = (DraygonBg2FrameDefinitions.Bank << 16) | pointer;
            ReadOnlySpan<DraygonCollisionHitbox> hitboxes =
                DraygonCollisionDefinitions.HitboxesAt(pointer);
            AssertEqual((int)ReadWord(list), hitboxes.Length,
                $"Draygon hitbox list {pointer:X4} rectangle count");
            for (int index = 0; index < hitboxes.Length; index++)
            {
                int record = list + 2 + index * 12;
                DraygonCollisionHitbox box = hitboxes[index];
                AssertEqual(ReadWord(record), unchecked((ushort)box.Left),
                    $"Draygon list {pointer:X4} box {index} left");
                AssertEqual(ReadWord(record + 2), unchecked((ushort)box.Top),
                    $"Draygon list {pointer:X4} box {index} top");
                AssertEqual(ReadWord(record + 4), unchecked((ushort)box.Right),
                    $"Draygon list {pointer:X4} box {index} right");
                AssertEqual(ReadWord(record + 6), unchecked((ushort)box.Bottom),
                    $"Draygon list {pointer:X4} box {index} bottom");
                AssertEqual(ReadWord(record + 8), box.TouchAi,
                    $"Draygon list {pointer:X4} box {index} touch callback");
                AssertEqual(ReadWord(record + 10), box.ShotAi,
                    $"Draygon list {pointer:X4} box {index} shot callback");
            }
        }

        // Invoke the production overlap walker with all cartridge reads
        // denied. This checks the actual dispatch, not just a matching table.
        var enemies = new RoomEnemySystem();
        const BindingFlags flags = BindingFlags.Instance | BindingFlags.NonPublic;
        typeof(RoomEnemySystem).GetField("_bus", flags)!.SetValue(enemies,
            new FrontendCartridgeReadGuard(rom));
        var walker = typeof(RoomEnemySystem).GetMethod(
            "TryFindExtendedHitboxCallback", flags)!;
        RoomEnemySlot slot = enemies.Slots[0];
        slot.EnemyDefinitionPointer = DraygonEnemyDefinitionPointers.Body;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = DraygonBg2FrameDefinitions.Bank,
        };
        slot.XPosition = 0x100;
        slot.YPosition = 0x100;
        foreach (ushort pointer in new ushort[] { 0xa31b, 0xa643, 0xa36b })
        {
            slot.SpritemapPointer = pointer;
            object?[] arguments =
                [slot, (ushort)0x100, (ushort)0x100, (ushort)0,
                    (ushort)0, true, (ushort)0];
            bool found = (bool)walker.Invoke(enemies, arguments)!;
            AssertEqual(pointer != 0xa36b, found,
                $"Draygon frame {pointer:X4} shot overlap without ROM reads");
            if (found)
                AssertEqual(EnemyAiCodePointers.BankA5.DraygonShot,
                    (ushort)arguments[^1]!,
                    $"Draygon frame {pointer:X4} selected shot callback");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonCollisionDefinitions.ComponentsAt(0x8000),
            "uncatalogued Draygon BG2 collision frame fails loudly");
        Console.WriteLine("  Draygon BG2 collision: 34 native frame roots, three hitbox lists, and the guarded live callback walker match the cartridge.");

        ushort ReadWord(int address) =>
            (ushort)(rom.ReadByte(address) | rom.ReadByte(address + 1) << 8);
    }

    private static byte[] DrawDraygonBg2(EnemyTileArtworkCatalog art,
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
        slot.EnemyDefinitionPointer = DraygonEnemyDefinitionPointers.Body;
        slot.Definition = default(RoomEnemyDefinition) with
        {
            Bank = DraygonBg2FrameDefinitions.Bank,
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
