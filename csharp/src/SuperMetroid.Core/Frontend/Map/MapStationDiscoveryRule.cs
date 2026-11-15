using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Stable marker identity and original exploration cell, independent of authored drawing coordinates.</summary>
/// <param name="Id">The name of the room the station stands in, the key its editable marker drawing position is installed under.</param>
/// <param name="Area">Area whose saved exploration bits govern this marker's visibility.</param>
/// <param name="Cell">Original exploration cell, obtained from the map-pixel position divided by eight.</param>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, Position16<MapTiles> Cell);
