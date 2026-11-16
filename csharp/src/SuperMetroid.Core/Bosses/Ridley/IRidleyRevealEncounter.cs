using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What an encounter decides in the reveal both Ridleys open with (#2229): how long he waits before his eyes fade and holds
/// his roar, and what he does while he holds it before his take-off.
/// </summary>
/// <typeparam name="TState">The encounter's state.</typeparam>
internal interface IRidleyRevealEncounter<in TState> where TState : RidleyEncounterState
{
    /// <summary>The updates Ridley waits, once Samus's door has finished, before his eyes fade.</summary>
    int InitialDelay { get; }

    /// <summary>The updates his roar's hold before take-off counts down from.</summary>
    int RoarHold { get; }

    /// <summary>Runs one update of the hold before Ridley's take-off, returning whether he takes off on it.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    bool TakesOff(RoomEnemyPlace place, TState ridley);
}
