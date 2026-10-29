using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Doors;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.AssetExtraction.Hud;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// Replays a room state's library-background command list as bank $82's interpreter runs it (#1406, #1466). Only extraction
/// runs the list: it decompresses tilemaps into the cartridge's tilemap staging, copies staged and sky tilemaps to BG2's
/// pages and the HUD's characters where Kraid's list copies them, and fills pages with the blank tile. The BG2 pages the
/// replay leaves are pictured as the background's image, which the runtime selects by name instead.
/// </summary>
internal static class LibraryBackgroundReplay
{
    /// <summary>
    /// Replays <paramref name="program"/> as a room entered through <paramref name="activeDoor"/> runs it, into
    /// <paramref name="vram"/>. The tilemap staging starts empty and the enemy BG2 staging holds
    /// <paramref name="enemyBg2Staging"/> (empty for none, as a room load leaves it).
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The list holds a command the interpreter does not accept, a zero-length transfer, or a transfer from ROM that no
    /// installed artwork owns.
    /// </exception>
    internal static void Execute(SnesVram vram, LibraryBackgroundProgram program,
        DoorDefinition activeDoor, LibraryBackgroundArtwork artwork, ReadOnlyMemory<byte> enemyBg2Staging = default)
    {
        Ensure.NotNull(vram);
        Ensure.NotNull(program);
        Ensure.NotNull(artwork);
        if (enemyBg2Staging.Length > LibraryBackgroundStagingDefinitions.EnemyBg2StagingByteCount)
            throw new ArgumentOutOfRangeException(nameof(enemyBg2Staging), enemyBg2Staging.Length, "The enemy BG2 staging is 4 KiB.");
        var staging = new Staging(enemyBg2Staging);
        foreach (LibraryBackgroundInstruction instruction in program.Instructions)
        {
            switch (instruction.Command)
            {
                case LibraryBackgroundCommand.Transfer:
                case LibraryBackgroundCommand.TransferForKraid:
                    Transfer(staging, vram, instruction, artwork);
                    break;
                case LibraryBackgroundCommand.Decompress:
                    if (instruction.Destination != LibraryBackgroundDestination.TilemapStaging || instruction.Source is not LibraryBackgroundSource.CompressedTilemap compressed)
                        throw new InvalidDataException($"A decompression in list {program.Name} stages {instruction.Source} at {instruction.Destination}.");
                    staging.WriteDecompressed(LibraryBackgroundStagingDefinitions.TilemapStagingBankOffset, BackgroundTileWords.ToBytes((artwork.TilemapArt ?? throw new InvalidOperationException(
                        "Library-background decompression requires installed tilemaps.")).Get(compressed.Tilemap).Tiles.Span),
                        instruction.Source);
                    break;
                case LibraryBackgroundCommand.ClearFxTilemap:
                    FillAndTransfer(staging, vram, LibraryBackgroundStagingDefinitions.ClearFx);
                    break;
                case LibraryBackgroundCommand.ClearBg2:
                    FillAndTransfer(staging, vram, LibraryBackgroundStagingDefinitions.ClearBg2);
                    break;
                case LibraryBackgroundCommand.ClearBg2ForKraid:
                    // Kraid's list repeats the filled staging at his own page before BG2's.
                    LibraryBackgroundClear clear = LibraryBackgroundStagingDefinitions.ClearBg2;
                    staging.FillWords(clear.WorkRamDestination, clear.ByteCount, clear.FillValue.Raw);
                    vram.ExecuteQueuedAssetWrite(staging.Read(clear.WorkRamSourceAddress, clear.ByteCount), LibraryBackgroundStagingDefinitions.KraidBg2VramDestinationWord);
                    vram.ExecuteQueuedAssetWrite(staging.Read(clear.WorkRamSourceAddress, clear.ByteCount), clear.VramDestination);
                    break;
                case LibraryBackgroundCommand.TransferForDoor:
                    if (instruction.Door == activeDoor)
                        Transfer(staging, vram, instruction, artwork);
                    break;
                default:
                    throw new InvalidDataException(
                        $"Unknown compiled library-background command {instruction.Command} in list {program.Name}.");
            }
        }
    }

