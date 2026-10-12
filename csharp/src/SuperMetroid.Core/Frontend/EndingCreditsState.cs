using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Spatial;
using SuperMetroid.Core.Rendering;

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
internal sealed class EndingCreditsState
{
    /// <summary>The <see cref="EndingCreditsPresentation"/> owner of this EndingCreditsState.</summary>
    internal EndingCreditsPresentation Presentation { get; }

    /// <summary>Reads the cartridge address space used by ending sequences.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Plays the ending music and sound effects.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Captured game-time hours used to select the ending reward.</summary>
    private readonly ushort gameTimeHours;
    /// <summary>Captured game-time minutes used to select the ending reward.</summary>
    private readonly ushort gameTimeMinutes;
    /// <summary>Captured inventory used by the ending presentation.</summary>
    private readonly EndingInventorySnapshot inventory;
    /// <summary>Selects the Japanese ending text layout.</summary>
    private readonly bool japaneseText;
    /// <summary>Selects the ending variant where the animals escaped.</summary>
    private readonly bool crittersEscaped;
    /// <summary>Video memory owned by the ending sequence.</summary>
    internal readonly SnesVram vram = new();
    /// <summary>Palette memory owned by the ending sequence.</summary>
    internal readonly SnesCgram cgram = new();
    /// <summary>Active sprites drawn during the ending sequence.</summary>
    internal readonly List<EndingSprite> sprites = [];
    /// <summary>Tilemap revealed after the staff credits.</summary>
    private readonly ushort[] postCreditsTilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];

    /// <summary>Current scrolling staff-credits object, when active.</summary>
    internal CreditsObjectState? credits;
    /// <summary>Star-field animation shown behind the staff credits.</summary>
    internal EndingShootingStars? shootingStars;
    /// <summary>Text shown after the staff credits finish.</summary>
    private EndingBackgroundTextState? postCreditsText;
    /// <summary>Current frame of the active ending cinematic.</summary>
    private ushort cinematicFrame;
    /// <summary>NMI waits <c>CinematicFunction_Ending_Setup</c> has made so far.</summary>
    private int setupNmiWaits;
    /// <summary>Countdown used by the current ending phase.</summary>
    internal int phaseTimer;
    /// <summary>Progress of the current palette fade.</summary>
    private int fadeCounter;
    /// <summary>Current ending-screen brightness.</summary>
    internal byte brightness;
    // Only X ever receives a subpixel; Y's fraction word stays zero as the native state has none.
    /// <summary>Mode 7 position, including the native X subpixel component.</summary>
    internal FixedPosition<Mode7Pixels> mode7;
    /// <summary>Current Mode 7 scale for the planet scene.</summary>
    internal ushort mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
    /// <summary>Current Mode 7 rotation angle for the planet scene.</summary>
    internal SnesAngle mode7Angle;
    /// <summary>Stores planet motion index.</summary>
    private ushort planetMotionIndex;
    /// <summary>Stores post credits vertical scroll.</summary>
    internal ushort postCreditsVerticalScroll;
    /// <summary>Stores planet velocity whole.</summary>
    private short planetVelocityWhole;
    /// <summary>Stores planet velocity fraction.</summary>
    private ushort planetVelocityFraction;
    /// <summary>Stores credits assets loaded.</summary>
    private bool creditsAssetsLoaded;
    /// <summary>Stores reward copyright shown.</summary>
    private bool rewardCopyrightShown;
    /// <summary>Stores reward gesture.</summary>
    internal EndingRewardGesture? rewardGesture;
    /// <summary>Stores reward jump.</summary>
    internal EndingRewardJump? rewardJump;
    /// <summary>Stores reward graphics.</summary>
    private EndingRewardGraphicsUpload? rewardGraphics;
    /// <summary>Stores post shot.</summary>
    internal EndingPostShot? postShot;
    /// <summary>Stores palette fx.</summary>
    private RoomPaletteFxSystem paletteFx = new();
    /// <summary>Stores ending text.</summary>
    [NonSerialized] private EndingTextPresentation? endingText;
    /// <summary>Stores ending font.</summary>
    [NonSerialized] private EndingFontAtlas? endingFont;
    /// <summary>Stores staff credits.</summary>
    [NonSerialized] private CreditsPresentation? staffCredits;
    /// <summary>Stores flight artwork.</summary>
    [NonSerialized] private CeresFlightArtworkCatalog? flightArtwork;
    /// <summary>Stores mode7 artwork.</summary>
    [NonSerialized] private EndingMode7ArtworkCatalog? mode7Artwork;
    /// <summary>Stores object artwork.</summary>
    [NonSerialized] internal EndingObjectArtworkCatalog? objectArtwork;

    /// <summary>Initializes a new ending credits state instance.</summary>
    public EndingCreditsState(ISnesAddressSpace bus, CartridgeAudioState audio, EndingCreditsSetup setup)
    {
        Presentation = new EndingCreditsPresentation(this);
        this.bus = Ensure.NotNull(bus);
        this.audio = Ensure.NotNull(audio);
        this.gameTimeHours = setup.GameTimeHours;
        this.gameTimeMinutes = setup.GameTimeMinutes;
        this.inventory = setup.Inventory;
        this.japaneseText = setup.JapaneseText;
        this.crittersEscaped = setup.CrittersEscaped;
        this.endingText = setup.EndingText;
        Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }

    /// <summary>Gets or sets phase.</summary>
    public EndingCreditsPhase Phase { get; private set; }
    /// <summary>
    /// True when the next call resumes inside the setup's NMI wait loop rather than starting
    /// a MainGameLoop iteration, so it makes no main-loop RNG call.
    /// </summary>
    internal bool ResumesAfterNmiWait =>
        Phase == EndingCreditsPhase.SetupEscapeFromZebes && setupNmiWaits > 0;
    /// <summary>Gets ending reward.</summary>
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
                    paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Lava, 0);
                    paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.Crust, 0);
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
                    postCreditsText = new EndingBackgroundTextState(bus, new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, InstructionPointer = EndingCreditsRomData.Instructions.SeeYouNextMissionText, Inventory = inventory, JapaneseText = japaneseText, TilemapDestination = postCreditsUploadWord, Presentation = endingText, InstalledSequence = endingText is null ? null : EndingTextSequence.FinalMessage });
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
        paletteFx.Step(bus, cgram, paletteFxArtwork ?? throw new InvalidOperationException(
                "Ending palette FX requires installed palette colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
        if (paletteFx.SoundRequests.Count != 0 || paletteFx.MusicRequests.Count != 0)
            throw new InvalidDataException("Ending palette program requested an unhandled audio command.");
        // $8B:D46B follows cinematic actors even while a text-only TM masks OBJ.
        // F734 enables the already-initialized records when the credits finish.
        if (Phase >= EndingCreditsPhase.PostCreditsBlank)
            (shootingStars ??= new EndingShootingStars()).Step();
        cinematicFrame++;
    }

    /// <summary>Sets up escape scene a.</summary>
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

    /// <summary>Sets up escape scene b.</summary>
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
        mode7 = new(mode7.X.WithWhole(0), default);
        mode7Zoom = EndingCreditsRomData.Motion.EscapeInitialScale;
        mode7Angle = SnesAngle.FromTableIndex(EndingCreditsRomData.Motion.EscapeInitialAngle);
        brightness = 0;
        fadeCounter = 1;
        Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    /// <summary>Sets up zebes explosion.</summary>
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

    /// <summary>Sets up planet escape.</summary>
    private void SetupPlanetEscape()
    {
        // Func120 clears the explosion flash's backdrop and transparent palette entries.
        var palettes = new FrontendPalettes(cgram);
        palettes.EndingBackdrop.Write(0, Bgr555.Black);
        palettes.EndingLogoBackground.Write(0, Bgr555.Black);
        palettes.EndingObjectTransparent.Write(0, Bgr555.Black);
        paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.PlanetAfterglow, 0);
        paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.GunshipEmergence, 0);
        mode7 = new(mode7.X.WithWhole(unchecked((ushort)-72)), new(unchecked((ushort)-104), 0));
        mode7Zoom = EndingCreditsRomData.Motion.PlanetEscapeInitialScale;
        mode7Angle = SnesAngle.NormalizeTableIndex(-112);
        phaseTimer = 192;
        flyawayWhite = EndingFlyawayFadeDefinitions.InitialWhite;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.InitialCountdown;
        planetMotionIndex = 0;
        Phase = EndingCreditsPhase.PlanetEscapeFast;
    }

    /// <summary>Sets up credits.</summary>
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

    /// <summary>Resolves ending font.</summary>
    private EndingFontAtlas ResolveEndingFont()
    {
        return endingFont ?? throw new InvalidOperationException(
            "Ending font requires installed presentation assets.");
    }

    /// <summary>Sets up post credits blank.</summary>
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

    /// <summary>Loads mode7.</summary>
    private void LoadMode7(EndingMode7SceneId scene)
    {
        EndingMode7ArtworkCatalog artwork = mode7Artwork ?? throw new InvalidOperationException(
            "Ending Mode-7 scene requires installed artwork.");
        new VramStore(vram).Clear();
        UploadMode7Artwork(artwork[scene]);
    }

    /// <summary>Uploads mode7 artwork.</summary>
    private void UploadMode7Artwork(EndingMode7SceneArtwork scene)
    {
        var mode7 = new Mode7Vram(vram);
        mode7.LoadMap(scene.Map.Span);
        mode7.LoadMap(scene.Map.Span, cell: EndingMode7ArtworkFormat.MapByteCount);
        mode7.LoadCharacters(scene.Characters.Span);
    }

    /// <summary>Loads escape cloud characters.</summary>
    private void LoadEscapeCloudCharacters()
    {
        byte[] clouds = (objectArtwork ?? throw new InvalidOperationException(
            "Escape clouds require installed object artwork.")).Clouds.Transfer.ToArray();
        RequireMinimum(clouds, EndingCreditsRomData.Rendering.Mode7Bytes, "escape cloud characters");
        new EndingVram(vram).PostCreditsObjects.Load(clouds.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
    }

    /// <summary>Loads ending object characters.</summary>
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

    /// <summary>Loads object fragment.</summary>
    private void LoadObjectFragment(EndingObjectFragmentId fragmentId)
    {
        byte[] fragment = (objectArtwork ?? throw new InvalidOperationException(
            "Ending object fragments require installed artwork.")).Fragment(fragmentId).Transfer.ToArray();
        RequireMinimum(fragment, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "ending OBJ fragment");
        new EndingVram(vram).ObjectFragment(fragmentId).Load(
            fragment.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    /// <summary>Loads credits and post credits assets.</summary>
    private void LoadCreditsAndPostCreditsAssets()
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

    /// <summary>Uploads reward graphic.</summary>
    private void UploadRewardGraphic(int index) =>
        (rewardGraphics ?? throw new InvalidOperationException(
            "Reward landing has no active graphics upload owner.")).Upload(vram, index);

    /// <summary>Loads credits character art.</summary>
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

    /// <summary>Steps escape clouds.</summary>
    private void StepEscapeClouds()
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            EndingCloudMotion.Step(wrapper, mode7Zoom);
            sprite.Step(instructionWord: objectArtwork is null
                ? null : EndingCloudInstructionDefinitions.ReadWord);
        }
    }

    /// <summary>Steps atmospheric transform.</summary>
    private void StepAtmosphericTransform(int scaleDelta)
    {
        // Func111/114 run during both fades as well as the fully visible interval.
        if ((cinematicFrame & 1) == 0)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        mode7Zoom = unchecked((ushort)(mode7Zoom + scaleDelta));
    }

    /// <summary>Steps sprites.</summary>
    private void StepSprites()
    {
        // Native traversal re-reads each slot. A callback can replace a lower slot,
        // which then runs later in this same frame; higher slots wait until next frame.
        for (int slot = EndingSpriteSlots.Count - 1; slot >= 0; slot--)
        {
            EndingSprite? wrapper = sprites.Find(actor => actor.NativeSlot == slot && actor.Sprite.IsActive);
            if (wrapper is null) continue;
            StepEndingSpritePreInstruction(wrapper);
            Func<ushort, ushort>? instructionWord = objectArtwork is null ? null : wrapper.Role.Family() switch
            {
                EndingSpriteFamily.Cloud => null,
                EndingSpriteFamily.Explosion => EndingExplosionInstructionDefinitions.ReadWord,
                EndingSpriteFamily.CompletionText => EndingCompletionTextInstructionDefinitions.ReadWord,
                EndingSpriteFamily.Reward => EndingRewardInstructionDefinitions.ReadWord,
                EndingSpriteFamily.AnimalEscape => EndingAnimalEscapeDefinitions.ReadWord,
                var family => throw new InvalidOperationException($"Undefined ending sprite family {family}."),
            };
            wrapper.Sprite.Step<EndingSpriteInstruction>(HandleSpriteOpcode, instructionWord);
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    /// <summary>Handles sprite opcode.</summary>
    private ushort HandleSpriteOpcode(EndingSpriteInstruction opcode, ushort cursor)
    {
        switch (opcode)
        {
            case EndingSpriteInstruction.FadeExplosionPalette:
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.FadePlanet, 0);
                new FrontendPalettes(cgram).EndingExplosionGlow.Write(0, new Bgr555(1, 0, 0));
                return cursor;

            case EndingSpriteInstruction.SpawnExplosionSilhouette:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                new FrontendPalettes(cgram).EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                return cursor;

            case EndingSpriteInstruction.StartZebesExplosion:
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

            case EndingSpriteInstruction.ExplosionFinale:
                explosionFinaleDisplay = true;
                paletteFx.SpawnDefinition(bus, EndingPaletteFxDefinitions.SupernovaFinale, 0);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return cursor;

            case EndingSpriteInstruction.EndZebesExplosion:
                // F32B disables TM/TS without deleting the actors: the stars must still
                // advance and become visible again when Func120 starts the flyaway.
                var flash = new FrontendPalettes(cgram);
                flash.EndingBackdrop.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingObjectTransparent.Write(0, EndingCreditsRomData.Rendering.WhiteColor);
                flash.EndingLogoBackground.Fill(EndingCreditsRomData.Rendering.WhiteColor);
                phaseTimer = 120;
                Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return cursor;

            case EndingSpriteInstruction.SpawnCompletedText:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return cursor;

            case EndingSpriteInstruction.SpawnClearTime:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return cursor;

            case EndingSpriteInstruction.SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingCreditsRomData.Text.HoursTensX);
                return cursor;

            case EndingSpriteInstruction.SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingCreditsRomData.Text.HoursUnitsX);
                return cursor;

            case EndingSpriteInstruction.SpawnColon:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return cursor;

            case EndingSpriteInstruction.SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingCreditsRomData.Text.MinutesTensX);
                return cursor;

            case EndingSpriteInstruction.SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingCreditsRomData.Text.MinutesUnitsX);
                return cursor;

            case EndingSpriteInstruction.TransitionToCredits:
                fadeCounter = 1;
                Phase = EndingCreditsPhase.FadeOutToCredits;
                return cursor;

            default:
                throw new InvalidOperationException($"Undefined EndingSpriteInstruction {opcode}.");
        }
    }

    /// <summary>Spawn digit.</summary>
    private void SpawnDigit(int digit, ushort x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        SpawnSprite(
            new(x, EndingCreditsRomData.Sprites.ClearTimeDigitY),
            0,
            unchecked((ushort)(
                EndingCreditsRomData.Sprites.ClearTimeDigitInstructionBase +
                normalized * EndingCreditsRomData.Sprites.ClearTimeDigitInstructionStride)),
            EndingSpriteRole.ClearTimeDigit);
    }

    /// <summary>Spawn ending reward actors.</summary>
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

    /// <summary>Uploads post credits tilemap.</summary>
    [PpuLayoutBoundary("codec: post-credits staging tilemap DMA to its scene-selected VMADD")]
    private void UploadPostCreditsTilemap() =>
        vram.ExecuteWordTransfer(
            postCreditsTilemap,
            postCreditsUploadWord,
            wordIncrement: 1);

    /// <summary>Spawn sprite.</summary>
    private void SpawnSprite(
        Position16<ScreenPixels> position,
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
        sprites.Add(new EndingSprite(new IntroDiscoverySprite(position, palette, instructionPointer), role)
            { NativeSlot = slot });
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            sprites[^1].Sprite.PreInstructionPointerForDiscovery((ushort)EndingSpritePreInstruction.WaitForFlyaway);
    }

    /// <summary>Spawn sprite.</summary>
    private void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.Position,
            definition.Attributes.Raw,
            definition.InstructionPointer,
            role);

    /// <summary>Steps planet escape fast.</summary>
    private void StepPlanetEscapeFast()
    {
        if (phaseTimer > 0)
            phaseTimer--;
        else StepFlyawayFade();
        mode7Angle = mode7Angle.AddTableUnits(-4);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, EndingCreditsRomData.Motion.PlanetFastDelta(planetMotionIndex)));
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

    /// <summary>Steps planet escape slow.</summary>
    private void StepPlanetEscapeSlow()
    {
        StepFlyawayFade();
        if (mode7Angle != EndingCreditsRomData.Motion.PlanetSlowTargetAngle)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, EndingCreditsRomData.Motion.PlanetSlowDelta(planetMotionIndex)));
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
                SpawnSprite(new(EndingAnimalEscapeDefinitions.Origin, EndingAnimalEscapeDefinitions.Origin),
                    EndingAnimalEscapeDefinitions.Palette, EndingAnimalEscapeDefinitions.InstructionStart,
                    EndingSpriteRole.AnimalEscape);
                IntroDiscoverySprite pod = sprites[^1].Sprite;
                pod.GeneralTimer = EndingAnimalEscapeDefinitions.GeneralTimer;
                // Native cinematic dispatch precedes actors, so a newly spawned pod
                // moves and selects its first animation frame on this same call.
                StepEndingSpritePreInstruction(sprites[^1]);
                pod.Step(instructionWord: EndingAnimalEscapeDefinitions.ReadWord);
            }
        }
    }

    /// <summary>Steps planet escape accelerating.</summary>
    private void StepPlanetEscapeAccelerating()
    {
        StepFlyawayFade();
        int velocity = (SnesSixteenSixteen.FromWhole(planetVelocityWhole)) | planetVelocityFraction;
        velocity = unchecked(
            velocity - EndingCreditsRomData.Motion.AccelerationDelta16Point16);
        planetVelocityWhole = unchecked((short)(SnesSixteenSixteen.WholeOf(velocity)));
        planetVelocityFraction = unchecked((ushort)velocity);
        mode7 = mode7.WithX(AddSignedFixed(mode7.X, velocity));
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

    /// <summary>Steps fast fade in.</summary>
    private bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Steps fast fade out.</summary>
    private bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Max(0, brightness - 1);
        }
        return brightness == 0;
    }

    /// <summary>Steps slow fade in.</summary>
    private bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    /// <summary>Adds signed fixed.</summary>
    private static SnesFixedPosition AddSignedFixed(SnesFixedPosition position, int delta) =>
        SnesSignedSixteenSixteen
            .FromRaw(delta)
            .AddTo(position.Whole, position.Fraction);

    /// <summary>Requires minimum.</summary>
    private static void RequireMinimum(ReadOnlyMemory<byte> data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Show reward copyright.</summary>
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

    /// <summary>Stores explosion finale display.</summary>
    private bool explosionFinaleDisplay;
    /// <summary>Stores explosion burst display.</summary>
    private bool explosionBurstDisplay;
    /// <summary>Gets uses explosion finale display.</summary>
    internal bool UsesExplosionFinaleDisplay => (explosionFinaleDisplay || explosionBurstDisplay) && !Presentation.ExplosionWhiteout
        && Phase < EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>Captures explosion finale.</summary>
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
            Presentation.CurrentEscapeObjectSelection, brightness);

        static Bg4BppRenderLayer Plane(ushort map, bool high) => new(map,
            EndingExplosionDisplayDefinitions.Characters,
            new Position16<BgPlanePixels>(0, EndingExplosionDisplayDefinitions.FirstVisibleBackgroundRow),
            new Extent16<TilemapCells>(32, 32), high);
    }

    /// <summary>Captures explosion burst.</summary>
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
            Presentation.CurrentEscapeObjectSelection, brightness);
    }

    /// <summary>Stores explosion fade source.</summary>
    private Bgr555[] explosionFadeSource = [];
    /// <summary>Stores explosion fade step.</summary>
    private int explosionFadeStep;
    /// <summary>Gets explosion crossfade active.</summary>
    internal bool ExplosionCrossfadeActive => Phase is EndingCreditsPhase.FadeInZebesExplosion
        or EndingCreditsPhase.ZebesExplosionPaletteCrossfade;

    /// <summary>Begin explosion crossfade.</summary>
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

    /// <summary>Steps explosion crossfade.</summary>
    private void StepExplosionCrossfade()
    {
        mode7Zoom = unchecked((ushort)(mode7Zoom + EndingExplosionFadeDefinitions.ZoomStep));
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

    /// <summary>Applies explosion fade.</summary>
    [PpuLayoutBoundary("codec: native ending fade over its CGRAM index range")]
    private void ApplyExplosionFade(int start, int factor)
    {
        for (int i = start; i < start + EndingExplosionFadeDefinitions.Colors; i++)
        {
            new CgramPalettes(cgram).All.Write(i, explosionFadeSource[i].Map((_, channel) =>
                channel * factor / EndingExplosionFadeDefinitions.Steps));
        }
    }

    /// <summary>Stores flyaway white.</summary>
    private byte flyawayWhite;
    /// <summary>Stores flyaway fade countdown.</summary>
    private int flyawayFadeCountdown;
    /// <summary>Gets flyaway fixed color.</summary>
    internal FixedColorAddRenderLayer? FlyawayFixedColor => flyawayWhite != 0
        && Phase >= EndingCreditsPhase.PlanetEscapeFast && Phase <= EndingCreditsPhase.FadeOutToCredits
        ? new(flyawayWhite, flyawayWhite, flyawayWhite) : null;

    /// <summary>Steps flyaway fade.</summary>
    private void StepFlyawayFade()
    {
        if (--flyawayFadeCountdown > 0) return;
        if (flyawayWhite > 0) flyawayWhite--;
        flyawayFadeCountdown = EndingFlyawayFadeDefinitions.Interval;
    }

    /// <summary>Stores flyaway characters.</summary>
    private byte[] flyawayCharacters = [];
    /// <summary>Stores flyaway map.</summary>
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

    /// <summary>Prepares flyaway uploads.</summary>
    private void PrepareFlyawayUploads()
    {
        PrepareFlyawayStreams();
        // Func117 restores the BG half before disabling BG1. OBJ VRAM and palettes
        // remain live while Func118 replaces the two Mode-7 lanes over sixteen NMIs.
        LoadStaticPalette(EndingPaletteId.Explosion, 0, 128, 0);
    }

    /// <summary>Prepares flyaway streams.</summary>
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

    /// <summary>Uploads flyaway chunk.</summary>
    private void UploadFlyawayChunk(int index)
    {
        int chunksPerLane = EndingCreditsRomData.Rendering.Mode7Bytes / EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        int offset = index % chunksPerLane * EndingCreditsRomData.Rendering.FlyawayUploadBytes;
        if (index < chunksPerLane)
            new Mode7Vram(vram).LoadCharacters(flyawayCharacters.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), offset);
        else
            new Mode7Vram(vram).LoadMap(flyawayMap.AsSpan(offset, EndingCreditsRomData.Rendering.FlyawayUploadBytes), offset);
    }

    /// <summary>Gets escape background enabled.</summary>
    internal bool EscapeBackgroundEnabled => Phase is < EndingCreditsPhase.ZebesExplosionTileUpload
        or >= EndingCreditsPhase.PlanetEscapeFast;

    /// <summary>Stores palette artwork.</summary>
    [NonSerialized] private EndingPaletteCatalog? paletteArtwork;
    /// <summary>Stores palette fx artwork.</summary>
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

    /// <summary>Loads static palette.</summary>
    private void LoadStaticPalette(EndingPaletteId id, int sourceColor, int count,
        int destinationColor)
    {
        (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].LoadTo(cgram, sourceColor, count, destinationColor);
    }

    /// <summary>Static palette color.</summary>
    private Bgr555 StaticPaletteColor(EndingPaletteId id, int color)
        => (paletteArtwork ?? throw new InvalidOperationException(
            "Ending palettes require installed artwork."))[id].Color(color);

    /// <summary>Steps ending sprite pre instruction.</summary>
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
                if (Enum.IsDefined((EndingSpritePreInstruction)sprite.PreInstructionPointer))
                    StepFlyawayStars(sprite, (EndingSpritePreInstruction)sprite.PreInstructionPointer);
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

    /// <summary>Steps flyaway stars.</summary>
    private void StepFlyawayStars(IntroDiscoverySprite sprite, EndingSpritePreInstruction preInstruction)
    {
        switch (preInstruction)
        {
            case EndingSpritePreInstruction.WaitForFlyaway:
                if (Phase == EndingCreditsPhase.PlanetEscapeFast)
                {
                    sprite.PreInstructionPointerForDiscovery((ushort)EndingSpritePreInstruction.MoveFlyawayStars);
                    // The Y subposition word doubles as the low velocity word.
                    sprite.Position = sprite.Position.WithY(sprite.Position.Y.WithFraction(0x4000));
                    sprite.GeneralTimer = 0;
                }
                break;
            case EndingSpritePreInstruction.MoveFlyawayStars:
                int velocity = unchecked((int)(SnesSixteenSixteen.Compose(sprite.GeneralTimer, sprite.Position.Y.Fraction))) - 32;
                sprite.GeneralTimer = unchecked((ushort)(SnesSixteenSixteen.WholeOf(velocity)));
                uint position = unchecked(IntroCinematicMotion.Packed(sprite.Position.X) + (uint)velocity);
                sprite.Position = new(
                    IntroCinematicMotion.Unpacked(position),
                    sprite.Position.Y.WithFraction(unchecked((ushort)velocity)));
                break;
            default:
                throw new InvalidOperationException(
                    $"Undefined {nameof(EndingSpritePreInstruction)} {(int)preInstruction}.");
        }
    }

    /// <summary>Applies waiting backdrop palette.</summary>
    private void ApplyWaitingBackdropPalette(int step)
        => ApplyPostCreditsPaletteRange(EndingCreditsRomData.Rendering.WaitingPaletteStart,
            EndingCreditsRomData.Rendering.WaitingPaletteCount, step);

    /// <summary>Stores reward palette step.</summary>
    private int rewardPaletteStep;

    /// <summary>Applies reward palette.</summary>
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

    /// <summary>Applies post credits palette range.</summary>
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

    /// <summary>Stores post credits upload word.</summary>
    internal ushort postCreditsUploadWord = EndingCreditsRomData.Rendering.PostCreditsTilemapWord;
    /// <summary>Stores post credits map height.</summary>
    internal int postCreditsMapHeight = 32;
    /// <summary>Stores white flash color.</summary>
    internal byte whiteFlashColor;
    /// <summary>Stores ending logo.</summary>
    internal EndingLogo? endingLogo;

    /// <summary>Begin post credits white flash.</summary>
    private void BeginPostCreditsWhiteFlash()
    {
        postShot = null;
        // E48A restores the shooting palette and selects BG1SC=$4E: the final
        // text map is two screens tall, not the credits' single-screen map.
        LoadStaticPalette(EndingPaletteId.PostCredits,
            EndingPostShotDefinitions.ShootingPaletteStart,
            EndingPostShotDefinitions.PaletteColors,
            EndingPostShotDefinitions.ShootingPaletteStart);
        postCreditsUploadWord = EndingPostShotDefinitions.FinalTextTilemapWord;
        postCreditsMapHeight = EndingPostShotDefinitions.FinalTextMapHeight;
        postCreditsVerticalScroll = 0;
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile,
            EndingPostShotDefinitions.ClearedCopyrightStart, EndingPostShotDefinitions.ClearedCopyrightWords);
        UploadPostCreditsTilemap();
        whiteFlashColor = EndingPostShotDefinitions.WhiteComponent;
        phaseTimer = EndingPostShotDefinitions.FadeFrames;
        Phase = EndingCreditsPhase.PostCreditsWhiteFlash;
    }

    /// <summary>Finish post credits white flash.</summary>
    private void FinishPostCreditsWhiteFlash()
    {
        // E504 copies the same staging map into the lower screen before the logo
        // actors enter. Subsequent percentage writes target the upper screen again.
        new EndingVram(vram).PostShotLowerTilemap.Write(postCreditsTilemap);
        rewardJump = null;
        sprites.Clear();
        endingLogo = new EndingLogo(bus, cgram,
            () => paletteFx.SpawnDefinition(bus, EndingLogoDefinitions.LandingPaletteFx, 0),
            paletteArtwork);
        Phase = EndingCreditsPhase.PostCreditsLogo;
    }

    /// <summary>Steps post credits logo.</summary>
    private void StepPostCreditsLogo()
    {
        endingLogo!.Step(cgram, objectArtwork is null
            ? null : EndingLogoInstructionDefinitions.ReadWord);
        if (!endingLogo.Completed) return;
        postCreditsText = new EndingBackgroundTextState(bus, new EndingBackgroundTextSetup { Tilemap = postCreditsTilemap, InstructionPointer = EndingCreditsRomData.Instructions.ItemPercentageText, Inventory = inventory, JapaneseText = japaneseText, TilemapDestination = EndingPostShotDefinitions.FinalTextTilemapWord, Presentation = endingText, InstalledSequence = endingText is null ? null : SuperMetroid.Core.Assets.EndingTextSequence.ItemPercentage });
        Phase = EndingCreditsPhase.ItemPercentage;
    }
}

