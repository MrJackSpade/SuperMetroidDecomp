using SuperMetroid.Core;
using SuperMetroid.Core.Frontend;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus.Death;

namespace SuperMetroid.AssetExtraction.Progression;

/// <summary>
/// The codec between the sequence and step a game is running and the number the cartridge's game-state word holds
/// (#1477), for the snapshot import and the checks that compare with a recording's game-state word. The game's sequences
/// and steps carry arbitrary values; the cartridge's numbering, which gives several steps of one sequence a number each,
/// lives only here.
/// </summary>
[EncodingOwner("game-state number")]
internal static class GameStateNumberCodec
{
    /// <summary>
    /// The steps in the cartridge's numbering: how a game enters each, and whether a running game is at it. Numbers
    /// three (a no-operation entry), 28 (an unreachable entry) and 29 (a debug-build menu) name no step the game has.
    /// Samus's death animation and the Ceres departure are numbered by the step their own sequence state is on; entering
    /// them enters the frontend's step, and the import leaves that state as the game left it.
    /// </summary>
    private static readonly Numbered?[] Numbers =
    [
        new(session => session.Enter(TitleStep.Starting), session => session.IsAt(TitleStep.Starting)),
        new(session => session.Enter(TitleStep.Showing), session => session.IsAt(TitleStep.Showing)),
        new(session => session.Enter(FileSelectStep.Options), session => session.IsAt(FileSelectStep.Options)),
        null,
        new(session => session.Enter(FileSelectStep.SaveSlots), session => session.IsAt(FileSelectStep.SaveSlots)),
        new(session => session.Enter(FileSelectStep.AreaMap), session => session.IsAt(FileSelectStep.AreaMap)),
        new(EnterGameDataLoad, session => session.GameState == SuperMetroidGameState.LoadingGame &&
            session.LoadingRoute != GameDataLoadRoute.SelectedGame),
        new(session => session.Enter(SuperMetroidGameState.GameplayFadeIn), session => session.GameState == SuperMetroidGameState.GameplayFadeIn),
        new(session => session.Enter(SuperMetroidGameState.MainGameplay), session => session.GameState == SuperMetroidGameState.MainGameplay),
        new(session => session.Enter(DoorTransitionStep.EnteringDoor), session => session.IsAt(DoorTransitionStep.EnteringDoor)),
        new(session => session.Enter(DoorTransitionStep.BeginningTransition), session => session.IsAt(DoorTransitionStep.BeginningTransition)),
        new(session => session.Enter(DoorTransitionStep.Transitioning), session => session.IsAt(DoorTransitionStep.Transitioning)),
        new(session => session.Enter(PauseStep.DarkeningGameplay), session => session.IsAt(PauseStep.DarkeningGameplay)),
        new(session => session.Enter(PauseStep.PreparingPauseScreen), session => session.IsAt(PauseStep.PreparingPauseScreen)),
        new(session => session.Enter(PauseStep.FadingIn), session => session.IsAt(PauseStep.FadingIn)),
        new(session => session.Enter(PauseStep.Menu), session => session.IsAt(PauseStep.Menu)),
        new(session => session.Enter(PauseStep.FadingOut), session => session.IsAt(PauseStep.FadingOut)),
        new(session => session.Enter(PauseStep.RestoringGameplay), session => session.IsAt(PauseStep.RestoringGameplay)),
        new(session => session.Enter(PauseStep.BrighteningGameplay), session => session.IsAt(PauseStep.BrighteningGameplay)),
        new(session => session.Enter(DeathSequenceStep.FinalGameplayPass), session => session.IsAt(DeathSequenceStep.FinalGameplayPass)),
        new(session => session.Enter(DeathSequenceStep.BlackingOutSurroundings), session => session.IsAt(DeathSequenceStep.BlackingOutSurroundings)),
        new(session => session.Enter(DeathSequenceStep.WaitingForMusic), session => session.IsAt(DeathSequenceStep.WaitingForMusic)),
        new(session => session.Enter(DeathSequenceStep.SamusDying), session => IsDying(session, new SamusDeathSequencePhase.Begun.PreFlashing())),
        new(session => session.Enter(DeathSequenceStep.SamusDying), session => IsDying(session, new SamusDeathSequencePhase.Begun.Flashing())),
        new(session => session.Enter(DeathSequenceStep.SamusDying), session => IsDying(session, new SamusDeathSequencePhase.Begun.SuitExplosion())),
        new(session => session.Enter(DeathSequenceStep.FinalBlackOut), session => session.IsAt(DeathSequenceStep.FinalBlackOut)),
        new(session => session.Enter(SuperMetroidGameState.GameOverMenu), session => session.GameState == SuperMetroidGameState.GameOverMenu),
        new(session => session.Enter(SuperMetroidGameState.ReserveTankRefill), session => session.GameState == SuperMetroidGameState.ReserveTankRefill),
        null,
        null,
        new(session => session.Enter(SuperMetroidGameState.IntroCinematic), session => session.GameState == SuperMetroidGameState.IntroCinematic),
        new(session => session.Enter(GameDataLoadRoute.SelectedGame), session => session.GameState == SuperMetroidGameState.LoadingGame &&
            session.LoadingRoute == GameDataLoadRoute.SelectedGame),
        new(session => session.Enter(SuperMetroidGameState.CeresDeparture), session => session.IsAt(CeresDeparturePhase.HoldingOnElevator)),
        new(session => session.Enter(SuperMetroidGameState.CeresDeparture), session => session.IsAt(CeresDeparturePhase.FadingToBlack)),
        new(session => session.BeginCeresDestruction(CeresDestructionOutcome.SamusEscaped), session => session.IsAt(CeresDestructionOutcome.SamusEscaped)),
        new(session => session.Enter(TimeUpStep.WhiteningOut), session => session.IsAt(TimeUpStep.WhiteningOut)),
        new(session => session.Enter(TimeUpStep.FadingToBlack), session => session.IsAt(TimeUpStep.FadingToBlack)),
        new(session => session.BeginCeresDestruction(CeresDestructionOutcome.SamusAboard), session => session.IsAt(CeresDestructionOutcome.SamusAboard)),
        new(session => session.Enter(EndingStep.LeavingZebes), session => session.IsAt(EndingStep.LeavingZebes)),
        new(session => session.Enter(EndingStep.Credits), session => session.IsAt(EndingStep.Credits)),
        new(session => session.Enter(AttractDemoStep.Loading), session => session.IsAt(AttractDemoStep.Loading)),
        new(session => session.Enter(AttractDemoStep.Starting), session => session.IsAt(AttractDemoStep.Starting)),
        new(session => session.Enter(AttractDemoStep.Playing), session => session.IsAt(AttractDemoStep.Playing)),
        new(session => session.Enter(AttractDemoStep.Unloading), session => session.IsAt(AttractDemoStep.Unloading)),
        new(session => session.Enter(AttractDemoStep.Routing), session => session.IsAt(AttractDemoStep.Routing)),
    ];

