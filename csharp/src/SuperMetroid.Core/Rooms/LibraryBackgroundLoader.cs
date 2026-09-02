using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Rooms;

/// <summary>
/// Executes bank-$82's room-state library-background command list into WRAM and VRAM.
/// </summary>
/// <remarks>
/// A room with an odd/fixed layer-2 scroll mode does not stream BG2 from level data. Its
/// state header instead points at one of these bank-$8F lists, which can decompress a static
/// tilemap into WRAM and DMA it to the two BG2 screen pages. Ignoring the list leaves the
/// previous room's tilemap resident—a particularly visible failure at Ceres room $DF8D.
/// </remarks>
public static class LibraryBackgroundLoader
{
    private const int RoomBank = 0x8f0000;
    private const int WorkRamBank = 0x7e0000;
    private const ushort Bg2TilemapBuffer = 0x4000;

    /// <summary>Runs one high-bank room-state list and returns its executed command count.</summary>
    public static int Execute(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort listPointer,
        ushort activeDoorPointer)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        if (unchecked((short)listPointer) >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(listPointer),
                $"Library-background pointers must address the high half of bank $8F, not ${listPointer:X4}.");
        }

        ushort cursor = listPointer;
        int executedCommands = 0;
        for (int guard = 0; guard < 128; guard++)
        {
            ushort commandAddress = cursor;
            ushort command = ReadWord(bus, cursor);
            cursor = unchecked((ushort)(cursor + 2));
            executedCommands++;

            switch ((LibraryBackgroundCommand)command)
            {
                case LibraryBackgroundCommand.End:
                    return executedCommands;

                case LibraryBackgroundCommand.TransferToVram:
                    cursor = TransferToVram(bus, vram, cursor);
                    break;

                case LibraryBackgroundCommand.DecompressToWorkRam:
                    cursor = DecompressToWorkRam(bus, cursor);
                    break;

                case LibraryBackgroundCommand.ClearFxTilemap:
                    // The retail command is unused, but its implementation is fully named:
                    // fill the shared $7E:4000 buffer and transfer $F00 bytes to VRAM $5880.
                    FillWords(bus, Bg2TilemapBuffer, byteCount: 0x0f00, value: 0x184e);
                    vram.ExecuteQueuedWrite(bus, 0x7e4000, 0x0f00, 0x5880);
                    break;

                case LibraryBackgroundCommand.TransferToVramForKraid:
                    // Kraid also changes BG3's character base after this ordinary transfer.
                    // That PPU-register side effect has no owner in the current room runtime;
                    // the VRAM behavior remains exact and the missing register is explicit.
                    cursor = TransferToVram(bus, vram, cursor);
                    break;

                case LibraryBackgroundCommand.ClearBg2:
                    ClearBg2(bus, vram, includeKraidPage: false);
                    break;

                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearBg2(bus, vram, includeKraidPage: true);
                    break;

                case LibraryBackgroundCommand.TransferForDoor:
                {
                    ushort candidateDoor = ReadWord(bus, cursor);
                    cursor = unchecked((ushort)(cursor + 2));
                    if (candidateDoor == activeDoorPointer)
                        cursor = TransferToVram(bus, vram, cursor);
                    else
                        cursor = unchecked((ushort)(cursor + 7));
                    break;
                }

                default:
                    throw new InvalidDataException(
                        $"Unknown library-background command ${command:X4} at $8F:{commandAddress:X4}.");
            }
        }

        throw new InvalidDataException(
            $"Library-background list $8F:{listPointer:X4} did not terminate within 128 commands.");
    }

    private static ushort TransferToVram(ISnesAddressSpace bus, SnesVram vram, ushort cursor)
    {
        int sourceAddress = ReadLong(bus, cursor);
        ushort destinationWord = ReadWord(bus, unchecked((ushort)(cursor + 3)));
        ushort byteCount = ReadWord(bus, unchecked((ushort)(cursor + 5)));
        if (byteCount == 0)
        {
            throw new InvalidDataException(
                $"Library-background transfer at $8F:{cursor:X4} uses unsupported DMA size zero.");
        }

        // Command 2 configures the same $2118/$2119 consecutive-word DMA represented by
        // ExecuteQueuedWrite. Its destination is a VRAM word, not a host byte offset.
        vram.ExecuteQueuedWrite(bus, sourceAddress, byteCount, destinationWord);
        return unchecked((ushort)(cursor + 7));
    }

    private static ushort DecompressToWorkRam(ISnesAddressSpace bus, ushort cursor)
    {
        int sourceAddress = ReadLong(bus, cursor);
        ushort destination = ReadWord(bus, unchecked((ushort)(cursor + 3)));
        byte[] decompressed = RomDataReader.Decompress(bus, sourceAddress);
        if (destination + decompressed.Length > 0x10000)
        {
            throw new InvalidDataException(
                $"Library-background decompression from ${sourceAddress:X6} crosses bank $7E.");
        }

        for (int index = 0; index < decompressed.Length; index++)
            bus.WriteByte(WorkRamBank | (destination + index), decompressed[index]);
        return unchecked((ushort)(cursor + 5));
    }

    private static void ClearBg2(ISnesAddressSpace bus, SnesVram vram, bool includeKraidPage)
    {
        // Clear_BG2_Tilemap fills both $800-byte WRAM pages with tile $0338, then performs
        // one $1000-byte DMA. Kraid repeats that same buffer at VRAM $4000 as well.
        FillWords(bus, Bg2TilemapBuffer, byteCount: 0x1000, value: 0x0338);
        if (includeKraidPage)
            vram.ExecuteQueuedWrite(bus, 0x7e4000, 0x1000, 0x4000);
        vram.ExecuteQueuedWrite(bus, 0x7e4000, 0x1000, 0x4800);
    }

    private static void FillWords(
        ISnesAddressSpace bus,
        ushort destination,
        int byteCount,
        ushort value)
    {
        if ((byteCount & 1) != 0 || destination + byteCount > 0x10000)
            throw new ArgumentOutOfRangeException(nameof(byteCount));

        for (int byteOffset = 0; byteOffset < byteCount; byteOffset += 2)
        {
            bus.WriteByte(WorkRamBank | (destination + byteOffset), (byte)value);
            bus.WriteByte(WorkRamBank | (destination + byteOffset + 1), (byte)(value >> 8));
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomBank | pointer;
        return unchecked((ushort)(
            bus.ReadByte(address) |
            (bus.ReadByte(RoomBank | unchecked((ushort)(pointer + 1))) << 8)));
    }

    private static int ReadLong(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadByte(RoomBank | pointer) |
        (bus.ReadByte(RoomBank | unchecked((ushort)(pointer + 1))) << 8) |
        (bus.ReadByte(RoomBank | unchecked((ushort)(pointer + 2))) << 16);
}
