using SuperMetroid.Core.Progression;

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

    /// <summary>The game's frame counter, whose eight-frame phase blinks the minimap's position marker.</summary>
    public required int FrameCounter { get; init; }

    /// <summary>Map reveal mode.</summary>
    public MapRevealMode MapRevealMode { get; init; } = MapRevealMode.None;

    /// <summary>Presentation map.</summary>
    public IAreaMapView? PresentationMap { get; init; } = null;
}
