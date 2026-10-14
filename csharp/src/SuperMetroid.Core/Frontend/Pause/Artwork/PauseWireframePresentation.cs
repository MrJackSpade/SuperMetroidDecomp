using System.Buffers.Binary;
using System.Text.Json;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Pause.Artwork;

/// <summary>Immutable wireframe artwork. It cannot change which equipment selects a visual.</summary>
public sealed class PauseWireframePresentation
{
    /// <summary>Four named equipment wireframe variants in compiled selection order.</summary>
    internal readonly Wireframe[] frames;
    /// <summary>Stores the validated variants without changing equipment selection rules.</summary>
    private PauseWireframePresentation(Wireframe[] frames) => this.frames = frames;

    /// <summary>Patches the selected eight-column, seventeen-row wireframe into the equipment page at tile (12, 7), matching native <c>EquipmentScreen_WriteSamusWireframeTilemap</c> at <c>$82:B20C</c>; leaves all other cells untouched and does not choose the inventory variant or queue VRAM.</summary>
    /// <param name="equipmentPage">Complete 2048-byte, 32-by-32 equipment tilemap in little-endian word order.</param>
    /// <param name="kind">One of the four compiled Power/Varia and regular/Hi-Jump visual variants.</param>
    public void ApplyTo(Span<byte> equipmentPage, PauseWireframeKind kind)
    {
        if ((uint)kind >= PauseWireframeDefinitions.Count) throw new ArgumentOutOfRangeException(nameof(kind));
        Ensure.LengthEqual(equipmentPage, PauseWireframeDefinitions.DestinationSize);
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
    internal sealed class Wireframe
    {
        /// <summary>Variant used to derive stock tilemap words when no override exists.</summary>
        private readonly PauseWireframeKind kind;
        /// <summary>Only cells whose authored words differ from the stock composition.</summary>
        internal readonly Dictionary<int, ushort> edits = [];

        /// <summary>Retains independent cell edits from a validated compiled wireframe.</summary>
        internal Wireframe(PauseWireframeKind kind, ReadOnlySpan<byte> words)
        {
            this.kind = kind;
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
            {
                ushort value = BinaryPrimitives.ReadUInt16LittleEndian(words[(cell * sizeof(ushort))..]);
                if (value != PauseWireframeDefinitions.StockWord(kind, cell)) edits.Add(cell, value);
            }
        }

        /// <summary>Resolves an edited cell or calculates the variant's stock word.</summary>
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
