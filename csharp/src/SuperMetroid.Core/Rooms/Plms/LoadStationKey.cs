using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Rooms.Plms;

/// <summary>A load station of an area.</summary>
/// <param name="Area">The area.</param>
/// <param name="Station">The station.</param>
internal readonly record struct LoadStationKey(AreaId Area, int Station);