/// <summary>Represents ending credits phase.</summary>
internal enum EndingCreditsPhase
{
    /// <summary>Identifies setup escape from zebes.</summary>
    SetupEscapeFromZebes,
    /// <summary>Identifies wait for escape music.</summary>
    WaitForEscapeMusic,
    /// <summary>Identifies fade in escape scene a.</summary>
    FadeInEscapeSceneA,
    /// <summary>Identifies escape scene a.</summary>
    EscapeSceneA,
    /// <summary>Identifies fade out escape scene a.</summary>
    FadeOutEscapeSceneA,
    /// <summary>Identifies fade in escape scene b.</summary>
    FadeInEscapeSceneB,
    /// <summary>Identifies escape scene b.</summary>
    EscapeSceneB,
    /// <summary>Identifies fade out escape scene b.</summary>
    FadeOutEscapeSceneB,
    /// <summary>Identifies fade in zebes explosion.</summary>
    FadeInZebesExplosion,
    /// <summary>Identifies zebes explosion palette crossfade.</summary>
    ZebesExplosionPaletteCrossfade,
    /// <summary>Identifies zebes explosion tile upload.</summary>
    ZebesExplosionTileUpload,
    /// <summary>Identifies zebes explosion animation.</summary>
    ZebesExplosionAnimation,
    /// <summary>Identifies wait for planet escape music.</summary>
    WaitForPlanetEscapeMusic,
    /// <summary>Identifies wait for planet escape music queue.</summary>
    WaitForPlanetEscapeMusicQueue,
    /// <summary>Identifies planet escape fast.</summary>
    PlanetEscapeFast,
    /// <summary>Identifies planet escape slow.</summary>
    PlanetEscapeSlow,
    /// <summary>Identifies planet escape accelerating.</summary>
    PlanetEscapeAccelerating,
    /// <summary>Identifies operation successful text.</summary>
    OperationSuccessfulText,
    /// <summary>Identifies fade out to credits.</summary>
    FadeOutToCredits,
    /// <summary>Identifies credits.</summary>
    Credits,
    /// <summary>Identifies post credits blank.</summary>
    PostCreditsBlank,
    /// <summary>Identifies post credits fade in.</summary>
    PostCreditsFadeIn,
    /// <summary>Identifies post credits shooting stars.</summary>
    PostCreditsShootingStars,
    /// <summary>Identifies post credits waiting backdrop.</summary>
    PostCreditsWaitingBackdrop,
    /// <summary>Identifies post credits waiting samus.</summary>
    PostCreditsWaitingSamus,
    /// <summary>Identifies post credits reward.</summary>
    PostCreditsReward,
    /// <summary>Identifies post credits copyright.</summary>
    PostCreditsCopyright,
    /// <summary>Identifies post credits gesture.</summary>
    PostCreditsGesture,
    /// <summary>Identifies post credits jump.</summary>
    PostCreditsJump,
    /// <summary>Identifies post credits shot.</summary>
    PostCreditsShot,
    /// <summary>Identifies post credits white flash.</summary>
    PostCreditsWhiteFlash,
    /// <summary>Identifies post credits logo.</summary>
    PostCreditsLogo,
    /// <summary>Identifies item percentage.</summary>
    ItemPercentage,
    /// <summary>Identifies item percentage scroll down.</summary>
    ItemPercentageScrollDown,
    /// <summary>Identifies see you next mission.</summary>
    SeeYouNextMission,
}

