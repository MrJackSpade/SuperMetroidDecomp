using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One phase for every sequence of the game (#2571), each a typed member: the session holds the running phase, and a
/// sequence that hands over names the phase it hands over to, so no update looks its phase up by the game-state word.
/// </summary>
[ArchitectureReview("back-reference", "The session makes its phases once and owns them for its whole life, and each phase " +
    "runs the session it was made for, so the two share one lifetime; the set keeps no reference to the session itself.")]
internal sealed partial class GamePhases
{
    /// <summary>Builds every sequence's phase for <paramref name="session"/>.</summary>
    /// <param name="session">The session the phases run.</param>
    internal GamePhases(FrontendSession session)
    {
        Ensure.NotNull(session);
        Gameplay = new GameplayGamePhases(session);
        LoadingGame = new LoadingGameSequence(session);
        GameplayFadeIn = new GamePhase(SuperMetroidGameState.GameplayFadeIn,
            PhaseRandomStep.RuntimeOwned,
            step: Gameplay.RunGameplayFadeIn,
            name: Gameplay.GameplayFadeInName);
        MainGameplay = new GamePhase(SuperMetroidGameState.MainGameplay,
            PhaseRandomStep.RuntimeOwned,
            step: Gameplay.RunMainGameplay,
            name: GameplayGamePhases.MainGameplayName);
        DoorTransition = new DoorTransitionGameSequence(session);
        ReserveTankRefill = new GamePhase(SuperMetroidGameState.ReserveTankRefill,
            PhaseRandomStep.RuntimeOwned,
            step: Gameplay.RunReserveTankRefill,
            name: GameplayGamePhases.ReserveTankRefillName);
        GameOverMenu = new GameOverMenuGamePhase(session);
        CeresDeparture = new CeresDepartureGameSequence(session);
        Ending = new EndingGameSequence(session);
    }

    /// <summary>The gameplay phases, which the pause's brightening and the time-up's white-out also run (<see cref="GameplayGamePhases"/>).</summary>
    internal GameplayGamePhases Gameplay { get; }

    /// <summary>The loading game sequence's phase.</summary>
    internal IGamePhase LoadingGame { get; }

    /// <summary>The gameplay fade in sequence's phase.</summary>
    internal IGamePhase GameplayFadeIn { get; }

    /// <summary>The main gameplay sequence's phase.</summary>
    internal IGamePhase MainGameplay { get; }

    /// <summary>The door transition sequence's phase.</summary>
    internal IGamePhase DoorTransition { get; }

    /// <summary>The reserve tank refill sequence's phase.</summary>
    internal IGamePhase ReserveTankRefill { get; }

    /// <summary>The game over menu sequence's phase.</summary>
    internal IGamePhase GameOverMenu { get; }

    /// <summary>The ceres departure sequence's phase.</summary>
    internal IGamePhase CeresDeparture { get; }

    /// <summary>The ending sequence's phase.</summary>
    internal IGamePhase Ending { get; }
}
