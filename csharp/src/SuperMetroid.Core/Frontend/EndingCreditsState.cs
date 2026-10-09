using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend;

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
internal sealed partial class EndingCreditsState
{
    /// <summary>Address space used by sprite interpreters, palette effects, and ending-specific memory reads.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Queues music and sound commands at the same points as the ending dispatcher.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Clear-time hours used for the result display and reward tier.</summary>
    private readonly ushort gameTimeHours;
    /// <summary>Clear-time minutes rendered on the completion screen.</summary>
    private readonly ushort gameTimeMinutes;
    /// <summary>Inventory snapshot consumed by the final item-percentage text sequence.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Selects the Japanese text sequence when the ending text assets provide one.</summary>
    private readonly bool japaneseText;
    /// <summary>Controls whether the animal pod joins the planet escape.</summary>
    private readonly bool crittersEscaped;
    /// <summary>Software mirror of video character and tilemap memory for ending scenes.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Software mirror of the color palette modified by cinematic fades and effects.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Active actors, kept with their native object-slot identities for ordered updates.</summary>
    private readonly List<EndingSprite> sprites = [];
    /// <summary>Tilemap assembled for the post-credit result panel and final messages.</summary>
    private readonly ushort[] postCreditsTilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];

    /// <summary>Incremental staff-roll state, created when the ending enters the credits.</summary>
    private CreditsObjectState? credits;
    /// <summary>Post-credit star actor state, advanced after the cinematic function.</summary>
    private EndingShootingStars? shootingStars;
    /// <summary>Active background-text sequence for the item percentage or final message.</summary>
    private EndingBackgroundTextState? postCreditsText;
    /// <summary>Number of dispatcher calls elapsed, used by cadence-sensitive cinematic motion.</summary>
    private ushort cinematicFrame;
    /// <summary>NMI waits <c>CinematicFunction_Ending_Setup</c> has made so far.</summary>
    private int setupNmiWaits;
    /// <summary>Countdown for waits and timed segments within the current phase.</summary>
    private int phaseTimer;
    /// <summary>Subframe counter controlling how often brightness changes during a fade.</summary>
    private int fadeCounter;
    /// <summary>SNES screen brightness in the native 0 through 15 range.</summary>
    private byte brightness;
    /// <summary>Mode-7 horizontal camera position, stored as the native whole component.</summary>
    private ushort mode7X;
    /// <summary>Fractional component paired with <see cref="mode7X"/> for signed fixed motion.</summary>
    private ushort mode7XSubposition;
    /// <summary>Mode-7 vertical camera position.</summary>
    private ushort mode7Y;
    /// <summary>Mode-7 scale used by the escape, explosion, and planet-flyaway scenes.</summary>
    private ushort mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
    /// <summary>Mode-7 rotation angle used by the planetary backgrounds.</summary>
    private SnesAngle mode7Angle;
    /// <summary>Index into the active planet motion pattern.</summary>
    private ushort planetMotionIndex;
    /// <summary>BG1 vertical offset during the final message's upward scroll.</summary>
    private ushort postCreditsVerticalScroll;
    /// <summary>Signed whole portion of the planet's 16.16 horizontal flyaway velocity.</summary>
    private short planetVelocityWhole;
    /// <summary>Unsigned fractional portion paired with <see cref="planetVelocityWhole"/>.</summary>
    private ushort planetVelocityFraction;
    /// <summary>Whether the shared credits font and character sheets have been installed in VRAM.</summary>
    private bool creditsAssetsLoaded;
    /// <summary>Prevents the reward screen's copyright panel from being shown more than once.</summary>
    private bool rewardCopyrightShown;
    /// <summary>Interpreter for the reward pose that precedes Samus's jump.</summary>
    private EndingRewardGesture? rewardGesture;
    /// <summary>Interpreter for the reward jump and landing sequence.</summary>
    private EndingRewardJump? rewardJump;
    /// <summary>Destination-specific graphics uploader invoked by reward-landing callbacks.</summary>
    private EndingRewardGraphicsUpload? rewardGraphics;
    /// <summary>State for the post-credits shot, palette fade, and tile replacement.</summary>
    private EndingPostShot? postShot;
    /// <summary>Bank-$8D palette programs used by the ending's color transitions.</summary>
    private RoomPaletteFxSystem paletteFx = new();
    /// <summary>Installed editable strings and text sequences used by ending screens.</summary>
    [NonSerialized] private EndingTextPresentation? endingText;
    /// <summary>Font atlas whose glyph transfers are copied into ending VRAM.</summary>
    [NonSerialized] private EndingFontAtlas? endingFont;
    /// <summary>Prepared staff-credit rows consumed by the incremental credits renderer.</summary>
    [NonSerialized] private CreditsPresentation? staffCredits;
    /// <summary>Installed Ceres flight artwork retained for state serialization boundaries.</summary>
    [NonSerialized] private CeresFlightArtworkCatalog? flightArtwork;
    /// <summary>Mode-7 scene and reward-icon artwork required by cinematic transitions.</summary>
    [NonSerialized] private EndingMode7ArtworkCatalog? mode7Artwork;
    /// <summary>Object character sheets and tilemaps used by ending actors and backdrops.</summary>
    [NonSerialized] private EndingObjectArtworkCatalog? objectArtwork;

    /// <summary>Creates the ending dispatcher with the completed run's results and installed text.</summary>
    /// <param name="bus">Address space for native actor data and cinematic memory interactions.</param>
    /// <param name="audio">Audio queue receiving ending music and sound requests.</param>
    /// <param name="gameTimeHours">Recorded clear-time hours used for display and reward selection.</param>
    /// <param name="gameTimeMinutes">Recorded clear-time minutes used for display.</param>
    /// <param name="inventory">Inventory snapshot used to calculate the final item percentage.</param>
    /// <param name="japaneseText">Whether Japanese ending strings are selected when available.</param>
    /// <param name="endingText">Installed text and font presentation data; required when text panels are reached.</param>
    /// <param name="crittersEscaped">Whether the animals escaped and should appear in the planet flyaway.</param>
    public EndingCreditsState(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        ushort gameTimeHours,
        ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default,
        bool japaneseText = false,
        EndingTextPresentation? endingText = null,
        bool crittersEscaped = false)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.gameTimeHours = gameTimeHours;
        this.gameTimeMinutes = gameTimeMinutes;
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        this.crittersEscaped = crittersEscaped;
        this.endingText = endingText;
        Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }

    /// <summary>Current ending coroutine phase, advanced by each call to <see cref="Step"/>.</summary>
    public EndingCreditsPhase Phase { get; private set; }
    /// <summary>
    /// True when the next call resumes inside the setup's NMI wait loop rather than starting
    /// a MainGameLoop iteration, so it makes no main-loop RNG call.
    /// </summary>
    internal bool ResumesAfterNmiWait =>
        Phase == EndingCreditsPhase.SetupEscapeFromZebes && setupNmiWaits > 0;
    /// <summary>Reward pose selected from clear time: suitless below three hours, helmetless below ten, otherwise armored.</summary>
    public EndingReward EndingReward =>
        gameTimeHours < EndingCreditsRomData.Rewards.SuitlessMaximumHoursExclusive
        ? EndingReward.Suitless
        : gameTimeHours < EndingCreditsRomData.Rewards.HelmetlessMaximumHoursExclusive
            ? EndingReward.Helmetless
            : EndingReward.Armored;

    /// <summary>Runs one state-$27 dispatcher call.</summary>
    /// <summary>Advances one cinematic dispatcher call, including its actor and palette updates.</summary>
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
                    paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Lava, 0);
                    paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Crust, 0);
                    brightness = 0;
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeInEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeInEscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds(sceneB: false);
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneA;
                break;

            case EndingCreditsPhase.EscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds(sceneB: false);
                if (mode7Zoom >= EndingCreditsRomData.Motion.EscapeEndScale)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneA:
                StepAtmosphericTransform(2);
                StepEscapeClouds(sceneB: false);
                if (StepFastFadeOut())
                    SetupEscapeSceneB();
                break;

            case EndingCreditsPhase.FadeInEscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds(sceneB: true);
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneB;
                break;

            case EndingCreditsPhase.EscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds(sceneB: true);
                if (mode7Zoom >= EndingCreditsRomData.Motion.EscapeEndScale)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneB;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneB:
                StepAtmosphericTransform(3);
                StepEscapeClouds(sceneB: true);
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
                CreditsObjectStepResult creditsStep = credits!.Step();
                if (creditsStep.CopiedRow)
                    credits.UploadTilemap(vram);
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
                    rewardGesture = new EndingRewardGesture(bus, EndingReward);
                    Phase = EndingCreditsPhase.PostCreditsGesture;
                }
                break;

            case EndingCreditsPhase.PostCreditsGesture:
                rewardGesture!.Step(objectArtwork is null
                    ? null : EndingRewardInstructionDefinitions.ReadWord);
                if (rewardGesture.JumpRequested)
                {
                    rewardGesture = null;
                    rewardGraphics = new EndingRewardGraphicsUpload(bus, mode7Artwork?.RewardIcon);
                    rewardJump = new EndingRewardJump(bus, EndingReward, UploadRewardGraphic);
                    Phase = EndingCreditsPhase.PostCreditsJump;
                }
                break;

            case EndingCreditsPhase.PostCreditsJump:
                rewardJump!.Step(objectArtwork is null
                    ? null : EndingRewardInstructionDefinitions.ReadWord);
                if (rewardJump.ShotRequested)
                {
                    postShot = new EndingPostShot(bus, cgram, ResolveEndingFont(), objectArtwork);
                    audio.QueueSound(EndingPostShotDefinitions.ShotSound, EndingPostShotDefinitions.SoundQueueLimit);
                    Phase = EndingCreditsPhase.PostCreditsShot;
                }
                break;

            case EndingCreditsPhase.PostCreditsShot:
                // Cinematic function runs before actors; palette fades and queued
                // tile replacements therefore precede the next sprite instruction.
                postShot!.Step(vram, cgram);
                rewardJump!.Step(objectArtwork is null
                    ? null : EndingRewardInstructionDefinitions.ReadWord);
                if (postShot.ReadyForWhiteFlash)
                    BeginPostCreditsWhiteFlash();
                break;

            case EndingCreditsPhase.PostCreditsWhiteFlash:
                if (whiteFlashColor > 0) whiteFlashColor--;
                rewardJump!.Step(objectArtwork is null
                    ? null : EndingRewardInstructionDefinitions.ReadWord);
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
                postCreditsText!.Step(vram);
                if (postCreditsText.RequestedItemPercentageScroll && postCreditsText.Completed)
                {
                    phaseTimer = 40;
                    Phase = EndingCreditsPhase.ItemPercentageScrollDown;
                }
                break;

            case EndingCreditsPhase.ItemPercentageScrollDown:
                // Function 148 moves BG1 down two pixels per call from zero through -80.
                // The software renderer applies the equivalent scroll while preserving the
                // already-built tilemap until the final object is spawned.
                postCreditsVerticalScroll = unchecked((ushort)(postCreditsVerticalScroll - 2));
                if (--phaseTimer <= 0)
                {
                    postCreditsText = new EndingBackgroundTextState(
                        bus,
                        postCreditsTilemap,
                        instructionPointer: EndingCreditsRomData.Instructions.SeeYouNextMissionText,
                        inventory,
                        japaneseText,
                        postCreditsUploadWord,
                        endingText,
                        endingText is null ? null : EndingTextSequence.FinalMessage);
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
        paletteFx.Step(bus, cgram,
            paletteFxArtwork ?? throw new InvalidOperationException(
                "Ending palette FX requires installed palette colors."), 0, 0, false, false);
        if (paletteFx.SoundRequests.Count != 0 || paletteFx.MusicRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled audio command.");
        // $8B:D46B follows cinematic actors even while a text-only TM masks OBJ.
        // F734 enables the already-initialized records when the credits finish.
        if (Phase >= EndingCreditsPhase.PostCreditsBlank)
            (shootingStars ??= new EndingShootingStars()).Step();
        cinematicFrame++;
    }

    /// <summary>Initializes the first escape panorama, its cloud actors, and the escape music queue.</summary>
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
        mode7X = mode7Y = 0;
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = SnesAngle.FromTableIndex(EndingCreditsRomData.Motion.EscapeInitialAngle);
        brightness = 0;
        postCreditsVerticalScroll = 0;
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        audio.QueueMusicDelayed8(MusicCommand.LoadData(
            EndingCreditsRomData.Music.EscapeDataIndex));
        audio.QueueMusicDelayed(
            MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
            MusicCommandDelay.FromDelayedYArgument(EndingCreditsRomData.Music.DelayArgument));
        Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    /// <summary>Replaces scene A with the second escape panorama and begins its fade-in.</summary>
    private void SetupEscapeSceneB()
    {
        ResetPaletteFx();
        paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.GreyClouds, 0);
        LoadMode7(EndingMode7SceneId.EscapeB);
        LoadEscapeCloudCharacters();
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopA, EndingSpriteRole.CloudTopA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopB, EndingSpriteRole.CloudTopB);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomA, EndingSpriteRole.CloudBottomA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomB, EndingSpriteRole.CloudBottomB);
        mode7X = mode7Y = 0;
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = SnesAngle.FromTableIndex(EndingCreditsRomData.Motion.EscapeInitialAngle);
        brightness = 0;
        fadeCounter = 1;
        Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    /// <summary>Loads the explosion scene and actors before starting the palette crossfade.</summary>
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
        mode7X = 0;
        mode7Y = 0;
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        fadeCounter = 1;
        BeginExplosionCrossfade();
        Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    /// <summary>Clears the explosion flash and initializes the planet's fast flyaway segment.</summary>
    private void SetupPlanetEscape()
    {
        // Func120 clears the explosion flash's backdrop and transparent palette entries.
        cgram.SetColor(0, 0);
        cgram.SetColor(16, 0);
        cgram.SetColor(128, 0);
        paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.PlanetAfterglow, 0);
        paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.GunshipEmergence, 0);
        mode7X = unchecked((ushort)-72);
        mode7Y = unchecked((ushort)-104);
        mode7Zoom = EndingCreditsRomData.Motion.PlanetEscapeInitialScale;
        mode7Angle = SnesAngle.NormalizeTableIndex(-112);
        phaseTimer = 192;
        flyawayWhite = EndingFlyawayFadeDefinitions.InitialWhite;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.InitialCountdown;
        planetMotionIndex = 0;
        Phase = EndingCreditsPhase.PlanetEscapeFast;
    }

    /// <summary>Installs credits assets, initializes the staff roll, and transfers control to its renderer.</summary>
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

    /// <summary>Returns the installed ending font atlas, failing when required presentation data is absent.</summary>
    private EndingFontAtlas ResolveEndingFont()
    {
        return endingFont ?? throw new InvalidOperationException(
            "Ending font requires installed presentation assets.");
    }

    /// <summary>Starts the blank pause between the staff roll and the post-credit result sequence.</summary>
    private void SetupPostCreditsBlank()
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
        EndingMode7SceneId? scene = Phase switch
        {
            >= EndingCreditsPhase.WaitForEscapeMusic and <= EndingCreditsPhase.FadeOutEscapeSceneA =>
                EndingMode7SceneId.EscapeA,
            >= EndingCreditsPhase.FadeInEscapeSceneB and <= EndingCreditsPhase.FadeOutEscapeSceneB =>
                EndingMode7SceneId.EscapeB,
            >= EndingCreditsPhase.FadeInZebesExplosion and <= EndingCreditsPhase.ZebesExplosionTileUpload =>
                EndingMode7SceneId.PlanetExplosion,
            _ => null,
        };
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

    /// <summary>Clears VRAM and installs both map copies and character data for one Mode-7 scene.</summary>
    /// <param name="scene">Catalog entry identifying the scene's map and character transfers.</param>
    private void LoadMode7(EndingMode7SceneId scene)
    {
        EndingMode7ArtworkCatalog artwork = mode7Artwork ?? throw new InvalidOperationException(
            "Ending Mode-7 scene requires installed artwork.");
        vram.Clear();
        UploadMode7Artwork(artwork[scene]);
    }

    /// <summary>Copies one scene's duplicated Mode-7 map and character sheet into the expected VRAM regions.</summary>
    /// <param name="scene">Expanded map and character data for the active Mode-7 backdrop.</param>
    private void UploadMode7Artwork(EndingMode7SceneArtwork scene)
    {
        vram.LoadMode7MapBytes(scene.Map.Span);
        vram.LoadMode7MapBytes(scene.Map.Span,
            destinationWord: EndingMode7ArtworkFormat.MapByteCount);
        vram.LoadMode7CharacterBytes(scene.Characters.Span);
    }

    /// <summary>Installs the shared cloud character sheet used by both escape panoramas.</summary>
    private void LoadEscapeCloudCharacters()
    {
        byte[] clouds = (objectArtwork ?? throw new InvalidOperationException(
            "Escape clouds require installed object artwork.")).Clouds.Transfer.ToArray();
        RequireMinimum(clouds, EndingCreditsRomData.Rendering.Mode7Bytes, "escape cloud characters");
        vram.LoadBytes(EndingCreditsRomData.Rendering.PostCreditsObjectDestination,
            clouds.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
    }

    /// <summary>Loads explosion objects, split character fragments, and the font region used during the explosion.</summary>
    private void LoadEndingObjectCharacters()
    {
        byte[] main = (objectArtwork ?? throw new InvalidOperationException(
            "Ending objects require installed artwork.")).Explosion.Transfer.ToArray();
        // $8B:D8C1 uploads the complete explosion object sheet from $7F:8000.
        RequireMinimum(main, EndingCreditsRomData.Rendering.ExplosionObjectBytes,
            "ending OBJ characters");
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.ObjectCharactersDestination,
            main.AsSpan(0, EndingCreditsRomData.Rendering.ExplosionObjectBytes));
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters70,
            EndingCreditsRomData.Rendering.Fragment70Destination,
            EndingObjectFragmentId.Segment70);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters74,
            EndingCreditsRomData.Rendering.Fragment74Destination,
            EndingObjectFragmentId.Segment74);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters78,
            EndingCreditsRomData.Rendering.Fragment78Destination,
            EndingObjectFragmentId.Segment78);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters7C,
            EndingCreditsRomData.Rendering.Fragment7CDestination,
            EndingObjectFragmentId.Segment7C);
        ReadOnlyMemory<byte> font = ResolveEndingFont().Transfer;
        RequireMinimum(font, EndingCreditsRomData.Rendering.ObjectFragmentLimit,
            "ending font characters");
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.FontCharactersDestination,
            font.Span[..EndingCreditsRomData.Rendering.ObjectFragmentLimit]);
    }

    /// <summary>Copies the requested expanded object fragment into its fixed VRAM character region.</summary>
    /// <param name="sourceAddress">Catalog source address retained by the caller's native transfer definition.</param>
    /// <param name="destinationByte">Destination byte offset in VRAM.</param>
    /// <param name="fragmentId">Installed artwork fragment corresponding to the destination region.</param>
    private void LoadObjectFragment(int sourceAddress, int destinationByte,
        EndingObjectFragmentId fragmentId)
    {
        byte[] fragment = (objectArtwork ?? throw new InvalidOperationException(
            "Ending object fragments require installed artwork.")).Fragment(fragmentId).Transfer.ToArray();
        RequireMinimum(fragment, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "ending OBJ fragment");
        vram.LoadBytes(
            destinationByte,
            fragment.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    /// <summary>Loads the credits palette, font, and shared post-credit character art once per ending run.</summary>
    private void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        LoadStaticPalette(EndingPaletteId.Credits, 0,
            EndingCreditsRomData.Rendering.PaletteHalfBytes, 0);
        ReadOnlyMemory<byte> font = ResolveEndingFont().Transfer;
        RequireMinimum(font, EndingCreditsRomData.Rendering.FontCharacterBytes,
            "credits font");

        vram.Clear();
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.ObjectCharactersDestination,
            font.Span[..EndingCreditsRomData.Rendering.FontCharacterBytes]);
        LoadCreditsCharacterArt();

        creditsAssetsLoaded = true;
    }

    /// <summary>Uploads a reward landing graphic through the currently active reward transfer owner.</summary>
    /// <param name="index">Landing-frame graphic index selected by the reward animation.</param>
    private void UploadRewardGraphic(int index) =>
        (rewardGraphics ?? throw new InvalidOperationException(
            "Reward landing has no active graphics upload owner.")).Upload(vram, index);

    /// <summary>Installs character sheets and tile fragments shared by the waiting and reward scenes.</summary>
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
        vram.LoadBytes(EndingCreditsRomData.Rendering.FontCharactersDestination,
            waiting.AsSpan(0, EndingCreditsRomData.Rendering.WaitingCharacterBytes));
        vram.LoadBytes(EndingCreditsRomData.Rendering.PostCreditsObjectDestination,
            shooting.AsSpan(0, EndingCreditsRomData.Rendering.ShootingCharacterBytes));
        vram.LoadBytes(EndingCreditsRomData.Rendering.WaitingTilemapDestination,
            waitingMap.AsSpan(0, EndingCreditsRomData.Rendering.WaitingTilemapBytes));
        // Function 126 selects suitless (<3h) or suited (>=3h) art; the
        // latter deliberately reuses the waiting-scene sheet, not Mode-7 art.
        vram.LoadBytes(0, result.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
        vram.LoadBytes(EndingCreditsRomData.Rendering.PostCreditsFragmentADestination,
            fragmentA.AsSpan(0, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes));
        vram.LoadBytes(EndingCreditsRomData.Rendering.PostCreditsFragmentBDestination,
            fragmentB.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    /// <summary>Advances each escape cloud's zoom-dependent motion and animation interpreter.</summary>
    /// <param name="sceneB">Selects the scene-B trajectory rules where actor-specific motion uses them.</param>
    private void StepEscapeClouds(bool sceneB)
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            EndingCloudMotion.Step(wrapper, mode7Zoom);
            sprite.Step(bus, instructionWord: objectArtwork is null
                ? null : EndingCloudInstructionDefinitions.ReadWord);
        }
    }

    /// <summary>Applies the alternating one-unit rotation and per-call scale change used by escape scenes.</summary>
    /// <param name="scaleDelta">Unsigned zoom increment for this scene's atmospheric expansion.</param>
    private void StepAtmosphericTransform(int scaleDelta)
    {
        // Func111/114 run during both fades as well as the fully visible interval.
        if ((cinematicFrame & 1) == 0)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        mode7Zoom = unchecked((ushort)(mode7Zoom + scaleDelta));
    }

    /// <summary>Runs active actors in native slot order, allowing callbacks to replace not-yet-visited slots.</summary>
    private void StepSprites()
    {
        // Native traversal re-reads each slot. A callback can replace a lower slot,
        // which then runs later in this same frame; higher slots wait until next frame.
        for (int slot = EndingSpriteSlots.Count - 1; slot >= 0; slot--)
        {
            EndingSprite? wrapper = sprites.Find(actor => actor.NativeSlot == slot && actor.Sprite.IsActive);
            if (wrapper is null) continue;
            StepEndingSpritePreInstruction(wrapper);
            Func<ushort, ushort>? instructionWord = objectArtwork is null ? null : wrapper.Role switch
            {
                >= EndingSpriteRole.ExplodingZebes and
                    <= EndingSpriteRole.ExplosionAfterglow => EndingExplosionInstructionDefinitions.ReadWord,
                >= EndingSpriteRole.OperationWasText and
                    <= EndingSpriteRole.ClearTimeDigit => EndingCompletionTextInstructionDefinitions.ReadWord,
                EndingSpriteRole.RewardSamus => EndingRewardInstructionDefinitions.ReadWord,
                EndingSpriteRole.AnimalEscape => EndingAnimalEscapeDefinitions.ReadWord,
                _ => null,
            };
            wrapper.Sprite.Step(bus, (opcode, cursor) =>
                HandleSpriteOpcode(wrapper, opcode, cursor),
                instructionWord: instructionWord);
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>Applies ending-specific side effects for an actor instruction and returns its resumed cursor.</summary>
    /// <param name="owner">Actor whose instruction stream issued the operation.</param>
    /// <param name="opcode">Decoded native instruction word.</param>
    /// <param name="cursor">Instruction cursor after the opcode, used when dispatch resumes.</param>
    /// <returns>The cursor to resume, or an instruction result when the operation changes control flow.</returns>
    private ushort? HandleSpriteOpcode(EndingSprite owner, ushort opcode, ushort cursor)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Ending_Instruction_FadeExplosionPalette:
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.FadePlanet, 0);
                cgram.SetColor(254, 1);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnExplosionSilhouette:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                cgram.SetColor(0, EndingCreditsRomData.Rendering.WhiteColor);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_StartZebesExplosion:
                explosionBurstDisplay = true;
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Supernova, 0);
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Explosion, 0);
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.WideExplosion, 0);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_ExplosionFinale:
                explosionFinaleDisplay = true;
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.SupernovaFinale, 0);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_EndZebesExplosion:
                // F32B disables TM/TS without deleting the actors: the stars must still
                // advance and become visible again when Func120 starts the flyaway.
                cgram.SetColor(0, EndingCreditsRomData.Rendering.WhiteColor);
                cgram.SetColor(128, EndingCreditsRomData.Rendering.WhiteColor);
                for (int color = 16; color < 32; color++)
                    cgram.SetColor(color, EndingCreditsRomData.Rendering.WhiteColor);
                phaseTimer = 120;
                Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnCompletedText:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnClearTime:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingCreditsRomData.Text.HoursTensX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingCreditsRomData.Text.HoursUnitsX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnColon:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingCreditsRomData.Text.MinutesTensX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingCreditsRomData.Text.MinutesUnitsX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_TransitionToCredits:
                fadeCounter = 1;
                Phase = EndingCreditsPhase.FadeOutToCredits;
                return cursor;

            default:
                throw new InvalidDataException(
                    $"Ending sprite opcode $8B:{opcode:X4} at $8B:{unchecked((ushort)(cursor - 2)):X4} is untranslated.");
        }
    }

    /// <summary>Creates one clear-time digit actor at its column, clamping the displayed value to a decimal digit.</summary>
    /// <param name="digit">Numeric digit before clamping to the supported 0 through 9 graphic range.</param>
    /// <param name="x">Screen-space horizontal position of the digit.</param>
    private void SpawnDigit(int digit, ushort x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        SpawnSprite(
            x,
            EndingCreditsRomData.Sprites.ClearTimeDigitY,
            0,
            unchecked((ushort)(
                EndingCreditsRomData.Sprites.ClearTimeDigitInstructionBase +
                normalized * EndingCreditsRomData.Sprites.ClearTimeDigitInstructionStride)),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>Replaces current actors with the body and head graphics matching the selected reward tier.</summary>
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
            default:
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardHead, EndingSpriteRole.RewardSamus);
                break;
        }
    }

    /// <summary>Transfers the assembled post-credit tilemap using the configured native word-transfer cursor.</summary>
    private void UploadPostCreditsTilemap() =>
        vram.ExecuteWordTransfer(
            postCreditsTilemap,
            postCreditsUploadWord,
            wordIncrement: 1);

    /// <summary>Creates or replaces an actor in its native slot, assigning fixed slots to special cinematic actors.</summary>
    /// <param name="x">Initial horizontal screen position.</param>
    /// <param name="y">Initial vertical screen position.</param>
    /// <param name="palette">Raw object attributes containing the actor's palette and priority bits.</param>
    /// <param name="instructionPointer">Initial animation instruction address.</param>
    /// <param name="role">Semantic role used for slot selection and instruction decoding.</param>
    private void SpawnSprite(
        ushort x,
        ushort y,
        ushort palette,
        ushort instructionPointer,
        EndingSpriteRole role)
    {
        int slot = role switch
        {
            EndingSpriteRole.ExplosionStarsRight => EndingSpriteSlots.RightStars,
            EndingSpriteRole.ExplosionStarsLeft => EndingSpriteSlots.LeftStars,
            EndingSpriteRole.ExplosionAfterglow => EndingSpriteSlots.Afterglow,
            EndingSpriteRole.AnimalEscape => EndingAnimalEscapeDefinitions.NativeSlot,
            _ => Enumerable.Range(0, EndingSpriteSlots.Count).Reverse()
                .FirstOrDefault(candidate => !sprites.Any(actor => actor.NativeSlot == candidate && actor.Sprite.IsActive), -1)
        };
        if (slot < 0) throw new InvalidOperationException("Ending cinematic actor slots exhausted.");
        sprites.RemoveAll(actor => actor.NativeSlot == slot);
        sprites.Add(new EndingSprite(new IntroDiscoverySprite(x, y, palette, instructionPointer), role)
            { NativeSlot = slot });
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            sprites[^1].Sprite.PreInstructionPointerForDiscovery(EndingSpritePreInstructions.WaitForFlyaway);
    }

    /// <summary>Creates an actor from its catalog definition while selecting the caller's behavior role.</summary>
    /// <param name="definition">Native position, attributes, and instruction pointer for the actor.</param>
    /// <param name="role">Semantic role used for update behavior and slot allocation.</param>
    private void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.X,
            definition.Y,
            definition.Attributes.Raw,
            definition.InstructionPointer,
            role);

    /// <summary>Advances the fast planet flyaway using its patterned horizontal displacement and rapid zoom-out.</summary>
    private void StepPlanetEscapeFast()
    {
        if (phaseTimer > 0)
            phaseTimer--;
        else StepFlyawayFade();
        mode7Angle = mode7Angle.AddTableUnits(-4);
        AddSignedFixed(
            ref mode7X,
            ref mode7XSubposition,
            EndingCreditsRomData.Motion.PlanetFastDelta(planetMotionIndex));
        planetMotionIndex = unchecked((ushort)(
            (planetMotionIndex + 1) &
            (EndingCreditsRomData.Motion.PlanetFastPatternLength - 1)));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 8));
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetFastEndScale)
        {
            planetMotionIndex = 0;
            Phase = EndingCreditsPhase.PlanetEscapeSlow;
        }
    }

    /// <summary>Advances the slower flyaway segment until motion transitions to accelerating departure.</summary>
    private void StepPlanetEscapeSlow()
    {
        StepFlyawayFade();
        if (mode7Angle != EndingCreditsRomData.Motion.PlanetSlowTargetAngle)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        AddSignedFixed(
            ref mode7X,
            ref mode7XSubposition,
            EndingCreditsRomData.Motion.PlanetSlowDelta(planetMotionIndex));
        planetMotionIndex = unchecked((ushort)(
            (planetMotionIndex + 1) &
            (EndingCreditsRomData.Motion.PlanetSlowPatternLength - 1)));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 2));
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetSlowEndScale)
        {
            planetVelocityWhole = 0;
            planetVelocityFraction = EndingCreditsRomData.Motion.InitialAccelerationFraction;
            Phase = EndingCreditsPhase.PlanetEscapeAccelerating;
            if (crittersEscaped)
            {
                SpawnSprite(EndingAnimalEscapeDefinitions.Origin, EndingAnimalEscapeDefinitions.Origin,
                    EndingAnimalEscapeDefinitions.Palette, EndingAnimalEscapeDefinitions.InstructionStart,
                    EndingSpriteRole.AnimalEscape);
                IntroDiscoverySprite pod = sprites[^1].Sprite;
                pod.GeneralTimer = EndingAnimalEscapeDefinitions.GeneralTimer;
                // Native cinematic dispatch precedes actors, so a newly spawned pod
                // moves and selects its first animation frame on this same call.
                StepEndingSpritePreInstruction(sprites[^1]);
                pod.Step(bus, instructionWord: EndingAnimalEscapeDefinitions.ReadWord);
            }
        }
    }

    /// <summary>Applies accelerating signed horizontal motion and turns the planet toward the gunship reveal.</summary>
    private void StepPlanetEscapeAccelerating()
    {
        StepFlyawayFade();
        int velocity = (planetVelocityWhole << 16) | planetVelocityFraction;
        velocity = unchecked(
            velocity - EndingCreditsRomData.Motion.AccelerationDelta16Point16);
        planetVelocityWhole = unchecked((short)(velocity >> 16));
        planetVelocityFraction = unchecked((ushort)velocity);
        AddSignedFixed(ref mode7X, ref mode7XSubposition, velocity);
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
            mode7Zoom = unchecked((ushort)(mode7Zoom - 4));
        }
    }

    /// <summary>Raises brightness by one every other call and reports when full brightness is reached.</summary>
    /// <returns><see langword="true"/> once brightness reaches the native maximum of 15.</returns>
    private bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Lowers brightness by one every other call and reports when the screen is fully dark.</summary>
    /// <returns><see langword="true"/> once brightness reaches zero.</returns>
    private bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Max(0, brightness - 1);
        }
        return brightness == 0;
    }

    /// <summary>Raises brightness by one every four calls for the slower post-credit fade.</summary>
    /// <returns><see langword="true"/> once brightness reaches the native maximum of 15.</returns>
    private bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Adds a signed 16.16 displacement to a native whole/fraction coordinate pair.</summary>
    /// <param name="whole">Whole-word coordinate component, updated in place.</param>
    /// <param name="fraction">Unsigned fractional component, updated in place with native wrap behavior.</param>
    /// <param name="delta">Signed 16.16 displacement to add.</param>
    private static void AddSignedFixed(ref ushort whole, ref ushort fraction, int delta)
    {
        SnesFixedPosition result = SnesSignedSixteenSixteen
            .FromRaw(delta)
            .AddTo(whole, fraction);
        whole = result.Whole;
        fraction = result.Fraction;
    }

    /// <summary>Rejects an expanded asset that is too short for the fixed-size native transfer.</summary>
    /// <param name="data">Expanded bytes available for the transfer.</param>
    /// <param name="minimum">Minimum byte count required by the destination layout.</param>
    /// <param name="name">Asset label included in the invalid-data error.</param>
    private static void RequireMinimum(ReadOnlyMemory<byte> data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

}

