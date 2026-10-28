using System.Collections.Frozen;
using SuperMetroid.AssetExtraction.CartridgeTables;
using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.TestSupport.VerificationImporters;

/// <summary>
/// The verification oracle's decoding of the cartridge's library-background operands into the named sources,
/// destinations and commands the compiled programs hold (#1414), and back to the cartridge's addresses.
/// </summary>
internal static class LibraryBackgroundOperandDecoding
{
    /// <summary>Bank $8F, which holds every room state's library-background command list.</summary>
    internal const int CommandListBank = 0x8f0000;

    /// <summary>The command word that ends a list.</summary>
    internal const ushort EndCommandWord = 0x0000;

    /// <summary><c>$7E:4000</c>: the tilemap staging decompressions fill and transfers read.</summary>
    internal const int TilemapStaging = 0x7e4000;

    /// <summary><c>$7E:2000</c>: the enemy tilemap staging boss-room lists copy to the background.</summary>
    internal const int EnemyTilemapStaging = 0x7e2000;

    /// <summary>The tilemap staging's offset within bank $7E, which a decompression's operand names.</summary>
    internal const ushort TilemapStagingOffset = 0x4000;

    /// <summary>The bank-$82 interpreter's word for each command.</summary>
    internal static readonly FrozenDictionary<LibraryBackgroundCommand, ushort> CommandWords =
        new Dictionary<LibraryBackgroundCommand, ushort>
        {
            [LibraryBackgroundCommand.Transfer] = 0x0002,
            [LibraryBackgroundCommand.Decompress] = 0x0004,
            [LibraryBackgroundCommand.ClearFxTilemap] = 0x0006,
            [LibraryBackgroundCommand.TransferForKraid] = 0x0008,
            [LibraryBackgroundCommand.ClearBg2] = 0x000a,
            [LibraryBackgroundCommand.ClearBg2ForKraid] = 0x000c,
            [LibraryBackgroundCommand.TransferForDoor] = 0x000e,
        }.ToFrozenDictionary();

    /// <summary>The cartridge's word for <paramref name="command"/>.</summary>
    internal static ushort Word(LibraryBackgroundCommand command) => CommandWords[command];

    /// <summary>The command the cartridge encodes as <paramref name="word"/>, or null for the end of the list.</summary>
    /// <exception cref="InvalidDataException">No command has that word.</exception>
    internal static LibraryBackgroundCommand? Command(ushort word)
    {
        if (word == EndCommandWord)
            return null;
        foreach ((LibraryBackgroundCommand command, ushort encoded) in CommandWords)
            if (encoded == word)
                return command;
        throw new InvalidDataException($"Unknown library-background command word ${word:X4}.");
    }

    /// <summary>Where the cartridge or work RAM holds <paramref name="source"/>.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="source"/> reads nothing.</exception>
    internal static int Address(LibraryBackgroundSource source) => source switch
    {
        LibraryBackgroundSource.CompressedTilemap compressed => RoomBackgroundSourceDefinitions.Tilemaps[compressed.Tilemap],
        LibraryBackgroundSource.StagedTilemap => TilemapStaging,
        LibraryBackgroundSource.StagedEnemyTilemap => EnemyTilemapStaging,
        LibraryBackgroundSource.SkyPage sky => LibraryBackgroundOperands.SkyPageAddress(sky.Page),
        LibraryBackgroundSource.StatueGhostCharacters => RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters,
        LibraryBackgroundSource.HudCharacterSet => MapSourceDefinitions.Artwork.HudCharacters,
        LibraryBackgroundSource.NothingRead => throw new ArgumentOutOfRangeException(nameof(source), source, "A clear command reads nothing."),
        _ => throw new ArgumentOutOfRangeException(nameof(source), source, "Unknown library-background source."),
    };

    /// <summary>The tilemap a decompression from <paramref name="address"/> stages.</summary>
    /// <exception cref="InvalidDataException">No background tilemap is stored there.</exception>
    internal static LibraryBackgroundSource.CompressedTilemap DecompressedSource(int address)
    {
        foreach ((RoomBackgroundTilemap tilemap, int stored) in RoomBackgroundSourceDefinitions.Tilemaps)
            if (stored == address)
                return new LibraryBackgroundSource.CompressedTilemap(tilemap);
        throw new InvalidDataException($"No background tilemap is stored at ${address:X6}.");
    }

    /// <summary>What a transfer from <paramref name="address"/> reads.</summary>
    /// <exception cref="InvalidDataException">No library-background source is stored there.</exception>
    internal static LibraryBackgroundSource TransferredSource(int address)
    {
        if (address == TilemapStaging)
            return LibraryBackgroundSource.TilemapStaging;
        if (address == EnemyTilemapStaging)
            return LibraryBackgroundSource.EnemyTilemapStaging;
        if (address == RoomBackgroundSourceDefinitions.TourianStatueGhostCharacters)
            return LibraryBackgroundSource.TourianStatueGhostCharacters;
        if (address == MapSourceDefinitions.Artwork.HudCharacters)
            return LibraryBackgroundSource.HudCharacters;
        for (int page = 0; page < RoomSkyTilemapFormat.PageCount; page++)
            if (address == LibraryBackgroundOperands.SkyPageAddress(page))
                return LibraryBackgroundSource.Sky(page);
        throw new InvalidDataException($"No library-background transfer source is stored at ${address:X6}.");
    }

    /// <summary>The destination a transfer to VRAM word <paramref name="word"/> fills.</summary>
    /// <exception cref="InvalidDataException">No library-background destination starts there.</exception>
    internal static LibraryBackgroundDestination TransferDestination(ushort word)
    {
        foreach ((LibraryBackgroundDestination destination, ushort encoded) in RoomBackgroundSourceDefinitions.DestinationWords)
            if (encoded == word)
                return destination;
        throw new InvalidDataException($"No library-background destination starts at VRAM word ${word:X4}.");
    }

    /// <summary>The destination a decompression to bank-$7E offset <paramref name="offset"/> fills: always the tilemap staging.</summary>
    /// <exception cref="InvalidDataException">The decompression stages elsewhere.</exception>
    internal static LibraryBackgroundDestination DecompressionDestination(ushort offset) =>
        offset == TilemapStagingOffset ? LibraryBackgroundDestination.TilemapStaging
            : throw new InvalidDataException($"A library-background decompression stages at $7E:{offset:X4}, not the tilemap staging.");
}
