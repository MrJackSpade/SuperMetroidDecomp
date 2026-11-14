using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Runtime;

/// <summary>
/// What each gameplay runtime's shared game state continues from (#1801): the session's random number generator, the
/// power-on earthquake rumble delay and the live and stored explored maps, which the runtime's
/// <see cref="SharedGameState"/> reads and advances and the session keeps across games and demos.
/// </summary>
internal sealed record SharedGameStateCarryover
{
    /// <summary>The session's random number generator, which the runtime's system state reads and advances.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The frame the save file found at power-on adds to every earthquake rumble's delay, fixed for the session.</summary>
    public required PowerOnEarthquakeSoundDelay EarthquakeSoundDelay { get; init; }

    /// <summary>The session's live and stored explored maps, which the game keeps across games and demos.</summary>
    public required StoredExploredMaps<AreaId> StoredExploredMaps { get; init; }
}
