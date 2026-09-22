using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.AssetExtraction;

/// <summary>Enumerates visual ROM inputs named by every compiled room background command list.</summary>
public static class LibraryBackgroundSourceInventory
{
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

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
        return unchecked((ushort)(bus.ReadByte(address) |
            (bus.ReadByte(RoomAssetRomData.LibraryBackground.CommandBank |
                unchecked((ushort)(pointer + 1))) << 8)));
    }

    private static int ReadLong(ISnesAddressSpace bus, ushort pointer) =>
        bus.ReadByte(RoomAssetRomData.LibraryBackground.CommandBank | pointer) |
        (bus.ReadByte(RoomAssetRomData.LibraryBackground.CommandBank |
            unchecked((ushort)(pointer + 1))) << 8) |
        (bus.ReadByte(RoomAssetRomData.LibraryBackground.CommandBank |
            unchecked((ushort)(pointer + 2))) << 16);
}

/// <summary>One source operand in a native library-background list, not a synthesized image.</summary>
public sealed record LibraryBackgroundSource(
    ushort ListPointer,
    ushort CommandPointer,
    LibraryBackgroundCommand Command,
    int SourceAddress,
    ushort? WorkRamDestination,
    ushort? VramDestination,
    ushort? TransferByteCount,
    ushort? DoorPointer = null);
