using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

/// <summary>
/// Executes every translated pure-scroll door callback against the bytes in the private
/// retail cartridge and compares its complete 50-byte result with the C# definition table.
/// </summary>
internal static class DoorSetupCallbackAudit
{
    private const int ScratchScrollSource = 0x7e2000;
    private const int ExpectedPureScrollCallbackCount = 74;
    private const byte UnwrittenSentinel = 0x7f;

    public static int Run(string romPath)
    {
        SuperMetroidAddressSpace bus = SuperMetroidAddressSpace.LoadRetailRom(romPath);
        ushort[] pointers = DoorScrollPrograms.Pointers.Order().ToArray();
        if (pointers.Length != ExpectedPureScrollCallbackCount)
        {
            throw new InvalidDataException(
                $"Translated {pointers.Length} pure door callbacks; the retail catalog contains " +
                $"{ExpectedPureScrollCallbackCount}.");
        }

        foreach (ushort pointer in pointers)
            VerifyProgram(bus, pointer);

        Console.WriteLine(
            $"Door setup callback audit passed {pointers.Length} pure scroll programs " +
            "against their cartridge instructions.");
        return 0;
    }

    private static void VerifyProgram(SuperMetroidAddressSpace bus, ushort pointer)
    {
        var expected = Enumerable.Repeat(
            UnwrittenSentinel,
            RoomScrollGrid.StorageByteCount).ToArray();
        ExecuteCartridgeScrollProgram(bus, pointer, expected);

        for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
            bus.WriteByte(ScratchScrollSource + index, UnwrittenSentinel);
        RoomScrollGrid actual = RoomScrollGrid.LoadExplicit(
            bus,
            ScratchScrollSource,
            widthInScreens: 10,
            heightInScreens: 5);

        if (!DoorScrollPrograms.TryApply(pointer, actual))
            throw new InvalidDataException($"Door callback $8F:{pointer:X4} was not recognized.");

        for (int index = 0; index < RoomScrollGrid.StorageByteCount; index++)
        {
            if (actual.ReadStorage(index) == expected[index])
                continue;
            throw new InvalidDataException(
                $"Door callback $8F:{pointer:X4} produced scroll[{index:X2}]=" +
                $"${actual.ReadStorage(index):X2}; cartridge instructions produce " +
                $"${expected[index]:X2}.");
        }
    }

    /// <summary>
    /// Interprets only the straight-line instruction subset used by retail's pure scroll
    /// callbacks. Encountering any other opcode fails the audit instead of quietly blessing
    /// a C# definition after the cartridge routine gained an unmodeled side effect.
    /// </summary>
    private static void ExecuteCartridgeScrollProgram(
        ISnesAddressSpace bus,
        ushort pointer,
        Span<byte> scrolls)
    {
        int pc = 0x8f0000 | pointer;
        bool accumulatorIsEightBit = false;
        ushort accumulator = 0;

        for (int instruction = 0; instruction < 32; instruction++)
        {
            byte opcode = bus.ReadByte(pc++);
            switch (opcode)
            {
                case 0x08: // PHP
                case 0x28: // PLP
                    break;

                case 0xe2: // SEP #$20
                    byte sepMask = bus.ReadByte(pc++);
                    if (sepMask != 0x20)
                        throw Unsupported(pointer, opcode, pc - 2);
                    accumulatorIsEightBit = true;
                    break;

                case 0xa9: // LDA immediate
                    accumulator = bus.ReadByte(pc++);
                    if (!accumulatorIsEightBit)
                        accumulator |= unchecked((ushort)(bus.ReadByte(pc++) << 8));
                    break;

                case 0x8f: // STA long
                    int destination = bus.ReadByte(pc) |
                        (bus.ReadByte(pc + 1) << 8) |
                        (bus.ReadByte(pc + 2) << 16);
                    pc += 3;
                    int storageIndex = destination - RoomScrollGrid.WorkRamAddress;
                    if ((uint)storageIndex >= RoomScrollGrid.StorageByteCount)
                        throw Unsupported(pointer, opcode, pc - 4);
                    scrolls[storageIndex] = unchecked((byte)accumulator);
                    if (!accumulatorIsEightBit)
                    {
                        if (storageIndex + 1 >= scrolls.Length)
                            throw Unsupported(pointer, opcode, pc - 4);
                        scrolls[storageIndex + 1] = unchecked((byte)(accumulator >> 8));
                    }
                    break;

                case 0x60: // RTS
                    return;

                default:
                    throw Unsupported(pointer, opcode, pc - 1);
            }
        }

        throw new InvalidDataException(
            $"Door callback $8F:{pointer:X4} did not return within 32 instructions.");
    }

    private static InvalidDataException Unsupported(
        ushort pointer,
        byte opcode,
        int opcodeAddress) =>
        new(
            $"Door callback $8F:{pointer:X4} uses unsupported reference-audit opcode " +
            $"${opcode:X2} at ${opcodeAddress >> 16:X2}:{opcodeAddress & 0xffff:X4}.");
}
