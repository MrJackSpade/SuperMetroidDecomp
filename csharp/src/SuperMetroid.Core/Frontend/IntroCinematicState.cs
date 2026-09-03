using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Opening-cinematic state $1E, beginning with “The last Metroid is in captivity”.
/// </summary>
/// <remarks>
/// Construction ports the complete graphics setup at $8B:A395. The visible state machine
/// continues through the first narration card, the complete first illustrated/typewriter
/// page, and the native palette transition into the Mother Brain gameplay flashback.
/// </remarks>
public sealed class IntroCinematicState
{
    private const int ScreenWidth = 256;
    private const int ScreenHeight = 224;

    // `$8B:A66F` writes cinematic_var10/BG1VOFS=8 when it creates the first illustrated
    // page. Neither `$8B:AEB8` (Mother Brain) nor `$8B:AF6C` (SR388 discovery) resets that
    // word, so both gameplay-style flashbacks deliberately inherit the same eight-pixel
    // source offset. Their actor coordinates are already screen-relative and must not be
    // moved with it.
    internal const ushort GameplayFlashbackBg1VerticalScroll = 8;

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly ControllerInputState controller = new();
    private readonly SamusProjectileSystem flashbackProjectiles = new();
    private readonly SamusBombProjectileSystem flashbackBombProjectiles = new();
    private readonly ushort[] textTilemap = new ushort[0x400];
    private readonly byte[] japaneseBlankCharacter;
    private readonly ushort[] introPalette = new ushort[SnesCgram.ColorCount];
    private IntroCinematicObjectSystem? objects;
    private CinematicPaletteFader? paletteFader;
    private SamusState? flashbackSamus;
    private IntroMotherBrainSpriteState? flashbackMotherBrain;
    private IntroMotherBrainExplosionSystem? flashbackMotherBrainExplosions;
    private IntroRinkaSystem? flashbackRinkas;
    private IntroBabyDiscoveryState? babyDiscovery;
    private IntroScientistCutsceneState? scientistCutscene;
    private IntroCeresFlightState? ceresFlight;
    private DemoInputState? flashbackDemoInput;
    private RoomLevelData? flashbackLevel;
    private ushort crossfadeCounter;
    private ushort introCrossfadeCounter;
    private ushort nmiFrameCounter;
    private int brightness;
    private int timer = 8;
    private int fadeDelay;

    public IntroCinematicState(ISnesAddressSpace bus, CartridgeAudioState? audio = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        this.bus = bus;
        this.audio = audio;
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(MusicCommand.LoadData(0x3f));
        cgram.LoadFromBus(bus, 0x8ce3e9);
        cgram.Colors.CopyTo(introPalette);

        byte[] bgCharacters = RomDataReader.Decompress(bus, 0x95f90e, maximumOutputBytes: 0x8000);
        byte[] fontOne = RomDataReader.Decompress(bus, 0x95d089, maximumOutputBytes: 0x0900);
        byte[] samusHeadTilemap = RomDataReader.Decompress(bus, 0x9788cc, maximumOutputBytes: 0x0800);
        byte[] bg1Pages = RomDataReader.Decompress(bus, 0x96ff14, maximumOutputBytes: 0x2000);
        byte[] introObjects = RomDataReader.Decompress(bus, 0x95e4c2, maximumOutputBytes: 0x2400);
        byte[] firstNarrationTilemap = RomDataReader.Decompress(bus, 0x978d12, maximumOutputBytes: 0x0800);

        RequireMinimum(bgCharacters, 0x8000, "intro BG1/BG2 characters");
        RequireMinimum(fontOne, 0x0900, "intro font one");
        RequireMinimum(samusHeadTilemap, 0x0800, "Samus-head BG2 tilemap");
        RequireMinimum(bg1Pages, 0x2000, "intro BG1 page tilemaps");
        RequireMinimum(introObjects, 0x2400, "intro OBJ characters");
        RequireMinimum(firstNarrationTilemap, 0x0800, "first narration BG3 tilemap");

        // BTS[$1E8E-$1E9D] aliases $7F:8290-$829F while the first font is resident in
        // decompression RAM. $8B:A86A repeats precisely this character when blanking the
        // optional Japanese glyph area; retain the source before host staging is discarded.
        japaneseBlankCharacter = fontOne.AsSpan(0x0290, 0x10).ToArray();

        // Literal VMADD values from `$8B:A469-$A529`, converted to physical byte offsets.
        vram.LoadBytes(0x0000, bgCharacters.AsSpan(0, 0x8000));
        vram.LoadBytes(0x8000, fontOne.AsSpan(0, 0x0900));       // VMADD $4000
        vram.LoadBytes(0x9000, samusHeadTilemap.AsSpan(0, 0x0800)); // VMADD $4800
        vram.LoadBytes(0x9800, firstNarrationTilemap.AsSpan(0, 0x0800)); // VMADD $4C00
        vram.LoadBytes(0xa000, bg1Pages.AsSpan(0, 0x2000));      // VMADD $5000
        vram.LoadBytes(0xc000, RomDataReader.ReadFixedBank(bus, 0x9ad200, 0x2000)); // VMADD $6000
        vram.LoadBytes(0xdc00, introObjects.AsSpan(0, 0x2400));  // VMADD $6E00

        // $8B:A3AC performs the ordinary beam tile/palette upload before copying the full
        // intro palette. The projectile tile DMA remains resident for both gameplay
        // flashbacks; restore the later intro CGRAM copy after using the shared helper.
        SamusProjectileSystem.LoadBeamTilesAndPalette(bus, vram, cgram, equippedBeams: 0);
        cgram.LoadFromBus(bus, 0x8ce3e9);

        // Font two is decompressed only after the initial VRAM setup in the native routine.
        // Retain the validation now even though English is the fresh-save default.
        _ = RomDataReader.Decompress(bus, 0x95d713, maximumOutputBytes: 0x1200);
        Phase = IntroCinematicPhase.WaitForInitialMusicQueue;
    }

    public IntroCinematicPhase Phase { get; private set; }

    /// <summary>
    /// Number of scripted missiles accepted by Mother Brain's intro collision routine.
    /// </summary>
    /// <remarks>
    /// This is deliberately a read-only debugger seam over the translated actor word. It
    /// lets the CLI prove that the ROM demo, ordinary missile producer, and cinematic actor
    /// agree without teaching the front-end dispatcher a second simulation path.
    /// </remarks>
    public ushort MotherBrainHitCount => flashbackMotherBrain?.HitCount ?? 0;

