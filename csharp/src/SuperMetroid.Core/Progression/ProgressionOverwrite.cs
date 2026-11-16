namespace SuperMetroid.Core.Progression;

/// <summary>
/// What the Spacetime Beam's runaway colour copy does to saved progress (#1318, #1330, #1474, #1475): which story events, the
/// Zebetite count (#1527), which area bosses, item locations, doors, used stations and acquired area maps it turns on or off, and the loading
/// state it leaves the game to resume in. The copy runs through progress in a fixed order (events, bosses, orbs, items,
/// doors, stations, maps, loading state), so it decides everything before the point where it stops and nothing after.
/// </summary>
/// <remarks>
/// <see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/> yields it and <see cref="SharedGameState"/> applies
/// it. Evidence: the recorded Pit Room probe's two input phases leave the corrupted loading state this overwrite gives,
/// and the game's copy, run over the game's data in every situation, leaves every event, boss, orb, item, door, station
/// and map as this overwrite does; only the progress part of the overwrite is kept (#1318).
/// </remarks>
/// <param name="Events">The story events the copy decides.</param>
/// <param name="ZebetitesDestroyed">The Zebetite count the copy leaves, or null when it stops before deciding it.</param>
/// <param name="AreaBosses">Each area whose defeated bosses the copy replaces, with the bosses it leaves defeated.</param>
/// <param name="CollectedItems">The item locations the copy decides.</param>
/// <param name="OpenedDoors">The doors the copy decides.</param>
/// <param name="UsedStations">The used save stations and elevators the copy decides.</param>
/// <param name="AreaMaps">Each area whose map the copy leaves acquired (true) or not (false).</param>
/// <param name="SavedStartupRoute">The saved startup route replacing the session's, or null when the copy stops before it.</param>
public sealed record ProgressionOverwrite(
    DecidedFlags<StoryEvent> Events,
    int? ZebetitesDestroyed,
    IReadOnlyDictionary<AreaId, DefeatedBosses> AreaBosses,
    DecidedFlags<ItemLocation> CollectedItems,
    DecidedFlags<LockedDoor> OpenedDoors,
    DecidedFlags<LoadStation> UsedStations,
    IReadOnlyDictionary<AreaId, bool> AreaMaps,
    SaveLoadingGameState? SavedStartupRoute);
