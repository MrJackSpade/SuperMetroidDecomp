using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Map;

/// <summary>Where Samus is and how the area map is revealed for one minimap update (#633).</summary>
public sealed record MinimapFrame
{
    /// <summary>Area index.</summary>
    public required AreaId AreaIndex { get; init; }

    /// <summary>Room map.</summary>
    public required Position16<MapTiles> RoomMap { get; init; }

    /// <summary>Room size.</summary>
    public required Extent16<RoomBlocks> RoomSize { get; init; }

    /// <summary>Samus.</summary>
    public required Position16<RoomPixels> Samus { get; init; }

    /// <summary>Nmi frame counter.</summary>
    public required int NmiFrameCounter { get; init; }

    /// <summary>Map reveal mode.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>Presentation map.</summary>
    public IAreaMapView? PresentationMap { get; init; } = null;
}
