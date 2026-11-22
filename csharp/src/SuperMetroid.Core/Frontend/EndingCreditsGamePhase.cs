using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The ending's credits (<c>EndingStep.Credits</c>, #2554): Zebes's destruction, the results and the credits, shown on
/// <paramref name="credits"/>, which leaving Zebes made once the screen was black.
/// </summary>
/// <param name="session">The frontend session the credits run against.</param>
/// <param name="credits">The credits' screen.</param>
internal sealed partial class EndingCreditsGamePhase(FrontendSession session, EndingCreditsState credits) : IUnattendedPhase
{
    /// <summary>The credits' screen.</summary>
    internal EndingCreditsState Screen { get; } = Ensure.NotNull(credits);

    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Ending;

    /// <inheritdoc/>
    /// <remarks>The credits advance the generator once per update.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.Advance;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => $"Ending: {Screen.Phase}";

    /// <inheritdoc/>
    /// <remarks>The credits' setup waits out frames of the display.</remarks>
    public bool IsWaitingOutDisplayFrame => Screen.IsWaitingOutDisplayFrame;

    /// <inheritdoc/>
    /// <remarks>The credits run without gameplay; once they reach their final message the game is recorded complete.</remarks>
    public void Step()
    {
        EndingCreditsPhase previousEndingPhase = Screen.Phase;
        Screen.Step();
        if (!EndingCreditsPhaseGroups.FinalMessage.Contains(previousEndingPhase) &&
            EndingCreditsPhaseGroups.FinalMessage.Contains(Screen.Phase))
        {
            session.saves.CompleteGame();
            session.RaiseSavesChanged();
        }
        session.display.PublishEnding(Screen);
    }
}
