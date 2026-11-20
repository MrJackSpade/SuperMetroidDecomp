using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// One phase for every sequence of the game (#2571), each a typed member: the session holds the running phase, and a
/// sequence that hands over names the phase it hands over to, so no update looks its phase up by the game-state word.
/// </summary>
[ArchitectureReview("back-reference", "The session makes its phases once and owns them for its whole life, and each phase " +
    "runs the session it was made for, so the two share one lifetime; the set keeps no reference to the session itself.")]
[ArchitectureReview("context-bag", "The game's phases, one typed member per sequence, not services handed around: a " +
    "sequence that hands over names the phase it enters here, so no update looks a phase up by the game-state word.")]
internal sealed class GamePhases
{
    /// <summary>Builds every sequence's phase for <paramref name="session"/>.</summary>
    /// <param name="session">The session the phases run.</param>
    internal GamePhases(FrontendSession session)
    {
        Ensure.NotNull(session);
        Gameplay = new GameplayGamePhases(session);
        Title = new TitleGameSequence(session);
        FileSelect = new FileSelectGameSequence(session);
        LoadingGame = new LoadingGameSequence(session);
        GameplayFadeIn = new GamePhase(SuperMetroidGameState.GameplayFadeIn,
            PhaseRandomStep.RuntimeOwned,
            step: (controllerInput, live) => Gameplay.RunGameplayFadeIn(controllerInput, live),
            name: () => session.GameplayFadeLeadsToCeresArrival ? $"Ceres elevator fade-in (brightness {session.GameplayFadeIn.Level})" : $"Landing Site fade-in (brightness {session.GameplayFadeIn.Level})");
        MainGameplay = new GamePhase(SuperMetroidGameState.MainGameplay,
            PhaseRandomStep.RuntimeOwned,
            step: (controllerInput, live) => Gameplay.RunMainGameplay(controllerInput, live),
            name: () => "Gameplay");
        DoorTransition = new DoorTransitionGameSequence(session);
        Pause = new PauseGameSequence(session, Gameplay);
        ReserveTankRefill = new GamePhase(SuperMetroidGameState.ReserveTankRefill,
            PhaseRandomStep.RuntimeOwned,
            step: (controllerInput, live) => Gameplay.RunReserveTankRefill(controllerInput, live),
            name: () => "Reserve tank refill");
        DeathSequence = new DeathGameSequence(session);
        GameOverMenu = new GameOverMenuGamePhase(session);
        IntroCinematic = new IntroCinematicGamePhase(session);
        CeresDeparture = new CeresDepartureGameSequence(session);
        CeresDestruction = new CeresDestructionGameSequence(session);
        TimeUp = new TimeUpGameSequence(session, Gameplay);
        Ending = new EndingGameSequence(session);
        AttractDemo = new AttractDemoGameSequence(session);
    }

    /// <summary>The gameplay phases, which the pause's brightening and the time-up's white-out also run (<see cref="GameplayGamePhases"/>).</summary>
    internal GameplayGamePhases Gameplay { get; }

    /// <summary>The title sequence's phase.</summary>
    internal IGamePhase Title { get; }

    /// <summary>The file select sequence's phase.</summary>
    internal IGamePhase FileSelect { get; }

    /// <summary>The loading game sequence's phase.</summary>
    internal IGamePhase LoadingGame { get; }

    /// <summary>The gameplay fade in sequence's phase.</summary>
    internal IGamePhase GameplayFadeIn { get; }

    /// <summary>The main gameplay sequence's phase.</summary>
    internal IGamePhase MainGameplay { get; }

    /// <summary>The door transition sequence's phase.</summary>
    internal IGamePhase DoorTransition { get; }

    /// <summary>The pause sequence's phase.</summary>
    internal IGamePhase Pause { get; }

    /// <summary>The reserve tank refill sequence's phase.</summary>
    internal IGamePhase ReserveTankRefill { get; }

    /// <summary>The death sequence sequence's phase.</summary>
    internal IGamePhase DeathSequence { get; }

    /// <summary>The game over menu sequence's phase.</summary>
    internal IGamePhase GameOverMenu { get; }

    /// <summary>The intro cinematic sequence's phase.</summary>
    internal IGamePhase IntroCinematic { get; }

    /// <summary>The ceres departure sequence's phase.</summary>
    internal IGamePhase CeresDeparture { get; }

    /// <summary>The ceres destruction sequence's phase.</summary>
    internal IGamePhase CeresDestruction { get; }

    /// <summary>The time up sequence's phase.</summary>
    internal IGamePhase TimeUp { get; }

    /// <summary>The ending sequence's phase.</summary>
    internal IGamePhase Ending { get; }

    /// <summary>The attract demo sequence's phase.</summary>
    internal IGamePhase AttractDemo { get; }
}
