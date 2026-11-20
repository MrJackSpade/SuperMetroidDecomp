using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.GameOver;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The first update of the Continue-or-title menu after death (<see cref="SuperMetroidGameState.GameOverMenu"/>): it opens
/// the menu, which runs its first update at once.
/// </summary>
/// <param name="session">The frontend session the menu runs against.</param>
internal sealed class GameOverOpeningPhase(FrontendSession session) : IFrontendPhase
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
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        var menu = new GameOverMenuState(session.audio, session.Artwork.ScreenContent, session.Artwork.SpriteSheets,
            continueLoadsCeresArrival: session.saves.ReadSlot(session.SelectedSaveSlot)?.StartupRoute ==
                SaveStartupRoute.CeresElevatorArrival);
        var shown = new GameOverMenuGamePhase(session, menu);
        session.Enter(shown);
        shown.Step(controllerInput);
    }
}
