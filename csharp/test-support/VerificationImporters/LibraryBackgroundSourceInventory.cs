using SuperMetroid.Core;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.AssetExtraction.Cartridge;

namespace SuperMetroid.AssetExtraction;

/// <summary>Enumerates visual ROM inputs named by every compiled room background command list.</summary>
[CpuBusBoundary("codec: ROM bank-$8F library background command lists → LibraryBackgroundSource inventory")]
internal static class LibraryBackgroundSourceInventory
{
    /// <summary>Traverses distinct compiled room background lists and records each source transfer operand.</summary>
    public static IReadOnlyList<LibraryBackgroundSource> Scan(ISnesAddressSpace bus)
    {
        Ensure.NotNull(bus);
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

    /// <summary>Reads a little-endian command word from bank $8F, wrapping the 16-bit list cursor.</summary>
    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
        return unchecked((ushort)(SnesLittleEndian.Word(bus.ReadCartridgeByte(address), bus.ReadCartridgeByte(RoomAssetRomData.LibraryBackground.CommandBank |
                unchecked((ushort)(pointer + 1))))));
    }

    /// <summary>Reads a 24-bit source address operand from the fixed library-background command bank.</summary>
    private static int ReadLong(ISnesAddressSpace bus, ushort pointer) =>
        (CartridgeImportSource.Require(bus).ReadCartridgeLongFixedBank(RoomAssetRomData.LibraryBackground.CommandBank | pointer));
}
