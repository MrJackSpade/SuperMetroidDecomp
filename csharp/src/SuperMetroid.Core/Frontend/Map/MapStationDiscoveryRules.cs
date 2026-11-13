using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Application-owned discovery cells from the retail station lists.</summary>
public static class MapStationDiscoveryRules
{
    /// <summary>The Brinstar and Maridia missile station cells.</summary>
    private static readonly MapStationDiscoveryRule[] missile =
    [
        new("Brinstar.Missile.0", AreaId.Brinstar, new(5, 8)),
        new("Maridia.Missile.0", AreaId.Maridia, new(38, 9)),
    ];
    /// <summary>The Brinstar, Norfair, Maridia and Tourian energy station cells.</summary>
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
    /// <summary>The five map station exploration cells.</summary>
    private static readonly MapStationDiscoveryRule[] map =
    [
        new("Crateria.Map.0", AreaId.Crateria, new(23, 8)),
        new("Brinstar.Map.0", AreaId.Brinstar, new(5, 5)),
        new("Norfair.Map.0", AreaId.Norfair, new(9, 5)),
        new("WreckedShip.Map.0", AreaId.WreckedShip, new(13, 20)),
        new("Maridia.Map.0", AreaId.Maridia, new(17, 18)),
    ];

    /// <summary>Selects an area's marker family without testing exploration or consulting editable drawing positions.</summary>
    /// <param name="area">Validated retail area identity whose saved map owns the discovery cells.</param>
    /// <param name="kind">The mutually exclusive missile, energy, or map station family.</param>
    /// <returns>Matching rules in list order, or an empty sequence if that area has no markers of the selected family.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The area or station kind is not a supported identity.</exception>
    public static IEnumerable<MapStationDiscoveryRule> Get(AreaId area, MapStationKind kind)
    {
        _ = AreaIds.Require(area);
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
