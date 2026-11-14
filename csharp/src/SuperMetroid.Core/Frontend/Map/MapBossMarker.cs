using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Map;

/// <summary>
/// A boss marker of an area map (#1773): the landmark it stands on and the boss it shows. The marker shows the defeated
/// picture once the area records <paramref name="Boss"/> defeated, and the living boss while the area's map is known.
/// </summary>
/// <param name="Id">The landmark identity naming the marker's installed position.</param>
/// <param name="Boss">The boss whose defeat the marker shows.</param>
public readonly record struct MapBossMarker(string Id, DefeatedBosses Boss);
