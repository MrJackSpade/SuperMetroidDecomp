using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Tooling;
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
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.TestSupport.RepositoryInstallation;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.AssetExtraction.World.Artwork;

/// <summary>Checks installed Draygon BG2 frames and collision components against native streams and live drawing.</summary>
internal static class EnemyTileArtworkDraygonBg2FramesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Compares 34 installed frames and 102 ordered tilemap writes with ROM, then checks guarded rendering and editable overrides.</summary>
    internal static void VerifyInstalledDraygonBg2Frames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        InstalledEnemyArtwork stock)
    {
        AssertTrue(stock.Bosses.DraygonBg2Frames is not null,
            "installed enemy art includes Draygon BG2 frames");
        var noCartridge = NewFrontendCartridgeReadGuard(rom);
        int writeCount = 0;
        foreach (EnemySpritemapDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            AssertTrue((stock.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires stock.DraygonBg2Frames.")).TryGet(frame.Frame,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"installed Draygon BG2 frame {frame.Name} exists");
            int root = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, frame.Pointer));
            AssertEqual((byte)1, rom.ReadCartridgeByte(root),
                $"Draygon {frame.Name} has one BG2 component");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 2)),
                $"Draygon {frame.Name} BG2 component X offset");
            AssertEqual((ushort)0, CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 4)),
                $"Draygon {frame.Name} BG2 component Y offset");
            ushort stream = CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 6));
            AssertEqual(EnemyBg2FrameLayoutConstants.StreamMarker,
                CartridgeImportSource.Require(rom).ReadCartridgeWord((((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, stream)))),
                $"Draygon {frame.Name} selects a BG2 command stream");
            int cursor = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, unchecked((ushort)(stream + 2))));
            var native = new List<EnemyTileArtworkDraygonBg2FramesNative>();
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
                native.Add(new EnemyTileArtworkDraygonBg2FramesNative(destination, tiles));
                cursor += 4 + count * 2;
            }
            AssertEqual(native.Count, installed.Length,
                $"Draygon {frame.Name} preserves native BG2 write order");
            for (int index = 0; index < native.Count; index++)
            {
                EnemyBg2TilemapWrite write = installed.Span[index];
                AssertEqual(native[index].Destination,
                    checked((ushort)(EnemyBg2FrameLayoutConstants.WorkingRamBase +
                        write.FirstCell * 2)),
                    $"Draygon {frame.Name} write {index} preserves destination");
                AssertTrue(write.Words.SequenceEqual(native[index].Tiles),
                    $"Draygon {frame.Name} write {index} preserves tile words");
            }
            writeCount += native.Count;
            AssertTrue(ReadReferenceExtendedBg2Vram(rom, DraygonBg2FrameToolingDefinitions.Bank, frame.Pointer, true)
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
        EnemySpritemapDefinition editedIdentity = DraygonBg2FrameDefinitions.Frames[0];
        EnemyBg2WriteDocument first = document.Frames[editedIdentity.Name][0];
        int changedTile = first.Tiles[0].PackedWord ^ 1;
        var editedTiles = (RoomBackgroundTilemapCell[])first.Tiles.Clone();
        editedTiles[0] = RoomBackgroundTilemapCellWords.Cell(changedTile);
        document.Frames[editedIdentity.Name][0] = first with { Tiles = editedTiles };
        string overrideDirectory = Path.Combine(stockDirectory, "draygon-bg2-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory,
            DraygonBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document,
            CamelCaseJson));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((edited.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires edited.DraygonBg2Frames.")).TryGet(editedIdentity.Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> editedFrame),
            "edited Draygon BG2 frame is installed");
        int vramByte = (EnemyBg2FrameLayoutConstants.VramBase +
            editedFrame.Span[0].FirstCell) * 2;
        AssertEqual((ushort)changedTile,
            BitConverter.ToUInt16(DrawDraygonBg2(edited, editedIdentity.Pointer, true), vramByte),
            "Draygon visual override changes the live BG2 tile word");
        AssertTrue(DrawDraygonBg2(edited, editedIdentity.Pointer, false).All(value => value == 0),
            "Draygon override preserves the native new-frame producer gate");
        InstalledEnemyArtwork reloaded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((reloaded.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires reloaded.DraygonBg2Frames.")).TryGet(editedIdentity.Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> reloadedFrame) &&
            reloadedFrame.Span[0].Words[0] == (ushort)changedTile,
            "Draygon BG2 override survives reload");
        // A player's override written at revision 2 named each frame by where the cartridge stores it; the import carries
        // it onto the frames' semantic names, so its edit keeps showing (#1465).
        var legacyOverride = document with
        {
            Version = EnemyFrameLegacyNameDefinitions.LastBg2FrameRevision,
            Frames = DraygonBg2FrameDefinitions.Frames.ToDictionary(frame => frame.LegacyName,
                frame => document.Frames[frame.Name], StringComparer.Ordinal),
        };
        AssertTrue(DraygonBg2FrameDefinitions.Frames.All(frame => !legacyOverride.Frames.ContainsKey(frame.Name)),
            "the revision-2 Draygon BG2 fixture names no frame by its semantic name");
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(legacyOverride, CamelCaseJson));
        InstalledEnemyArtwork legacyEdited = EnemyTileArtworkFiles.Load(stockDirectory, overrideDirectory);
        foreach (EnemySpritemapDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            bool legacyInstalled = (legacyEdited.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires legacyEdited.DraygonBg2Frames.")).TryGet(frame.Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> legacyFrame);
            bool currentInstalled = (edited.Bosses.DraygonBg2Frames ?? throw new InvalidOperationException("Verification requires edited.DraygonBg2Frames.")).TryGet(frame.Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> currentFrame);
            AssertTrue(legacyInstalled && currentInstalled && legacyFrame.Length == currentFrame.Length,
                $"revision-2 Draygon BG2 override installs {frame.Name} as its current-revision equal does");
            for (int index = 0; index < legacyFrame.Length; index++)
                AssertTrue(legacyFrame.Span[index].FirstCell == currentFrame.Span[index].FirstCell &&
                    legacyFrame.Span[index].Words.SequenceEqual(currentFrame.Span[index].Words),
                    $"revision-2 Draygon BG2 override keeps {frame.Name} write {index}");
        }
        File.WriteAllBytes(overridePath, JsonSerializer.SerializeToUtf8Bytes(document, CamelCaseJson));
        document.Frames[editedIdentity.Name][0] = first with { X = 32 };
        using var invalid = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => DraygonBg2FrameCatalog.Load(invalid),
            "Draygon BG2 write outside its tilemap fails loudly");
        Console.WriteLine("  Draygon BG2 frames: 34 selected frames and 102 native writes match installed VRAM; live edit, producer gate, ROM guard, reload, hash and strict validation pass.");
    }

    /// <summary>Compares native frame roots and hitbox lists, then runs the production overlap walker with cartridge reads denied.</summary>
    internal static void VerifyCompiledDraygonBg2Collision(
        SuperMetroidAddressSpace rom)
    {
        var nativeLists = new Dictionary<ushort, IReadOnlyList<DraygonCollisionHitbox>>();
        foreach (EnemySpritemapDefinition frame in DraygonBg2FrameDefinitions.Frames)
        {
            int root = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, frame.Pointer));
            DraygonCollisionComponent[] components =
                [.. DraygonCollision.ComponentsAt(frame.Pointer)];
            AssertEqual(rom.ReadCartridgeByte(root), components.Length,
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
            int list = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, pointer));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(list), hitboxes.Count,
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
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
        foreach (ushort pointer in new ushort[] { 0xa31b, 0xa643, 0xa36b })
        {
            slot.SpritemapPointer = pointer;
            object?[] arguments =
                [slot, new Position16<RoomPixels>(0x100, 0x100), new Extent16<RoomPixels>(0, 0), true, (ushort)0];
            bool found = ExtendedHitboxWalker.Find(enemies, arguments);
            AssertEqual(pointer != 0xa36b, found,
                $"Draygon frame {pointer:X4} shot overlap without ROM reads");
            if (found)
                AssertEqual(EnemyAiRoutineDefinitions.BankA5.DraygonShot.Offset,
                    (ushort)(arguments[^1] ?? throw new InvalidOperationException("Verification requires arguments[^1].")),
                    $"Draygon frame {pointer:X4} selected shot callback");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonCollision.ComponentsAt(0x8000),
            "uncatalogued Draygon BG2 collision frame fails loudly");
        Console.WriteLine("  Draygon BG2 collision: 34 native frame roots, three hitbox lists, and the guarded live callback walker match the cartridge.");
    }

    /// <summary>Draws one selected Draygon frame through the enemy layer producer and captures its resulting VRAM bytes.</summary>
    private static byte[] DrawDraygonBg2(InstalledEnemyArtwork art,
        ushort pointer, bool newInstructionFrame)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        // His fight attaches the body layer his body frames draw on.
        BossBodyCanvas image = BossBodyCanvasFixtures.AttachBlank(enemies, RepositoryInstallation.Artwork.BossBodySheets
            ?? throw new InvalidOperationException("The repository installation has no boss body sheets."), BossBodyOwner.Draygon);
        enemies.Drawing.Enqueue(0, 0);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        if (newInstructionFrame)
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.NewInstructionFrame);
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        enemies.Drawing.DrawLayersAsOam(new OamBuffer(), new(0, 0), 0, 0);
        return BossBodyCanvasFixtures.AsVram(image, enemies, EnemyBg2FrameLayoutConstants.VramBase, vram.Bytes.Length);
    }
}
