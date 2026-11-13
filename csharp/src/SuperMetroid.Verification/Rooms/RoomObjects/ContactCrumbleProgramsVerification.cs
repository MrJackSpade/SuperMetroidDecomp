using SuperMetroid.AssetExtraction;
using static SuperMetroid.Verification.Rooms.RoomObjects.RoomObjectRunHeaderAssertions;
using SuperMetroid.AssetExtraction.Rooms;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.RoomObjects.ShotBlockObjectProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.RoomObjects;
using SuperMetroid.AssetExtraction.Cartridge;
namespace SuperMetroid.Verification.Rooms.RoomObjects;
using SuperMetroid.Verification.Harness;
using SuperMetroid.Core.World;

/// <summary>Retail and guarded playback checks for all eight contact-crumble PLM programs and linked restore draws.</summary>
internal static class ContactCrumbleProgramsVerification
{
    /// <summary>Native bank-$84 list bounds and timed-record counts from the pinned disassembly.</summary>
    private static ContactCrumbleNativeLayoutsResult[] ContactCrumbleNativeLayouts() =>
        [new ContactCrumbleNativeLayoutsResult(0xc9f9,7,0xca1c),new ContactCrumbleNativeLayoutsResult(0xca1c,8,0xca41),new ContactCrumbleNativeLayoutsResult(0xca41,8,0xca66),new ContactCrumbleNativeLayoutsResult(0xca66,8,0xca8b),
         new ContactCrumbleNativeLayoutsResult(0xca8b,4,0xcaa0),new ContactCrumbleNativeLayoutsResult(0xcaa0,4,0xcab5),new ContactCrumbleNativeLayoutsResult(0xcab5,4,0xcaca),new ContactCrumbleNativeLayoutsResult(0xcaca,4,0xcadf)];

    /// <summary>Compares compiled timing, restoration and deletion words over the full native address domain.</summary>
    private static void VerifyContactCrumbleControlMapping(CartridgeImportAddressSpace rom)
    {
        var addresses = new List<ushort>();
        foreach (var layout in ContactCrumbleNativeLayouts())
        {
            addresses.Add(layout.Start);
            int cursor = layout.Start + 3;
            for (int frame = 0; frame < layout.Frames; frame++, cursor += 4)
                addresses.Add((ushort)cursor);
            for (; cursor < layout.End; cursor += 2)
                addresses.Add((ushort)cursor);
        }
        AssertEqual(64, addresses.Count, "contact crumble native control extent");
        AssertTrue(addresses.SequenceEqual(RoomObjectContactCrumbleProgramDefinitions.MechanicsWordAddresses()),
            "contact crumble complete native control enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectContactCrumbleProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value);
            AssertEqual(known.Contains((ushort)address), found, "contact crumble full control address domain");
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native timing, restoration and deletion controls");
        }
    }

