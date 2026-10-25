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
            InstalledArtwork.BindStaffCredits(artwork.MapPresentation?.StaffCredits);
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
    /// <summary>The ending's screen: its palette and the graphics transfers its scenes have made (#1358).</summary>
    internal readonly EndingScreen screen = new();
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>Staging copy of the two-screen post-credits result tilemap.</summary>
    private readonly ushort[] postCreditsTilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];
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
        CreditsRoll = new EndingCreditsRoll(inventory, japaneseText, screen, sprites, postCreditsTilemap, Stage, InstalledArtwork);
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
                if (setupFrameWaits < EndingCreditsRomData.SetupFrameWaits)
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
                if (Stage.FloorZoom >= EndingCreditsRomData.Motion.EscapeEndScale)
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
                if (Stage.FloorZoom >= EndingCreditsRomData.Motion.EscapeEndScale)
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
                        MusicRequest.SelectTrack(EndingCreditsRomData.Music.Track),
                        MusicQueueDelay.AtLeastShortest(
                            EndingCreditsRomData.Music.DelayArgument));
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
                CreditsObjectStepResult creditsStep = Credits.Step();
                if (creditsStep.CopiedRow)
                    Credits.UploadTilemap(screen);
                if (creditsStep.Finished)
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
                    Stage.PhaseTimer = EndingCreditsRomData.Timing.WaitingPaletteFadeFrames;
                    CreditsRoll.ApplyWaitingBackdropPalette(0);
                    Stage.Phase = EndingCreditsPhase.PostCreditsShootingStars;
                }
                break;

            case EndingCreditsPhase.PostCreditsShootingStars:
                CreditsRoll.ApplyWaitingBackdropPalette(EndingCreditsRomData.Timing.WaitingPaletteFadeFrames - Stage.PhaseTimer + 1);
                if (--Stage.PhaseTimer <= 0)
                {
                    // Func131 finishes its palette fade, then Func132 holds the waiting
                    // backdrop for three seconds before displaying the producer panel.
                    Stage.PhaseTimer = EndingCreditsRomData.Timing.WaitingBackdropFrames;
                    Stage.Phase = EndingCreditsPhase.PostCreditsWaitingBackdrop;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingBackdrop:
                if (--Stage.PhaseTimer <= 0)
                {
                    // Function 132 installs the complete $8C:DC9B result panel into rows
                    // nine through seventeen before the following 180-frame hold.
                    (InstalledArtwork.EndingText ?? throw new InvalidOperationException(
                        "Post-credits text requires installed ending text.")).BuildResultPanel().CopyTo(
                        postCreditsTilemap, EndingCreditsRomData.Text.ResultPanelFirstCell);
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
                    Stage.PhaseTimer = EndingCreditsRomData.Timing.RewardRevealHalfFrames;
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
                    RequireRewardIcon(Stage.FloorArtwork?.RewardIcon);
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
                    Stage.PhaseTimer = EndingCreditsRomData.Timing.RewardRevealHalfFrames;
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
                Stage.PostCreditsScroll -= new Offset16<BgPlanePixels>(0, 2);
                if (--Stage.PhaseTimer <= 0)
                {
                    InstalledArtwork.StartPostCreditsText(new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, Inventory = inventory, JapaneseText = japaneseText, DestinationTilemap = CreditsRoll.PostCreditsTextTilemap, Presentation = InstalledArtwork.EndingText, InstalledSequence = InstalledArtwork.EndingText is null ? null : EndingTextSequence.FinalMessage }));
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
        if (Stage.Phase >= EndingCreditsPhase.PostCreditsBlank)
            (CreditsRoll.ShootingStars ??= new EndingShootingStars()).Step();
        Flyaway.CinematicFrame++;
    }

    /// <summary>Requires installed reward-icon artwork that fills the native WRAM upload range.</summary>
    private static void RequireRewardIcon(EndingRewardIconArtwork? artwork)
    {
        EndingRewardIconArtwork icon = artwork ?? throw new InvalidOperationException(
            "Ending reward requires installed icon artwork.");
        if (icon.Map.Length + icon.CharacterCount * EndingFloorArtworkFormat.CharacterBytes < EndingRewardGraphicsUploadDefinitions.SourceBytes)
            throw new InvalidDataException("Post-credits icon graphics do not fill the native WRAM upload range.");
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
        && Stage.Phase < EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>What <paramref name="layer"/> shows reading <paramref name="depth"/> characters: their pre-coloured tiles (#1392).</summary>
    internal ScreenLayerPictures LayerTiles(EndingTileLayer layer, BgBitDepth depth) => screen.Tiles(InstalledArtwork.Sheets, layer, depth);

    /// <summary>The floor's cells: what the floor scene, flyaway and reward uploads lay out.</summary>
    internal byte[] FloorCells() => screen.FloorCells(Stage.FloorArtwork, Flyaway.FlightArtwork);

    /// <summary>What the Mode 7 map shows: the pre-coloured characters the uploads leave (#1392).</summary>
    internal FloorTilePictures FloorTiles() => screen.FloorTiles(InstalledArtwork.Sheets);

    /// <summary>Captures the finale BG priority planes and one winning subscreen color-add layer.</summary>
    internal LayeredScene CaptureExplosionFinale()
    {
        if (!EscapeScenes.ExplosionFinaleDisplay) return CaptureExplosionBurst();
        // F2FA: TM=BG1|BG2, TS=BG2|OBJ, CGADSUB=$33. Resolve the two
        // main backgrounds by priority, then add the single winning subscreen pixel.
        // OBJ is not independently drawn on main, which would punch holes in the glow.
        RenderLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, BgTilePriority.Low),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, BgTilePriority.Low),
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, BgTilePriority.High),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, BgTilePriority.High),
            new TileAddRenderLayer(ExplosionCells(EndingExplosionDisplayDefinitions.FinaleSubMap), IncludeObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(EndingCreditsPresentation.CaptureMemory(this, EndingCreditsPresentation.CurrentEscapeObjectSheet(this)), layers, checked((byte)Stage.Brightness));

        TileRenderLayer Plane(EndingObjectFragmentId map, BgTilePriority priority) => new(ExplosionCells(map),
            new Position16<BgPlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow), priority == BgTilePriority.High);
    }

    /// <summary>The cells of the explosion's one-screen <paramref name="map"/>, showing its characters' pre-coloured tiles (#1392).</summary>
    private ScreenTileMap ExplosionCells(EndingObjectFragmentId map) =>
        ScreenTileMap.Of(screen.ExplosionPlane(map, Stage.ObjectArtwork), TileViewportRenderLayer.MapSize, LayerTiles(EndingTileLayer.Explosion, BgBitDepth.FourBpp));

    /// <summary>
    /// The cells of the <paramref name="size"/> map <paramref name="plane"/> shows (#1404), showing <paramref name="layer"/>'s
    /// pre-coloured tiles of <paramref name="depth"/> characters. A two-screen text map shows the final text's lower screen
    /// below its upper one.
    /// </summary>
    /// <exception cref="ArgumentException">A plane other than the text plane is asked for two screens.</exception>
    internal ScreenTileMap Cells(EndingPlane plane, Extent16<TilemapCells> size, EndingTileLayer layer, BgBitDepth depth)
    {
        ReadOnlySpan<ushort> upper = screen.Plane(plane, Stage.ObjectArtwork);
        if (size.Y <= EndingCreditsRomData.Rendering.TilemapHeight)
            return ScreenTileMap.Of(upper[..(size.X * size.Y)], size, LayerTiles(layer, depth));
        if (plane != EndingPlane.Text)
            throw new ArgumentException($"The {plane} plane has one screen.", nameof(plane));
        ushort[] words = [.. upper, .. screen.Plane(EndingPlane.TextLower, Stage.ObjectArtwork)];
        return ScreenTileMap.Of(words.AsSpan(0, size.X * size.Y), size, LayerTiles(layer, depth));
    }

    /// <summary>Captures the burst's BG1/OBJ main screen and BG2 subscreen color math.</summary>
    private LayeredScene CaptureExplosionBurst()
    {
        // F2B7: BG1 and OBJ on main, BG2 on sub, math only for BG1 and
        // eligible OBJ palettes. Keep the backdrop and OBJ palettes 0–3 unmodified.
        var main = new TileRenderLayer(ExplosionCells(EndingExplosionDisplayDefinitions.InitialMap),
            new Position16<BgPlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow), null);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
            main with { Priority = false }, new ObjPriorityRenderLayer(2),
            main with { Priority = true }, new ObjPriorityRenderLayer(3),
            new TileAddRenderLayer(ExplosionCells(EndingExplosionDisplayDefinitions.BurstSubMap), main, MainObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(EndingCreditsPresentation.CaptureMemory(this, EndingCreditsPresentation.CurrentEscapeObjectSheet(this)), layers, checked((byte)Stage.Brightness));
    }

    /// <summary>Whether the current cinematic phase leaves its escape or planet backdrop visible.</summary>
    internal bool EscapeBackgroundEnabled => Stage.Phase is < EndingCreditsPhase.ZebesExplosionTileUpload
        or >= EndingCreditsPhase.PlanetEscapeFast;
}
