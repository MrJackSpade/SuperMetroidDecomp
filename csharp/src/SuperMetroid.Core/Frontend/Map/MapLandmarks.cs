using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Lists each area's map landmarks (<see cref="MapLandmarkDefinitions"/>): its boss markers, each naming the boss it shows,
/// its elevator labels with their destinations, and the sprite each destination's elevator shows.
/// </summary>
public static partial class MapLandmarks
{
    /// <summary>Whether <paramref name="area"/>'s map shows the gunship, where it landed.</summary>
    public static bool ShowsGunship(AreaId area) => MapLandmarkDefinitions.GunshipAreas.Contains(area);

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
    public static ImmutableArray<MapElevatorLabel> Elevators(ZebesArea area) => ElevatorsByArea[area];

    /// <summary>The elevator labels of each of the six Zebes areas' maps.</summary>
    private static IdentityTable<ZebesArea, ImmutableArray<MapElevatorLabel>> ElevatorsByArea => field ??= new(new Dictionary<ZebesArea, ImmutableArray<MapElevatorLabel>>
    {
        [ZebesArea.Crateria] = MapLandmarkDefinitions.CrateriaElevators,
        [ZebesArea.Brinstar] = MapLandmarkDefinitions.BrinstarElevators,
        [ZebesArea.Norfair] = MapLandmarkDefinitions.NorfairElevators,
        [ZebesArea.WreckedShip] = MapLandmarkDefinitions.WreckedShipElevators,
        [ZebesArea.Maridia] = MapLandmarkDefinitions.MaridiaElevators,
        [ZebesArea.Tourian] = MapLandmarkDefinitions.TourianElevators,
    });

    /// <summary>The map picture of the elevator label leading to <paramref name="destination"/>.</summary>
    public static MapSpriteId ElevatorPicture(MapElevatorDestination destination) => ElevatorPictures[destination];

    /// <summary>The picture of each area an elevator label can lead to.</summary>
    private static IdentityTable<MapElevatorDestination, MapSpriteId> ElevatorPictures => field ??= new(new Dictionary<MapElevatorDestination, MapSpriteId>
    {
        [MapElevatorDestination.Crateria] = MapSpriteId.ElevatorCrateria,
        [MapElevatorDestination.Brinstar] = MapSpriteId.ElevatorBrinstar,
        [MapElevatorDestination.Norfair] = MapSpriteId.ElevatorNorfair,
        [MapElevatorDestination.WreckedShip] = MapSpriteId.ElevatorWreckedShip,
        [MapElevatorDestination.Maridia] = MapSpriteId.ElevatorMaridia,
    });
}
