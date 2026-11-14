using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.ColourEffects;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending owner, from the escape's end through the credits and
/// post-credit result screen.
/// </summary>
/// <remarks>
/// This is deliberately a cinematic state machine rather than a video or an invented
/// slideshow. Backgrounds, pictures, palettes, sprite frames, and animation durations
/// preserve the game's behavior; editable text and fonts come from installed assets. The named phases follow the
/// game's cinematic steps one for one, so the scene steps through the same boundaries the game does.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own colour effects for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class EndingCreditsState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            InstalledArtwork.BindEndingText(artwork.MapPresentation?.EndingText);
        }
        if (!ReferenceEquals(previous.EndingBackdrops, artwork.EndingBackdrops))
            InstalledArtwork.BindBackdrops(artwork.EndingBackdrops);
        if (!ReferenceEquals(previous.SpriteSheets, artwork.SpriteSheets))
            InstalledArtwork.BindSpriteSheets(artwork.SpriteSheets);
    }
    /// <summary>Queues the authored ending music transitions.</summary>
    private readonly AudioQueues audio;
    /// <summary>Collected-item snapshot displayed with the final result.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Chooses the localized post-credits text asset.</summary>
    private readonly bool japaneseText;
    /// <summary>The ending's screen: its colours and the final page's lower text (#1358, #1515).</summary>
    internal readonly EndingScreen screen = new();
    /// <summary>Dynamic cinematic actors in slot traversal order.</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>The state of the credits scroll, which exists while that phase runs.</summary>
    internal CreditsObjectState Credits => InstalledArtwork.CurrentCredits ??
        throw new InvalidOperationException("The credits scroll is not running.");
    /// <summary>The post-credits typed text the ending advances, which exists while its phases run.</summary>
    private EndingTypewriter PostCreditsTyping => InstalledArtwork.PostCreditsText.Typing ??
        throw new InvalidOperationException("The post-credits text is not running.");
    /// <summary>Waiting updates the ending setup has made so far.</summary>
    private int setupFrameWaits;
    /// <summary>Post-credits suitless or helmet-removal gesture actor.</summary>
    internal EndingRewardGesture? CurrentRewardGesture { get; private set; }
    /// <summary>The state of the reward gesture, which exists while that phase runs.</summary>
    internal EndingRewardGesture RewardGesture => CurrentRewardGesture ??
        throw new InvalidOperationException("The reward gesture is not running.");
    /// <summary>The state of the reward jump, which exists while that phase runs.</summary>
    internal EndingRewardJump RewardJump => CreditsRoll.CurrentRewardJump ??
        throw new InvalidOperationException("The reward jump is not running.");
    /// <summary>The state of the reward shot, which exists while that phase runs.</summary>
    internal EndingPostShot PostShot => CreditsRoll.CurrentPostShot ??
        throw new InvalidOperationException("The reward shot is not running.");

    /// <summary>Zebes exploding in the ending: the finale and burst displays and the crossfade between them.</summary>
    internal EndingZebesExplosion Explosion { get; }

    /// <summary>What every ending scene draws on: brightness, the phase timer, the turning-floor view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    internal EndingStage Stage { get; }

    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars and the fade.</summary>
    internal EndingFlyaway Flyaway { get; }

    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    internal EndingArtwork InstalledArtwork { get; }

    /// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
    internal EndingEscapeScenes EscapeScenes { get; }

    /// <summary>The staff credits and what follows them: the reward copyright and palette, the white flash and the post-credits logo.</summary>
    internal EndingCreditsRoll CreditsRoll { get; }

    /// <summary>Initializes the ending and credits from the saved clear-time and ending setup snapshot.</summary>
    public EndingCreditsState(AudioQueues audio, EndingCreditsSetup setup)
    {
        this.audio = Ensure.NotNull(audio);
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        Stage = new EndingStage(setup.GameTimeHours, screen, sprites);
        InstalledArtwork = new EndingArtwork(Stage);
        Flyaway = new EndingFlyaway(setup.CrittersEscaped, screen, Stage);
        Explosion = new EndingZebesExplosion(screen, sprites, Stage, Flyaway);
        EscapeScenes = new EndingEscapeScenes(audio, setup.GameTimeHours, setup.GameTimeMinutes, screen, sprites, Stage, InstalledArtwork);
        CreditsRoll = new EndingCreditsRoll(inventory, japaneseText, screen, sprites, Stage, InstalledArtwork);
        InstalledArtwork.BindEndingText(setup.EndingText);
        Stage.Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }
    /// <summary>
    /// True while the ending's setup waits out frames of the display: the next update finishes that wait instead of
    /// starting a new update of the ending (<see cref="DisplayFrameWaitContinuation"/>).
    /// </summary>
    internal bool IsWaitingOutDisplayFrame =>
        Stage.Phase == EndingCreditsPhase.SetupEscapeFromZebes && setupFrameWaits > 0;

    /// <summary>Runs one update of the ending and credits.</summary>
    public void Step()
    {
        switch (Stage.Phase)
        {
            case EndingCreditsPhase.SetupEscapeFromZebes:
                // Each wait ends the call inside the setup phase, before the phase's
                // manual-return tail; the call after the last wait runs the scene setup.
                if (setupFrameWaits < EndingSequenceDefinitions.SetupFrameWaits)
                {
                    setupFrameWaits++;
                    return;
                }
                EscapeScenes.SetupEscapeSceneA();
                break;

            case EndingCreditsPhase.WaitForEscapeMusic:
                if (!audio.HasQueuedMusic)
                {
                    Stage.ColourEffects.Start(EndingColourEffectDefinitions.Lava, 0);
                    Stage.ColourEffects.Start(EndingColourEffectDefinitions.Crust, 0);
                    Stage.Brightness = 0;
                    Stage.RestartFadeCadence();
                    Stage.Phase = EndingCreditsPhase.FadeInEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeInEscapeSceneA:
                StepAtmosphericTransform(2);
                EscapeScenes.StepEscapeClouds();
                if (Stage.StepFastFadeIn())
                    Stage.Phase = EndingCreditsPhase.EscapeSceneA;
                break;

            case EndingCreditsPhase.EscapeSceneA:
                StepAtmosphericTransform(2);
                EscapeScenes.StepEscapeClouds();
                if (Stage.FloorZoom >= EndingMotionDefinitions.EscapeEndZoom)
                {
                    Stage.RestartFadeCadence();
                    Stage.Phase = EndingCreditsPhase.FadeOutEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneA:
                StepAtmosphericTransform(2);
                EscapeScenes.StepEscapeClouds();
                if (Stage.StepFastFadeOut())
                    EscapeScenes.SetupEscapeSceneB();
                break;

            case EndingCreditsPhase.FadeInEscapeSceneB:
                StepAtmosphericTransform(3);
                EscapeScenes.StepEscapeClouds();
                if (Stage.StepFastFadeIn())
                    Stage.Phase = EndingCreditsPhase.EscapeSceneB;
                break;

            case EndingCreditsPhase.EscapeSceneB:
                StepAtmosphericTransform(3);
                EscapeScenes.StepEscapeClouds();
                if (Stage.FloorZoom >= EndingMotionDefinitions.EscapeEndZoom)
                {
                    Stage.RestartFadeCadence();
                    Stage.Phase = EndingCreditsPhase.FadeOutEscapeSceneB;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneB:
                StepAtmosphericTransform(3);
                EscapeScenes.StepEscapeClouds();
                if (Stage.StepFastFadeOut())
                    Explosion.SetupZebesExplosion();
                break;

            case EndingCreditsPhase.FadeInZebesExplosion:
                Explosion.StepExplosionCrossfade();
                EscapeScenes.StepSprites();
                if (Stage.StepFastFadeIn())
                    Stage.Phase = EndingCreditsPhase.ZebesExplosionPaletteCrossfade;
                break;

            case EndingCreditsPhase.ZebesExplosionPaletteCrossfade:
                Explosion.StepExplosionCrossfade();
                EscapeScenes.StepSprites();
                break;

            case EndingCreditsPhase.ZebesExplosionHold:
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                    Stage.Phase = EndingCreditsPhase.ZebesExplosionAnimation;
                break;

            case EndingCreditsPhase.ZebesExplosionAnimation:
                // The explosion actor owns the remaining timing. Its music step publishes the
                // exact 120-frame post-explosion music wait, just as the game does.
                EscapeScenes.StepSprites();
                break;

            case EndingCreditsPhase.WaitForPlanetEscapeMusic:
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                {
                    audio.QueueMusic(MusicRequest.Stop);
                    audio.QueueMusic(MusicRequest.LoadDataSet(MusicDataSet.Credits));
                    audio.ScheduleMusic(
                        MusicRequest.SelectTrack(EndingSequenceDefinitions.Music.CreditsSong),
                        MusicQueueDelay.AtLeastShortest(
                            EndingSequenceDefinitions.Music.DelayArgument));
                    Stage.Phase = EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
                }
                break;

            case EndingCreditsPhase.WaitForPlanetEscapeMusicQueue:
                EscapeScenes.StepSprites();
                if (!audio.HasQueuedMusic)
                    Flyaway.SetupPlanetEscape();
                break;

            case EndingCreditsPhase.PlanetEscapeFast:
                EscapeScenes.StepSprites();
                Flyaway.StepPlanetEscapeFast();
                break;

            case EndingCreditsPhase.PlanetEscapeSlow:
                EscapeScenes.StepSprites();
                Flyaway.StepPlanetEscapeSlow();
                break;

            case EndingCreditsPhase.PlanetEscapeAccelerating:
                EscapeScenes.StepSprites();
                Flyaway.StepPlanetEscapeAccelerating();
                break;

            case EndingCreditsPhase.OperationSuccessfulText:
                EscapeScenes.StepSprites();
                break;

            case EndingCreditsPhase.FadeOutToCredits:
                EscapeScenes.StepSprites();
                if (Stage.StepFastFadeOut())
                    CreditsRoll.SetupCredits();
                break;

            case EndingCreditsPhase.Credits:
                Credits.Step();
                if (Credits.Finished)
                    CreditsRoll.SetupPostCreditsBlank();
                break;

            case EndingCreditsPhase.PostCreditsBlank:
                if (--Stage.PhaseTimer <= 0)
                {
                    Stage.RestartFadeCadence();
                    Stage.Phase = EndingCreditsPhase.PostCreditsFadeIn;
                }
                break;

            case EndingCreditsPhase.PostCreditsFadeIn:
                if (Stage.StepSlowFadeIn())
                {
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.WaitingPaletteFadeFrames;
                    CreditsRoll.ApplyWaitingBackdropPalette(0);
                    Stage.Phase = EndingCreditsPhase.PostCreditsShootingStars;
                }
                break;

            case EndingCreditsPhase.PostCreditsShootingStars:
                CreditsRoll.ApplyWaitingBackdropPalette(EndingSequenceDefinitions.Timing.WaitingPaletteFadeFrames - Stage.PhaseTimer + 1);
                if (--Stage.PhaseTimer <= 0)
                {
                    // The palette fade finishes, then the waiting phase holds the
                    // backdrop for three seconds before displaying the producer panel.
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.WaitingBackdropFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsWaitingBackdrop;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingBackdrop:
                if (--Stage.PhaseTimer <= 0)
                {
                    // The backdrop wait's end shows the complete result panel before the following 180-frame hold.
                    _ = InstalledArtwork.EndingText ?? throw new InvalidOperationException("Post-credits text requires installed ending text.");
                    InstalledArtwork.PostCreditsText.ShowResultPanel();
                    Stage.PhaseTimer = 180;
                    Stage.Phase = EndingCreditsPhase.PostCreditsWaitingSamus;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingSamus:
                if (--Stage.PhaseTimer <= 0)
                {
                    EscapeScenes.SpawnEndingRewardActors();
                    CreditsRoll.ResetRewardPalette();
                    CreditsRoll.ApplyRewardPalette();
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.RewardRevealHalfFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.PostCreditsReward:
                // Both fade halves test their counter before the decrement. Both 64-frame halves advance
                // their shared palette step on the fourth, eighth, ... calls.
                if (EndingCadenceDefinitions.RewardColourStepEveryFourthUpdate.IsDue(Stage.PhaseTimer))
                {
                    CreditsRoll.AdvanceRewardPalette();
                    CreditsRoll.ApplyRewardPalette();
                }
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                {
                    if (!CreditsRoll.RewardCopyrightShown)
                    {
                        CreditsRoll.ShowRewardCopyright();
                        break;
                    }
                    // The reward step replaces the idle actors with the gesture animations.
                    // The backgrounds are hidden during this sprite-only animation.
                    sprites.Clear();
                    CurrentRewardGesture = new EndingRewardGesture(Stage.EndingReward);
                    Stage.Phase = EndingCreditsPhase.PostCreditsGesture;
                }
                break;

            case EndingCreditsPhase.PostCreditsGesture:
                RewardGesture.Step();
                if (RewardGesture.JumpRequested)
                {
                    CurrentRewardGesture = null;
                    CreditsRoll.CurrentRewardJump = new EndingRewardJump(Stage.EndingReward);
                    Stage.Phase = EndingCreditsPhase.PostCreditsJump;
                }
                break;

            case EndingCreditsPhase.PostCreditsJump:
                RewardJump.Step();
                if (RewardJump.ShotRequested)
                {
                    CreditsRoll.CurrentPostShot = new EndingPostShot();
                    audio.QueueSound(EndingPostShotDefinitions.ShotSound, EndingPostShotDefinitions.ShotSoundLimit);
                    Stage.Phase = EndingCreditsPhase.PostCreditsShot;
                }
                break;

            case EndingCreditsPhase.PostCreditsShot:
                // The cinematic phase runs before actors; palette fades and queued
                // picture replacements therefore precede the next sprite cue.
                PostShot.Step(screen);
                RewardJump.Step();
                if (PostShot.ReadyForWhiteFlash)
                    CreditsRoll.BeginPostCreditsWhiteFlash();
                break;

            case EndingCreditsPhase.PostCreditsWhiteFlash:
                if (CreditsRoll.WhiteFlashColor > 0) CreditsRoll.WhiteFlashColor--;
                RewardJump.Step();
                if (--Stage.PhaseTimer <= 0) CreditsRoll.FinishPostCreditsWhiteFlash();
                break;

            case EndingCreditsPhase.PostCreditsLogo:
                CreditsRoll.StepPostCreditsLogo();
                break;

            case EndingCreditsPhase.PostCreditsCopyright:
                // The sprites keep animating while only the first background layer shows; hiding the actors
                // must not reset their animations or their reveal palette state.
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                {
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.RewardRevealHalfFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.ItemPercentage:
                PostCreditsTyping.Step();
                if (PostCreditsTyping.RequestedItemPercentageScroll && PostCreditsTyping.Completed)
                {
                    Stage.PhaseTimer = 40;
                    Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
                }
                break;

            case EndingCreditsPhase.ItemPercentageScrollDown:
                // The item-percentage scroll moves the shown text down two pixels per call, eighty in all, before the final
                // message starts above it.
                InstalledArtwork.PostCreditsText.Drop += EndingSequenceDefinitions.Timing.TextDropPixelsPerCall;
                if (--Stage.PhaseTimer <= 0)
                {
                    InstalledArtwork.PostCreditsText.StartFinalMessage(CreditsRoll.TypewriterSetup());
                    Stage.Phase = EndingCreditsPhase.SeeYouNextMission;
                }
                break;

            case EndingCreditsPhase.SeeYouNextMission:
                // The game parks on this final screen. There is no hidden
                // automatic reset: the console remains on this frame until reset/power-off.
                EscapeScenes.StepSprites();
                PostCreditsTyping.Step();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingCreditsPhase)} {Stage.Phase}.");
        }

        // The cinematic runs its colour effects after actors; the ending's colours show the record each holds (#1512, #1597).
        Stage.ColourEffects.StepScreen(new ColourEffectInputs { SamusY = 0, EquippedItems = 0, AreaMiniBossDefeated = false });
        if (Stage.ColourEffects.AudioRequests.SoundRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled sound.");
        // The ending follows cinematic actors even while a text-only screen hides sprites.
        // The credits' end enables the already-initialized records when the credits finish.
        if (EndingCreditsPhaseGroups.PostCredits.Contains(Stage.Phase))
            (CreditsRoll.ShootingStars ??= new EndingShootingStars()).Step();
        Flyaway.CinematicFrame++;
    }

    /// <summary>
    /// Rotates the turning floor every other cinematic call while widening its zoom by <paramref name="zoomStep"/> 256ths
    /// of a floor pixel per screen pixel.
    /// </summary>
    private void StepAtmosphericTransform(int zoomStep)
    {
        // The floor turns during both fades as well as the fully visible interval.
        if (EndingCadenceDefinitions.AtmosphereFloorTurnEverySecondUpdate.IsDue(Flyaway.CinematicFrame))
            Stage.FloorAngle = Stage.FloorAngle.TurnedBySteps(-1);
        Stage.FloorZoom = Stage.FloorZoom.Widened(zoomStep);
    }

    /// <summary>The animations showing the clear-time digits, indexed by digit.</summary>
    internal static readonly CinematicSpriteSegment[] ClearTimeDigitAnimations =
    [
        CinematicSpriteSegment.ClearTimeDigit0, CinematicSpriteSegment.ClearTimeDigit1, CinematicSpriteSegment.ClearTimeDigit2,
        CinematicSpriteSegment.ClearTimeDigit3, CinematicSpriteSegment.ClearTimeDigit4, CinematicSpriteSegment.ClearTimeDigit5,
        CinematicSpriteSegment.ClearTimeDigit6, CinematicSpriteSegment.ClearTimeDigit7, CinematicSpriteSegment.ClearTimeDigit8,
        CinematicSpriteSegment.ClearTimeDigit9,
    ];

    /// <summary>Whether the explosion currently uses its dedicated glow composition.</summary>
    internal bool UsesExplosionFinaleDisplay => (EscapeScenes.ExplosionFinaleDisplay || EscapeScenes.ExplosionBurstDisplay) && !EndingCreditsPresentation.ExplosionWhiteout(this)
        && EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Stage.Phase);

    /// <summary>
    /// The turning floor the ending's phase shows (#1515): its shot's installed picture (#1467) in the ending's colours
    /// (#1512), or none in a phase that shows no floor, when the floor layer shows nothing.
    /// </summary>
    internal AffineFloor? Floor()
    {
        EndingFloorShot shot = EndingScreenFloor.ShotOf(Stage.Phase);
        return shot == EndingFloorShot.None ? null : EndingScreenFloor.Of(InstalledArtwork.Sheets, shot)
            .Shaded(screen.Colours.SceneryShading(EndingFloorDefinitions.Scenery[shot]));
    }

    /// <summary>
    /// Captures the explosion's finale (#1514): the glow's picture behind the planet's, then the glow added over the whole
    /// screen with the debris glowing over it. The debris is drawn only as glow, never over the planet, which would punch
    /// holes in it. The per-square priority tiers that interleaved the two pictures, and the single pixel that blended where
    /// a sprite overlapped the glow, are presentation quirks the model does not keep: the glow and the debris now both add.
    /// </summary>
    internal LayeredScene CaptureExplosionFinale()
    {
        if (!EscapeScenes.ExplosionFinaleDisplay) return CaptureExplosionBurst();
        DisplayLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleGlow),
            Plane(EndingExplosionDisplayDefinitions.FinaleExplosion),
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.FinaleGlow)),
            EndingCreditsPresentation.Sprites(this, blend: SpriteBlend.Glows),
        ];
        return new(EndingCreditsPresentation.Paint(this), layers, Stage.Brightness);

        PicturedPlaneLayer Plane(EndingObjectFragmentId map) => new(ExplosionPlane(map), Offset16<PlanePixels>.Zero);
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
    /// The explosion's one-screen picture <paramref name="map"/> (#1467, #1515), placed whole at the screen's top-left: the
    /// import pictures it as the screen shows it (#1626).
    /// </summary>
    private PicturedPlane ExplosionPlane(EndingObjectFragmentId map) =>
        new([new PlacedPlanePicture(Picture(SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(map)),
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

    /// <summary>The credits roll's plane: its held rows' line pictures (#1467) in the ending's colours (#1512).</summary>
    internal PicturedPlane CreditsPlane() => CreditsObjectState.Plane(InstalledArtwork.SpriteSheet(SpriteSheetId.EndingCreditsRows),
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
    internal bool EscapeBackgroundEnabled => EndingCreditsPhaseGroups.BeforeZebesExplosionHold.Contains(Stage.Phase)
        || !EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Stage.Phase);
}
