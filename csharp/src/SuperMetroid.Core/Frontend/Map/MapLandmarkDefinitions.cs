using System.Collections.Immutable;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// The area maps' landmarks (#1773): each area's boss markers with the boss each shows, each area's elevator labels with
/// the area each leads to, in drawing order, and the gunship. Each identity names an installed position.
/// </summary>
public static partial class MapLandmarkDefinitions
{
    /// <summary>The gunship's marker on the Crateria map, shown whatever Samus has explored.</summary>
    public const string Gunship = "Crateria.Gunship";

    /// <summary>The areas whose maps show the gunship: Crateria, where it landed.</summary>
    internal static readonly AreaId[] GunshipAreas = [AreaId.Crateria];

    /// <summary>Kraid's marker on the Brinstar map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> BrinstarBosses = [new("Boss.Kraid", AreaBoss.Kraid)];
    /// <summary>Ridley's marker on the Norfair map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> NorfairBosses = [new("Boss.Ridley", AreaBoss.Ridley)];
    /// <summary>Phantoon's marker on the Wrecked Ship map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> WreckedShipBosses = [new("Boss.Phantoon", AreaBoss.Phantoon)];
    /// <summary>Draygon's marker on the Maridia map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> MaridiaBosses = [new("Boss.Draygon", AreaBoss.Draygon)];
    /// <summary>Ridley's marker on the Ceres map.</summary>
    internal static readonly ImmutableArray<MapBossMarker> CeresBosses = [new("Boss.CeresRidley", AreaBoss.CeresRidley)];

    /// <summary>Crateria's labels: three to Brinstar from west to east, then Wrecked Ship and Maridia.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> CrateriaElevators =
    [
        new("Crateria.Elevator.ToBrinstarWest", MapElevatorDestination.Brinstar),
        new("Crateria.Elevator.ToBrinstarCentral", MapElevatorDestination.Brinstar),
        new("Crateria.Elevator.ToBrinstarEast", MapElevatorDestination.Brinstar),
        new("Crateria.Elevator.ToWreckedShip", MapElevatorDestination.WreckedShip),
        new("Crateria.Elevator.ToMaridia", MapElevatorDestination.Maridia),
    ];
    /// <summary>Brinstar's labels: three to Crateria from west to east, then Maridia and Norfair.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> BrinstarElevators =
    [
        new("Brinstar.Elevator.ToCrateriaWest", MapElevatorDestination.Crateria),
        new("Brinstar.Elevator.ToCrateriaCentral", MapElevatorDestination.Crateria),
        new("Brinstar.Elevator.ToCrateriaEast", MapElevatorDestination.Crateria),
        new("Brinstar.Elevator.ToMaridia", MapElevatorDestination.Maridia),
        new("Brinstar.Elevator.ToNorfair", MapElevatorDestination.Norfair),
    ];
    /// <summary>Norfair's label to Brinstar.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> NorfairElevators = [new("Norfair.Elevator.ToBrinstar", MapElevatorDestination.Brinstar)];
    /// <summary>Wrecked Ship's labels: two to Crateria, west then east.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> WreckedShipElevators =
    [
        new("WreckedShip.Elevator.ToCrateriaWest", MapElevatorDestination.Crateria),
        new("WreckedShip.Elevator.ToCrateriaEast", MapElevatorDestination.Crateria),
    ];
    /// <summary>Maridia's labels: Crateria, then two to Brinstar, west then east.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> MaridiaElevators =
    [
        new("Maridia.Elevator.ToCrateria", MapElevatorDestination.Crateria),
        new("Maridia.Elevator.ToBrinstarWest", MapElevatorDestination.Brinstar),
        new("Maridia.Elevator.ToBrinstarEast", MapElevatorDestination.Brinstar),
    ];
    /// <summary>Tourian's label to Crateria.</summary>
    internal static readonly ImmutableArray<MapElevatorLabel> TourianElevators = [new("Tourian.Elevator.ToCrateria", MapElevatorDestination.Crateria)];
}
