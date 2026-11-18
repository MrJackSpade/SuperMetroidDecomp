using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Map;

/// <summary>Where Samus is and how the area map is revealed for one minimap update (#633).</summary>
public sealed record MinimapFrame
{
    /// <summary>The area the minimap shows.</summary>
    public required AreaId Area { get; init; }

    /// <summary>The area-map cell of the room's top-left screen.</summary>
    public required Position16<MapTiles> RoomMap { get; init; }

    /// <summary>The room's size, which bounds where the minimap places Samus.</summary>
    public required Extent16<RoomBlocks> RoomSize { get; init; }

    /// <summary>Samus's place in the room, which the minimap marks.</summary>
    public required Position16<RoomPixels> Samus { get; init; }

    /// <summary>The game's frame counter, whose eight-frame phase blinks the minimap's position marker.</summary>
    public required int FrameCounter { get; init; }

    /// <summary>How much of the area map the minimap reveals.</summary>
    public required MapRevealMode MapRevealMode { get; init; }

    /// <summary>The installed area map the minimap shows from.</summary>
    public required IAreaMapView PresentationMap { get; init; }
}
