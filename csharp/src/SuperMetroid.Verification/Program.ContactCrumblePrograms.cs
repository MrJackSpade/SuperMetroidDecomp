using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyContactCrumblePrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        int wordCount = 0;
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsWord(
                address, out ushort compiled), $"contact-crumble control ${address:X4} exists");
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled,
                $"contact-crumble control ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmContactCrumbleProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsByte(
                address, out byte compiled), $"contact-crumble sound ${address:X4} exists");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"contact-crumble sound ${address:X4} matches pinned cartridge");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(64, wordCount, "all eight contact-crumble control streams are compiled");
        AssertEqual(8, byteCount, "all eight contact-crumble sound operands are compiled");
        AssertTrue(!RoomPlmContactCrumbleProgramDefinitions.TryReadMechanicsWord(
                RoomPlmInstructionLists.ContactCrumble1x1Respawning + 5, out _),
            "an interleaved draw pointer is not classified as control");

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

        Console.WriteLine($"Contact-crumble PLMs: {wordCount} control words, {byteCount} sound bytes, and {restoreCount} restoration lists match ROM; all eight programs run with source reads forbidden.");
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
