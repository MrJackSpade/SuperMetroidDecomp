using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The ending's credits (<see cref="SuperMetroidGameState.Ending"/>): Zebes's destruction, the results, the staff credits and
/// the completion reward.
/// </summary>
/// <param name="session">The frontend session the step runs against.</param>
/// <param name="credits">The ending and credits the step shows.</param>
internal sealed class EndingCreditsGamePhase(FrontendSession session, EndingCreditsState credits) : IInputFreePhase, IScreenPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Ending;

    /// <inheritdoc/>
    /// <remarks>The credits advance the generator once per update.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => $"Ending: {credits.Phase}";

    /// <inheritdoc/>
    /// <remarks>The credits' setup waits out frames of the display.</remarks>
    public bool IsWaitingOutDisplayFrame => credits.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    public IFrontendScreen Screen => credits;

    /// <inheritdoc/>
    /// <remarks>The credits run without gameplay or input.</remarks>
    public void Step()
    {
        EndingCreditsPhase previousEndingPhase = credits.Phase;
        credits.Step();
        if (previousEndingPhase != EndingCreditsPhase.SeeYouNextMission &&
            credits.Phase == EndingCreditsPhase.SeeYouNextMission)
        {
            session.saves.CompleteGame();
            session.RaiseSavesChanged();
        }
        session.display.PublishEnding(credits);
    }
}
