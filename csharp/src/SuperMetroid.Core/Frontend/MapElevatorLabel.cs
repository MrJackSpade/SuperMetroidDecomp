using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Frontend;

/// <summary>A cosmetic elevator label attached to a fixed map destination identity.</summary>
public readonly record struct MapElevatorLabel(string Id, AreaId Destination);
