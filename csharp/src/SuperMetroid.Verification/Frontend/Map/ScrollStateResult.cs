using SuperMetroid.Core.Frontend.Map;

namespace SuperMetroid.Verification.Frontend.Map;

/// <summary>The first, second, third and fourth of one scroll state result.</summary>
/// <param name="First">The first.</param>
/// <param name="Second">The second.</param>
/// <param name="Third">The third.</param>
/// <param name="Fourth">The fourth.</param>
internal readonly record struct ScrollStateResult(Position16<MapPixels> First, Position16<MapPixels> Second, Position16<MapPixels> Third, MapScrollDirection Fourth);
