using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>A map station marker's identity and the map cell whose exploration shows it, independent of where the marker is drawn.</summary>
/// <param name="Id">The name of the room the station stands in, the key its editable marker drawing position is installed under.</param>
/// <param name="Area">Area whose saved exploration bits govern this marker's visibility.</param>
/// <param name="Cell">The map cell whose exploration shows the marker: the cell its map position lies in.</param>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, Position16<MapTiles> Cell);
