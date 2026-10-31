namespace SuperMetroid.Core.Progression;

/// <summary>
/// What SpaceTime's runaway palette copy does to saved progress (#1318, #1330, #1474, #1475): which story events, area
/// bosses, Chozo orbs, item locations, doors, used stations and acquired area maps it turns on or off, and the loading
/// state it leaves the game to resume in. The copy runs through progress in a fixed order (events, bosses, orbs, items,
/// doors, stations, maps, loading state), so it decides everything before the point where it stops and nothing after.
/// </summary>
/// <remarks>
/// <see cref="SpaceTimeProgressionOverwrite{TCursor, TOverwrite}"/> yields it and <see cref="SharedGameState"/> applies
/// it. Evidence: the SpaceTime check replays the native Pit Room probe's two alpha passes and matches its corrupted loading
/// state, and runs the original copy over the cartridge for every situation, requiring this overwrite to leave every
/// event, boss, orb, item, door, station and map as the copy does; the owner kept only the progress part of the
/// overwrite (#1318).
/// </remarks>
/// <param name="Events">The story events, by event number, the copy decides.</param>
/// <param name="AreaBosses">Each area whose defeated bosses the copy replaces, with the bosses it leaves defeated.</param>
/// <param name="ChozoOrbs">The Chozo orbs, by room-data number, the copy decides.</param>
/// <param name="CollectedItems">The item locations, by room-data number, the copy decides.</param>
/// <param name="OpenedDoors">The doors, by room-data number, the copy decides.</param>
/// <param name="UsedStations">Each area whose used save stations and elevators, by station number, the copy decides.</param>
/// <param name="AreaMaps">Each area whose map the copy leaves acquired (true) or not (false).</param>
/// <param name="SavedLoadingGameState">The saved loading state replacing the session's, 0 through 65535, or null when the copy stops before it.</param>
public sealed record ProgressionOverwrite(
    FlagOverwrite Events,
    IReadOnlyDictionary<AreaId, BossBits> AreaBosses,
    FlagOverwrite ChozoOrbs,
    FlagOverwrite CollectedItems,
    FlagOverwrite OpenedDoors,
    IReadOnlyDictionary<AreaId, FlagOverwrite> UsedStations,
    IReadOnlyDictionary<AreaId, bool> AreaMaps,
    int? SavedLoadingGameState);
