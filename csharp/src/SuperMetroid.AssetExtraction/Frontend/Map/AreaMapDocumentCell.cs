using System.Text.Json.Serialization;
using SuperMetroid.Core.Map;

namespace SuperMetroid.AssetExtraction.Frontend.Map;

/// <summary>
/// One editable cell of an area's map document (#1513): the kind of place it shows, its sloped-hallway piece and what
/// bounds each edge, by name. These fields say how the cell looks; whether it can be explored and whether exploring it
/// reveals the cell above follow from them, and map-station reveals come from the station mask, never from the document.
/// </summary>
public sealed record AreaMapDocumentCell
{
    /// <summary>The kind of place the cell shows.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellKind>))]
    public required MapCellKind Kind { get; init; }

    /// <summary>The piece of a sloped hallway the cell shows, or <see cref="MapCellSlope.None"/>.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellSlope>))]
    public required MapCellSlope Slope { get; init; }

    /// <summary>What bounds the cell's top edge.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellEdge>))]
    public required MapCellEdge Top { get; init; }

    /// <summary>What bounds the cell's bottom edge.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellEdge>))]
    public required MapCellEdge Bottom { get; init; }

    /// <summary>What bounds the cell's left edge.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellEdge>))]
    public required MapCellEdge Left { get; init; }

    /// <summary>What bounds the cell's right edge.</summary>
    [JsonConverter(typeof(JsonStringEnumConverter<MapCellEdge>))]
    public required MapCellEdge Right { get; init; }
}
