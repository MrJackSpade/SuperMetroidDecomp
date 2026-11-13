namespace SuperMetroid.Core.Progression;

/// <summary>
/// A place the game loads Samus at: a save station, an elevator's arrival, the Ceres start or the arrival on Zebes. It is
/// an identity, named in <see cref="LoadStationDefinitions"/>: a save names the station it resumes at, the used-station
/// record (<see cref="PersistentProgress.UsedStations"/>) keeps the stations Samus has used, and the room loader reads
/// where loading at the station puts Samus from the station's placement. A station is its name and area, so a copy of
/// one, such as a restored saved state holds, is the same station.
/// </summary>
public sealed record LoadStation
{
    /// <summary>Names one load station.</summary>
    /// <param name="name">The station's name.</param>
    /// <param name="area">The area the station is in.</param>
    internal LoadStation(string name, AreaId area)
    {
        Name = Ensure.NotNull(name);
        Area = AreaIds.Require(area);
    }

    /// <summary>The station's name.</summary>
    public string Name { get; }

    /// <summary>The area the station is in.</summary>
    public AreaId Area { get; }

    /// <summary>The station's name.</summary>
    public override string ToString() => Name;
}