/// <summary>Represents ending reward.</summary>
internal enum EndingReward
{
    /// <summary>Identifies suitless.</summary>
    Suitless,
    /// <summary>Identifies helmetless.</summary>
    Helmetless,
    /// <summary>Identifies armored.</summary>
    Armored,
}

/// <summary>Represents ending sprite role.</summary>
internal enum EndingSpriteRole
{
    /// <summary>Identifies cloud right a.</summary>
    CloudRightA,
    /// <summary>Identifies cloud left a.</summary>
    CloudLeftA,
    /// <summary>Identifies cloud right b.</summary>
    CloudRightB,
    /// <summary>Identifies cloud left b.</summary>
    CloudLeftB,
    /// <summary>Identifies cloud top a.</summary>
    CloudTopA,
    /// <summary>Identifies cloud top b.</summary>
    CloudTopB,
    /// <summary>Identifies cloud bottom a.</summary>
    CloudBottomA,
    /// <summary>Identifies cloud bottom b.</summary>
    CloudBottomB,
    /// <summary>Identifies exploding zebes.</summary>
    ExplodingZebes,
    /// <summary>Identifies explosion lava.</summary>
    ExplosionLava,
    /// <summary>Identifies explosion glow.</summary>
    ExplosionGlow,
    /// <summary>Identifies explosion stars.</summary>
    ExplosionStars,
    /// <summary>Identifies explosion silhouette.</summary>
    ExplosionSilhouette,
    /// <summary>Identifies explosion stars right.</summary>
    ExplosionStarsRight,
    /// <summary>Identifies explosion stars left.</summary>
    ExplosionStarsLeft,
    /// <summary>Identifies explosion afterglow.</summary>
    ExplosionAfterglow,
    /// <summary>Identifies operation was text.</summary>
    OperationWasText,
    /// <summary>Identifies completed successfully text.</summary>
    CompletedSuccessfullyText,
    /// <summary>Identifies clear time text.</summary>
    ClearTimeText,
    /// <summary>Identifies clear time digit.</summary>
    ClearTimeDigit,
    /// <summary>Identifies reward samus.</summary>
    RewardSamus,
    /// <summary>Identifies animal escape.</summary>
    AnimalEscape,
}

