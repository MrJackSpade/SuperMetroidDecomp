using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The colourings each map sprite is drawn in on the map screens (#1513), its standard colouring first: the scroll arrows
/// in the highlight cycle's, a boss's marker live and under a defeated boss's cross, the station icons, gunship, cross and
/// position indicator as markers, the elevator labels and the world map's title as labels, and each world-map area label
/// lit when selected and dim otherwise. The map sprite sheet pictures each sprite once, in its standard colouring, and a
/// draw names one of these.
/// </summary>
internal static class MapSpriteColourings
{
    /// <summary>Each map sprite's colourings, its standard one first.</summary>
    internal static readonly IReadOnlyDictionary<MapSpriteId, MapSpriteColouring[]> Drawn = new Dictionary<MapSpriteId, MapSpriteColouring[]>
    {
        [MapSpriteId.ArrowRight] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowLeft] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowUp] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.ArrowDown] = [MapSpriteColouring.Highlighted],
        [MapSpriteId.MarkerBoss] = [MapSpriteColouring.Markers, MapSpriteColouring.DefeatedBossMarker],
        [MapSpriteId.StationEnergy] = [MapSpriteColouring.Markers],
        [MapSpriteId.StationMissile] = [MapSpriteColouring.Markers],
        [MapSpriteId.StationMap] = [MapSpriteColouring.Markers],
        [MapSpriteId.MarkerDefeatedBoss] = [MapSpriteColouring.Markers],
        [MapSpriteId.MarkerGunship] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorBacking] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame0] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame1] = [MapSpriteColouring.Markers],
        [MapSpriteId.IndicatorFrame2] = [MapSpriteColouring.Markers],
        [MapSpriteId.ElevatorCrateria] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorBrinstar] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorNorfair] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorWreckedShip] = [MapSpriteColouring.Labels],
        [MapSpriteId.ElevatorMaridia] = [MapSpriteColouring.Labels],
        [MapSpriteId.WorldTitle] = [MapSpriteColouring.Labels],
        [MapSpriteId.WorldCrateria] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldBrinstar] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldNorfair] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldWreckedShip] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldMaridia] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
        [MapSpriteId.WorldTourian] = [MapSpriteColouring.Labels, MapSpriteColouring.DimLabels],
    };
}
