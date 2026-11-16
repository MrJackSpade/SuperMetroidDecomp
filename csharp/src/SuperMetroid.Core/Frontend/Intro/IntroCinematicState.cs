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
[RoomScopeBoundary("A cinematic scene that owns its own flashback level, Samus projectiles and colour effects for the scene's lifetime; no gameplay room is loaded.")]
public sealed class IntroCinematicState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            NarrationPresentation = artwork.MapPresentation?.IntroNarration;
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            BindCharacterArtwork(artwork.IntroCinematicArt);
        if (!ReferenceEquals(previous.SamusBodyArt, artwork.SamusBodyArt))
            BindSamusBodyArtwork(artwork.SamusBodyArt ?? throw new InvalidOperationException(
                "The opening cinematic's flashback Samus requires the installed Samus body artwork."));
        if (!ReferenceEquals(previous.ProjectileFrameBindings, artwork.ProjectileFrameBindings))
            BindProjectileFrames(artwork.ProjectileFrameBindings);
        if (!ReferenceEquals(previous.SpriteSheets, artwork.SpriteSheets))
            BindSpriteSheets(artwork.SpriteSheets ?? throw new InvalidOperationException(
                "The opening cinematic requires the installed sprite sheets."));
    }

    /// <summary>Binds the installed sprite sheets the flashbacks' Samus and projectiles draw from.</summary>
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
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

    /// <summary>Binds the host-owned frame artwork the Mother Brain flashback's projectile and bomb slots draw.</summary>
    public void BindProjectileFrames(InstalledProjectileFrameBindings? bindings) =>
        flashbackProjectiles.Animation.FrameBindings = flashbackBombProjectiles.FrameBindings = bindings;

    /// <summary>The opening's installed backdrops and the Ceres scenes' placements (#1616).</summary>
    internal IntroCinematicArtwork? CharacterArtwork { get; private set; }
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
    /// <summary>The installed narration content.</summary>
    public IntroNarrationPresentation? NarrationPresentation
    {
        get;
        set
        {
            field = value;
            CurrentNarration?.BindNarration(value);
        }
    }
    /// <summary>Binds the opening's installed backdrops and the Ceres scenes' placements when the game is bound to its installed content.</summary>
    public void BindCharacterArtwork(IntroCinematicArtwork? value)
    {
        CharacterArtwork = value;
        CurrentCeresFlight?.BindArtwork(value?.CeresFlight);
        if (value is not null)
        {
            // A restored fade keeps its levels; its pictures show the installed sheets' shades of them (#1512). The first card and the final line are installed pictures the narration plane places (#1467), so a
            // rebind keeps the live letters and caret state as they are.
        }
    }

    /// <summary>
    /// The room picture's vertical scroll both gameplay flashbacks inherit from the first illustrated page.
    /// Their actor coordinates remain screen-relative and are not shifted with the background.
    /// </summary>
    internal const int GameplayFlashbackVerticalScroll = 8;

    /// <summary>Owns queued cinematic music and sound effects when installed.</summary>
    private readonly AudioQueues audio;
    /// <summary>The narration text the intro shows across narration and flashback scenes (#1467).</summary>
    internal readonly IntroNarrationText narrationText = new();
    /// <summary>The cinematic actors' sprite entries over the intro sprite page (#1358).</summary>
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
    private readonly GameplayAudioRequests flashbackAudioRequests;
    /// <summary>Persistent narration actor and caret walker for illustrated pages.</summary>
    internal IntroNarrationActors? CurrentNarration { get; private set; }
    /// <summary>The intro's narration actors, which exist from the first text page onward.</summary>
    internal IntroNarrationActors Narration => CurrentNarration ??
        throw new InvalidOperationException("The intro's narration actors have not been created; the text pages create them.");
    /// <summary>Applies the selected Japanese subtitle treatment to narration pages.</summary>
    private readonly IntroJapaneseSubtitles subtitles;
    /// <summary>
    /// The current crossfade between the narration and a flashback scene (#1512, #1597); every scene shows at full until
    /// the first crossfade.
    /// </summary>
    private IntroCrossfade crossfade = new();
    /// <summary>
    /// The opening's colour effects (#1733): the old Mother Brain fight's background lights, from the end of the crossfade
    /// into the fight until page two's crossfade recolours the room.
    /// </summary>
    internal ColourEffectSystem ColourEffects { get; private set; } = new(new ColourEffectSessionState());
    /// <summary>Samus gameplay state shared by the Mother Brain and SR388 flashbacks.</summary>
    internal SamusState? CurrentFlashbackSamus { get; private set; }
    /// <summary>Samus in the Mother Brain flashback, which exists while the flashback runs.</summary>
    internal SamusState FlashbackSamus => CurrentFlashbackSamus ??
        throw new InvalidOperationException("The Mother Brain flashback has not introduced Samus.");
    /// <summary>How the Mother Brain flashback shows Samus: the flashback shows her, and deleting Mother Brain hides her.</summary>
    private IntroSamusDisplay flashbackSamusDisplay;
    /// <summary>Mother Brain animation state active only in the first gameplay flashback.</summary>
    internal IntroMotherBrainSpriteState? CurrentFlashbackMotherBrain { get; private set; }
    /// <summary>Mother Brain's sprite in the flashback, which exists while the flashback runs.</summary>
    internal IntroMotherBrainSpriteState FlashbackMotherBrain => CurrentFlashbackMotherBrain ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Explosion actors shown after the Mother Brain demo input ends.</summary>
    internal IntroMotherBrainExplosionSystem? FlashbackMotherBrainExplosions { get; set; }
    /// <summary>Rinka actors and collision for the Mother Brain scene.</summary>
    internal IntroRinkaSystem? FlashbackRinkas { get; set; }
    /// <summary>Egg and baby actors in the SR388 discovery flashback.</summary>
    internal IntroBabyDiscoveryState? CurrentBabyDiscovery { get; private set; }
    /// <summary>The Baby Metroid discovery scene, which exists while that scene runs.</summary>
    internal IntroBabyDiscoveryState BabyDiscovery => CurrentBabyDiscovery ??
        throw new InvalidOperationException("The Baby Metroid discovery scene is not running.");
    /// <summary>Laboratory actors shared by delivery and examination scenes.</summary>
    internal IntroScientistCutsceneState? CurrentScientistCutscene { get; private set; }
    /// <summary>The scientist cutscene, which exists while that scene runs.</summary>
    internal IntroScientistCutsceneState ScientistCutscene => CurrentScientistCutscene ??
        throw new InvalidOperationException("The scientist cutscene is not running.");
    /// <summary>Final SPACE COLONY flight sequence after the narrated pages.</summary>
    internal IntroCeresFlightState? CurrentCeresFlight { get; set; }
    /// <summary>The flight to Ceres, which exists while that scene runs.</summary>
    internal IntroCeresFlightState CeresFlight => CurrentCeresFlight ??
        throw new InvalidOperationException("The Ceres flight is not running.");
    /// <summary>Scripted controller input for the Mother Brain gameplay demonstration.</summary>
    internal DemoInputState? CurrentFlashbackDemoInput { get; private set; }
    /// <summary>The flashback's scripted controller input, which exists while the flashback runs.</summary>
    internal DemoInputState FlashbackDemoInput => CurrentFlashbackDemoInput ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Collision data for the gameplay-style flashback room.</summary>
    internal RoomLevelData? CurrentFlashbackLevel { get; private set; }
    /// <summary>The flashback room's collision, which exists while the flashback runs.</summary>
    internal RoomLevelData FlashbackLevel => CurrentFlashbackLevel ??
        throw new InvalidOperationException("The Mother Brain flashback room is not loaded.");
    /// <summary>
    /// What the running crossfade did on this update (#1980), or null on an update no crossfade runs: the actors the
    /// crossfade's finish ends react to it after the cinematic phase has advanced it.
    /// </summary>
    private IntroCrossfadeTick? crossfadeTick;
    /// <summary>
    /// The shared Samus systems' frame counter: the updates the cinematic has run, counted from zero at its creation. The
    /// intro reaches the title long before the game counter's 65536-update wrap, so it is a plain count.
    /// </summary>
    private int frameCounter;
    /// <summary>
    /// How bright the narration screen is and the cadence of its fades at cinematic entry, between pages and at exit; the
    /// intro starts black.
    /// </summary>
    private readonly ScreenFade fade = new(0);
    /// <summary>The updates the running narration hold has left.</summary>
    private int timer;

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
    /// <param name="powerBombGlowColors">The installed Power Bomb glow colours the flashbacks' bombs glow in.</param>
    /// <param name="characterArtwork">The installed opening-cinematic background and sprite artwork.</param>
    /// <param name="japaneseText">The options menu's Japanese text setting.</param>
    public IntroCinematicState(
        IntroCinematicOverreads overreads,
        AudioQueues audio,
        SamusArtworkBinding samusArtwork,
        InstalledPowerBombGlowColors powerBombGlowColors,
        IntroCinematicArtwork? characterArtwork = null,
        bool japaneseText = false)
    {
        flashbackSamusArtwork = Ensure.NotNull(samusArtwork);
        SceneSheets = samusArtwork.Sheets;
        projectileSheets = ProjectileSheets.IntroFlashbacks(samusArtwork.Sheets);
        Ensure.NotNull(overreads);
        samusOverreads = Ensure.NotNull(overreads.Samus);
        shellFallPastCurve = Ensure.NotNull(overreads.ShellFall);
        flashbackAudioRequests = new GameplayAudioRequests(new CinematicAudioQueues(Ensure.NotNull(audio)));
        flashbackBombProjectiles = new SamusBombProjectileSystem(flashbackAudioRequests, powerBombGlowColors);
        flashbackProjectiles = new SamusProjectileSystem(flashbackAudioRequests, new BeamChargeState(), samusOverreads,
            new LeftMuzzleSubpixelCarry(SamusProjectileSystem.PlaceCount));

        subtitles = new IntroJapaneseSubtitles(japaneseText);
        this.audio = Ensure.NotNull(audio);
        CharacterArtwork = characterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic requires installed artwork.");

        // Setup loads the sprite sheets the intro binds; the narration, the first narration card, the scene pages and
        // the portrait are installed pictures (#1467).
        screenSprites = new ScreenSprites();

        // The game's setup loads the ordinary beam colours before the full intro colours replace them, so the flashback
        // Samus shows the opening's colours from the start (her intro shades); the flashback projectile pages bind the
        // beam's pictures.

        // The loads above are invisible: the intro stays black until its fade.
        // The setup update itself runs as the next update's cinematic phase.
        Phase = IntroCinematicPhase.Initial;
    }

    /// <summary>Gets the current opening-cinematic state-machine phase.</summary>
    public IntroCinematicPhase Phase { get; private set; }

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
        bool explosionsExistedBeforeThisFrame = FlashbackMotherBrainExplosions is not null;
        frameCounter++;
        crossfadeTick = null;
        switch (Phase)
        {
            case IntroCinematicPhase.Initial:
                // Stop the music, then load the opening music data.
                audio.QueueMusic(MusicRequest.Stop);
                audio.QueueMusic(
                    MusicRequest.LoadDataSet(MusicDataSet.LastMetroidInCaptivity));
                Phase = IntroCinematicPhase.WaitForInitialMusicQueue;
                break;

            case IntroCinematicPhase.WaitForInitialMusicQueue:
                // A request queued with the shortest delay waits eight updates before HasQueuedMusic clears.
                if (MusicQueueFinished())
                {
                    Phase = IntroCinematicPhase.FadeInFirstNarration;
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicPhase.FadeInFirstNarration:
                if (fade.Brighten())
                {
                    Phase = IntroCinematicPhase.LastMetroidIsInCaptivity;
                    timer = IntroCinematicDefinitions.Timing.FirstLineHold;
                }
                break;

            case IntroCinematicPhase.LastMetroidIsInCaptivity:
                if (--timer <= 0)
                {
                    audio.QueueMusic(MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.CaptivityLineSong));
                    Phase = IntroCinematicPhase.GalaxyIsAtPeace;
                    timer = IntroCinematicDefinitions.Timing.CaptivityLineHold;
                }
                break;

            case IntroCinematicPhase.GalaxyIsAtPeace:
                if (--timer <= 0)
                {
                    // The next music requests wait on the music queue before the
                    // documented four-second hold; the shortest delay is their slot.
                    audio.QueueMusic(MusicRequest.Stop);
                    audio.QueueMusic(
                        MusicRequest.LoadDataSet(MusicDataSet.GalaxyAtPeace));
                    audio.ScheduleMusic(
                        MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.PeaceLineSong),
                        IntroCinematicDefinitions.Music.SceneTrackDelay);
                    Phase = IntroCinematicPhase.WaitForSecondMusicQueue;
                }
                break;

            case IntroCinematicPhase.WaitForSecondMusicQueue:
                if (MusicQueueFinished())
                {
                    Phase = IntroCinematicPhase.FourSecondHold;
                    timer = IntroCinematicDefinitions.Timing.FourSecondHold;
                }
                break;

            case IntroCinematicPhase.FourSecondHold:
                if (--timer <= 0)
                {
                    Phase = IntroCinematicPhase.FadeOutFirstNarration;
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicPhase.FadeOutFirstNarration:
                // The fade-out installs page one's setup, which runs in the next update.
                if (fade.Darken())
                    Phase = IntroCinematicPhase.SetupPageOne;
                break;

            case IntroCinematicPhase.SetupPageOne:
                SetupFirstIllustratedPage();
                break;

            case IntroCinematicPhase.WaitForPageOneMusicQueue:
                // The scene song scheduled fourteen updates ahead is the longest request issued by
                // page one's setup, so HasQueuedMusic becomes clear after fourteen update slots.
                if (MusicQueueFinished())
                {
                    Narration.StartEnglishPageOne();
                    Phase = IntroCinematicPhase.FadeInPageOne;
                    fade.StepEvery(IntroCinematicDefinitions.Fade.NarrationUpdatesPerStep);
                }
                break;

            case IntroCinematicPhase.FadeInPageOne:
                if (fade.Brighten())
                    Phase = IntroCinematicPhase.PageOneText;
                break;

            case IntroCinematicPhase.PageOneText:
                break;

            case IntroCinematicPhase.PageOneAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupMotherBrainFlashback();
                break;

            case IntroCinematicPhase.MotherBrainCrossfade:
                StepMotherBrainCrossfade();
                break;

            case IntroCinematicPhase.MotherBrainFlashback:
                if (CurrentFlashbackMotherBrain?.PageTwoRequested == true)
                    SetupPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoCrossfade:
                StepPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoText:
                break;

            case IntroCinematicPhase.PageTwoAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscoveryCrossfade:
                StepBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscovery:
                if (CurrentBabyDiscovery?.PageThreeRequested == true)
                    SetupPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeCrossfade:
                StepPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeText:
                break;

            case IntroCinematicPhase.PageThreeAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyMetroidDelivery();
                break;

            case IntroCinematicPhase.BabyMetroidDeliveryCrossfade:
                StepBabyMetroidDeliveryCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidDelivery:
                if (CurrentScientistCutscene?.PageFourRequested == true)
                    SetupPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourCrossfade:
                StepPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourText:
                break;

            case IntroCinematicPhase.PageFourAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyMetroidExamination();
                break;

            case IntroCinematicPhase.BabyMetroidExaminationCrossfade:
                StepBabyMetroidExaminationCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidExamination:
                if (CurrentScientistCutscene?.PageFiveRequested == true)
                    SetupPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveCrossfade:
                StepPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveText:
                break;

            case IntroCinematicPhase.PageFiveAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupPageSix();
                break;

            case IntroCinematicPhase.StartPageSix:
                StartPageSix();
                break;

            case IntroCinematicPhase.PageSixText:
                break;

            case IntroCinematicPhase.IntroFadeOut:
                // The intro's end fades out and finishes once the screen is black.
                if (fade.Darken())
                {
                    // The Ceres approach immediately replaces the narration display setup with the
                    // Ceres turning-floor scene. Give that scene its own display, colour, and
                    // brightness owner: the narration has just deliberately reached
                    // a blanked screen, while the approach restores full brightness only after
                    // the fourteen-frame delayed music request has completed.
                    CurrentCeresFlight = new IntroCeresFlightState(audio,
                        CharacterArtwork?.CeresFlight, subtitles.Enabled,
                        SceneSheets);
                    Phase = IntroCinematicPhase.CeresFlight;
                }
                break;

            case IntroCinematicPhase.CeresFlight:
                CeresFlight.Step();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroCinematicPhase)} {Phase}.");
        }

        if (CurrentNarration is not null)
        {
            CurrentNarration.Step(controller.NewlyPressed);
            if (CurrentNarration.PageOneAwaitingInput)
            {
                if (Phase == IntroCinematicPhase.PageOneText)
                    Phase = IntroCinematicPhase.PageOneAwaitingInput;
            }
            if (CurrentNarration.PageTwoAwaitingInput && Phase == IntroCinematicPhase.PageTwoText)
                Phase = IntroCinematicPhase.PageTwoAwaitingInput;
            if (CurrentNarration.PageThreeAwaitingInput && Phase == IntroCinematicPhase.PageThreeText)
                Phase = IntroCinematicPhase.PageThreeAwaitingInput;
            if (CurrentNarration.PageFourAwaitingInput && Phase == IntroCinematicPhase.PageFourText)
                Phase = IntroCinematicPhase.PageFourAwaitingInput;
            if (CurrentNarration.PageFiveAwaitingInput && Phase == IntroCinematicPhase.PageFiveText)
                Phase = IntroCinematicPhase.PageFiveAwaitingInput;
            if (CurrentNarration.IntroFinishRequested && Phase == IntroCinematicPhase.PageSixText)
            {
                // Page six starts the final fade-out; the screen on this request frame is still full-brightness page six.
                fade.StepEvery(IntroCinematicDefinitions.Fade.FinishUpdatesPerStep);
                Phase = IntroCinematicPhase.IntroFadeOut;
            }
        }

        // The cinematic step runs Samus's input and movement phases, ahead of the sprite pass, only while the intro shows her.
        if (CurrentFlashbackSamus is not null && flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            // The intro demo's input phase refreshes the live radius before projectiles and movement.
            // Pose commits at the end of the previous frame deliberately leave it unchanged.
            SamusPoseMetadata.RefreshCollisionRadii(CurrentFlashbackSamus);

            // Samus's update for her current state runs the demo input,
            // after the cinematic phase, so a demo loaded by the scene setup plays its first
            // entry in the same update, ahead of the Samus pose input that consumes it.
            if (CurrentFlashbackDemoInput is not null)
                StepMotherBrainDemo();

            StepMotherBrainFlashbackSamus();
        }

        // The cinematic game state handles cinematic sprites after the active cinematic phase and
        // after Samus/projectiles. Mother Brain can therefore consume a missile at its new
        // position during this same frame, exactly as the game does.
        if (CurrentFlashbackMotherBrain is not null)
        {
            CurrentFlashbackMotherBrain.RunBehavior(
                frameCounter,
                CrossfadeFinishing);
            // Deleting Mother Brain after the page-two crossfade hides Samus from
            // the next update on.
            if (!CurrentFlashbackMotherBrain.IsVisible)
                flashbackSamusDisplay = IntroSamusDisplay.Hidden;
            if (CurrentFlashbackMotherBrain.TakeMissileHit(flashbackProjectiles.EnemyImpacts, flashbackBombProjectiles))
            {
                FlashbackMotherBrainExplosions = new IntroMotherBrainExplosionSystem();
                FlashbackMotherBrainExplosions.SpawnFourthHitExplosions();
            }
        }

        if (FlashbackRinkas is not null && CurrentFlashbackSamus is not null)
        {
            FlashbackRinkas.Step(
                CurrentFlashbackSamus,
                motherBrainExploding: CurrentFlashbackMotherBrain?.ExplosionStarted == true,
                explosionsAllocated: FlashbackMotherBrainExplosions is not null);
        }

        // This ordering makes a newly spawned timer-one actor select its first frame
        // on the same accepted frame as the page-one input transition.
        CurrentFlashbackMotherBrain?.Step();
        if (explosionsExistedBeforeThisFrame)
        {
            // The eight actors are allocated while the first actor is already being processed,
            // so their first cinematic sprite update occurs on the following frame. Hit count
            // four is reached on the spawn frame, after the update passed their free slots.
            FlashbackMotherBrainExplosions?.Step(CrossfadeFinishing);
        }

        // The cinematic sprites update after the cinematic phase, so the
        // actors a scene setup spawns take their first step in that same update.
        CurrentBabyDiscovery?.Step(frameCounter, CrossfadeFinishing);
        CurrentScientistCutscene?.Step(crossfadeTick);

        // The cinematic's game state runs the colour effects after its actors. The fight lights' own check ends
        // them on the update Mother Brain starts page two, before their countdown runs: the record they showed stays on the
        // room's colours until page two's crossfade recolours the room on the next update (#1733).
        if (Phase != IntroCinematicPhase.MotherBrainFlashback || CurrentFlashbackMotherBrain?.PageTwoRequested != true)
            ColourEffects.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
    }

    /// <summary>Installs Mother Brain room collision, actors, Samus demo state, and the incoming gameplay colours.</summary>
    internal void SetupMotherBrainFlashback()
    {
        // The shared scene setup places the persistent narration caret off screen.
        // It remains alive and will be restored by the next text-page setup. Starting the cross-fade turns the page,
        // which deletes the Japanese text's next-page arrow.
        Narration.HideCaret();
        Narration.AcceptPageTurn();

        // The discovery setup shows the Mother Brain room page. Its 32x32 visual map was already
        // loaded with the initial artwork; the room's block data below is gameplay
        // collision data and must never be expanded as a visual block map.
        CurrentFlashbackSamus = new SamusState(flashbackAudioRequests, samusOverreads.FramesBeyondAnimation, UnauthoredBoostCadence,
            SamusCompatibility.ForNewSamus(), flashbackProjectiles.Charge, flashbackSamusArtwork)
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(IntroCinematicDefinitions.Flashback.SamusPosition),
            SelectedHudItem = HudItem.Missile,
            Missiles = IntroCinematicDefinitions.Flashback.Missiles,
            MaxMissiles = IntroCinematicDefinitions.Flashback.Missiles,
        };
        // Samus's shared frame phases read the active cinematic phase to select
        // intro colour restoration and suppress gameplay-only sound/landing effects.
        CurrentFlashbackSamus.LiquidPhysics.CinematicActive = true;
        SamusPoseMetadata.RefreshCollisionRadii(CurrentFlashbackSamus);
        SamusPresentation.InitializeAnimation(CurrentFlashbackSamus);
        CurrentFlashbackSamus.CommitPoseHistory();
        CurrentFlashbackMotherBrain = new IntroMotherBrainSpriteState();
        FlashbackMotherBrainExplosions = null;
        FlashbackRinkas = new IntroRinkaSystem();

        // The flashback's collision room is built from its own terrain blocks, so the
        // visual room page never stands in for the collision contract.
        CurrentFlashbackLevel = CreateMotherBrainLevel();
        flashbackProjectiles.Reset();
        CurrentFlashbackDemoInput = new DemoInputState();
        CurrentFlashbackDemoInput.Clear();
        CurrentFlashbackDemoInput.Enable();
        CurrentFlashbackDemoInput.Load(DemoInputRecordingId.IntroMotherBrainFlashback);
        // Samus is processed, and drawn ahead of the cinematic actors.
        flashbackSamusDisplay = IntroSamusDisplay.SamusFirst;

        // The scene setup starts the crossfade from the narration into the flashback, which starts black.
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Flashback);

        Phase = IntroCinematicPhase.MotherBrainCrossfade;
    }

    /// <summary>Advances 32 component updates of the text-to-gameplay fade before exposing the Mother Brain room.</summary>
    private void StepMotherBrainCrossfade()
    {
        if (!AdvanceCrossfade())
            return;

        // Transition completion selects the visible layers and blanks the English narration text. The
        // ornamental top and bottom rows survive.
        ClearVisibleNarrationText();
        StartFightLightsIfSamusDrawsFirst(flashbackSamusDisplay);
        Phase = IntroCinematicPhase.MotherBrainFlashback;
    }

    /// <summary>
    /// The end of a crossfade into a flashback starts the old Mother Brain fight's background lights (#1733) while
    /// <paramref name="display"/>, how the intro shows Samus, has Samus draw first: the Mother Brain flashback sets it so, the
    /// discovery's setup has its actors draw first.
    /// </summary>
    private void StartFightLightsIfSamusDrawsFirst(IntroSamusDisplay display)
    {
        if (display == IntroSamusDisplay.SamusFirst)
            ColourEffects.Start(IntroCinematicDefinitions.FightLights, 0);
    }

    /// <summary>Starts the second narration page while restoring its caret and fading back from gameplay colors.</summary>
    private void SetupPageTwoCrossfade()
    {
        // Page two spawns its narration, selects the reverse gameplay-to-
        // text colour step, and restores the already allocated caret.
        Narration.StartEnglishPageTwo();
        // The crossfade recolours every colour from the opening's own, so the fight lights' last record no longer shows.
        ColourEffects = new ColourEffectSystem(new ColourEffectSessionState());
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Flashback, IntroCrossfadeScene.Narration);
        Phase = IntroCinematicPhase.PageTwoCrossfade;
    }

    /// <summary>Completes the Mother Brain-to-page-two colour transition.</summary>
    private void StepPageTwoCrossfade()
    {
        // The reverse crossfade is the exact inverse of the earlier transition: text/portrait ranges
        // fade in while Mother Brain gameplay, projectile, and sprite ranges fade out.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageTwoText);
    }

    /// <summary>Fades gameplay and projectile ranges out while restoring portrait and text ranges.</summary>
    private void StepReverseGameplayToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if (AdvanceCrossfade())
            Phase = completedPhase;
    }

    /// <summary>Replaces Mother Brain actors with SR388 egg and baby owners and selects the second gameplay room page.</summary>
    internal void SetupBabyDiscoveryCrossfade()
    {
        Narration.HideCaret();
        Narration.AcceptPageTurn();

        // The discovery setup shows the second authored room page, declares a
        // 32x16 collision room, carries Samus into new egg/baby/demo owners, and reuses the same
        // text-to-gameplay colour crossfade as the Mother Brain scene.
        CurrentBabyDiscovery = new IntroBabyDiscoveryState(Random, CurrentFlashbackSamus
            ?? throw new InvalidOperationException("Discovery setup requires the preceding flashback Samus state."), shellFallPastCurve, audio);

        // The old gameplay actors have reached their delete animations. Releasing the scoped
        // references prevents their projectile and collision steps from running behind the
        // new scene while preserving the shared artwork and colours they intentionally reuse.
        CurrentFlashbackSamus = null;
        CurrentFlashbackMotherBrain = null;
        FlashbackMotherBrainExplosions = null;
        FlashbackRinkas = null;
        CurrentFlashbackDemoInput = null;
        CurrentFlashbackLevel = null;
        flashbackProjectiles.Reset();

        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Flashback);
        Phase = IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    /// <summary>Reuses the text-to-gameplay fade before showing the SR388 discovery room alone.</summary>
    internal void StepBabyDiscoveryCrossfade()
    {
        if (!AdvanceCrossfade())
            return;

        // The crossfade's end clears the English text region after the final fade update. The
        // second room page is already shown, so the SR388 room becomes the sole room page.
        ClearVisibleNarrationText();
        StartFightLightsIfSamusDrawsFirst(BabyDiscovery.SamusDisplay);
        Phase = IntroCinematicPhase.BabyDiscovery;
    }

    /// <summary>Starts the third narration page while SR388 actors continue updating beneath its text fade.</summary>
    private void SetupPageThreeCrossfade()
    {
        // The egg's page-three step selects page three. On the next cinematic-phase call page three
        // spawns its narration and enters the same gameplay-to-text colour
        // step used after Mother Brain. The SR388 actors continue running beneath it.
        Narration.StartEnglishPageThree();
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Flashback, IntroCrossfadeScene.Narration);
        Phase = IntroCinematicPhase.PageThreeCrossfade;
    }

    /// <summary>Runs the shared gameplay-to-text transition for the SR388 discovery scene.</summary>
    private void StepPageThreeCrossfade()
    {
        // The reverse crossfade is shared by pages two and three. The named outer phase keeps
        // showing the SR388 actors until its final step shows the text page alone again.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageThreeText);
    }

    /// <summary>Starts the first laboratory scene with its rightward page pan and separate scientist fade timer.</summary>
    internal void SetupBabyMetroidDelivery()
    {
        // Scientist crossfades use the sibling setup, which performs
        // the same off-screen caret move before changing the visible layer configuration.
        Narration.HideCaret();
        Narration.AcceptPageTurn();

        // The delivery setup shows the laboratory page, starts it 32 pixels right with vertical scroll eight, and
        // spawns the delivered baby before starting the crossfade into the laboratory. Its picture is already in the
        // intro's artwork.
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateDelivery();
        CurrentBabyDiscovery = null;

        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Laboratory);
        Phase = IntroCinematicPhase.BabyMetroidDeliveryCrossfade;
    }

    /// <summary>Fades page-three text and portrait ranges into the delivery laboratory colours.</summary>
    internal void StepBabyMetroidDeliveryCrossfade()
    {
        // The scientist crossfade fades the narration away while bringing in the laboratory and its scientists.
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidDelivery);
    }

    /// <summary>Advances the crossfade from the narration into the laboratory.</summary>
    private void StepTextToScientistCrossfade(IntroCinematicPhase completedPhase)
    {
        if (!AdvanceCrossfade())
            return;

        ClearVisibleNarrationText();
        Phase = completedPhase;
    }

    /// <summary>Starts page four and the alternate-timer fade back from the delivery laboratory.</summary>
    private void SetupPageFourCrossfade()
    {
        // The baby's page-four step selects page four, which starts its text and the crossfade back from the laboratory.
        Narration.StartEnglishPageFour();
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Laboratory, IntroCrossfadeScene.Narration);
        Phase = IntroCinematicPhase.PageFourCrossfade;
    }

    /// <summary>Restores text and portrait colors while fading the delivery laboratory out.</summary>
    private void StepPageFourCrossfade()
    {
        // The scientist-specific inverse: the narration fades in as the laboratory and its scientists fade out.
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFourText);
    }

    /// <summary>Advances the crossfade from the laboratory back into the narration.</summary>
    private void StepScientistToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if (AdvanceCrossfade())
            Phase = completedPhase;
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
    private void SetupBabyMetroidExamination()
    {
        Narration.HideCaret();
        Narration.AcceptPageTurn();

        // The examination setup shows the examination page, starts the page at Y=-24, and spawns the examined baby.
        // Its setup is otherwise the same scientist crossfade as delivery.
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateExamination();
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Narration, IntroCrossfadeScene.Laboratory);
        Phase = IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    /// <summary>Reuses the text-to-scientist fade for the examination scene.</summary>
    private void StepBabyMetroidExaminationCrossfade() =>
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidExamination);

    /// <summary>Starts page five after the baby examination scene and selects the reverse colour transition.</summary>
    private void SetupPageFiveCrossfade()
    {
        Narration.StartEnglishPageFive();
        crossfade = new IntroCrossfade(IntroCrossfadeScene.Laboratory, IntroCrossfadeScene.Narration);
        Phase = IntroCinematicPhase.PageFiveCrossfade;
    }

    /// <summary>Reuses the scientist-to-text fade for the fifth narration page.</summary>
    private void StepPageFiveCrossfade() =>
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFiveText);

    /// <summary>Seeds the final narration page's timer, preserving the English and Japanese pages' difference in timing.</summary>
    private void SetupPageSix()
    {
        // Page six's setup turns the page, which deletes the Japanese text's next-page arrow. English falls straight
        // through into page six; Japanese text selects page six as the next update's cinematic phase.
        Narration.AcceptPageTurn();
        if (subtitles.Enabled)
            Phase = IntroCinematicPhase.StartPageSix;
        else
            StartPageSix();
    }

    /// <summary>Replaces the page text, caret, and eye animation without a colour transition.</summary>
    private void StartPageSix()
    {
        // Page six has no colour transition. Only the English text region, caret, eye
        // animation, and final page are replaced.
        Narration.StartEnglishPageSix();
        CurrentScientistCutscene = null;
        Phase = IntroCinematicPhase.PageSixText;
    }

    /// <summary>Runs the scripted Samus gameplay, projectiles, collisions, and colour updates for the Mother Brain flashback.</summary>
    internal void StepMotherBrainDemo()
    {
        // The update's requests, which the frontend's queues already hold, leave the flashbacks' record.
        flashbackAudioRequests.BeginUpdate();
        FlashbackDemoInput.Step(HandleMotherBrainDemoCue);

        // The HUD and projectile step calls the shared cooldown owner before
        // its ordinary-projectile half. Standing facing left cannot place bombs, but running
        // the real owner is still required for missile admission timing.
        flashbackBombProjectiles.StepFrame(new SamusTerrain { Level = FlashbackLevel, RoomObjects = IntroCollisionRoomObjects.Instance }, FlashbackSamus, FlashbackDemoInput.Held, FlashbackDemoInput.NewlyPressed);
        // A missile's firing sound joins the frontend's queues as the shot fires.
        flashbackProjectiles.StepFrame(new SamusTerrain { Level = FlashbackLevel, RoomObjects = IntroCollisionRoomObjects.Instance }, FlashbackSamus, new ProjectileButtonInput { ControllerInput = FlashbackDemoInput.Held, ControllerNewInput = FlashbackDemoInput.NewlyPressed, ControllerPreviousNewInput = FlashbackDemoInput.PublishedPreviousNewlyPressed }, cameraPosition: Position16<RoomPixels>.Zero, sharedProjectiles: flashbackBombProjectiles);

        // Both the demo and locked movement modes end by resetting the
        // movement and pose-change state, so a later missile inherits only the
        // movement recorded in its own update.
        SamusAerialPoseTransitions.DropPendingTransitionShot(FlashbackSamus);
        FlashbackSamus.ProjectileInheritance.ClearMovement();
    }

    /// <summary>
    /// Runs the movement/animation/interruption/colour slice of the intro demo's
    /// movement phase, followed by the timer owner.
    /// </summary>
    private void StepMotherBrainFlashbackSamus()
    {
        SamusState samus = FlashbackSamus;
        RoomLevelData level = FlashbackLevel;
        GameplayAction demoInput = CurrentFlashbackDemoInput?.Held ?? 0;

        // Each frame shows the definitions selected by the previous Samus draw. This
        // is what makes pose/frame changes use their corresponding artwork rather than
        // reading stale standing pictures as a hurt body on the next visible frame.

        AerialMovementResult? fallingMovement = null;
        BlockMoveResult? hurtEndingProbe = null;
        bool transitionAccepted = false;
        bool groundedDemoStepped = false;
        if (samus.KnockbackActive)
        {
            // The intro demo's movement phase runs the knockback movement that owns Samus. Its shared
            // vertical calculation now carries the eleven-frame Rinka arc through its apex.
            SamusKnockbackMovement.Step(level, samus, frameCounter, IntroCollisionRoomObjects.Instance);
        }
        else if (samus.Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose)
        {
            hurtEndingProbe = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
                level, samus, frameCounter, IntroCollisionRoomObjects.Instance);
        }
        else if (CurrentFlashbackDemoInput?.Enabled == true &&
            (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose)))
        {
            // The demo's later run/jump records use the same input/movement order as the
            // discovery scene. This coordinator already animates and commits its pose.
            IntroSamusDemoMovement.StepGroundedLeft(level, samus, demoInput,
                CurrentFlashbackDemoInput.NewlyPressed, frameCounter, Random);
            groundedDemoStepped = true;
        }
        else if (samus.Pose == SamusPoseId.SpinJumpLeftPose)
        {
            fallingMovement = SamusAerialMovement.StepSpinJump(level, samus,
                demoInput, frameCounter, CurrentFlashbackDemoInput?.NewlyPressed ?? 0, IntroCollisionRoomObjects.Instance);
        }
        else if (SamusPoseMetadata.IsLeftFacingNormalJumpPose(samus.Pose))
        {
            fallingMovement = SamusAerialMovement.StepNormalJump(level, samus,
                demoInput, frameCounter, IntroCollisionRoomObjects.Instance);
        }
        else if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose)
        {
            // When knockback ends humanoid knockback it selects the right or left falling pose and restores
            // normal movement. The following frames therefore run Samus's ordinary fall until the room's
            // collision reports the laboratory floor.
            fallingMovement = SamusAerialMovement.StepFalling(

                level,
                samus,
                demoInput,
                frameCounter,
                IntroCollisionRoomObjects.Instance);
        }

        // Intro-demo state is not an animation shortcut. The intro demo's movement phase advances the same
        // animation delays as gameplay, including every visible hurt/fall frame.
        if (!groundedDemoStepped)
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { ControllerInput = demoInput, FrameCounter = frameCounter, Random = Random });

        // The pose animation's transition steps publish a prospective pose; they are not ordinary frame
        // delays. The intro demo's movement phase consumes that publication before the value following the step
        // can ever be misread as another animation duration. This completes landing art's
        // transition from NormalLandingLeftPose to FacingLeftNormalPose instead of walking beyond its pose animation.
        bool animationTransitionApplied = SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus);
        animationTransitionApplied |= SamusKnockbackMovement.TryFinishExpiredHitInterruption(samus);
        transitionAccepted |= animationTransitionApplied;

        if (!animationTransitionApplied && hurtEndingProbe is { IsUnobstructedDownwardMovement: true })
        {
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus, level, SamusPoseTransitions.SelectFallingPoseForCurrentAim(samus), frameCounter, IntroCollisionRoomObjects.Instance);
            animationTransitionApplied = transitionAccepted = true;
        }

        // Samus's ordinary update resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation all come from the same landing implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true })
        {
            SamusPoseTransitions.ApplyAerialLanding(samus, wasSpinning: samus.Pose == SamusPoseId.SpinJumpLeftPose, demoInput);
            transitionAccepted = true;
        }

        // The intro demo's movement phase consumes the knockback timer the first Rinka's hit published on the
        // preceding cinematic-sprite pass. Keeping this after animation matches Samus's update during the intro demo:
        // the hit frame finishes its standing animation before the knockback start installs the knockback pose.
        if (!samus.KnockbackActive &&
            samus.KnockbackTimer != 0 &&
            samus.KnockbackDirection == Samus.Movement.SamusKnockbackDirection.None)
        {
            SamusKnockbackMovement.Start(samus, demoInput, KnockbackLaunch.FromPendingHit(samus), poseCollision: null);
            transitionAccepted = true;
        }

        // The intro uses the ordinary transition epilogue after movement, animation,
        // and hurt interruption. Publish only the final accepted pose, once per slot;
        // an unchanged animation frame must not erase the preceding transition history.
        if (transitionAccepted)
            samus.CommitPoseHistory();

        // The intro demo runs Samus's ordinary colouring even in intro-demo state.
        // This supplies the alternating hurt colors and eventual suit-colour restoration;
        // invincibility flicker remains independently enforced by Samus.Draw.
        SamusHurtFlash.Update(flashbackSamusColours, samus, demoInput);

        // The cinematic ages Samus's two hurt timers after both of her state updates, but before the
        // cinematic-actor walker can publish a new Rinka collision later in this frame.
        samus.DecrementHurtTimers();
    }

    /// <summary>Runs the Mother Brain demo's end step, freezing input after its prescribed Samus pose change.</summary>
    private void HandleMotherBrainDemoCue(DemoInputCue cue)
    {
        if (cue != DemoInputCue.EndIntroMotherBrainFlashback)
            throw new InvalidOperationException($"The Mother Brain flashback demo handles no {cue} cue.");

        // The flashback's end locks Samus standing facing left, reinitializes the pose/animation bookkeeping,
        // disables demo publication, and then returns to the demo's animation. The next
        // step is the shared delete, so the actor is removed on the same
        // update. Freezing input after the exact pose change gives the remaining Mother Brain explosion
        // frames the standing Samus the original gives them once her demo stops driving her.
        FlashbackSamus.Pose = SamusPoseId.FacingLeftNormalPose;
        SamusPoseMetadata.RefreshCollisionRadii(FlashbackSamus);
        // A Samus already standing facing left keeps her running animation.
        SamusPresentation.SetAnimationFrameIfPoseChanged(FlashbackSamus);
        FlashbackSamus.CommitPoseHistory();
        FlashbackDemoInput.Disable();
    }

    /// <summary>Builds the flashback's room: collision only, which the flashback's own pictures show.</summary>
    private static RoomLevelData CreateMotherBrainLevel() => RoomLevelData.CollisionOnly(
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
    internal SpriteFrameBuffer PrepareMotherBrainSprites()
    {
        // Both direct rendering and detached capture enter here once per display.
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        // Samus shown over the cinematic actors draws her and both projectile passes before the
        // cinematic actors; once deleting Mother Brain clears it, only the actors remain.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            FlashbackSamus.Presentation.Draw(FlashbackSamus, sprites, cameraPosition: Position16<RoomPixels>.Zero, frameCounter);
            // The flashback's artwork holds the power beam's pictures whatever beam its Samus fires.
            var projectileArtwork = new ProjectileDrawArtwork(projectileSheets);
            flashbackProjectiles.DrawLiveProjectiles(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
            flashbackProjectiles.AdvanceTrails(timeIsFrozen: false);
            flashbackProjectiles.DrawTrails(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
            flashbackProjectiles.DrawExplosions(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
        }
        FlashbackRinkas?.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroRinka));
        FlashbackMotherBrainExplosions?.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroMotherBrainExplosion));
        // The blank pose draws nothing.
        if (FlashbackMotherBrain.IsVisible && FlashbackMotherBrain.Pose is { } motherBrainPose &&
            motherBrainPose != CinematicSpritePose.Blank)
        {
            // Samus shown over the cinematic actors makes the intro's sprite draw put Samus first and cinematic
            // actors afterward. Retaining that drawing order preserves overlap wins.
            IntroSprites(CinematicSpriteFamily.IntroMotherBrain).Draw(
                motherBrainPose, screenSprites,
                IntroMotherBrainSpriteState.Position,
                FlashbackMotherBrain.Shading(crossfade.ActorShading(IntroMotherBrainSpriteState.Colouring)));
        }
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Orders Samus and discovery actors by how the intro shows Samus before finalizing sprite data.</summary>
    internal SpriteFrameBuffer PrepareBabyDiscoverySprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        // The cinematic draw layers Samus and the cinematic actors by how the intro shows her.
        IntroSamusDisplay display = BabyDiscovery.SamusDisplay;
        if (display == IntroSamusDisplay.SamusFirst)
            DrawBabyDiscoverySamus(sprites);
        screenSprites.BeginFrame();
        BabyDiscovery.DrawActors(screenSprites, IntroSprites(CinematicSpriteFamily.IntroEggEffect),
            IntroSprites(CinematicSpriteFamily.IntroDiscoveryActor));
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        if (display == IntroSamusDisplay.ActorsFirst)
            DrawBabyDiscoverySamus(sprites);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Draws Samus in screen-relative coordinates as the demo brings her in from the right.</summary>
    private void DrawBabyDiscoverySamus(SpriteFrameBuffer sprites)
    {
        // This cinematic deliberately keeps the camera's X at zero. Samus starts at X 376,
        // outside the 256-pixel viewport, and the demo makes her enter from the right; the
        // second room page shows different art, not a hidden camera coordinate of 256.
        BabyDiscovery.Samus.Presentation.Draw(BabyDiscovery.Samus, sprites, cameraPosition: Position16<RoomPixels>.Zero, frameCounter);
    }

    /// <summary>Collects the scientist scene's actors for the current cinematic display.</summary>
    internal SpriteFrameBuffer PrepareScientistSprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        ScientistCutscene.Draw(screenSprites, IntroSprites(CinematicSpriteFamily.IntroScientist));
        screenSprites.FinishFrame();
        foreach (SpriteDraw draw in screenSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();
        return sprites;
    }

    /// <summary>Collects the persistent caret, the Japanese text's next-page arrow and illustrated-page cinematic actors in display order.</summary>
    internal SpriteFrameBuffer PrepareIllustratedPageSprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        screenSprites.BeginFrame();
        // The caret's blank pose, shown while it blinks off, draws nothing.
        if (Narration.CaretPose is { } caretPose && caretPose != CinematicSpritePose.Blank)
        {
            // The persistent caret begins at its narration place, goes off screen during illustrated
            // crossfades, and is restored when the next narration page starts.
            IntroSprites(CinematicSpriteFamily.IntroCaret).Draw(
                caretPose, screenSprites, Narration.Caret,
                IntroShades.Drawn(IntroSpriteColouring.CrossFadeActors));
        }
        // The Japanese text's next-page arrow is drawn after the caret, which comes first (#1655).
        Narration.NextPageArrow?.Draw(screenSprites, installedArt: IntroSprites(CinematicSpriteFamily.IntroNextPageArrow));
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
        CurrentNarration = new IntroNarrationActors(
            narrationText,
            subtitles,
            audio,
            NarrationPresentation);
        audio.QueueMusic(MusicRequest.Stop);
        audio.QueueMusic(
            MusicRequest.LoadDataSet(MusicDataSet.Intro));
        audio.ScheduleMusic(
            MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.IntroductionSong),
            IntroCinematicDefinitions.Music.SceneTrackDelay);
        Phase = IntroCinematicPhase.WaitForPageOneMusicQueue;
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

    /// <summary>Prepares the current display once and owns all inputs needed by a detached renderer.</summary>
    internal LayeredScene CaptureTranslatedRenderSnapshot()
    {
        if (CurrentCeresFlight is not null && Phase == IntroCinematicPhase.CeresFlight)
            return CurrentCeresFlight.CaptureRenderSnapshot();

        SpriteFrameBuffer sprites;
        PicturedPlaneLayer room;
        if (Phase is IntroCinematicPhase.MotherBrainCrossfade or
            IntroCinematicPhase.MotherBrainFlashback or IntroCinematicPhase.PageTwoCrossfade)
        {
            sprites = PrepareMotherBrainSprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.MotherBrainRoom, FightRoomShading),
                (CurrentFlashbackMotherBrain?.Background ??
                    new Position16<RoomPixels>(0, GameplayFlashbackVerticalScroll)).AsPlaneOffset());
        }
        else if (Phase is IntroCinematicPhase.BabyDiscoveryCrossfade or
            IntroCinematicPhase.BabyDiscovery or IntroCinematicPhase.PageThreeCrossfade)
        {
            sprites = PrepareBabyDiscoverySprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.BabyDiscoveryRoom, crossfade.SceneShading), new(0, GameplayFlashbackVerticalScroll));
        }
        else if (Phase is IntroCinematicPhase.BabyMetroidDeliveryCrossfade or
            IntroCinematicPhase.BabyMetroidDelivery or IntroCinematicPhase.PageFourCrossfade or
            IntroCinematicPhase.BabyMetroidExaminationCrossfade or
            IntroCinematicPhase.BabyMetroidExamination or IntroCinematicPhase.PageFiveCrossfade)
        {
            sprites = PrepareScientistSprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePages.Laboratory(ScientistCutscene.Scene), crossfade.SceneShading), ScientistCutscene.Background);
        }
        else if (CurrentNarration is null)
        {
            // The first card is opaque and unscrolled and shows no sprites; later text is transparent
            // and cropped eight pixels down. Do not conflate those two display setups.
            return new(new FramePaint(Backdrop),
                new DisplayLayer[] { new PicturedPlaneLayer(IntroNarrationPictures.FirstCardPlane(SceneSheets, crossfade.SceneShading), default) { DrawsOpaque = true } },
                fade.Level);
        }
        else
        {
            sprites = PrepareIllustratedPageSprites();
            room = Plane(IntroScenePictures.PortraitPlane(SceneSheets, CurrentNarration.EyeFrame, crossfade.SceneShading), new(0, GameplayFlashbackVerticalScroll));
        }

        var text = new PicturedPlaneLayer(IntroNarrationPictures.PagesPlane(SceneSheets, narrationText, crossfade.SceneShading, crossfade.NarrationShading), default);
        // Back to front (#1514): the page's room, the flashback's actors over it, and the narration over both. Each picture
        // and sprite is drawn whole at its layer's place; the per-square and per-sprite-part priority tiers that once
        // interleaved them are a presentation quirk the model does not keep.
        return new(new FramePaint(Backdrop), new DisplayLayer[]
        {
            room, new SpritesLayer(FlashbackDraws(sprites.Draws)), text,
        }, fade.Level);
    }


    /// <summary>
    /// The colour the opening shows where nothing is drawn (#1512): the installed backdrop at the narration's level, as the
    /// backdrop is the first of the narration's colours.
    /// </summary>
    private RgbColor Backdrop
    {
        get
        {
            RgbColor installed = (CharacterArtwork ?? throw new InvalidOperationException(
                "Opening cinematic rendering requires installed artwork.")).Backdrop;
            int level = crossfade.LevelOf(IntroCrossfadeScene.Narration);
            return level == IntroCrossfade.Levels ? installed : new ColourLevel(level, IntroCrossfade.Levels).Apply(installed);
        }
    }

    /// <summary>
    /// <paramref name="draws"/> with the flashback Samus's draws in her shading (#1512), and every draw shown in her beams'
    /// shade also at the level the crossfade shows her beam colours at: the shading, not the kind of draw, says which
    /// draws show the faded beam colours.
    /// </summary>
    private ImmutableArray<SpriteDraw> FlashbackDraws(ImmutableArray<SpriteDraw> draws)
    {
        ImmutableArray<SpriteDraw> shaded = flashbackSamusColours.ShadeOwnDraws(draws);
        int level = crossfade.LevelOf(IntroCrossfadeScene.Narration);
        if (level == IntroCrossfade.Levels)
            return shaded;
        var beamLevel = new ColourLevel(level, IntroCrossfade.Levels);
        var faded = ImmutableArray.CreateBuilder<SpriteDraw>(shaded.Length);
        foreach (SpriteDraw draw in shaded)
        {
            faded.Add(draw is ImageSpriteDraw { Shading: { } shading } image && shading.Shades.Contains(SamusShades.IntroBeams)
                ? image with { Shading = shading.With(beamLevel) }
                : draw);
        }
        return faded.MoveToImmutable();
    }

    /// <summary>The scene plane showing <paramref name="pictures"/> at the selected scroll (#1467).</summary>
    private static PicturedPlaneLayer Plane(PicturedPlane pictures, Offset16<PlanePixels> scroll) => new(pictures, scroll);

    /// <summary>The installed sheets the scene pictures are drawn from.</summary>
    private InstalledSpriteSheets SceneSheets { get; set; }

}
