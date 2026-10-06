using System.Buffers.Binary;
using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable wireframe artwork. It cannot change which equipment selects a visual.</summary>
public sealed class PauseWireframePresentation
{
    private readonly Wireframe[] frames;
    private PauseWireframePresentation(Wireframe[] frames) => this.frames = frames;

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
        private readonly PauseWireframeKind kind;
        private readonly Dictionary<int, ushort> edits = [];

        internal Wireframe(PauseWireframeKind kind, ReadOnlySpan<byte> words)
        {
            this.kind = kind;
            for (int cell = 0; cell < PauseWireframeDefinitions.Cells; cell++)
            {
                ushort value = BinaryPrimitives.ReadUInt16LittleEndian(words[(cell * sizeof(ushort))..]);
                if (value != PauseWireframeDefinitions.StockWord(kind, cell)) edits.Add(cell, value);
            }
        }

        internal ushort Word(int cell) => edits.TryGetValue(cell, out ushort value)
            ? value : PauseWireframeDefinitions.StockWord(kind, cell);
    }
    public static void Write(Stream output, PauseWireframeDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, MapPresentationFormat.JsonOptions);
        _ = Load(new MemoryStream(bytes, writable: false));
        output.Write(bytes);
    }
}

public sealed record PauseWireframeDocument
{
    public required int Version { get; init; }
    public required Dictionary<string, PauseBackdropCell[]> Frames { get; init; }
}
