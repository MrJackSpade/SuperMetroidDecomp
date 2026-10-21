using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The scroll and direction of one verify compiled map scroll controls case.</summary>
/// <param name="Scroll">The scroll.</param>
/// <param name="Direction">The direction.</param>
internal readonly record struct VerifyCompiledMapScrollControlsCase(Position16<MapPixels> Scroll, MapScrollDirection Direction);
