using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The area maps' landmarks (#1773): each area's boss markers with the boss each shows, each area's elevator labels with
/// the area each leads to, in drawing order, and the gunship. Each identity names an installed position.
/// </summary>
public static class MapLandmarkDefinitions
{
    /// <summary>The first save coordinate is also the unconditional Crateria gunship icon.</summary>
    public const string Gunship = "Crateria.Gunship";

    /// <summary>Kraid's marker on the Brinstar map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> BrinstarBosses = [new("Boss.Kraid", DefeatedBosses.AreaBoss)];
    /// <summary>Ridley's marker on the Norfair map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> NorfairBosses = [new("Boss.Ridley", DefeatedBosses.AreaBoss)];
    /// <summary>Phantoon's marker on the Wrecked Ship map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> WreckedShipBosses = [new("Boss.Phantoon", DefeatedBosses.AreaBoss)];
    /// <summary>Draygon's marker on the Maridia map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> MaridiaBosses = [new("Boss.Draygon", DefeatedBosses.AreaBoss)];
    /// <summary>Ridley's marker on the Ceres map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> CeresBosses = [new("Boss.CeresRidley", DefeatedBosses.AreaBoss)];

    /// <summary>Crateria's labels: three to Brinstar from west to east, then Wrecked Ship and Maridia.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> CrateriaElevators =
    [
        new("Crateria.Elevator.ToBrinstarWest", AreaId.Brinstar),
        new("Crateria.Elevator.ToBrinstarCentral", AreaId.Brinstar),
        new("Crateria.Elevator.ToBrinstarEast", AreaId.Brinstar),
        new("Crateria.Elevator.ToWreckedShip", AreaId.WreckedShip),
        new("Crateria.Elevator.ToMaridia", AreaId.Maridia),
    ];
    /// <summary>Brinstar's labels: three to Crateria from west to east, then Maridia and Norfair.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> BrinstarElevators =
    [
        new("Brinstar.Elevator.ToCrateriaWest", AreaId.Crateria),
        new("Brinstar.Elevator.ToCrateriaCentral", AreaId.Crateria),
        new("Brinstar.Elevator.ToCrateriaEast", AreaId.Crateria),
        new("Brinstar.Elevator.ToMaridia", AreaId.Maridia),
        new("Brinstar.Elevator.ToNorfair", AreaId.Norfair),
    ];
    /// <summary>Norfair's label to Brinstar.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> NorfairElevators = [new("Norfair.Elevator.ToBrinstar", AreaId.Brinstar)];
    /// <summary>Wrecked Ship's labels: two to Crateria, west then east.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> WreckedShipElevators =
    [
        new("WreckedShip.Elevator.ToCrateriaWest", AreaId.Crateria),
        new("WreckedShip.Elevator.ToCrateriaEast", AreaId.Crateria),
    ];
    /// <summary>Maridia's labels: Crateria, then two to Brinstar, west then east.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> MaridiaElevators =
    [
        new("Maridia.Elevator.ToCrateria", AreaId.Crateria),
        new("Maridia.Elevator.ToBrinstarWest", AreaId.Brinstar),
        new("Maridia.Elevator.ToBrinstarEast", AreaId.Brinstar),
    ];
    /// <summary>Tourian's label to Crateria.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> TourianElevators = [new("Tourian.Elevator.ToCrateria", AreaId.Crateria)];
}
