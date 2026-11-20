using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending owner, from the escape's end through the credits and
/// post-credit result screen.
/// </summary>
/// <remarks>
/// This is deliberately a cinematic state machine rather than a video or an invented
/// slideshow. Backgrounds, pictures, colours, sprite frames, and animation durations
/// preserve the game's behavior; editable text and fonts come from installed assets. Each named phase is one stage of
/// the ending, and the scene passes from one to the next on the update the game does.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingCreditsState : IFrontendScreen, IEndingActorScene
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
            InstalledArtwork.BindEndingText(artwork.ScreenContent.EndingText);
        if (!ReferenceEquals(previous.EndingBackdrops, artwork.EndingBackdrops))
            InstalledArtwork.BindBackdrops(artwork.EndingBackdrops);
        if (!ReferenceEquals(previous.SpriteSheets, artwork.SpriteSheets))
            InstalledArtwork.BindSpriteSheets(artwork.SpriteSheets);
    }
    /// <summary>Queues the authored ending music transitions.</summary>
    private readonly AudioQueues audio;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    internal readonly EndingScreen screen = new();
    /// <summary>The ending's actors in their running order, in which they step and draw (<see cref="EndingCastOrderDefinitions"/>).</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>
    /// The ending's updates so far, counted at the end of each. Only its parity and quarter phase are read: the turning
    /// floor's every-other-update turn and the planet's every-fourth-update turn.
    /// </summary>
    private int cinematicFrame;

    /// <summary>Current cinematic phase, including terminal display states; only the ending's own updates change it (#2541).</summary>
    public EndingCreditsPhase Phase { get; private set; }

    /// <summary>
    /// What the ending shows with its phase, which owns that scene's own actors (#2541): the cast, the staff credits, the
    /// reward's reveal, gesture, jump, shot and white flash, or the final logo. The ending changes it only with its phase.
    /// </summary>
    internal EndingScene Scene { get; private set; } = new EndingCastScene(EndingCreditsPhase.SetupEscapeFromZebes);

    /// <summary>
    /// The wait of the phase running now (#2541), begun as the phase is entered: the setup's waits, the explosion's hold and
    /// whiteout, and the post-credits scenes' holds, the waiting backdrop's age read from it.
    /// </summary>
    internal UpdateCountdown PhaseHold { get; private set; } = UpdateCountdown.Lasting(EndingSequenceDefinitions.SetupFrameWaits);

    /// <summary>The turning floor's zoom, which the escape's clouds read.</summary>
    public FloorZoom FloorZoom => Stage.Floor.Zoom;

    /// <summary>Whether the exploding planet's actor is still shown.</summary>
    public bool ShowsExplodingPlanet => Stage.ShowsExplodingPlanet;

    /// <summary>What every ending scene draws on: brightness, the turning-floor view, colours, the shared sprites and stars, and the artwork they share.</summary>
    internal EndingStage Stage { get; }

    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the white hold, the stars and the fade.</summary>
    internal EndingFlyaway Flyaway { get; }

    /// <summary>Zebes exploding in the ending: its setup and the colour crossfade into it.</summary>
    internal EndingZebesExplosion Explosion { get; }

    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, sprite and colour assets.</summary>
    internal EndingArtwork InstalledArtwork { get; }

    /// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
    internal EndingEscapeScenes EscapeScenes { get; }

    /// <summary>The staff credits and what follows them: the shooting stars, the reward's copyright and colours, the white flash and the post-credits logo.</summary>
    internal EndingCreditsRoll CreditsRoll { get; }

    /// <summary>Initializes the ending and credits from the saved clear-time and ending setup snapshot and the installed artwork the ending draws.</summary>
    /// <param name="audio">The queues the ending's music and sounds go to.</param>
    /// <param name="setup">The game-completion facts the ending presents.</param>
    /// <param name="endingText">The installed text the ending types.</param>
    /// <param name="backdrops">The installed scene backdrops (#1544).</param>
    /// <param name="spriteSheets">The installed sprite sheets the ending's actors and backgrounds draw from.</param>
    public EndingCreditsState(AudioQueues audio, EndingCreditsSetup setup, EndingTypewriterText endingText, EndingBackdrops backdrops,
        InstalledSpriteSheets spriteSheets)
    {
        this.audio = Ensure.NotNull(audio);
        Ensure.NotNull(setup);
        Stage = new EndingStage(setup.GameTimeHours, screen, sprites, backdrops);
        InstalledArtwork = new EndingArtwork(Stage, endingText, spriteSheets);
        Flyaway = new EndingFlyaway(setup.CrittersEscaped, screen, Stage);
        Explosion = new EndingZebesExplosion(screen, sprites, Stage);
        EscapeScenes = new EndingEscapeScenes(audio, setup.GameTimeHours, setup.GameTimeMinutes, screen, sprites, Stage, InstalledArtwork);
        CreditsRoll = new EndingCreditsRoll(setup.Inventory, setup.JapaneseText, screen, sprites, Stage, InstalledArtwork);
        Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }
    /// <summary>
    /// True while the ending's setup waits out frames of the display: the next update finishes that wait instead of
    /// starting a new update of the ending (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool IsWaitingOutDisplayFrame =>
        Phase == EndingCreditsPhase.SetupEscapeFromZebes && PhaseHold.Age > 0;

    /// <summary>Enters <paramref name="phase"/>, which shows the scene already showing.</summary>
    private void Enter(EndingCreditsPhase phase) => Phase = phase;

    /// <summary>Enters the cast's <paramref name="phase"/>, which shows the cast in that phase.</summary>
    private void Enter(EndingCastPhase phase) => Show(phase, new EndingCastScene(phase));

    /// <summary>Enters the cast's <paramref name="phase"/>, which shows the cast in that phase and waits <paramref name="holdUpdates"/> updates.</summary>
    private void Enter(EndingCastPhase phase, int holdUpdates) => Show(phase, new EndingCastScene(phase), holdUpdates);

    /// <summary>Enters <paramref name="phase"/>, which waits <paramref name="holdUpdates"/> updates, from the next update on.</summary>
    private void Enter(EndingCreditsPhase phase, int holdUpdates)
    {
        PhaseHold = UpdateCountdown.Lasting(holdUpdates);
        Phase = phase;
    }

    /// <summary>Enters <paramref name="phase"/>, which shows <paramref name="scene"/>.</summary>
    private void Show(EndingCreditsPhase phase, EndingScene scene)
    {
        Scene = Ensure.NotNull(scene);
        Phase = phase;
    }

    /// <summary>Enters <paramref name="phase"/>, which shows <paramref name="scene"/> and waits <paramref name="holdUpdates"/> updates.</summary>
    private void Show(EndingCreditsPhase phase, EndingScene scene, int holdUpdates)
    {
        PhaseHold = UpdateCountdown.Lasting(holdUpdates);
        Show(phase, scene);
    }

    /// <summary>Runs one update of the ending and credits.</summary>
    /// <exception cref="InvalidOperationException">The phase is not one the scene showing runs.</exception>
    public void Step()
    {
        if (!Scene.Step(this))
            return;

        // The cinematic runs its colour effects after actors; the ending's colours show the frame each shows (#1512, #1597).
        // The screen binds no audio requests, so an effect that made a sound would fail loudly (#2057).
        Stage.ColourEffects.StepScreen();
        // The ending follows cinematic actors even while a text-only screen hides sprites.
        // The credits' end enables the already-started colour effects when the credits finish.
        if (EndingCreditsPhaseGroups.PostCredits.Contains(Phase))
            CreditsRoll.ShootingStars.Step();
        cinematicFrame++;
    }

    /// <summary>Runs one update of the reward's gesture, which hands over to its jump as the gesture ends.</summary>
    internal void StepGesture(EndingRewardGestureScene scene)
    {
        RequirePhase(EndingCreditsPhase.PostCreditsGesture);
        if (scene.Gesture.Step())
            Show(EndingCreditsPhase.PostCreditsJump, new EndingRewardJumpScene(new EndingRewardJump(Stage.EndingReward)));
    }

    /// <summary>Runs one update of the reward's jump, which hands over to its shot, with the shot's sound, as Samus lands.</summary>
    internal void StepJump(EndingRewardJumpScene scene)
    {
        RequirePhase(EndingCreditsPhase.PostCreditsJump);
        EndingRewardJump jump = scene.Jump;
        if (jump.Step())
        {
            Show(EndingCreditsPhase.PostCreditsShot, new EndingPostShotScene(jump, new EndingPostShot()));
            audio.QueueSound(EndingPostShotDefinitions.ShotSound, EndingPostShotDefinitions.ShotSoundPriority);
        }
    }

    /// <summary>Runs one update of the reward's shot over the landed Samus, which hands over to the white flash.</summary>
    internal void StepPostShot(EndingPostShotScene scene)
    {
        RequirePhase(EndingCreditsPhase.PostCreditsShot);
        EndingRewardJump jump = scene.Jump;
        EndingPostShot shot = scene.Shot;
        // The cinematic phase runs before actors; colour fades and queued
        // picture replacements therefore precede the next sprite cue.
        shot.Step(screen);
        jump.Step();
        if (shot.ReadyForWhiteFlash)
            Show(EndingCreditsPhase.PostCreditsWhiteFlash, new EndingWhiteFlashScene(jump, CreditsRoll.BeginPostCreditsWhiteFlash()),
                EndingPostShotDefinitions.FadeSteps);
    }

    /// <summary>Runs one update of the white flash over the landed Samus, which hands over to the final logo as its hold ends.</summary>
    internal void StepWhiteFlash(EndingWhiteFlashScene scene)
    {
        RequirePhase(EndingCreditsPhase.PostCreditsWhiteFlash);
        scene.Flash.Fade();
        scene.Jump.Step();
        if (PhaseHold.Count())
            Show(EndingCreditsPhase.PostCreditsLogo, new EndingLogoScene(CreditsRoll.FinishPostCreditsWhiteFlash()));
    }

    /// <summary>Fails unless the ending runs <paramref name="phase"/>, a phase of the scene showing.</summary>
    /// <exception cref="InvalidOperationException">The ending runs another phase.</exception>
    private void RequirePhase(EndingCreditsPhase phase)
    {
        if (Phase != phase)
            throw new InvalidOperationException($"The {Scene.GetType().Name} runs no {Phase} phase.");
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.SetupEscapeFromZebes"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastSetupEscapeFromZebes()
    {
        if (!PhaseHold.IsOver)
        {
            PhaseHold.Count();
            return false;
        }
        EscapeScenes.SetupSideCloudEscape();
        Enter(EndingCreditsPhase.WaitForEscapeMusic);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.WaitForEscapeMusic"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastWaitForEscapeMusic()
    {
        if (!audio.HasQueuedMusic)
        {
            Stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.Lava);
            Stage.ColourEffects.StartScreen(EndingColourEffectDefinitions.Crust);
            Stage.Fade.Show(0);
            Stage.StartFastFade();
            Enter(EndingCreditsPhase.FadeInSideCloudEscape);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeInSideCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeInSideCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.SideCloudEscapeZoomStep);
        StepActors();
        if (Stage.Fade.Brighten())
            Enter(EndingCreditsPhase.SideCloudEscape);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.SideCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastSideCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.SideCloudEscapeZoomStep);
        StepActors();
        if (Stage.Floor.Zoom >= EndingMotionDefinitions.EscapeEndZoom)
        {
            Stage.StartFastFade();
            Enter(EndingCreditsPhase.FadeOutSideCloudEscape);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeOutSideCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeOutSideCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.SideCloudEscapeZoomStep);
        StepActors();
        if (Stage.Fade.Darken())
        {
            EscapeScenes.SetupTopAndBottomCloudEscape();
            Enter(EndingCreditsPhase.FadeInTopAndBottomCloudEscape);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeInTopAndBottomCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeInTopAndBottomCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.TopAndBottomCloudEscapeZoomStep);
        StepActors();
        if (Stage.Fade.Brighten())
            Enter(EndingCreditsPhase.TopAndBottomCloudEscape);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.TopAndBottomCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastTopAndBottomCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.TopAndBottomCloudEscapeZoomStep);
        StepActors();
        if (Stage.Floor.Zoom >= EndingMotionDefinitions.EscapeEndZoom)
        {
            Stage.StartFastFade();
            Enter(EndingCreditsPhase.FadeOutTopAndBottomCloudEscape);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeOutTopAndBottomCloudEscape"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeOutTopAndBottomCloudEscape()
    {
        StepAtmosphericTransform(EndingMotionDefinitions.TopAndBottomCloudEscapeZoomStep);
        StepActors();
        if (Stage.Fade.Darken())
        {
            Explosion.SetupZebesExplosion();
            Enter(EndingCreditsPhase.FadeInZebesExplosion);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeInZebesExplosion"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeInZebesExplosion()
    {
        StepExplosionCrossfade();
        StepActors();
        if (Stage.Fade.Brighten())
            Enter(EndingCreditsPhase.ZebesExplosionCrossfade);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.ZebesExplosionCrossfade"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastZebesExplosionCrossfade()
    {
        StepExplosionCrossfade();
        StepActors();
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.ZebesExplosionHold"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastZebesExplosionHold()
    {
        StepActors();
        if (PhaseHold.Count())
            Enter(EndingCreditsPhase.ZebesExplosionAnimation);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.ZebesExplosionAnimation"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastZebesExplosionAnimation()
    {
        // The explosion actor's animation runs the finale: its ending cue whitens the screen and starts the wait
        // for the planet escape's music (EndingSequenceDefinitions.Timing.PlanetEscapeMusicWaitFrames).
        StepActors();
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.WaitForPlanetEscapeMusic"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastWaitForPlanetEscapeMusic()
    {
        StepActors();
        if (PhaseHold.Count())
        {
            audio.QueueMusic(MusicRequest.Stop);
            audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Credits));
            audio.ScheduleMusic(
                MusicRequest.SelectTrack(EndingSequenceDefinitions.Music.CreditsSong),
                EndingSequenceDefinitions.Music.CreditsSongDelay);
            Enter(EndingCreditsPhase.WaitForPlanetEscapeMusicQueue);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.WaitForPlanetEscapeMusicQueue"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastWaitForPlanetEscapeMusicQueue()
    {
        StepActors();
        if (!audio.HasQueuedMusic)
        {
            Flyaway.SetupPlanetEscape();
            Enter(EndingCreditsPhase.PlanetEscapeFast);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PlanetEscapeFast"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPlanetEscapeFast()
    {
        StepActors();
        if (Flyaway.StepPlanetEscapeFast())
            Enter(EndingCreditsPhase.PlanetEscapeSlow);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PlanetEscapeSlow"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPlanetEscapeSlow()
    {
        StepActors();
        if (Flyaway.StepPlanetEscapeSlow())
        {
            Enter(EndingCreditsPhase.PlanetEscapeAccelerating);
            Flyaway.ReleaseEscapedAnimals(this);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PlanetEscapeAccelerating"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPlanetEscapeAccelerating()
    {
        StepActors();
        if (Flyaway.StepPlanetEscapeAccelerating(cinematicFrame))
            Enter(EndingCreditsPhase.OperationSuccessfulText);
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.OperationSuccessfulText"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastOperationSuccessfulText()
    {
        StepActors();
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.FadeOutToCredits"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastFadeOutToCredits()
    {
        StepActors();
        if (Stage.Fade.Darken())
            Show(EndingCreditsPhase.Credits, new EndingCreditsRollScene(CreditsRoll.SetupCredits()));
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PostCreditsBlank"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPostCreditsBlank()
    {
        if (PhaseHold.Count())
        {
            Stage.StartPostCreditsFade();
            Enter(EndingCreditsPhase.PostCreditsFadeIn);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PostCreditsFadeIn"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPostCreditsFadeIn()
    {
        if (Stage.Fade.Brighten())
        {
            CreditsRoll.FadeWaitingBackdrop(0);
            Enter(EndingCreditsPhase.PostCreditsShootingStars, EndingSequenceDefinitions.Timing.WaitingBackdropFadeSteps);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PostCreditsShootingStars"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPostCreditsShootingStars()
    {
        bool faded = PhaseHold.Count();
        CreditsRoll.FadeWaitingBackdrop(PhaseHold.Age);
        if (faded)
        {
            // The colour fade finishes, then the waiting phase holds the
            // backdrop for three seconds before displaying the producer panel.
            Enter(EndingCreditsPhase.PostCreditsWaitingBackdrop, EndingSequenceDefinitions.Timing.WaitingBackdropFrames);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PostCreditsWaitingBackdrop"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPostCreditsWaitingBackdrop()
    {
        if (PhaseHold.Count())
        {
            // The backdrop wait's end shows the complete result panel before its hold.
            InstalledArtwork.PostCreditsText.ShowResultPanel();
            Enter(EndingCreditsPhase.PostCreditsWaitingSamus, EndingSequenceDefinitions.Timing.ResultPanelHoldFrames);
        }
        return true;
    }

    /// <summary>Runs one update of the cast's <see cref="EndingCreditsPhase.PostCreditsWaitingSamus"/> phase.</summary>
    /// <returns>Whether the update goes on past the phase to the ending's colour effects and stars: false for a setup wait.</returns>
    internal bool CastPostCreditsWaitingSamus()
    {
        if (PhaseHold.Count())
        {
            EscapeScenes.SpawnEndingRewardActors();
            Show(EndingCreditsPhase.PostCreditsReward, new EndingRewardRevealScene(CreditsRoll.StartRewardFade()),
                EndingSequenceDefinitions.Timing.RewardRevealHalfFrames);
        }
        return true;
    }

    /// <summary>
    /// One update of the explosion's colour crossfade; the update that takes its last step enters the explosion's hold,
    /// as the flyaway's colours are restored.
    /// </summary>
    private void StepExplosionCrossfade()
    {
        if (!Explosion.StepExplosionCrossfade())
            return;
        Flyaway.PrepareFlyaway();
        Enter(EndingCreditsPhase.ZebesExplosionHold, EndingExplosionFadeDefinitions.HoldUpdates);
    }

    /// <summary>Runs the cast's actors in running order, their cues answered by <see cref="HandleSceneCue"/>.</summary>
    private void StepActors() => EscapeScenes.StepSprites(this, HandleSceneCue);

    /// <summary>
    /// Answers a cast actor's cue: the explosion's end and the completion text's end change the ending's phase, and
    /// every other cue goes to the escape scenes.
    /// </summary>
    private void HandleSceneCue(EndingSprite actor, EndingSceneCue cue)
    {
        switch (cue)
        {
            case EndingSceneCue.EndZebesExplosion:
                // The scene hides the sprite layer without deleting the actors: the stars must still
                // advance and become visible again when the flyaway starts.
                Stage.Colours.Flash(EndingFlash.ExplosionWhiteout, RgbColor.White);
                Enter(EndingCreditsPhase.WaitForPlanetEscapeMusic, EndingSequenceDefinitions.Timing.PlanetEscapeMusicWaitFrames);
                return;

            case EndingSceneCue.TransitionToCredits:
                Stage.StartFastFade();
                Enter(EndingCreditsPhase.FadeOutToCredits);
                return;

            case EndingSceneCue.WaitForFlyaway:
            case EndingSceneCue.FadeExplosionColours:
            case EndingSceneCue.SpawnExplosionSilhouette:
            case EndingSceneCue.StartZebesExplosion:
            case EndingSceneCue.ExplosionFinale:
            case EndingSceneCue.SpawnCompletedText:
            case EndingSceneCue.SpawnClearTime:
            case EndingSceneCue.SpawnHoursTens:
            case EndingSceneCue.SpawnHoursUnits:
            case EndingSceneCue.SpawnColon:
            case EndingSceneCue.SpawnMinutesTens:
            case EndingSceneCue.SpawnMinutesUnits:
                EscapeScenes.HandleSpriteCue(actor, cue);
                return;

            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingSceneCue)} {cue}.");
        }
    }

    /// <summary>One update of the staff credits, whose end blanks the screen for the post-credits scenes' first hold.</summary>
    internal void StepCredits(CreditsScroll scroll)
    {
        RequirePhase(EndingCreditsPhase.Credits);
        scroll.Step();
        if (!scroll.Finished)
            return;
        CreditsRoll.SetupPostCreditsBlank();
        Enter(EndingCreditsPhase.PostCreditsBlank, EndingSequenceDefinitions.Timing.PostCreditsBlankFrames);
    }

    /// <summary>
    /// One update of the reward's reveal: its two halves fade the reward's colours in, the copyright's hold between them
    /// showing the copyright alone, and the second half's end starts the reward's gesture.
    /// </summary>
    /// <exception cref="InvalidOperationException">The phase is not one the reveal runs.</exception>
    internal void StepRewardReveal(EndingRewardRevealScene reveal)
    {
        if (Phase == EndingCreditsPhase.PostCreditsCopyright)
        {
            // The sprites keep animating while only the copyright panel shows; hiding the actors must not reset their
            // animations or their reveal colour state.
            StepActors();
            if (PhaseHold.Count())
                Enter(EndingCreditsPhase.PostCreditsReward, EndingSequenceDefinitions.Timing.RewardRevealHalfFrames);
            return;
        }
        RequirePhase(EndingCreditsPhase.PostCreditsReward);
        // Both 64-update halves advance their shared colour fade on their fourth update and every fourth update after.
        CreditsRoll.StepRewardFade(reveal.Fade);
        StepActors();
        if (!PhaseHold.Count())
            return;
        if (!CreditsRoll.RewardCopyrightShown)
        {
            // The copyright panel replaces the producer panel. The text alone is shown until the copyright's hold ends.
            CreditsRoll.ShowRewardCopyright();
            Enter(EndingCreditsPhase.PostCreditsCopyright, EndingSequenceDefinitions.Timing.CopyrightHoldFrames);
            return;
        }
        // The reward step replaces the idle actors with the gesture animations.
        // The backgrounds are hidden during this sprite-only animation.
        sprites.Clear();
        Show(EndingCreditsPhase.PostCreditsGesture, new EndingRewardGestureScene(new EndingRewardGesture(Stage.EndingReward)));
    }

    /// <summary>
    /// One update of the final logo: its cues until it completes, which starts the item percentage, and the item
    /// percentage's move down, whose end starts the final message.
    /// </summary>
    /// <exception cref="InvalidOperationException">The phase is not one the logo runs.</exception>
    internal void StepLogo(EndingLogoScene scene)
    {
        if (Phase == EndingCreditsPhase.ItemPercentageScrollDown)
        {
            // The item-percentage scroll moves the shown text down two pixels an update, eighty in all, before the final
            // message starts above it.
            InstalledArtwork.PostCreditsText.Drop += EndingSequenceDefinitions.Timing.TextDropPixelsPerUpdate;
            if (PhaseHold.Count())
                Show(EndingCreditsPhase.SeeYouNextMission, new EndingTypingScene(scene.Logo, CreditsRoll.StartFinalMessage()));
            return;
        }
        RequirePhase(EndingCreditsPhase.PostCreditsLogo);
        scene.Logo.Step(Stage.Colours);
        if (scene.Logo.Completed)
            Show(EndingCreditsPhase.ItemPercentage, new EndingTypingScene(scene.Logo, CreditsRoll.StartItemPercentage()));
    }

    /// <summary>
    /// One update of a post-credits text typing beneath the logo: the item percentage until it asks for its move down, then
    /// the final message, which the ending stays on.
    /// </summary>
    /// <exception cref="InvalidOperationException">The phase is not one a typed text runs.</exception>
    internal void StepTyping(EndingTypingScene scene)
    {
        if (Phase == EndingCreditsPhase.SeeYouNextMission)
        {
            // The ending stays on this final screen: nothing leaves it until the player resets the game.
            StepActors();
            scene.Typing.Step();
            return;
        }
        RequirePhase(EndingCreditsPhase.ItemPercentage);
        scene.Typing.Step();
        if (scene.Typing.TypesItemPercentage && scene.Typing.Completed)
            Show(EndingCreditsPhase.ItemPercentageScrollDown, new EndingLogoScene(scene.Logo),
                EndingSequenceDefinitions.Timing.ItemPercentageDropUpdates);
    }

    /// <summary>
    /// Rotates the turning floor every other cinematic update while widening its zoom by <paramref name="zoomStep"/>.
    /// </summary>
    private void StepAtmosphericTransform(FloorZoomStep zoomStep)
    {
        // The floor turns during both fades as well as the fully visible interval.
        if (EndingCadenceDefinitions.AtmosphereFloorTurnEverySecondUpdate.IsDue(cinematicFrame))
            Stage.Floor.Turn(EndingMotionDefinitions.AtmosphereTurnSteps);
        Stage.Floor.Widen(zoomStep);
    }

    /// <summary>The animations showing the clear-time digits, indexed by digit.</summary>
    internal static readonly EndingSceneAnimation[] ClearTimeDigitAnimations =
    [
        EndingSceneAnimation.ClearTimeDigit0, EndingSceneAnimation.ClearTimeDigit1, EndingSceneAnimation.ClearTimeDigit2,
        EndingSceneAnimation.ClearTimeDigit3, EndingSceneAnimation.ClearTimeDigit4, EndingSceneAnimation.ClearTimeDigit5,
        EndingSceneAnimation.ClearTimeDigit6, EndingSceneAnimation.ClearTimeDigit7, EndingSceneAnimation.ClearTimeDigit8,
        EndingSceneAnimation.ClearTimeDigit9,
    ];

    /// <summary>Whether the explosion currently uses its dedicated glow composition.</summary>
    internal bool UsesExplosionFinaleDisplay => EscapeScenes.ExplosionDisplay != EndingExplosionDisplay.NotStarted && !EndingCreditsPresentation.ExplosionWhiteout(this)
        && EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Phase);

    /// <summary>
    /// The turning floor the ending's phase shows (#1515): its shot's installed picture (#1467) in the ending's colours
    /// (#1512), or none in a phase that shows no floor, when the floor layer shows nothing.
    /// </summary>
    internal AffineFloor? Floor()
    {
        EndingFloorShot shot = EndingScreenFloor.ShotOf(Phase);
        return shot == EndingFloorShot.None ? null : EndingScreenFloor.Of(InstalledArtwork.Sheets, shot)
            .Shaded(screen.Colours.SceneryShading(EndingFloorDefinitions.Scenery[shot]));
    }

    /// <summary>
    /// Captures the explosion's finale (#1514): the glow's picture behind the planet's, then the glow added over the whole
    /// screen with the debris glowing over it. The debris is drawn only as glow, never over the planet: the glow and the
    /// debris both add.
    /// </summary>
    internal LayeredScene CaptureExplosionFinale()
    {
        if (EscapeScenes.ExplosionDisplay != EndingExplosionDisplay.Finale) return CaptureExplosionBurst();
        DisplayLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleGlow),
            Plane(EndingExplosionDisplayDefinitions.FinaleExplosion),
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.FinaleGlow)),
            EndingCreditsPresentation.Sprites(this, blend: SpriteBlend.Glows),
        ];
        return new(EndingCreditsPresentation.Paint(this), layers, Stage.Brightness);

        PicturedPlaneLayer Plane(EndingExplosionPicture picture) => new(ExplosionPlane(picture), Offset16<PlanePixels>.Zero);
    }


    /// <summary>The ending's installed picture <paramref name="picture"/> of sheet <paramref name="sheet"/>.</summary>
    private SpriteSheetFrame Picture(SpriteSheetId sheet, string picture) => InstalledArtwork.SpriteSheet(sheet).Frame(picture);

    /// <summary>
    /// A one-screen plane showing the whole-screen picture <paramref name="picture"/>, which is <paramref name="scenery"/>, in
    /// its own of the ending's colours (#1467, #1512, #1544): the phase that shows it shows it whole, never gated by whether
    /// whether its picture has loaded (#1515).
    /// </summary>
    internal PicturedPlane ScreenPicture(SpriteSheetId sheet, string picture, EndingScenery scenery) =>
        new([new PlacedPlanePicture(Picture(sheet, picture), Offset16<PlanePixels>.Zero, screen.Colours.SceneryShading(scenery))]);

    /// <summary>
    /// The explosion's one-screen picture <paramref name="picture"/> (#1467, #1515), placed whole at the screen's top-left:
    /// the import pictures it as the screen shows it (#1626).
    /// </summary>
    private PicturedPlane ExplosionPlane(EndingExplosionPicture picture) =>
        new([new PlacedPlanePicture(Picture(SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(picture)),
            Offset16<PlanePixels>.Zero, screen.Colours.SceneryShading(EndingScenery.ExplosionPictures))]);

    /// <summary>
    /// The wavy Samus's texture (#1555): its one-screen picture in the ending's colours (#1512), repeated across and down
    /// the plane edge to edge, so the texture covers every screen line as its scroll climbs through the whole hold and
    /// its wave pushes each line sideways, less than one picture's width either way.
    /// </summary>
    internal PicturedPlane WavySamusTexture()
    {
        SpriteSheetFrame picture = Picture(SpriteSheetId.EndingWavySamusPicture, EndingPictureNames.WavySamus);
        PictureShading shading = screen.Colours.SceneryShading(EndingScenery.WavySamus);
        int width = picture.Size.X, height = picture.Size.Y;
        int lowestShownLine = EndingWavySamus.VerticalScrollAtAge(EndingSequenceDefinitions.Timing.WaitingBackdropFrames)
            + ScreenGeometry.ScreenHeightPixels;
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        for (int top = 0; top < lowestShownLine; top += height)
        {
            for (int left = -width; left <= width; left += width)
                placed.Add(new PlacedPlanePicture(picture, Offset16<PlanePixels>.Of(left, top), shading));
        }
        return new PicturedPlane(placed.ToImmutable());
    }

    /// <summary>The credits roll's plane: its staff lines' pictures (#1467) in the ending's colours (#1512).</summary>
    internal PicturedPlane CreditsPlane() => CreditsScroll.Plane(InstalledArtwork.SpriteSheet(SpriteSheetId.EndingCreditsRows),
        screen.Colours.SceneryShading(EndingScenery.CreditsRows));

    /// <summary>The waiting scene's backdrop (#1467, #1515), in the ending's colours (#1512).</summary>
    internal PicturedPlane WaitingPlane() => new(
        [new PlacedPlanePicture(Picture(SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.WaitingBackdrop), Offset16<PlanePixels>.Zero,
            screen.Colours.SceneryShading(EndingScenery.WaitingBackdrop))]);

    /// <summary>
    /// The screen of post-credits text (#1467, #1515, #1555): each piece the text scene shows now at its screen pixel,
    /// moved down by the scene's drop, in the ending's colours (#1512).
    /// </summary>
    internal PicturedPlane PostCreditsTextPlane()
    {
        SpriteSheet sheet = InstalledArtwork.SpriteSheet(SpriteSheetId.EndingPostCreditsPictures);
        PictureShading shading = screen.Colours.SceneryShading(EndingScenery.PostCreditsText);
        EndingPostCreditsText text = InstalledArtwork.PostCreditsText;
        return new PicturedPlane([.. text.Shown().Select(shown => new PlacedPlanePicture(sheet.Frame(shown.Picture),
            Offset16<PlanePixels>.Of(shown.Corner.X, shown.Corner.Y + text.Drop), shading))]);
    }

    /// <summary>
    /// Captures the explosion's burst (#1514): the stars behind the planet, the planet, the debris in front of it, and the
    /// burst's glow added over the planet and the debris that takes part in blends, never over the sky behind them.
    /// </summary>
    private LayeredScene CaptureExplosionBurst()
    {
        var planet = new PicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.PlanetExplosion), Offset16<PlanePixels>.Zero);
        SpritesLayer debris = EndingCreditsPresentation.Sprites(this, EndingSpriteLayer.Actors);
        DisplayLayer[] layers =
        [
            EndingCreditsPresentation.Sprites(this, EndingSpriteLayer.Stars), planet, debris,
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.BurstGlow), planet, debris),
        ];
        return new(EndingCreditsPresentation.Paint(this), layers, Stage.Brightness);
    }

    /// <summary>Whether the current cinematic phase leaves its escape or planet backdrop visible.</summary>
    internal bool EscapeBackgroundEnabled => EndingCreditsPhaseGroups.BeforeZebesExplosionHold.Contains(Phase)
        || !EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Phase);
}
