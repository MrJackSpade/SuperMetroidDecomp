using SuperMetroid.TestSupport.RepositoryInstallation.Verification;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Tooling;
using System.Text.Json;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Enemies.Artwork.ExtendedBg2ReferenceVerification;
using static SuperMetroid.Verification.Harness.InstalledArtworkBindingVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Artwork;
using SuperMetroid.Core.Bosses.Phantoon.Artwork;
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

/// <summary>Retail write and collision comparisons for Phantoon's installed BG2 frame artwork.</summary>
internal static class EnemyTileArtworkPhantoonBg2FramesVerification
{
    /// <summary>JSON options: Camel-case property names.</summary>
    private static readonly JsonSerializerOptions CamelCaseJson = new() { PropertyNamingPolicy = JsonNamingPolicy.CamelCase };

    /// <summary>Checks 22 BG2 selectors and native tile writes, the new-frame gate and engine hitboxes.</summary>
    internal static void VerifyInstalledPhantoonBg2Frames(
        CartridgeImportAddressSpace rom, string extractedDirectory,
        ImportEnemyArtwork stock)
    {
        AssertTrue(stock.Bosses.PhantoonBg2Frames is not null,
            "installed enemy art includes Phantoon BG2 frames");
        var identities = new HashSet<ushort>();
        foreach (EnemyFrameName frame in PhantoonBg2FrameDefinitions.Frames)
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

        EnemyBg2FrameDocument document =
            RequiredJson.Deserialize<EnemyBg2FrameDocument>(File.ReadAllBytes(Path.Combine(extractedDirectory, PhantoonBg2FrameDefinitions.FileName)),
                new JsonSerializerOptions { PropertyNameCaseInsensitive = true });
        const string frameName = "body_invulnerable";
        EnemyBg2WriteDocument first = document.Frames[frameName][0];
        ushort firstPointer = PhantoonBg2FrameDefinitions.Frames[0].Pointer;
        // His bodies clear the eye socket his eye fills whenever his body shows, so the import pictures each with his closed
        // eye written into it (#1513): the cartridge's body frame, then its closed-eye frame over it.
        byte[] nativeVram = ReadReferenceExtendedBg2Vram(rom, PhantoonBg2FrameDefinitionsConstants.Bank, firstPointer,
            newInstructionFrame: true);
        byte[] closedEyeVram = ReadReferenceExtendedBg2Vram(rom, PhantoonBg2FrameDefinitionsConstants.Bank,
            PhantoonBg2FrameDefinitions.Frames.Single(candidate => candidate.Frame == EnemyFrameId.PhantoonEyeClosed).Pointer,
            newInstructionFrame: true);
        for (int index = 0; index < nativeVram.Length; index += sizeof(ushort))
        {
            if (closedEyeVram[index] != 0 || closedEyeVram[index + 1] != 0)
            {
                nativeVram[index] = closedEyeVram[index];
                nativeVram[index + 1] = closedEyeVram[index + 1];
            }
        }
        var noCartridge = NewCartridgeReadGuard(rom);
        byte[] stockVram = DrawPhantoonBg2(stock, firstPointer, newInstructionFrame: true);
        int[] differ = [.. Enumerable.Range(0, nativeVram.Length).Where(index => nativeVram[index] != stockVram[index])];
        AssertTrue(differ.Length == 0,
            $"installed Phantoon frame draws native BG2 without cartridge reads; {differ.Length} bytes differ, from " +
            $"${(differ.Length == 0 ? 0 : differ[0]):X5} to ${(differ.Length == 0 ? 0 : differ[^1]):X5}; first: " +
            string.Join(" ", differ.Take(24).Select(index => $"${index:X5} native {nativeVram[index]:X2} installed {stockVram[index]:X2}")) +
            $"; native nonzero bytes {nativeVram.Count(value => value != 0)}, installed nonzero bytes {stockVram.Count(value => value != 0)}");
        AssertTrue(DrawPhantoonBg2(stock, firstPointer, newInstructionFrame: false).All(value => value == 0),
            "Phantoon BG2 drawing keeps the native new-frame producer gate");
        Suite(nameof(VerifyPhantoonCollisionDefinitions), () => VerifyPhantoonCollisionDefinitions(rom, stock));

        document.Frames[frameName][0] = first with { X = 32 };
        using var invalid = new MemoryStream(
            JsonSerializer.SerializeToUtf8Bytes(document,
                CamelCaseJson));
        AssertThrows<InvalidDataException>(
            () => PhantoonBg2FrameCatalog.Load(invalid),
            "Phantoon BG2 frame outside tilemap fails loudly");
        Console.WriteLine("  Phantoon BG2 frames: all 22 selectors preserve native writes and engine-owned hitboxes; collision ROM guard and invalid-data checks passed.");
    }

