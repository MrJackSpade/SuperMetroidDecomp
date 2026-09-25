using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

internal static partial class Program
{
    private static void VerifyCompiledRoomScrollPrograms(SuperMetroidAddressSpace rom)
    {
        ushort[] pointers = RoomPlmScrollProgramDefinitions.Pointers.Order().ToArray();
        AssertEqual(RoomPlmScrollProgramDefinitions.RetailProgramCount,
            pointers.Length, "retail scroll-program count");
        int byteCount = 0;
        int pairCount = 0;
        foreach (ushort pointer in pointers)
        {
            ReadOnlySpan<byte> program = RoomPlmScrollProgramDefinitions.Get(pointer).Span;
            for (int offset = 0; offset < program.Length; offset++)
                AssertEqual(rom.ReadByte(0x8f0000 | (pointer + offset)),
                    program[offset],
                    $"scroll program $8F:{pointer:X4} byte {offset} matches ROM");
            byteCount += program.Length;
            pairCount += (program.Length - 1) / 2;
        }
        AssertEqual(RoomPlmScrollProgramDefinitions.RetailByteCount,
            byteCount, "retail scroll-program byte count");
        AssertEqual(RoomPlmScrollProgramDefinitions.RetailPairCount,
            pairCount, "retail scroll-program pair count");
        AssertThrows<InvalidDataException>(
            () => RoomPlmScrollProgramDefinitions.Get(0xffff),
            "unknown retail scroll-program pointer fails loudly");

        // Retail population $8F:8230 contains one resident $B703 trigger at
        // (8,13). Its $8F:94FA program changes storage cell zero to green.
        // Guard every compiled retail program range during the actual room PLM
        // load/touch/handler path. Constructed-room programs are exercised by
        // VerifyRoomScrollPlms and must continue to read their supplied bus.
        var guarded = new RetailScrollProgramReadGuard(rom);
        const int width = 64;
        var level = new RoomLevelData(width, 64,
            new ushort[width * 64], new byte[width * 64],
            new ushort[width * 64], new byte[0x400 * 8]);
        var plms = new RoomPlmSystem();
        AssertEqual(1, plms.LoadRoomPopulation(guarded, level,
                level.CreateBackgroundStreamer(), new SnesVram(), 0x8230,
                new Bank80SystemState(), AreaId.Crateria,
                () => new SamusState(), () => false,
                useCompiledRetailPopulation: true),
            "retail scroll-only population loads from compiled placement");
        RoomScrollGrid scrolls = RoomScrollGrid.CreateImplicit(guarded,
            widthInScreens: 4, heightInScreens: 4,
            lastRowState: RoomScrollState.RedBoundary);
        scrolls.SetStorage(0, RoomScrollState.RedBoundary);
        AssertTrue(plms.TryNotifyScrollTouch(level.GetBlockIndex(8, 13)),
            "retail scroll trigger wakes at its authored coordinate");
        plms.Step(guarded, level, level.CreateBackgroundStreamer(),
            0, 0, 0, scrolls);
        AssertEqual((byte)RoomScrollState.Green, scrolls.ReadStorage(0),
            "compiled retail scroll program mutates the intended cell");
        AssertTrue(!plms.ScrollPlms[0].Triggered,
            "retail scroll trigger returns to resident sleep");
        AssertEqual(0, guarded.ForbiddenReadAttempts,
            "retail scroll PLM did not read any compiled program from the ROM bus");
    }

    private sealed class RetailScrollProgramReadGuard(ISnesAddressSpace source)
        : ISnesAddressSpace
    {
        internal int ForbiddenReadAttempts { get; private set; }

        public byte ReadByte(int address)
        {
            foreach (ushort pointer in RoomPlmScrollProgramDefinitions.Pointers)
            {
                int first = 0x8f0000 | pointer;
                if (address >= first &&
                    address < first + RoomPlmScrollProgramDefinitions.Get(pointer).Length)
                {
                    ForbiddenReadAttempts++;
                    throw new InvalidOperationException(
                        $"Retail scroll PLM reread compiled program byte ${address:X6}.");
                }
            }
            return source.ReadByte(address);
        }

        public void WriteByte(int address, byte value) => source.WriteByte(address, value);
    }
}
