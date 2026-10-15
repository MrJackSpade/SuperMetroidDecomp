using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>A cosmetic elevator label attached to a fixed map destination identity.</summary>
public readonly record struct MapElevatorLabel(string Id, AreaId Destination);
