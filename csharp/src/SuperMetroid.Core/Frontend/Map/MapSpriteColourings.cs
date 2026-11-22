using SuperMetroid.Core.Frontend.Map.Artwork;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The colourings each map sprite is drawn in on the map screens (#1513), its standard colouring named: the scroll arrows
/// in the highlight cycle's, a boss's marker live and under a defeated boss's cross, the station icons, gunship, cross and
/// position indicator as markers, the elevator labels and the world map's title as labels, and each world-map area label
/// lit when selected and dim otherwise. The map sprite sheet pictures each sprite once, in its standard colouring, and a
/// draw names one of these.
/// </summary>
internal static partial class MapSpriteColourings
{
    /// <summary>Each map sprite's colourings, its standard one named.</summary>
    internal static readonly IReadOnlyDictionary<MapSpriteId, MapSpriteDrawnColourings> Drawn = new Dictionary<MapSpriteId, MapSpriteDrawnColourings>
    {
        [MapSpriteId.ArrowRight] = new(MapSpriteColouring.Highlighted),
        [MapSpriteId.ArrowLeft] = new(MapSpriteColouring.Highlighted),
        [MapSpriteId.ArrowUp] = new(MapSpriteColouring.Highlighted),
        [MapSpriteId.ArrowDown] = new(MapSpriteColouring.Highlighted),
        [MapSpriteId.MarkerBoss] = new(MapSpriteColouring.Markers, MapSpriteColouring.DefeatedBossMarker),
        [MapSpriteId.StationEnergy] = new(MapSpriteColouring.Markers),
        [MapSpriteId.StationMissile] = new(MapSpriteColouring.Markers),
        [MapSpriteId.StationMap] = new(MapSpriteColouring.Markers),
        [MapSpriteId.MarkerDefeatedBoss] = new(MapSpriteColouring.Markers),
        [MapSpriteId.MarkerGunship] = new(MapSpriteColouring.Markers),
        [MapSpriteId.IndicatorBacking] = new(MapSpriteColouring.Markers),
        [MapSpriteId.IndicatorSmall] = new(MapSpriteColouring.Markers),
        [MapSpriteId.IndicatorMiddle] = new(MapSpriteColouring.Markers),
        [MapSpriteId.IndicatorLarge] = new(MapSpriteColouring.Markers),
        [MapSpriteId.ElevatorCrateria] = new(MapSpriteColouring.Labels),
        [MapSpriteId.ElevatorBrinstar] = new(MapSpriteColouring.Labels),
        [MapSpriteId.ElevatorNorfair] = new(MapSpriteColouring.Labels),
        [MapSpriteId.ElevatorWreckedShip] = new(MapSpriteColouring.Labels),
        [MapSpriteId.ElevatorMaridia] = new(MapSpriteColouring.Labels),
        [MapSpriteId.WorldTitle] = new(MapSpriteColouring.Labels),
        [MapSpriteId.WorldCrateria] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
        [MapSpriteId.WorldBrinstar] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
        [MapSpriteId.WorldNorfair] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
        [MapSpriteId.WorldWreckedShip] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
        [MapSpriteId.WorldMaridia] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
        [MapSpriteId.WorldTourian] = new(MapSpriteColouring.Labels, MapSpriteColouring.DimLabels),
    };
}
