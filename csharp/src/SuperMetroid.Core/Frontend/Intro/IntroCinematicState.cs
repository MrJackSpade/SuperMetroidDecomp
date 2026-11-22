using System.Collections.Immutable;
using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Colours;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// The opening cinematic state, beginning with “The last Metroid is in captivity”.
/// </summary>
/// <remarks>
/// Construction runs the complete graphics setup. The visible state machine
/// continues through the first narration card, the complete first illustrated/typewriter
/// page, and the colour transition into the Mother Brain gameplay flashback.
/// </remarks>
public sealed partial class IntroCinematicState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.ScreenContent, artwork.ScreenContent))
        {
            NarrationPresentation = artwork.ScreenContent.IntroNarration;
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            BindCharacterArtwork(artwork.IntroCinematicArt);
        if (!ReferenceEquals(previous.SamusBodyArt, artwork.SamusBodyArt))
            BindSamusBodyArtwork(artwork.SamusBodyArt);
        if (!ReferenceEquals(previous.ProjectileFrameBindings, artwork.ProjectileFrameBindings))
            flashbackSamusArtwork.ProjectileFrames = artwork.ProjectileFrameBindings;
        if (!ReferenceEquals(previous.SpriteSheets, artwork.SpriteSheets))
            BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the flashbacks' Samus and projectiles draw from.</summary>
    internal void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        SceneSheets = Ensure.NotNull(sheets);
        projectileSheets = ProjectileSheets.IntroFlashbacks(sheets);
        flashbackSamusArtwork.Sheets = sheets;
        CurrentCeresFlight?.BindSheets(sheets);
    }

    /// <summary>Draws <paramref name="family"/>'s frames from the intro sheet.</summary>
    private CinematicSpriteSheet IntroSprites(CinematicSpriteFamily family) =>
        new(SceneSheets.Get(SpriteSheetId.IntroSprites), family, crossfade.ActorShading);

    /// <summary>The intro's projectile sheet, which the flashbacks' projectiles, bombs and trails draw from.</summary>
    private ProjectileSheets projectileSheets;

    /// <summary>The session's random number generator, which the demo Samus's animation draws from.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The session's unauthored Speed Booster cadence, bound into the flashback Samus.</summary>
    public required UnauthoredBoostCadence UnauthoredBoostCadence { get; init; }

    /// <summary>The opening's installed backdrops and the Ceres scenes' placements (#1616).</summary>
    internal IntroCinematicArtwork CharacterArtwork { get; set; }
    /// <summary>The installed Samus artwork the flashbacks' Samus draws from, rebound with the host's artwork.</summary>
    private readonly SamusArtworkBinding flashbackSamusArtwork;
    /// <summary>
    /// The flashback Samus's colour state (#1512): her suit and beams in the opening's colours, which her hurt flash
    /// changes as gameplay's does. Her draws publish its shading.
    /// </summary>
    private readonly SamusColours flashbackSamusColours = IntroSamusColours();
    /// <summary>Rebinds installed Samus pixels to active and later intro flashbacks.</summary>
    public void BindSamusBodyArtwork(InstalledSamusBodyArtwork value)
    {
        flashbackSamusArtwork.Body = Ensure.NotNull(value);
        flashbackSamusArtwork.ArmCannon = value.ArmCannon;
    }
    /// <summary>The installed narration content, rebound with the host's artwork.</summary>
    public required IntroNarrationPresentation NarrationPresentation
    {
        get;
        set
        {
            field = Ensure.NotNull(value);
            CurrentNarration?.BindNarration(value);
        }
    }
    /// <summary>Binds the opening's installed backdrops and the Ceres scenes' placements when the game is bound to its installed content.</summary>
    internal void BindCharacterArtwork(IntroCinematicArtwork value)
    {
        CharacterArtwork = Ensure.NotNull(value);
        CurrentCeresFlight?.BindArtwork(value.CeresFlight);
        // Rebinding keeps a restored fade's levels and the live letters and caret: the pictures show the installed sheets'
        // shades of the levels (#1512), and the first card and the final line are installed pictures the narration plane
        // places (#1467).
    }

    /// <summary>
    /// The room picture's vertical scroll in both gameplay flashbacks, in pixels: the screen shows the picture from this
    /// row down. Their actors' places are on the screen and do not move with the picture.
    /// </summary>
    internal const int GameplayFlashbackVerticalScroll = 8;

    /// <summary>Owns queued cinematic music and sound effects when installed.</summary>
    private readonly AudioQueues audio;
    /// <summary>The narration text the intro shows across narration and flashback scenes (#1467).</summary>
    internal readonly IntroNarrationText narrationText = new();
    /// <summary>The cinematic actors' sprites over the intro sprite page (#1358).</summary>
    internal readonly ScreenSprites screenSprites;
    /// <summary>Tracks the current physical button edges alongside scripted demo input.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Runs ordinary beam projectiles during gameplay-style flashbacks.</summary>
    internal readonly SamusProjectileSystem flashbackProjectiles;
    /// <summary>The installed services reproducing Samus's glitch reads, handed to the flashbacks' Samus and projectiles.</summary>
    private readonly SamusOverreadServices samusOverreads;
    /// <summary>The installed steps the egg's shell fragments fall by past their gravity curve, handed to the discovery scene.</summary>
    private readonly ShellFragmentFallPastCurve shellFallPastCurve;
    /// <summary>Runs the shared bomb owner, including projectile cooldown, in flashbacks.</summary>
    private readonly SamusBombProjectileSystem flashbackBombProjectiles;
    /// <summary>
    /// The update's audio requests the flashbacks' Samus, shots and bombs join, which go straight on into the frontend's
    /// queues (#2057): her missiles' firing sounds. The cinematic's own suppressions keep the rest away from the queues:
    /// missile impacts in a cinematic frame and her landing, footstep and liquid sounds while a cinematic is active.
    /// </summary>
    private readonly GameplaySoundRequests flashbackAudioRequests;
    /// <summary>Persistent narration actor and the caret that moves along the text of illustrated pages.</summary>
    internal IntroNarrationActors? CurrentNarration { get; private set; }
    /// <summary>Applies the selected Japanese subtitle treatment to narration pages.</summary>
    private readonly IntroJapaneseSubtitles subtitles;
    /// <summary>
    /// The current crossfade between the narration and a flashback scene (#1512, #1597); every scene shows at full until
    /// the first crossfade.
    /// </summary>
    private IntroCrossfade crossfade = new();
    /// <summary>
    /// The illustration the screen shows (#2489): from the crossfade into it until the next page's crossfade out of it ends;
    /// null while only the narration shows.
    /// </summary>
    private IntroIllustration? illustration;
    /// <summary>
    /// The opening's colour effects (#1733): the old Mother Brain fight's background lights, from the end of the crossfade
    /// into the fight until page two's crossfade recolours the room.
    /// </summary>
    internal ColourEffectSystem ColourEffects { get; set; } = ColourEffectSystem.ForScreen();
    /// <summary>
    /// The Mother Brain flashback while it runs, from its setup until the discovery's setup carries its Samus on and
    /// releases it; none outside it.
    /// </summary>
    internal IntroMotherBrainFlashback? MotherBrainFlashback { get; set; }
    /// <summary>How the Mother Brain flashback shows Samus: the flashback shows her, and deleting Mother Brain hides her.</summary>
    private IntroSamusDisplay flashbackSamusDisplay;
    /// <summary>Egg and baby actors in the SR388 discovery flashback.</summary>
    internal IntroBabyDiscoveryState? CurrentBabyDiscovery { get; set; }
    /// <summary>Laboratory actors shared by delivery and examination scenes.</summary>
    internal IntroScientistCutsceneState? CurrentScientistCutscene { get; private set; }
    /// <summary>Final SPACE COLONY flight sequence after the narrated pages.</summary>
    internal IntroCeresFlightState? CurrentCeresFlight { get; private set; }
    /// <summary>
    /// What the running crossfade did on this update (#1980), or null on an update no crossfade runs: the actors the
    /// crossfade's finish ends react to it after the cinematic phase has advanced it.
    /// </summary>
    private IntroCrossfadeTick? crossfadeTick;
    /// <summary>
    /// The updates the cinematic has run, counted from zero at its creation, which Samus's systems read.
    /// </summary>
    private int frameCounter;
    /// <summary>
    /// How bright the narration screen is and the cadence of its fades at cinematic entry, between pages and at exit; the
    /// intro starts black.
    /// </summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Black);
    /// <summary>The running narration hold.</summary>
    private UpdateCountdown timer = UpdateCountdown.Ended();

    /// <summary>Creates the opening cinematic and performs its initial graphics and music setup.</summary>
    /// <param name="overreads">
    /// The installed services reproducing Samus's glitch reads, for the flashbacks' Samus and projectiles, and the steps
    /// the egg's shell fragments fall by past their gravity curve.
    /// </param>
    /// <param name="audio">The audio state that owns queued music and sound effects; the cinematic waits on its music queue.</param>
    /// <param name="samusArtwork">
    /// The installed Samus artwork the flashbacks' Samus draws from; its sprite sheets are the ones the flashbacks' Samus
    /// and projectiles draw from until <see cref="BindSpriteSheets"/> binds others.
    /// </param>
    /// <param name="characterArtwork">The installed opening-cinematic background and sprite artwork.</param>
    /// <param name="language">The options menu's text language.</param>
    public IntroCinematicState(
        IntroCinematicOverreads overreads,
        AudioQueues audio,
        SamusArtworkBinding samusArtwork,
        IntroCinematicArtwork characterArtwork,
        TextLanguage language)
    {
        flashbackSamusArtwork = Ensure.NotNull(samusArtwork);
        SceneSheets = samusArtwork.Sheets;
        projectileSheets = ProjectileSheets.IntroFlashbacks(samusArtwork.Sheets);
        Ensure.NotNull(overreads);
        samusOverreads = Ensure.NotNull(overreads.Samus);
        shellFallPastCurve = Ensure.NotNull(overreads.ShellFall);
        flashbackAudioRequests = new GameplaySoundRequests(new CinematicSoundQueues(Ensure.NotNull(audio)));
        flashbackBombProjectiles = new SamusBombProjectileSystem(flashbackAudioRequests, samusArtwork);
        // The flashback's shots shake an earthquake of their own, which nothing in the flashback shows.
        flashbackProjectiles = new SamusProjectileSystem(flashbackAudioRequests, new BeamChargeState(), samusOverreads,
            new LeftBeamStartFromLastShot(SamusProjectileSystem.PlaceCount), new SamusShotCount(), samusArtwork, new RoomEarthquake());

        subtitles = new IntroJapaneseSubtitles(language);
        this.audio = Ensure.NotNull(audio);
        CharacterArtwork = Ensure.NotNull(characterArtwork);

        // Setup loads the sprite sheets the intro binds; the narration, the first narration card, the scene pages and
        // the portrait are installed pictures (#1467).
        screenSprites = new ScreenSprites();

        // The game's setup loads the ordinary beam colours before the full intro colours replace them, so the flashback
        // Samus shows the opening's colours from the start (her intro shades); the flashback projectile pages bind the
        // beam's pictures.

        // The loads above are invisible: the intro stays black until its fade.
        // The setup update itself runs as the next update's cinematic phase.
        step = new IntroCinematicStep.Initial();
    }

    /// <summary>The cinematic's step, carrying what its phase runs on.</summary>
    private IntroCinematicStep step;

    /// <summary>Gets the current opening-cinematic state-machine phase.</summary>
    public IntroCinematicPhase Phase => step.Phase;

    /// <summary>
    /// The narration page the intro has reached (#2489): the page <see cref="IntroCinematicPhase.PageText"/>,
    /// <see cref="IntroCinematicPhase.PageAwaitingInput"/> and <see cref="IntroCinematicPhase.PageCrossfade"/> show, and
    /// the one the illustration it turned to has followed.
    /// </summary>
    internal IntroNarrationPageId Page { get; set; } = IntroNarrationPageDefinitions.FirstPage;

    /// <summary>True when the SPACE COLONY caption and its final fade have completed.</summary>
    public bool CeresFlightFinished => CurrentCeresFlight?.Finished ?? false;

    /// <summary>Advances the opening cinematic by one gameplay update.</summary>
    /// <param name="controllerInput">The current raw held controller buttons.</param>
    public void Step(ControllerButton controllerInput)
    {
        flashbackProjectiles.ImpactAudio.BeginImpactAudioFrame(context: ImpactAudioContext.Cinematic);
        // Cinematic phases consume the newly pressed buttons, not the raw held ones. Latching at
        // this state boundary preserves a one-frame edge and prevents a held A/Start from
        // skipping both the options menu and the first narration page.
        controller.Latch(controllerInput);
        bool explosionsExistedBeforeThisFrame = MotherBrainFlashback?.Explosions is not null;
        frameCounter++;
        crossfadeTick = null;
        switch (step)
        {
            case IntroCinematicStep.Initial:
                // Stop the music, then load the opening music data.
                audio.QueueMusic(MusicRequest.Stop);
                audio.QueueMusic(
                    MusicRequest.LoadDataSet(MusicDataSet.LastMetroidInCaptivity));
                step = new IntroCinematicStep.WaitForInitialMusicQueue();
                break;

            case IntroCinematicStep.WaitForInitialMusicQueue:
                // A request queued with the shortest delay waits out that delay before HasQueuedMusic clears.
                if (MusicQueueFinished())
                {
                    step = new IntroCinematicStep.FadeInFirstNarration();
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicStep.FadeInFirstNarration:
                if (fade.Brighten())
                {
                    step = new IntroCinematicStep.LastMetroidIsInCaptivity();
                    timer = UpdateCountdown.For(IntroCinematicDefinitions.Timing.FirstLineHold);
                }
                break;

            case IntroCinematicStep.LastMetroidIsInCaptivity:
                if (timer.Step())
                {
                    audio.QueueMusic(MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.CaptivityLineSong));
                    step = new IntroCinematicStep.GalaxyIsAtPeace();
                    timer = UpdateCountdown.For(IntroCinematicDefinitions.Timing.CaptivityLineHold);
                }
                break;

            case IntroCinematicStep.GalaxyIsAtPeace:
                if (timer.Step())
                {
                    // The next music requests wait on the music queue before the
                    // documented four-second hold; they take the shortest delay.
                    audio.QueueMusic(MusicRequest.Stop);
                    audio.QueueMusic(
                        MusicRequest.LoadDataSet(MusicDataSet.GalaxyAtPeace));
                    audio.ScheduleMusic(
                        MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.PeaceLineSong),
                        IntroCinematicDefinitions.Music.SceneTrackDelay);
                    step = new IntroCinematicStep.WaitForSecondMusicQueue();
                }
                break;

            case IntroCinematicStep.WaitForSecondMusicQueue:
                if (MusicQueueFinished())
                {
                    step = new IntroCinematicStep.HoldFirstNarration();
                    timer = UpdateCountdown.For(IntroCinematicDefinitions.Timing.PeaceLineHold);
                }
                break;

            case IntroCinematicStep.HoldFirstNarration:
                if (timer.Step())
                {
                    step = new IntroCinematicStep.FadeOutFirstNarration();
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicStep.FadeOutFirstNarration:
                // The first page is set up on the update after the fade-out reaches black.
                if (fade.Darken())
                    step = new IntroCinematicStep.SetupFirstPage();
                break;

            case IntroCinematicStep.SetupFirstPage:
                SetupFirstIllustratedPage();
                break;

            case IntroCinematicStep.WaitForFirstPageMusicQueue waiting:
                // The scene song scheduled its track delay ahead is the longest request issued by
                // the first page's setup, so HasQueuedMusic becomes clear once that delay has passed.
                if (MusicQueueFinished())
                {
                    waiting.Narration.StartPage(Page);
                    step = new IntroCinematicStep.FadeInFirstPage(waiting.Narration);
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicStep.FadeInFirstPage fadingIn:
                if (fade.Brighten())
                    step = new IntroCinematicStep.PageText(fadingIn.Narration);
                break;

            case IntroCinematicStep.PageText:
                break;

            case IntroCinematicStep.PageAwaitingInput awaiting:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    TurnPage(awaiting.Narration);
                break;

            case IntroCinematicStep.MotherBrainCrossfade flashbackCrossfade:
                StepMotherBrainCrossfade(flashbackCrossfade.Narration);
                break;

            case IntroCinematicStep.MotherBrainFlashback flashbackPage:
                if (MotherBrainFlashback?.MotherBrain.NextPageRequested == true)
                {
                    // The crossfade recolours the room, so the fight lights' last colours stop showing.
                    ColourEffects = ColourEffectSystem.ForScreen();
                    StartPageCrossfade(flashbackPage.Narration, IntroCrossfadeScene.Flashback, IntroIllustration.MotherBrainFlashback);
                }
                break;

            case IntroCinematicStep.PageCrossfade pageCrossfade:
                // The reverse crossfade undoes the one into the illustration exactly: the narration and the portrait fade in
                // while the illustration, its projectiles and its sprites fade out, which keep running beneath it.
                if (AdvanceCrossfade())
                {
                    illustration = null;
                    step = new IntroCinematicStep.PageText(pageCrossfade.Narration);
                }
                break;

            case IntroCinematicStep.BabyDiscoveryCrossfade discoveryCrossfade:
                StepBabyDiscoveryCrossfade(discoveryCrossfade.Narration, discoveryCrossfade.Discovery);
                break;

            case IntroCinematicStep.BabyDiscovery discoveryPage:
                if (CurrentBabyDiscovery?.NextPageRequested == true)
                    StartPageCrossfade(discoveryPage.Narration, IntroCrossfadeScene.Flashback, IntroIllustration.BabyDiscovery);
                break;

            case IntroCinematicStep.BabyMetroidDeliveryCrossfade deliveryCrossfade:
                if (StepTextToScientistCrossfade())
                    step = new IntroCinematicStep.BabyMetroidDelivery(deliveryCrossfade.Narration);
                break;

            case IntroCinematicStep.BabyMetroidDelivery delivery:
                if (CurrentScientistCutscene?.NextPageRequested == true)
                    StartPageCrossfade(delivery.Narration, IntroCrossfadeScene.Laboratory, IntroIllustration.BabyDelivery);
                break;

            case IntroCinematicStep.BabyMetroidExamination examination:
                if (CurrentScientistCutscene?.NextPageRequested == true)
                    StartPageCrossfade(examination.Narration, IntroCrossfadeScene.Laboratory, IntroIllustration.BabyExamination);
                break;

            case IntroCinematicStep.BabyMetroidExaminationCrossfade examinationCrossfade:
                if (StepTextToScientistCrossfade())
                    step = new IntroCinematicStep.BabyMetroidExamination(examinationCrossfade.Narration);
                break;

            case IntroCinematicStep.StartFinalPage finalPage:
                StartFinalPage(finalPage.Narration);
                break;

            case IntroCinematicStep.IntroFadeOut:
                // The intro's end fades out and finishes once the screen is black.
                if (fade.Darken())
                {
                    // The Ceres approach immediately replaces the narration display setup with the
                    // Ceres turning-floor scene. Give that scene its own display, colour, and
                    // brightness owner: the narration has just deliberately reached
                    // a blanked screen, while the approach restores full brightness only after
                    // the delayed music request has completed.
                    CurrentCeresFlight = new IntroCeresFlightState(audio,
                        CharacterArtwork.CeresFlight,
                        SceneSheets, subtitles.Language);
                    step = new IntroCinematicStep.CeresFlight(CurrentCeresFlight);
                }
                break;

            case IntroCinematicStep.CeresFlight flight:
                flight.Flight.Step();
                break;
            default:
                throw new InvalidOperationException($"Undefined intro step {step}.");
        }

        if (CurrentNarration is not null)
        {
            CurrentNarration.Step(controller.NewlyPressed);
            // The page's text phase becomes its input wait once the narration's wait for that page starts.
            if (step is IntroCinematicStep.PageText typing && CurrentNarration.AwaitingInputPage == Page)
                step = new IntroCinematicStep.PageAwaitingInput(typing.Narration);
            if (CurrentNarration.FinalHoldEnded && step is IntroCinematicStep.PageText lastPage)
            {
                // The last page starts the final fade-out; the screen on this request frame is still the full-brightness
                // last page.
                fade.StepEvery(IntroCinematicDefinitions.Fade.FinishUpdatesPerStep);
                step = new IntroCinematicStep.IntroFadeOut(lastPage.Narration);
            }
        }

        // The cinematic step runs Samus's input and movement phases, ahead of the sprite pass, only while the intro shows her.
        if (MotherBrainFlashback is { } flashback)
            StepMotherBrainFlashbackActors(flashback, explosionsExistedBeforeThisFrame);

        // The cinematic sprites update after the cinematic phase, so the
        // actors a scene setup spawns take their first step in that same update.
        CurrentBabyDiscovery?.Step(frameCounter, CrossfadeFinishing);
        CurrentScientistCutscene?.Step(crossfadeTick);

        // The fight lights end on the update Mother Brain starts page two: their last colours stay until page two's
        // crossfade recolours the room on the next update (#1733).
        if (Phase != IntroCinematicPhase.MotherBrainFlashback || MotherBrainFlashback?.MotherBrain.NextPageRequested != true)
            ColourEffects.StepScreen();
    }

    /// <summary>
    /// Runs the flashback's Samus, then its cinematic actors, in the cinematic's order: Samus's input and movement phases
    /// while the intro shows her, Mother Brain's behaviour and the Rinkas, then Mother Brain's sprite and the explosions
    /// her fourth hit spawned on an earlier update.
    /// </summary>
    /// <param name="flashback">The running flashback.</param>
    /// <param name="explosionsExistedBeforeThisFrame">Whether her fourth hit spawned the explosions before this update.</param>
    internal void StepMotherBrainFlashbackActors(IntroMotherBrainFlashback flashback, bool explosionsExistedBeforeThisFrame)
    {
        // The cinematic step runs Samus's input and movement phases, ahead of the sprite pass, only while the intro shows her.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            // The intro demo's input phase refreshes the live radius before projectiles and movement.
            // Pose commits at the end of the previous frame deliberately leave it unchanged.
            SamusPoseMetadata.RefreshCollisionRadii(flashback.Samus);

            // Samus's update for her current state runs the demo input,
            // after the cinematic phase, so a demo loaded by the scene setup plays its first
            // entry in the same update, ahead of the Samus pose input that consumes it.
            StepMotherBrainDemo(flashback);

            StepMotherBrainFlashbackSamus(flashback);
        }

        // The cinematic's sprites update after the active cinematic phase and after Samus and her shots, so Mother
        // Brain can take a missile at its new position this same update.
        IntroMotherBrainSpriteState motherBrain = flashback.MotherBrain;
        motherBrain.RunBehavior(
            frameCounter,
            CrossfadeFinishing);
        // Deleting Mother Brain after the page-two crossfade hides Samus from
        // the next update on.
        if (!motherBrain.IsVisible)
            flashbackSamusDisplay = IntroSamusDisplay.Hidden;
        if (motherBrain.TakeMissileHit(flashbackProjectiles.EnemyImpacts, flashbackBombProjectiles))
        {
            flashback.Explosions = IntroMotherBrainExplosionSystem.SpawnFourthHitExplosions();
        }

        flashback.Rinkas.Step(
            flashback.Samus,
            motherBrainExploding: motherBrain.ExplosionStarted);

        // Mother Brain's sprite steps last, so one spawned on this update shows its first frame on the
        // same update as the first page's input transition.
        motherBrain.Step();
        if (explosionsExistedBeforeThisFrame)
        {
            // The eight explosions first animate on the update after they spawn; the fourth hit comes on the update
            // they spawn.
            flashback.Explosions?.Step(CrossfadeFinishing);
        }
    }

    /// <summary>Installs Mother Brain room collision, actors, Samus demo state, and the incoming gameplay colours.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void SetupMotherBrainFlashback(IntroNarrationActors narration)
    {
        Ensure.NotNull(narration);
        // The shared scene setup places the persistent narration caret off screen.
        // It remains alive and will be restored by the next text-page setup. Starting the cross-fade turns the page,
        // which deletes the Japanese text's next-page arrow.
        narration.HideCaret();
        narration.AcceptPageTurn();

        // The discovery shows the Mother Brain room's installed page picture; the room's blocks below are its terrain,
        // which Samus collides with, not what the room shows.
        var samus = new SamusState(flashbackAudioRequests, samusOverreads.FramesBeyondAnimation, UnauthoredBoostCadence,
            SamusCompatibility.ForNewSamus(), flashbackProjectiles.Charge, flashbackSamusArtwork)
        {
            Position = FixedPosition<RoomPixels>.FromWhole(IntroCinematicDefinitions.Flashback.SamusPosition),
            SelectedHudItem = HudItem.Missile,
            Missiles = IntroCinematicDefinitions.Flashback.Missiles,
            MaxMissiles = IntroCinematicDefinitions.Flashback.Missiles,
        };
        // Samus's shared frame phases read the active cinematic phase to select
        // intro colour restoration and suppress gameplay-only sounds.
        samus.Presentation.PresentInCinematic();
        samus.ChangePose(SamusPoseId.FacingLeftNormalPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.CommitPoseHistory();
        var motherBrain = new IntroMotherBrainSpriteState();
        var rinkas = new IntroRinkaSystem();

        // The flashback's collision room is built from its own terrain blocks, so the
        // visual room page never stands in for the collision contract.
        PlainTerrain terrain = CreateMotherBrainTerrain();
        var progression = new SharedGameState(Random);
        flashbackProjectiles.Reset();
        var demoInput = new DemoInputState();
        demoInput.Clear();
        demoInput.Enable();
        var flashback = new IntroMotherBrainFlashback(samus, motherBrain, rinkas, terrain, progression, demoInput);
        demoInput.Load(DemoSignallingRecordingId.IntroMotherBrainFlashback, () => HandleMotherBrainDemoCue(flashback));
        MotherBrainFlashback = flashback;
        // Samus is processed, and drawn ahead of the cinematic actors.
        flashbackSamusDisplay = IntroSamusDisplay.InFrontOfActors;

        // The scene setup starts the crossfade from the narration into the flashback, which starts black.
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Flashback);
        illustration = IntroIllustration.MotherBrainFlashback;

        step = new IntroCinematicStep.MotherBrainCrossfade(narration);
    }

    /// <summary>Advances the steps of the crossfade from the narration to the Mother Brain room, then shows the room.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void StepMotherBrainCrossfade(IntroNarrationActors narration)
    {
        if (!AdvanceCrossfade())
            return;

        // Transition completion selects the visible layers and blanks the English narration text. The
        // ornamental top and bottom rows survive.
        ClearVisibleNarrationText();
        StartFightLightsIfSamusDrawsFirst(flashbackSamusDisplay);
        step = new IntroCinematicStep.MotherBrainFlashback(narration);
    }

    /// <summary>
    /// The end of a crossfade into a flashback starts the old Mother Brain fight's background lights (#1733) while
    /// <paramref name="display"/>, how the intro shows Samus, has Samus draw first: the Mother Brain flashback sets it so, the
    /// discovery's setup has its actors draw first.
    /// </summary>
    private void StartFightLightsIfSamusDrawsFirst(IntroSamusDisplay display)
    {
        if (display == IntroSamusDisplay.InFrontOfActors)
            ColourEffects.StartScreen(IntroCinematicDefinitions.FightLights);
    }

    /// <summary>
    /// Turns the page whose input wait accepted a press (#2489): it starts the illustration the page turns to, or the next
    /// page at once when it turns to none.
    /// </summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void TurnPage(IntroNarrationActors narration)
    {
        IntroIllustration? next = IntroNarrationPageDefinitions.Pages[Page].TurnsTo;
        switch (next)
        {
            case IntroIllustration.MotherBrainFlashback:
                SetupMotherBrainFlashback(narration);
                return;
            case IntroIllustration.BabyDiscovery when MotherBrainFlashback is { } flashback:
                SetupBabyDiscoveryCrossfade(narration, flashback);
                return;
            case IntroIllustration.BabyDiscovery:
                throw new InvalidOperationException("The discovery follows the Mother Brain flashback, whose Samus it carries on.");
            case IntroIllustration.BabyDelivery:
                SetupBabyMetroidDelivery(narration);
                return;
            case IntroIllustration.BabyExamination:
                SetupBabyMetroidExamination(narration);
                return;
            case null:
                SetupFinalPage(narration);
                return;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroIllustration)} {next}.");
        }
    }

    /// <summary>
    /// Starts the page <paramref name="shown"/>, the illustration showing, leads into (#2489): its narration spawns and
    /// restores the caret, and the crossfade from <paramref name="from"/> back to the narration starts while the
    /// illustration's actors keep running beneath it.
    /// </summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    /// <param name="from">The scene the crossfade leaves.</param>
    /// <param name="shown">The illustration showing, whose actor asked for the next page.</param>
    private void StartPageCrossfade(IntroNarrationActors narration, IntroCrossfadeScene from, IntroIllustration shown)
    {
        Page = IntroNarrationPageDefinitions.PageAfter[shown];
        narration.StartPage(Page);
        crossfade = new IntroCrossfade(from, IntroCrossfadeScene.Narration);
        step = new IntroCinematicStep.PageCrossfade(narration);
    }

    /// <summary>Replaces the Mother Brain flashback with the discovery scene: the SR388 egg and baby.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    /// <param name="flashback">The Mother Brain flashback the discovery replaces, whose Samus it carries on.</param>
    internal void SetupBabyDiscoveryCrossfade(IntroNarrationActors narration, IntroMotherBrainFlashback flashback)
    {
        Ensure.NotNull(narration);
        Ensure.NotNull(flashback);
        narration.HideCaret();
        narration.AcceptPageTurn();

        // The discovery setup shows the second authored room page, declares a
        // collision room, carries Samus into new egg/baby/demo owners, and reuses the same
        // text-to-gameplay colour crossfade as the Mother Brain scene.
        var discovery = new IntroBabyDiscoveryState(Random, flashback.Samus, shellFallPastCurve, audio);
        CurrentBabyDiscovery = discovery;

        // The old gameplay actors have reached their delete animations. Releasing the scoped
        // references prevents their projectile and collision steps from running behind the
        // new scene while preserving the shared artwork and colours they intentionally reuse.
        MotherBrainFlashback = null;
        flashbackProjectiles.Reset();

        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Flashback);
        illustration = IntroIllustration.BabyDiscovery;
        step = new IntroCinematicStep.BabyDiscoveryCrossfade(narration, discovery);
    }

    /// <summary>Reuses the text-to-gameplay fade before showing the SR388 discovery room alone.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    /// <param name="discovery">The discovery scene the crossfade shows.</param>
    internal void StepBabyDiscoveryCrossfade(IntroNarrationActors narration, IntroBabyDiscoveryState discovery)
    {
        if (!AdvanceCrossfade())
            return;

        // The crossfade's end clears the English text region after the final fade update. The
        // second room page is already shown, so the SR388 room becomes the sole room page.
        ClearVisibleNarrationText();
        StartFightLightsIfSamusDrawsFirst(discovery.SamusDisplay);
        step = new IntroCinematicStep.BabyDiscovery(narration);
    }

    /// <summary>Starts the first laboratory scene with its rightward page pan and separate scientist fade timer.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    internal void SetupBabyMetroidDelivery(IntroNarrationActors narration)
    {
        Ensure.NotNull(narration);
        // Scientist crossfades use the sibling setup, which performs
        // the same off-screen caret move before changing the visible layer configuration.
        narration.HideCaret();
        narration.AcceptPageTurn();

        // The delivery setup shows the laboratory page, starts it scrolled right and down, and
        // spawns the delivered baby before starting the crossfade into the laboratory. Its picture is already in the
        // intro's artwork.
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateDelivery();
        CurrentBabyDiscovery = null;

        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Laboratory);
        illustration = IntroIllustration.BabyDelivery;
        step = new IntroCinematicStep.BabyMetroidDeliveryCrossfade(narration);
    }

    /// <summary>
    /// Advances the crossfade from the narration into the laboratory: the narration fades away while the laboratory and its
    /// scientists come in. True once it has ended, which clears the narration's text.
    /// </summary>
    private bool StepTextToScientistCrossfade()
    {
        if (!AdvanceCrossfade())
            return false;

        ClearVisibleNarrationText();
        return true;
    }

    /// <summary>
    /// Advances the running crossfade by one update and publishes what it did to this update's actors (#1980); true once
    /// it has ended.
    /// </summary>
    private bool AdvanceCrossfade()
    {
        IntroCrossfadeTick tick = crossfade.Advance();
        crossfadeTick = tick;
        return tick.Ended;
    }

    /// <summary>Whether the running crossfade reached its final update on this update (#1980).</summary>
    private bool CrossfadeFinishing => crossfadeTick?.Finishing == true;

    /// <summary>Starts the second laboratory scene on its vertically offset page.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void SetupBabyMetroidExamination(IntroNarrationActors narration)
    {
        narration.HideCaret();
        narration.AcceptPageTurn();

        // The examination setup shows the examination page, starts the page scrolled up, and spawns the examined baby.
        // Its setup is otherwise the same scientist crossfade as delivery.
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateExamination();
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Laboratory);
        illustration = IntroIllustration.BabyExamination;
        step = new IntroCinematicStep.BabyMetroidExaminationCrossfade(narration);
    }

    /// <summary>Seeds the last narration page's timer, preserving the English and Japanese pages' difference in timing.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void SetupFinalPage(IntroNarrationActors narration)
    {
        // The last page's setup turns the page before it, which deletes the Japanese text's next-page arrow. English
        // starts the last page in this same update; Japanese text starts it on the next update.
        narration.AcceptPageTurn();
        if (subtitles.Enabled)
            step = new IntroCinematicStep.StartFinalPage(narration);
        else
            StartFinalPage(narration);
    }

    /// <summary>Replaces the page text, caret, and eye animation without a colour transition.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    private void StartFinalPage(IntroNarrationActors narration)
    {
        // The last page has no colour transition. Only the English text region, caret, eye
        // animation, and final page are replaced.
        Page = IntroNarrationPageDefinitions.FinalPage;
        narration.StartPage(Page);
        CurrentScientistCutscene = null;
        step = new IntroCinematicStep.PageText(narration);
    }

    /// <summary>Runs the scripted Samus gameplay, projectiles, collisions, and colour updates for the Mother Brain flashback.</summary>
    /// <param name="flashback">The running flashback.</param>
    internal void StepMotherBrainDemo(IntroMotherBrainFlashback flashback)
    {
        DemoInputState demoInput = flashback.DemoInput;
        SamusState samus = flashback.Samus;
        demoInput.Step();

        // The HUD and projectile step counts down the shared firing cooldown before
        // Samus's shots update. Standing facing left she cannot place bombs, but the cooldown
        // still decides when a missile may fire.
        flashbackBombProjectiles.StepFrame(flashback.Terrain, samus, demoInput.Held, demoInput.NewlyPressed);
        // A missile's firing sound joins the frontend's queues as the shot fires.
        flashbackProjectiles.StepFrame(flashback.Terrain, samus, new ProjectileButtonInput { ControllerInput = demoInput.Held, ControllerNewInput = demoInput.NewlyPressed, ControllerPreviousNewInput = demoInput.PublishedPreviousNewlyPressed }, cameraPosition: Position16<RoomPixels>.Zero, samusBombs: flashbackBombProjectiles, progression: flashback.Progression);

        // Both the demo and locked movement modes end by resetting the
        // movement and pose-change state, so a later missile inherits only the
        // movement recorded in its own update.
        SamusAerialPoseTransitions.DropTransitionShot(samus);
        samus.ProjectileInheritance.ClearMovement();
    }

    /// <summary>
    /// Runs the movement/animation/interruption/colour slice of the intro demo's
    /// movement phase, followed by the timer owner.
    /// </summary>
    private void StepMotherBrainFlashbackSamus(IntroMotherBrainFlashback flashback)
    {
        SamusState samus = flashback.Samus;
        PlainTerrain terrain = flashback.Terrain;
        DemoInputState flashbackInput = flashback.DemoInput;
        GameplayAction demoInput = flashbackInput.Held;

        // Each frame shows the definitions selected by the previous Samus draw. This
        // is what makes pose/frame changes use their corresponding artwork rather than
        // reading stale standing pictures as a hurt body on the next visible frame.

        AerialMovementResult? fallingMovement = null;
        BlockMoveResult? hurtEndingProbe = null;
        bool transitionAccepted = false;
        bool groundedDemoStepped = false;
        if (samus.Knockback is SamusKnockback.Moving knockback)
        {
            // The intro demo's movement phase runs the knockback movement that owns Samus. Its shared
            // vertical calculation now carries the Rinka knockback's arc through its apex.
            SamusKnockbackMovement.Step(terrain, samus, knockback, frameCounter);
        }
        else if (SamusPoseMetadata.IsKnockbackPose(samus.Pose))
        {
            hurtEndingProbe = SamusGroundedMovement.RunNoSpeedCalculationGroundingProbe(
                terrain, samus, frameCounter);
        }
        else if (flashbackInput.Enabled &&
            (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose)))
        {
            // The demo's later run/jump records use the same input/movement order as the
            // discovery scene. This coordinator already animates and commits its pose.
            IntroSamusDemoMovement.StepGroundedLeft(terrain, samus, demoInput,
                flashbackInput.NewlyPressed, frameCounter, Random);
            groundedDemoStepped = true;
        }
        else if (samus.Pose == SamusPoseId.SpinJumpLeftPose)
        {
            fallingMovement = SamusAerialMovement.StepSpinJump(SamusMovement.SpinJumping, terrain, samus,
                demoInput, frameCounter, flashbackInput.NewlyPressed);
        }
        else if (SamusPoseMetadata.IsLeftFacingNormalJumpPose(samus.Pose))
        {
            fallingMovement = SamusAerialMovement.StepNormalJump(SamusMovement.NormalJumping, terrain, samus,
                demoInput, frameCounter);
        }
        else if (SamusPoseTraits.IsIn(samus.Pose, SamusPoseFamily.PlainFallingOnly))
        {
            // When knockback ends humanoid knockback it selects the right or left falling pose and restores
            // normal movement. The following frames therefore run Samus's ordinary fall until the room's
            // collision reports the laboratory floor.
            fallingMovement = SamusAerialMovement.StepFalling(SamusMovement.Falling,

                terrain,
                samus,
                demoInput,
                frameCounter);
        }

        // Intro-demo state is not an animation shortcut. The intro demo's movement phase advances the same
        // animation delays as gameplay, including every visible hurt/fall frame.
        if (!groundedDemoStepped)
            SamusPresentation.Animate(samus, new SamusAnimationStep { Ground = IntroCinematicDefinitions.Flashback.Ground, ControllerInput = demoInput, FrameCounter = frameCounter, Random = Random });

        // The pose animation's transition steps publish a prospective pose; they are not ordinary frame
        // delays. The intro demo's movement phase consumes that publication before the value following the step
        // can ever be misread as another animation duration. This completes landing art's
        // transition from NormalLandingLeftPose to FacingLeftNormalPose instead of walking beyond its pose animation.
        bool animationTransitionApplied = SamusPoseTransitions.ApplyPendingAnimationTransition(samus);
        animationTransitionApplied |= SamusKnockbackMovement.TryFinishExpiredHitInterruption(samus);
        transitionAccepted |= animationTransitionApplied;

        if (!animationTransitionApplied && hurtEndingProbe is { IsUnobstructedDownwardMovement: true })
        {
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus, terrain, frameCounter);
            animationTransitionApplied = transitionAccepted = true;
        }

        // Samus's ordinary update resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation all come from the same landing implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true } && SamusPoseMetadata.ReadAim(samus) is SamusPoseAim.ISteady sourceAim)
        {
            SamusPoseTransitions.ApplyAerialLanding(samus, sourceAim, wasSpinning: samus.Pose == SamusPoseId.SpinJumpLeftPose, demoInput);
            transitionAccepted = true;
        }

        // The intro demo's movement phase consumes the knockback timer the first Rinka's hit published on the
        // preceding cinematic-sprite pass. Keeping this after animation matches Samus's update during the intro demo:
        // the hit frame finishes its standing animation before the knockback start installs the knockback pose.
        if (samus.Knockback is SamusKnockback.NoKnockback &&
            samus.KnockbackHold.IsRunning)
        {
            SamusKnockbackMovement.Start(samus, demoInput, KnockbackLaunch.FromHurtState(samus), poseCollision: null);
            transitionAccepted = true;
        }

        // The intro finishes its pose transition as ordinary gameplay does, after movement, animation,
        // and hurt interruption. Publish only the final accepted pose, once per update;
        // an unchanged animation frame must not erase the preceding transition history.
        if (transitionAccepted)
            samus.CommitPoseHistory();

        // The intro demo runs Samus's ordinary colouring even in intro-demo state.
        // This supplies the alternating hurt colors and eventual suit-colour restoration;
        // invincibility flicker remains independently enforced by Samus.Draw.
        SamusHurtFlash.Update(flashbackSamusColours, samus, demoInput);

        // The cinematic ages Samus's two hurt timers after both of her state updates, but before the
        // cinematic's actors can publish a new Rinka collision later in this frame.
        samus.DecrementHurtTimers();
    }

    /// <summary>Runs the Mother Brain demo's end step, freezing input after its prescribed Samus pose change.</summary>
    /// <remarks>The flashback's recording runs it as its signal (<see cref="DemoInputCue.EndIntroMotherBrainFlashback"/>).</remarks>
    private static void HandleMotherBrainDemoCue(IntroMotherBrainFlashback flashback)
    {
        // The flashback's end stops the demo on this update, with Samus standing facing left and her pose and its
        // animation started afresh; she shows standing through the rest of Mother Brain's explosions.
        SamusState samus = flashback.Samus;
        // A Samus already standing facing left keeps her running animation.
        samus.ChangePoseRestartingIfChanged(SamusPoseId.FacingLeftNormalPose);
        SamusPoseMetadata.RefreshCollisionRadii(samus);
        samus.CommitPoseHistory();
        flashback.DemoInput.Disable();
    }

    /// <summary>Builds the flashback's room: a plain terrain, which the flashback's own pictures show.</summary>
    private static PlainTerrain CreateMotherBrainTerrain() => new(
        new Extent16<RoomBlocks>(IntroMotherBrainCollisionDefinitions.Columns, IntroMotherBrainCollisionDefinitions.RoomRows),
        IntroMotherBrainCollision.CreateCollision());

    /// <summary>The flashback Samus's starting colour state: her suit and beams in the opening's own colours (#1512).</summary>
    private static SamusColours IntroSamusColours()
    {
        var colours = new SamusColours();
        colours.ShowSuit(SamusShades.IntroSuit);
        colours.ShowBeams(SamusShades.IntroBeams);
        return colours;
    }

    /// <summary>
    /// Prepares Mother Brain's sprites once per display; trail drawing advances state, so a captured frame cannot be redrawn through this path.
    /// </summary>
    /// <param name="flashback">The running flashback.</param>
    internal SpriteFrameBuffer PrepareMotherBrainSprites(IntroMotherBrainFlashback flashback)
    {
        // Both direct rendering and detached capture enter here once per display.
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        // Samus shown over the cinematic actors draws her and both projectile passes before the
        // cinematic actors; once deleting Mother Brain clears it, only the actors remain.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            flashback.Samus.Presentation.Draw(flashback.Samus, sprites, cameraPosition: Position16<RoomPixels>.Zero, frameCounter);
            // The flashback's artwork holds the power beam's pictures whatever beam its Samus fires.
            var projectileArtwork = new ProjectileDrawArtwork(projectileSheets);
            flashbackProjectiles.DrawLiveProjectiles(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
            flashbackProjectiles.AdvanceTrails(timeIsFrozen: false);
            flashbackProjectiles.DrawTrails(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
            flashbackProjectiles.DrawExplosions(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
        }
        flashback.Rinkas.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroRinka));
        flashback.Explosions?.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroMotherBrainExplosion));
        // A hidden Mother Brain shows no pose and draws nothing.
        IntroMotherBrainSpriteState motherBrain = flashback.MotherBrain;
        if (motherBrain.IsVisible && motherBrain.Pose is { } motherBrainPose)
        {
            // Samus shown over the cinematic actors makes the intro's sprite draw put Samus first and cinematic
            // actors afterward. Retaining that drawing order preserves overlap wins.
            IntroSprites(CinematicSpriteFamily.IntroMotherBrain).Draw(
                motherBrainPose, screenSprites,
                IntroMotherBrainSpriteState.Position,
                motherBrain.Shading(crossfade.ActorShading(IntroMotherBrainSpriteState.Colouring)));
        }
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Orders Samus and discovery actors by how the intro shows Samus before finalizing sprite data.</summary>
    /// <param name="discovery">The discovery scene shown.</param>
    internal SpriteFrameBuffer PrepareBabyDiscoverySprites(IntroBabyDiscoveryState discovery)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        // The cinematic draw layers Samus and the cinematic actors by how the intro shows her.
        IntroSamusDisplay display = discovery.SamusDisplay;
        if (display == IntroSamusDisplay.InFrontOfActors)
            DrawBabyDiscoverySamus(discovery, sprites);
        screenSprites.BeginFrame();
        discovery.DrawActors(screenSprites, IntroSprites(CinematicSpriteFamily.IntroEggEffect),
            IntroSprites(CinematicSpriteFamily.IntroDiscoveryActor));
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        if (display == IntroSamusDisplay.BehindActors)
            DrawBabyDiscoverySamus(discovery, sprites);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Draws Samus in screen-relative coordinates as the demo brings her in from the right.</summary>
    private void DrawBabyDiscoverySamus(IntroBabyDiscoveryState discovery, SpriteFrameBuffer sprites)
    {
        // This cinematic deliberately keeps the camera's X at zero. Samus starts right of
        // the viewport, and the demo makes her enter from the right; the second room page
        // shows different art, not a hidden camera coordinate a screen to the right.
        discovery.Samus.Presentation.Draw(discovery.Samus, sprites, cameraPosition: Position16<RoomPixels>.Zero, frameCounter);
    }

    /// <summary>Collects the scientist scene's actors for the current cinematic display.</summary>
    /// <param name="cutscene">The scientist scene shown.</param>
    internal SpriteFrameBuffer PrepareScientistSprites(IntroScientistCutsceneState cutscene)
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        cutscene.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroScientist));
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();
        return sprites;
    }

    /// <summary>Collects the persistent caret, the Japanese text's next-page arrow and illustrated-page cinematic actors in display order.</summary>
    /// <param name="narration">The narration actors the pages run with.</param>
    internal SpriteFrameBuffer PrepareIllustratedPageSprites(IntroNarrationActors narration)
    {
        Ensure.NotNull(narration);
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        // The caret blinks off by hiding, showing no pose.
        if (narration.CaretPose is { } caretPose)
        {
            // The persistent caret begins at its narration place, goes off screen during illustrated
            // crossfades, and is restored when the next narration page starts.
            IntroSprites(CinematicSpriteFamily.IntroCaret).Draw(
                caretPose, screenSprites, narration.Caret,
                IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors));
        }
        // The Japanese text's next-page arrow is drawn after the caret, which comes first (#1655).
        narration.NextPageArrow?.Draw(screenSprites, installedArt: IntroSprites(CinematicSpriteFamily.IntroNextPageArrow));
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Builds the portrait page, clears optional Japanese glyphs, and seeds progressive narration and caret state.</summary>
    internal void SetupFirstIllustratedPage()
    {
        // The pages show blank text rows over the installed final line, drawn with the Japanese glyphs blanked (#1467).
        narrationText.ClearText();
        var narration = new IntroNarrationActors(
            narrationText,
            subtitles,
            audio,
            NarrationPresentation);
        CurrentNarration = narration;
        audio.QueueMusic(MusicRequest.Stop);
        audio.QueueMusic(
            MusicRequest.LoadDataSet(MusicDataSet.Intro));
        audio.ScheduleMusic(
            MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.IntroductionSong),
            IntroCinematicDefinitions.Music.SceneTrackDelay);
        step = new IntroCinematicStep.WaitForFirstPageMusicQueue(narration);
    }

    /// <summary>Whether the music queue has sent every request the cinematic queued.</summary>
    private bool MusicQueueFinished() => !audio.HasQueuedMusic;

    /// <summary>Clears the narration's text rows.</summary>
    private void ClearVisibleNarrationText() => narrationText.ClearText();

    /// <summary>
    /// The Mother Brain room's shading: the crossfade's, with the record the fight's background lights last showed laid
    /// over it while they run (#1733).
    /// </summary>
    internal PictureShading FightRoomShading =>
        ColourEffects.FrameOf(ColourEffectColours.OldMotherBrainBackgroundLights) is { } lights
            ? crossfade.SceneShading.With(ColourEffectShades.Of(ColourEffectColours.OldMotherBrainBackgroundLights, lights))
            : crossfade.SceneShading;

    /// <summary>Prepares the current display once, holding everything the display needs to draw it.</summary>
    internal LayeredScene ComposeScene()
    {
        if (step is IntroCinematicStep.CeresFlight flight)
            return flight.Flight.ComposeScene();

        SpriteFrameBuffer sprites;
        PicturedPlaneLayer room;
        if (illustration == IntroIllustration.MotherBrainFlashback && MotherBrainFlashback is { } flashback)
        {
            sprites = PrepareMotherBrainSprites(flashback);
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.MotherBrainRoom, FightRoomShading),
                flashback.MotherBrain.Background.AsPlaneOffset());
        }
        else if (illustration == IntroIllustration.BabyDiscovery && CurrentBabyDiscovery is { } discovery)
        {
            sprites = PrepareBabyDiscoverySprites(discovery);
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.BabyDiscoveryRoom, crossfade.SceneShading), new(0, GameplayFlashbackVerticalScroll));
        }
        else if (illustration is IntroIllustration.BabyDelivery or IntroIllustration.BabyExamination && CurrentScientistCutscene is { } cutscene)
        {
            sprites = PrepareScientistSprites(cutscene);
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePages.Laboratory(cutscene.Scene), crossfade.SceneShading), cutscene.Background);
        }
        else if (CurrentNarration is { } narration)
        {
            sprites = PrepareIllustratedPageSprites(narration);
            room = Plane(IntroScenePictures.PortraitPlane(SceneSheets, narration.EyeFrame, crossfade.SceneShading), new(0, GameplayFlashbackVerticalScroll));
        }
        else
        {
            // The first card is opaque and unscrolled and shows no sprites; later text is transparent
            // and cropped down by GameplayFlashbackVerticalScroll. Do not conflate those two display setups.
            return new(new FramePaint(Backdrop),
                new DisplayLayer[] { new PicturedPlaneLayer(IntroNarrationPictures.FirstCardPlane(SceneSheets, crossfade.SceneShading), default) { DrawsOpaque = true } },
                fade.Level);
        }

        var text = new PicturedPlaneLayer(IntroNarrationPictures.PagesPlane(SceneSheets, narrationText, crossfade.SceneShading, crossfade.NarrationShading), default);
        // Back to front (#1514): the page's room, the flashback's actors over it, and the narration over both. Each picture
        // and sprite is drawn whole at its layer's place, in its layer's order.
        return new(new FramePaint(Backdrop), new DisplayLayer[]
        {
            room, new SpritesLayer(FlashbackDraws(sprites.Draws)), text,
        }, fade.Level);
    }

    /// <summary>
    /// The colour the opening shows where nothing is drawn (#1512): the installed backdrop at the narration's level, as the
    /// backdrop fades with the narration.
    /// </summary>
    private ShownColour Backdrop
    {
        get
        {
            RgbColor installed = CharacterArtwork.Backdrop;
            int level = crossfade.LevelOf(IntroCrossfadeScene.Narration);
            return level == IntroCrossfade.Levels ? installed : new ShownColour.Faded(installed, new FadeLevel(level, IntroCrossfade.Levels));
        }
    }

    /// <summary>
    /// <paramref name="draws"/> with the flashback Samus's draws in her shading (#1512), and every draw shown in her beams'
    /// shade also at the level the crossfade shows her beam colours at: the shading, not the kind of draw, says which
    /// draws show the faded beam colours.
    /// </summary>
    private ImmutableArray<ImageSpriteDraw> FlashbackDraws(ImmutableArray<SpriteDraw> draws)
    {
        ImmutableArray<ImageSpriteDraw> shaded = flashbackSamusColours.ShadeOwnDraws(draws);
        int level = crossfade.LevelOf(IntroCrossfadeScene.Narration);
        if (level == IntroCrossfade.Levels)
            return shaded;
        var beamLevel = new ColourLevel(new FadeLevel(level, IntroCrossfade.Levels));
        var faded = ImmutableArray.CreateBuilder<ImageSpriteDraw>(shaded.Length);
        foreach (ImageSpriteDraw draw in shaded)
        {
            faded.Add(draw.Shading.Shades.Contains(SamusShades.IntroBeams)
                ? draw with { Shading = draw.Shading.With(beamLevel) }
                : draw);
        }
        return faded.MoveToImmutable();
    }

    /// <summary>The scene plane showing <paramref name="pictures"/> at the selected scroll (#1467).</summary>
    private static PicturedPlaneLayer Plane(PicturedPlane pictures, Offset16<PlanePixels> scroll) => new(pictures, scroll);

    /// <summary>The installed sheets the scene pictures are drawn from.</summary>
    private InstalledSpriteSheets SceneSheets { get; set; }
}
