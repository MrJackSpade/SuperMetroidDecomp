using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The Continue-or-title menu after death.</summary>
/// <param name="session">The frontend session the menu runs against.</param>
internal sealed class GameOverMenuGamePhase(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.GameOverMenu;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => "Game over";

    /// <inheritdoc/>
    /// <remarks>The menu's first two steps each wait out a frame of the display.</remarks>
    public bool IsWaitingOutDisplayFrame => session.ScreenAs<GameOverMenuState>()?.IsWaitingOutDisplayFrame == true;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        GameOverMenuState gameOver = session.ScreenAs<GameOverMenuState>() ?? session.Show(new GameOverMenuState(session.audio,
            session.Artwork.ScreenContent, session.Artwork.SpriteSheets, continueLoadsCeresArrival: session.saves.ReadSlot(session.SelectedSaveSlot)?.StartupRoute ==
                SaveLoadingGameState.CeresElevatorArrival));
        gameOver.Step(controllerInput);
        session.display.PublishMenu(gameOver);
        if (gameOver.Outcome == GameOverMenuOutcome.CeresArrival)
        {
            // Starting a game publishes the Ceres load and reloads the save slot in the same update;
            // the Ceres loader runs on the next update. Gameplay owners are
            // rebuilt from the save file, so only the live random number survives.
            session.Close<GameOverMenuState>();
            session.ContinueSavedGame();
            session.ReleaseRuntime();
            session.Enter(GameDataLoadRoute.SelectedGame);
        }
        else if (gameOver.Outcome == GameOverMenuOutcome.Continue)
        {
            // The game-over menu's continue enters the area map after the fully black
            // frame. Keep that boundary separate from save reload.
            session.Close<GameOverMenuState>();
            session.Enter(FileSelectStep.AreaMap);
            session.display.PublishBlack();
        }
        else if (gameOver.Outcome == GameOverMenuOutcome.Title)
        {
            // The game-over menu's exit step soft-resets only after its fade reaches black.
            session.Close<GameOverMenuState>();
            session.Enter(TitleStep.Starting);
            session.display.PublishBlack();
        }
    }
}
