using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    // Native list boundaries and timed-record counts from pinned bank_84.asm.
    private static (ushort Start, int Frames, ushort End)[] ContactCrumbleNativeLayouts() =>
        [(0xc9f9,7,0xca1c),(0xca1c,8,0xca41),(0xca41,8,0xca66),(0xca66,8,0xca8b),
         (0xca8b,4,0xcaa0),(0xcaa0,4,0xcab5),(0xcab5,4,0xcaca),(0xcaca,4,0xcadf)];

    private static void VerifyContactCrumbleControlMapping(SuperMetroidAddressSpace rom)
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
        AssertTrue(addresses.SequenceEqual(RoomPlmContactCrumbleProgramDefinitions.MechanicsWordAddresses()),
            "contact crumble complete native control enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsWord((ushort)address, out ushort value);
            AssertEqual(known.Contains((ushort)address), found, "contact crumble full control address domain");
            AssertEqual(found ? ReadBotwoonInstructionWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native timing, restoration and deletion controls");
        }
    }

    private static void VerifyContactCrumbleDrawMapping(SuperMetroidAddressSpace rom)
    {
        var addresses = new HashSet<ushort>();
        foreach (var layout in ContactCrumbleNativeLayouts())
        for (int frame = 0; frame < layout.Frames; frame++)
            addresses.Add((ushort)(layout.Start + 5 + frame * 4));
        AssertEqual(47, addresses.Count, "contact crumble native draw operand extent");
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmContactCrumbleProgramDefinitions.TryReadDrawPointerWord((ushort)address, out ushort value);
            AssertEqual(addresses.Contains((ushort)address), found, "contact crumble full draw address domain");
            AssertEqual(found ? ReadBotwoonInstructionWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native breakup, reverse reveal and linked restoration draws");
        }
    }

    private static void VerifyContactCrumbleSoundMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] addresses = [0xc9fb,0xca1e,0xca43,0xca68,0xca8d,0xcaa2,0xcab7,0xcacc];
        AssertTrue(addresses.SequenceEqual(RoomPlmContactCrumbleProgramDefinitions.MechanicsByteAddresses()),
            "contact crumble native packed sound enumeration");
        var known = addresses.ToHashSet();
        for (int address = 0; address <= ushort.MaxValue; address++)
        {
            bool found = RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsByte((ushort)address, out byte value);
            AssertEqual(known.Contains((ushort)address), found, "contact crumble full sound address domain");
            AssertEqual(found ? rom.ReadByte(0x840000 | address) : (byte)0,
                value, "contact crumble native packed sound bytes");
        }
    }

    private static void VerifyContactCrumblePrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        VerifyContactCrumbleControlMapping(rom);
        VerifyContactCrumbleDrawMapping(rom);
        VerifyContactCrumbleSoundMapping(rom);
        var forbidden = new HashSet<int>();
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsWordAddresses())
        {
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
        }
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsByteAddresses())
            forbidden.Add(0x840000 | address);

        int restoreCount = 0;
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmContactCrumbleRestoreDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                AssertEqual(run.DirectionAndCount,
                    unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                        rom.ReadByte(0x840000 | (cursor + 1)) << 8)),
                    $"contact restoration ${list.Pointer:X4} direction/count");
                forbidden.Add(0x840000 | cursor++);
                forbidden.Add(0x840000 | cursor++);
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(word,
                        unchecked((ushort)(rom.ReadByte(0x840000 | cursor) |
                            rom.ReadByte(0x840000 | (cursor + 1)) << 8)),
                        $"contact restoration ${list.Pointer:X4} level word");
                    forbidden.Add(0x840000 | cursor++);
                    forbidden.Add(0x840000 | cursor++);
                }

                AssertEqual(unchecked((byte)run.NextX), rom.ReadByte(0x840000 | cursor),
                    $"contact restoration ${list.Pointer:X4} next X");
                forbidden.Add(0x840000 | cursor++);
                AssertEqual(unchecked((byte)run.NextY), rom.ReadByte(0x840000 | cursor),
                    $"contact restoration ${list.Pointer:X4} next Y");
                forbidden.Add(0x840000 | cursor++);
            }

            restoreCount++;
        }

        AssertEqual(3, restoreCount, "all linked contact-crumble restoration lists are compiled");
        AssertTrue(!RoomPlmContactCrumbleRestoreDrawDefinitions.TryGet(0xa4a0, out _),
            "an unknown nearby contact restoration pointer does not alias a compiled list");

        // The four shape-specific breakup animations are shared with shot and bomb
        // blocks. They must remain compiled even when a contact-crumble list selects them.
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList list in
                 RoomPlmShotBlockDrawDefinitions.All)
        {
            int cursor = list.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in list.Runs.Span)
            {
                int length = 2 + 2 * run.LevelWords.Length + 2;
                for (int offset = 0; offset < length; offset++)
                    forbidden.Add(0x840000 | (cursor + offset));
                cursor += length;
            }
        }

        for (byte bts = 0; bts < 8; bts++)
        {
            ContactCrumbleFixture native = NewContactCrumbleFixture(bts);
            ContactCrumbleFixture compiled = NewContactCrumbleFixture(bts);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
            {
                native.Plms.Step(rom, native.Level, native.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(native.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"contact-crumble BTS {bts} active count, frame {frame}");
                AssertEqual(native.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"contact-crumble BTS {bts} sound count, frame {frame}");
                for (int block = 0; block < 64; block++)
                {
                    AssertEqual(native.Level.GetCollisionBlockByIndex(block).LevelWord,
                        compiled.Level.GetCollisionBlockByIndex(block).LevelWord,
                        $"contact-crumble BTS {bts} block {block}, frame {frame}");
                    AssertEqual(native.Level.GetCollisionBlockByIndex(block).Behavior,
                        compiled.Level.GetCollisionBlockByIndex(block).Behavior,
                        $"contact-crumble BTS {bts} behavior {block}, frame {frame}");
                }
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"contact-crumble BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"contact-crumble BTS {bts} completes its native timeline");
        }

        VerifyContactCrumbleVisualSeparation(rom, forbidden);
        VerifyLinkedRestoreVisualSeparation(rom, forbidden, bomb: false);

        Console.WriteLine($"Contact-crumble PLMs: 64 control words, 8 sound bytes, and {restoreCount} restoration lists match ROM; all eight programs run with source reads forbidden.");
    }

    private static void VerifyContactCrumbleVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden)
    {
        RoomPlmShotBlockVisualEntry[] entries = RoomPlmShotBlockDrawDefinitions.All
            .Select(list => new RoomPlmShotBlockVisualEntry(list.Pointer,
                list.Runs.Span.ToArray().Select(run =>
                    run.LevelWords.Span.ToArray().Select(word =>
                        new RoomLevelWord(word).VisualWord).ToArray()).ToArray()))
            .ToArray();
        RoomPlmShotBlockVisualEntry first = entries.Single(entry =>
            entry.DrawPointer == RoomPlmShotBlockDrawDefinitions.SingleFrame0);
        first.Runs[0][0] = 0x0054;
        var edited = new RoomPlmShotBlockVisualCatalog(entries);

        (ushort physical, ushort immediate, ushort streamed) RenderBreak(
            RoomPlmShotBlockVisualCatalog visuals)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new ushort[width * width];
            words[blockIndex] = 0xb321;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
            {
                definitions[0x53 * 8 + tile * 2] = 0x17;
                definitions[0x54 * 8 + tile * 2] = 0x18;
            }

            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var plms = new RoomPlmSystem { ShotBlockVisuals = visuals };
            AssertTrue(plms.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                    new RoomBlockBehavior(0)),
                "visual test installs the real contact-crumble PLM");
            var streamer = level.CreateBackgroundStreamer();
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 4; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "contact-break draw avoids compiled source bytes");
            AssertEqual(1, plms.TilemapUpdates.Count,
                "contact-break first frame publishes one immediate upload");
            return (level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                plms.TilemapUpdates[0].TopRow[0],
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0]);
        }

        var stock = RenderBreak(RoomPlmShotBlockVisualCatalog.Stock());
        var changed = RenderBreak(edited);
        AssertEqual((ushort)0x0053, stock.physical,
            "contact-crumble first frame installs native air collision");
        AssertEqual(stock.physical, changed.physical,
            "shared breakup-art edit leaves contact-crumble collision unchanged");
        AssertEqual((ushort)0x0017, stock.immediate,
            "stock contact breakup reaches the immediate tile upload");
        AssertEqual((ushort)0x0018, changed.immediate,
            "edited contact breakup reaches the immediate tile upload");
        AssertEqual((ushort)0x0018, changed.streamed,
            "edited contact breakup persists through camera streaming");

        (ushort physical, ushort streamed) RenderRestoredParent(byte tileIndex)
        {
            const int width = 8;
            const int blockIndex = 27;
            var words = new ushort[width * width];
            words[blockIndex] = 0xb321;
            var definitions = new byte[0x400 * 8];
            for (int tile = 0; tile < 4; tile++)
                definitions[0xbc * 8 + tile * 2] = tileIndex;
            var level = new RoomLevelData(width, width, words,
                new byte[words.Length], new ushort[words.Length], definitions);
            var plms = new RoomPlmSystem();
            AssertTrue(plms.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                    new RoomBlockBehavior(1)),
                "restored-art test installs linked contact-crumble PLM");
            var streamer = level.CreateBackgroundStreamer();
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
                plms.Step(guarded, level, streamer, 0, 0, 0);
            AssertEqual(0, guarded.ForbiddenReadAttempts,
                "linked contact restoration uses no compiled source bytes");
            return (level.GetCollisionBlockByIndex(blockIndex).LevelWord,
                level.CreateBackgroundStreamer().BuildPlmLevelBlockUpdate(
                    blockIndex, 0).TopRow[0]);
        }

        var stockParent = RenderRestoredParent(0x17);
        var editedParent = RenderRestoredParent(0x18);
        AssertEqual((ushort)0xb0bc, stockParent.physical,
            "linked contact block restores its native special parent word");
        AssertEqual(stockParent.physical, editedParent.physical,
            "editing block $0BC composition leaves special collision intact");
        AssertEqual((ushort)0x0017, stockParent.streamed,
            "stock restored contact parent uses room-block composition");
        AssertEqual((ushort)0x0018, editedParent.streamed,
            "edited block $0BC composition reaches later streaming");
    }

    private sealed record ContactCrumbleFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    private static ContactCrumbleFixture NewContactCrumbleFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xb321;
        var behaviors = new byte[words.Length];
        behaviors[blockIndex] = bts;
        var level = new RoomLevelData(width, width, words, behaviors,
            new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                new RoomBlockBehavior(bts)),
            $"contact-crumble BTS {bts} installs its native program");
        return new ContactCrumbleFixture(level, level.CreateBackgroundStreamer(), plms);
    }
}
