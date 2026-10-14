using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using static SuperMetroid.Verification.Bosses.MotherBrain.BabyMetroidRouteDefinitionsVerification;
using static SuperMetroid.Verification.Rooms.LevelData.LinkedRestoreVisualsVerification;
using static SuperMetroid.Verification.Rooms.Plms.ShotBlockPlmProgramsVerification;
using static SuperMetroid.Verification.Harness.SuiteTimingVerification;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Rooms.Plms;
namespace SuperMetroid.Verification.Rooms.Plms;
using SuperMetroid.Verification.Harness;

/// <summary>Retail and guarded playback checks for all eight contact-crumble PLM programs and linked restore draws.</summary>
internal static class ContactCrumbleProgramsVerification
{
    /// <summary>Native bank-$84 list bounds and timed-record counts from the pinned disassembly.</summary>
    private static (ushort Start, int Frames, ushort End)[] ContactCrumbleNativeLayouts() =>
        [(0xc9f9,7,0xca1c),(0xca1c,8,0xca41),(0xca41,8,0xca66),(0xca66,8,0xca8b),
         (0xca8b,4,0xcaa0),(0xcaa0,4,0xcab5),(0xcab5,4,0xcaca),(0xcaca,4,0xcadf)];

    /// <summary>Compares compiled timing, restoration and deletion words over the full native address domain.</summary>
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
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native timing, restoration and deletion controls");
        }
    }

    /// <summary>Checks every timed draw operand and rejects ownership of non-draw bank-$84 words.</summary>
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
            AssertEqual(found ? ReadRomWord(rom, 0x840000 | address) : (ushort)0,
                value, "contact crumble native breakup, reverse reveal and linked restoration draws");
        }
    }

    /// <summary>Compares packed sound bytes at the eight list starts against native bank-$84 data.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for contact crumble sound mapping")]
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
            AssertEqual(found ? rom.ReadCartridgeByte(0x840000 | address) : (byte)0,
                value, "contact crumble native packed sound bytes");
        }
    }

    /// <summary>Checks the three native restoration lists and their exact draw-pointer ownership.</summary>
    [CpuBusBoundary("exact bus test: retail ROM oracle for contact crumble restoration mapping")]
    private static void VerifyContactCrumbleRestorationMapping(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = [0xa4a1, 0xa4a9, 0xa4b1];
        AssertTrue(pointers.SequenceEqual(RoomPlmContactCrumbleRestoreDrawDefinitions.All.Select(draw => draw.Pointer)),
            "contact restoration export order matches native layouts");
        for (int pointer = 0; pointer <= ushort.MaxValue; pointer++)
        {
            bool expected = pointers.Contains((ushort)pointer);
            bool found = RoomPlmContactCrumbleRestoreDrawDefinitions.TryDescribe((ushort)pointer, out var draw);
            bool exported = RoomPlmContactCrumbleRestoreDrawDefinitions.TryGet((ushort)pointer, out var list);
            AssertEqual(expected, found, "contact restoration full native pointer domain");
            AssertEqual(expected, exported, "contact restoration export pointer domain");
            if (!found)
            {
                AssertEqual(default(RoomPlmContactCrumbleRestoreDrawDefinitions.Draw), draw, "missing restoration descriptor cleared");
                AssertEqual(default(RoomPlmShotBlockDrawDefinitions.DrawList), list, "missing restoration export cleared");
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
                AssertEqual(nativeCount, exportedRun.DirectionAndCount, "contact restoration exported direction/count");
                AssertEqual(count, exportedRun.LevelWords.Length, "contact restoration exported word count");
                for (int block = 0; block < count; block++)
                {
                    ushort native = ReadRomWord(rom, 0x840000 | (cursor + 2 + 2 * block));
                    AssertEqual(native, draw.WordAt(run, block), "contact restoration calculated parent/child word");
                    AssertEqual(native, exportedRun.LevelWords.Span[block], "contact restoration exported word");
                }
                cursor += 2 + 2 * count;
                byte x = rom.ReadCartridgeByte(0x840000 | cursor);
                byte y = rom.ReadCartridgeByte(0x840000 | (cursor + 1));
                AssertEqual(unchecked((sbyte)x), exportedRun.Next.X, "contact restoration next X");
                AssertEqual(unchecked((sbyte)y), exportedRun.Next.Y, "contact restoration next Y");
                AssertEqual((byte)0, x, "contact restoration calculated next row X");
                AssertEqual(run + 1 < draw.RunCount ? (byte)1 : (byte)0, y, "contact restoration calculated next row Y");
                run++;
                cursor += 2;
                if (x == 0 && y == 0) break;
            }
            AssertEqual(run, draw.RunCount, "contact restoration complete calculated runs");
            AssertEqual(run, list.Runs.Length, "contact restoration complete exported runs");
            foreach (int invalid in new[] {int.MinValue, -1, run, int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(invalid, 0), "contact restoration run bounds");
            foreach (int invalid in new[] {int.MinValue, -1, 2, int.MaxValue})
                AssertThrows<IndexOutOfRangeException>(() => draw.WordAt(0, invalid), "contact restoration block bounds");
        }
    }

    /// <summary>Runs all eight BTS timelines against retail behavior with compiled source bytes blocked, then checks edited visual isolation.</summary>
    internal static void VerifyContactCrumblePrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroid.AssetExtraction.CartridgeImportAddressSpaceTooling.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        Suite(nameof(VerifyContactCrumbleControlMapping), () => VerifyContactCrumbleControlMapping(rom));
        Suite(nameof(VerifyContactCrumbleDrawMapping), () => VerifyContactCrumbleDrawMapping(rom));
        Suite(nameof(VerifyContactCrumbleSoundMapping), () => VerifyContactCrumbleSoundMapping(rom));
        var forbidden = new HashSet<int>();
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsWordAddresses())
        {
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
        }
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsByteAddresses())
            forbidden.Add(0x840000 | address);

        Suite(nameof(VerifyContactCrumbleRestorationMapping), () => VerifyContactCrumbleRestorationMapping(rom));
        // Guard the independently identified native restoration byte regions.
        for (int address = 0xa4a1; address < 0xa4c1; address++)
            forbidden.Add(0x840000 | address);

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
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
            {
                native.Plms.Step(rom, native.Level, native.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
                AssertEqual(native.Plms.SlotTable.ActiveCount, compiled.Plms.SlotTable.ActiveCount,
                    $"contact-crumble BTS {bts} active count, frame {frame}");
                AssertEqual(native.Plms.Sounds.SoundRequests.Count, compiled.Plms.Sounds.SoundRequests.Count,
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
            AssertEqual(0, compiled.Plms.SlotTable.ActiveCount,
                $"contact-crumble BTS {bts} completes its native timeline");
        }

        Suite(nameof(VerifyContactCrumbleVisualSeparation), () => VerifyContactCrumbleVisualSeparation(rom, forbidden));
        Suite(nameof(VerifyLinkedRestoreVisualSeparation), () => VerifyLinkedRestoreVisualSeparation(rom, forbidden, bomb: false));

        Console.WriteLine($"Contact-crumble PLMs: 64 control words, 8 sound bytes, and 3 restoration lists match ROM; all eight programs run with source reads forbidden.");
    }

    /// <summary>Confirms an edited breakup tile changes the immediate and streamed visual while physical block behavior stays native.</summary>
    private static void VerifyContactCrumbleVisualSeparation(
        SuperMetroidAddressSpace rom, HashSet<int> forbidden)
    {
        RoomPlmShotBlockVisualEntry[] entries = RoomPlmShotBlockDrawDefinitions.All
            .Select(list => new RoomPlmShotBlockVisualEntry(list.Pointer,
                list.Runs.Span.SelectToArray(run =>
                    run.LevelWords.Span.SelectToArray(word =>
                        new RoomLevelWord(word).VisualWord))))
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

            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), definitions);
            var plms = new RoomPlmSystem { Drawing = { ShotBlockVisuals = visuals } };
            AssertTrue(plms.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                    new RoomBlockBehavior(0)),
                "visual test installs the real contact-crumble PLM");
            var streamer = level.CreateBackgroundStreamer();
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 4; frame++)
                plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
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
            var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, new byte[words.Length], new ushort[words.Length]), definitions);
            var plms = new RoomPlmSystem();
            AssertTrue(plms.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                    new RoomBlockBehavior(1)),
                "restored-art test installs linked contact-crumble PLM");
            var streamer = level.CreateBackgroundStreamer();
            var guarded = NewShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 100; frame++)
                plms.Step(guarded, level, streamer, new PlmView { Layer1Position = new(0, 0), Bg1XOffset = 0 });
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

    /// <summary>Constructed room, streamer and PLM owner used for one contact-crumble BTS timeline.</summary>
    /// <param name="Level">Room block plane containing the triggered contact tile.</param>
    /// <param name="Streamer">BG tilemap update owner observing restored artwork.</param>
    /// <param name="Plms">PLM system holding the native crumble instruction list.</param>
    internal sealed record ContactCrumbleFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    /// <summary>Installs the selected contact-crumble BTS on a synthetic room block and spawns its native PLM program.</summary>
    private static ContactCrumbleFixture NewContactCrumbleFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xb321;
        var behaviors = new byte[words.Length];
        behaviors[blockIndex] = bts;
        var level = new RoomLevelData(new Extent16<RoomBlocks>(width, width), new RoomLevelPlanes(words, behaviors, new ushort[words.Length]), new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.BlockReactions.TrySpawnSamusContactCrumbleBlock(level, blockIndex,
                new RoomBlockBehavior(bts)),
            $"contact-crumble BTS {bts} installs its native program");
        return new ContactCrumbleFixture(level, level.CreateBackgroundStreamer(), plms);
    }

    /// <summary>Runs the contact-crumble program and restore-draw verification command.</summary>
    [VerificationCommand("--contact-crumble-programs")]
    internal static int ContactCrumbleProgramsCommand()
    {
        Suite(nameof(VerifyContactCrumblePrograms), () => VerifyContactCrumblePrograms());
        return 0;
    }
}
