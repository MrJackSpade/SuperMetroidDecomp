using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.Title;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Power-on or a soft reset (<see cref="SuperMetroidGameState.Title"/>): the random generator is reseeded, every gameplay
/// owner and kept explored map is discarded, and the title opening is shown.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
internal sealed class TitleResetPhase(FrontendSession session) : IFrontendPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Title;

    /// <inheritdoc/>
    /// <remarks>Only power-on and a soft reset reseed; the title itself advances the generator once per update.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Reseed;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => "Reset";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    /// <remarks>Discards every owner the last game kept, then shows the title opening.</remarks>
    public void Step(ControllerButton controllerInput)
    {
        session.Demo.Reset();
        // The soft reset ultimately enters the opening cinematic and initializes
        // the cinematic phase to the opening.
        session.audio.Reset();
        // A soft reset discards every gameplay
        // owner. The reseeded random generator and the frame clock already live in the session.
        session.DiscardRuntime();
        // The reset forgets everything the game kept, the live and stored explored maps with it.
        StoredExploredMapLoads.ClearAll(session.storedExploredMaps);
        var title = new TitleSequenceState(
            session.audio,
            session.Artwork.ScreenContent.TitleGradient,
            session.Artwork.ScreenContent.TitleBackdrop,
            session.Artwork.SpriteSheets);
        session.ForgetQueuedAudio();
        session.ForgetSpacetimeRestart();
        session.Enter(new TitleScreenPhase(session, title));
        session.display.PublishMenu(title);
    }
}
