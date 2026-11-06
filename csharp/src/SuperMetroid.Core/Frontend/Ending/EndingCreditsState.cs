using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// The ending owner, from the escape's end through the credits and
/// post-credit result screen.
/// </summary>
/// <remarks>
/// This is deliberately a cinematic state machine rather than a video or a host-authored
/// slideshow. Backgrounds, character art, palettes, spritemaps, and animation durations
/// preserve the game's behavior; editable text and fonts come from installed assets. The named phases merely replace the
/// game's <c>cinematic_function</c> entries, so the scene steps through the same boundaries the game does.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
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
            InstalledArtwork.BindPaletteFxColors(artwork.MapPresentation?.RoomPaletteFx);
            Stage.BindEndingFont(artwork.MapPresentation?.EndingFont);
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            Flyaway.BindFlightArtwork(artwork.IntroCinematicArt?.CeresFlight);
        if (!ReferenceEquals(previous.EndingFloorArt, artwork.EndingFloorArt))
            Stage.BindFloorArtwork(artwork.EndingFloorArt);
        if (!ReferenceEquals(previous.EndingObjectArt, artwork.EndingObjectArt))
            InstalledArtwork.BindObjectArtwork(artwork.EndingObjectArt);
        if (!ReferenceEquals(previous.EndingPaletteArt, artwork.EndingPaletteArt))
            InstalledArtwork.BindPaletteArtwork(artwork.EndingPaletteArt);
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
    /// <summary>The post-credits staging page.</summary>
    private readonly EndingTextPage postCreditsPage = new();
    /// <summary>The state of the credits scroll, which exists while that phase runs.</summary>
    internal CreditsObjectState Credits => InstalledArtwork.CurrentCredits ??
        throw new InvalidOperationException("The credits scroll is not running.");
    /// <summary>The state of the post-credits text, which exists while that phase runs.</summary>
    private EndingBackgroundTextState PostCreditsText => InstalledArtwork.ActivePostCreditsText ??
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

    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars, the fade and the flight artwork.</summary>
    internal EndingFlyaway Flyaway { get; }

    /// <summary>The ending's installed artwork: its text, font, staff credits, turning floor, object and palette assets.</summary>
    internal EndingArtwork InstalledArtwork { get; }

    /// <summary>The ending's escape scenes: their setup, the clouds, and the ending sprites and their cues.</summary>
    internal EndingEscapeScenes EscapeScenes { get; }

    /// <summary>The staff credits and what follows them: the reward copyright and palette, the white flash and the post-credits logo.</summary>
    internal EndingCreditsRoll CreditsRoll { get; }

    /// <summary>Initializes state-$27 from the saved clear-time and ending setup snapshot.</summary>
    public EndingCreditsState(AudioQueues audio, EndingCreditsSetup setup)
    {
        this.audio = Ensure.NotNull(audio);
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        Stage = new EndingStage(setup.GameTimeHours, screen, sprites);
        InstalledArtwork = new EndingArtwork(Stage);
        Flyaway = new EndingFlyaway(setup.CrittersEscaped, screen, sprites, Stage);
        Explosion = new EndingZebesExplosion(screen, sprites, Stage, Flyaway, InstalledArtwork);
        EscapeScenes = new EndingEscapeScenes(audio, setup.GameTimeHours, setup.GameTimeMinutes, screen, sprites, Stage, InstalledArtwork);
        CreditsRoll = new EndingCreditsRoll(inventory, japaneseText, screen, sprites, postCreditsPage, Stage, InstalledArtwork);
        InstalledArtwork.BindEndingText(setup.EndingText);
        Stage.Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }
    /// <summary>
    /// True when the next call continues the setup's wait rather than starting
    /// a new update of the ending, so it makes no per-update random advance.
    /// </summary>
    internal bool ResumesAfterFrameWait =>
        Stage.Phase == EndingCreditsPhase.SetupEscapeFromZebes && setupFrameWaits > 0;

    /// <summary>Runs one state-$27 dispatcher call.</summary>
    public void Step()
    {
        switch (Stage.Phase)
        {
            case EndingCreditsPhase.SetupEscapeFromZebes:
                // Each wait ends the call inside the setup function, before the dispatcher's
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
                    Stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.Lava, 0);
                    Stage.PaletteFxSystem.SpawnDefinition(EndingPaletteFxDefinitions.Crust, 0);
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
                if (Stage.FloorZoom >= EndingMotionDefinitions.EscapeEndScale)
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
                if (Stage.FloorZoom >= EndingMotionDefinitions.EscapeEndScale)
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
                        MusicRequest.SelectTrack(EndingSequenceDefinitions.Music.Track),
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
                    // Func131 finishes its palette fade, then Func132 holds the waiting
                    // backdrop for three seconds before displaying the producer panel.
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.WaitingBackdropFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsWaitingBackdrop;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingBackdrop:
                if (--Stage.PhaseTimer <= 0)
                {
                    // Function 132 shows the complete result panel in rows nine through seventeen before the
                    // following 180-frame hold.
                    _ = InstalledArtwork.EndingText ?? throw new InvalidOperationException("Post-credits text requires installed ending text.");
                    postCreditsPage.Write(EndingPictureNames.ResultPanel, EndingResultTextLayout.ResultPanelCell);
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
                // E265/E314 test var4 before decrement. Both 64-frame halves advance
                // their shared palette accumulator on the fourth, eighth, ... calls.
                if ((Stage.PhaseTimer & 3) == 1)
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
                // Cinematic function runs before actors; palette fades and queued
                // tile replacements therefore precede the next sprite instruction.
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
                // The sprite interpreter continues behind TM=$01; hiding the actors
                // must not reset their instruction lists or their reveal palette state.
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                {
                    Stage.PhaseTimer = EndingSequenceDefinitions.Timing.RewardRevealHalfFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.ItemPercentage:
                PostCreditsText.Step();
                if (PostCreditsText.RequestedItemPercentageScroll && PostCreditsText.Completed)
                {
                    Stage.PhaseTimer = 40;
                    Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
                }
                break;

            case EndingCreditsPhase.ItemPercentageScrollDown:
                // Function 148 moves the text page down two pixels per call from zero through -80,
                // preserving the already-built page until the final object is spawned.
                Stage.PostCreditsScroll -= new Offset16<PlanePixels>(0, 2);
                if (--Stage.PhaseTimer <= 0)
                {
                    InstalledArtwork.StartPostCreditsText(new EndingBackgroundTextState(new EndingBackgroundTextSetup { Page = postCreditsPage, Inventory = inventory, JapaneseText = japaneseText, Presentation = InstalledArtwork.EndingText, InstalledSequence = InstalledArtwork.EndingText is null ? null : EndingTextSequence.FinalMessage }));
                    Stage.Phase = EndingCreditsPhase.SeeYouNextMission;
                }
                break;

            case EndingCreditsPhase.SeeYouNextMission:
                // Retail parks in the final null cinematic function. There is no hidden
                // automatic reset: the console remains on this frame until reset/power-off.
                EscapeScenes.StepSprites();
                InstalledArtwork.ActivePostCreditsText?.Step();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingCreditsPhase)} {Stage.Phase}.");
        }

        // The cinematic invokes the shared palette-FX interpreter after actors; each record shown shows over the colours
        // the scenes loaded before it, as its write does (#1512).
        IPaletteFxColorSource paletteFxColours = InstalledArtwork.PaletteFxArtwork ?? throw new InvalidOperationException(
            "Ending palette FX requires installed palette colors.");
        Stage.PaletteFxSystem.StepScreen(new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false },
            (colours, record) => screen.Colours.PaletteFx(colours, record, PaletteFxBackdrop(colours, record, paletteFxColours)));
        if (Stage.PaletteFxSystem.SoundRequests.Count != 0 || Stage.PaletteFxSystem.MusicRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled audio command.");
        // The ending follows cinematic actors even while a text-only screen hides sprites.
        // The credits' end enables the already-initialized records when the credits finish.
        if (EndingCreditsPhaseGroups.PostCredits.Contains(Stage.Phase))
            (CreditsRoll.ShootingStars ??= new EndingShootingStars()).Step();
        Flyaway.CinematicFrame++;
    }

    /// <summary>
    /// The backdrop a palette-effect <paramref name="record"/> in <paramref name="colours"/> shows (#1512): the space behind
    /// the exploding planet is the backdrop, so a record of its colours that writes its first colour shows that colour
    /// behind every layer; any other record leaves the backdrop as it was.
    /// </summary>
    /// <exception cref="InvalidDataException">The record's first colour is not installed.</exception>
    private static RgbColor? PaletteFxBackdrop(PaletteFxColours colours, PaletteFxFrame record, IPaletteFxColorSource installed)
    {
        if (colours != EndingPaletteFxDefinitions.SpaceColours || PaletteFxRecordDefinitions.Layout(record)[0].Skip)
            return null;
        return installed.TryReadColor(new PaletteFxColorKey(record, 0), out RgbColor backdrop)
            ? backdrop
            : throw new InvalidDataException($"Palette-FX record {record.Record}'s first colour has no installed definition.");
    }

    /// <summary>Rotates the turning floor every other cinematic call while applying the current zoom delta.</summary>
    private void StepAtmosphericTransform(int scaleDelta)
    {
        // Func111/114 run during both fades as well as the fully visible interval.
        if ((Flyaway.CinematicFrame & 1) == 0)
            Stage.FloorAngle = Stage.FloorAngle.AddTableUnits(-1);
        Stage.FloorZoom += scaleDelta;
    }

    /// <summary>The programs showing the clear-time digits, indexed by digit.</summary>
    internal static readonly CinematicSpriteProgram[] ClearTimeDigitPrograms =
    [
        CinematicSpriteProgram.ClearTimeDigit0, CinematicSpriteProgram.ClearTimeDigit1, CinematicSpriteProgram.ClearTimeDigit2,
        CinematicSpriteProgram.ClearTimeDigit3, CinematicSpriteProgram.ClearTimeDigit4, CinematicSpriteProgram.ClearTimeDigit5,
        CinematicSpriteProgram.ClearTimeDigit6, CinematicSpriteProgram.ClearTimeDigit7, CinematicSpriteProgram.ClearTimeDigit8,
        CinematicSpriteProgram.ClearTimeDigit9,
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
        return shot == EndingFloorShot.None ? null : EndingScreenFloor.Of(InstalledArtwork.Sheets, shot).Shaded(screen.Colours.Shading);
    }

    /// <summary>
    /// Captures the explosion's finale (#1514): the glow's picture behind the planet's, then the glow added over the whole
    /// screen with the debris glowing over it. The debris is drawn only as glow, never over the planet, which would punch
    /// holes in it. The per-tile priority tiers that interleaved the two pictures, and the single pixel that blended where
    /// a sprite overlapped the glow, are presentation quirks the port does not keep: the glow and the debris now both add.
    /// </summary>
    internal LayeredScene CaptureExplosionFinale()
    {
        if (!EscapeScenes.ExplosionFinaleDisplay) return CaptureExplosionBurst();
        DisplayLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap),
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.FinaleSubMap)),
            EndingCreditsPresentation.Sprites(this, EndingCreditsPresentation.CurrentEscapeObjectSheet(this), blend: SpriteBlend.Glows),
        ];
        return new(EndingCreditsPresentation.Paint(this), layers, Stage.Brightness);

        PicturedPlaneLayer Plane(EndingObjectFragmentId map) => new(ExplosionPlane(map), Position16<PlanePixels>.Zero);
    }

    /// <summary>One screen of cells, the size of every whole-screen ending picture.</summary>
    private static readonly Extent16<TilemapCells> ScreenCells = new(EndingScreenDefinitions.TilemapWidth, EndingScreenDefinitions.TilemapHeight);

    /// <summary>The ending's installed picture <paramref name="picture"/> of sheet <paramref name="sheet"/>.</summary>
    private SpriteSheetFrame Picture(SpriteSheetId sheet, string picture) => InstalledArtwork.SpriteSheet(sheet).Frame(picture);

    /// <summary>
    /// A one-screen plane showing the whole-screen picture <paramref name="picture"/> in the ending's colours (#1467, #1512):
    /// the phase that shows it shows it whole, never gated by whether its upload has happened (#1515).
    /// </summary>
    internal PicturedPlane ScreenPicture(SpriteSheetId sheet, string picture) =>
        new(ScreenCells, [new PlacedPlanePicture(Picture(sheet, picture), Position16<PlanePixels>.Zero, screen.Colours.Shading)]);

    /// <summary>
    /// The explosion's one-screen <paramref name="map"/> (#1467, #1515), its picture placed where the screen shows it: its
    /// top <see cref="EndingExplosionDisplayDefinitions.MapRowsAboveScreen"/> lie above the screen's top, so the screen shows
    /// the map from the row below its first.
    /// </summary>
    private PicturedPlane ExplosionPlane(EndingObjectFragmentId map)
    {
        int height = ScreenCells.Y * ScreenGeometry.BackgroundTileSizePixels;
        var corner = new Position16<PlanePixels>(0, height - EndingExplosionDisplayDefinitions.MapRowsAboveScreen);
        return new PicturedPlane(ScreenCells, [new PlacedPlanePicture(Picture(SpriteSheetId.EndingExplosionPictures,
            EndingPictureNames.ExplosionMap(map)), corner, screen.Colours.Shading)]);
    }

    /// <summary>The credits roll's plane: its held rows' line pictures (#1467) in the ending's colours (#1512).</summary>
    internal PicturedPlane CreditsPlane() => Credits.Plane(InstalledArtwork.SpriteSheet(SpriteSheetId.EndingCreditsRows), screen.Colours.Shading);

    /// <summary>The waiting scene's backdrop, a <paramref name="size"/> plane (#1467, #1515), in the ending's colours (#1512).</summary>
    internal PicturedPlane WaitingPlane(Extent16<TilemapCells> size) => new(size,
        [new PlacedPlanePicture(Picture(SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.WaitingBackdrop), Position16<PlanePixels>.Zero,
            screen.Colours.Shading)]);

    /// <summary>
    /// The <paramref name="size"/> plane of the text page <paramref name="page"/> (#1467, #1515): the staging page's text as
    /// it is now, each piece's picture at its cell, in the ending's colours (#1512). A two-screen final page shows its lower
    /// screen's text below it.
    /// </summary>
    /// <exception cref="ArgumentException">The result page is asked for two screens.</exception>
    internal PicturedPlane TextPagePlane(EndingTextScreen page, Extent16<TilemapCells> size)
    {
        SpriteSheet sheet = InstalledArtwork.SpriteSheet(SpriteSheetId.EndingPostCreditsPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        PictureShading shading = screen.Colours.Shading;
        bool twoScreens = size.Y > EndingScreenDefinitions.TilemapHeight;
        if (page == EndingTextScreen.ResultPanel)
        {
            if (twoScreens)
                throw new ArgumentException("The result page has one screen.", nameof(size));
            Place(postCreditsPage.Written, 0);
        }
        else
        {
            Place(postCreditsPage.Written, 0);
            if (twoScreens)
                Place(screen.FinalPageLowerText, EndingScreenDefinitions.TilemapHeight);
        }
        return new PicturedPlane(size, placed.ToImmutable());

        // Places each piece of text `firstRow` rows down the plane.
        void Place(ImmutableArray<WrittenPicture> text, int firstRow)
        {
            foreach (WrittenPicture shown in text)
                placed.Add(new PlacedPlanePicture(sheet.Frame(shown.Picture), new Position16<PlanePixels>(
                    shown.Cell.X * ScreenGeometry.BackgroundTileSizePixels,
                    (shown.Cell.Y + firstRow) * ScreenGeometry.BackgroundTileSizePixels), shading));
        }
    }

    /// <summary>
    /// Captures the explosion's burst (#1514): the stars behind the planet, the planet, the debris in front of it, and the
    /// burst's glow added over the planet and the debris that takes part in blends, never over the sky behind them.
    /// </summary>
    private LayeredScene CaptureExplosionBurst()
    {
        EndingObjectSheet sheet = EndingCreditsPresentation.CurrentEscapeObjectSheet(this);
        var planet = new PicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.InitialMap), Position16<PlanePixels>.Zero);
        SpritesLayer debris = EndingCreditsPresentation.Sprites(this, sheet, EndingSpriteLayer.Actors);
        DisplayLayer[] layers =
        [
            EndingCreditsPresentation.Sprites(this, sheet, EndingSpriteLayer.Stars), planet, debris,
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.BurstSubMap), planet, debris),
        ];
        return new(EndingCreditsPresentation.Paint(this), layers, Stage.Brightness);
    }

    /// <summary>Whether the current cinematic phase leaves its escape or planet backdrop visible.</summary>
    internal bool EscapeBackgroundEnabled => EndingCreditsPhaseGroups.BeforeZebesExplosionHold.Contains(Stage.Phase)
        || !EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Stage.Phase);
}
