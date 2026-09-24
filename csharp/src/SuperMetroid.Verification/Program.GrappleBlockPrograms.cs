using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyGrappleBlockPrograms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        var forbidden = new HashSet<int>();
        int wordCount = 0;
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsWordAddresses())
        {
            AssertTrue(RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(
                address, out ushort compiled), $"Grapple-block control ${address:X4} exists");
            ushort native = unchecked((ushort)(rom.ReadByte(0x840000 | address) |
                rom.ReadByte(0x840000 | (address + 1)) << 8));
            AssertEqual(native, compiled, $"Grapple-block control ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            forbidden.Add(0x840000 | (address + 1));
            wordCount++;
        }

        int byteCount = 0;
        foreach (ushort address in RoomPlmGrappleBlockProgramDefinitions.MechanicsByteAddresses())
        {
            AssertTrue(RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsByte(
                address, out byte compiled), $"Grapple-block sound ${address:X4} exists");
            AssertEqual(rom.ReadByte(0x840000 | address), compiled,
                $"Grapple-block sound ${address:X4} matches ROM");
            forbidden.Add(0x840000 | address);
            byteCount++;
        }

        AssertEqual(19, wordCount, "both Grapple-block programs contain all control words");
        AssertEqual(2, byteCount, "both Grapple-block programs contain sound bytes");
        AssertTrue(!RoomPlmGrappleBlockProgramDefinitions.TryReadMechanicsWord(
                RoomPlmInstructionLists.RespawningBreakableGrappleBlock + 2, out _),
            "first draw pointer remains outside the compiled control domain");

        int drawCount = 0;
        foreach (RoomPlmGrappleBlockDrawDefinitions.DrawList draw in
                 RoomPlmGrappleBlockDrawDefinitions.All)
        {
            ushort[] native = new ushort[2];
            for (int word = 0; word < native.Length; word++)
            {
                int address = 0x840000 | (draw.Pointer + word * 2);
                native[word] = unchecked((ushort)(rom.ReadByte(address) |
                    rom.ReadByte(address + 1) << 8));
                forbidden.Add(address);
                forbidden.Add(address + 1);
            }

            AssertEqual(RoomPlmGrappleBlockDrawDefinitions.DrawList.DirectionAndCount,
                native[0], $"Grapple draw ${draw.Pointer:X4} native block count");
            AssertEqual(draw.LevelWord, native[1],
                $"Grapple draw ${draw.Pointer:X4} native level word");
            int terminator = 0x840000 | (draw.Pointer + 4);
            AssertEqual(unchecked((byte)RoomPlmGrappleBlockDrawDefinitions.DrawList.NextX),
                rom.ReadByte(terminator),
                $"Grapple draw ${draw.Pointer:X4} native X terminator");
            AssertEqual(unchecked((byte)RoomPlmGrappleBlockDrawDefinitions.DrawList.NextY),
                rom.ReadByte(terminator + 1),
                $"Grapple draw ${draw.Pointer:X4} native Y terminator");
            forbidden.Add(terminator);
            forbidden.Add(terminator + 1);
            drawCount++;
        }

        AssertEqual(5, drawCount, "all breakable-Grapple-block draw lists are compiled");
        AssertTrue(!RoomPlmGrappleBlockDrawDefinitions.TryGet(0xa4f8, out _),
            "a nearby unknown draw pointer does not alias a compiled Grapple list");

        for (byte bts = 1; bts <= 2; bts++)
        {
            GrappleBlockFixture native = NewGrappleBlockFixture(bts);
            GrappleBlockFixture compiled = NewGrappleBlockFixture(bts);
            var guarded = new ShotBlockProgramReadGuard(rom, forbidden);
            for (int frame = 0; frame < 300; frame++)
            {
                native.Plms.Step(rom, native.Level, native.Streamer, 0, 0, 0);
                compiled.Plms.Step(guarded, compiled.Level, compiled.Streamer, 0, 0, 0);
                AssertEqual(native.Plms.ActiveCount, compiled.Plms.ActiveCount,
                    $"Grapple BTS {bts} active count, frame {frame}");
                AssertEqual(native.Plms.SoundRequests.Count, compiled.Plms.SoundRequests.Count,
                    $"Grapple BTS {bts} sound count, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).LevelWord,
                    compiled.Level.GetCollisionBlockByIndex(27).LevelWord,
                    $"Grapple BTS {bts} level word, frame {frame}");
                AssertEqual(native.Level.GetCollisionBlockByIndex(27).Behavior,
                    compiled.Level.GetCollisionBlockByIndex(27).Behavior,
                    $"Grapple BTS {bts} BTS, frame {frame}");
            }

            AssertEqual(0, guarded.ForbiddenReadAttempts,
                $"Grapple BTS {bts} never rereads compiled control bytes");
            AssertEqual(0, compiled.Plms.ActiveCount,
                $"Grapple BTS {bts} finishes its cartridge timeline");
        }

        Console.WriteLine($"Grapple-block PLMs: {wordCount} control words, {byteCount} sound bytes, and {drawCount} draw lists match ROM; both programs run with source reads forbidden.");
    }

    private sealed record GrappleBlockFixture(
        RoomLevelData Level, BackgroundTilemapStreamer Streamer, RoomPlmSystem Plms);

    private static GrappleBlockFixture NewGrappleBlockFixture(byte bts)
    {
        const int width = 8;
        const int blockIndex = 27;
        var words = new ushort[width * width];
        words[blockIndex] = 0xe123;
        var level = new RoomLevelData(width, width, words,
            new byte[words.Length], new ushort[words.Length], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnBreakableGrappleBlock(level, blockIndex, bts),
            $"Grapple BTS {bts} installs its native program");
        return new GrappleBlockFixture(level, level.CreateBackgroundStreamer(), plms);
    }
}
