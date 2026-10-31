using SuperMetroid.Core.Map.Artwork;

namespace SuperMetroid.Core.Frontend.FileSelect.Artwork;

/// <summary>One authored glyph or artwork cell in a sparse file-select tilemap patch.</summary>
[SpatialComponents("External format: file-select.json stores each patch cell offset as flat x/y fields.")]
public sealed record FileSelectPatchCellDocument
{
    /// <summary>The authored tile offset from the patch's placement anchor.</summary>
    [System.Text.Json.Serialization.JsonIgnore]
    public Position16<TilemapCells> Offset => new(X, Y);
    /// <summary>Nonnegative horizontal tile offset from the patch's placement anchor.</summary>
    public required int X { get; init; }
    /// <summary>Nonnegative vertical tile offset from the patch's placement anchor.</summary>
    public required int Y { get; init; }
    /// <summary>Character-sheet coordinates and palette, priority, and flip attributes compiled into the destination tilemap word.</summary>
    public required MapPresentationCell Cell { get; init; }
}