    /// <summary>Enters <paramref name="game"/> at the step the cartridge numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">The number names no step the game has, or one the import cannot enter.</exception>
    public static void Enter(SuperMetroidGame game, int number)
    {
        Ensure.NotNull(game);
        Of(number).Enter(game.Session);
    }

    /// <summary>Whether <paramref name="game"/> is at the step the cartridge numbers <paramref name="number"/>.</summary>
    /// <exception cref="InvalidDataException">The number names no step the game has.</exception>
    public static bool IsAt(SuperMetroidGame game, int number)
    {
        Ensure.NotNull(game);
        return Of(number).IsRunning(game.Session);
    }

    /// <summary>The cartridge's number for the step <paramref name="game"/> is at.</summary>
    /// <exception cref="InvalidOperationException">The game is at a step the cartridge numbers no way.</exception>
    public static int Encode(SuperMetroidGame game)
    {
        Ensure.NotNull(game);
        for (int number = 0; number < Numbers.Length; number++)
            if (Numbers[number] is { } numbered && numbered.IsRunning(game.Session))
                return number;
        throw new InvalidOperationException($"The game's {game.GameState} sequence is at a step the cartridge numbers no way.");
    }

    /// <summary>The numbered step <paramref name="number"/>.</summary>
    private static Numbered Of(int number) =>
        number >= 0 && number < Numbers.Length && Numbers[number] is { } numbered
            ? numbered
            : throw new InvalidDataException($"Game-state number {number} names no step the game has.");

    /// <summary>
    /// The cartridge's game-data load holds a continued save's checkpoint or the Zebes landing in other memory; the import
    /// does not read which.
    /// </summary>
    private static void EnterGameDataLoad(FrontendSession session) =>
        throw new InvalidDataException("A snapshot taken during the game-data load does not say which load it runs.");

    /// <summary>Whether the session runs Samus's death animation while her death sequence is at <paramref name="phase"/>.</summary>
    private static bool IsDying(FrontendSession session, SamusDeathSequencePhase phase) =>
        session.IsAt(DeathSequenceStep.SamusDying) && session.Runtime?.Player.Samus?.DeathSequence.Phase == phase;

    /// <summary>One numbered step: how a game enters it, and whether a running game is at it.</summary>
    /// <param name="Enter">Enters the step.</param>
    /// <param name="IsRunning">Whether a session is at the step.</param>
    private sealed record Numbered(Action<FrontendSession> Enter, Func<FrontendSession, bool> IsRunning);
}
