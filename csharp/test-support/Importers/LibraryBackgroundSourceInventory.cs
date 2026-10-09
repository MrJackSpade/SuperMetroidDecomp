using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Enumerates visual ROM inputs named by every compiled room background command list.</summary>
internal static class LibraryBackgroundSourceInventory
{
    /// <summary>
    /// Walks the distinct native library-background lists referenced by room definitions
    /// and records source and destination operands for commands that move background data.
    /// </summary>
    /// <param name="bus">Cartridge address space containing bank-$8F command lists and their operands.</param>
    /// <returns>Asset-bearing commands in list order; end markers and tilemap-clear commands have no entries.</returns>
    public static IReadOnlyList<LibraryBackgroundSource> Scan(ISnesAddressSpace bus)
    {
        ArgumentNullException.ThrowIfNull(bus);
        var sources = new List<LibraryBackgroundSource>();
        foreach (ushort listPointer in RoomStateDefinitions.All
            .Select(state => state.BackgroundDataPointer)
            .Where(pointer => unchecked((short)pointer) < 0)
            .Distinct()
            .Order())
        {
            ushort cursor = listPointer;
            bool terminated = false;
            for (int guard = 0; guard < RoomAssetRomData.LibraryBackground.MaximumCommandsPerList;
                guard++)
            {
                ushort commandPointer = cursor;
                LibraryBackgroundCommand command = (LibraryBackgroundCommand)ReadWord(bus, cursor);
                cursor = unchecked((ushort)(cursor + 2));
                switch (command)
                {
                    case LibraryBackgroundCommand.End:
                        terminated = true;
                        break;

                    case LibraryBackgroundCommand.DecompressToWorkRam:
                        sources.Add(new LibraryBackgroundSource(listPointer, commandPointer,
                            command, ReadLong(bus, cursor), ReadWord(bus,
                                unchecked((ushort)(cursor + 3))), null, null));
                        cursor = unchecked((ushort)(cursor + 5));
                        break;

                    case LibraryBackgroundCommand.TransferToVram:
                    case LibraryBackgroundCommand.TransferToVramForKraid:
                        sources.Add(ReadTransfer(listPointer, commandPointer, command, cursor));
                        cursor = unchecked((ushort)(cursor + 7));
                        break;

                    case LibraryBackgroundCommand.TransferForDoor:
                    {
                        ushort door = ReadWord(bus, cursor);
                        cursor = unchecked((ushort)(cursor + 2));
                        sources.Add(ReadTransfer(listPointer, commandPointer, command, cursor)
                            with { DoorPointer = door });
                        cursor = unchecked((ushort)(cursor + 7));
                        break;
                    }

                    case LibraryBackgroundCommand.ClearFxTilemap:
                    case LibraryBackgroundCommand.ClearBg2:
                    case LibraryBackgroundCommand.ClearBg2ForKraid:
                        break;

                    default:
                        throw new InvalidDataException(
                            $"Unknown library-background command ${(ushort)command:X4} " +
                            $"at $8F:{commandPointer:X4}.");
                }
                if (terminated) break;
            }
            if (!terminated)
                throw new InvalidDataException(
                    $"Library-background list $8F:{listPointer:X4} did not terminate.");
        }
        return sources;

        LibraryBackgroundSource ReadTransfer(ushort list, ushort commandPointer,
            LibraryBackgroundCommand command, ushort operandPointer) =>
            new(list, commandPointer, command, ReadLong(bus, operandPointer), null,
                ReadWord(bus, unchecked((ushort)(operandPointer + 3))),
                ReadWord(bus, unchecked((ushort)(operandPointer + 5))));
    }

    /// <summary>Reads a little-endian 16-bit operand from bank $8F.</summary>
    /// <param name="bus">Cartridge address space containing the command list.</param>
    /// <param name="pointer">Bank-local address of the operand's low byte.</param>
    /// <returns>The low byte followed by the high byte as a 16-bit value.</returns>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
        return unchecked((ushort)(bus.ReadCartridgeByte(address) |
            (bus.ReadCartridgeByte(RoomAssetRomData.LibraryBackground.CommandBank |
                unchecked((ushort)(pointer + 1))) << 8)));
    }

    /// <summary>Reads a three-byte little-endian bus address operand from bank $8F.</summary>
    /// <param name="bus">Cartridge address space containing the command list.</param>
    /// <param name="pointer">Bank-local address of the operand's least-significant byte.</param>
    /// <returns>The three operand bytes combined into a 24-bit bus address.</returns>
    private static int ReadLong(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadCartridgeByte(RoomAssetRomData.LibraryBackground.CommandBank | pointer) |
        (bus.ReadCartridgeByte(RoomAssetRomData.LibraryBackground.CommandBank |
            unchecked((ushort)(pointer + 1))) << 8) |
        (bus.ReadCartridgeByte(RoomAssetRomData.LibraryBackground.CommandBank |
            unchecked((ushort)(pointer + 2))) << 16);
}

/// <summary>One source operand in a native library-background list, not a synthesized image.</summary>
/// <param name="ListPointer">Bank-$8F pointer identifying the containing room background-command list.</param>
/// <param name="CommandPointer">Bank-local address of this command's opcode word.</param>
/// <param name="Command">Operation that determines which source and destination operands are populated.</param>
/// <param name="SourceAddress">24-bit bus address read by the transfer or decompression command.</param>
/// <param name="WorkRamDestination">Bank-$7E destination offset for decompression, or <see langword="null"/> for other commands.</param>
/// <param name="VramDestination">VRAM word destination for transfer commands, or <see langword="null"/> otherwise.</param>
/// <param name="TransferByteCount">Number of bytes transferred to VRAM, or <see langword="null"/> when the command has no transfer.</param>
/// <param name="DoorPointer">Door identity gating a door-specific transfer; absent for commands that run unconditionally.</param>
internal sealed record LibraryBackgroundSource(
    ushort ListPointer,
    ushort CommandPointer,
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort? WorkRamDestination,
    ushort? VramDestination,
    ushort? TransferByteCount,
    ushort? DoorPointer = null);
