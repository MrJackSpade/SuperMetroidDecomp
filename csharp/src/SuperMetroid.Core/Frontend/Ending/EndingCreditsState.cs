using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Ending;

/// <summary>
/// Bank $8B's state-$27 ending owner, from <c>$8B:D443</c> through the credits and
/// post-credit result screen.
/// </summary>
/// <remarks>
/// This is deliberately a cinematic state machine rather than a video or a host-authored
/// slideshow. Backgrounds, character art, palettes, spritemaps, and animation durations
/// preserve cartridge behavior; editable text and fonts come from installed assets. The named phases merely replace the native
/// <c>cinematic_function</c> address, making the same coroutine boundaries debuggable in C#.
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
    /// <summary>The ending's screen: its palette, the pictures its scenes have loaded and its text pages (#1358, #1467).</summary>
    internal readonly EndingScreen screen = new();
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>The post-credits staging page.</summary>
    private readonly EndingTextPage postCreditsPage = new();
    /// <summary>The state of the credits scroll, which exists while that phase runs.</summary>
    internal CreditsObjectState Credits => InstalledArtwork.CurrentCredits ??
        throw new InvalidOperationException("The credits scroll is not running.");
    /// <summary>The state of the post-credits text, which exists while that phase runs.</summary>
    private EndingBackgroundTextState PostCreditsText => InstalledArtwork.ActivePostCreditsText ??
        throw new InvalidOperationException("The post-credits text is not running.");
    /// <summary>NMI waits <c>CinematicFunction_Ending_Setup</c> has made so far.</summary>
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

    /// <summary>What every ending scene draws on: brightness, the phase timer, the Mode 7 view, palettes, the shared sprites and stars, and the artwork they share.</summary>
    internal EndingStage Stage { get; }

    /// <summary>The gunship's flight from Zebes: the planet escape's speeds, the stars, the fade and the flight artwork's uploads.</summary>
    internal EndingFlyaway Flyaway { get; }

    /// <summary>The ending's installed artwork: its text, font, staff credits, Mode 7, object and palette assets and their loads.</summary>
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
        InstalledArtwork = new EndingArtwork(screen, Stage);
        Flyaway = new EndingFlyaway(setup.CrittersEscaped, screen, sprites, Stage);
        Explosion = new EndingZebesExplosion(screen, sprites, Stage, Flyaway, InstalledArtwork);
        EscapeScenes = new EndingEscapeScenes(audio, setup.GameTimeHours, setup.GameTimeMinutes, screen, sprites, Stage, InstalledArtwork);
        CreditsRoll = new EndingCreditsRoll(inventory, japaneseText, screen, sprites, postCreditsPage, Stage, InstalledArtwork);
        InstalledArtwork.BindEndingText(setup.EndingText);
        Stage.Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }
    /// <summary>
    /// True when the next call resumes inside the setup's NMI wait loop rather than starting
    /// a MainGameLoop iteration, so it makes no main-loop RNG call.
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

            case EndingCreditsPhase.ZebesExplosionTileUpload:
                Flyaway.UploadFlyawayChunk(16 - Stage.PhaseTimer);
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                    Stage.Phase = EndingCreditsPhase.ZebesExplosionAnimation;
                break;

            case EndingCreditsPhase.ZebesExplosionAnimation:
                // The EE9D actor owns the remaining timing. Its F32B opcode publishes the
                // exact 120-frame post-explosion music wait, just as on the cartridge.
                EscapeScenes.StepSprites();
                break;

            case EndingCreditsPhase.WaitForPlanetEscapeMusic:
                EscapeScenes.StepSprites();
                if (--Stage.PhaseTimer <= 0)
                {
                    audio.QueueMusicDelayed8(MusicRequest.Stop);
                    audio.QueueMusicDelayed8(MusicRequest.LoadDataSet(MusicDataSet.Credits));
                    audio.QueueMusicDelayed(
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
                    // Function 132 shows the complete $8C:DC9B result panel in rows nine through seventeen before the
                    // following 180-frame hold.
                    _ = InstalledArtwork.EndingText ?? throw new InvalidOperationException("Post-credits text requires installed ending text.");
                    postCreditsPage.Write(EndingPictureNames.ResultPanel, EndingResultTextLayout.ResultPanelCell);
                    CreditsRoll.UploadPostCreditsTilemap();
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
                    // E342 replaces the idle actors with the cartridge gesture lists.
                    // BG1/BG2 are disabled during this OBJ-only animation.
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
                    InstalledArtwork.BeginRewardIconUpload();
                    CreditsRoll.CurrentRewardJump = new EndingRewardJump(Stage.EndingReward, InstalledArtwork.UploadRewardGraphic);
                    Stage.Phase = EndingCreditsPhase.PostCreditsJump;
                }
                break;

            case EndingCreditsPhase.PostCreditsJump:
                RewardJump.Step();
                if (RewardJump.ShotRequested)
                {
                    CreditsRoll.CurrentPostShot = new EndingPostShot(screen.Palette);
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
                PostCreditsText.Step(screen);
                if (PostCreditsText.RequestedItemPercentageScroll && PostCreditsText.Completed)
                {
                    Stage.PhaseTimer = 40;
                    Stage.Phase = EndingCreditsPhase.ItemPercentageScrollDown;
                }
                break;

            case EndingCreditsPhase.ItemPercentageScrollDown:
                // Function 148 moves BG1 down two pixels per call from zero through -80.
                // The software renderer applies the equivalent scroll while preserving the
                // already-built tilemap until the final object is spawned.
                Stage.PostCreditsScroll -= new Offset16<TilePlanePixels>(0, 2);
                if (--Stage.PhaseTimer <= 0)
                {
                    InstalledArtwork.StartPostCreditsText(new EndingBackgroundTextState(new EndingBackgroundTextSetup { Page = postCreditsPage, Inventory = inventory, JapaneseText = japaneseText, DestinationTilemap = CreditsRoll.PostCreditsTextTilemap, Presentation = InstalledArtwork.EndingText, InstalledSequence = InstalledArtwork.EndingText is null ? null : EndingTextSequence.FinalMessage }));
                    Stage.Phase = EndingCreditsPhase.SeeYouNextMission;
                }
                break;

            case EndingCreditsPhase.SeeYouNextMission:
                // Retail parks in the final null cinematic function. There is no hidden
                // automatic reset: the console remains on this frame until reset/power-off.
                EscapeScenes.StepSprites();
                InstalledArtwork.ActivePostCreditsText?.Step(screen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(EndingCreditsPhase)} {Stage.Phase}.");
        }

        // The native cinematic invokes the shared bank-$8D interpreter after actors.
        Stage.PaletteFxSystem.Step(new ScreenPaletteFxColours(screen.Palette), InstalledArtwork.PaletteFxArtwork ?? throw new InvalidOperationException(
                "Ending palette FX requires installed palette colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        if (Stage.PaletteFxSystem.SoundRequests.Count != 0 || Stage.PaletteFxSystem.MusicRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled audio command.");
        // $8B:D46B follows cinematic actors even while a text-only TM masks OBJ.
        // F734 enables the already-initialized records when the credits finish.
        if (EndingCreditsPhaseGroups.PostCredits.Contains(Stage.Phase))
            (CreditsRoll.ShootingStars ??= new EndingShootingStars()).Step();
        Flyaway.CinematicFrame++;
    }

    /// <summary>Rotates Mode 7 every other cinematic call while applying the current zoom delta.</summary>
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

    /// <summary>Whether the explosion currently uses its dedicated color-math composition.</summary>
    internal bool UsesExplosionFinaleDisplay => (EscapeScenes.ExplosionFinaleDisplay || EscapeScenes.ExplosionBurstDisplay) && !EndingCreditsPresentation.ExplosionWhiteout(this)
        && EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Stage.Phase);

    /// <summary>
    /// The turning floor the scenes have loaded: its shot's installed picture (#1467), or none before a floor has loaded
    /// and once the post-credits art has replaced it, when the floor layer shows nothing.
    /// </summary>
    internal AffineFloor? Floor() =>
        screen.FloorShot == EndingFloorShot.None ? null : EndingScreenFloor.Of(InstalledArtwork.Sheets, screen.FloorShot);

    /// <summary>Captures the finale BG priority planes and one winning subscreen color-add layer.</summary>
    internal LayeredScene CaptureExplosionFinale()
    {
        if (!EscapeScenes.ExplosionFinaleDisplay) return CaptureExplosionBurst();
        // F2FA: TM=BG1|BG2, TS=BG2|OBJ, CGADSUB=$33. Resolve the two
        // main backgrounds by priority, then add the single winning subscreen pixel.
        // OBJ is not independently drawn on main, which would punch holes in the glow.
        DisplayLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, TileDepth.Back),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, TileDepth.Back),
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, TileDepth.Front),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, TileDepth.Front),
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.FinaleSubMap), IncludeObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(EndingCreditsPresentation.CaptureMemory(this, EndingCreditsPresentation.CurrentEscapeObjectSheet(this)), layers, checked((byte)Stage.Brightness));

        PicturedPlaneLayer Plane(EndingObjectFragmentId map, TileDepth priority) => new(ExplosionPlane(map),
            new Position16<TilePlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow), priority == TileDepth.Front);
    }

    /// <summary>One screen of cells, the size of every whole-screen ending picture.</summary>
    private static readonly Extent16<TilemapCells> ScreenCells = new(EndingScreenDefinitions.TilemapWidth, EndingScreenDefinitions.TilemapHeight);

    /// <summary>The ending's installed picture <paramref name="picture"/> of sheet <paramref name="sheet"/>.</summary>
    private SpriteSheetFrame Picture(SpriteSheetId sheet, string picture) => InstalledArtwork.SpriteSheet(sheet).Frame(picture);

    /// <summary>A one-screen plane showing the whole-screen picture <paramref name="picture"/> once <paramref name="loaded"/>, otherwise nothing (#1467).</summary>
    internal PicturedPlane ScreenPicture(SpriteSheetId sheet, string picture, bool loaded) => loaded
        ? new PicturedPlane(ScreenCells, [new PlacedPlanePicture(Picture(sheet, picture), Position16<TilePlanePixels>.Zero)])
        : PicturedPlane.Empty(ScreenCells);

    /// <summary>The explosion's one-screen <paramref name="map"/>, once the explosion has loaded its maps (#1467).</summary>
    private PicturedPlane ExplosionPlane(EndingObjectFragmentId map) =>
        ScreenPicture(SpriteSheetId.EndingExplosionPictures, EndingPictureNames.ExplosionMap(map), screen.ExplosionMapsLoaded);

    /// <summary>The credits roll's plane: its held rows' line pictures (#1467).</summary>
    internal PicturedPlane CreditsPlane() => Credits.Plane(InstalledArtwork.SpriteSheet(SpriteSheetId.EndingCreditsRows));

    /// <summary>The waiting scene's backdrop, a <paramref name="size"/> plane, once the post-credits art is loaded (#1467).</summary>
    internal PicturedPlane WaitingPlane(Extent16<TilemapCells> size) => new(size, screen.PostCreditsArtLoaded
        ? [new PlacedPlanePicture(Picture(SpriteSheetId.EndingPostCreditsPictures, EndingPictureNames.WaitingBackdrop), Position16<TilePlanePixels>.Zero)]
        : []);

    /// <summary>
    /// The <paramref name="size"/> plane of the text page <paramref name="page"/> (#1467): the text it shows, each piece's
    /// picture at its cell. A two-screen final page shows its lower screen's text below its upper screen's.
    /// </summary>
    /// <exception cref="ArgumentException">The result page is asked for two screens.</exception>
    internal PicturedPlane TextPagePlane(EndingTextTilemap page, Extent16<TilemapCells> size)
    {
        SpriteSheet sheet = InstalledArtwork.SpriteSheet(SpriteSheetId.EndingPostCreditsPictures);
        var placed = ImmutableArray.CreateBuilder<PlacedPlanePicture>();
        bool twoScreens = size.Y > EndingScreenDefinitions.TilemapHeight;
        if (page == EndingTextTilemap.ResultPanel)
        {
            if (twoScreens)
                throw new ArgumentException("The result page has one screen.", nameof(size));
            Place(screen.ResultPageText, 0);
        }
        else if (screen.FinalPageText is { } upper)
        {
            Place(upper, 0);
            if (twoScreens)
                Place(screen.FinalPageLowerText, EndingScreenDefinitions.TilemapHeight);
        }
        return new PicturedPlane(size, placed.ToImmutable());

        // Places each piece of text `firstRow` rows down the plane.
        void Place(ImmutableArray<ShownEndingText> text, int firstRow)
        {
            foreach (ShownEndingText shown in text)
                placed.Add(new PlacedPlanePicture(sheet.Frame(shown.Picture), new Position16<TilePlanePixels>(
                    checked((ushort)(shown.Cell.X * ScreenGeometry.BackgroundTileSizePixels)),
                    checked((ushort)((shown.Cell.Y + firstRow) * ScreenGeometry.BackgroundTileSizePixels)))));
        }
    }

    /// <summary>Captures the burst's BG1/OBJ main screen and BG2 subscreen color math.</summary>
    private LayeredScene CaptureExplosionBurst()
    {
        // F2B7: BG1 and OBJ on main, BG2 on sub, math only for BG1 and
        // eligible OBJ palettes. Keep the backdrop and OBJ palettes 0–3 unmodified.
        var main = new PicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.InitialMap),
            new Position16<TilePlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow), null);
        DisplayLayer[] layers =
        [
            new SpriteDepthLayer(0), new SpriteDepthLayer(1),
            main with { Priority = false }, new SpriteDepthLayer(2),
            main with { Priority = true }, new SpriteDepthLayer(3),
            new AddedPicturedPlaneLayer(ExplosionPlane(EndingExplosionDisplayDefinitions.BurstSubMap), main, MainObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(EndingCreditsPresentation.CaptureMemory(this, EndingCreditsPresentation.CurrentEscapeObjectSheet(this)), layers, checked((byte)Stage.Brightness));
    }

    /// <summary>Whether the current cinematic phase leaves its escape or planet backdrop visible.</summary>
    internal bool EscapeBackgroundEnabled => EndingCreditsPhaseGroups.BeforeZebesExplosionTileUpload.Contains(Stage.Phase)
        || !EndingCreditsPhaseGroups.BeforePlanetEscape.Contains(Stage.Phase);
}
