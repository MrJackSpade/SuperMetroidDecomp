using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>The Continue-or-title menu after death.</summary>
/// <param name="session">The frontend session the menu runs against.</param>
/// <param name="menu">The menu the step shows.</param>
internal sealed class GameOverMenuGamePhase(FrontendSession session, GameOverMenuState menu) : IFrontendPhase, IScreenPhase
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
    public bool IsWaitingOutDisplayFrame => menu.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    public IFrontendScreen Screen => menu;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        menu.Step(controllerInput);
        session.display.PublishMenu(menu);
        if (menu.Outcome == GameOverMenuOutcome.CeresArrival)
        {
            // Starting a game publishes the Ceres load and reloads the save slot in the same update;
            // the Ceres loader runs on the next update. Gameplay owners are
            // rebuilt from the save file, so only the live random number survives.
            session.ContinueSavedGame();
            session.ReleaseRuntime();
            session.Enter(GameDataLoadRoute.SelectedGame);
        }
        else if (menu.Outcome == GameOverMenuOutcome.Continue)
        {
            // The game-over menu's continue enters the area map after the fully black
            // frame. Keep that boundary separate from save reload.
            session.Enter(new AreaMapOpeningPhase(session));
            session.display.PublishBlack();
        }
        else if (menu.Outcome == GameOverMenuOutcome.Title)
        {
            // The game-over menu's exit step soft-resets only after its fade reaches black.
            session.Enter(new TitleResetPhase(session));
            session.display.PublishBlack();
        }
    }
}
