using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedBg2ReferenceVerification;
using static SuperMetroid.Verification.Frontend.FrontendRomFreeVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Enemies;
using SuperMetroid.AssetExtraction.Enemies.Callbacks;
using SuperMetroid.Core;
using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Verification.Enemies.Artwork;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;
using SuperMetroid.Core.Rooms.Artwork;

/// <summary>Retail write and collision comparisons for Phantoon's installed BG2 frame artwork.</summary>
internal static class EnemyTileArtworkPhantoonBg2FramesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Checks 22 BG2 selectors and native tile writes, then proves an override changes live VRAM without changing engine hitboxes or the new-frame gate.</summary>
    internal static void VerifyInstalledPhantoonBg2Frames(
        SuperMetroidAddressSpace rom, string stockDirectory,
        InstalledEnemyArtwork stock)
    {
        AssertTrue(stock.Bosses.PhantoonBg2Frames is not null,
            "installed enemy art includes Phantoon BG2 frames");
        var identities = new HashSet<ushort>();
        foreach (EnemySpritemapDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            AssertTrue(identities.Add(frame.Pointer),
                $"Phantoon frame {frame.Name} has a distinct physical selector");
            AssertTrue((stock.Bosses.PhantoonBg2Frames ?? throw new InvalidOperationException("Verification requires stock.PhantoonBg2Frames.")).TryGet(frame.Frame,
                    out ReadOnlyMemory<EnemyBg2TilemapWrite> installed),
                $"installed Phantoon BG2 frame {frame.Name} exists");
            var native = new List<EnemyTileArtworkDraygonBg2FramesNative>();
            int root = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, frame.Pointer));
            int componentCount = rom.ReadCartridgeByte(root);
            for (int component = 0; component < componentCount; component++)
            {
                ushort stream = CartridgeImportSource.Require(rom).ReadCartridgeWord((root + 2 + component * 8 + 4));
                int cursor = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, unchecked((ushort)(stream + 2))));
                for (int command = 0; command < 128; command++)
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
            }
            AssertEqual(native.Count, installed.Length,
                $"Phantoon {frame.Name} preserves native BG2 write order");
            for (int index = 0; index < native.Count; index++)
            {
                EnemyBg2TilemapWrite write = installed.Span[index];
                AssertEqual(native[index].Destination,
                    checked((ushort)(PhantoonBg2FrameDefinitions.WorkingRamBase +
                        write.FirstCell * 2)),
                    $"Phantoon {frame.Name} write {index} preserves BG2 destination");
                AssertTrue(write.Words.SequenceEqual(native[index].Tiles),
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

        EnemyBg2FrameDocument document =
            RequiredJson.Deserialize<EnemyBg2FrameDocument>(original,
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string frameName = "body_invulnerable";
        EnemyBg2WriteDocument first = document.Frames[frameName][0];
        int originalTile = first.Tiles[0].PackedWord;
        var editedTiles = (RoomBackgroundTilemapCell[])first.Tiles.Clone();
        editedTiles[0] = RoomBackgroundTilemapCellWords.Cell(originalTile ^ 1);
        document.Frames[frameName][0] = first with { Tiles = editedTiles };
        string overrideDirectory = Path.Combine(stockDirectory, "phantoon-bg2-overrides");
        Directory.CreateDirectory(overrideDirectory);
        string overridePath = Path.Combine(overrideDirectory,
            PhantoonBg2FrameDefinitions.FileName);
        File.WriteAllBytes(overridePath,
            JsonSerializer.SerializeToUtf8Bytes(document,
                CamelCaseJson));
        InstalledEnemyArtwork edited = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((edited.Bosses.PhantoonBg2Frames ?? throw new InvalidOperationException("Verification requires edited.PhantoonBg2Frames.")).TryGet(
            PhantoonBg2FrameDefinitions.Frames[0].Frame,
            out ReadOnlyMemory<EnemyBg2TilemapWrite> editedFrame),
            "Phantoon BG2 edited frame is installed");
        AssertEqual((ushort)(originalTile ^ 1), editedFrame.Span[0].Words[0],
            "Phantoon BG2 override changes the selected tile reference");
        AssertEqual((ushort)originalTile,
            (stock.Bosses.PhantoonBg2Frames ?? throw new InvalidOperationException("Verification requires stock.PhantoonBg2Frames.")).TryGet(
                PhantoonBg2FrameDefinitions.Frames[0].Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> stockFrame)
                ? stockFrame.Span[0].Words[0] : (ushort)0,
            "Phantoon BG2 override leaves stock frame intact");
        ushort firstPointer = PhantoonBg2FrameDefinitions.Frames[0].Pointer;
        byte[] nativeVram = ReadReferenceExtendedBg2Vram(rom, PhantoonBg2FrameDefinitionsConstants.Bank, firstPointer,
            newInstructionFrame: true);
        var noCartridge = NewFrontendCartridgeReadGuard(rom);
        byte[] stockVram = DrawPhantoonBg2(stock, firstPointer, newInstructionFrame: true);
        byte[] editedVram = DrawPhantoonBg2(edited, firstPointer, newInstructionFrame: true);
        AssertTrue(nativeVram.SequenceEqual(stockVram),
            "installed Phantoon frame draws native BG2 without cartridge reads");
        int changedVramByte = (PhantoonBg2FrameDefinitions.VramBase +
            editedFrame.Span[0].FirstCell) * 2;
        AssertEqual((ushort)(originalTile ^ 1),
            BitConverter.ToUInt16(editedVram, changedVramByte),
            "Phantoon BG2 override changes the live rendered tile word");
        AssertTrue(DrawPhantoonBg2(edited, firstPointer, newInstructionFrame: false).All(value => value == 0),
            "Phantoon BG2 override retains the native new-frame producer gate");
        Suite(nameof(VerifyPhantoonCollisionDefinitions), () => VerifyPhantoonCollisionDefinitions(rom, stock, edited));
        InstalledEnemyArtwork reloaded = EnemyTileArtworkFiles.Load(
            stockDirectory, overrideDirectory);
        AssertTrue((reloaded.Bosses.PhantoonBg2Frames ?? throw new InvalidOperationException("Verification requires reloaded.PhantoonBg2Frames.")).TryGet(
                PhantoonBg2FrameDefinitions.Frames[0].Frame,
                out ReadOnlyMemory<EnemyBg2TilemapWrite> reloadedFrame) &&
            reloadedFrame.Span[0].Words[0] == (ushort)(originalTile ^ 1),
            "Phantoon BG2 override survives catalog reload");

        document.Frames[frameName][0] = first with { X = 32 };
        using var invalid = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(document,
                CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => PhantoonBg2FrameCatalog.Load(invalid),
            "Phantoon BG2 frame outside tilemap fails loudly");
        Console.WriteLine("  Phantoon BG2 frames: all 22 selectors preserve native writes and engine-owned hitboxes; live VRAM edit, collision ROM guard, reload and invalid-data checks passed.");
    }

    /// <summary>Compares compiled frame components and three hitbox lists with retail data while checking stock and edited artwork select the same callbacks.</summary>
    private static void VerifyPhantoonCollisionDefinitions(
        SuperMetroidAddressSpace rom, InstalledEnemyArtwork stock, InstalledEnemyArtwork edited)
    {
        var checkedLists = new HashSet<ushort>();
        foreach (EnemySpritemapDefinition frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, frame.Pointer));
            System.Collections.Immutable.ImmutableArray<PhantoonCollisionComponent> components =
                PhantoonCollisionDefinitions.ComponentsAt(frame.Pointer);
            AssertEqual(rom.ReadCartridgeByte(root), (byte)components.Length,
                $"Phantoon {frame.Name} compiled hitbox component count");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                PhantoonCollisionComponent component = components[index];
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(record)), component.Offset.X,
                    $"Phantoon {frame.Name} component X");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2))), component.Offset.Y,
                    $"Phantoon {frame.Name} component Y");
                // The ROM names the component's hitbox list by address; every compiled component must hold exactly
                // the rectangles and reactions of the list the ROM names.
                ushort listPointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                checkedLists.Add(listPointer);
                int listAddress = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, listPointer));
                IReadOnlyList<PhantoonCollisionHitbox> hitboxes = component.Hitboxes;
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(listAddress), checked((ushort)hitboxes.Count),
                    $"Phantoon {frame.Name} component {index} rectangle count");
                for (int hitboxIndex = 0; hitboxIndex < hitboxes.Count; hitboxIndex++)
                {
                    int address = listAddress + 2 + hitboxIndex * 12;
                    PhantoonCollisionHitbox hitbox = hitboxes[hitboxIndex];
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address)), hitbox.Left,
                        "Phantoon compiled left bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), hitbox.Top,
                        "Phantoon compiled top bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))), hitbox.Right,
                        "Phantoon compiled right bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6))), hitbox.Bottom,
                        "Phantoon compiled bottom bound");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 8)),
                        PhantoonHitboxCallbackCodec.ToCallbackWord(hitbox.Touch), "Phantoon compiled touch reaction");
                    AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 10)),
                        PhantoonHitboxCallbackCodec.ToCallbackWord(hitbox.Shot), "Phantoon compiled shot reaction");
                }
            }
            _ = FindPhantoonHitboxCallback(stock, frame.Pointer, new(0x0080, 0x0080), selectShot: true);
            _ = FindPhantoonHitboxCallback(stock, frame.Pointer, new(0x0080, 0x00a0), selectShot: false);
        }
        AssertEqual(3, checkedLists.Count,
            "Phantoon uses the three compiled retail hitbox lists");
        AssertEqual(EnemyAiRoutineDefinitions.BankA7.PhantoonShot.Offset,
            FindPhantoonHitboxCallback(stock, (ushort)PhantoonCollisionFrame.BodyFullHitbox,
                new(0x0080, 0x0080), selectShot: true),
            "Phantoon full-body frame selects native shot callback");
        AssertEqual(EnemyAiRoutineDefinitions.BankA7.PhantoonShot.Offset,
            FindPhantoonHitboxCallback(edited, (ushort)PhantoonCollisionFrame.BodyFullHitbox,
                new(0x0080, 0x0080), selectShot: true),
            "Phantoon BG2 artwork edit does not move or replace hitbox callbacks");
        AssertEqual(EnemyAiRoutineDefinitions.BankA7.PhantoonShot.Offset,
            FindPhantoonHitboxCallback(edited, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
                new(0x0080, 0x0099), selectShot: true),
            "Phantoon eye-only frame selects native vulnerable eye callback");
    }

    /// <summary>Invokes the production extended-hitbox walker for a selected Phantoon frame and returns its touch or shot callback.</summary>
    internal static ushort FindPhantoonHitboxCallback(
        InstalledEnemyArtwork? art, ushort pointer, Position16<RoomPixels> target, bool selectShot)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art?.Enemies, BossArtwork = art?.Bosses, AreaArtwork = art?.Areas };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.PhantoonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        object?[] arguments =
            [slot, target, new Extent16<RoomPixels>(2, 2), selectShot, null];
        bool found = ExtendedHitboxWalker.Find(enemies, arguments);
        return found ? (ushort)(arguments[4] ?? throw new InvalidOperationException("Verification requires arguments[4].")) : (ushort)0;
    }

    /// <summary>Draws a Phantoon BG2 frame into fresh VRAM, optionally setting the native new-instruction-frame upload bit.</summary>
    private static byte[] DrawPhantoonBg2(InstalledEnemyArtwork art,
        ushort pointer, bool newInstructionFrame)
    {
        var enemies = new RoomEnemySystem(new EnemySession(), definitions: CompiledRoomEnemyDefinitions.Instance) { TileArtwork = art.Enemies, BossArtwork = art.Bosses, AreaArtwork = art.Areas };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        // His fight attaches the BG2 image his body frames draw into.
        EnemyBg2Image image = EnemyBg2ImageFixtures.AttachBlank(enemies.Presentation);
        enemies.Drawing.Enqueue(0, 0);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.PhantoonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.ExtraProperties = slot.ExtraProperties.With(
            EnemyExtraProperties.UsesExtendedSpritemap);
        if (newInstructionFrame)
            slot.ExtraProperties = slot.ExtraProperties.With(
                EnemyExtraProperties.NewInstructionFrame);
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        enemies.Drawing.DrawLayersAsOam(new OamBuffer(), new(0, 0), 0, 0);
        return EnemyBg2ImageFixtures.AsVram(image, PhantoonBg2FrameDefinitions.VramBase, vram.Bytes.Length);
    }
}
