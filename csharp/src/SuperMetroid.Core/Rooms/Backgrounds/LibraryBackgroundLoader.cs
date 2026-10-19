using SuperMetroid.Core.Hud.Artwork;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// Executes bank-$82's room-state library-background command list into the tilemap staging and VRAM.
/// </summary>
/// <remarks>
/// A room with an odd/fixed layer-2 scroll mode does not stream BG2 from level data. Its
/// state header instead points at one of these bank-$8F lists, which can decompress a static
/// tilemap into WRAM and DMA it to the two BG2 screen pages. Ignoring the list leaves the
/// previous room's tilemap resident—a particularly visible failure at Ceres room $DF8D.
/// </remarks>
public static class LibraryBackgroundLoader
{
    /// <summary>Executes an already-defined program; constructed fixtures never require a runtime byte decoder.</summary>
    public static LibraryBackgroundExecutionResult ExecuteProgram(LibraryBackgroundStaging staging, SnesVram vram, LibraryBackgroundProgram program, CartridgeDoorHeader activeDoor, LibraryBackgroundArtwork artwork)
    {
        Ensure.NotNull(staging);
        Ensure.NotNull(vram);
        Ensure.NotNull(program);
        bool selectsKraidHudCharacters = false;
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.TransferToVram:
                    TransferToVram(staging, vram, instruction, artwork);
                    break;
                case LibraryBackgroundCommand.DecompressToWorkRam:
                    DecompressToWorkRam(staging, instruction.SourceAddress,
                        instruction.Destination, artwork.TilemapArt);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    FillAndTransfer(staging, vram, RoomAssetRomData.LibraryBackground.ClearFx);
                    break;
                case LibraryBackgroundCommand.TransferToVramForKraid:
                    TransferToVram(staging, vram, instruction, artwork);
                    selectsKraidHudCharacters = true;
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                    ClearBg2(staging, vram, includeKraidPage: false);
                    break;
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    ClearBg2(staging, vram, includeKraidPage: true);
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                    if (instruction.Door == activeDoor)
                        TransferToVram(staging, vram, instruction, artwork);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unknown compiled library-background command {instruction.Command} " +
                        $"in list {program.Name}.");
            }
        }

        return new LibraryBackgroundExecutionResult(selectsKraidHudCharacters);
    }

    /// <summary>Binds a library background's character transfer at its VRAM word (#1350).</summary>
    [PpuLayoutBoundary("codec: library-background character transfers' native VRAM words")]
    private static void BindCharacters(LibraryBackgroundArtwork artwork, LibraryBackgroundInstruction transfer, ReadOnlySpan<byte> characters, ushort destination) =>
        (artwork.Backgrounds ?? throw new InvalidOperationException("A library background's character transfer requires background bindings."))
            .Bind(new BgArtworkIdentity($"library characters ${transfer.SourceAddress:X6}"), destination, characters);

    /// <summary>Routes a transfer command (plain, Kraid or door-selected) to live memory or its installed art catalog, preserving its VRAM word destination.</summary>
    [PpuLayoutBoundary("codec: library-background command 2 VRAM DMA (native source and VMADD)")]
    private static void TransferToVram(LibraryBackgroundStaging staging, SnesVram vram, LibraryBackgroundInstruction transfer, LibraryBackgroundArtwork artwork)
    {
        (RoomSkyTilemapCatalog? skyArt, HudTileAtlas? hudArt, RoomCharacterAtlasCatalog? characterArt) =
            (artwork.SkyArt, artwork.HudArt, artwork.CharacterArt);
        if (transfer.ByteCount == 0)
        {
            throw new InvalidDataException(
                $"Library-background transfer from ${transfer.SourceAddress:X6} uses unsupported DMA size zero.");
        }

        // Command 2 configures the same $2118/$2119 consecutive-word DMA represented by
        // memory or installed-art transfers. Its destination is a VRAM word, not a byte offset.
        SnesDmaSourceKind sourceKind = SnesDmaSourceMap.Classify(SnesAddress.FromBusAddress(transfer.SourceAddress));
        ushort destination = checked((ushort)transfer.Destination);
        if (sourceKind is SnesDmaSourceKind.WorkRam)
            staging.LoadNativeTransfer(vram, transfer.SourceAddress, transfer.ByteCount, destination);
        else if (characterArt is not null &&
            transfer.SourceAddress == RoomAssetRomData.LibraryBackground.TourianStatueGhost.SourceAddress)
        {
            // A bound installation owns this visual source. An unexpected list shape
            // must fail instead of silently reverting to the ROM's stock pixels.
            if (transfer.ByteCount != RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount)
                throw new InvalidDataException(
                    $"Tourian statue-ghost transfer requested {transfer.ByteCount} bytes, " +
                    $"not {RoomAssetRomData.LibraryBackground.TourianStatueGhost.TransferByteCount}.");
            BindCharacters(artwork, transfer, characterArt.Get(transfer.SourceAddress).Transfer.Span, destination);
        }
        else if (hudArt is not null && transfer.SourceAddress == HudTileAtlasFormat.SourceAddress &&
            transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount)
            BindCharacters(artwork, transfer, hudArt.Transfer.Span[..HudTileAtlasFormat.CharacterByteCount], destination);
        else if (skyArt is not null && skyArt.TryResolve(transfer.SourceAddress, transfer.ByteCount,
                out ReadOnlyMemory<byte> selected))
            vram.ExecuteQueuedAssetWrite(selected.Span, destination);
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
    private static void DecompressToWorkRam(LibraryBackgroundStaging staging, int sourceAddress,
        int destination, RoomBackgroundTilemapCatalog? tilemapArt)
    {
        byte[] decompressed = (tilemapArt ?? throw new InvalidOperationException(
            "Library-background decompression requires installed tilemaps."))
            .Get(sourceAddress).Transfer.ToArray();
        staging.Tilemap.WriteDecompressed(destination, decompressed, sourceAddress);
    }

    /// <summary>Fills both BG2 staging pages and queues the native DMA, including Kraid's extra page when requested.</summary>
    [PpuLayoutBoundary("codec: Clear_BG2_Tilemap native fill-and-DMA")]
    private static void ClearBg2(LibraryBackgroundStaging staging, SnesVram vram, bool includeKraidPage)
    {
        // Clear_BG2_Tilemap fills both $800-byte WRAM pages with tile $0338, then performs
        // one $1000-byte DMA. Kraid repeats that same buffer at VRAM $4000 as well.
        RoomAssetRomData.TilemapTransfer transfer = RoomAssetRomData.LibraryBackground.ClearBg2;
        FillWords(staging, transfer.WorkRamDestination, transfer.ByteCount, transfer.FillValue);
        if (includeKraidPage)
        {
            staging.LoadNativeTransfer(vram, transfer.WorkRamSourceAddress, transfer.ByteCount,
                RoomAssetRomData.LibraryBackground.KraidBg2VramDestinationWord);
        }
        staging.LoadNativeTransfer(vram, transfer.WorkRamSourceAddress, transfer.ByteCount, transfer.VramDestinationWord);
    }

    /// <summary>Applies one named native fill-and-DMA definition without unpacked literals.</summary>
    [PpuLayoutBoundary("codec: native library-background fill-and-DMA definition")]
    private static void FillAndTransfer(
        LibraryBackgroundStaging staging,
        SnesVram vram,
        RoomAssetRomData.TilemapTransfer transfer)
    {
        FillWords(staging, transfer.WorkRamDestination, transfer.ByteCount, transfer.FillValue);
        staging.LoadNativeTransfer(vram, transfer.WorkRamSourceAddress, transfer.ByteCount, transfer.VramDestinationWord);
    }

    /// <summary>Fills the selected WRAM staging range with repeated native tilemap words.</summary>
    private static void FillWords(
        LibraryBackgroundStaging staging,
        ushort destination,
        int byteCount,
        ushort value) =>
        staging.Tilemap.FillWords(destination, byteCount, value);

}
