using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The ending's first step (<see cref="EndingStep.LeavingZebes"/>): the gunship leaves Zebes while the screen darkens; the
/// credits follow, Zebes's destruction, the results and the credits themselves (<see cref="EndingCreditsGamePhase"/>, #2554).
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed class EndingGameSequence(FrontendSession session) : IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Ending;

    /// <summary>How each step of the ending advances the random generator.</summary>
    private static IdentityTable<EndingStep, PhaseRandomStep> RandomSteps => field ??= new(new Dictionary<EndingStep, PhaseRandomStep>
    {
        [EndingStep.LeavingZebes] = PhaseRandomStep.RuntimeOwned,
        [EndingStep.Credits] = PhaseRandomStep.Advance,
    });

    /// <inheritdoc/>
<<<<<<< ours
    /// <remarks>Leaving Zebes runs gameplay, which advances the generator itself.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;
||||||| base
    /// <remarks>Leaving Zebes runs gameplay, which advances the generator itself; the credits advance it once per update.</remarks>
    public PhaseRandomStep RandomStep => session.EndingStep switch
    {
        EndingStep.LeavingZebes => PhaseRandomStep.RuntimeOwned,
        EndingStep.Credits => PhaseRandomStep.Advance,
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingStep)} {session.EndingStep}."),
    };
=======
    /// <remarks>Leaving Zebes runs gameplay, which advances the generator itself; the credits advance it once per update.</remarks>
    public PhaseRandomStep RandomStep => RandomSteps[session.EndingStep];
>>>>>>> theirs

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

<<<<<<< ours
    /// <inheritdoc/>
    public string Name => $"Zebes escape fade-out ({session.EscapeFade.Level})";
||||||| base
    /// <inheritdoc/>
    public string Name => session.EndingStep switch
    {
        EndingStep.LeavingZebes => $"Zebes escape fade-out ({session.EscapeFade.Level})",
        EndingStep.Credits => $"Ending: {session.ScreenAs<EndingCreditsState>()?.Phase}",
        _ => throw new InvalidOperationException($"Undefined {nameof(EndingStep)} {session.EndingStep}."),
    };
=======
    /// <summary>The diagnostic name of each step of the ending, read from the session it runs against.</summary>
    private static IdentityTable<EndingStep, Func<FrontendSession, string>> Names => field ??= new(new Dictionary<EndingStep, Func<FrontendSession, string>>
    {
        [EndingStep.LeavingZebes] = static running => $"Zebes escape fade-out ({running.EscapeFade.Level})",
        [EndingStep.Credits] = static running => $"Ending: {running.ScreenAs<EndingCreditsState>()?.Phase}",
    });

    /// <inheritdoc/>
    public string Name => Names[session.EndingStep](session);
>>>>>>> theirs

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <summary>What <c>Step</c> does for each <see cref="EndingStep"/> member, each member's own arm.</summary>
    private IdentityTable<EndingStep, Action<ControllerButton>> StepForEndingStep => field ??= new(new Dictionary<EndingStep, Action<ControllerButton>>
    {
        [EndingStep.LeavingZebes] = controllerInput => LeaveZebes(controllerInput),
        [EndingStep.Credits] = controllerInput => RunCredits(),
    });

    /// <inheritdoc/>
<<<<<<< ours
    /// <remarks>Samus leaves Zebes in the loaded room.</remarks>
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => LeaveZebes(controllerInput, live);
||||||| base
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
=======
    public void Step(ControllerButton controllerInput)
    {
        StepForEndingStep[session.EndingStep](controllerInput);
    }
>>>>>>> theirs

    /// <summary>One update of the departing gunship's gameplay while the screen darkens, which starts the credits once black.</summary>
    private void LeaveZebes(ControllerButton controllerInput, LoadedGameplay live)
    {
        // Leaving Zebes runs the complete main-gameplay update before
        // the fade-out. The fleeing gunship, room animations, and sound publishers
        // therefore continue behind every darkening frame, and each counts the play time (#2517).
        live.Runtime.StepFrame(controllerInput, new RuntimeFrameRequest { Owner = GameplayUpdateOwner.LeavingZebes }, live.Room);
        session.display.PublishGameplay(live);
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
            SamusState endingSamus = live.Runtime.Player.Samus;
            EndingCreditsState credits = session.Show(new EndingCreditsState(session.audio, new EndingCreditsSetup { GameTimeHours = session.Assists.EndingClearTimeMinutes is { } endingMinutes ? endingMinutes / 60 : live.Runtime.GameTime.Hours, GameTimeMinutes = session.Assists.EndingClearTimeMinutes is { } totalMinutes ? totalMinutes % 60 : live.Runtime.GameTime.Minutes, Inventory = new EndingInventorySnapshot(
                    endingSamus.MaxHealth,
                    endingSamus.MaxReserveEnergy,
                    endingSamus.MaxMissiles,
                    endingSamus.MaxSuperMissiles,
                    endingSamus.MaxPowerBombs,
                    endingSamus.CollectedItems,
                    endingSamus.CollectedBeams), Language = live.Runtime.Language, CrittersEscaped = live.Runtime.System.HasEvent(StoryEvent.CrittersEscaped) },
                session.Artwork.ScreenContent.EndingText, session.Artwork.EndingBackdrops, session.Artwork.SpriteSheets));
            session.EnterCredits(credits);
            session.display.PublishBlack();
        }
    }
}
