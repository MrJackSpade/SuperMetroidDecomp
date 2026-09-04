namespace SuperMetroid.Core.Game;

/// <summary>Combines persistent cartridge map state with a nonpersistent host reveal mode.</summary>
public static class AreaMapVisibility
{
    /// <summary>
    /// Determines whether one cell is presented without modifying exploration or map-station
    /// state. The caller separately retains <paramref name="explored"/> so entered cells can
    /// continue using their distinct palette in every reveal mode.
    /// </summary>
    public static bool IsVisible(
        bool explored,
        bool areaMapAcquired,
        bool stationVisible,
        bool discoverable,
        MapRevealMode revealMode) =>
        explored || revealMode switch
        {
            MapRevealMode.None => areaMapAcquired && stationVisible,
            MapRevealMode.Public => stationVisible,
            MapRevealMode.Secret => discoverable,
            _ => throw new ArgumentOutOfRangeException(
                nameof(revealMode),
                revealMode,
                "Map reveal mode is not defined."),
        };
}