    /// <summary>Live ordinary-projectile slots in the Mother Brain flashback.</summary>
    public ushort ActiveFlashbackProjectileCount => flashbackProjectiles.ProjectileCounter;

    /// <summary>Allocated fourth-hit explosion actors that remain visible in this scene.</summary>
    public int ActiveMotherBrainExplosionCount => flashbackMotherBrainExplosions?.ActiveCount ?? 0;

    /// <summary>Live ring-projectile actors spawned by the retail $8B:CF27 script.</summary>
    public int ActiveIntroRinkaCount => flashbackRinkas?.ActiveCount ?? 0;

    /// <summary>Total Rinkas allocated so far, including actors already deleted on hit.</summary>
    public int SpawnedIntroRinkaCount => flashbackRinkas?.SpawnedCount ?? 0;

    /// <summary>Persistent narration-caret Y word, including native off-screen value $F8.</summary>
    public ushort IntroCaretY => objects?.CaretY ?? 0;

    /// <summary>Live typewriter-block X position updated by each drawn text character.</summary>
    public ushort IntroCaretX => objects?.CaretX ?? 0;

    /// <summary>Live scripted hit timer used to prove the first Rinka reached Samus.</summary>
    public ushort FlashbackSamusInvincibilityTimer => flashbackSamus?.InvincibilityTimer ?? 0;

    /// <summary>Current SR388 Samus pose, exposed so tests verify a hit is visibly consumed.</summary>
    public byte FlashbackSamusPose => flashbackSamus?.Pose ?? 0;

    /// <summary>Current SR388 Samus world X, including the native Rinka knockback motion.</summary>
    public ushort FlashbackSamusX => flashbackSamus?.XPosition ?? 0;

    /// <summary>Current SR388 Samus world Y across the complete hurt arc and landing.</summary>
    public ushort FlashbackSamusY => flashbackSamus?.YPosition ?? 0;

    /// <summary>Current ROM delay-list frame, exposed to audit visible hurt animation.</summary>
    public ushort FlashbackSamusAnimationFrame => flashbackSamus?.AnimationFrame ?? 0;

    /// <summary>Whether the bank-$91 knockback handler owns SR388 Samus this frame.</summary>
    public bool FlashbackSamusKnockbackActive => flashbackSamus?.KnockbackActive ?? false;

    /// <summary>World X of the live SR388 demo Samus, or zero before that scene.</summary>
    public ushort BabyDiscoverySamusX => babyDiscovery?.Samus.XPosition ?? 0;

    /// <summary>Whether the egg actor's $A8E8 proximity test has redirected its ROM list.</summary>
    public bool BabyDiscoveryEggHatchingStarted => babyDiscovery?.EggHatchingStarted ?? false;

    /// <summary>Live shell fragments spawned by the egg's cartridge opcode $8B:A918.</summary>
    public int ActiveBabyDiscoveryEggParticleCount => babyDiscovery?.ActiveEggParticleCount ?? 0;

    /// <summary>True after the final narration fade hands control to the Ceres flight.</summary>
    public bool NarrationFinished { get; private set; }

    /// <summary>True when the SPACE COLONY caption and its final fade have completed.</summary>
    public bool CeresFlightFinished => ceresFlight?.Finished ?? false;

    /// <summary>Debugger-readable inner phase while the outer dispatcher remains state $1E.</summary>
    public string CeresFlightPhaseName => ceresFlight?.Phase.ToString() ?? string.Empty;

    public void Step(ushort controllerInput)
    {
        // Cinematic functions consume joypad1_newkeys, not the raw held word. Latching at
        // this state boundary preserves a one-frame edge and prevents a held A/Start from
        // skipping both the options menu and the first narration page.
        controller.Latch(controllerInput);
        bool demoWasLoadedBeforeThisFrame = flashbackDemoInput is not null;
        bool explosionsExistedBeforeThisFrame = flashbackMotherBrainExplosions is not null;
        bool babyDiscoveryExistedBeforeThisFrame = babyDiscovery is not null;
        bool scientistCutsceneExistedBeforeThisFrame = scientistCutscene is not null;
        nmiFrameCounter++;
        switch (Phase)
        {
            case IntroCinematicPhase.WaitForInitialMusicQueue:
                // QueueMusic_Delayed8 owns an eight-frame delay before HasQueuedMusic clears.
                if (MusicQueueFinished())
                {
                    Phase = IntroCinematicPhase.FadeInFirstNarration;
                    fadeDelay = 0;
                }
                break;

            case IntroCinematicPhase.FadeInFirstNarration:
                if (StepSlowFade(inward: true, delayReload: 2))
                {
                    Phase = IntroCinematicPhase.LastMetroidIsInCaptivity;
                    timer = 60;
                }
                break;

            case IntroCinematicPhase.LastMetroidIsInCaptivity:
                if (--timer <= 0)
                {
                    audio?.QueueMusicDelayed8(MusicCommand.SelectTrack(5));
                    Phase = IntroCinematicPhase.GalaxyIsAtPeace;
                    timer = 200;
                }
                break;

            case IntroCinematicPhase.GalaxyIsAtPeace:
                if (--timer <= 0)
                {
                    // The next music commands wait for APU acknowledgement before the
                    // documented four-second hold. Eight frames models their delayed slot.
                    audio?.QueueMusicDelayed8(MusicCommand.Stop);
                    audio?.QueueMusicDelayed8(MusicCommand.LoadData(0x42));
                    audio?.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x0e));
                    Phase = IntroCinematicPhase.WaitForSecondMusicQueue;
                    timer = 8;
                }
                break;

            case IntroCinematicPhase.WaitForSecondMusicQueue:
                if (MusicQueueFinished())
                {
                    Phase = IntroCinematicPhase.FourSecondHold;
                    timer = 240;
                }
                break;

            case IntroCinematicPhase.FourSecondHold:
                if (--timer <= 0)
                {
                    Phase = IntroCinematicPhase.FadeOutFirstNarration;
                    fadeDelay = 0;
                }
                break;

            case IntroCinematicPhase.FadeOutFirstNarration:
                if (StepSlowFade(inward: false, delayReload: 2))
                    SetupFirstIllustratedPage();
                break;

            case IntroCinematicPhase.WaitForPageOneMusicQueue:
                // QueueMusic_DelayedY(track 5, $0E) is the longest command issued by
                // $8B:A66F, so HasQueuedMusic becomes clear after fourteen update slots.
                if (MusicQueueFinished())
                {
                    objects!.StartEnglishPageOne();
                    Phase = IntroCinematicPhase.FadeInPageOne;
                    fadeDelay = 0;
                }
                break;

