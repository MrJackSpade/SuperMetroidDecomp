using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Mutually exclusive station marker families in the normal file-select map.</summary>
public enum MapStationKind { Missile, Energy, Map }

/// <summary>Stable marker identity and original exploration cell, independent of authored drawing coordinates.</summary>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, MapStationKind Kind, int Index, int CellX, int CellY);

/// <summary>Application-owned discovery cells from the retail $82:C7DB/C7EB/C7FB station lists.</summary>
public static class MapStationDiscoveryRules
{
    /// <summary>$82:C8A3: Brinstar missile station; $82:C9E1: Maridia missile station.</summary>
    private static readonly MapStationDiscoveryRule[] missile =
    [
        new("Brinstar.Missile.0", AreaId.Brinstar, MapStationKind.Missile, 0, 5, 8),
        new("Maridia.Missile.0", AreaId.Maridia, MapStationKind.Missile, 0, 38, 9),
    ];
    /// <summary>$82:C8A9/C913/C9E7/CA49: Brinstar, Norfair, Maridia and Tourian energy station cells.</summary>
    private static readonly MapStationDiscoveryRule[] energy =
    [
        new("Brinstar.Energy.0", AreaId.Brinstar, MapStationKind.Energy, 0, 9, 13),
        new("Brinstar.Energy.1", AreaId.Brinstar, MapStationKind.Energy, 1, 32, 19),
        new("Brinstar.Energy.2", AreaId.Brinstar, MapStationKind.Energy, 2, 54, 19),
        new("Norfair.Energy.0", AreaId.Norfair, MapStationKind.Energy, 0, 20, 10),
        new("Norfair.Energy.1", AreaId.Norfair, MapStationKind.Energy, 1, 21, 16),
        new("Maridia.Energy.0", AreaId.Maridia, MapStationKind.Energy, 0, 42, 7),
        new("Tourian.Energy.0", AreaId.Tourian, MapStationKind.Energy, 0, 11, 17),
    ];
    /// <summary>$82:C84D/C8B7/C91D/C98B/C9ED: the five map station exploration cells.</summary>
    private static readonly MapStationDiscoveryRule[] map =
    [
        new("Crateria.Map.0", AreaId.Crateria, MapStationKind.Map, 0, 23, 8),
        new("Brinstar.Map.0", AreaId.Brinstar, MapStationKind.Map, 0, 5, 5),
        new("Norfair.Map.0", AreaId.Norfair, MapStationKind.Map, 0, 9, 5),
        new("WreckedShip.Map.0", AreaId.WreckedShip, MapStationKind.Map, 0, 13, 20),
        new("Maridia.Map.0", AreaId.Maridia, MapStationKind.Map, 0, 17, 18),
    ];

    public static IEnumerable<MapStationDiscoveryRule> All => missile.Concat(energy).Concat(map);
    public static IEnumerable<MapStationDiscoveryRule> Get(AreaId area, MapStationKind kind)
    {
        _ = AreaIds.ToIndex(area);
        MapStationDiscoveryRule[] source = kind switch
        {
            MapStationKind.Missile => missile,
            MapStationKind.Energy => energy,
            MapStationKind.Map => map,
            _ => throw new ArgumentOutOfRangeException(nameof(kind))
        };
        return source.Where(rule => rule.Area == area);
    }
}