/// <summary>The artwork and instruction-list family an ending sprite role belongs to.</summary>
internal enum EndingSpriteFamily
{
    /// <summary>Identifies cloud.</summary>
    Cloud,
    /// <summary>Identifies explosion.</summary>
    Explosion,
    /// <summary>Identifies completion text.</summary>
    CompletionText,
    /// <summary>Identifies reward.</summary>
    Reward,
    /// <summary>Identifies animal escape.</summary>
    AnimalEscape,
}

/// <summary>Domain operations over the ending's closed phase and sprite-role sets.</summary>
internal static class EndingCreditsDomains
{
    /// <summary>The Mode 7 backdrop a phase displays, or null when it displays none.</summary>
    internal static EndingMode7SceneId? Mode7Scene(this EndingCreditsPhase phase) => phase switch
    {
        EndingCreditsPhase.WaitForEscapeMusic or EndingCreditsPhase.FadeInEscapeSceneA or
            EndingCreditsPhase.EscapeSceneA or EndingCreditsPhase.FadeOutEscapeSceneA => EndingMode7SceneId.EscapeA,
        EndingCreditsPhase.FadeInEscapeSceneB or EndingCreditsPhase.EscapeSceneB or
            EndingCreditsPhase.FadeOutEscapeSceneB => EndingMode7SceneId.EscapeB,
        EndingCreditsPhase.FadeInZebesExplosion or EndingCreditsPhase.ZebesExplosionPaletteCrossfade or
            EndingCreditsPhase.ZebesExplosionTileUpload => EndingMode7SceneId.PlanetExplosion,
        EndingCreditsPhase.SetupEscapeFromZebes or EndingCreditsPhase.ZebesExplosionAnimation or
            EndingCreditsPhase.WaitForPlanetEscapeMusic or EndingCreditsPhase.WaitForPlanetEscapeMusicQueue or
            EndingCreditsPhase.PlanetEscapeFast or EndingCreditsPhase.PlanetEscapeSlow or
            EndingCreditsPhase.PlanetEscapeAccelerating or EndingCreditsPhase.OperationSuccessfulText or
            EndingCreditsPhase.FadeOutToCredits or EndingCreditsPhase.Credits or EndingCreditsPhase.PostCreditsBlank or
            EndingCreditsPhase.PostCreditsFadeIn or EndingCreditsPhase.PostCreditsShootingStars or
            EndingCreditsPhase.PostCreditsWaitingBackdrop or EndingCreditsPhase.PostCreditsWaitingSamus or
            EndingCreditsPhase.PostCreditsReward or EndingCreditsPhase.PostCreditsCopyright or
            EndingCreditsPhase.PostCreditsGesture or EndingCreditsPhase.PostCreditsJump or
            EndingCreditsPhase.PostCreditsShot or EndingCreditsPhase.PostCreditsWhiteFlash or
            EndingCreditsPhase.PostCreditsLogo or EndingCreditsPhase.ItemPercentage or
            EndingCreditsPhase.ItemPercentageScrollDown or EndingCreditsPhase.SeeYouNextMission => null,
        _ => throw new ArgumentOutOfRangeException(nameof(phase), phase, "Undefined ending phase."),
    };