    /// <summary>Checks every timed draw operand and rejects ownership of non-draw bank-$84 words.</summary>
    private static void VerifyContactCrumbleDrawMapping(CartridgeImportAddressSpace rom)
    {
        var addresses = new HashSet<ushort>();
        foreach (var layout in ContactCrumbleNativeLayouts())
        for (int frame = 0; frame < layout.Frames; frame++)
            addresses.Add((ushort)(layout.Start + 5 + frame * 4));
        AssertEqual(47, addresses.Count, "contact crumble native draw operand extent");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectContactCrumbleProgramDefinitions.TryReadDrawPointerWord((ushort)address, out ushort value);
            AssertEqual(addresses.Contains((ushort)address), found, "contact crumble full draw address domain");
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native breakup, reverse reveal and linked restoration draws");
        }
    }

    /// <summary>Compares packed sound bytes at the eight list starts against native bank-$84 data.</summary>
    private static void VerifyContactCrumbleSoundMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] addresses = [0xc9fb,0xca1e,0xca43,0xca68,0xca8d,0xcaa2,0xcab7,0xcacc];
        AssertTrue(addresses.SequenceEqual(RoomObjectContactCrumbleProgramDefinitions.MechanicsByteAddresses()),
            "contact crumble native packed sound enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomObjectContactCrumbleProgramDefinitions.TryReadMechanicsByte((ushort)address, out byte value);
            AssertEqual(known.Contains((ushort)address), found, "contact crumble full sound address domain");
            AssertEqual(found ? rom.ReadCartridgeByte(0x840000 | address) : (byte)0,
                value, "contact crumble native packed sound bytes");
        }
    }

    /// <summary>Checks the three native restoration lists and their exact draw-pointer ownership.</summary>
    private static void VerifyContactCrumbleRestorationMapping(CartridgeImportAddressSpace rom)
    {
        ushort[] pointers = [0xa4a1, 0xa4a9, 0xa4b1];
        AssertTrue(pointers.SequenceEqual(RoomObjectLinkedRestoreVisualFiles.AllOf(LinkedRestoreObjectFamily.ContactCrumble).Select(draw => RoomObjectDrawAddressCodec.Of(draw.Draw))),
            "contact restoration export order matches native layouts");
        InstalledRoomObjectLinkedRestoreVisuals pictures = InstalledRoomObjectLinkedRestoreVisuals.Stock();
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool expected = pointers.Contains((ushort)pointer);
            bool found = LinkedRestoreObjectFamily.ContactCrumble.TryDescribe(NativeObjectDraws.At<ContactCrumbleRestoreDraw>(pointer), out var draw);
            bool exported = LinkedRestoreObjectFamily.ContactCrumble.TryGet(NativeObjectDraws.At<ContactCrumbleRestoreDraw>(pointer), out var list);
            AssertEqual(expected, found, "contact restoration full native pointer domain");
            AssertEqual(expected, exported, "contact restoration export pointer domain");
            if (!found)
            {
                AssertEqual(default(LinkedRestoreObjectFamily.Draw), draw, "missing restoration descriptor cleared");
                AssertEqual(default(RoomObjectShotBlockDrawDefinitions.DrawList), list, "missing restoration export cleared");
                continue;
            }
            int cursor = pointer;
            int run = 0;
            while (true)
            {
                ushort nativeCount = ReadRomWord(rom, 0x840000 | cursor);
                int count = nativeCount & 0x7fff;
                AssertEqual(2, count, "contact restoration two words per native run");
                AssertEqual((nativeCount & 0x8000) != 0, draw.Vertical, "contact restoration native orientation");
                var exportedRun = list.Runs.Span[run];
                AssertRunHeader(nativeCount, exportedRun, "contact restoration exported direction/count");
                AssertEqual(count, exportedRun.Count, "contact restoration exported word count");
                for (int block = 0; block < count; block++)
                {
                    RoomTerrainBlock native = RoomLevelWordCodec.Decode(ReadRomWord(rom, 0x840000 | (cursor + 2 + 2 * block)));
                    AssertEqual(native.Collision, draw.CollisionAt(run, block), "contact restoration calculated parent/child collision");
                    AssertEqual(native.Collision, exportedRun.Collisions.Span[block], "contact restoration exported collision");
                    AssertEqual(native.Appearance, pictures.Image(list.Draw).At(exportedRun.CellAt(block)), "contact restoration installed picture");
                }
                cursor += 2 + 2 * count;
                byte x = rom.ReadCartridgeByte(0x840000 | cursor);
                byte y = rom.ReadCartridgeByte(0x840000 | (cursor + 1));
                AssertEqual(unchecked((sbyte)x), RoomObjectRunContinuationCodecConstants.X(list, run), "contact restoration next X");
                AssertEqual(unchecked((sbyte)y), RoomObjectRunContinuationCodecConstants.Y(list, run), "contact restoration next Y");
                AssertEqual((byte)0, x, "contact restoration calculated next row X");
                AssertEqual(run + 1 < draw.RunCount ? (byte)1 : (byte)0, y, "contact restoration calculated next row Y");
                run++;
                cursor += 2;
                if (x == 0 && y == 0) break;
            }
            AssertEqual(run, draw.RunCount, "contact restoration complete calculated runs");
            AssertEqual(run, list.Runs.Length, "contact restoration complete exported runs");
            foreach (int invalid in new[] {int.MinValue, -1, run, int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => draw.CollisionAt(invalid, 0), "contact restoration run bounds");
            foreach (int invalid in new[] {int.MinValue, -1, 2, int.MaxValue})
                AssertThrows<ArgumentOutOfRangeException>(() => draw.CollisionAt(0, invalid), "contact restoration block bounds");
        }
    }

    /// <summary>Runs all eight BTS timelines against retail behavior with compiled source bytes blocked, then checks the stock breakup and restored art.</summary>
    internal static void VerifyContactCrumblePrograms()
    {
        CartridgeImportAddressSpace rom = SuperMetroid.AssetExtraction.Cartridge.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyContactCrumbleControlMapping), () => VerifyContactCrumbleControlMapping(rom));
        Suite(nameof(VerifyContactCrumbleDrawMapping), () => VerifyContactCrumbleDrawMapping(rom));
        Suite(nameof(VerifyContactCrumbleSoundMapping), () => VerifyContactCrumbleSoundMapping(rom));
        Suite(nameof(VerifyMirroredCrumbleShowsUnmirroredParent), VerifyMirroredCrumbleShowsUnmirroredParent);
        var forbidden = new HashSet<int>();
        foreach (ushort address in RoomObjectContactCrumbleProgramDefinitions.MechanicsWordAddresses())
        {
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
        }
        foreach (ushort address in RoomObjectContactCrumbleProgramDefinitions.MechanicsByteAddresses())
            forbidden.Add(0x840000 | address);

        Suite(nameof(VerifyContactCrumbleRestorationMapping), () => VerifyContactCrumbleRestorationMapping(rom));
        // Guard the independently identified native restoration byte regions.
        for (int address = 0xa4a1; address < 0xa4c1; address++)
            forbidden.Add(0x840000 | address);

        // The four shape-specific breakup animations are shared with shot and bomb
        // blocks. They must remain compiled even when a contact-crumble list selects them.
        foreach (RoomObjectShotBlockDrawDefinitions.DrawList list in
                 RoomObjectShotBlockVisualFiles.All)
        {
            int cursor = RoomObjectDrawAddressCodec.Of(list.Draw);
            foreach (RoomObjectShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                int length = 2 + 2 * run.Count + 2;
                for (int offset = 0; offset < length; offset++)
                    forbidden.Add(0x840000 | (cursor + offset));
                cursor += length;
            }
        }

        for (byte bts = 0; bts < 8; bts++)
        {
            ContactCrumbleFixture native = NewContactCrumbleFixture(bts);
            ContactCrumbleFixture compiled = NewContactCrumbleFixture(bts);
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
            {
                native.RoomObjects.Step(native.Level);
                compiled.RoomObjects.Step(compiled.Level);
                AssertEqual(native.RoomObjects.SlotTable.ActiveCount, compiled.RoomObjects.SlotTable.ActiveCount,
                    $"contact-crumble BTS {bts} active count, frame {frame}");
                AssertEqual(native.RoomObjects.Sounds.SoundRequests.Count, compiled.RoomObjects.Sounds.SoundRequests.Count,
                    $"contact-crumble BTS {bts} sound count, frame {frame}");
                for (int block = 0; block < 64; block++)
                {
                    AssertEqual(native.Level.GetCollisionBlockByIndex(block).Terrain,
                        compiled.Level.GetCollisionBlockByIndex(block).Terrain,
                        $"contact-crumble BTS {bts} block {block}, frame {frame}");
                    AssertEqual(RoomBlockBehaviorCodec.Encode(native.Level.GetCollisionBlockByIndex(block).Behavior),
                        RoomBlockBehaviorCodec.Encode(compiled.Level.GetCollisionBlockByIndex(block).Behavior),
                        $"contact-crumble BTS {bts} behavior {block}, frame {frame}");
                }
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"contact-crumble BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.RoomObjects.SlotTable.ActiveCount,
                $"contact-crumble BTS {bts} completes its native timeline");
        }

        Suite(nameof(VerifyContactCrumbleVisualSeparation), () => VerifyContactCrumbleVisualSeparation(rom, forbidden));

        Console.WriteLine($"Contact-crumble PLMs: 64 control words, 8 sound bytes, and 3 restoration lists match ROM; all eight programs run with source reads forbidden.");
    }

    /// <summary>Confirms the stock breakup frame's physical word and tile, and the native restored parent.</summary>
    private static void VerifyContactCrumbleVisualSeparation(
        CartridgeImportAddressSpace rom, HashSet<int> forbidden)
    {
        RenderBreakResult RenderBreak(
            InstalledRoomObjectShotBlockVisuals visuals)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new RoomTerrainBlock[width * width];
            words[blockIndex] = RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.SpecialBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new RoomBlockBehavior[words.Length], new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound(new RoomObjectVisuals { RoomObjectShotBlockVisuals = visuals, RoomObjectSetupBlocks = InstalledRoomObjectSetupBlocks.Stock() });
            AssertTrue(roomObjects.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, level.BlockAt(blockIndex),
                    CrumbleOf(0)),
                "visual test installs the real contact-crumble PLM");
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 4; frame++)
                roomObjects.Step(level);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "contact-break draw avoids compiled source bytes");
            return new RenderBreakResult(level.GetCollisionBlockByIndex(blockIndex).Terrain,
                level.DisplayedAppearance(blockIndex),
                level.DisplayedAppearance(blockIndex));
        }

        var stock = RenderBreak(InstalledRoomObjectShotBlockVisuals.Stock());
        AssertEqual(RoomTerrainBlock.Of(0x053, BlockMirroring.None, RoomCollisionType.Air), stock.Physical,
            "contact-crumble first frame installs native air collision");
        // The block BG1 shows names its picture (#1431): the stock breakup frame's block.
        AssertEqual(RoomBlockAppearance.Of(0x053), stock.Immediate,
            "stock contact breakup's block reaches the immediate redraw");

        ContactCrumbleProgramsRenderRestoredParentResult RenderRestoredParent()
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new RoomTerrainBlock[width * width];
            words[blockIndex] = RoomTerrainBlock.Of(0x321, BlockMirroring.None, RoomCollisionType.SpecialBlock);
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new RoomBlockBehavior[words.Length], new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
            var roomObjects = RoomObjectFixtures.Unbound();
            AssertTrue(roomObjects.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, level.BlockAt(blockIndex),
                    CrumbleOf(1)),
                "restored-art test installs linked contact-crumble PLM");
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
                roomObjects.Step(level);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "linked contact restoration uses no compiled source bytes");
            return new ContactCrumbleProgramsRenderRestoredParentResult(level.GetCollisionBlockByIndex(blockIndex).Terrain,
                level.DisplayedAppearance(blockIndex));
        }

        // A block's look is its picture in the block sheet (#1431), so the restored parent is checked as the block it
        // shows: block $0BC unmirrored, whose picture an edit of the block sheet repaints.
        var restoredParent = RenderRestoredParent();
        AssertEqual(RoomTerrainBlock.Of(0x0bc, BlockMirroring.None, RoomCollisionType.SpecialBlock), restoredParent.Physical,
            "linked contact block restores its native special parent word");
        AssertEqual(RoomBlockAppearance.Of(0x0bc), restoredParent.Streamed,
            "restored contact parent shows block $0BC to later streaming");
    }

    /// <summary>Constructed room and PLM owner used for one contact-crumble BTS timeline.</summary>
    /// <param name="Level">Room block plane containing the triggered contact tile.</param>
    /// <param name="RoomObjects">PLM system holding the native crumble instruction list.</param>
    internal sealed record ContactCrumbleFixture(
        RoomLevelData Level, RoomObjectSystem RoomObjects);

    /// <summary>
    /// A crumble block mirrored both ways keeps only its collision under its setup's parent block (#1581): the block it shows
    /// and restores is the parent, unmirrored, as the cartridge's setup writes it.
    /// </summary>
    private static void VerifyMirroredCrumbleShowsUnmirroredParent()
    {
        ContactCrumbleFixture fixture = NewContactCrumbleFixture(0, BlockMirroring.Horizontal | BlockMirroring.Vertical);
        AssertEqual(RoomBlockAppearance.Of(0x0bc), fixture.Level.GetCollisionBlockByIndex(27).Terrain.Appearance,
            "a mirrored crumble block's setup shows the parent block unmirrored");
    }

    /// <summary>Installs the selected contact-crumble BTS on a synthetic room block and spawns its native PLM program.</summary>
    private static ContactCrumbleFixture NewContactCrumbleFixture(byte bts, BlockMirroring mirroring = BlockMirroring.None)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new RoomTerrainBlock[width * width];
        words[blockIndex] = RoomTerrainBlock.Of(0x321, mirroring, RoomCollisionType.SpecialBlock);
        var behaviors = new RoomBlockBehavior[words.Length];
        behaviors[blockIndex] = RoomBlockBehaviorCodec.Decode(bts);
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, behaviors, new RoomBlockAppearance[words.Length]), RoomLevelData.NothingBeyondGrid());
        var roomObjects = RoomObjectFixtures.Unbound();
        AssertTrue(roomObjects.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, level.BlockAt(blockIndex),
                CrumbleOf(bts)),
            $"contact-crumble BTS {bts} installs its native program");
        return new ContactCrumbleFixture(level, roomObjects);
    }

    /// <summary>The crumble block a special block with <paramref name="bts"/> selects; the fixtures only place crumble blocks.</summary>
    private static ContactCrumbleBlock CrumbleOf(int bts) =>
        RoomBlockBehaviorCodec.Decode(bts).TryGetContactCrumble(out ContactCrumbleBlock crumble)
            ? crumble
            : throw new InvalidOperationException($"Verification failed: special-block BTS {bts} selects no crumble block.");

    /// <summary>Runs the contact-crumble program and restore-draw verification command.</summary>
    [VerificationCommand("--contact-crumble-programs")]
    internal static int ContactCrumbleProgramsCommand()
    {
        Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
        return 0;
    }
}
