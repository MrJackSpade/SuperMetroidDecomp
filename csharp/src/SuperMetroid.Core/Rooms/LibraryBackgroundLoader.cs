using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;

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
            return ExecuteProgram(bus, vram, program, activeDoorPointer,
                tilemapArt, skyArt, hudArt, characterArt);

        // All retail high-bank lists are compiled. Once the host has bound every
        // direct-transfer catalog, a missing list is a definition error rather
        // than permission to interpret unknown cartridge bytes at runtime.
        throw new InvalidDataException(
            $"Library-background program $8F:{listPointer:X4} is not compiled.");
    }

    /// <summary>Executes an already-defined program; constructed fixtures never require a runtime byte decoder.</summary>
    public static LibraryBackgroundExecutionResult ExecuteProgram(
        ISnesAddressSpace bus, SnesVram vram, LibraryBackgroundProgram program,
        ushort activeDoorPointer, RoomBackgroundTilemapCatalog? tilemapArt = null,
        RoomSkyTilemapCatalog? skyArt = null, HudTileAtlas? hudArt = null,
        RoomCharacterAtlasCatalog? characterArt = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(vram);
        ArgumentNullException.ThrowIfNull(program);
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
bg3CharacterBaseWord);
    }

    /// <summary>Copies a compiled list transfer into VRAM from live memory or the matching installed artwork.</summary>
    /// <param name="bus">Address space used to access cartridge-visible memory.</param>
    /// <param name="vram">VRAM queue receiving the transfer at a word-addressed destination.</param>
    /// <param name="sourceAddress">Source bus address encoded by the library-background command.</param>
    /// <param name="destinationWord">Destination VRAM word address from the command.</param>
    /// <param name="byteCount">Number of source bytes to transfer.</param>
    /// <param name="skyArt">Installed room-sky transfers that may own the source.</param>
    /// <param name="hudArt">Installed HUD character data that may own the source.</param>
    /// <param name="characterArt">Installed character data, including the statue-ghost transfer.</param>
    /// <exception cref="InvalidDataException">The transfer size is zero or installed resources do not own the source.</exception>
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
        // memory or installed-art transfers. Its destination is a VRAM word, not a byte offset.
        SnesDmaSourceKind sourceKind = SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(sourceAddress));
        if (sourceKind is SnesDmaSourceKind.WorkRam or SnesDmaSourceKind.SaveRam)
            vram.ExecuteQueuedMemoryWrite(RequireWorkMemory(bus), sourceAddress, byteCount, destinationWord);
        else if (characterArt is not null &&
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
            throw new InvalidDataException(
                $"Library-background transfer ${sourceAddress:X6} has no installed artwork.");
    }

    /// <summary>Copies an installed, decompressed room tilemap into its bank-$7E staging destination.</summary>
    /// <param name="bus">Address space receiving the decompressed bytes.</param>
    /// <param name="sourceAddress">ROM source address used to select the installed tilemap.</param>
    /// <param name="destination">Bank-relative WRAM byte offset at which the tilemap is staged.</param>
    /// <param name="tilemapArt">Catalog supplying the decompressed tilemap data.</param>
    /// <exception cref="InvalidOperationException">No tilemap catalog is installed.</exception>
    /// <exception cref="InvalidDataException">The tilemap would extend beyond the WRAM bank.</exception>
    private static void DecompressToWorkRam(ISnesAddressSpace bus, int sourceAddress,
        ushort destination, RoomBackgroundTilemapCatalog? tilemapArt)
    {
        byte[] decompressed = (tilemapArt ?? throw new InvalidOperationException(
            "Library-background decompression requires installed tilemaps."))
            .Get(sourceAddress).Transfer.ToArray();
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

    /// <summary>Fills the BG2 staging pages with the cleared tile and queues their VRAM transfer.</summary>
    /// <param name="bus">Address space containing the WRAM staging pages.</param>
    /// <param name="vram">VRAM queue receiving the BG2 pages.</param>
    /// <param name="includeKraidPage">Whether to also copy the page used by Kraid's background layout.</param>
    private static void ClearBg2(ISnesAddressSpace bus, SnesVram vram, bool includeKraidPage)
    {
        // Clear_BG2_Tilemap fills both $800-byte WRAM pages with tile $0338, then performs
        // one $1000-byte DMA. Kraid repeats that same buffer at VRAM $4000 as well.
        RoomAssetRomData.TilemapTransfer transfer = RoomAssetRomData.LibraryBackground.ClearBg2;
        FillWords(bus, transfer.WorkRamDestination, transfer.ByteCount, transfer.FillValue);
        if (includeKraidPage)
        {
            vram.ExecuteQueuedMemoryWrite(
                RequireWorkMemory(bus),
                transfer.WorkRamSourceAddress,
                transfer.ByteCount,
                RoomAssetRomData.LibraryBackground.KraidBg2VramDestinationWord);
        }
        vram.ExecuteQueuedMemoryWrite(
            RequireWorkMemory(bus),
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
        vram.ExecuteQueuedMemoryWrite(
            RequireWorkMemory(bus),
            transfer.WorkRamSourceAddress,
            transfer.ByteCount,
            transfer.VramDestinationWord);
    }

    /// <summary>Requires the address space to expose mutable WRAM for queued memory-to-VRAM transfers.</summary>
    /// <param name="bus">Address space that must provide live writable memory.</param>
    /// <returns>The mutable memory view used as the queued transfer source.</returns>
    /// <exception cref="InvalidOperationException">The address space does not expose mutable memory.</exception>
    private static ISnesMutableMemory RequireWorkMemory(ISnesAddressSpace bus) =>
        bus as ISnesMutableMemory ?? throw new InvalidOperationException(
            "Library-background staging transfers require live WRAM.");

    /// <summary>Fills an even-sized bank-relative WRAM range with repeated little-endian words.</summary>
    /// <param name="bus">Address space receiving the fill writes.</param>
    /// <param name="destination">Bank-relative byte offset of the first word.</param>
    /// <param name="byteCount">Even number of bytes to fill within the bank.</param>
    /// <param name="value">Word value written at each successive two-byte position.</param>
    /// <exception cref="ArgumentOutOfRangeException">The byte count is odd or the range exceeds the bank.</exception>
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


}

/// <summary>
/// Observable output of one bank-$82 library-background list, including PPU register
/// side effects which cannot be represented by VRAM writes alone.
/// </summary>
/// <param name="Bg3CharacterBaseWord">BG3 character-base register value requested by a list, or null when unchanged.</param>
public readonly record struct LibraryBackgroundExecutionResult(
    ushort? Bg3CharacterBaseWord);
