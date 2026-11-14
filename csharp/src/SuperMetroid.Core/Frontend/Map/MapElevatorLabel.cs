using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>An elevator label of an area map (#1773): the named landmark it stands on and the area it leads to.</summary>
/// <param name="Id">The landmark identity naming the label's installed position.</param>
/// <param name="Destination">The area the elevator leads to, which chooses the label's picture.</param>
public readonly record struct MapElevatorLabel(string Id, AreaId Destination);
