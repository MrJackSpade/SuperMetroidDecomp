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
    public static LibraryBackgroundExecutionResult Execute(ISnesAddressSpace bus, SnesVram vram, ushort listPointer, ushort activeDoorPointer, LibraryBackgroundArtwork artwork)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        if (unchecked((short)listPointer) >= 0)
        {
            throw new ArgumentOutOfRangeException(
                nameof(listPointer),
                $"Library-background pointers must address the high half of bank $8F, not ${listPointer:X4}.");
        }

        if (LibraryBackgroundProgramDefinitions.TryGet(listPointer,
                out LibraryBackgroundProgram program))
            return ExecuteProgram(bus, vram, program, activeDoorPointer, artwork);

        // All retail high-bank lists are compiled. Once the host has bound every
        // direct-transfer catalog, a missing list is a definition error rather
        // than permission to interpret unknown cartridge bytes at runtime.
        throw new InvalidDataException(
            $"Library-background program $8F:{listPointer:X4} is not compiled.");
    }

    /// <summary>Executes an already-defined program; constructed fixtures never require a runtime byte decoder.</summary>
    public static LibraryBackgroundExecutionResult ExecuteProgram(ISnesAddressSpace bus, SnesVram vram, LibraryBackgroundProgram program, ushort activeDoorPointer, LibraryBackgroundArtwork artwork)
    {
        Ensure.NotNull(bus);
        Ensure.NotNull(vram);
        Ensure.NotNull(program);
        ushort? bg3CharacterBaseWord = null;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferToVram:
                    TransferToVram(bus, vram, new LibraryBackgroundTransfer { SourceAddress = instruction.SourceAddress, DestinationWord = instruction.Destination, ByteCount = instruction.ByteCount }, artwork.SkyArt, artwork.HudArt, artwork.CharacterArt);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    DecompressToWorkRam(bus, instruction.SourceAddress,
                        instruction.Destination, artwork.TilemapArt);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    FillAndTransfer(bus, vram, RoomAssetRomData.LibraryBackground.ClearFx);
                    break;
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    TransferToVram(bus, vram, new LibraryBackgroundTransfer { SourceAddress = instruction.SourceAddress, DestinationWord = instruction.Destination, ByteCount = instruction.ByteCount }, artwork.SkyArt, artwork.HudArt, artwork.CharacterArt);
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
                        TransferToVram(bus, vram, new LibraryBackgroundTransfer { SourceAddress = instruction.SourceAddress, DestinationWord = instruction.Destination, ByteCount = instruction.ByteCount }, artwork.SkyArt, artwork.HudArt, artwork.CharacterArt);
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

    /// <summary>Routes a native DMA command to live memory or its installed art catalog, preserving its VRAM word destination.</summary>
    [PpuLayoutBoundary("codec: library-background command 2 VRAM DMA (native source and VMADD)")]
    private static void TransferToVram(ISnesAddressSpace bus, SnesVram vram, LibraryBackgroundTransfer transfer, RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt, RoomCharacterAtlasCatalog? characterArt)
    {
        if (transfer.ByteCount == 0)
        {
            throw new InvalidDataException(
                $"Library-background transfer from ${transfer.SourceAddress:X6} uses unsupported DMA size zero.");
        }

        // Command 2 configures the same $2118/$2119 consecutive-word DMA represented by
        // memory or installed-art transfers. Its destination is a VRAM word, not a byte offset.
        SnesDmaSourceKind sourceKind = SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(transfer.SourceAddress));
        if (sourceKind is SnesDmaSourceKind.WorkRam or SnesDmaSourceKind.SaveRam)
            vram.ExecuteQueuedMemoryWrite(RequireWorkMemory(bus), transfer.SourceAddress, transfer.ByteCount, transfer.DestinationWord);
        else if (characterArt is not null &&
            transfer.SourceAddress == RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress)
        {
            // A bound installation owns this visual source. An unexpected list shape
            // must fail instead of silently reverting to the ROM's stock pixels.
            if (transfer.ByteCount != RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount)
                throw new InvalidDataException(
                    $"Tourian statue-ghost transfer requested {transfer.ByteCount} bytes, " +
                    $"not {RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount}.");
            vram.ExecuteQueuedAssetWrite(characterArt.Get(transfer.SourceAddress).Transfer.Span,
                transfer.DestinationWord);
        }
        else if (hudArt is not null && transfer.SourceAddress == HudTileAtlasFormat.SourceAddress &&
            transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount)
            vram.ExecuteQueuedAssetWrite(
                hudArt.Transfer.Span[..HudTileAtlasFormat.CharacterByteCount], transfer.DestinationWord);
        else if (skyArt is not null && skyArt.TryResolve(transfer.SourceAddress, transfer.ByteCount,
                out ReadOnlyMemory<byte> selected))
            vram.ExecuteQueuedAssetWrite(selected.Span, transfer.DestinationWord);
        else if (transfer.SourceAddress >= RoomAssetRomData.LibraryBackground.RomSourceAddressFloor &&
            skyArt is not null && hudArt is not null && characterArt is not null)
        {
            // The installed host binds all three direct-transfer catalogs. A source
            // or byte count that none of them owns must not silently substitute the
            // cartridge's stock presentation data for a missing/invalid resource.
            throw new InvalidDataException(
                $"Installed library-background art does not own ROM transfer " +
                $"${transfer.SourceAddress:X6} ({transfer.ByteCount} bytes).");
        }
        else
            throw new InvalidDataException(
                $"Library-background transfer ${transfer.SourceAddress:X6} has no installed artwork.");
    }

    /// <summary>Copies an installed decompressed tilemap into the command's WRAM staging destination.</summary>
    private static void DecompressToWorkRam(ISnesAddressSpace bus, int sourceAddress,
        ushort destination, RoomBackgroundTilemapCatalog? tilemapArt)
    {
        byte[] decompressed = (tilemapArt ?? throw new InvalidOperationException(
            "Library-background decompression requires installed tilemaps."))
            .Get(sourceAddress).Transfer.ToArray();
        new LibraryBackgroundMemory(RequireWorkMemory(bus)).WriteDecompressed(destination, decompressed, sourceAddress);
    }

    /// <summary>Fills both BG2 staging pages and queues the native DMA, including Kraid's extra page when requested.</summary>
    [PpuLayoutBoundary("codec: Clear_BG2_Tilemap native fill-and-DMA")]
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
    [PpuLayoutBoundary("codec: native library-background fill-and-DMA definition")]
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

    /// <summary>Requires a mutable memory bus for background staging reads and writes.</summary>
    private static ISnesMemoryBus RequireWorkMemory(ISnesAddressSpace bus) =>
        SnesMemoryBusRequirement.Require(bus, "Library-background staging transfers require live WRAM.");

    /// <summary>Fills the selected WRAM staging range with repeated native tilemap words.</summary>
    private static void FillWords(
        ISnesAddressSpace bus,
        ushort destination,
        int byteCount,
        ushort value) =>
        new LibraryBackgroundMemory(RequireWorkMemory(bus)).FillWords(destination, byteCount, value);


}

/// <summary>
/// Observable output of one bank-$82 library-background list, including PPU register
/// side effects which cannot be represented by VRAM writes alone.
/// </summary>
public readonly record struct LibraryBackgroundExecutionResult(
    ushort? Bg3CharacterBaseWord);
