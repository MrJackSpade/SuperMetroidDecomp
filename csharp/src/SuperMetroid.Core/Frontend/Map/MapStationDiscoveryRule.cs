using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>A map station marker's identity and the map cell whose exploration shows it, independent of where the marker is drawn.</summary>
/// <param name="Id">
/// The key the marker's editable drawing position is installed under, the name of the room the station stands in, written
/// out in <see cref="MapStationDiscoveryRules"/> so renaming a room never changes it (#2548).
/// </param>
/// <param name="Area">The area whose exploration shows the marker.</param>
/// <param name="Cell">The map cell whose exploration shows the marker: the cell its map position lies in.</param>
public readonly partial record struct MapStationDiscoveryRule(string Id, AreaId Area, Position16<MapTiles> Cell);
