using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>Explored map cells belonging to one persisted Zebes area.</summary>
/// <param name="Area">Area whose map coordinate space owns the cells.</param>
/// <param name="Tiles">Distinct explored 8-by-8 map-tile coordinates in that area.</param>
public sealed record ExploredAreaJsonDocument(
    AreaId Area,
    MapCoordinateJsonDocument[] Tiles);
