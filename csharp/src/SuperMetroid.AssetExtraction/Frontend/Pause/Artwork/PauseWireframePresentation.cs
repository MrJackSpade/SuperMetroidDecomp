using System.Text.Json;
using SuperMetroid.Core;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Pause.Artwork;

/// <summary>
/// Immutable wireframe artwork, kept as installed (#1615): every variant's 136 cells. It cannot change which equipment
/// selects a visual.
/// </summary>
public sealed class PauseWireframePresentation
{
    /// <summary>Four named equipment wireframe variants' installed cells, in eight-column row order.</summary>
    private readonly Dictionary<PauseWireframeKind, BackgroundTile[]> frames;
    /// <summary>Stores the validated variants without changing equipment selection rules.</summary>
    private PauseWireframePresentation(Dictionary<PauseWireframeKind, BackgroundTile[]> frames) => this.frames = frames;

    /// <summary>Patches the selected eight-column, seventeen-row wireframe into the equipment page at tile (12, 7), matching native <c>EquipmentScreen_WriteSamusWireframeTilemap</c> at <c>$82:B20C</c>; leaves all other cells untouched and does not choose the inventory variant or queue VRAM.</summary>
    /// <param name="equipmentPage">Complete 32-by-32 equipment page.</param>
    /// <param name="kind">One of the four compiled Power/Varia and regular/Hi-Jump visual variants.</param>
    public void ApplyTo(Span<BackgroundTile> equipmentPage, PauseWireframeKind kind)
    {
        if (!frames.TryGetValue(kind, out BackgroundTile[]? source)) throw new ArgumentOutOfRangeException(nameof(kind));
        Ensure.LengthEqual(equipmentPage, PauseWireframeDefinitions.PageTiles);
        for (int row = 0; row < PauseWireframeDefinitions.Rows; row++)
            source.AsSpan(row * PauseWireframeDefinitions.Columns, PauseWireframeDefinitions.Columns)
                .CopyTo(equipmentPage[(PauseWireframeDefinitions.FirstPageTile + row * PauseWireframeDefinitions.PageColumns)..]);
    }

    /// <summary>Loads all four named wireframe variants at the supported version, requiring 136 row-major cells per variant and validating each cell's atlas, tile coordinates, and palette before compiling native tilemap words.</summary>
    /// <param name="json">Caller-owned UTF-8 JSON stream containing the complete wireframe document.</param>
    /// <returns>The immutable wireframe presentation holding every installed cell.</returns>
    public static PauseWireframePresentation Load(Stream json)
    {
        PauseWireframeDocument document;
        try { document = JsonAssetDocument.Read<PauseWireframeDocument>(json, ArtworkJson.Options)
            ?? throw new InvalidDataException("Pause wireframe document is null."); }
        catch (JsonException error) { throw new InvalidDataException("Invalid pause wireframe JSON.", error); }
        if (document.Version != PauseWireframeDefinitions.Version || document.Frames is null || document.Frames.Count != PauseWireframeDefinitions.Count)
            throw new InvalidDataException("Pause wireframes require version 1 and all four named frames.");
        var frames = new Dictionary<PauseWireframeKind, BackgroundTile[]>();
        foreach (PauseWireframeKind kind in Enum.GetValues<PauseWireframeKind>())
        {
            if (!document.Frames.TryGetValue(kind.ToString(), out var cells) || cells is null || cells.Length != PauseWireframeDefinitions.Cells)
                throw new InvalidDataException($"Pause wireframe {kind} requires 136 cells in eight-column row order.");
            frames.Add(kind, PauseTileGrid.Compile(cells, kind.ToString()));
        }
        return new(frames);
    }

    /// <summary>Serializes a wireframe document using the map-presentation JSON options and writes it to the destination.</summary>
    /// <param name="output">Caller-owned destination stream for the UTF-8 JSON.</param>
    /// <param name="document">Document containing all four complete wireframe variants.</param>
    public static void Write(Stream output, PauseWireframeDocument document)
    {
        byte[] bytes = JsonSerializer.SerializeToUtf8Bytes(document, ArtworkJson.Options);
        output.Write(bytes);
    }
}
