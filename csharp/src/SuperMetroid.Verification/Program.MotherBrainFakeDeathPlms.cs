using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledMotherBrainFakeDeathPlms()
    {
        SuperMetroidAddressSpace rom = SuperMetroidAddressSpace.LoadRetailRom(
            Path.GetFullPath("Super Metroid.smc"));
        ushort[] addresses = MotherBrainFakeDeathPlmProgramDefinitions
            .NativeWordAddresses().ToArray();
        AssertEqual(66, addresses.Length,
            "Mother Brain fake death owns twenty-two three-word programs");
        foreach (ushort address in addresses)
        {
            AssertTrue(MotherBrainFakeDeathPlmProgramDefinitions
                    .TryReadMechanicsWord(address, out ushort compiled),
                $"Mother Brain fake-death instruction ${address:X4} is compiled");
            AssertEqual(ReadWord(rom, address), compiled,
                $"Mother Brain fake-death instruction ${address:X4} matches ROM");
        }
        AssertTrue(!MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(
                MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive, out _),
            "following native region is not claimed as fake-death instructions");

        RoomPlmShotBlockDrawDefinitions.DrawList[] draws =
            MotherBrainFakeDeathPlmDrawDefinitions.All.ToArray();
        AssertEqual(22, draws.Length,
            "Mother Brain fake death owns twenty-two physical draws");
        for (int programIndex = 0;
             programIndex < MotherBrainFakeDeathPlmProgramDefinitions.ProgramCount;
             programIndex++)
        {
            ushort pointer = checked((ushort)(
                MotherBrainFakeDeathPlmProgramDefinitions.Start +
                programIndex * MotherBrainFakeDeathPlmProgramDefinitions.ProgramByteLength + 2));
            AssertTrue(MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(
                    pointer, out ushort drawPointer) &&
                MotherBrainFakeDeathPlmDrawDefinitions.TryGet(drawPointer, out _),
                $"Mother Brain program ${pointer - 2:X4} selects a compiled draw");
        }
        foreach (RoomPlmShotBlockDrawDefinitions.DrawList draw in draws)
        {
            int cursor = draw.Pointer;
            foreach (RoomPlmShotBlockDrawDefinitions.Run run in draw.Runs.Span)
            {
                AssertEqual(ReadWord(rom, cursor), run.DirectionAndCount,
                    $"Mother Brain draw ${draw.Pointer:X4} direction/count at ${cursor:X4}");
                cursor += 2;
                foreach (ushort word in run.LevelWords.Span)
                {
                    AssertEqual(ReadWord(rom, cursor), word,
                        $"Mother Brain draw ${draw.Pointer:X4} block at ${cursor:X4}");
                    cursor += 2;
                }
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextX),
                    $"Mother Brain draw ${draw.Pointer:X4} X offset");
                AssertEqual(rom.ReadByte(0x840000 | cursor++),
                    unchecked((byte)run.NextY),
                    $"Mother Brain draw ${draw.Pointer:X4} Y offset");
            }
            AssertTrue(cursor <= MotherBrainFakeDeathPlmDrawDefinitions.EndExclusive,
                $"Mother Brain draw ${draw.Pointer:X4} remains in its bounded region");
        }

        (ushort Header, ushort Program)[] reachable =
        [
            (RoomPlmHeaders.FillMotherBrainsWall, RoomPlmInstructionLists.FillMotherBrainsWall),
            (RoomPlmHeaders.MotherBrainsRoomEscapeDoor, RoomPlmInstructionLists.MotherBrainsRoomEscapeDoor),
            (RoomPlmHeaders.MotherBrainsBackgroundRow2, RoomPlmInstructionLists.MotherBrainsBackgroundRow2),
            (RoomPlmHeaders.MotherBrainsBackgroundRow3, RoomPlmInstructionLists.MotherBrainsBackgroundRow3),
            (RoomPlmHeaders.MotherBrainsBackgroundRow4, RoomPlmInstructionLists.MotherBrainsBackgroundRow4),
            (RoomPlmHeaders.MotherBrainsBackgroundRow5, RoomPlmInstructionLists.MotherBrainsBackgroundRow5),
            (RoomPlmHeaders.MotherBrainsBackgroundRow6, RoomPlmInstructionLists.MotherBrainsBackgroundRow6),
            (RoomPlmHeaders.MotherBrainsBackgroundRow7, RoomPlmInstructionLists.MotherBrainsBackgroundRow7),
            (RoomPlmHeaders.MotherBrainsBackgroundRow8, RoomPlmInstructionLists.MotherBrainsBackgroundRow8),
            (RoomPlmHeaders.MotherBrainsBackgroundRow9, RoomPlmInstructionLists.MotherBrainsBackgroundRow9),
            (RoomPlmHeaders.MotherBrainsBackgroundRowA, RoomPlmInstructionLists.MotherBrainsBackgroundRowA),
            (RoomPlmHeaders.MotherBrainsBackgroundRowB, RoomPlmInstructionLists.MotherBrainsBackgroundRowB),
            (RoomPlmHeaders.MotherBrainsBackgroundRowC, RoomPlmInstructionLists.MotherBrainsBackgroundRowC),
            (RoomPlmHeaders.MotherBrainsBackgroundRowD, RoomPlmInstructionLists.MotherBrainsBackgroundRowD),
            (RoomPlmHeaders.ClearMotherBrainCeilingBlock, RoomPlmInstructionLists.ClearMotherBrainCeilingBlock),
            (RoomPlmHeaders.ClearMotherBrainCeilingTube, RoomPlmInstructionLists.ClearMotherBrainCeilingTube),
            (RoomPlmHeaders.ClearMotherBrainBottomMiddleSideTube, RoomPlmInstructionLists.ClearMotherBrainBottomMiddleSideTube),
            (RoomPlmHeaders.ClearMotherBrainBottomMiddleTubes, RoomPlmInstructionLists.ClearMotherBrainBottomMiddleTubes),
            (RoomPlmHeaders.ClearMotherBrainBottomLeftTube, RoomPlmInstructionLists.ClearMotherBrainBottomLeftTube),
            (RoomPlmHeaders.ClearMotherBrainBottomRightTube, RoomPlmInstructionLists.ClearMotherBrainBottomRightTube),
        ];
        foreach ((ushort header, ushort program) in reachable)
            VerifyMotherBrainFakeDeathMutation(header, program);

        Console.WriteLine(
            "Mother Brain fake-death PLMs: 66 instruction words, 22 full draws, and 20 reachable production mutations match ROM without source reads.");

        static ushort ReadWord(ISnesAddressSpace bus, int address) =>
            (ushort)(bus.ReadByte(0x840000 | address) |
                bus.ReadByte(0x840000 | (address + 1)) << 8);
    }

    private static void VerifyMotherBrainFakeDeathMutation(
        ushort header, ushort program)
    {
        const int width = 32;
        const int height = 16;
        ushort[] words = Enumerable.Repeat((ushort)0x8123,
            width * height).ToArray();
        RoomLevelData level = CreateRoom(width, height, words,
            new byte[words.Length], blockDefinitions: new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertTrue(plms.TrySpawnMotherBrainMutation(level, 5, 3, header),
            $"Mother Brain mutation ${header:X4} allocates");
        ushort[] expected = Enumerable.Range(0, words.Length)
            .Select(index => level.GetCollisionBlockByIndex(index).LevelWord)
            .ToArray();
        AssertTrue(MotherBrainFakeDeathPlmProgramDefinitions.TryReadMechanicsWord(
                checked((ushort)(program + 2)), out ushort drawPointer),
            $"Mother Brain mutation ${header:X4} selects a compiled draw");
        AssertTrue(MotherBrainFakeDeathPlmDrawDefinitions.TryGet(
                drawPointer, out var draw),
            $"Mother Brain mutation ${header:X4} has a physical draw");
        for (int runIndex = 0; runIndex < draw.Runs.Length; runIndex++)
        {
            RoomPlmShotBlockDrawDefinitions.Run run = draw.Runs.Span[runIndex];
            int x = 5 + (runIndex == 0 ? 0 :
                draw.Runs.Span[runIndex - 1].NextX);
            int y = 3 + (runIndex == 0 ? 0 :
                draw.Runs.Span[runIndex - 1].NextY);
            bool vertical = (run.DirectionAndCount & 0x8000) != 0;
            for (int block = 0; block < run.LevelWords.Length; block++)
                expected[(y + (vertical ? block : 0)) * width +
                    x + (vertical ? 0 : block)] = run.LevelWords.Span[block];
        }
        var guard = new MotherBrainFakeDeathSourceGuard(new TestAddressSpace());
        BackgroundTilemapStreamer streamer = level.CreateBackgroundStreamer();
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(1, plms.ActiveCount,
            $"Mother Brain mutation ${header:X4} persists through its draw frame");
        for (int index = 0; index < expected.Length; index++)
            AssertEqual(expected[index],
                level.GetCollisionBlockByIndex(index).LevelWord,
                $"Mother Brain mutation ${header:X4} level block {index}");
        plms.Step(guard, level, streamer, 0, 0, 0);
        AssertEqual(0, plms.ActiveCount,
            $"Mother Brain mutation ${header:X4} deletes on the following frame");
        AssertEqual(0, guard.ForbiddenReadAttempts,
            $"Mother Brain mutation ${header:X4} reads no migrated source bytes");
    }

    private sealed class MotherBrainFakeDeathSourceGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            int pointer = address & 0xffff;
            if ((address >> 16) == 0x84 &&
                ((pointer >= MotherBrainFakeDeathPlmProgramDefinitions.Start &&
                  pointer < MotherBrainFakeDeathPlmProgramDefinitions.EndExclusive) ||
                 MotherBrainFakeDeathPlmDrawDefinitions.All.Any(draw =>
                     pointer >= draw.Pointer && pointer < draw.Pointer +
                         draw.Runs.Span.ToArray().Sum(run =>
                             4 + run.LevelWords.Length * 2))))
            {
                ForbiddenReadAttempts++;
                throw new InvalidOperationException(
                    $"Mother Brain mutation reread migrated source ${address:X6}.");
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
