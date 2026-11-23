using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>
/// What an encounter decides in the reveal both Ridleys open with (#2229): how long he waits before his eyes fade and holds
/// his roar, and what he does while he holds it before his take-off.
/// </summary>
/// <typeparam name="TState">The encounter's state.</typeparam>
internal interface IRidleyRevealEncounter<in TState> where TState : RidleyEncounterState
{
    /// <summary>The updates Ridley waits once Samus's door has finished; on the last his eyes begin to fade in.</summary>
    int InitialDelayUpdates { get; }

    /// <summary>Where Ridley's picture stands among the room's actors once his body has faded in (#2618).</summary>
    EnemyStacking RevealedStacking { get; }

    /// <summary>Begins the hold that starts with his roar, which <see cref="TakesOff"/> runs (#2409).</summary>
    /// <param name="ridley">Ridley's encounter.</param>
    void BeginRoarHold(TState ridley);

    /// <summary>Runs one update of the hold before Ridley's take-off, returning whether he takes off on it.</summary>
    /// <param name="place">Ridley's body.</param>
    /// <param name="ridley">Ridley's encounter.</param>
    bool TakesOff(RoomEnemyPlace place, TState ridley);
}
