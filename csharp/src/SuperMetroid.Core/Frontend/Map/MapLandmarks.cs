using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Lists each area's map landmarks (<see cref="MapLandmarkDefinitions"/>): its boss markers, each naming the boss it shows,
/// its elevator labels with their destinations, and the sprite each destination's elevator shows.
/// </summary>
public static class MapLandmarks
{
    /// <summary>The boss markers of each area.</summary>
    private static IdentityTable<AreaId, ImmutableArray<MapBossMarker>> BossMarkersByArea => field ??= new(new Dictionary<AreaId, ImmutableArray<MapBossMarker>>
    {
        [AreaId.Crateria] = [],
        [AreaId.Tourian] = [],
        [AreaId.Brinstar] = MapLandmarkDefinitions.BrinstarBosses,
        [AreaId.Norfair] = MapLandmarkDefinitions.NorfairBosses,
        [AreaId.WreckedShip] = MapLandmarkDefinitions.WreckedShipBosses,
        [AreaId.Maridia] = MapLandmarkDefinitions.MaridiaBosses,
        [AreaId.Ceres] = MapLandmarkDefinitions.CeresBosses,
    });

    /// <summary>The boss markers of <paramref name="area"/>'s map; Crateria and Tourian show none.</summary>
    public static ImmutableArray<MapBossMarker> BossMarkers(AreaId area) => BossMarkersByArea[area];

    /// <summary>The elevator labels of one of the six Zebes areas' maps, in drawing order; Ceres has none.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="area"/> is Ceres: only the six Zebes areas have elevator map labels.</exception>
    public static ImmutableArray<MapElevatorLabel> Elevators(AreaId area) => Ensure.Listed(ElevatorsByArea, area);

    /// <summary>The elevator labels of each of the six Zebes areas' maps.</summary>
    private static FrozenDictionary<AreaId, ImmutableArray<MapElevatorLabel>> ElevatorsByArea => field ??= new Dictionary<AreaId, ImmutableArray<MapElevatorLabel>>
    {
        [AreaId.Crateria] = MapLandmarkDefinitions.CrateriaElevators,
        [AreaId.Brinstar] = MapLandmarkDefinitions.BrinstarElevators,
        [AreaId.Norfair] = MapLandmarkDefinitions.NorfairElevators,
        [AreaId.WreckedShip] = MapLandmarkDefinitions.WreckedShipElevators,
        [AreaId.Maridia] = MapLandmarkDefinitions.MaridiaElevators,
        [AreaId.Tourian] = MapLandmarkDefinitions.TourianElevators,
    }.ToFrozenDictionary();

    /// <summary>The map picture of the elevator label leading to <paramref name="destination"/>, one of the five for Crateria through Maridia.</summary>
    public static MapSpriteId ElevatorPicture(AreaId destination) => Ensure.Listed(ElevatorPictures, destination);

    /// <summary>The picture of each area an elevator label can lead to; no elevator leads to Tourian or Ceres.</summary>
    private static FrozenDictionary<AreaId, MapSpriteId> ElevatorPictures => field ??= new Dictionary<AreaId, MapSpriteId>
    {
        [AreaId.Crateria] = MapSpriteId.ElevatorCrateria,
        [AreaId.Brinstar] = MapSpriteId.ElevatorBrinstar,
        [AreaId.Norfair] = MapSpriteId.ElevatorNorfair,
        [AreaId.WreckedShip] = MapSpriteId.ElevatorWreckedShip,
        [AreaId.Maridia] = MapSpriteId.ElevatorMaridia,
    }.ToFrozenDictionary();
}
