using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Frontend.Ending.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

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
internal sealed class EndingCreditsState
{
    /// <summary>The <see cref="EndingCreditsPresentation"/> owner of this EndingCreditsState.</summary>
    internal EndingCreditsPresentation Presentation { get; }
    /// <summary>Queues the authored ending music transitions.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Saved clear-time hours, as the game clock or the configured override supplies them, used to select the post-credits reward.</summary>
    private readonly int gameTimeHours;
    /// <summary>Saved clear-time minutes displayed with the clear time.</summary>
    private readonly int gameTimeMinutes;
    /// <summary>Collected-item snapshot displayed with the final result.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Chooses the localized post-credits text asset.</summary>
    private readonly bool japaneseText;
    /// <summary>Controls the animals' escape detail in the planet cinematic.</summary>
    private readonly bool crittersEscaped;
    /// <summary>Live ending tile and character memory across cinematic phases.</summary>
    internal readonly SnesVram vram = new();
    /// <summary>Live ending palette memory, including partial fades.</summary>
    internal readonly SnesCgram cgram = new();
    /// <summary>Dynamic cinematic actors in native slot traversal order.</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>Staging copy of the two-screen post-credits result tilemap.</summary>
    private readonly ushort[] postCreditsTilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];

    /// <summary>Scrolling staff-list owner active during the credits phase.</summary>
    internal CreditsObjectState? credits;
    /// <summary>The state of the credits scroll, which exists while that phase runs.</summary>
    internal CreditsObjectState Credits => credits ??
        throw new InvalidOperationException("The credits scroll is not running.");
    /// <summary>Background star animation retained into the post-credits scene.</summary>
    internal EndingShootingStars? shootingStars;
    /// <summary>Current result-panel text writer over the staging tilemap.</summary>
    private EndingBackgroundTextState? postCreditsText;
    /// <summary>The state of the post-credits text, which exists while that phase runs.</summary>
    private EndingBackgroundTextState PostCreditsText => postCreditsText ??
        throw new InvalidOperationException("The post-credits text is not running.");
    /// <summary>
    /// Cinematic calls made so far. Only its parity and quarter phase are read (the alternating
    /// atmospheric rotation and the planet turn cadence), so a growing count reproduces the
    /// cartridge's wrapping word exactly.
    /// </summary>
    internal int cinematicFrame;
    /// <summary>NMI waits <c>CinematicFunction_Ending_Setup</c> has made so far.</summary>
    private int setupNmiWaits;
    /// <summary>Countdown interpreted by the currently installed cinematic function.</summary>
    internal int phaseTimer;
    /// <summary>Shared fade progression through scene and palette handoffs.</summary>
    private int fadeCounter;
    /// <summary>Master brightness during cinematic fades, from 0 (black) through 15 (full); every fade step clamps to that range.</summary>
    internal int brightness;
    /// <summary>Mode-7 translation; only X uses a subpixel because native Y has no fractional word.</summary>
    internal FixedPosition<Mode7Pixels> mode7;
    /// <summary>
    /// Mode-7 scale in 8.8 units (256 is unity), initially the identity. Every scene starts it at an authored
    /// scale and moves it by a few units per call toward a bound it stops at, so it stays positive and small.
    /// </summary>
    internal int mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
    /// <summary>Mode-7 byte-angle shared by the escape and planet scenes.</summary>
    internal SnesAngle mode7Angle;
    /// <summary>Position in the current planet-escape shake pattern, cycling through its entries.</summary>
    internal int planetMotionIndex;
    /// <summary>Scroll of the post-credits text map; only the vertical axis moves, during the item-percentage scroll-down.</summary>
    internal Position16<BgPlanePixels> postCreditsScroll;
    /// <summary>Planet-escape velocity: a signed 16.16 displacement that grows more negative every accelerating update.</summary>
    internal SnesSignedSixteenSixteen planetVelocity;
    /// <summary>Whether the credits and reward art have been transferred to VRAM.</summary>
    private bool creditsAssetsLoaded;
    /// <summary>Prevents repeating the native copyright-panel handoff.</summary>
    private bool rewardCopyrightShown;
    /// <summary>Post-credits suitless or helmet-removal gesture actor.</summary>
    internal EndingRewardGesture? rewardGesture;
    /// <summary>The state of the reward gesture, which exists while that phase runs.</summary>
    internal EndingRewardGesture RewardGesture => rewardGesture ??
        throw new InvalidOperationException("The reward gesture is not running.");
    /// <summary>Post-credits reward jump and landing actor.</summary>
    internal EndingRewardJump? rewardJump;
    /// <summary>The state of the reward jump, which exists while that phase runs.</summary>
    internal EndingRewardJump RewardJump => rewardJump ??
        throw new InvalidOperationException("The reward jump is not running.");
    /// <summary>Uploads reward graphics on the actor's native instruction boundary.</summary>
    private EndingRewardGraphicsUpload? rewardGraphics;
    /// <summary>Final shot-into-camera actor and its drawing state.</summary>
    internal EndingPostShot? postShot;
    /// <summary>The state of the reward shot, which exists while that phase runs.</summary>
    internal EndingPostShot PostShot => postShot ??
        throw new InvalidOperationException("The reward shot is not running.");
    /// <summary>Palette-object interpreter used by clouds and planet afterglow.</summary>
    private RoomPaletteFxSystem paletteFx = new();
    /// <summary>Reloadable localized ending text presentation.</summary>
    [NonSerialized] private EndingTextPresentation? endingText;
    /// <summary>Reloadable font bytes for ending panels and staff credits.</summary>
    [NonSerialized] private EndingFontAtlas? endingFont;
    /// <summary>Reloadable staff credits content used by the scrolling owner.</summary>
    [NonSerialized] private CreditsPresentation? staffCredits;
    /// <summary>Reloadable Ceres source art reused for the flyaway DMA.</summary>
    [NonSerialized] private CeresFlightArtworkCatalog? flightArtwork;
    /// <summary>Reloadable Mode-7 maps and characters for each ending scene.</summary>
    [NonSerialized] private EndingMode7ArtworkCatalog? mode7Artwork;
    /// <summary>Reloadable object character art for clouds, explosion and reward actors.</summary>
    [NonSerialized] internal EndingObjectArtworkCatalog? objectArtwork;

    /// <summary>Initializes state-$27 from the saved clear-time and ending setup snapshot.</summary>
    public EndingCreditsState(CartridgeAudioState audio, EndingCreditsSetup setup)
    {
        Presentation = new EndingCreditsPresentation(this);
        this.audio = Ensure.NotNull(audio);
        this.gameTimeHours = setup.GameTimeHours;
        this.gameTimeMinutes = setup.GameTimeMinutes;
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        this.crittersEscaped = setup.CrittersEscaped;
        this.endingText = setup.EndingText;
        Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }

    /// <summary>Current bank-$8B cinematic-function replacement, including terminal display states.</summary>
    public EndingCreditsPhase Phase { get; internal set; }
    /// <summary>
    /// True when the next call resumes inside the setup's NMI wait loop rather than starting
    /// a MainGameLoop iteration, so it makes no main-loop RNG call.
    /// </summary>
    internal bool ResumesAfterNmiWait =>
        Phase == EndingCreditsPhase.SetupEscapeFromZebes && setupNmiWaits > 0;
    /// <summary>Selects suitless, helmetless or armored presentation from native clear-time hour thresholds.</summary>
    public EndingReward EndingReward =>
        gameTimeHours < EndingCreditsRomData.Rewards.SuitlessMaximumHoursExclusive
        ? EndingReward.Suitless
        : gameTimeHours < EndingCreditsRomData.Rewards.HelmetlessMaximumHoursExclusive
            ? EndingReward.Helmetless
            : EndingReward.Armored;

    /// <summary>Runs one state-$27 dispatcher call.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case EndingCreditsPhase.SetupEscapeFromZebes:
                // Each wait ends the call inside the setup function, before the dispatcher's
                // manual-return tail; the call after the last wait runs the scene setup.
                if (setupNmiWaits < EndingCreditsRomData.SetupNmiWaits)
                {
                    setupNmiWaits++;
                    return;
                }
                SetupEscapeSceneA();
                break;

            case EndingCreditsPhase.WaitForEscapeMusic:
                if (!audio.HasQueuedMusic)
                {
                    paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.Lava, 0);
                    paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.Crust, 0);
                    brightness = 0;
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeInEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeInEscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds();
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneA;
                break;

            case EndingCreditsPhase.EscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds();
                if (mode7Zoom >= EndingCreditsRomData.Motion.EscapeEndScale)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds();
                if (StepFastFadeOut())
                    SetupEscapeSceneB();
                break;

            case EndingCreditsPhase.FadeInEscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds();
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneB;
                break;

            case EndingCreditsPhase.EscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds();
                if (mode7Zoom >= EndingCreditsRomData.Motion.EscapeEndScale)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneB;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds();
                if (StepFastFadeOut())
                    SetupZebesExplosion();
                break;

            case EndingCreditsPhase.FadeInZebesExplosion:
                StepExplosionCrossfade();
                StepSprites();
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.ZebesExplosionPaletteCrossfade;
                break;

            case EndingCreditsPhase.ZebesExplosionPaletteCrossfade:
                StepExplosionCrossfade();
                StepSprites();
                break;

            case EndingCreditsPhase.ZebesExplosionTileUpload:
                UploadFlyawayChunk(16 - phaseTimer);
                StepSprites();
                if (--phaseTimer <= 0)
                    Phase = EndingCreditsPhase.ZebesExplosionAnimation;
                break;

            case EndingCreditsPhase.ZebesExplosionAnimation:
                // The EE9D actor owns the remaining timing. Its F32B opcode publishes the
                // exact 120-frame post-explosion music wait, just as on the cartridge.
                StepSprites();
                break;

            case EndingCreditsPhase.WaitForPlanetEscapeMusic:
                StepSprites();
                if (--phaseTimer <= 0)
                {
                    audio.QueueMusicDelayed8(MusicCommand.Stop);
                    audio.QueueMusicDelayed8(MusicCommand.LoadData(
                        EndingCreditsRomData.Music.EndingDataIndex));
                    audio.QueueMusicDelayed(
                        MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
                        MusicCommandDelay.FromDelayedYArgument(
                            EndingCreditsRomData.Music.DelayArgument));
                    Phase = EndingCreditsPhase.WaitForPlanetEscapeMusicQueue;
                }
                break;

            case EndingCreditsPhase.WaitForPlanetEscapeMusicQueue:
                StepSprites();
                if (!audio.HasQueuedMusic)
                    SetupPlanetEscape();
                break;

            case EndingCreditsPhase.PlanetEscapeFast:
                StepSprites();
                StepPlanetEscapeFast();
                break;

            case EndingCreditsPhase.PlanetEscapeSlow:
                StepSprites();
                StepPlanetEscapeSlow();
                break;

            case EndingCreditsPhase.PlanetEscapeAccelerating:
                StepSprites();
                StepPlanetEscapeAccelerating();
                break;

            case EndingCreditsPhase.OperationSuccessfulText:
                StepSprites();
                break;

            case EndingCreditsPhase.FadeOutToCredits:
                StepSprites();
                if (StepFastFadeOut())
                    SetupCredits();
                break;

            case EndingCreditsPhase.Credits:
                CreditsObjectStepResult creditsStep = Credits.Step();
                if (creditsStep.CopiedRow)
                    Credits.UploadTilemap(vram);
                if (creditsStep.Finished)
                    SetupPostCreditsBlank();
                break;

            case EndingCreditsPhase.PostCreditsBlank:
                if (--phaseTimer <= 0)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.PostCreditsFadeIn;
                }
                break;

            case EndingCreditsPhase.PostCreditsFadeIn:
                if (StepSlowFadeIn())
                {
                    phaseTimer = EndingCreditsRomData.Timing.WaitingPaletteFadeFrames;
                    ApplyWaitingBackdropPalette(0);
                    Phase = EndingCreditsPhase.PostCreditsShootingStars;
                }
                break;

            case EndingCreditsPhase.PostCreditsShootingStars:
                ApplyWaitingBackdropPalette(EndingCreditsRomData.Timing.WaitingPaletteFadeFrames - phaseTimer + 1);
                if (--phaseTimer <= 0)
                {
                    // Func131 finishes its palette fade, then Func132 holds the waiting
                    // backdrop for three seconds before displaying the producer panel.
                    phaseTimer = EndingCreditsRomData.Timing.WaitingBackdropFrames;
                    Phase = EndingCreditsPhase.PostCreditsWaitingBackdrop;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingBackdrop:
                if (--phaseTimer <= 0)
                {
                    // Function 132 installs the complete $8C:DC9B result panel into rows
                    // nine through seventeen before the following 180-frame hold.
                    (endingText ?? throw new InvalidOperationException(
                        "Post-credits text requires installed ending text.")).BuildResultPanel().CopyTo(
                        postCreditsTilemap, EndingCreditsRomData.Text.ResultPanelDestination);
                    UploadPostCreditsTilemap();
                    phaseTimer = 180;
                    Phase = EndingCreditsPhase.PostCreditsWaitingSamus;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingSamus:
                if (--phaseTimer <= 0)
                {
                    SpawnEndingRewardActors();
                    rewardPaletteStep = 0;
                    ApplyRewardPalette();
                    phaseTimer = EndingCreditsRomData.Timing.RewardRevealHalfFrames;
                    Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.PostCreditsReward:
                // E265/E314 test var4 before decrement. Both 64-frame halves advance
                // their shared palette accumulator on the fourth, eighth, ... calls.
                if ((phaseTimer & 3) == 1)
                {
                    rewardPaletteStep++;
                    ApplyRewardPalette();
                }
                StepSprites();
                if (--phaseTimer <= 0)
                {
                    if (!rewardCopyrightShown)
                    {
                        ShowRewardCopyright();
                        break;
                    }
                    // E342 replaces the idle actors with the cartridge gesture lists.
                    // BG1/BG2 are disabled during this OBJ-only animation.
                    sprites.Clear();
                    rewardGesture = new EndingRewardGesture(EndingReward);
                    Phase = EndingCreditsPhase.PostCreditsGesture;
                }
                break;

            case EndingCreditsPhase.PostCreditsGesture:
                RewardGesture.Step();
                if (RewardGesture.JumpRequested)
                {
                    rewardGesture = null;
                    rewardGraphics = new EndingRewardGraphicsUpload(mode7Artwork?.RewardIcon);
                    rewardJump = new EndingRewardJump(EndingReward, UploadRewardGraphic);
                    Phase = EndingCreditsPhase.PostCreditsJump;
                }
                break;

            case EndingCreditsPhase.PostCreditsJump:
                RewardJump.Step();
                if (RewardJump.ShotRequested)
                {
                    postShot = new EndingPostShot(cgram, ResolveEndingFont(), objectArtwork);
                    audio.QueueSound(EndingPostShotDefinitions.ShotSound, EndingPostShotDefinitions.SoundQueueLimit);
                    Phase = EndingCreditsPhase.PostCreditsShot;
                }
                break;

            case EndingCreditsPhase.PostCreditsShot:
                // Cinematic function runs before actors; palette fades and queued
                // tile replacements therefore precede the next sprite instruction.
                PostShot.Step(vram, cgram);
                RewardJump.Step();
                if (PostShot.ReadyForWhiteFlash)
                    BeginPostCreditsWhiteFlash();
                break;

            case EndingCreditsPhase.PostCreditsWhiteFlash:
                if (whiteFlashColor > 0) whiteFlashColor--;
                RewardJump.Step();
                if (--phaseTimer <= 0) FinishPostCreditsWhiteFlash();
                break;

            case EndingCreditsPhase.PostCreditsLogo:
                StepPostCreditsLogo();
                break;

            case EndingCreditsPhase.PostCreditsCopyright:
                // The sprite interpreter continues behind TM=$01; hiding the actors
                // must not reset their instruction lists or their reveal palette state.
                StepSprites();
                if (--phaseTimer <= 0)
                {
                    phaseTimer = EndingCreditsRomData.Timing.RewardRevealHalfFrames;
                    Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.ItemPercentage:
                PostCreditsText.Step(vram);
                if (PostCreditsText.RequestedItemPercentageScroll && PostCreditsText.Completed)
                {
                    phaseTimer = 40;
                    Phase = EndingCreditsPhase.ItemPercentageScrollDown;
                }
                break;

            case EndingCreditsPhase.ItemPercentageScrollDown:
                // Function 148 moves BG1 down two pixels per call from zero through -80.
                // The software renderer applies the equivalent scroll while preserving the
                // already-built tilemap until the final object is spawned.
                postCreditsScroll -= new Offset16<BgPlanePixels>(0, 2);
                if (--phaseTimer <= 0)
                {
                    postCreditsText = new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, Inventory = inventory, JapaneseText = japaneseText, DestinationTilemap = postCreditsTextTilemap, Presentation = endingText, InstalledSequence = endingText is null ? null : EndingTextSequence.FinalMessage });
                    Phase = EndingCreditsPhase.SeeYouNextMission;
                }
                break;

            case EndingCreditsPhase.SeeYouNextMission:
                // Retail parks in the final null cinematic function. There is no hidden
                // automatic reset: the console remains on this frame until reset/power-off.
                StepSprites();
                postCreditsText?.Step(vram);
                break;
        }

        // The native cinematic invokes the shared bank-$8D interpreter after actors.
        paletteFx.Step(cgram, paletteFxArtwork ?? throw new InvalidOperationException(
                "Ending palette FX requires installed palette colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        if (paletteFx.SoundRequests.Count != 0 || paletteFx.MusicRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled audio command.");
        // $8B:D46B follows cinematic actors even while a text-only TM masks OBJ.
        // F734 enables the already-initialized records when the credits finish.
        if (Phase >= EndingCreditsPhase.PostCreditsBlank)
            (shootingStars ??= new EndingShootingStars()).Step();
        cinematicFrame++;
    }

    /// <summary>Loads the first cloud scene, resets its Mode-7 transform and queues escape music.</summary>
    private void SetupEscapeSceneA()
    {
        LoadStaticPalette(EndingPaletteId.Escape, 0, 256, 0);
        LoadMode7(EndingMode7SceneId.EscapeA);
        LoadEscapeCloudCharacters();
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightTop, EndingSpriteRole.CloudRightA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftTop, EndingSpriteRole.CloudLeftA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightBottom, EndingSpriteRole.CloudRightB);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftBottom, EndingSpriteRole.CloudLeftB);
        mode7 = new(mode7.X.WithWhole(0), default);
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = EndingCreditsRomData.Motion.EscapeInitialAngle.ToAngle();
        brightness = 0;
        postCreditsScroll = Position16<BgPlanePixels>.Zero;
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        audio.QueueMusicDelayed8(MusicCommand.LoadData(
            EndingCreditsRomData.Music.EscapeDataIndex));
        audio.QueueMusicDelayed(
            MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
            MusicCommandDelay.FromDelayedYArgument(EndingCreditsRomData.Music.DelayArgument));
        Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    /// <summary>Installs the second cloud backdrop and grey-cloud palette effect before its fade-in.</summary>
    private void SetupEscapeSceneB()
    {
        ResetPaletteFx();
        paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.GreyClouds, 0);
        LoadMode7(EndingMode7SceneId.EscapeB);
        LoadEscapeCloudCharacters();
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopA, EndingSpriteRole.CloudTopA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopB, EndingSpriteRole.CloudTopB);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomA, EndingSpriteRole.CloudBottomA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomB, EndingSpriteRole.CloudBottomB);
        mode7 = new(mode7.X.WithWhole(0), default);
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = EndingCreditsRomData.Motion.EscapeInitialAngle.ToAngle();
        brightness = 0;
        fadeCounter = 1;
        Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    /// <summary>Loads the planet explosion's Mode-7 and OBJ art and starts its palette crossfade.</summary>
    private void SetupZebesExplosion()
    {
        LoadMode7(EndingMode7SceneId.PlanetExplosion);
        LoadEndingObjectCharacters();
        LoadStaticPalette(EndingPaletteId.Explosion,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            EndingCreditsRomData.Rendering.PaletteHalfBytes);
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionLava, EndingSpriteRole.ExplosionLava);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionStars, EndingSpriteRole.ExplosionStars);
        mode7 = new(mode7.X.WithWhole(0), default);
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        fadeCounter = 1;
        BeginExplosionCrossfade();
        Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    /// <summary>Clears flash backdrop colors and starts the accelerating planet flyaway with its authored palette effects.</summary>
    private void SetupPlanetEscape()
    {
        // Func120 clears the explosion flash's backdrop and transparent palette entries.
        var palettes = new FrontendPalettes(cgram);
        palettes.EndingBackdrop.Write(0, Bgr555.Black);
        palettes.EndingLogoBackground.Write(0, Bgr555.Black);
        palettes.EndingObjectTransparent.Write(0, Bgr555.Black);
        paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.PlanetAfterglow, 0);
        paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.GunshipEmergence, 0);
        mode7 = new(mode7.X.WithWhole(unchecked((ushort)-72)), new(unchecked((ushort)-104), 0));
        mode7Zoom = EndingCreditsRomData.Motion.PlanetEscapeInitialScale;
        mode7Angle = SnesAngle.NormalizeTableIndex(-112);
        phaseTimer = 192;
        flyawayWhite = EndingFlyawayFadeDefinitions.InitialWhite;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.InitialCountdown;
        planetMotionIndex = 0;
        Phase = EndingCreditsPhase.PlanetEscapeFast;
    }

    /// <summary>Replaces planet actors with the staff scroll, shooting stars and blank reward staging map.</summary>
    private void SetupCredits()
    {
        shootingStars = new EndingShootingStars();
        // Func126 clears palette objects before installing credits/reward palettes.
        ResetPaletteFx();
        LoadCreditsAndPostCreditsAssets();
        credits = new CreditsObjectState(staffCredits ?? throw new InvalidOperationException(
            "Ending credits require installed ending-credits.json content."));
        credits.UploadTilemap(vram);
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile);
        sprites.Clear();
        brightness = 15;
        Phase = EndingCreditsPhase.Credits;
    }

    /// <summary>Rebinds host-owned ending text after catalog reload or graph restoration.</summary>
    public void BindEndingText(EndingTextPresentation? value)
    {
        endingText = value;
        postCreditsText?.BindPresentation(value);
    }

    /// <summary>Rebinds the host-owned ending font after catalog reload or restoration.</summary>
    public void BindEndingFont(EndingFontAtlas? value) => endingFont = value;

    /// <summary>Rebinds host-owned staff credits after catalog reload or restoration.</summary>
    public void BindStaffCredits(CreditsPresentation? value)
    {
        staffCredits = value;
        credits?.BindPresentation(value);
    }

    /// <summary>Requires installed ending font art before a native VRAM transfer.</summary>
    private EndingFontAtlas ResolveEndingFont()
    {
        return endingFont ?? throw new InvalidOperationException(
            "Ending font requires installed presentation assets.");
    }

    /// <summary>Restores the post-credits palette except colors zero through three and holds a blank screen for sixty calls.</summary>
    internal void SetupPostCreditsBlank()
    {
        // F6FE copies Intro4 colors $04-$FF, disables text glow, forces blank, and arms
        // function 129 for sixty calls. Preserve colors zero through three as native does.
        LoadStaticPalette(EndingPaletteId.PostCredits,
            EndingCreditsRomData.Rendering.PostCreditsPaletteDestination,
            EndingCreditsRomData.Rendering.PostCreditsPaletteBytes,
            EndingCreditsRomData.Rendering.PostCreditsPaletteDestination);
        brightness = 0;
        UploadPostCreditsTilemap();
        phaseTimer = 60;
        Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    /// <summary>
    /// Selects current host artwork after a debugger-state restore. The active
    /// backdrop is refreshed without resetting actors, palette, transform or music.
    /// During the native flyaway DMA, already-uploaded chunks are reapplied last.
    /// </summary>
    internal void BindMode7Artwork(EndingMode7ArtworkCatalog? value)
    {
        mode7Artwork = value;
        if (value is null) return;
        if (Phase == EndingCreditsPhase.PostCreditsJump && rewardGraphics is not null)
        {
            rewardGraphics.BindArtwork(value.RewardIcon, vram);
            return;
        }
        EndingMode7SceneId? scene = Phase.Mode7Scene();
        if (scene is null) return;
        UploadMode7Artwork(value[scene.Value]);
        if (Phase == EndingCreditsPhase.ZebesExplosionTileUpload)
        {
            int completedChunks = Math.Clamp(16 - phaseTimer, 0, 16);
            for (int index = 0; index < completedChunks; index++)
                UploadFlyawayChunk(index);
        }
    }

    /// <summary>Reapplies the current ending OBJ uploads after a state restore.</summary>
    internal void BindObjectArtwork(EndingObjectArtworkCatalog? value)
    {
        objectArtwork = value;
        postShot?.BindArtwork(value, vram);
        if (value is null) return;
        if (Phase >= EndingCreditsPhase.PostCreditsWhiteFlash)
            EndingPostShot.RebindCompletedLogoArtwork(vram, value);
        if (Phase is >= EndingCreditsPhase.WaitForEscapeMusic and
            <= EndingCreditsPhase.FadeOutEscapeSceneB)
            LoadEscapeCloudCharacters();
        else if (Phase is >= EndingCreditsPhase.FadeInZebesExplosion and
            < EndingCreditsPhase.Credits)
            LoadEndingObjectCharacters();
        else if (creditsAssetsLoaded && Phase is >= EndingCreditsPhase.Credits and
            <= EndingCreditsPhase.PostCreditsGesture)
            LoadCreditsCharacterArt();
    }

    /// <summary>Clears VRAM and installs the selected ending scene's Mode-7 map and characters.</summary>
    private void LoadMode7(EndingMode7SceneId scene)
    {
        EndingMode7ArtworkCatalog artwork = mode7Artwork ?? throw new InvalidOperationException(
            "Ending Mode-7 scene requires installed artwork.");
        new VramStore(vram).Clear();
        UploadMode7Artwork(artwork[scene]);
    }

    /// <summary>Writes both Mode-7 map cells and their character bytes into live VRAM.</summary>
    private void UploadMode7Artwork(EndingMode7SceneArtwork scene)
    {
        var mode7 = new Mode7Vram(vram);
        mode7.LoadMap(scene.Map.Span);
        mode7.LoadMap(scene.Map.Span, cell: EndingMode7ArtworkFormat.MapByteCount);
        mode7.LoadCharacters(scene.Characters.Span);
    }

    /// <summary>Transfers the cloud object's character sheet into the ending OBJ lane.</summary>
    private void LoadEscapeCloudCharacters()
    {
        byte[] clouds = (objectArtwork ?? throw new InvalidOperationException(
            "Escape clouds require installed object artwork.")).Clouds.Transfer.ToArray();
        RequireMinimum(clouds, EndingCreditsRomData.Rendering.Mode7Bytes, "escape cloud characters");
        new EndingVram(vram).PostCreditsObjects.Load(clouds.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
    }

    /// <summary>Uploads explosion OBJ art, four fragments and the ending font at their native VRAM destinations.</summary>
    private void LoadEndingObjectCharacters()
    {
        byte[] main = (objectArtwork ?? throw new InvalidOperationException(
            "Ending objects require installed artwork.")).Explosion.Transfer.ToArray();
        // $8B:D8C1 uploads the complete explosion object sheet from $7F:8000.
        RequireMinimum(main, EndingCreditsRomData.Rendering.ExplosionObjectBytes,
            "ending OBJ characters");
        new EndingVram(vram).ExplosionObjects.Load(main.AsSpan(0, EndingCreditsRomData.Rendering.ExplosionObjectBytes));
        LoadObjectFragment(EndingObjectFragmentId.Segment70);
        LoadObjectFragment(EndingObjectFragmentId.Segment74);
        LoadObjectFragment(EndingObjectFragmentId.Segment78);
        LoadObjectFragment(EndingObjectFragmentId.Segment7C);
        ReadOnlyMemory<byte> font = ResolveEndingFont().Transfer;
        RequireMinimum(font, EndingCreditsRomData.Rendering.ObjectFragmentLimit,
            "ending font characters");
        new EndingVram(vram).EndingFont.Load(font.Span[..EndingCreditsRomData.Rendering.ObjectFragmentLimit]);
    }

    /// <summary>Copies one authored ending OBJ fragment into its named VRAM lane.</summary>
    private void LoadObjectFragment(EndingObjectFragmentId fragmentId)
    {
        byte[] fragment = (objectArtwork ?? throw new InvalidOperationException(
            "Ending object fragments require installed artwork.")).Fragment(fragmentId).Transfer.ToArray();
        RequireMinimum(fragment, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "ending OBJ fragment");
        new EndingVram(vram).ObjectFragment(fragmentId).Load(
            fragment.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    /// <summary>Performs the one-time palette, font and post-credits art transfers at the staff handoff.</summary>
    internal void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        LoadStaticPalette(EndingPaletteId.Credits, 0,
            EndingCreditsRomData.Rendering.PaletteHalfBytes, 0);
        ReadOnlyMemory<byte> font = ResolveEndingFont().Transfer;
        RequireMinimum(font, EndingCreditsRomData.Rendering.FontCharacterBytes,
            "credits font");

        new VramStore(vram).Clear();
        new EndingVram(vram).CreditsFont.Load(font.Span[..EndingCreditsRomData.Rendering.FontCharacterBytes]);
        LoadCreditsCharacterArt();

        creditsAssetsLoaded = true;
    }

    /// <summary>Uploads the indexed reward fragment requested by the active landing actor.</summary>
    private void UploadRewardGraphic(int index) =>
        (rewardGraphics ?? throw new InvalidOperationException(
            "Reward landing has no active graphics upload owner.")).Upload(vram, index);

    /// <summary>Transfers waiting, shooting and selected reward art plus their post-credits map fragments.</summary>
    private void LoadCreditsCharacterArt()
    {
        EndingObjectArtworkCatalog artwork = objectArtwork ?? throw new InvalidOperationException(
            "Post-credits scenes require installed object artwork.");
        byte[] waiting = artwork.WaitingSamus.Transfer.ToArray();
        byte[] shooting = artwork.ShootingScreen.Transfer.ToArray();
        byte[] waitingMap = artwork.WaitingTilemap.Transfer.ToArray();
        byte[] fragmentA = artwork.PostCreditsFragmentA.Transfer.ToArray();
        byte[] fragmentB = artwork.PostCreditsFragmentB.Transfer.ToArray();
        byte[] result = EndingReward == EndingReward.Suitless
            ? artwork.SuitlessSamus.Transfer.ToArray()
            : waiting;
        RequireMinimum(waiting, EndingCreditsRomData.Rendering.Mode7Bytes,
            "waiting-Samus characters");
        RequireMinimum(shooting, EndingCreditsRomData.Rendering.ShootingCharacterBytes,
            "post-credits OBJ characters");
        RequireMinimum(waitingMap, EndingCreditsRomData.Rendering.WaitingTilemapBytes,
            "waiting-Samus tilemap");
        RequireMinimum(fragmentA, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes,
            "post-credits tile fragment A");
        RequireMinimum(fragmentB, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "post-credits tile fragment B");
        RequireMinimum(result, EndingCreditsRomData.Rendering.Mode7Bytes,
            "post-credits reward characters");
        var ending = new EndingVram(vram);
        ending.WaitingCharacters.Load(waiting.AsSpan(0, EndingCreditsRomData.Rendering.WaitingCharacterBytes));
        ending.PostCreditsObjects.Load(shooting.AsSpan(0, EndingCreditsRomData.Rendering.ShootingCharacterBytes));
        ending.WaitingTilemap.Load(waitingMap.AsSpan(0, EndingCreditsRomData.Rendering.WaitingTilemapBytes));
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the
        // latter deliberately reuses the waiting-scene sheet, not Mode-7 art.
        ending.RewardCharacters.Load(result.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
        ending.PostCreditsFragmentA.Load(fragmentA.AsSpan(0, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes));
        ending.PostCreditsFragmentB.Load(fragmentB.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    /// <summary>Advances each cloud's zoom-relative motion and its private instruction stream.</summary>
    private void StepEscapeClouds()
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            EndingCloudMotion.Step(wrapper, mode7Zoom);
            sprite.Step();
        }
    }

    /// <summary>Rotates Mode 7 every other cinematic call while applying the current zoom delta.</summary>
    private void StepAtmosphericTransform(int scaleDelta)
    {
        // Func111/114 run during both fades as well as the fully visible interval.
        if ((cinematicFrame & 1) == 0)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        mode7Zoom += scaleDelta;
    }

    /// <summary>Visits live cinematic slots from high to low, allowing callback replacements in lower slots to run this frame.</summary>
    private void StepSprites()
    {
        // Native traversal re-reads each slot. A callback can replace a lower slot,
        // which then runs later in this same frame; higher slots wait until next frame.
        for (int slot = EndingSpriteSlots.Count - 1; slot >= 0; slot--)
        {
            EndingSprite? wrapper = sprites.Find(actor => actor.NativeSlot == slot && actor.Sprite.IsActive);
            if (wrapper is null) continue;
            StepEndingSpritePreInstruction(wrapper);
            wrapper.Sprite.Step(HandleSpriteCue);
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>Carries out the ending sprite programs' cues that spawn actors, trigger palette effects or advance the cinematic phase.</summary>
    private void HandleSpriteCue(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.FadeExplosionPalette:
                paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.FadePlanet, 0);
                new FrontendPalettes(cgram).EndingExplosionGlow.Write(0, new Bgr555(1, 0, 0));
                return;

            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                new FrontendPalettes(cgram).EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                return;

            case CinematicSpriteCueKind.StartZebesExplosion:
                explosionBurstDisplay = true;
                paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.Supernova, 0);
                paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.Explosion, 0);
                paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.WideExplosion, 0);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return;

            case CinematicSpriteCueKind.ExplosionFinale:
                explosionFinaleDisplay = true;
                paletteFx.SpawnDefinition(EndingPaletteFxDefinitions.SupernovaFinale, 0);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return;

            case CinematicSpriteCueKind.EndZebesExplosion:
                // F32B disables TM/TS without deleting the actors: the stars must still
                // advance and become visible again when Func120 starts the flyaway.
                var flash = new FrontendPalettes(cgram);
                flash.EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingObjectTransparent.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingLogoBackground.Fill(EndingCreditsRomData.Rendering.WhiteColor);
                phaseTimer = 120;
                Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return;

            case CinematicSpriteCueKind.SpawnCompletedText:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return;

            case CinematicSpriteCueKind.SpawnClearTime:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return;

            case CinematicSpriteCueKind.SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingCreditsRomData.Text.HoursTensX);
                return;

            case CinematicSpriteCueKind.SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingCreditsRomData.Text.HoursUnitsX);
                return;

            case CinematicSpriteCueKind.SpawnColon:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return;

            case CinematicSpriteCueKind.SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingCreditsRomData.Text.MinutesTensX);
                return;

            case CinematicSpriteCueKind.SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingCreditsRomData.Text.MinutesUnitsX);
                return;

            case CinematicSpriteCueKind.TransitionToCredits:
                fadeCounter = 1;
                Phase = EndingCreditsPhase.FadeOutToCredits;
                return;

            default:
                throw new InvalidOperationException($"The ending sprite programs publish no {cue} cue.");
        }
    }

    /// <summary>Chooses a clamped digit's authored sprite list for one clear-time column.</summary>
    private void SpawnDigit(int digit, ushort x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        SpawnSprite(
            new(x, EndingCreditsRomData.Sprites.ClearTimeDigitY),
            0,
            ClearTimeDigitProgram(normalized),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>The program showing clear-time <paramref name="digit"/>.</summary>
    private static CinematicSpriteProgram ClearTimeDigitProgram(int digit) => digit switch
    {
        0 => CinematicSpriteProgram.ClearTimeDigit0,
        1 => CinematicSpriteProgram.ClearTimeDigit1,
        2 => CinematicSpriteProgram.ClearTimeDigit2,
        3 => CinematicSpriteProgram.ClearTimeDigit3,
        4 => CinematicSpriteProgram.ClearTimeDigit4,
        5 => CinematicSpriteProgram.ClearTimeDigit5,
        6 => CinematicSpriteProgram.ClearTimeDigit6,
        7 => CinematicSpriteProgram.ClearTimeDigit7,
        8 => CinematicSpriteProgram.ClearTimeDigit8,
        9 => CinematicSpriteProgram.ClearTimeDigit9,
        _ => throw new ArgumentOutOfRangeException(nameof(digit), digit, "A clear-time digit is 0..9."),
    };

    /// <summary>Installs the two-piece Samus reward art selected by clear-time thresholds.</summary>
    private void SpawnEndingRewardActors()
    {
        sprites.Clear();
        switch (EndingReward)
        {
            case EndingReward.Suitless:
                SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Helmetless:
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.HelmetlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Armored:
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardHead, EndingSpriteRole.RewardSamus);
                break;
            default:
                throw new InvalidOperationException($"Undefined ending reward {EndingReward}.");
        }
    }

    /// <summary>Copies the current result-panel staging words to the phase-selected VRAM tilemap destination.</summary>
    [PpuLayoutBoundary("codec: post-credits staging tilemap DMA to its scene-selected VMADD")]
    private void UploadPostCreditsTilemap() =>
        vram.ExecuteWordTransfer(
            postCreditsTilemap,
            EndingVram.TextTilemapWord(postCreditsTextTilemap),
            wordIncrement: 1);

    /// <summary>Allocates or replaces a native cinematic actor slot, reserving fixed slots for stars, afterglow and animals.</summary>
    private void SpawnSprite(
        Position16<ScreenPixels> position,
        ushort palette,
        CinematicSpriteProgram program,
        EndingSpriteRole role)
    {
        int slot = role switch
        {
            EndingSpriteRole.ExplosionStarsRight => EndingSpriteSlots.RightStars,
            EndingSpriteRole.ExplosionStarsLeft => EndingSpriteSlots.LeftStars,
            EndingSpriteRole.ExplosionAfterglow => EndingSpriteSlots.Afterglow,
            EndingSpriteRole.AnimalEscape => EndingAnimalEscapeDefinitions.NativeSlot,
            // Every other actor takes the highest free dynamic slot.
            EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudRightB or
            EndingSpriteRole.CloudLeftB or EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB or
            EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB or EndingSpriteRole.ExplodingZebes or
            EndingSpriteRole.ExplosionLava or EndingSpriteRole.ExplosionGlow or EndingSpriteRole.ExplosionStars or
            EndingSpriteRole.ExplosionSilhouette or EndingSpriteRole.OperationWasText or
            EndingSpriteRole.CompletedSuccessfullyText or EndingSpriteRole.ClearTimeText or
            EndingSpriteRole.ClearTimeDigit or EndingSpriteRole.RewardSamus => Enumerable.Range(0, EndingSpriteSlots.Count).Reverse()
                .FirstOrDefault(candidate => !sprites.Any(actor => actor.NativeSlot == candidate && actor.Sprite.IsActive), -1),
            _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role."),
        };
        if (slot < 0) throw new InvalidOperationException("Ending cinematic actor slots exhausted.");
        sprites.RemoveAll(actor => actor.NativeSlot == slot);
        sprites.Add(new EndingSprite(new IntroDiscoverySprite(position, palette, program), role)
            { NativeSlot = slot });
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            sprites[^1].Sprite.InstallPreInstruction(CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway);
    }

    /// <summary>Installs a sprite from its authored position, attributes and program.</summary>
    private void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.Position,
            definition.Attributes.Raw,
            definition.Program,
            role);

    /// <summary>Runs the first rotating planet-escape motion pattern until the fast zoom threshold.</summary>
    internal void StepPlanetEscapeFast()
    {
        if (phaseTimer > 0)
            phaseTimer--;
        else StepFlyawayFade();
        mode7Angle = mode7Angle.AddTableUnits(-4);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, EndingCreditsRomData.Motion.PlanetFastDelta(planetMotionIndex)));
        planetMotionIndex = (planetMotionIndex + 1) % EndingCreditsRomData.Motion.PlanetFastPatternLength;
        mode7Zoom -= 8;
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetFastEndScale)
        {
            planetMotionIndex = 0;
            Phase = EndingCreditsPhase.PlanetEscapeSlow;
        }
    }

    /// <summary>Runs the slower motion pattern and starts the accelerating leg, optionally spawning the escaped animals.</summary>
    internal void StepPlanetEscapeSlow()
    {
        StepFlyawayFade();
        if (mode7Angle != EndingCreditsRomData.Motion.PlanetSlowTargetAngle)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, EndingCreditsRomData.Motion.PlanetSlowDelta(planetMotionIndex)));
        planetMotionIndex = (planetMotionIndex + 1) % EndingCreditsRomData.Motion.PlanetSlowPatternLength;
        mode7Zoom -= 2;
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetSlowEndScale)
        {
            planetVelocity = SnesSignedSixteenSixteen.FromParts(0, EndingCreditsRomData.Motion.InitialAccelerationFraction);
            Phase = EndingCreditsPhase.PlanetEscapeAccelerating;
            if (crittersEscaped)
            {
                SpawnSprite(new(EndingAnimalEscapeDefinitions.Origin, EndingAnimalEscapeDefinitions.Origin),
                    EndingAnimalEscapeDefinitions.Palette, CinematicSpriteProgram.EndingAnimalEscape,
                    EndingSpriteRole.AnimalEscape);
                IntroDiscoverySprite pod = sprites[^1].Sprite;
                pod.GeneralTimer = EndingAnimalEscapeDefinitions.GeneralTimer;
                // Native cinematic dispatch precedes actors, so a newly spawned pod
                // moves and selects its first animation frame on this same call.
                StepEndingSpritePreInstruction(sprites[^1]);
                pod.Step();
            }
        }
    }

    /// <summary>Accumulates split planet velocity and turns toward the final gunship text at the exit scale.</summary>
    private void StepPlanetEscapeAccelerating()
    {
        StepFlyawayFade();
        planetVelocity -= SnesSignedSixteenSixteen.FromRaw(EndingCreditsRomData.Motion.AccelerationDelta16Point16);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, planetVelocity.RawValue));
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetTurnStartScale &&
            (cinematicFrame & 3) == 0 &&
            mode7Angle != EndingCreditsRomData.Motion.PlanetExitTargetAngle)
        {
            mode7Angle = mode7Angle.AddTableUnits(2);
        }
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetExitScale)
        {
            LoadStaticPalette(EndingPaletteId.FinalGunship, 0, 16, 80);
            SpawnSprite(
                EndingCreditsRomData.Sprites.OperationWasText,
                EndingSpriteRole.OperationWasText);
            Phase = EndingCreditsPhase.OperationSuccessfulText;
        }
        else
        {
            mode7Zoom -= 4;
        }
    }

    /// <summary>Raises native brightness one step every two calls until full brightness.</summary>
    private bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Lowers native brightness one step every two calls until blank.</summary>
    private bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = Math.Max(0, brightness - 1);
        }
        return brightness == 0;
    }

    /// <summary>Raises native brightness one step every four calls for the post-credits reveal.</summary>
    private bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            brightness = Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Adds a signed 16.16 motion delta to a Mode-7 coordinate with native word wrapping.</summary>
    private static SnesFixedPosition AddSignedFixed(SnesFixedPosition position, int delta) =>
        SnesSignedSixteenSixteen
            .FromRaw(delta)
            .AddTo(position.Whole, position.Fraction);

    /// <summary>Rejects decoded artwork too short for its native VRAM transfer span.</summary>
    private static void RequireMinimum(ReadOnlyMemory<byte> data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Replaces the producer result rows with the copyright panel and starts its authored hold.</summary>
    private void ShowRewardCopyright()
    {
        // Func135 removes the producer panel and replaces rows twelve/thirteen with
        // the literal DEDB copyright tilemap. BG1 alone is visible until Func137 ends.
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingCreditsRomData.Text.ResultPanelDestination, EndingCreditsRomData.Text.ResultPanelWords);
        (endingText ?? throw new InvalidOperationException(
            "Ending copyright requires installed text assets.")).BuildCopyrightPanel().CopyTo(
            postCreditsTilemap, EndingCreditsRomData.Text.CopyrightPanelDestination);
        UploadPostCreditsTilemap();
        rewardCopyrightShown = true;
        phaseTimer = EndingCreditsRomData.Timing.CopyrightHoldFrames;
        Phase = EndingCreditsPhase.PostCreditsCopyright;
    }

    /// <summary>Whether the explosion actor installed the final BG and subscreen arrangement.</summary>
    private bool explosionFinaleDisplay;
    /// <summary>Whether the explosion actor installed its initial burst arrangement.</summary>
    private bool explosionBurstDisplay;
    /// <summary>Whether the explosion currently uses its dedicated color-math composition.</summary>
    internal bool UsesExplosionFinaleDisplay => (explosionFinaleDisplay || explosionBurstDisplay) && !Presentation.ExplosionWhiteout
        && Phase < EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>Captures the finale BG priority planes and one winning subscreen color-add layer.</summary>
    internal LayeredRenderSnapshot CaptureExplosionFinale()
    {
        if (!explosionFinaleDisplay) return CaptureExplosionBurst();
        // F2FA: TM=BG1|BG2, TS=BG2|OBJ, CGADSUB=$33. Resolve the two
        // main backgrounds by priority, then add the single winning subscreen pixel.
        // OBJ is not independently drawn on main, which would punch holes in the glow.
        RenderLayer[] layers =
        [
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, false),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, false),
            Plane(EndingExplosionDisplayDefinitions.FinaleSubMap, true),
            Plane(EndingExplosionDisplayDefinitions.FinaleMainMap, true),
            new BgSubscreenAddRenderLayer(EndingExplosionDisplayDefinitions.FinaleSubMap,
                EndingExplosionDisplayDefinitions.Characters, FourBpp: true, IncludeObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, Presentation.PrepareSprites()), layers,
            EndingCreditsPresentation.ObjectSelection(Presentation.CurrentEscapeObjectSheet), checked((byte)brightness));

        static Bg4BppRenderLayer Plane(ushort map, bool high) => new(map,
            EndingExplosionDisplayDefinitions.Characters,
            new Position16<BgPlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
            new Extent16<TilemapCells>(32, 32), high);
    }

    /// <summary>Captures the burst's BG1/OBJ main screen and BG2 subscreen color math.</summary>
    private LayeredRenderSnapshot CaptureExplosionBurst()
    {
        // F2B7: BG1 and OBJ on main, BG2 on sub, math only for BG1 and
        // eligible OBJ palettes. Keep the backdrop and OBJ palettes 0–3 unmodified.
        var main = new Bg4BppRenderLayer(EndingExplosionDisplayDefinitions.InitialMap,
            EndingExplosionDisplayDefinitions.Characters, new Position16<BgPlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow), new Extent16<TilemapCells>(32, 32), null);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
            main with { Priority = false }, new ObjPriorityRenderLayer(2),
            main with { Priority = true }, new ObjPriorityRenderLayer(3),
            new BgSubscreenAddRenderLayer(EndingExplosionDisplayDefinitions.BurstSubMap,
                EndingExplosionDisplayDefinitions.Characters, main, FourBpp: true, MainObjects: true,
                VerticalScroll: EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, Presentation.PrepareSprites()), layers,
            EndingCreditsPresentation.ObjectSelection(Presentation.CurrentEscapeObjectSheet), checked((byte)brightness));
    }

    /// <summary>CGRAM snapshot taken before the planet explosion's object/background crossfade.</summary>
    private Bgr555[] explosionFadeSource = [];
    /// <summary>Current native palette interpolation step during the explosion crossfade.</summary>
    private int explosionFadeStep;
    /// <summary>Whether the explosion's palette interpolation still owns CGRAM writes.</summary>
    internal bool ExplosionCrossfadeActive => Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    /// <summary>Stops cloud palette effects, captures CGRAM and seeds the planet object's first fade step.</summary>
    private void BeginExplosionCrossfade()
    {
        // Func115 clears the previous cloud palette programs before taking its source
        // snapshot; otherwise their next instruction can overwrite the new OBJ fade.
        ResetPaletteFx();
        explosionFadeSource = new CgramPalettes(cgram).Image.ToArray();
        explosionFadeStep = 0;
        ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, 0);
        ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, 0);
        phaseTimer = EndingExplosionFadeDefinitions.InitialCountdown;
    }

    /// <summary>Interpolates explosion palettes on alternating calls and enters the sixteen-chunk flyaway transfer.</summary>
    private void StepExplosionCrossfade()
    {
        mode7Zoom += EndingExplosionFadeDefinitions.ZoomStep;
        if ((phaseTimer & 1) == 0)
        {
            explosionFadeStep++;
            ApplyExplosionFade(EndingExplosionFadeDefinitions.BackgroundStart,
                EndingExplosionFadeDefinitions.Steps - explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.FirstObjectStart, explosionFadeStep);
            ApplyExplosionFade(EndingExplosionFadeDefinitions.SecondObjectStart, explosionFadeStep);
        }
        if (--phaseTimer < 0)
        {
            phaseTimer = 16;
            PrepareFlyawayUploads();
            Phase = EndingCreditsPhase.ZebesExplosionTileUpload;
        }
    }

    /// <summary>Scales one native explosion palette range from the captured CGRAM source by the current fade factor.</summary>
    [PpuLayoutBoundary("codec: native ending fade over its CGRAM index range")]
    private void ApplyExplosionFade(int start, int factor)
    {
        for (int i = start; i < start + EndingExplosionFadeDefinitions.Colors; i++)
        {
            new CgramPalettes(cgram).All.Write(i, explosionFadeSource[i].Map((_, channel) =>
                channel * factor / EndingExplosionFadeDefinitions.Steps));
        }
    }

    /// <summary>Fixed-color white component over the planet escape, from full white (31) fading to zero, where it stays.</summary>
    private int flyawayWhite;
    /// <summary>Countdown between native flyaway fixed-color decrements.</summary>
    private int flyawayFadeCountdown;
    /// <summary>Fixed-color addition active only during visible planet flyaway phases.</summary>
    internal FixedColorAddRenderLayer? FlyawayFixedColor => flyawayWhite != 0
        && Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase <= EndingCreditsPhase.FadeOutToCredits
        ? WhiteAddition(flyawayWhite) : null;

    /// <summary>Adds the same five-bit <paramref name="white"/> component to all three channels of the composed screen.</summary>
    internal static FixedColorAddRenderLayer WhiteAddition(int white)
    {
        byte component = checked((byte)white);
        return new(component, component, component);
    }

    /// <summary>Decreases the flyaway white component at its cartridge interval.</summary>
    private void StepFlyawayFade()
    {
        if (--flyawayFadeCountdown > 0) return;
        if (flyawayWhite > 0) flyawayWhite--;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.Interval;
    }

    /// <summary>Decoded Mode-7 character lane uploaded in the first eight flyaway chunks.</summary>
    private byte[] flyawayCharacters = [];
    /// <summary>Padded Mode-7 map lane uploaded in the remaining flyaway chunks.</summary>
    private byte[] flyawayMap = [];

    /// <summary>
    /// The ending's first flyaway reuses the Ceres approach's cartridge source.
    /// Rebind only chunks already transferred by the native 16-NMI DMA sequence;
    /// future chunks must still arrive on their original scheduled frames.
    /// </summary>
    internal void BindFlightArtwork(CeresFlightArtworkCatalog? value)
    {
        flightArtwork = value;
        if (value is null || Phase < EndingCreditsPhase.ZebesExplosionTileUpload ||
            Phase >= EndingCreditsPhase.FadeOutToCredits)
            return;
        PrepareFlyawayStreams();
        int completedChunks = Phase == EndingCreditsPhase.ZebesExplosionTileUpload
            ? Math.Clamp(16 - phaseTimer, 0, 16) : 16;
        for (int index = 0; index < completedChunks; index++)
            UploadFlyawayChunk(index);
    }

    /// <summary>Prepares Ceres-derived flyaway streams while restoring the explosion's BG palette half.</summary>
    private void PrepareFlyawayUploads()
    {
        PrepareFlyawayStreams();
        // Func117 restores the BG half before disabling BG1. OBJ VRAM and palettes
        // remain live while Func118 replaces the two Mode-7 lanes over sixteen NMIs.
        LoadStaticPalette(EndingPaletteId.Explosion, 0, 128, 0);
    }

    /// <summary>Copies and pads Ceres flight map and character bytes for the native sixteen-NMI DMA sequence.</summary>
    private void PrepareFlyawayStreams()
    {
        CeresFlightArtworkCatalog artwork = flightArtwork ?? throw new InvalidOperationException(
            "Ending flyaway requires installed flight artwork.");
        flyawayCharacters = artwork.Mode7Characters.ToArray();
        byte[] decodedMap = artwork.Mode7Maps.ToArray();
        RequireMinimum(flyawayCharacters, EndingCreditsRomData.Rendering.Mode7Bytes, "flyaway characters");
        RequireMinimum(decodedMap, EndingCreditsRomData.Rendering.FlyawayMapDataBytes, "flyaway map");
        flyawayMap = new byte[EndingCreditsRomData.Rendering.Mode7Bytes];
        decodedMap.AsSpan(0, EndingCreditsRomData.Rendering.FlyawayMapDataBytes).CopyTo(flyawayMap);
        flyawayMap.AsSpan(EndingCreditsRomData.Rendering.FlyawayMapDataBytes,
            EndingCreditsRomData.Rendering.Mode7Bytes - EndingCreditsRomData.Rendering.FlyawayMapDataBytes)
            .Fill(EndingCreditsRomData.Rendering.FlyawayBlankTile);
    }

    /// <summary>Transfers one scheduled flyaway chunk to its Mode-7 character or map lane.</summary>
    private void UploadFlyawayChunk(int index)
    {
        int chunksPerLane = EndingCreditsRomData.Rendering.Mode7Bytes / EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        int offset = index % chunksPerLane * EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        if (index < chunksPerLane)
            new Mode7Vram(vram).LoadCharacters(flyawayCharacters.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), offset);
        else
            new Mode7Vram(vram).LoadMap(flyawayMap.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), offset);
    }

    /// <summary>Whether the current cinematic phase leaves its escape or planet backdrop visible.</summary>
    internal bool EscapeBackgroundEnabled => Phase is < EndingCreditsPhase.ZebesExplosionTileUpload
        or >= EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>Reloadable authored static palettes used at ending scene boundaries.</summary>
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    /// <summary>Reloadable editable color payloads for native palette-FX objects.</summary>
    [NonSerialized] private RoomPaletteFxPresentation? paletteFxArtwork;

    /// <summary>
    /// Rebinds authored ending colors after state restoration. The current CGRAM image
    /// remains serialized live state: overwriting it here would discard a partially
    /// completed native fade or palette-FX step. Future native palette transfers and
    /// target-color reads use the newly selected resource.
    /// </summary>
    internal void BindPaletteArtwork(EndingPaletteCatalog? value)
    {
        paletteArtwork = value;
        endingLogo?.BindPaletteArtwork(value);
    }

    /// <summary>
    /// Uses the same editable bank-$8D color payloads as gameplay while leaving
    /// ending-specific object allocation, palette destinations and timing native.
    /// Rebinding affects future interpreter reads without resetting a live fade.
    /// </summary>
    internal void BindPaletteFxColors(RoomPaletteFxPresentation? value)
    {
        paletteFxArtwork = value;
    }

    /// <summary>Native scene boundaries clear slots but retain installed color ownership.</summary>
    private void ResetPaletteFx()
    {
        paletteFx = new RoomPaletteFxSystem();
    }

    /// <summary>Copies a range of authored ending colors into the live CGRAM destination.</summary>
    private void LoadStaticPalette(EndingPaletteId id, int sourceColor, int count,
        int destinationColor)
    {
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].LoadTo(cgram, sourceColor, count, destinationColor);
    }

    /// <summary>Reads one authored palette target without replacing the current fade state.</summary>
    private Bgr555 StaticPaletteColor(EndingPaletteId id, int color)
        => (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].Color(color);

    /// <summary>Moves animal and star actors and deletes subordinate explosion layers at their native handoffs.</summary>
    private void StepEndingSpritePreInstruction(EndingSprite wrapper)
    {
        IntroDiscoverySprite sprite = wrapper.Sprite;
        switch (wrapper.Role)
        {
            case EndingSpriteRole.AnimalEscape:
                uint y = IntroCinematicMotion.Packed(sprite.Position.Y) + EndingAnimalEscapeDefinitions.YFractionVelocity;
                sprite.Position = new(
                    sprite.Position.X.WithWhole(unchecked((ushort)(sprite.Position.X.Whole + EndingAnimalEscapeDefinitions.XVelocity))),
                    IntroCinematicMotion.Unpacked(y));
                if (sprite.Position.X.Whole >= EndingAnimalEscapeDefinitions.DeleteX)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionGlow:
            case EndingSpriteRole.ExplosionStars:
            case EndingSpriteRole.ExplosionLava:
                // F2A5 observes the original planet actor's instruction pointer, not a
                // cinematic timer. These subordinate layers disappear with that actor.
                if (!sprites.Any(actor => actor.Role == EndingSpriteRole.ExplodingZebes && actor.Sprite.IsActive))
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionStarsRight:
            case EndingSpriteRole.ExplosionStarsLeft:
                // The right starfield runs no flyaway pre-instruction until its list installs $F35A.
                if (sprite.PreInstruction is CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway or CinematicSpritePreInstruction.ZebesExplosionStarsMoving)
                    StepFlyawayStars(sprite, sprite.PreInstruction.Value);
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            case EndingSpriteRole.ExplosionAfterglow:
                // F39B deletes the afterglow at the same native null-function handoff.
                if (Phase == EndingCreditsPhase.OperationSuccessfulText)
                    sprite.Delete();
                break;
            // These actors run no pre-instruction.
            case EndingSpriteRole.CloudRightA:
            case EndingSpriteRole.CloudLeftA:
            case EndingSpriteRole.CloudRightB:
            case EndingSpriteRole.CloudLeftB:
            case EndingSpriteRole.CloudTopA:
            case EndingSpriteRole.CloudTopB:
            case EndingSpriteRole.CloudBottomA:
            case EndingSpriteRole.CloudBottomB:
            case EndingSpriteRole.ExplodingZebes:
            case EndingSpriteRole.ExplosionSilhouette:
            case EndingSpriteRole.OperationWasText:
            case EndingSpriteRole.CompletedSuccessfullyText:
            case EndingSpriteRole.ClearTimeText:
            case EndingSpriteRole.ClearTimeDigit:
            case EndingSpriteRole.RewardSamus:
                break;
            default:
                throw new InvalidOperationException($"Undefined ending sprite role {wrapper.Role}.");
        }
    }

    /// <summary>Starts starfield motion at flyaway and advances its aliased Y-fraction velocity word.</summary>
    private void StepFlyawayStars(IntroDiscoverySprite sprite, CinematicSpritePreInstruction preInstruction)
    {
        switch (preInstruction)
        {
            case CinematicSpritePreInstruction.ZebesExplosionStarsWaitForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.InstallPreInstruction(CinematicSpritePreInstruction.ZebesExplosionStarsMoving);
                    // The Y subposition word doubles as the low velocity word.
                    sprite.Position = sprite.Position.WithY(sprite.Position.Y.WithFraction((ushort)(SnesSixteenSixteen.FromWhole(1) / 4)));
                    sprite.GeneralTimer = 0;
                }
                break;
            case CinematicSpritePreInstruction.ZebesExplosionStarsMoving:
                int velocity = unchecked((int)(SnesSixteenSixteen.Compose(sprite.GeneralTimer, sprite.Position.Y.Fraction))) - 32;
                sprite.GeneralTimer = unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)));
                uint position = unchecked(IntroCinematicMotion.Packed(sprite.Position.X) + (uint)velocity);
                sprite.Position = new(
                    IntroCinematicMotion.Unpacked(position),
                    sprite.Position.Y.WithFraction(unchecked((ushort)velocity)));
                break;
            default:
                throw new InvalidOperationException(
                    $"Flyaway stars do not run {preInstruction}.");
        }
    }

    /// <summary>Fades the post-credits waiting backdrop toward its authored palette.</summary>
    private void ApplyWaitingBackdropPalette(int step)
        => ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.WaitingPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, step);

    /// <summary>Interpolation step for suitless or helmetless reward colors.</summary>
    private int rewardPaletteStep;

    /// <summary>Crossfades waiting colors into the selected non-armored reward palette.</summary>
    private void ApplyRewardPalette()
    {
        if (EndingReward == EndingReward.Armored) return;
        ApplyWaitingBackdropPalette(EndingCreditsRomData.Timing.WaitingPaletteFadeFrames - rewardPaletteStep);
        ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.SuitlessRewardPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, rewardPaletteStep);
        if (EndingReward == EndingReward.Helmetless)
            ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.SuitedRewardPaletteStart,
                EndingCreditsRomData.Rendering.WaitingPaletteCount, rewardPaletteStep);
    }

    /// <summary>Applies native 8.8 component truncation to a post-credits CGRAM range.</summary>
    [PpuLayoutBoundary("codec: native ending fade over its CGRAM index range")]
    private void ApplyPostCreditsPaletteRange(int start, int count, int step)
    {
        // E110 saves Intro4 as its target, clears colors $20-$2F, then E158 runs the
        // shared 8CB2 component accumulator 32 times. Each increment is component<<3
        // in 8.8 precision; composing takes only its high byte, preserving truncation.
        for (int index = start; index < start + count; index++)
        {
            Bgr555 target = StaticPaletteColor(EndingPaletteId.PostCredits, index);
            new CgramPalettes(cgram).All.Write(index, target.Map((_, channel) => SnesEightEight.Multiply(channel << 3, step)));
        }
    }

    /// <summary>Background map the post-credits staging tilemap is uploaded into, switched by the white flash.</summary>
    internal EndingTextTilemap postCreditsTextTilemap = EndingTextTilemap.ResultPanel;
    /// <summary>Visible result-panel map height, enlarged for the final shot scene.</summary>
    internal int postCreditsMapHeight = 32;
    /// <summary>Fixed-color white component of the brief transition into the final logo, fading from 31 to zero, where it stays.</summary>
    internal int whiteFlashColor;
    /// <summary>Final logo actor created after the white flash.</summary>
    internal EndingLogo? endingLogo;
    /// <summary>The state of the ending logo, which exists while that phase runs.</summary>
    internal EndingLogo EndingLogo => endingLogo ??
        throw new InvalidOperationException("The ending logo is not running.");

    /// <summary>Restores the shooting palette, switches to the tall text map and begins the white transition.</summary>
    private void BeginPostCreditsWhiteFlash()
    {
        postShot = null;
        // E48A restores the shooting palette and selects BG1SC=$4E: the final
        // text map is two screens tall, not the credits' single-screen map.
        LoadStaticPalette(EndingPaletteId.PostCredits,
            EndingPostShotDefinitions.ShootingPaletteStart,
            EndingPostShotDefinitions.PaletteColors,
            EndingPostShotDefinitions.ShootingPaletteStart);
        postCreditsTextTilemap = EndingTextTilemap.FinalText;
        postCreditsMapHeight = EndingPostShotDefinitions.FinalTextMapHeight;
        postCreditsScroll = Position16<BgPlanePixels>.Zero;
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingPostShotDefinitions.ClearedCopyrightStart, EndingPostShotDefinitions.ClearedCopyrightWords);
        UploadPostCreditsTilemap();
        whiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        phaseTimer = EndingPostShotDefinitions.FadeFrames;
        Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Copies the staged lower tilemap and installs the final logo actor.</summary>
    private void FinishPostCreditsWhiteFlash()
    {
        // E504 copies the same staging map into the lower screen before the logo
        // actors enter. Subsequent percentage writes target the upper screen again.
        new EndingVram(vram).PostShotLowerTilemap.Write(postCreditsTilemap);
        rewardJump = null;
        sprites.Clear();
        endingLogo = new EndingLogo(cgram,
            () => paletteFx.SpawnDefinition(EndingLogoDefinitions.LandingPaletteFx, 0),
            paletteArtwork);
        Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Advances logo instructions and starts the item-percentage text once the logo completes.</summary>
    private void StepPostCreditsLogo()
    {
        EndingLogo.Step(cgram);
        if (!EndingLogo.Completed) return;
        postCreditsText = new EndingBackgroundTextState(new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, Inventory = inventory, JapaneseText = japaneseText, DestinationTilemap = EndingTextTilemap.FinalText, Presentation = endingText, InstalledSequence = endingText is null ? null : SuperMetroid.Core.Frontend.Ending.Artwork.EndingTextSequence.ItemPercentage });
        Phase = EndingCreditsPhase.ItemPercentage;
    }
}
