using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// Application-owned discovery cells from the retail station lists, each named for the room its station stands in (#1975):
/// the name its editable marker position is installed under.
/// </summary>
public static class MapStationDiscoveryRules
{
    /// <summary>The Brinstar and Maridia missile station cells.</summary>
    private static readonly MapStationDiscoveryRule[] missile =
    [
        new(nameof(RoomDefinitions.GreenBrinstarMissileRefill), AreaId.Brinstar, new(5, 8)),
        new(nameof(RoomDefinitions.MaridiaMissileRefill), AreaId.Maridia, new(38, 9)),
    ];
    /// <summary>The Brinstar, Norfair, Maridia and Tourian energy station cells.</summary>
    private static readonly MapStationDiscoveryRule[] energy =
    [
        new(nameof(RoomDefinitions.DachoraEnergyRefill), AreaId.Brinstar, new(9, 13)),
        new(nameof(RoomDefinitions.RedTowerRefill), AreaId.Brinstar, new(32, 19)),
        new(nameof(RoomDefinitions.KraidRefill), AreaId.Brinstar, new(54, 19)),
        new(nameof(RoomDefinitions.UpperNorfairRefill), AreaId.Norfair, new(20, 10)),
        new(nameof(RoomDefinitions.GTEnergyRefill), AreaId.Norfair, new(21, 16)),
        new(nameof(RoomDefinitions.MaridiaEnergyRefill), AreaId.Maridia, new(42, 7)),
        new(nameof(RoomDefinitions.TourianRecharge), AreaId.Tourian, new(11, 17)),
    ];
    /// <summary>The five map station exploration cells.</summary>
    private static readonly MapStationDiscoveryRule[] map =
    [
        new(nameof(RoomDefinitions.CrateriaMap), AreaId.Crateria, new(23, 8)),
        new(nameof(RoomDefinitions.GreenBrinstarMap), AreaId.Brinstar, new(5, 5)),
        new(nameof(RoomDefinitions.NorfairMap), AreaId.Norfair, new(9, 5)),
        new(nameof(RoomDefinitions.WreckedShipMap), AreaId.WreckedShip, new(13, 20)),
        new(nameof(RoomDefinitions.MaridiaMap), AreaId.Maridia, new(17, 18)),
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