    /// <summary>
    /// One transfer command: a staged source copies the staging; the HUD's characters, which only Kraid's list copies,
    /// copy their installed artwork; the Tourian statue ghost's characters reach the sprite characters, which only the
    /// statue ghost would draw, and it draws nothing, so they are written nowhere; a sky page copies its installed rows.
    /// </summary>
    /// <exception cref="InvalidDataException">
    /// The transfer is empty, reads a source no installed artwork owns, moves only part of the statue ghost's characters,
    /// or moves the HUD's characters from a list other than Kraid's.
    /// </exception>
    /// <exception cref="InvalidOperationException">The transfer brings characters whose installed artwork is not supplied.</exception>
    private static void Transfer(Staging staging, SnesVram vram, LibraryBackgroundInstruction transfer, LibraryBackgroundArtwork artwork)
    {
        if (transfer.ByteCount == 0)
            throw new InvalidDataException($"Library-background transfer from {transfer.Source} uses unsupported DMA size zero.");
        // The destination is an encoded VRAM word with the increment in its top bit, as the DMA's VMADD/VMAIN take it.
        int destination = LibraryBackgroundOperands.DestinationWord(transfer.Destination);
        if (staging.TryRead(transfer.Source, transfer.ByteCount, out ReadOnlySpan<byte> staged))
            vram.ExecuteQueuedAssetWrite(staged, destination);
        else if (transfer.Source == LibraryBackgroundSource.TourianStatueGhostCharacters)
            RequireWholeStatueGhost(transfer, artwork);
        else if (transfer.Source == LibraryBackgroundSource.HudCharacters && transfer.ByteCount == HudTileAtlasFormat.CharacterByteCount)
            vram.ExecuteQueuedAssetWrite(KraidHudCharacters(transfer, artwork).Span, destination);
        else if (transfer.Source is LibraryBackgroundSource.SkyPage sky && artwork.SkyArt is { } skies)
            vram.ExecuteQueuedAssetWrite(BackgroundTileWords.ToBytes(skies.Page(sky.Page, transfer.ByteCount / sizeof(ushort)).Span), destination);
        else
            // A source no bound catalog owns must not fall back to the cartridge's bytes.
            throw new InvalidDataException(
                $"Library-background transfer from {transfer.Source} ({transfer.ByteCount} bytes) has no installed artwork.");
    }

    /// <summary>Checks the statue ghost's transfer brings all of its installed characters.</summary>
    /// <exception cref="InvalidOperationException">No room characters are installed.</exception>
    /// <exception cref="InvalidDataException">The transfer brings only some of them.</exception>
    private static void RequireWholeStatueGhost(LibraryBackgroundInstruction transfer, LibraryBackgroundArtwork artwork)
    {
        RoomCharacterAtlases characters = artwork.CharacterArt
            ?? throw new InvalidOperationException("The Tourian statue ghost's library characters have no installed character artwork.");
        ReadOnlyMemory<byte> ghost = characters.Get(RoomCharacterSet.TourianStatueGhost).Characters;
        if (transfer.ByteCount != ghost.Length)
            throw new InvalidDataException($"Tourian statue-ghost transfer requested {transfer.ByteCount} bytes, not its {ghost.Length}.");
    }

    /// <summary>The installed HUD characters Kraid's list copies.</summary>
    /// <exception cref="InvalidDataException">A list other than Kraid's copies them.</exception>
    /// <exception cref="InvalidOperationException">No HUD characters are installed.</exception>
    private static ReadOnlyMemory<byte> KraidHudCharacters(LibraryBackgroundInstruction transfer, LibraryBackgroundArtwork artwork)
    {
        if (transfer.Command != LibraryBackgroundCommand.TransferForKraid)
            throw new InvalidDataException($"A library background's {transfer.Command} moves the HUD's characters, which only Kraid's list copies.");
        HudTileAtlas hud = artwork.HudArt
            ?? throw new InvalidOperationException("Kraid's library background moves the HUD's characters, which have no installed artwork.");
        return hud.Bytes[..transfer.ByteCount];
    }

    /// <summary>Fills the staging as <paramref name="clear"/> names, then transfers it to its VRAM destination.</summary>
    private static void FillAndTransfer(Staging staging, SnesVram vram, LibraryBackgroundClear clear)
    {
        staging.FillWords(clear.WorkRamDestination, clear.ByteCount, clear.FillValue.Raw);
        vram.ExecuteQueuedAssetWrite(staging.Read(clear.WorkRamSourceAddress, clear.ByteCount), clear.VramDestination);
    }

    /// <summary>The work RAM a replay reads and writes: the tilemap staging, and the enemy BG2 staging it only reads.</summary>
    /// <param name="enemyBg2">The enemy BG2 staging's leading bytes; the rest are zero.</param>
    private sealed class Staging(ReadOnlyMemory<byte> enemyBg2)
    {
        /// <summary>The tilemap staging's bytes in address order.</summary>
        private readonly byte[] tilemap = new byte[LibraryBackgroundStagingDefinitions.TilemapStagingByteCount];

        /// <summary>The enemy BG2 staging's bytes in address order.</summary>
        private readonly byte[] enemy = Copied(enemyBg2);