    /// <summary>The artwork and instruction-list family of a sprite role.</summary>
    internal static EndingSpriteFamily Family(this EndingSpriteRole role) => role switch
    {
        EndingSpriteRole.CloudRightA or EndingSpriteRole.CloudLeftA or EndingSpriteRole.CloudRightB or
            EndingSpriteRole.CloudLeftB or EndingSpriteRole.CloudTopA or EndingSpriteRole.CloudTopB or
            EndingSpriteRole.CloudBottomA or EndingSpriteRole.CloudBottomB => EndingSpriteFamily.Cloud,
        EndingSpriteRole.ExplodingZebes or EndingSpriteRole.ExplosionLava or EndingSpriteRole.ExplosionGlow or
            EndingSpriteRole.ExplosionStars or EndingSpriteRole.ExplosionSilhouette or
            EndingSpriteRole.ExplosionStarsRight or EndingSpriteRole.ExplosionStarsLeft or
            EndingSpriteRole.ExplosionAfterglow => EndingSpriteFamily.Explosion,
        EndingSpriteRole.OperationWasText or EndingSpriteRole.CompletedSuccessfullyText or
            EndingSpriteRole.ClearTimeText or EndingSpriteRole.ClearTimeDigit => EndingSpriteFamily.CompletionText,
        EndingSpriteRole.RewardSamus => EndingSpriteFamily.Reward,
        EndingSpriteRole.AnimalEscape => EndingSpriteFamily.AnimalEscape,
        _ => throw new ArgumentOutOfRangeException(nameof(role), role, "Undefined ending sprite role."),
    };
}

/// <summary>Represents ending sprite.</summary>
internal sealed record EndingSprite(IntroDiscoverySprite Sprite, EndingSpriteRole Role)
{
    /// <summary>Gets or sets native slot.</summary>
    public int NativeSlot { get; init; }
    /// <summary>Gets or sets cloud moving.</summary>
    public bool CloudMoving { get; set; }
}
