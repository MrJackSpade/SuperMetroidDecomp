using System.Text.Json.Nodes;
using SuperMetroid.AssetExtraction;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rom;

/// <summary>The Kraid and Maridia elevatube visual catalogs a Kraid draw check installs (#633).</summary>
internal readonly record struct KraidRoomVisualCatalogs
{
    /// <summary>Kraid.</summary>
    public required RoomPlmKraidVisualCatalog? Kraid { get; init; }

    /// <summary>Tube.</summary>
    public required RoomPlmMaridiaElevatubeVisualCatalog Tube { get; init; }
}