/// <summary>Coroutine positions corresponding to the ending cinematic and its post-credit result screens.</summary>
internal enum EndingCreditsPhase
{
    /// <summary>Consumes the setup routine's initial NMI waits before loading escape scene A.</summary>
    SetupEscapeFromZebes,
    /// <summary>Waits for escape music commands to finish before fading in scene A.</summary>
    WaitForEscapeMusic,
    /// <summary>Fades in the first escape panorama while its clouds move.</summary>
    FadeInEscapeSceneA,
    /// <summary>Displays and advances the first escape panorama.</summary>
    EscapeSceneA,
    /// <summary>Fades out scene A before replacing it with scene B.</summary>
    FadeOutEscapeSceneA,
    /// <summary>Fades in the second escape panorama.</summary>
    FadeInEscapeSceneB,
    /// <summary>Displays and advances the second escape panorama.</summary>
    EscapeSceneB,
    /// <summary>Fades out scene B before starting the planetary explosion.</summary>
    FadeOutEscapeSceneB,
    /// <summary>Fades in the explosion while its palette crossfade and actors advance.</summary>
    FadeInZebesExplosion,
    /// <summary>Continues the explosion palette transition after the initial fade.</summary>
    ZebesExplosionPaletteCrossfade,
    /// <summary>Uploads the explosion's tilemap chunks over successive dispatcher calls.</summary>
    ZebesExplosionTileUpload,
    /// <summary>Runs the explosion actors until their instruction stream begins the music wait.</summary>
    ZebesExplosionAnimation,
    /// <summary>Holds the post-explosion scene for its authored delay.</summary>
    WaitForPlanetEscapeMusic,
    /// <summary>Waits for the delayed planet-escape music command to drain.</summary>
    WaitForPlanetEscapeMusicQueue,
    /// <summary>Runs the fast initial portion of the planet flyaway.</summary>
    PlanetEscapeFast,
    /// <summary>Runs the slow middle portion of the planet flyaway.</summary>
    PlanetEscapeSlow,
    /// <summary>Accelerates the planet away and reveals the successful-operation text.</summary>
    PlanetEscapeAccelerating,
    /// <summary>Displays the operation-success text until its actor transitions to credits.</summary>
    OperationSuccessfulText,
    /// <summary>Fades out the ending scene before initializing the staff roll.</summary>
    FadeOutToCredits,
    /// <summary>Incrementally renders the staff roll.</summary>
    Credits,
    /// <summary>Holds the blank interval between staff credits and the result panel.</summary>
    PostCreditsBlank,
    /// <summary>Fades in the post-credit backdrop.</summary>
    PostCreditsFadeIn,
    /// <summary>Fades the waiting-scene palette toward its final colors.</summary>
    PostCreditsShootingStars,
    /// <summary>Holds the waiting backdrop before installing the result panel.</summary>
    PostCreditsWaitingBackdrop,
    /// <summary>Shows the result panel for its delay before introducing the reward pose.</summary>
    PostCreditsWaitingSamus,
    /// <summary>Reveals the selected reward and presents its copyright panel.</summary>
    PostCreditsReward,
    /// <summary>Retains the reward actors while the copyright panel is displayed.</summary>
    PostCreditsCopyright,
    /// <summary>Runs the reward pose animation until it requests the jump.</summary>
    PostCreditsGesture,
    /// <summary>Runs the reward jump and uploads landing graphics as requested.</summary>
    PostCreditsJump,
    /// <summary>Runs the shot effect and its palette/tile transitions.</summary>
    PostCreditsShot,
    /// <summary>Holds and fades the white flash after the shot.</summary>
    PostCreditsWhiteFlash,
    /// <summary>Displays the ending logo sequence.</summary>
    PostCreditsLogo,
    /// <summary>Displays the final item-percentage text.</summary>
    ItemPercentage,
    /// <summary>Scrolls the item-percentage panel away before the final message.</summary>
    ItemPercentageScrollDown,
    /// <summary>Displays the final message and remains on the authored terminal screen.</summary>
    SeeYouNextMission,
}