    /// <summary>Compares compiled frame components and three hitbox lists with retail data and the callbacks the installed artwork selects.</summary>
    private static void VerifyPhantoonCollisionDefinitions(
        CartridgeImportAddressSpace rom, ImportEnemyArtwork stock)
    {
        var checkedLists = new HashSet<ushort>();
        foreach (EnemyFrameName frame in PhantoonBg2FrameDefinitions.Frames)
        {
            int root = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, frame.Pointer));
            System.Collections.Immutable.ImmutableArray<EnemyCollisionComponent> components =
                PhantoonCollision.ComponentsAt(frame.Pointer);
            AssertEqual(rom.ReadCartridgeByte(root), (byte)components.Length,
                $"Phantoon {frame.Name} compiled hitbox component count");
            for (int index = 0; index < components.Length; index++)
            {
                int record = root + 2 + index * 8;
                EnemyCollisionComponent component = components[index];
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(record)), component.Offset.X,
                    $"Phantoon {frame.Name} component X");
                AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 2))), component.Offset.Y,
                    $"Phantoon {frame.Name} component Y");
                // The ROM names the component's hitbox list by address; every compiled component must hold exactly
                // the rectangles and reactions of the list the ROM names.
                ushort listPointer = CartridgeImportSource.Require(rom).ReadCartridgeWord((record + 6));
                checkedLists.Add(listPointer);
                int listAddress = ((int)new SnesAddress(PhantoonBg2FrameDefinitionsConstants.Bank, listPointer));
                IReadOnlyList<EnemyCollisionHitbox> hitboxes = component.Hitboxes;
                AssertEqual(CartridgeImportSource.Require(rom).ReadCartridgeWord(listAddress), checked((ushort)hitboxes.Count),
                    $"Phantoon {frame.Name} component {index} rectangle count");
                for (int hitboxIndex = 0; hitboxIndex < hitboxes.Count; hitboxIndex++)
                {
                    int address = listAddress + 2 + hitboxIndex * 12;
                    EnemyCollisionHitbox hitbox = hitboxes[hitboxIndex];
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord(address)), hitbox.Left,
                        "Phantoon compiled left bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 2))), hitbox.Top,
                        "Phantoon compiled top bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 4))), hitbox.Right,
                        "Phantoon compiled right bound");
                    AssertEqual(unchecked((short)CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 6))), hitbox.Bottom,
                        "Phantoon compiled bottom bound");
                    AssertEqual(EnemyReactionAccess.Of(0xa7, CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 8)), shot: false),
                        (EnemyReaction?)hitbox.Touch, "Phantoon compiled touch reaction");
                    AssertEqual(EnemyReactionAccess.Of(0xa7, CartridgeImportSource.Require(rom).ReadCartridgeWord((address + 10)), shot: true),
                        (EnemyReaction?)hitbox.Shot, "Phantoon compiled shot reaction");
                }
            }
            _ = FindPhantoonHitboxReaction(stock, frame.Pointer, new(0x0080, 0x0080), selectShot: true);
            _ = FindPhantoonHitboxReaction(stock, frame.Pointer, new(0x0080, 0x00a0), selectShot: false);
        }
        AssertEqual(3, checkedLists.Count,
            "Phantoon uses the three compiled retail hitbox lists");
        AssertEqual((EnemyReaction?)EnemyReaction.PhantoonShot,
            FindPhantoonHitboxReaction(stock, (ushort)PhantoonCollisionFrame.BodyFullHitbox,
                new(0x0080, 0x0080), selectShot: true),
            "Phantoon full-body frame selects Phantoon's shot reaction");
        AssertEqual((EnemyReaction?)EnemyReaction.PhantoonShot,
            FindPhantoonHitboxReaction(stock, (ushort)PhantoonCollisionFrame.BodyEyeHitboxOnly,
                new(0x0080, 0x0099), selectShot: true),
            "Phantoon eye-only frame selects the vulnerable eye's shot reaction");
    }

    /// <summary>Invokes the production extended-hitbox walker for a selected Phantoon frame and returns its touch or shot reaction.</summary>
    internal static EnemyReaction? FindPhantoonHitboxReaction(
        ImportEnemyArtwork? art, ushort pointer, Position16<RoomPixels> target, bool selectShot)
    {
        var enemies = new RoomEnemySystem((art is null ? EnemySessionFixtures.WithInstalledServices() : EnemySessionFixtures.WithInstalledServices(art.Enemies)), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = art?.Bosses };
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.PhantoonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        object?[] arguments =
            [slot, target, new Extent16<RoomPixels>(2, 2), selectShot, null];
        bool found = ExtendedHitboxWalker.Find(arguments);
        return found ? ExtendedHitboxWalker.Reaction(arguments) : null;
    }

    /// <summary>Draws a Phantoon BG2 frame into fresh VRAM, optionally setting the native new-instruction-frame upload bit.</summary>
    private static byte[] DrawPhantoonBg2(ImportEnemyArtwork art,
        ushort pointer, bool newInstructionFrame)
    {
        var enemies = new RoomEnemySystem(EnemySessionFixtures.WithInstalledServices(art.Enemies), RoomEnemyCompatibilityFixtures.ForNewRoom(), definitions: CompiledRoomEnemyDefinitions.Instance) { BossArtwork = art.Bosses };
        var vram = new SnesVram();
        enemies.Presentation.BindBackgrounds(BgArtworkFixtures.For(vram));
        // His fight attaches the body his body frames show on.
        BossBodyParts image = BossBodyCanvasFixtures.AttachBlank(enemies, InstalledFixtures.BossBodySheets, BossBodyOwner.Phantoon);
        enemies.Drawing.Enqueue(enemies.SlotTable.Slots[0], reached: true);
        RoomEnemySlot slot = enemies.SlotTable.Slots[0];
        slot.Bind(EnemyDefinitionId.PhantoonBody, slot.Definition);
        slot.Bind(slot.DefinitionId, default(RoomEnemyDefinition));
        slot.CollidesByFrameHitboxes = true;
        if (newInstructionFrame)
            slot.NewInstructionFrame = true;
        slot.SpritemapPointer = pointer;
        slot.Position = slot.Position.WithWhole(new(0x0080, 0x0080));
        enemies.Drawing.DrawAsOam(new OamBuffer(), new(0, 0));
        return BossBodyCanvasFixtures.AsVram(image, BossBodyOwner.Phantoon, art.Bosses, PhantoonBg2FrameDefinitions.VramBase, vram.Bytes.Length);
    }
}
