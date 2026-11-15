using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>A map scroll's state, compared between two scrolls framed from the same map.</summary>
/// <param name="Scroll">The scroll position.</param>
/// <param name="Limits">How far the revealed map reaches.</param>
/// <param name="Direction">The step under way.</param>
internal readonly record struct ScrollStateResult(Position16<MapPixels> Scroll, MapScrollLimits Limits, MapScrollDirection Direction);