            case IntroCinematicPhase.FadeInPageOne:
                if (StepSlowFade(inward: true, delayReload: 2))
                    Phase = IntroCinematicPhase.PageOneText;
                break;

            case IntroCinematicPhase.PageOneText:
                break;

            case IntroCinematicPhase.PageOneAwaitingInput:
                if (controller.NewlyPressed != 0)
                    SetupMotherBrainFlashback();
                break;

            case IntroCinematicPhase.MotherBrainCrossfade:
                StepMotherBrainCrossfade();
                break;

            case IntroCinematicPhase.MotherBrainFlashback:
                if (flashbackMotherBrain?.PageTwoRequested == true)
                    SetupPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoCrossfade:
                StepPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoText:
                break;

            case IntroCinematicPhase.PageTwoAwaitingInput:
                if (controller.NewlyPressed != 0)
                    SetupBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscoveryCrossfade:
                StepBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscovery:
                if (babyDiscovery?.PageThreeRequested == true)
                    SetupPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeCrossfade:
                StepPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeText:
                break;

            case IntroCinematicPhase.PageThreeAwaitingInput:
                if (controller.NewlyPressed != 0)
                    SetupBabyMetroidDelivery();
                break;

            case IntroCinematicPhase.BabyMetroidDeliveryCrossfade:
                StepBabyMetroidDeliveryCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidDelivery:
                if (scientistCutscene?.PageFourRequested == true)
                    SetupPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourCrossfade:
                StepPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourText:
                break;

            case IntroCinematicPhase.PageFourAwaitingInput:
                if (controller.NewlyPressed != 0)
                    SetupBabyMetroidExamination();
                break;

            case IntroCinematicPhase.BabyMetroidExaminationCrossfade:
                StepBabyMetroidExaminationCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidExamination:
                if (scientistCutscene?.PageFiveRequested == true)
                    SetupPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveCrossfade:
                StepPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveText:
                break;

            case IntroCinematicPhase.PageFiveAwaitingInput:
                if (controller.NewlyPressed != 0)
                    SetupPageSix();
                break;

            case IntroCinematicPhase.PageSixText:
                break;

            case IntroCinematicPhase.IntroFadeOut:
                if (StepSlowFade(inward: false, delayReload: 1))
                {
                    NarrationFinished = true;
                    // $8B:BCA0 immediately replaces the narration PPU setup with the
                    // Ceres Mode 7 scene. Give that scene its own VRAM, CGRAM, and
                    // brightness owner: the narration has just deliberately reached
                    // INIDISP zero, while $8B:BDE4 restores full brightness only after
                    // the fourteen-frame delayed music command has completed.
                    ceresFlight = new IntroCeresFlightState(bus);
                    Phase = IntroCinematicPhase.CeresFlight;
                }
                break;

