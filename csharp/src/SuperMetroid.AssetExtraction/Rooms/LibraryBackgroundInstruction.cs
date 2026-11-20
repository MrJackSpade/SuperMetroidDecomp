using SuperMetroid.Core.Rooms.Doors;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>
/// One library-background command (#1414, #2558): each kind carries exactly its own operands, named for the room's
/// background rather than the cartridge's operands, which extraction decodes once. A program holds no terminator; its end
/// is the end of its instructions.
/// </summary>
public abstract record LibraryBackgroundInstruction
{
    /// <summary>Limits the instructions to the kinds nested here.</summary>
    private LibraryBackgroundInstruction()
    {
    }

    /// <summary>A copy of <paramref name="ByteCount"/> bytes read from <paramref name="Source"/> to <paramref name="Destination"/>.</summary>
    /// <param name="Source">What the copy reads.</param>
    /// <param name="Destination">Where it puts it.</param>
    /// <param name="ByteCount">
    /// The payload length in bytes, required to be nonzero by the execution consumer. A count, not a native word: every
    /// producer supplies a sixteen-bit operand (at most 4096 in retail), nothing modifies it, and it is only compared and
    /// used as a span length.
    /// </param>
    public abstract record Copy(LibraryBackgroundSource Source, LibraryBackgroundDestination Destination, int ByteCount)
        : LibraryBackgroundInstruction;

    /// <summary>A copy every room loading the program makes.</summary>
    /// <inheritdoc cref="Copy" path="/param"/>
    public sealed record Transfer(LibraryBackgroundSource Source, LibraryBackgroundDestination Destination, int ByteCount)
        : Copy(Source, Destination, ByteCount);

    /// <summary>A copy of Kraid's list, the only one that may move the HUD's characters.</summary>
    /// <inheritdoc cref="Copy" path="/param"/>
    public sealed record TransferForKraid(LibraryBackgroundSource Source, LibraryBackgroundDestination Destination, int ByteCount)
        : Copy(Source, Destination, ByteCount);

    /// <summary>A copy made only by a room entered through <paramref name="Door"/>, which shows a picture of its own.</summary>
    /// <param name="Door">The door the copy belongs to.</param>
    /// <param name="Source">What the copy reads.</param>
    /// <param name="Destination">Where it puts it.</param>
    /// <param name="ByteCount">The payload length in bytes.</param>
    public sealed record TransferForDoor(DoorDefinition Door, LibraryBackgroundSource Source, LibraryBackgroundDestination Destination, int ByteCount)
        : Copy(Source, Destination, ByteCount);

    /// <summary>A decompression of <paramref name="Source"/> into the tilemap staging, which later transfers read.</summary>
    /// <param name="Source">The compressed tilemap.</param>
    public sealed record Decompress(LibraryBackgroundSource.CompressedTilemap Source) : LibraryBackgroundInstruction;

    /// <summary>The clear of the effects layer's tilemap, whose pages the command fixes.</summary>
    public sealed record ClearFxTilemap : LibraryBackgroundInstruction;

    /// <summary>The clear of the background layer's pages, which the command fixes.</summary>
    public sealed record ClearBg2 : LibraryBackgroundInstruction;

    /// <summary>The clear of the background layer's pages and Kraid's own page before them.</summary>
    public sealed record ClearBg2ForKraid : LibraryBackgroundInstruction;
}
