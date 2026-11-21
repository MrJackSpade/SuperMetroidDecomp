namespace SuperMetroid.Core.Map;

/// <summary>Combines persistent map state with a nonpersistent host reveal mode.</summary>
public static class AreaMapVisibility
{
    /// <summary>
    /// Determines whether one cell is presented without modifying exploration or map-station
    /// state. The map separately retains <paramref name="explored"/> so entered cells can
    /// continue using their distinct colours in every reveal mode.
    /// </summary>
    [IndependentFlag("Exploration, map acquisition, station reveal and discoverability are independent per-tile facts.")]
    public static bool IsVisible(
        bool explored,
        bool areaMapAcquired,
        bool stationVisible,
        bool discoverable,
        MapRevealMode revealMode) =>
        explored || (revealMode == MapRevealMode.None ? areaMapAcquired && stationVisible
            : revealMode == MapRevealMode.Public ? stationVisible
            // The remaining member, MapRevealMode.Secret.
            : discoverable);
}