        /// <summary>The staged bytes at <paramref name="address"/>, which must lie in one staging.</summary>
        /// <exception cref="InvalidDataException">The range lies in neither staging.</exception>
        public ReadOnlySpan<byte> Read(int address, int byteCount) =>
            TryRead(address, byteCount, out ReadOnlySpan<byte> bytes) ? bytes
                : throw new InvalidDataException($"Library-background work-RAM transfer ${address:X6}+{byteCount} has no staging.");

        /// <summary>Whether <paramref name="source"/> is a staging holding <paramref name="byteCount"/> bytes, and those bytes.</summary>
        /// <exception cref="InvalidDataException">The range leaves the staging.</exception>
        public bool TryRead(LibraryBackgroundSource source, int byteCount, out ReadOnlySpan<byte> bytes)
        {
            int address = source switch
            {
                LibraryBackgroundSource.StagedTilemap => (int)LibraryBackgroundStagingDefinitions.TilemapStagingAddress,
                LibraryBackgroundSource.StagedEnemyTilemap => (int)LibraryBackgroundStagingDefinitions.EnemyBg2StagingAddress,
                _ => -1,
            };
            if (address < 0)
            {
                bytes = default;
                return false;
            }
            bytes = Read(address, byteCount);
            return true;
        }

        /// <summary>Whether <paramref name="address"/> and its <paramref name="byteCount"/> bytes lie in one staging, and those bytes.</summary>
        private bool TryRead(int address, int byteCount, out ReadOnlySpan<byte> bytes)
        {
            if (Within(address, byteCount, (int)LibraryBackgroundStagingDefinitions.TilemapStagingAddress, LibraryBackgroundStagingDefinitions.TilemapStagingByteCount))
            {
                bytes = tilemap.AsSpan(address - (int)LibraryBackgroundStagingDefinitions.TilemapStagingAddress, byteCount);
                return true;
            }
            if (Within(address, byteCount, (int)LibraryBackgroundStagingDefinitions.EnemyBg2StagingAddress, LibraryBackgroundStagingDefinitions.EnemyBg2StagingByteCount))
            {
                bytes = enemy.AsSpan(address - (int)LibraryBackgroundStagingDefinitions.EnemyBg2StagingAddress, byteCount);
                return true;
            }
            bytes = default;
            return false;
        }

        /// <summary>Writes a decompressed tilemap at bank-$7E <paramref name="destination"/>.</summary>
        /// <exception cref="InvalidDataException">The tilemap leaves the staging.</exception>
        public void WriteDecompressed(int destination, ReadOnlySpan<byte> bytes, LibraryBackgroundSource source) =>
            bytes.CopyTo(tilemap.AsSpan(Offset(destination, bytes.Length,
                $"Library-background decompression of {source} to $7E:{destination:X4} leaves the tilemap staging buffer.")));

        /// <summary>Fills <paramref name="byteCount"/> bytes at bank-$7E <paramref name="destination"/> with <paramref name="value"/> words.</summary>
        /// <exception cref="InvalidDataException">The fill leaves the staging.</exception>
        public void FillWords(int destination, int byteCount, int value)
        {
            if ((byteCount & 1) != 0)
                throw new ArgumentOutOfRangeException(nameof(byteCount));
            int first = Offset(destination, byteCount,
                $"Library-background fill of {byteCount} bytes at $7E:{destination:X4} leaves the tilemap staging buffer.");
            for (int offset = 0; offset < byteCount; offset += sizeof(ushort))
            {
                tilemap[first + offset] = LittleEndianBytes.LowByte(value);
                tilemap[first + offset + 1] = LittleEndianBytes.HighByte(value);
            }
        }

        /// <summary>The staging offset of bank-$7E <paramref name="destination"/>, whose range must lie in the tilemap staging.</summary>
        private static int Offset(int destination, int byteCount, string failure)
        {
            int first = destination - LibraryBackgroundStagingDefinitions.TilemapStagingBankOffset;
            return first >= 0 && first + byteCount <= LibraryBackgroundStagingDefinitions.TilemapStagingByteCount ? first : throw new InvalidDataException(failure);
        }

        /// <summary>Whether <paramref name="byteCount"/> bytes from <paramref name="address"/> lie in the staging at <paramref name="first"/>.</summary>
        private static bool Within(int address, int byteCount, int first, int stagingBytes) =>
            address >= first && address + byteCount <= first + stagingBytes;

        /// <summary>A 4 KiB staging holding <paramref name="leading"/> first.</summary>
        private static byte[] Copied(ReadOnlyMemory<byte> leading)
        {
            var bytes = new byte[LibraryBackgroundStagingDefinitions.EnemyBg2StagingByteCount];
            leading.Span.CopyTo(bytes);
            return bytes;
        }
    }
}
