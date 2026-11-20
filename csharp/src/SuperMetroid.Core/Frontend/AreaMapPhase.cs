using SuperMetroid.Core.Input;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// A saved game's area map (<see cref="SuperMetroidGameState.FileSelect"/>), where the player chooses the save station to
/// continue from. The map opens only for a save outside Ceres that does not resume at its destruction.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
/// <param name="map">The map the step shows.</param>
/// <param name="slot">The saved game whose map it is.</param>
internal sealed class AreaMapPhase(FrontendSession session, FileSelectMapMenuState map, SuperMetroidSaveSlot slot)
    : IFrontendPhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.FileSelect;

    /// <inheritdoc/>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => map.Phase.ToString();

    /// <inheritdoc/>
    /// <remarks>
    /// The map's final update latched its load request, blanked the display and waits out one frame; the next update
    /// continues that step.
    /// </remarks>
    public bool IsWaitingOutDisplayFrame => map.Outcome == FileSelectMapOutcome.Load;

    /// <inheritdoc/>
    public IFrontendScreen Screen => map;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        if (map.Outcome == FileSelectMapOutcome.Load)
        {
            // The previous map update blanked the display and waited out this frame;
            // continuing, it enters the game load. The map runs no more.
            session.Enter(GameDataLoadRoute.SavedCheckpoint);
            session.display.PublishBlack();
            return;
        }
        map.Step(controllerInput);
        SavedGameStarts.PublishAreaMap(session, map, slot);
    }
}
