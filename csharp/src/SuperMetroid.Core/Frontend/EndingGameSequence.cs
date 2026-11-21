using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend;

/// <summary>The ending: the gunship leaves Zebes while the screen darkens, then Zebes's destruction, the results and the credits.</summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed partial class EndingGameSequence(FrontendSession session) : IGamePhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Ending;

    /// <inheritdoc/>
    /// <remarks>Leaving Zebes runs gameplay, which advances the generator itself; the credits advance it once per update.</remarks>
    public PhaseRandomStep RandomStep => session.EndingStep switch
    {
        EndingStep.LeavingZebes => PhaseRandomStep.RuntimeOwned,
        EndingStep.Credits => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingStep)} {session.EndingStep}."),
    };

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => session.EndingStep switch
    {
        EndingStep.LeavingZebes => $"Zebes escape fade-out ({session.EscapeFade.Level})",
        EndingStep.Credits => $"Ending: {session.ScreenAs<EndingCreditsState>()?.Phase}",
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingStep)} {session.EndingStep}."),
    };

    /// <inheritdoc/>
    /// <remarks>The credits' setup waits out frames of the display.</remarks>
    public bool IsWaitingOutDisplayFrame =>
        session.EndingStep == EndingStep.Credits && session.ScreenAs<EndingCreditsState>()?.IsWaitingOutDisplayFrame == true;

    /// <inheritdoc/>
    public void Step(ControllerButton controllerInput)
    {
        switch (session.EndingStep)
        {
            case EndingStep.LeavingZebes:
                LeaveZebes(controllerInput);
                break;
            case EndingStep.Credits:
                RunCredits();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingStep)} {session.EndingStep}.");
        }
    }

    /// <summary>One update of the departing gunship's gameplay while the screen darkens, which starts the credits once black.</summary>
    private void LeaveZebes(ControllerButton controllerInput)
    {
        // Leaving Zebes runs the complete main-gameplay update before
        // the fade-out. The fleeing gunship, room animations, and sound publishers
        // therefore continue behind every darkening frame, and each counts the play time (#2517).
        session.GameplayRuntime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.LeavingZebes });
        session.display.PublishGameplay(session.GameplayRuntime);
        // The gunship's lift-off left the screen fade no wait between its steps, so the
        // brightness falls one step on every update.
        bool black = session.EscapeFade.Darken();
        session.SetDisplayBrightness(session.EscapeFade.Level);
        if (black)
        {
            // The liftoff resets display ownership, stops the escape timer/music,
            // cancels all three SFX libraries, and enters the credits. The credits
            // perform their own setup on the following update.
            session.audio.QueueMusic(MusicRequest.Stop);
            session.audio.QueueCancelSoundEffects(SoundRequestPriority.Essential);
            SamusState endingSamus = session.GameplayRuntime.Player.IntroducedSamus;
            session.Show(new EndingCreditsState(session.audio, new EndingCreditsSetup { GameTimeHours = session.Assists.EndingClearTimeMinutes is { } endingMinutes ? endingMinutes / 60 : session.GameplayRuntime.GameTime.Hours, GameTimeMinutes = session.Assists.EndingClearTimeMinutes is { } totalMinutes ? totalMinutes % 60 : session.GameplayRuntime.GameTime.Minutes, Inventory = new EndingInventorySnapshot(
                    endingSamus.MaxHealth,
                    endingSamus.MaxReserveEnergy,
                    endingSamus.MaxMissiles,
                    endingSamus.MaxSuperMissiles,
                    endingSamus.MaxPowerBombs,
                    endingSamus.CollectedItems,
                    endingSamus.CollectedBeams), Language = session.GameplayRuntime.Language, CrittersEscaped = session.GameplayRuntime.System.HasEvent(StoryEvent.CrittersEscaped) },
                session.Artwork.ScreenContent.EndingText, session.Artwork.EndingBackdrops, session.Artwork.SpriteSheets));
            session.Enter(EndingStep.Credits);
            session.display.PublishBlack();
        }
    }

    /// <summary>One update of the credits, which records the completed game once they reach their final message.</summary>
    private void RunCredits()
    {
        EndingCreditsPhase previousEndingPhase = session.ActiveEndingCredits.Phase;
        session.ActiveEndingCredits.Step();
        if (!EndingCreditsPhaseGroups.FinalMessage.Contains(previousEndingPhase) &&
            EndingCreditsPhaseGroups.FinalMessage.Contains(session.ActiveEndingCredits.Phase))
        {
            session.saves.CompleteGame();
            session.RaiseSavesChanged();
        }
        session.display.PublishEnding(session.ActiveEndingCredits);
    }
}
