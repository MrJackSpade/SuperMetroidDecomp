using System.Buffers.Binary;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable wireframe artwork. It cannot change which equipment selects a visual.</summary>
public sealed class PauseWireframePresentation
{
    /// <summary>Compiled visual variants indexed by <see cref="PauseWireframeKind"/>.</summary>
    private readonly Wireframe[] frames;

    /// <summary>Creates a presentation from its complete set of compiled equipment wireframes.</summary>
    /// <param name="frames">One compiled wireframe for each supported equipment appearance.</param>
    private PauseWireframePresentation(Wireframe[] frames) => this.frames = frames;

    /// <summary>Patches the selected eight-column, seventeen-row wireframe into the equipment page at tile (12, 7), matching native <c>EquipmentScreen_WriteSamusWireframeTilemap</c> at <c>$82:B20C</c>; leaves all other cells untouched and does not choose the inventory variant or queue VRAM.</summary>
    /// <param name="equipmentPage">Complete 2048-byte, 32-by-32 equipment tilemap in little-endian word order.</param>
    /// <param name="kind">One of the four compiled Power/Varia and regular/Hi-Jump visual variants.</param>
    public void ApplyTo(Span<byte> equipmentPage, PauseWireframeKind kind)
    {
        if ((uint)kind >= PauseWireframeDefinitions.Count) throw new ArgumentOutOfRangeException(nameof(kind));
        if (equipmentPage.Length != PauseWireframeDefinitions.DestinationSize)
            throw new ArgumentException("Wireframe requires the complete equipment tilemap.", nameof(equipmentPage));
        Wireframe source = frames[(int)kind];
        for (int row = 0; row < PauseWireframeDefinitions.Rows; row++)
        for (int column = 0; column < PauseWireframeDefinitions.Columns; column++)
            BinaryPrimitives.WriteUInt16LittleEndian(equipmentPage.Slice(
                PauseWireframeDefinitions.DestinationByte + row * PauseWireframeDefinitions.DestinationStride + column * sizeof(ushort)),
                source.Word(row * PauseWireframeDefinitions.Columns + column));
    }

    /// <summary>Loads all four named wireframe variants at the supported version, requiring 136 row-major cells per variant and validating each cell's atlas, tile coordinates, and palette before compiling native tilemap words.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the complete wireframe document.</param>
    /// <returns>The immutable wireframe presentation preserving independently supplied cell edits.</returns>
    public static PauseWireframePresentation Load(Stream json)
    {
        PauseWireframeDocument document;
        try { document = JsonAssetDocument.Read<PauseWireframeDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause wireframe document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause wireframe JSON.", error); }
        if (document.Version != PauseWireframeDefinitions.Version || document.Frames is null || document.Frames.Count != PauseWireframeDefinitions.Count)
            throw new InvalidDataException("Pause wireframes require version 1 and all four named frames.");
        var frames = new Wireframe[PauseWireframeDefinitions.Count];
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            if (!document.Frames.TryGetValue(kind.ToString(), out var cells) || cells is null || cells.Length != PauseWireframeDefinitions.Cells)
                throw new InvalidDataException($"Pause wireframe {kind} requires 136 cells in eight-column row order.");
            frames[(int)kind] = new Wireframe(kind, PauseTileGrid.Compile(cells, kind.ToString()));
        }
        return new(frames);
    }

    /// <summary>Calculated authored composition with independent supplied word overrides.</summary>
    private sealed class Wireframe
    {
        /// <summary>Variant whose compiled stock words provide fallback tilemap cells.</summary>
        private readonly PauseWireframeKind kind;

        /// <summary>Authored tilemap words that differ from this variant's calculated stock composition.</summary>
        private readonly Dictionary<int, ushort> edits = [];

        /// <summary>Builds sparse authored overrides by comparing each compiled cell with its stock word.</summary>
        /// <param name="kind">Equipment wireframe variant used to calculate unchanged cells.</param>
        /// <param name="words">Little-endian compiled tilemap words in row-major cell order.</param>
        internal Wireframe(PauseWireframeKind kind, ReadOnlySpan<byte> words)
        {
            this.kind = kind;
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
            {
                ushort value = BinaryPrimitives.ReadUInt16LittleEndian(words[(cell * sizeof(ushort))..]);
                if (value != PauseWireframeDefinitions.StockWord(kind, cell)) edits.Add(cell, value);
            }
        }

        /// <summary>Resolves a cell from its authored override or the calculated stock wireframe.</summary>
        /// <param name="cell">Zero-based row-major cell index.</param>
        /// <returns>The supplied tilemap word when edited, otherwise the stock word for this variant.</returns>
        internal ushort Word(int cell) => edits.TryGetValue(cell, out ushort value)
            ? value : PauseWireframeDefinitions.StockWord(kind, cell);
    }
    /// <summary>Serializes a wireframe document using the map-presentation JSON options and validates it through <see cref="Load"/> before writing any bytes to the destination.</summary>
    /// <param name="output">Caller-owned destination stream for the validated UTF-8 JSON.</param>
    /// <param name="document">Document containing all four complete wireframe variants.</param>
    public static void Write(Stream output, PauseWireframeDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

/// <summary>Editable JSON schema for pause-menu wireframe artwork; equipped-item rules still select which variant is shown, independently of authored cell content.</summary>
public sealed record PauseWireframeDocument
{
    /// <summary>Schema revision, which must equal <see cref="PauseWireframeDefinitions.Version"/>.</summary>
    public required int Version { get; init; }
    /// <summary>All four <see cref="PauseWireframeKind"/> names, each mapping to 136 cells in eight-column, seventeen-row order with separate atlas, palette, priority, and flip selections.</summary>
    public required Dictionary<string, PauseBackdropCell[]> Frames { get; init; }
}
