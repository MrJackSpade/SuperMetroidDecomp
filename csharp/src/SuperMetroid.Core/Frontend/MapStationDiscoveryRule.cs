using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>Stable marker identity and original exploration cell, independent of authored drawing coordinates.</summary>
/// <param name="Id">Stable area/family/ordinal key used to resolve the editable marker drawing position.</param>
/// <param name="Area">Area whose saved exploration bits govern this marker's visibility.</param>
/// <param name="Cell">Original exploration cell, obtained from the native map-pixel position divided by eight.</param>
public readonly record struct MapStationDiscoveryRule(string Id, AreaId Area, Position16<MapTiles> Cell);
