using SuperMetroid.Core.Map;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map.Artwork;

/// <summary>
/// The installed area maps and the content the pause and file-select map screens draw them with (#2203): each area's map
/// with its exploration rules, the highlight cycle, the world-map labels, the station, landmark and save-marker positions
/// and the scroll arrows. Edits in place never alter exploration rules.
/// </summary>
public sealed partial class AreaMapPresentation : IAreaMapSource
{
    /// <summary>One validated map view per area.</summary>
    private readonly IReadOnlyDictionary<AreaId, IAreaMapView> areas;

    /// <summary>Takes each area's installed map and the map screens' installed content.</summary>
    internal AreaMapPresentation(IReadOnlyDictionary<AreaId, IAreaMapView> areas, MapHighlightCycle highlightCycle,
        WorldMapLabelPositions labels, MapStationPositions stations, MapLandmarkPositions landmarks,
        MapSaveMarkerPositions saveMarkers, MapArrowPresentation arrows)
    {
        this.areas = Ensure.NotNull(areas);
        HighlightCycle = Ensure.NotNull(highlightCycle);
        Labels = Ensure.NotNull(labels);
        Stations = Ensure.NotNull(stations);
        Landmarks = Ensure.NotNull(landmarks);
        SaveMarkers = Ensure.NotNull(saveMarkers);
        Arrows = Ensure.NotNull(arrows);
    }

    /// <summary>Selected highlight color frames used by map presentation; how the highlight steps is the game's, not the artwork's.</summary>
    public MapHighlightCycle HighlightCycle { get; }
    /// <summary>World-map area-name drawing anchors, independent of area selection and map exploration.</summary>
    public WorldMapLabelPositions Labels { get; }
    /// <summary>Map-station label drawing positions; moving these does not change discovery cells or station reveal masks.</summary>
    public MapStationPositions Stations { get; }
    /// <summary>Boss, elevator, and gunship drawing anchors, independent of progression and destination rules.</summary>
    public MapLandmarkPositions Landmarks { get; }
    /// <summary>The installed file-select save-marker drawing anchors, independent of load targets and scrolling.</summary>
    public MapSaveMarkerPositions SaveMarkers { get; }
    /// <summary>Map-arrow artwork and presentation frames for scrolling indicators.</summary>
    public MapArrowPresentation Arrows { get; }

    /// <summary>Returns an area's selected map presentation with its stock exploration and station-reveal rules.</summary>
    /// <param name="area">One of the seven areas, including Ceres.</param>
    /// <returns>The immutable view built when this catalog was loaded.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The value is not an area.</exception>
    public IAreaMapView Get(AreaId area) => areas[AreaIds.Require(area)];
}
