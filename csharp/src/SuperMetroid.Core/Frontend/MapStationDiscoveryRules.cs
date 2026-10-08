using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Mutually exclusive station marker families in the normal file-select map.</summary>
public enum MapStationKind
{
    /// <summary>Missile-refill markers selected through the area-pointer table at $82:C7DB.</summary>
    Missile,
    /// <summary>Energy-refill markers selected through the area-pointer table at $82:C7EB.</summary>
    Energy,
    /// <summary>Map-download station markers selected through the area-pointer table at $82:C7FB.</summary>
    Map
}

/// <summary>Stable marker identity and original exploration cell, independent of authored drawing coordinates.</summary>
/// <param name="Id">Stable area/family/ordinal key used to resolve the editable marker drawing position.</param>
/// <param name="Area">Area whose saved exploration bits govern this marker's visibility.</param>
/// <param name="CellX">Original horizontal exploration-cell index, obtained from the native map-pixel X divided by eight.</param>
/// <param name="CellY">Original vertical exploration-cell index, obtained from the native map-pixel Y divided by eight.</param>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, int CellX, int CellY);

/// <summary>Application-owned discovery cells from the retail $82:C7DB/C7EB/C7FB station lists.</summary>
public static class MapStationDiscoveryRules
{
    /// <summary>$82:C8A3: Brinstar missile station; $82:C9E1: Maridia missile station.</summary>
    private static readonly MapStationDiscoveryRule[] missile =
    [
        new("Brinstar.Missile.0", AreaId.Brinstar, 5, 8),
        new("Maridia.Missile.0", AreaId.Maridia, 38, 9),
    ];
    /// <summary>$82:C8A9/C913/C9E7/CA49: Brinstar, Norfair, Maridia and Tourian energy station cells.</summary>
    private static readonly MapStationDiscoveryRule[] energy =
    [
        new("Brinstar.Energy.0", AreaId.Brinstar, 9, 13),
        new("Brinstar.Energy.1", AreaId.Brinstar, 32, 19),
        new("Brinstar.Energy.2", AreaId.Brinstar, 54, 19),
        new("Norfair.Energy.0", AreaId.Norfair, 20, 10),
        new("Norfair.Energy.1", AreaId.Norfair, 21, 16),
        new("Maridia.Energy.0", AreaId.Maridia, 42, 7),
        new("Tourian.Energy.0", AreaId.Tourian, 11, 17),
    ];
    /// <summary>$82:C84D/C8B7/C91D/C98B/C9ED: the five map station exploration cells.</summary>
    private static readonly MapStationDiscoveryRule[] map =
    [
        new("Crateria.Map.0", AreaId.Crateria, 23, 8),
        new("Brinstar.Map.0", AreaId.Brinstar, 5, 5),
        new("Norfair.Map.0", AreaId.Norfair, 9, 5),
        new("WreckedShip.Map.0", AreaId.WreckedShip, 13, 20),
        new("Maridia.Map.0", AreaId.Maridia, 17, 18),
    ];

    /// <summary>Every compiled station marker in missile, energy, then map order; used to require the complete marker set in editable layouts.</summary>
    public static IEnumerable<MapStationDiscoveryRule> All => missile.Concat(energy).Concat(map);
    /// <summary>Selects an area's marker family without testing exploration or consulting editable drawing positions.</summary>
    /// <param name="area">Validated retail area identity whose saved map owns the discovery cells.</param>
    /// <param name="kind">The mutually exclusive missile, energy, or map station family.</param>
    /// <returns>Matching rules in native list order, or an empty sequence if that area has no markers of the selected family.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station kind is not a supported identity.</exception>
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
