using System.Text.Json;

namespace SuperMetroid.Core.Assets;

/// <summary>Immutable wireframe artwork. It cannot change which equipment selects a visual.</summary>
public sealed class PauseWireframePresentation
{
    private readonly byte[][] frames;
    private PauseWireframePresentation(byte[][] frames) => this.frames = frames;

    public void ApplyTo(Span<byte> equipmentPage, PauseWireframeKind kind)
    {
        if ((uint)kind >= PauseWireframeDefinitions.Count) throw new ArgumentOutOfRangeException(nameof(kind));
        if (equipmentPage.Length != PauseWireframeDefinitions.DestinationSize)
            throw new ArgumentException("Wireframe requires the complete equipment tilemap.", nameof(equipmentPage));
        byte[] source = frames[(int)kind];
        for (int row = 0; row < PauseWireframeDefinitions.Rows; row++)
            source.AsSpan(row * PauseWireframeDefinitions.Columns * 2, PauseWireframeDefinitions.Columns * 2)
                .CopyTo(equipmentPage.Slice(PauseWireframeDefinitions.DestinationByte + row * PauseWireframeDefinitions.DestinationStride));
    }

    public static PauseWireframePresentation Load(Stream json)
    {
        PauseWireframeDocument document;
        try { document = JsonSerializer.Deserialize<PauseWireframeDocument>(json, MapPresentationFormat.JsonOptions)
            ?? throw new InvalidDataException("Pause wireframe document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause wireframe JSON.", error); }
        if (document.Version != PauseWireframeDefinitions.Version || document.Frames is null || document.Frames.Count != PauseWireframeDefinitions.Count)
            throw new InvalidDataException("Pause wireframes require version 1 and all four named frames.");
        var frames = new byte[PauseWireframeDefinitions.Count][];
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            if (!document.Frames.TryGetValue(kind.ToString(), out var cells) || cells is null || cells.Length != PauseWireframeDefinitions.Cells)
                throw new InvalidDataException($"Pause wireframe {kind} requires 136 cells in eight-column row order.");
            frames[(int)kind] = PauseTileGrid.Compile(cells, kind.ToString());
        }
        return new(frames);
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
