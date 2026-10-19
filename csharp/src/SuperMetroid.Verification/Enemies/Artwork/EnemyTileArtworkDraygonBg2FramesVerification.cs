using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Tooling;
using System.Reflection;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonOamCollisionVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkSporeSpawnCollisionVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedBg2ReferenceVerification;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;

/// <summary>Checks installed Draygon BG2 frames and collision components against native streams and live drawing.</summary>
internal static class EnemyTileArtworkDraygonBg2FramesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Compares 34 installed frames and 102 ordered tilemap writes with ROM, then checks guarded rendering and editable overrides.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for installed Draygon BG2 frames")]
    internal static void VerifyInstalledDraygonBg2Frames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        EnemyTileArtworkCatalog stock)
    {
        AssertTrue(stock.DraygonBg2Frames is not null,
            "installed enemy art includes Draygon BG2 frames");
        var noCartridge = NewFrontendCartridgeReadGuard(rom);
        int writeCount = 0;
        foreach (EnemyBg2FrameDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            AssertTrue((stock.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires stock.DraygonBg2Frames.")).TryGet(frame.Pointer,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"installed Draygon BG2 frame {frame.Name} exists");
            int root = ((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, frame.Pointer));
            AssertEqual((byte)1, rom.ReadCartridgeByte(root),
                $"Draygon {frame.Name} has one BG2 component");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 2)),
                $"Draygon {frame.Name} BG2 component X offset");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 4)),
                $"Draygon {frame.Name} BG2 component Y offset");
            ushort stream = CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 6));
            AssertEqual(EnemyBg2FrameLayout.StreamMarker,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, stream)))),
                $"Draygon {frame.Name} selects a BG2 command stream");
            int cursor = ((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, unchecked((ushort)(stream + 2))));
            var native = new List<(ushort Destination, ushort[] Tiles)>();
            for (int command = 0;
                 command < EnemyBg2FrameLayout.MaximumCommandsPerStream; command++)
            {
                ushort destination = CartridgeImportSource.Require(rom).ReadCartridgeWord(cursor);
                if (destination == 0xffff)
                    break;
                int count = CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 2));
                var tiles = new ushort[count];
                for (int tile = 0; tile < count; tile++)
                    tiles[tile] = CartridgeImportSource.Require(rom).ReadCartridgeWord((cursor + 4 + tile * 2));
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
                    .SequenceEqual(DrawDraygonBg2(stock, frame.Pointer, true)),
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
            RequiredJson.Deserialize<EnemyBg2FrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
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
            CamelCaseJson));
        EnemyTileArtworkCatalog edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((edited.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires edited.DraygonBg2Frames.")).TryGet(editedIdentity.Pointer,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> editedFrame),
            "edited Draygon BG2 frame is installed");
        int vramByte = (EnemyBg2FrameLayout.VramBase +
            editedFrame.Span[0].DestinationWord) * 2;
        AssertEqual((ushort)changedTile,
            BitConverter.ToUInt16(DrawDraygonBg2(edited, editedIdentity.Pointer, true), vramByte),
            "Draygon visual override changes the live BG2 tile word");
        AssertTrue(DrawDraygonBg2(edited, editedIdentity.Pointer, false).All(value => value == 0),
            "Draygon override preserves the native new-frame producer gate");
        EnemyTileArtworkCatalog reloaded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((reloaded.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires reloaded.DraygonBg2Frames.")).TryGet(editedIdentity.Pointer,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> reloadedFrame) &&
            reloadedFrame.Span[0].Tiles.Span[0] == (ushort)changedTile,
            "Draygon BG2 override survives reload");
        document.Frames[editedIdentity.Name][0] = first with { X = 32 };
        using var invalid = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => DraygonBg2FrameCatalog.Load(invalid),
            "Draygon BG2 write outside its tilemap fails loudly");
        Console.WriteLine("  Draygon BG2 frames: 34 selected frames and 102 native writes match installed VRAM; live edit, producer gate, ROM guard, reload, hash and strict validation pass.");
    }

    /// <summary>Compares native frame roots and hitbox lists, then runs the production overlap walker with cartridge reads denied.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for compiled Draygon BG2 collision")]
    internal static void VerifyCompiledDraygonBg2Collision(
        SuperMetroidAddressSpace rom)
    {
        var nativeLists = new Dictionary<ushort, IReadOnlyList<DraygonCollisionHitbox>>();
        foreach (EnemyBg2FrameDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            int root = ((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, frame.Pointer));
            DraygonCollisionComponent[] components =
                [.. DraygonCollisionDefinitions.ComponentsAt(frame.Pointer)];
            AssertEqual((int)rom.ReadCartridgeByte(root), components.Length,
                $"Draygon {frame.Name} compiled collision component count");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                DraygonCollisionComponent component = components[index];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)component.Offset.X),
                    $"Draygon {frame.Name} collision X offset");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)component.Offset.Y),
                    $"Draygon {frame.Name} collision Y offset");
                ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                if (nativeLists.TryGetValue(pointer, out IReadOnlyList<DraygonCollisionHitbox>? shared))
                    AssertTrue(ReferenceEquals(shared, component.Hitboxes),
                        $"Draygon {frame.Name} shares the list every frame naming hitbox list {pointer:X4} holds");
                else
                    nativeLists.Add(pointer, component.Hitboxes);
            }
        }
        AssertEqual(3, nativeLists.Count,
            "Draygon BG2 frames have three distinct native hitbox lists");
        foreach ((ushort pointer, IReadOnlyList<DraygonCollisionHitbox> hitboxes) in nativeLists)
        {
            int list = ((int)new SnesAddress(DraygonBg2FrameDefinitions.Bank, pointer));
            AssertEqual((int)CartridgeImportSource.Require(rom).ReadCartridgeWord(list), hitboxes.Count,
                $"Draygon hitbox list {pointer:X4} rectangle count");
            for (int index = 0; index < hitboxes.Count; index++)
            {
                int record = list + 2 + index * 12;
                DraygonCollisionHitbox box = hitboxes[index];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)box.Left),
                    $"Draygon list {pointer:X4} box {index} left");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)box.Top),
                    $"Draygon list {pointer:X4} box {index} top");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4)), unchecked((ushort)box.Right),
                    $"Draygon list {pointer:X4} box {index} right");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6)), unchecked((ushort)box.Bottom),
                    $"Draygon list {pointer:X4} box {index} bottom");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 8)), DraygonHitboxCallbackCodec.ToCallbackWord(box.Touch),
                    $"Draygon list {pointer:X4} box {index} touch reaction");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 10)), DraygonHitboxCallbackCodec.ToCallbackWord(box.Shot),
                    $"Draygon list {pointer:X4} box {index} shot reaction");
            }
        }

        // Invoke the production overlap walker with all cartridge reads
        // denied. This checks the actual dispatch, not just a matching table.
        var enemies = new RoomEnemySystem(new EnemySession());
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            Bank = DraygonBg2FrameDefinitions.Bank,
        });
        slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
        foreach (ushort pointer in new ushort[] { 0xa31b, 0xa643, 0xa36b })
        {
            slot.SpritemapPointer = pointer;
            object?[] arguments =
                [slot, new Position16<RoomPixels>((ushort)0x100, (ushort)0x100), new Extent16<RoomPixels>((ushort)0, (ushort)0), true, (ushort)0];
            bool found = ExtendedHitboxWalker.Find(enemies, arguments);
            AssertEqual(pointer != 0xa36b, found,
                $"Draygon frame {pointer:X4} shot overlap without ROM reads");
            if (found)
                AssertEqual(EnemyAiCodePointers.BankA5.DraygonShot,
                    (ushort)(arguments[^1] ?? throw new InvalidOperationException("Verification requires arguments[^1].")),
                    $"Draygon frame {pointer:X4} selected shot callback");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonCollisionDefinitions.ComponentsAt(0x8000),
            "uncatalogued Draygon BG2 collision frame fails loudly");
        Console.WriteLine("  Draygon BG2 collision: 34 native frame roots, three hitbox lists, and the guarded live callback walker match the cartridge.");
    }

    /// <summary>Draws one selected Draygon frame through the enemy layer producer and captures its resulting VRAM bytes.</summary>
    [PpuLayoutBoundary("exact hardware test: checks native VRAM words")]
    private static byte[] DrawDraygonBg2(EnemyTileArtworkCatalog art,
        ushort pointer, bool newInstructionFrame)
    {
        var enemies = new RoomEnemySystem(new EnemySession()) { TileArtwork = art };
        var vram = new SnesVram();
        enemies.BindFixtureDefinitions(CompiledRoomEnemyDefinitions.Instance);
        enemies.Presentation.BindVram(vram);
        enemies.Drawing.Enqueue(0, 0);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.EnemyDefinitionPointer, default(RoomEnemyDefinition) with
        {
            Bank = DraygonBg2FrameDefinitions.Bank,
        });
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        if (newInstructionFrame)
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.NewInstructionFrame);
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        enemies.Drawing.DrawLayersAsOam(new OamBuffer(), new(0, 0), 0, 0);
        return vram.Bytes.ToArray();
    }
}