            case IntroCinematicPhase.CeresFlight:
                ceresFlight!.Step();
                break;
        }

        if (objects is not null)
        {
            objects.Step();
            if (objects.PageOneAwaitingInput)
            {
                if (Phase == IntroCinematicPhase.PageOneText)
                    Phase = IntroCinematicPhase.PageOneAwaitingInput;
            }
            if (objects.PageTwoAwaitingInput && Phase == IntroCinematicPhase.PageTwoText)
                Phase = IntroCinematicPhase.PageTwoAwaitingInput;
            if (objects.PageThreeAwaitingInput && Phase == IntroCinematicPhase.PageThreeText)
                Phase = IntroCinematicPhase.PageThreeAwaitingInput;
            if (objects.PageFourAwaitingInput && Phase == IntroCinematicPhase.PageFourText)
                Phase = IntroCinematicPhase.PageFourAwaitingInput;
            if (objects.PageFiveAwaitingInput && Phase == IntroCinematicPhase.PageFiveText)
                Phase = IntroCinematicPhase.PageFiveAwaitingInput;
            if (objects.IntroFinishRequested && Phase == IntroCinematicPhase.PageSixText)
            {
                // $B240 selects the ordinary fade-out owner with delay/counter one. The
                // framebuffer on this request frame is still full-brightness page six.
                fadeDelay = 0;
                Phase = IntroCinematicPhase.IntroFadeOut;
            }
        }

        // DemoInputObjectHandler runs before game state $25. A demo loaded by the cinematic
        // function therefore starts on the next frame, never on the accepting input frame.
        if (demoWasLoadedBeforeThisFrame && flashbackDemoInput is not null)
            StepMotherBrainDemo();

        if (flashbackSamus is not null)
            StepMotherBrainFlashbackSamus();

        // Game state $25 handles cinematic sprites after the active cinematic function and
        // after Samus/projectiles. Mother Brain can therefore consume a missile at its new
        // position during this same frame, exactly as $8B:B786 does.
        if (flashbackMotherBrain is not null)
        {
            flashbackMotherBrain.RunPreInstruction(
                cgram,
                introPalette,
                nmiFrameCounter,
                crossfadeCounter);
            if (flashbackProjectiles.TryImpactIntroMotherBrainMissile(bus, flashbackBombProjectiles))
            {
                bool fourthHit = flashbackMotherBrain.RegisterMissileHit();
                if (fourthHit)
                {
                    flashbackMotherBrainExplosions = new IntroMotherBrainExplosionSystem();
                    flashbackMotherBrainExplosions.SpawnFourthHitExplosions();
                }
            }
        }

        if (flashbackRinkas is not null && flashbackSamus is not null)
        {
            flashbackRinkas.Step(
                bus,
                flashbackSamus,
                motherBrainExploding: flashbackMotherBrain?.ExplosionStarted == true);
        }

        // This ordering makes a newly spawned timer-one object select its first spritemap
        // on the same accepted frame as the page-one input transition.
        flashbackMotherBrain?.Step(bus);
        if (explosionsExistedBeforeThisFrame)
        {
            // The eight actors are allocated while object zero is already being processed,
            // so their first generic-handler call occurs on the following frame. Hit count
            // four is reached on the spawn frame, after the handler passed their free slots.
            flashbackMotherBrainExplosions?.Step(bus, crossfadeCounter);
        }

        if (babyDiscoveryExistedBeforeThisFrame)
            babyDiscovery?.Step(nmiFrameCounter, crossfadeCounter);

        if (scientistCutsceneExistedBeforeThisFrame)
            scientistCutscene?.Step(bus, crossfadeCounter, introCrossfadeCounter);
    }

    public Rgba32[] Render()
    {
        // The Mode 7 flight owns a fresh PPU setup and INIDISP value. Returning its frame
        // directly is intentional: applying the narration brightness (zero after the
        // preceding fade) would erase the scene after $8B:BDE4 turns the display back on.
        if (ceresFlight is not null && Phase == IntroCinematicPhase.CeresFlight)
        {
            return ceresFlight.Render();
        }

        Rgba32[] pixels = Phase is
            IntroCinematicPhase.MotherBrainCrossfade or
            IntroCinematicPhase.MotherBrainFlashback or
            IntroCinematicPhase.PageTwoCrossfade
            ? RenderMotherBrainFlashback()
            : Phase is IntroCinematicPhase.BabyDiscoveryCrossfade or
                IntroCinematicPhase.BabyDiscovery or
                IntroCinematicPhase.PageThreeCrossfade
                ? RenderBabyDiscovery()
            : Phase is IntroCinematicPhase.BabyMetroidDeliveryCrossfade or
                IntroCinematicPhase.BabyMetroidDelivery or
                IntroCinematicPhase.PageFourCrossfade or
                IntroCinematicPhase.BabyMetroidExaminationCrossfade or
                IntroCinematicPhase.BabyMetroidExamination or
                IntroCinematicPhase.PageFiveCrossfade
                ? RenderScientistCutscene()
            : objects is null
                ? RenderFirstNarration()
                : RenderFirstIllustratedPage();
        ApplyMasterBrightness(pixels);
        return pixels;
    }

    private void SetupMotherBrainFlashback()
    {
        // Shared native setup $8B:B018 moves the persistent narration caret to Y=$F8.
        // It remains alive and will be restored by the next text-page setup.
        objects!.PlaceCaretOffScreen();

        // $8B:AEB8 changes BG1SC to $50. The corresponding 32x32 visual map was already
        // uploaded by the initial $96:FF14 decompression; $8C:BEC3 below is gameplay
        // collision data and must never be expanded as a visual block map.
        flashbackSamus = new SamusState
        {
            Pose = SamusPoseIds.FacingLeftNormalPose,
            XPosition = 155,
            YPosition = 115,
            SelectedHudItem = 1,
            Missiles = 900,
            MaxMissiles = 900,
        };
        flashbackSamus.RefreshCollisionRadii(bus);
        flashbackSamus.InitializeAnimation(bus);
        flashbackSamus.PrimeGraphics(bus);
        // The native NMI consumes the freshly selected top/bottom definitions before the
        // next displayed OAM image. Apply that dedicated DMA now so the first host-rendered
        // flashback frame names initialized OBJ characters rather than stale intro art.
        flashbackSamus.TileTransfers.TransferToVram(bus, vram);
        flashbackMotherBrain = new IntroMotherBrainSpriteState();
        flashbackMotherBrainExplosions = null;
        flashbackRinkas = new IntroRinkaSystem();

        // Retain the exact 224-word level-data copy as debugger-visible state. The later
        // $91:8784 demo movement/collision translation will consume this array; loading it
        // now proves the visual BG1 screen is not standing in for the collision contract.
        MotherBrainLevelData = RomDataReader.ReadFixedBank(bus, 0x8cbec3, 448);
        flashbackLevel = CreateMotherBrainLevel(MotherBrainLevelData);
        flashbackProjectiles.Reset();
        flashbackDemoInput = new DemoInputState();
        flashbackDemoInput.Clear();
        flashbackDemoInput.Enable();
        flashbackDemoInput.LoadObject(bus, 0x8784);

        // $8B:B018 replaces the target palette with kPalettes_Intro, decomposes every
        // component, clears only the incoming gameplay ranges, and immediately composes.
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0028, 0x0003);
        paletteFader.Clear(0x00e0, 0x0010);
        paletteFader.Clear(0x0180, 0x0020);
        paletteFader.Clear(0x01e0, 0x0010);
        paletteFader.ComposeInto(cgram);

        // The sprite setup stored 127 in both cinematic_var13 and (through sprite object
        // $CE55's setup) cinematic_var4. $B250 tests the old counter, then decrements it.
        crossfadeCounter = 127;
        Phase = IntroCinematicPhase.MotherBrainCrossfade;
    }

    /// <summary>
    /// Exact 448-byte $8C:BEC3 room-collision image copied by the Mother Brain setup.
    /// Null until page one has accepted its input edge.
    /// </summary>
    public byte[]? MotherBrainLevelData { get; private set; }

    private void StepMotherBrainCrossfade()
    {
        // $8B:B250 updates only when the pre-decrement counter is divisible by four.
        // Values 124..0 therefore yield exactly 32 component updates.
        if ((crossfadeCounter & 3) == 0)
        {
            paletteFader!.FadeOut(0x0000, 0x0014);
            paletteFader.FadeOut(0x0060, 0x0010);
            paletteFader.FadeOut(0x01d2, 0x0006);
            paletteFader.FadeIn(0x0028, 0x0003);
            paletteFader.FadeIn(0x00e0, 0x0010);
            paletteFader.FadeIn(0x0180, 0x0020);
            paletteFader.FadeIn(0x01e0, 0x0010);
            paletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & 0x8000) == 0)
            return;

        // Transition completion sets TM=$15 and clears English words 128..767 to tile
        // $002F. The top/bottom ornamental rows survive because the loop starts at $80.
        Array.Fill(textTilemap, (ushort)0x002f, startIndex: 128, count: 640);
        vram.ExecuteWordTransfer(textTilemap.AsSpan(0, 0x3c0), 0x4c00, 1);
        Phase = IntroCinematicPhase.MotherBrainFlashback;
    }

    private void SetupPageTwoCrossfade()
    {
        // $8B:B35F spawns page-two's bank-$8C BG object, selects the reverse gameplay-to-
        // text palette routine, and restores the already allocated caret object.
        objects!.StartEnglishPageTwo();
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0000, 0x0010);
        paletteFader.Clear(0x0060, 0x0010);
        paletteFader.Clear(0x01d2, 0x0006);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.PageTwoCrossfade;
    }

    private void StepPageTwoCrossfade()
    {
        // $8B:B3F4 is the exact inverse of the earlier transition: text/portrait ranges
        // fade in while Mother Brain gameplay, projectile, and OBJ ranges fade out.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageTwoText);
    }

    private void StepReverseGameplayToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((crossfadeCounter & 3) == 0)
        {
            paletteFader!.FadeIn(0x0000, 0x0010);
            paletteFader.FadeIn(0x0060, 0x0010);
            paletteFader.FadeIn(0x01d2, 0x0006);
            paletteFader.FadeOut(0x0028, 0x0003);
            paletteFader.FadeOut(0x00e0, 0x0010);
            paletteFader.FadeOut(0x0180, 0x0020);
            paletteFader.FadeOut(0x01e0, 0x0010);
            paletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & 0x8000) != 0)
            Phase = completedPhase;
    }

    private void SetupBabyDiscoveryCrossfade()
    {
        objects!.PlaceCaretOffScreen();

        // $8B:AF7B selects BG1SC=$54 (the second cartridge-authored room page), declares a
        // 32x16 collision room, creates Samus/egg/baby/demo owners, and reuses the same
        // text-to-gameplay palette crossfade as the Mother Brain scene.
        babyDiscovery = new IntroBabyDiscoveryState(bus, audio);
        babyDiscovery.Samus.TileTransfers.TransferToVram(bus, vram);

        // The old gameplay objects have reached their delete lists. Releasing the scoped
        // references prevents their projectile/collision handlers from running behind the
        // new scene while preserving the shared VRAM and CGRAM they intentionally reuse.
        flashbackSamus = null;
        flashbackMotherBrain = null;
        flashbackMotherBrainExplosions = null;
        flashbackRinkas = null;
        flashbackDemoInput = null;
        flashbackLevel = null;
        flashbackProjectiles.Reset();

        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0028, 0x0003);
        paletteFader.Clear(0x00e0, 0x0010);
        paletteFader.Clear(0x0180, 0x0020);
        paletteFader.Clear(0x01e0, 0x0010);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    private void StepBabyDiscoveryCrossfade()
    {
        if ((crossfadeCounter & 3) == 0)
        {
            paletteFader!.FadeOut(0x0000, 0x0014);
            paletteFader.FadeOut(0x0060, 0x0010);
            paletteFader.FadeOut(0x01d2, 0x0006);
            paletteFader.FadeIn(0x0028, 0x0003);
            paletteFader.FadeIn(0x00e0, 0x0010);
            paletteFader.FadeIn(0x0180, 0x0020);
            paletteFader.FadeIn(0x01e0, 0x0010);
            paletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & 0x8000) == 0)
            return;

        // $8B:B29F leaves TM=$15 and clears the English text region after the final fade
        // update. BG1SC already points at $54, so the SR388 room becomes the sole BG1 page.
        Array.Fill(textTilemap, (ushort)0x002f, startIndex: 128, count: 640);
        vram.ExecuteWordTransfer(textTilemap.AsSpan(0, 0x3c0), 0x4c00, 1);
        Phase = IntroCinematicPhase.BabyDiscovery;
    }

    private void SetupPageThreeCrossfade()
    {
        // Egg opcode $B33E selects page three. On the next cinematic-function call $B370
        // spawns its bank-$8C text object and enters the same gameplay-to-text palette
        // routine used after Mother Brain. The SR388 actors continue running beneath it.
        objects!.StartEnglishPageThree();
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0000, 0x0010);
        paletteFader.Clear(0x0060, 0x0010);
        paletteFader.Clear(0x01d2, 0x0006);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.PageThreeCrossfade;
    }

    private void StepPageThreeCrossfade()
    {
        // $B3F4 is shared byte-for-byte by pages two and three. The named outer phase keeps
        // rendering the SR388 actors until its final decrement switches TM back to text.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageThreeText);
    }

    private void SetupBabyMetroidDelivery()
    {
        // Scientist crossfades use the sibling native setup at $8B:B151, which performs
        // the same off-screen caret move before changing the visible layer configuration.
        objects!.PlaceCaretOffScreen();

        // $B0F2 selects BG1SC=$58, starts it 32 pixels right with vertical scroll eight,
        // seeds IntroCrossFadeTimer, and spawns definition $CE61 before configuring the
        // scientist palette transition. Its tilemap is already in the $96:FF14 payload.
        scientistCutscene = IntroScientistCutsceneState.CreateDelivery(audio);
        babyDiscovery = null;

        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0040, 0x0010);
        paletteFader.Clear(0x01c0, 0x0009);
        paletteFader.ComposeInto(cgram);

        // The native intro alternates two adjacent WRAM counters. Page-three setup left
        // CinematicFunctionTimer at $007F; this setup writes the separate intro counter.
        crossfadeCounter = 0x007f;
        introCrossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.BabyMetroidDeliveryCrossfade;
    }

    private void StepBabyMetroidDeliveryCrossfade()
    {
        // $B2D2 fades the portrait/text ranges away while bringing in laboratory BG/OBJ
        // palettes. Only CinematicFunctionTimer is decremented on this direction.
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidDelivery);
    }

    private void StepTextToScientistCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((crossfadeCounter & 3) == 0)
        {
            paletteFader!.FadeOut(0x0000, 0x0014);
            paletteFader.FadeOut(0x0060, 0x0010);
            paletteFader.FadeOut(0x01d2, 0x0006);
            paletteFader.FadeIn(0x0040, 0x0010);
            paletteFader.FadeIn(0x01c0, 0x0009);
            paletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & 0x8000) == 0)
            return;

        Array.Fill(textTilemap, (ushort)0x002f, startIndex: 128, count: 640);
        vram.ExecuteWordTransfer(textTilemap.AsSpan(0, 0x3c0), 0x4c00, 1);
        Phase = completedPhase;
    }

    private void SetupPageFourCrossfade()
    {
        // Actor opcode $B346 selects page four. $B381 starts its text stream and seeds the
        // cinematic-function counter, while the reverse routine deliberately consumes the
        // still-$007F IntroCrossFadeTimer set by the delivery scene.
        objects!.StartEnglishPageFour();
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0000, 0x0010);
        paletteFader.Clear(0x0060, 0x0010);
        paletteFader.Clear(0x01d2, 0x0006);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.PageFourCrossfade;
    }

    private void StepPageFourCrossfade()
    {
        // $B458 is the scientist-specific inverse: text/portrait colors fade in as the
        // laboratory BG and its nine OBJ colors fade out. It consumes the alternate counter.
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFourText);
    }

    private void StepScientistToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((introCrossfadeCounter & 3) == 0)
        {
            paletteFader!.FadeIn(0x0000, 0x0010);
            paletteFader.FadeIn(0x0060, 0x0010);
            paletteFader.FadeIn(0x01d2, 0x0006);
            paletteFader.FadeOut(0x0040, 0x0010);
            paletteFader.FadeOut(0x01c0, 0x0009);
            paletteFader.ComposeInto(cgram);
        }

        introCrossfadeCounter = unchecked((ushort)(introCrossfadeCounter - 1));
        if ((introCrossfadeCounter & 0x8000) != 0)
            Phase = completedPhase;
    }

    private void SetupBabyMetroidExamination()
    {
        objects!.PlaceCaretOffScreen();

        // $B123 selects BG1SC=$5C, starts the page at Y=-24, and spawns definition $CE67.
        // Its setup is otherwise the same two-counter scientist crossfade as delivery.
        scientistCutscene = IntroScientistCutsceneState.CreateExamination(audio);
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0040, 0x0010);
        paletteFader.Clear(0x01c0, 0x0009);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        introCrossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    private void StepBabyMetroidExaminationCrossfade() =>
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidExamination);

    private void SetupPageFiveCrossfade()
    {
        objects!.StartEnglishPageFive();
        paletteFader = new CinematicPaletteFader(introPalette);
        paletteFader.Clear(0x0000, 0x0010);
        paletteFader.Clear(0x0060, 0x0010);
        paletteFader.Clear(0x01d2, 0x0006);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = 0x007f;
        Phase = IntroCinematicPhase.PageFiveCrossfade;
    }

    private void StepPageFiveCrossfade() =>
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFiveText);

    private void SetupPageSix()
    {
        // English takes B1F4's direct fall-through into B207: no palette transition. Only
        // the English text region, caret, eye stream, and final page object are replaced.
        introCrossfadeCounter = 0x007f;
        objects!.StartEnglishPageSix();
        scientistCutscene = null;
        Phase = IntroCinematicPhase.PageSixText;
    }

    private void StepMotherBrainDemo()
    {
        flashbackDemoInput!.Step(bus, specialInstruction: HandleMotherBrainDemoInstruction);

        // HandleHUDSpecificBehaviorAndProjectiles calls the shared cooldown owner before
        // its ordinary-projectile half. Standing pose two cannot place bombs, but running
        // the real owner is still required for missile admission timing.
        flashbackBombProjectiles.StepFrame(
            bus,
            flashbackLevel!,
            flashbackSamus!,
            flashbackDemoInput.Held,
            flashbackDemoInput.NewlyPressed);
        flashbackProjectiles.StepFrame(
            bus,
            flashbackLevel!,
            flashbackSamus!,
            flashbackDemoInput.Held,
            flashbackDemoInput.NewlyPressed,
            layer1X: 0,
            layer1Y: 0,
            flashbackBombProjectiles,
            controllerPreviousNewInput: flashbackDemoInput.PublishedPreviousNewlyPressed);
        if (flashbackProjectiles.LastFrameResult.QueuedSoundEffect is { } soundEffect)
        {
            audio?.QueueSound(
                soundEffect,
                flashbackProjectiles.LastFrameResult.QueuedSoundMaximum);
        }
    }

    /// <summary>
    /// Runs the movement/animation/interruption/palette slice of the native intro-demo
    /// handler at <c>$90:E833-$90:E84D</c>, followed by the timer owner at <c>$8B:8E1A</c>.
    /// </summary>
    private void StepMotherBrainFlashbackSamus()
    {
        SamusState samus = flashbackSamus!;
        RoomLevelData level = flashbackLevel!;
        ushort demoInput = flashbackDemoInput?.Held ?? 0;

        // Accepted NMI transfers the definitions selected by the previous Samus draw. This
        // is what makes pose/frame changes use their corresponding ROM graphics rather than
        // interpreting stale standing tiles as a hurt body on the next visible frame.
        samus.TileTransfers.TransferToVram(bus, vram);

        AerialMovementResult? fallingMovement = null;
        if (samus.KnockbackActive)
        {
            // `$90:E83C` dispatches the installed `$90:DF38` movement handler. Its shared
            // vertical calculation now carries the eleven-frame Rinka arc through its apex.
            SamusKnockbackMovement.Step(bus, level, samus, nmiFrameCounter);
        }
        else if (samus.Pose is SamusPoseIds.FallingRightPose or SamusPoseIds.FallingLeftPose)
        {
            // When `$90:DDE9` ends humanoid knockback it selects pose $29/$2A and restores
            // normal movement. The following frames therefore execute the ordinary type-6
            // fall until the cartridge collision layer reports the original laboratory floor.
            fallingMovement = SamusAerialMovement.StepFalling(
                bus,
                level,
                samus,
                demoInput,
                nmiFrameCounter);
        }

        // Intro-demo state is not an animation shortcut. `$90:E83F` advances the same
        // cartridge delay programs as gameplay, including every visible hurt/fall frame.
        samus.AnimateNoFx(bus, demoInput, nmiFrameCounter);

        // Delay opcodes `$F8/$FD` publish a prospective pose; they are not ordinary frame
        // delays. `$90:E849` consumes that publication before the byte following the opcode
        // can ever be misread as another animation duration. This completes landing art's
        // native A5 -> 02 transition instead of walking beyond its ROM delay list.
        bool animationTransitionApplied = samus.ApplyPendingVerifiedAnimationTransition(bus);

        // The normal new-state handler resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation list all come from the same bank-$91 implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true })
            samus.ApplyAerialLanding(bus, wasSpinning: false, demoInput);

        // `$90:E842` consumes the timer published by Rinka zero on the preceding cinematic-
        // sprite pass. Keeping this after animation matches SamusNewStateHandler_IntroDemo:
        // the hit frame finishes its standing animation before command one installs $53/$54.
        if (!samus.KnockbackActive &&
            samus.KnockbackTimer != 0 &&
            samus.KnockbackDirection == 0)
        {
            SamusKnockbackMovement.Start(
                bus,
                samus,
                demoInput,
                samus.KnockbackXDirection,
                samus.KnockbackTimer);
        }

        // `$90:E84D` runs the ordinary Samus palette handler even in intro-demo state.
        // This supplies the alternating hurt colors and eventual suit-palette restoration;
        // invincibility flicker remains independently enforced by Samus.Draw.
        SamusHurtFlashPalette.Update(bus, cgram, samus, demoInput);

        // `$8B:8E0D` ages the two hit words after both Samus state handlers, but before the
        // cinematic-object walker can publish a new Rinka collision later in this frame.
        samus.DecrementHurtTimers();
    }

    private DemoInputInstructionResult HandleMotherBrainDemoInstruction(
        DemoInputState demo,
        ushort instructionPointer,
        ushort argumentPointer)
    {
        if (instructionPointer != 0x8739)
            return DemoInputInstructionResult.NotHandled(argumentPointer);

        // $91:8739 locks Samus in pose two, reinitializes the pose/animation bookkeeping,
        // disables demo publication, and then returns to the bytecode interpreter. The next
        // physical word is shared delete opcode $8427, so the object is removed in the same
        // handler call. State-handler function pointers are not independently dispatched by
        // this scoped frontend yet; freezing input after the exact pose change has the same
        // observable contract for the remaining Mother Brain explosion frames.
        flashbackSamus!.Pose = SamusPoseIds.FacingLeftNormalPose;
        flashbackSamus.RefreshCollisionRadii(bus);
        flashbackSamus.InitializeAnimation(bus);
        demo.Disable();
        return DemoInputInstructionResult.ContinueAt(argumentPointer);
    }

    private static RoomLevelData CreateMotherBrainLevel(ReadOnlySpan<byte> source)
    {
        const int width = 16;
        const int height = 16;
        var foreground = new ushort[width * height];
        for (int byteOffset = 0; byteOffset < source.Length; byteOffset += 2)
        {
            foreground[byteOffset / 2] = unchecked((ushort)(
                source[byteOffset] | (source[byteOffset + 1] << 8)));
        }

        // $8B:AF30 clears the complete 512-byte word-addressable BTS allocation. BG2 and
        // block-definition data are irrelevant to bank-$94 collision in this visual room;
        // their arrays exist only to satisfy the shared RoomLevelData ownership contract.
        return new RoomLevelData(
            width,
            height,
            foreground,
            new byte[width * height],
            new ushort[width * height],
            Array.Empty<byte>());
    }

    private Rgba32[] RenderMotherBrainFlashback()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight);
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (flashbackMotherBrain!.IsVisible)
        {
            // $B82E clears cinematic_var15 when the reverse crossfade reaches zero. That
            // suppresses Samus and both projectile passes together; it is not merely a
            // Mother Brain visibility bit.
            flashbackSamus!.Draw(bus, oam, layer1X: 0, layer1Y: 0, nmiFrameCounter);
            flashbackProjectiles.DrawLiveProjectiles(bus, oam, 0, 0, nmiFrameCounter);
            flashbackProjectiles.HandleTrailsAndDraw(bus, oam, 0, 0, timeIsFrozen: false);
            flashbackProjectiles.DrawExplosions(bus, oam, 0, 0);
            flashbackRinkas?.Draw(bus, oam);
        }
        flashbackMotherBrainExplosions?.Draw(bus, oam);
        if (flashbackMotherBrain.IsVisible && flashbackMotherBrain.SpriteMapPointer != 0)
        {
            // cinematic_var15=$FFFF makes DrawIntroSprites draw Samus first and cinematic
            // actors afterward. Retaining that OAM insertion order preserves overlap wins.
            oam.AddOnScreenSpritemap(
                bus,
                (int)new SnesAddress(0x8c, flashbackMotherBrain.SpriteMapPointer),
                IntroMotherBrainSpriteState.XPosition,
                IntroMotherBrainSpriteState.YPosition,
                IntroMotherBrainSpriteState.PaletteBits);
        }
        oam.FinalizeFrame();

        // Final TM=$15 Mode-1 ordering, back to front. BG2 is disabled; the surviving BG3
        // ornamental rows and text plane bracket BG1 and OBJ according to priority bits.
        CompositeObjPriority(pixels, oam, 0);
        CompositeTextPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 1);
        CompositeMotherBrainRoomPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 2);
        CompositeMotherBrainRoomPriority(pixels, priority: true);
        CompositeObjPriority(pixels, oam, 3);
        CompositeTextPriority(pixels, priority: true);
        return pixels;
    }

    private Rgba32[] RenderBabyDiscovery()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight);
        var oam = new OamBuffer();
        oam.BeginFrame();
        babyDiscovery!.DrawActors(oam);

        // This cinematic deliberately keeps layer1_x_pos at zero. Samus starts at $178,
        // outside the 256-pixel viewport, and the demo makes her enter from the right; the
        // $54 BG1 screen-base selects different art, not a hidden +$100 camera coordinate.
        babyDiscovery.Samus.TileTransfers.TransferToVram(bus, vram);
        babyDiscovery.Samus.Draw(bus, oam, layer1X: 0, layer1Y: 0, nmiFrameCounter);
        oam.FinalizeFrame();

        CompositeObjPriority(pixels, oam, 0);
        CompositeTextPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 1);
        CompositeBabyDiscoveryRoomPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 2);
        CompositeBabyDiscoveryRoomPriority(pixels, priority: true);
        CompositeObjPriority(pixels, oam, 3);
        CompositeTextPriority(pixels, priority: true);
        return pixels;
    }

    private Rgba32[] RenderScientistCutscene()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight);
        var oam = new OamBuffer();
        oam.BeginFrame();
        scientistCutscene!.Draw(bus, oam);
        oam.FinalizeFrame();

        // TM=$15 uses the same BG1/BG3/OBJ priority ladder as the gameplay flashbacks.
        // BG1SC=$58 selects the first laboratory page; its crossfade pan is real PPU scroll.
        CompositeObjPriority(pixels, oam, 0);
        CompositeTextPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 1);
        CompositeScientistRoomPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 2);
        CompositeScientistRoomPriority(pixels, priority: true);
        CompositeObjPriority(pixels, oam, 3);
        CompositeTextPriority(pixels, priority: true);
        return pixels;
    }

    private void CompositeScientistRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] room = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: scientistCutscene?.TilemapBaseWord ?? 0x5800,
            characterBaseWord: 0,
            horizontalScroll: scientistCutscene?.BackgroundX ?? 0,
            verticalScroll: scientistCutscene?.BackgroundY ?? 0,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, room);
    }

    private void CompositeBabyDiscoveryRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] room = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x5400,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: GameplayFlashbackBg1VerticalScroll,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, room);
    }

    private void CompositeMotherBrainRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] room = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x5000,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: flashbackMotherBrain?.BackgroundVerticalScroll ??
                GameplayFlashbackBg1VerticalScroll,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, room);
    }

    private Rgba32[] RenderFirstNarration()
    {
        // Initial SetupPpu_Intro sets TM=$04: only BG3 is visible. BG3SC=$4C and BG34NBA=$04
        // select tilemap word $4C00 and 2-bpp character word $4000 respectively.
        return SnesBgTilemapRenderer.Render2Bpp(
            vram, cgram, tilemapBaseWord: 0x4c00, characterBaseWord: 0x4000, rowCount: 28);
    }

    private Rgba32[] RenderFirstIllustratedPage()
    {
        // TM=$16 enables BG2, BG3, and OBJ. BG2 is the 4-bpp Samus portrait at SC=$48;
        // BG3 is the progressively written 2-bpp narration at SC=$4C. Both vertical scroll
        // registers are eight, so source scanline eight is the first visible output line.
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight);
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (objects!.SpriteMapPointer != 0)
        {
            // The persistent caret begins at (8,24), moves to (8,$F8) during illustrated
            // crossfades, and is restored when the next narration page starts.
            // The spritemap itself lives in bank $8C and OBSEL=$03 selects word $6000 as
            // the OBJ character base, matching the initial $9A:D200 -> VMADD $6000 DMA.
            oam.AddOnScreenSpritemap(
                bus,
                (int)new SnesAddress(0x8c, objects.SpriteMapPointer),
                originX: objects.CaretX,
                originY: objects.CaretY,
                paletteBits: 0x0c00);
        }
        oam.FinalizeFrame();

        // BGMODE=$09 selects Mode 1 with BG3-priority enabled. With BG1 disabled by TM,
        // the back-to-front order is OBJ0, BG3-low, OBJ1, BG2-low, OBJ2, BG2-high, OBJ3,
        // BG3-high. Rendering priority planes separately is essential: blank low-priority
        // text tiles sit behind the portrait, while the green narration sits above it.
        CompositeObjPriority(pixels, oam, 0);
        CompositeTextPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 1);
        CompositePortraitPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 2);
        CompositePortraitPriority(pixels, priority: true);
        CompositeObjPriority(pixels, oam, 3);
        CompositeTextPriority(pixels, priority: true);

        return pixels;
    }

    private void CompositePortraitPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] portrait = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            tilemapBaseWord: 0x4800,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: 8,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, portrait);
    }

    private void CompositeTextPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] fullText = SnesBgTilemapRenderer.Render2Bpp(
            vram,
            cgram,
            tilemapBaseWord: 0x4c00,
            characterBaseWord: 0x4000,
            rowCount: 32,
            transparentColorZero: true,
            priority: priority);
        var visibleText = new Rgba32[ScreenWidth * ScreenHeight];
        fullText.AsSpan(8 * ScreenWidth, visibleText.Length).CopyTo(visibleText);
        SnesLayerCompositor.Composite(pixels, visibleText);
    }

    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        Rgba32[] sprites = SnesObjRenderer.Render(
            oam,
            vram,
            cgram,
            obsel: 3,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, sprites);
    }

    private void SetupFirstIllustratedPage()
    {
        // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times to
        // $7E:4000, then DMA copies the resulting $600 bytes to VMADD $4180 (byte $8300).
        // English still performs this clear; omitting it exposes stale font glyphs in the
        // otherwise empty lower margin.
        var blankJapaneseCharacters = new byte[0x0600];
        for (int offset = 0; offset < blankJapaneseCharacters.Length; offset += japaneseBlankCharacter.Length)
            japaneseBlankCharacter.CopyTo(blankJapaneseCharacters, offset);
        vram.LoadBytes(0x8300, blankJapaneseCharacters);

        // ClearCinematicBgObjects($2F) fills the complete $7E:3000 staging tilemap. The
        // native border then replaces four 32-tile rows at the top and bottom.
        Array.Fill(textTilemap, (ushort)0x002f);
        for (int index = 0; index < 128; index++)
        {
            textTilemap[index] = 0x3c29;
            textTilemap[index + 896] = 0x3c29;
        }

        // $8B:A72B supplies the four-row ornamental divider at rows 24-27.
        for (int index = 0; index < 128; index++)
            textTilemap[768 + index] = RomDataReader.ReadWordFixedBank(bus, 0x8ba72b + index * 2);

        // `menu.menu_tilemap` begins $600 bytes into the same WRAM union. Its byte offset
        // $11E therefore aliases words 911/912 of the staging map; both receive $1C29.
        textTilemap[911] = 0x1c29;
        textTilemap[912] = 0x1c29;

        vram.ExecuteWordTransfer(textTilemap, 0x4c00, 1);
        objects = new IntroCinematicObjectSystem(bus, vram, textTilemap, audio);
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(MusicCommand.LoadData(0x36));
        audio?.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x0e));
        timer = 0x0e;
        Phase = IntroCinematicPhase.WaitForPageOneMusicQueue;
    }

    /// <summary>
    /// Production follows bank $80's real HasQueuedMusic result. Standalone actor tests do
    /// not own an APU queue, so they retain the previously explicit local countdown.
    /// </summary>
    private bool MusicQueueFinished() => audio is null ? --timer <= 0 : !audio.HasQueuedMusic;

    private void ApplyMasterBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            if (brightness <= 0)
                pixels[pixel] = new Rgba32(0, 0, 0, 255);
            else if (brightness < 15)
                pixels[pixel] = new Rgba32(
                    (byte)(color.R * brightness / 15),
                    (byte)(color.G * brightness / 15),
                    (byte)(color.B * brightness / 15),
                    255);
        }
    }

    private bool StepSlowFade(bool inward, int delayReload)
    {
        // `AdvanceSlowScreenFadeIn/Out` decrements an 8-bit delay first and changes INIDISP
        // when it reaches zero/wraps. The host integer form below preserves the resulting
        // one brightness step every delayReload+1 calls without emulating unrelated PPU IO.
        if (fadeDelay-- > 0)
            return false;
        fadeDelay = delayReload;
        brightness = inward ? Math.Min(15, brightness + 1) : Math.Max(0, brightness - 1);
        return inward ? brightness == 15 : brightness == 0;
    }

    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
    }
}

public enum IntroCinematicPhase
{
    WaitForInitialMusicQueue,
    FadeInFirstNarration,
    LastMetroidIsInCaptivity,
    GalaxyIsAtPeace,
    WaitForSecondMusicQueue,
    FourSecondHold,
    FadeOutFirstNarration,
    WaitForPageOneMusicQueue,
    FadeInPageOne,
    PageOneText,
    PageOneAwaitingInput,
    MotherBrainCrossfade,
    MotherBrainFlashback,
    PageTwoCrossfade,
    PageTwoText,
    PageTwoAwaitingInput,
    BabyDiscoveryCrossfade,
    BabyDiscovery,
    PageThreeCrossfade,
    PageThreeText,
    PageThreeAwaitingInput,
    BabyMetroidDeliveryCrossfade,
    BabyMetroidDelivery,
    PageFourCrossfade,
    PageFourText,
    PageFourAwaitingInput,
    BabyMetroidExaminationCrossfade,
    BabyMetroidExamination,
    PageFiveCrossfade,
    PageFiveText,
    PageFiveAwaitingInput,
    PageSixText,
    IntroFadeOut,
    CeresFlight,
}
