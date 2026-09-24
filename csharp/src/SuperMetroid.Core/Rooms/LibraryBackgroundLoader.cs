using SuperMetroid.Core.Assets;
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
    /// <summary>Runs one high-bank room-state list and returns its executed command count.</summary>
    public static LibraryBackgroundExecutionResult Execute(
        ISnesAddressSpace bus,
        SnesVram vram,
        ushort listPointer,
        ushort activeDoorPointer,
        RoomBackgroundTilemapCatalog? tilemapArt = null,
        RoomSkyTilemapCatalog? skyArt = null,
        HudTileAtlas? hudArt = null,
        RoomCharacterAtlasCatalog? characterArt = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        if (unchecked((short)listPointer) >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(listPointer),
                $"Library-background pointers must address the high half of bank $8F, not ${listPointer:X4}.");
        }

        if (LibraryBackgroundProgramDefinitions.TryGet(listPointer,
                out LibraryBackgroundProgram program))
            return ExecuteCompiled(bus, vram, program, activeDoorPointer,
                tilemapArt, skyArt, hudArt, characterArt);

        // All retail high-bank lists are compiled. Once the host has bound every
        // direct-transfer catalog, a missing list is a definition error rather
        // than permission to interpret unknown cartridge bytes at runtime.
        // Reference fixtures can still call ExecuteNativeForVerification explicitly.
        if (tilemapArt is not null && skyArt is not null && hudArt is not null &&
            characterArt is not null)
            throw new InvalidDataException(
                $"Installed library-background program $8F:{listPointer:X4} is not compiled.");

        return ExecuteNative(bus, vram, listPointer, activeDoorPointer,
            tilemapArt, skyArt, hudArt, characterArt);
    }

    private static LibraryBackgroundExecutionResult ExecuteCompiled(
        ISnesAddressSpace bus, SnesVram vram, LibraryBackgroundProgram program,
        ushort activeDoorPointer, RoomBackgroundTilemapCatalog? tilemapArt,
        RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt,
        RoomCharacterAtlasCatalog? characterArt)
    {
        ushort? bg3CharacterBaseWord = null;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferToVram:
                    TransferToVram(bus, vram, instruction.SourceAddress,
                        instruction.Destination, instruction.ByteCount,
                        skyArt, hudArt, characterArt);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    DecompressToWorkRam(bus, instruction.SourceAddress,
                        instruction.Destination, tilemapArt);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    FillAndTransfer(bus, vram, RoomAssetRomData.LibraryBackground.ClearFx);
                    break;
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    TransferToVram(bus, vram, instruction.SourceAddress,
                        instruction.Destination, instruction.ByteCount,
                        skyArt, hudArt, characterArt);
                    bg3CharacterBaseWord =
                        RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord;
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                    ClearBg2(bus, vram, includeKraidPage: false);
                    break;
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearBg2(bus, vram, includeKraidPage: true);
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                    if (instruction.DoorPointer == activeDoorPointer)
                        TransferToVram(bus, vram, instruction.SourceAddress,
                            instruction.Destination, instruction.ByteCount,
                            skyArt, hudArt, characterArt);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unknown compiled library-background command {instruction.Command} " +
                        $"in list $8F:{program.Pointer:X4}.");
            }
        }

        return new LibraryBackgroundExecutionResult(
            program.Instructions.Count + 1, bg3CharacterBaseWord);
    }

    /// <summary>Reference interpreter retained for pinned-ROM parity verification.</summary>
    internal static LibraryBackgroundExecutionResult ExecuteNativeForVerification(
        ISnesAddressSpace bus, SnesVram vram, ushort listPointer,
        ushort activeDoorPointer, RoomBackgroundTilemapCatalog? tilemapArt = null,
        RoomSkyTilemapCatalog? skyArt = null, HudTileAtlas? hudArt = null,
        RoomCharacterAtlasCatalog? characterArt = null) =>
        ExecuteNative(bus, vram, listPointer, activeDoorPointer,
            tilemapArt, skyArt, hudArt, characterArt);

    private static LibraryBackgroundExecutionResult ExecuteNative(
        ISnesAddressSpace bus, SnesVram vram, ushort listPointer,
        ushort activeDoorPointer, RoomBackgroundTilemapCatalog? tilemapArt,
        RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt,
        RoomCharacterAtlasCatalog? characterArt)
    {
        ushort cursor = listPointer;
        int executedCommands = 0;
        ushort? bg3CharacterBaseWord = null;
        for (int guard = 0; guard < RoomAssetRomData.LibraryBackground.MaximumCommandsPerList; guard++)
        {
            ushort commandAddress = cursor;
            ushort command = ReadWord(bus, cursor);
            cursor = unchecked((ushort)(cursor + 2));
            executedCommands++;

            switch ((LibraryBackgroundCommand)command)
            {
                case LibraryBackgroundCommand.End:
                    return new LibraryBackgroundExecutionResult(
                        executedCommands,
                        bg3CharacterBaseWord);

                case LibraryBackgroundCommand.TransferToVram:
                    cursor = TransferToVram(bus, vram, cursor, skyArt, hudArt, characterArt);
                    break;

                case LibraryBackgroundCommand.DecompressToWorkRam:
                    cursor = DecompressToWorkRam(bus, cursor, tilemapArt);
                    break;

                case LibraryBackgroundCommand.ClearFxTilemap:
                    // The retail command is unused, but its implementation is fully named:
                    // fill the shared $7E:4000 buffer and transfer $F00 bytes to VRAM $5880.
                    FillAndTransfer(bus, vram, RoomAssetRomData.LibraryBackground.ClearFx);
                    break;

                case LibraryBackgroundCommand.TransferToVramForKraid:
                    cursor = TransferToVram(bus, vram, cursor, skyArt, hudArt, characterArt);
                    // `$82:EA66` writes BG34NBA=$02 after the transfer. BG3 therefore
                    // consumes characters at word $2000 while Kraid's private BG2 map owns
                    // word $4000. Return the register-derived base to the room PPU owner.
                    bg3CharacterBaseWord =
                        RoomAssetRomData.LibraryBackground.KraidHudCharacterBaseWord;
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
                        cursor = TransferToVram(bus, vram, cursor, skyArt, hudArt, characterArt);
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

    private static ushort TransferToVram(ISnesAddressSpace bus, SnesVram vram, ushort cursor,
        RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt,
        RoomCharacterAtlasCatalog? characterArt)
    {
        int sourceAddress = ReadLong(bus, cursor);
        ushort destinationWord = ReadWord(bus, unchecked((ushort)(cursor + 3)));
        ushort byteCount = ReadWord(bus, unchecked((ushort)(cursor + 5)));
        TransferToVram(bus, vram, sourceAddress, destinationWord, byteCount,
            skyArt, hudArt, characterArt);
        return unchecked((ushort)(cursor + 7));
    }

    private static void TransferToVram(ISnesAddressSpace bus, SnesVram vram,
        int sourceAddress, ushort destinationWord, ushort byteCount,
        RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt,
        RoomCharacterAtlasCatalog? characterArt)
    {
        if (byteCount == 0)
        {
            throw new InvalidDataException(
                $"Library-background transfer from ${sourceAddress:X6} uses unsupported DMA size zero.");
        }

        // Command 2 configures the same $2118/$2119 consecutive-word DMA represented by
        // ExecuteQueuedWrite. Its destination is a VRAM word, not a host byte offset.
        if (characterArt is not null &&
            sourceAddress == RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress)
        {
            // A bound installation owns this visual source. An unexpected list shape
            // must fail instead of silently reverting to the ROM's stock pixels.
            if (byteCount != RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount)
                throw new InvalidDataException(
                    $"Tourian statue-ghost transfer requested {byteCount} bytes, " +
                    $"not {RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount}.");
            vram.ExecuteQueuedAssetWrite(characterArt.Get(sourceAddress).Transfer.Span,
                destinationWord);
        }
        else if (hudArt is not null && sourceAddress == HudTileAtlasFormat.SourceAddress &&
            byteCount == HudTileAtlasFormat.CharacterByteCount)
            vram.ExecuteQueuedAssetWrite(
                hudArt.Transfer.Span[..HudTileAtlasFormat.CharacterByteCount], destinationWord);
        else if (skyArt is not null && skyArt.TryResolve(sourceAddress, byteCount,
                out ReadOnlyMemory<byte> selected))
            vram.ExecuteQueuedAssetWrite(selected.Span, destinationWord);
        else if (sourceAddress >= RoomAssetRomData.LibraryBackground.RomSourceAddressFloor &&
            skyArt is not null && hudArt is not null && characterArt is not null)
        {
            // The installed host binds all three direct-transfer catalogs. A source
            // or byte count that none of them owns must not silently substitute the
            // cartridge's stock presentation data for a missing/invalid resource.
            throw new InvalidDataException(
                $"Installed library-background art does not own ROM transfer " +
                $"${sourceAddress:X6} ({byteCount} bytes).");
        }
        else
            vram.ExecuteQueuedWrite(bus, sourceAddress, byteCount, destinationWord);
    }

    private static ushort DecompressToWorkRam(ISnesAddressSpace bus, ushort cursor,
        RoomBackgroundTilemapCatalog? tilemapArt)
    {
        int sourceAddress = ReadLong(bus, cursor);
        ushort destination = ReadWord(bus, unchecked((ushort)(cursor + 3)));
        DecompressToWorkRam(bus, sourceAddress, destination, tilemapArt);
        return unchecked((ushort)(cursor + 5));
    }

    private static void DecompressToWorkRam(ISnesAddressSpace bus, int sourceAddress,
        ushort destination, RoomBackgroundTilemapCatalog? tilemapArt)
    {
        byte[] decompressed = tilemapArt?.Get(sourceAddress).Transfer.ToArray() ??
            RomDataReader.Decompress(bus, sourceAddress);
        if (destination + decompressed.Length > RoomAssetRomData.LibraryBackground.BankByteCount)
        {
            throw new InvalidDataException(
                $"Library-background decompression from ${sourceAddress:X6} crosses bank $7E.");
        }

        for (int index = 0; index < decompressed.Length; index++)
            bus.WriteByte(
                RoomAssetRomData.LibraryBackground.WorkRamBank | (destination + index),
                decompressed[index]);
    }

    private static void ClearBg2(ISnesAddressSpace bus, SnesVram vram, bool includeKraidPage)
    {
        // Clear_BG2_Tilemap fills both $800-byte WRAM pages with tile $0338, then performs
        // one $1000-byte DMA. Kraid repeats that same buffer at VRAM $4000 as well.
        RoomAssetRomData.TilemapTransfer transfer = RoomAssetRomData.LibraryBackground.ClearBg2;
        FillWords(bus, transfer.WorkRamDestination, transfer.ByteCount, transfer.FillValue);
        if (includeKraidPage)
        {
            vram.ExecuteQueuedWrite(
                bus,
                transfer.WorkRamSourceAddress,
                transfer.ByteCount,
                RoomAssetRomData.LibraryBackground.KraidBg2VramDestinationWord);
        }
        vram.ExecuteQueuedWrite(
            bus,
            transfer.WorkRamSourceAddress,
            transfer.ByteCount,
            transfer.VramDestinationWord);
    }

    /// <summary>Applies one named native fill-and-DMA definition without unpacked literals.</summary>
    private static void FillAndTransfer(
        ISnesAddressSpace bus,
        SnesVram vram,
        RoomAssetRomData.TilemapTransfer transfer)
    {
        FillWords(bus, transfer.WorkRamDestination, transfer.ByteCount, transfer.FillValue);
        vram.ExecuteQueuedWrite(
            bus,
            transfer.WorkRamSourceAddress,
            transfer.ByteCount,
            transfer.VramDestinationWord);
    }

    private static void FillWords(
        ISnesAddressSpace bus,
        ushort destination,
        int byteCount,
        ushort value)
    {
        if ((byteCount & 1) != 0 ||
            destination + byteCount > RoomAssetRomData.LibraryBackground.BankByteCount)
            throw new ArgumentOutOfRangeException(nameof(byteCount));

        for (int byteOffset = 0; byteOffset < byteCount; byteOffset += 2)
        {
            bus.WriteByte(
                RoomAssetRomData.LibraryBackground.WorkRamBank | (destination + byteOffset),
                (byte)value);
            bus.WriteByte(
                RoomAssetRomData.LibraryBackground.WorkRamBank | (destination + byteOffset + 1),
                (byte)(value >> 8));
        }
    }

    private static ushort ReadWord(ISnesAddressSpace bus, ushort pointer)
    {
        int address = RoomAssetRomData.LibraryBackground.CommandBank | pointer;
        return unchecked((ushort)(
            bus.ReadByte(address) |
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

/// <summary>
/// Observable output of one bank-$82 library-background list, including PPU register
/// side effects which cannot be represented by VRAM writes alone.
/// </summary>
public readonly record struct LibraryBackgroundExecutionResult(
    int ExecutedCommandCount,
    ushort? Bg3CharacterBaseWord);
