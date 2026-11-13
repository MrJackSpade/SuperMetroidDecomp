using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkDraygonOamCollisionVerification;
using static SuperMetroid.Verification.Enemies.Artwork.EnemyTileArtworkSporeSpawnCollisionVerification;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedBg2ReferenceVerification;
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.AssetExtraction.Bosses;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Enemies.BossBodies;
using SuperMetroid.AssetExtraction.Rooms;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.Artwork;

/// <summary>Checks installed Draygon BG2 frames and collision components against native streams and live drawing.</summary>
internal static class EnemyTileArtworkDraygonBg2FramesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Compares 34 installed frames and 102 ordered tilemap writes with ROM, then checks guarded rendering.</summary>
    internal static void VerifyInstalledDraygonBg2Frames(
        SuperMetroidAddressSpace rom, string extractedDirectory,
        ImportEnemyArtwork stock)
    {
        AssertTrue(stock.Bosses.DraygonBg2Frames is not null,
            "installed enemy art includes Draygon BG2 frames");
        var noCartridge = NewCartridgeReadGuard(rom);
        int writeCount = 0;
        foreach (EnemyFrameName frame in DraygonBg2FrameDefinitions.Frames)
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
            // A frame the cartridge writes over another is shown whole (#1515): every word the cartridge writes matches.
            AssertTrue(ShowsWrittenWords(ReadReferenceExtendedBg2Vram(rom, DraygonBg2FrameToolingDefinitions.Bank, frame.Pointer, true),
                    DrawDraygonBg2(stock, frame.Pointer, true)),
                $"Draygon {frame.Name} stock VRAM shows the native words without cartridge reads");
        }
        AssertEqual(34, DraygonBg2FrameDefinitions.Frames.Length,
            "all selected Draygon BG2 frame identities are installed");
        AssertEqual(102, writeCount, "all Draygon BG2 command writes are installed");
        Suite(nameof(VerifyCompiledDraygonBg2Collision), () => VerifyCompiledDraygonBg2Collision(rom));
        Suite(nameof(VerifyCompiledDraygonOamCollision), () => VerifyCompiledDraygonOamCollision(rom));
        Suite(nameof(VerifyCompiledSporeSpawnCollision), () => VerifyCompiledSporeSpawnCollision(rom));

        EnemyBg2FrameDocument document =
            RequiredJson.Deserialize<EnemyBg2FrameDocument>(File.ReadAllBytes(Path.Combine(extractedDirectory, DraygonBg2FrameDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        EnemyFrameName firstIdentity = DraygonBg2FrameDefinitions.Frames[0];
        EnemyBg2WriteDocument first = document.Frames[firstIdentity.Name][0];
        AssertTrue(DrawDraygonBg2(stock, firstIdentity.Pointer, false).All(value => value == 0),
            "Draygon BG2 drawing keeps the native new-frame producer gate");
        document.Frames[firstIdentity.Name][0] = first with { X = 32 };
        using var invalid = new MemoryStream(JsonSerializer.SerializeToUtf8Bytes(document,
            CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => DraygonBg2FrameCatalog.Load(invalid),
            "Draygon BG2 write outside its tilemap fails loudly");
        Console.WriteLine("  Draygon BG2 frames: 34 selected frames and 102 native writes match installed VRAM; producer gate, ROM guard and strict validation pass.");
    }

    /// <summary>Compares native frame roots and hitbox lists, then runs the production overlap walker with cartridge reads denied.</summary>
    internal static void VerifyCompiledDraygonBg2Collision(
        SuperMetroidAddressSpace rom)
    {
        var nativeLists = new Dictionary<ushort, IReadOnlyList<EnemyCollisionHitbox>>();
        foreach (EnemyFrameName frame in DraygonBg2FrameDefinitions.Frames)
        {
            int root = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, frame.Pointer));
            EnemyCollisionComponent[] components =
                [.. DraygonCollision.ComponentsAt(frame.Pointer)];
            AssertEqual(rom.ReadCartridgeByte(root), components.Length,
                $"Draygon {frame.Name} compiled collision component count");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                EnemyCollisionComponent component = components[index];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)component.Offset.X),
                    $"Draygon {frame.Name} collision X offset");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)component.Offset.Y),
                    $"Draygon {frame.Name} collision Y offset");
                ushort pointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                if (nativeLists.TryGetValue(pointer, out IReadOnlyList<EnemyCollisionHitbox>? shared))
                    AssertTrue(ReferenceEquals(shared, component.Hitboxes),
                        $"Draygon {frame.Name} shares the list every frame naming hitbox list {pointer:X4} holds");
                else
                    nativeLists.Add(pointer, component.Hitboxes);
            }
        }
        AssertEqual(3, nativeLists.Count,
            "Draygon BG2 frames have three distinct native hitbox lists");
        foreach ((ushort pointer, IReadOnlyList<EnemyCollisionHitbox> hitboxes) in nativeLists)
        {
            int list = ((int)new SnesAddress(DraygonBg2FrameToolingDefinitions.Bank, pointer));
            AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(list), hitboxes.Count,
                $"Draygon hitbox list {pointer:X4} rectangle count");
            for (int index = 0; index < hitboxes.Count; index++)
            {
                int record = list + 2 + index * 12;
                EnemyCollisionHitbox box = hitboxes[index];
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(record), unchecked((ushort)box.Left),
                    $"Draygon list {pointer:X4} box {index} left");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2)), unchecked((ushort)box.Top),
                    $"Draygon list {pointer:X4} box {index} top");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 4)), unchecked((ushort)box.Right),
                    $"Draygon list {pointer:X4} box {index} right");
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6)), unchecked((ushort)box.Bottom),
                    $"Draygon list {pointer:X4} box {index} bottom");
                AssertEqual(EnemyReactionAccess.Of(0xa5, CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 8)), shot: false), (EnemyReaction?)box.Touch,
                    $"Draygon list {pointer:X4} box {index} touch reaction");
                AssertEqual(EnemyReactionAccess.Of(0xa5, CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 10)), shot: true), (EnemyReaction?)box.Shot,
                    $"Draygon list {pointer:X4} box {index} shot reaction");
            }
        }

        // Invoke the production overlap walker with all cartridge reads
        // denied. This checks the actual dispatch, not just a matching table.
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.Position = slot.Position.WithWhole(new(0x100, 0x100));
        foreach (ushort pointer in new ushort[] { 0xa31b, 0xa643, 0xa36b })
        {
            slot.SpritemapPointer = pointer;
            object?[] arguments =
                [slot, new Position16<RoomPixels>(0x100, 0x100), new Extent16<RoomPixels>(0, 0), true, (ushort)0];
            bool found = ExtendedHitboxWalker.Find(arguments);
            AssertEqual(pointer != 0xa36b, found,
                $"Draygon frame {pointer:X4} shot overlap without ROM reads");
            if (found)
                AssertEqual((EnemyReaction?)EnemyReaction.DraygonShot, ExtendedHitboxWalker.Reaction(arguments),
                    $"Draygon frame {pointer:X4} selected shot reaction");
        }

        AssertThrows<InvalidDataException>(
            () => DraygonCollision.ComponentsAt(0x8000),
            "uncatalogued Draygon BG2 collision frame fails loudly");
        Console.WriteLine("  Draygon BG2 collision: 34 native frame roots, three hitbox lists, and the guarded live callback walker match the cartridge.");
    }

    /// <summary>Whether <paramref name="shown"/> holds every tilemap word <paramref name="native"/> writes, a nonzero word.</summary>
    private static bool ShowsWrittenWords(byte[] native, byte[] shown)
    {
        for (int index = 0; index + 1 < native.Length; index += 2)
        {
            if ((native[index] != 0 || native[index + 1] != 0) && (native[index] != shown[index] || native[index + 1] != shown[index + 1]))
                return false;
        }
        return true;
    }

    /// <summary>Draws one selected Draygon frame through the enemy layer producer and captures its resulting VRAM bytes.</summary>
    private static byte[] DrawDraygonBg2(ImportEnemyArtwork art,
        ushort pointer, bool newInstructionFrame)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = art.Bosses };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        // His fight attaches the body his body frames show on.
        BossBodyParts image = BossBodyCanvasFixtures.AttachBlank(enemies, InstalledFixtures.BossBodySheets, BossBodyOwner.Draygon);
        enemies.Drawing.Enqueue(enemies.SlotTable.Slots[0], reached: true);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.DraygonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.CollidesByFrameHitboxes = true;
        if (newInstructionFrame)
            slot.NewInstructionFrame = true;
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        enemies.Drawing.DrawAsOam(new OamBuffer(), new(0, 0));
        return BossBodyCanvasFixtures.AsVram(image, BossBodyOwner.Draygon, art.Bosses, EnemyBg2FrameLayoutConstants.VramBase, vram.Bytes.Length);
    }
}
