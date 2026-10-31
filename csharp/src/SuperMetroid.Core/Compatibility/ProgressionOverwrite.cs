namespace SuperMetroid.Core.Compatibility;

/// <summary>
/// What SpaceTime's runaway palette copy writes over saved progression (#1318, #1330, #1474): for each progression table,
/// the leading entries it replaces, in the table's own stored form. The copy reaches progression from the start of the
/// story events and runs on in table order, so it replaces a prefix of the tables: every table before the one where it
/// stops entirely, that table's leading entries, and nothing after. The loading state the game resumes in is replaced
/// whole or not at all.
/// </summary>
/// <remarks>
/// Applied by the progression state when <see cref="SpaceTimeProgressionOverwrite{TCursor}"/> yields it. Evidence: the
/// SpaceTime check replays the native Pit Room probe's two alpha passes and matches its progression hash and corrupted
/// loading state; the owner kept only the progression part of the overwrite (#1318).
/// </remarks>
/// <param name="Events">The leading story-event bitset bytes replaced, each 0 through 255.</param>
/// <param name="AreaBosses">The leading per-area boss bitsets replaced, each 0 through 255.</param>
/// <param name="ChozoOrbs">The leading opened-Chozo-orb bitset bytes replaced, each 0 through 255.</param>
/// <param name="CollectedItems">The leading collected-item bitset bytes replaced, each 0 through 255.</param>
/// <param name="OpenedDoors">The leading opened-door bitset bytes replaced, each 0 through 255.</param>
/// <param name="UsedStations">The leading used save-station and elevator bitset bytes replaced, each 0 through 255.</param>
/// <param name="MapStations">The leading acquired map-station bytes replaced, each 0 through 255.</param>
/// <param name="SavedLoadingGameState">The saved loading state replacing the session's, 0 through 65535, or null when the copy stops before it.</param>
[CompatibilityService("SpaceTime's palette copy runs past the palette and overwrites saved progression")]
public sealed record ProgressionOverwrite(
    IReadOnlyList<int> Events,
    IReadOnlyList<int> AreaBosses,
    IReadOnlyList<int> ChozoOrbs,
    IReadOnlyList<int> CollectedItems,
    IReadOnlyList<int> OpenedDoors,
    IReadOnlyList<int> UsedStations,
    IReadOnlyList<int> MapStations,
    int? SavedLoadingGameState);
