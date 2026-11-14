using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Lists each area's map landmarks (<see cref="MapLandmarkDefinitions"/>): its boss markers, each naming the boss it shows,
/// its elevator labels with their destinations, and the sprite each destination's elevator shows.
/// </summary>
public static class MapLandmarks
{
    /// <summary>The boss markers of <paramref name="area"/>'s map; Crateria and Tourian show none.</summary>
    public static ImmutableArray<MapBossMarker> BossMarkers(AreaId area) => area switch
    {
        AreaId.Crateria or AreaId.Tourian => [],
        AreaId.Brinstar => MapLandmarkDefinitions.BrinstarBosses,
        AreaId.Norfair => MapLandmarkDefinitions.NorfairBosses,
        AreaId.WreckedShip => MapLandmarkDefinitions.WreckedShipBosses,
        AreaId.Maridia => MapLandmarkDefinitions.MaridiaBosses,
        AreaId.Ceres => MapLandmarkDefinitions.CeresBosses,
        _ => throw new ArgumentOutOfRangeException(nameof(area)),
    };

    /// <summary>The elevator labels of one of the six Zebes areas' maps, in drawing order; Ceres has none.</summary>
    public static ImmutableArray<MapElevatorLabel> Elevators(AreaId area) => area switch
    {
        AreaId.Crateria => MapLandmarkDefinitions.CrateriaElevators,
        AreaId.Brinstar => MapLandmarkDefinitions.BrinstarElevators,
        AreaId.Norfair => MapLandmarkDefinitions.NorfairElevators,
        AreaId.WreckedShip => MapLandmarkDefinitions.WreckedShipElevators,
        AreaId.Maridia => MapLandmarkDefinitions.MaridiaElevators,
        AreaId.Tourian => MapLandmarkDefinitions.TourianElevators,
        AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels."),
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels."),
    };

    /// <summary>The map picture of the elevator label leading to <paramref name="destination"/>, one of the five for Crateria through Maridia.</summary>
    public static MapSpriteId ElevatorPicture(AreaId destination) => destination switch
    {
        AreaId.Crateria => MapSpriteId.ElevatorCrateria, AreaId.Brinstar => MapSpriteId.ElevatorBrinstar,
        AreaId.Norfair => MapSpriteId.ElevatorNorfair, AreaId.WreckedShip => MapSpriteId.ElevatorWreckedShip,
        AreaId.Maridia => MapSpriteId.ElevatorMaridia,
        AreaId.Tourian or
            AreaId.Ceres => throw new ArgumentOutOfRangeException(nameof(destination)),
        _ => throw new ArgumentOutOfRangeException(nameof(destination))
    };
}
