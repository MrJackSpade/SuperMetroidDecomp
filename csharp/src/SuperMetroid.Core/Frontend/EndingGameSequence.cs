using SuperMetroid.Core.Input;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Frontend.Ending;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// The ending's first step, leaving Zebes: the gunship leaves Zebes while the screen darkens; the
/// credits follow, Zebes's destruction, the results and the credits themselves (<see cref="EndingCreditsGamePhase"/>, #2554).
/// </summary>
/// <param name="session">The frontend session the sequence runs against.</param>
internal sealed partial class EndingGameSequence(FrontendSession session) : IGameplayPhase
{
    /// <inheritdoc/>
    public SuperMetroidGameState State => SuperMetroidGameState.Ending;

    /// <inheritdoc/>
    /// <remarks>Leaving Zebes runs gameplay, which advances the generator itself.</remarks>
    public PhaseRandomStep RandomStep => PhaseRandomStep.RuntimeOwned;

    /// <inheritdoc/>
    public GamePhaseAudio Audio => GamePhaseAudio.Ordinary;

    /// <inheritdoc/>
    public string Name => $"Zebes escape fade-out ({session.EscapeFade.Level})";

    /// <inheritdoc/>
    public bool IsWaitingOutDisplayFrame => false;

    /// <inheritdoc/>
    /// <remarks>Samus leaves Zebes in the loaded room.</remarks>
    public void Step(ControllerButton controllerInput, LoadedGameplay live) => LeaveZebes(controllerInput, live);

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
