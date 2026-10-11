using SuperMetroid.Core.Game;
using SuperMetroid.Core.Spatial;

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
/// <param name="Cell">Original exploration cell, obtained from the native map-pixel position divided by eight.</param>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, Position16<MapTiles> Cell);

/// <summary>Application-owned discovery cells from the retail $82:C7DB/C7EB/C7FB station lists.</summary>
public static class MapStationDiscoveryRules
{
    /// <summary>$82:C8A3: Brinstar missile station; $82:C9E1: Maridia missile station.</summary>
    private static readonly MapStationDiscoveryRule[] missile =
    [
        new("Brinstar.Missile.0", AreaId.Brinstar, new(5, 8)),
        new("Maridia.Missile.0", AreaId.Maridia, new(38, 9)),
    ];
    /// <summary>$82:C8A9/C913/C9E7/CA49: Brinstar, Norfair, Maridia and Tourian energy station cells.</summary>
    private static readonly MapStationDiscoveryRule[] energy =
    [
        new("Brinstar.Energy.0", AreaId.Brinstar, new(9, 13)),
        new("Brinstar.Energy.1", AreaId.Brinstar, new(32, 19)),
        new("Brinstar.Energy.2", AreaId.Brinstar, new(54, 19)),
        new("Norfair.Energy.0", AreaId.Norfair, new(20, 10)),
        new("Norfair.Energy.1", AreaId.Norfair, new(21, 16)),
        new("Maridia.Energy.0", AreaId.Maridia, new(42, 7)),
        new("Tourian.Energy.0", AreaId.Tourian, new(11, 17)),
    ];
    /// <summary>$82:C84D/C8B7/C91D/C98B/C9ED: the five map station exploration cells.</summary>
    private static readonly MapStationDiscoveryRule[] map =
    [
        new("Crateria.Map.0", AreaId.Crateria, new(23, 8)),
        new("Brinstar.Map.0", AreaId.Brinstar, new(5, 5)),
        new("Norfair.Map.0", AreaId.Norfair, new(9, 5)),
        new("WreckedShip.Map.0", AreaId.WreckedShip, new(13, 20)),
        new("Maridia.Map.0", AreaId.Maridia, new(17, 18)),
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