/// <summary>Reward artwork tier chosen from the completed run's clear time.</summary>
internal enum EndingReward
{
    /// <summary>Under three hours: Samus appears without the suit.</summary>
    Suitless,
    /// <summary>Three to under ten hours: Samus appears without the helmet.</summary>
    Helmetless,
    /// <summary>Ten hours or more: Samus appears in the armored suit.</summary>
    Armored,
}

/// <summary>Semantic actor categories that select native slots and instruction interpreters.</summary>
internal enum EndingSpriteRole
{
    /// <summary>Right-moving upper cloud in escape scene A.</summary>
    CloudRightA,
    /// <summary>Left-moving upper cloud in escape scene A.</summary>
    CloudLeftA,
    /// <summary>Right-moving lower cloud in escape scene A.</summary>
    CloudRightB,
    /// <summary>Left-moving lower cloud in escape scene A.</summary>
    CloudLeftB,
    /// <summary>First upper cloud in escape scene B.</summary>
    CloudTopA,
    /// <summary>Second upper cloud in escape scene B.</summary>
    CloudTopB,
    /// <summary>First lower cloud in escape scene B.</summary>
    CloudBottomA,
    /// <summary>Second lower cloud in escape scene B.</summary>
    CloudBottomB,
    /// <summary>Planet actor used during the explosion sequence.</summary>
    ExplodingZebes,
    /// <summary>Lava layer actor in the explosion sequence.</summary>
    ExplosionLava,
    /// <summary>Glow layer actor in the explosion sequence.</summary>
    ExplosionGlow,
    /// <summary>Initial explosion star actor.</summary>
    ExplosionStars,
    /// <summary>Silhouette spawned during the explosion instructions.</summary>
    ExplosionSilhouette,
    /// <summary>Right-side stars occupying their fixed native slot.</summary>
    ExplosionStarsRight,
    /// <summary>Left-side stars occupying their fixed native slot and waiting for flyaway.</summary>
    ExplosionStarsLeft,
    /// <summary>Afterglow actor spawned at the explosion finale.</summary>
    ExplosionAfterglow,
    /// <summary>Operation-success message actor.</summary>
    OperationWasText,
    /// <summary>Completion-success message actor.</summary>
    CompletedSuccessfullyText,
    /// <summary>Clear-time label actor.</summary>
    ClearTimeText,
    /// <summary>Digit or punctuation actor composing the clear-time display.</summary>
    ClearTimeDigit,
    /// <summary>Reward-pose body or head actor.</summary>
    RewardSamus,
    /// <summary>Animal escape pod actor accompanying the planet flyaway.</summary>
    AnimalEscape,
}

/// <summary>An active ending actor paired with its semantic role and native object-slot bookkeeping.</summary>
/// <param name="Sprite">Mutable native sprite interpreter state for this actor.</param>
/// <param name="Role">Category selecting actor-specific slot, pre-instruction, and opcode behavior.</param>
internal sealed record EndingSprite(IntroDiscoverySprite Sprite, EndingSpriteRole Role)
{
    /// <summary>Native object-table slot used to preserve actor update and replacement order.</summary>
    public int NativeSlot { get; init; }
    /// <summary>Whether the actor participates in the escape-cloud motion update.</summary>
    public bool CloudMoving { get; set; }
}
