using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>A cosmetic elevator label attached to a fixed map destination identity.</summary>
public readonly record struct MapElevatorLabel(string Id, AreaId Destination);

/// <summary>Landmark identities, native slot order and eligibility remain application-owned.</summary>
public static class MapLandmarkDefinitions
{
    /// <summary>$82:C83B: three unused boss slots still consume boss-state bits.</summary>
    private static readonly string?[] crateriaBosses = [null, null, null];
    /// <summary>$82:C89D: Brinstar's visible boss marker.</summary>
    private static readonly string?[] brinstarBosses = ["Boss.Kraid"];
    /// <summary>$82:C90B: Norfair's visible boss marker.</summary>
    private static readonly string?[] norfairBosses = ["Boss.Ridley"];
    /// <summary>$82:C981: Wrecked Ship's visible boss marker.</summary>
    private static readonly string?[] wreckedShipBosses = ["Boss.Phantoon"];
    /// <summary>$82:C9DB: Maridia's visible boss marker.</summary>
    private static readonly string?[] maridiaBosses = ["Boss.Draygon"];
    /// <summary>$82:CA9B: Ceres's boss-list entry; no world-select elevator list exists.</summary>
    private static readonly string?[] ceresBosses = ["Boss.CeresRidley"];
    /// <summary>$82:C853 first save coordinate is also the unconditional Crateria gunship icon.</summary>
    public const string Gunship = "Crateria.Gunship";

    /// <summary>$82:C759: five Crateria elevator destination labels, native order.</summary>
    private static readonly MapElevatorLabel[] crateriaElevators =
    [new("Crateria.Elevator.0", AreaId.Brinstar), new("Crateria.Elevator.1", AreaId.Brinstar),
     new("Crateria.Elevator.2", AreaId.Brinstar), new("Crateria.Elevator.3", AreaId.WreckedShip), new("Crateria.Elevator.4", AreaId.Maridia)];
    /// <summary>$82:C779: five Brinstar elevator destination labels.</summary>
    private static readonly MapElevatorLabel[] brinstarElevators =
    [new("Brinstar.Elevator.0", AreaId.Crateria), new("Brinstar.Elevator.1", AreaId.Crateria),
     new("Brinstar.Elevator.2", AreaId.Crateria), new("Brinstar.Elevator.3", AreaId.Maridia), new("Brinstar.Elevator.4", AreaId.Norfair)];
    /// <summary>$82:C799: Norfair's Brinstar destination label.</summary>
    private static readonly MapElevatorLabel[] norfairElevators = [new("Norfair.Elevator.0", AreaId.Brinstar)];
    /// <summary>$82:C7A1: Wrecked Ship's two Crateria destination labels.</summary>
    private static readonly MapElevatorLabel[] wreckedShipElevators = [new("WreckedShip.Elevator.0", AreaId.Crateria), new("WreckedShip.Elevator.1", AreaId.Crateria)];
    /// <summary>$82:C7AF: Maridia's Crateria and two Brinstar destination labels.</summary>
    private static readonly MapElevatorLabel[] maridiaElevators = [new("Maridia.Elevator.0", AreaId.Crateria), new("Maridia.Elevator.1", AreaId.Brinstar), new("Maridia.Elevator.2", AreaId.Brinstar)];
    /// <summary>$82:C7C3: Tourian's Crateria destination label.</summary>
    private static readonly MapElevatorLabel[] tourianElevators = [new("Tourian.Elevator.0", AreaId.Crateria)];

    public static ReadOnlySpan<string?> Bosses(AreaId area) => area switch
    {
        AreaId.Crateria => crateriaBosses, AreaId.Brinstar => brinstarBosses,
        AreaId.Norfair => norfairBosses, AreaId.WreckedShip => wreckedShipBosses,
        AreaId.Maridia => maridiaBosses, AreaId.Tourian => [], AreaId.Ceres => ceresBosses,
        _ => throw new ArgumentOutOfRangeException(nameof(area))
    };
    public static ReadOnlySpan<MapElevatorLabel> Elevators(AreaId area) => area switch
    {
        AreaId.Crateria => crateriaElevators, AreaId.Brinstar => brinstarElevators,
        AreaId.Norfair => norfairElevators, AreaId.WreckedShip => wreckedShipElevators,
        AreaId.Maridia => maridiaElevators, AreaId.Tourian => tourianElevators,
        _ => throw new ArgumentOutOfRangeException(nameof(area), "Only the six Zebes areas have elevator map labels.")
    };

    /// <summary>$82:C759..C7CA use menu spritemaps $59..$5D for Crateria through Maridia labels.</summary>
    public static ushort ElevatorSpritemap(AreaId destination) => destination switch
    {
        AreaId.Crateria => 0x59, AreaId.Brinstar => 0x5a, AreaId.Norfair => 0x5b,
        AreaId.WreckedShip => 0x5c, AreaId.Maridia => 0x5d,
        _ => throw new ArgumentOutOfRangeException(nameof(destination))
    };

    public static IEnumerable<string> AllIds()
    {
        foreach (AreaId area in Enum.GetValues<AreaId>())
        {
            foreach (string? id in Bosses(area).ToArray()) if (id is not null) yield return id;
            if (area != AreaId.Ceres)
                foreach (var label in Elevators(area).ToArray()) yield return label.Id;
        }
        yield return Gunship;
    }
}
