using SuperMetroid.Core.Progression;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The area, station, x and y of one original map load anchor.</summary>
/// <param name="Area">The area.</param>
/// <param name="Station">The station.</param>
/// <param name="X">The x.</param>
/// <param name="Y">The y.</param>
internal readonly record struct OriginalMapLoadAnchor(AreaId Area, int Station, ushort X, ushort Y);
