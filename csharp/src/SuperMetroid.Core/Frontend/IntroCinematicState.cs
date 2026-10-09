using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
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
public sealed partial class IntroCinematicState
{
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Current host appearance; snapshots retain simulation state, not external overrides.</summary>
    [NonSerialized] private ProjectileTrailCatalog? trailArtwork;
    [NonSerialized] private bool trailArtworkRefreshPending;
    /// <summary>Gets or sets the host-owned projectile-trail artwork used by flashbacks.</summary>
    public ProjectileTrailCatalog? TrailArtwork
    {
        get => trailArtwork;
        set { trailArtwork = value; trailArtworkRefreshPending = value?.Tiles is not null; }
    }
    /// <summary>Current timed-projectile composition selected by the host.</summary>
    [field: NonSerialized]
    public ProjectileSpriteCatalog? ProjectileCompositions { get; set; }
    /// <summary>Host-owned frame artwork for the Mother Brain flashback projectile slots.</summary>
    public ProjectileFrameBindingCatalog? ProjectileFrameBindings
    {
        get => flashbackProjectiles.FrameBindings;
        set => flashbackProjectiles.FrameBindings = value;
    }
    [NonSerialized] private IntroNarrationPresentation? narrationPresentation;
    [NonSerialized] private IntroFontAtlas? introFont;
    [NonSerialized] private IntroCinematicArtworkCatalog? characterArtwork;
    [NonSerialized] private BeamTileCatalog? beamArtwork;
    [NonSerialized] private SamusBodyArtworkCatalog? samusBodyArtwork;
    [NonSerialized] private SamusHurtColorCatalog? samusHurtColors;
    /// <summary>Editable suit-flash colors used by the cinematic's ordinary Samus hurt handler.</summary>
    public void BindSamusHurtColors(SamusHurtColorCatalog? value) => samusHurtColors = value;
    /// <summary>Rebinds installed Samus pixels to active and later intro flashbacks.</summary>
    public void BindSamusBodyArtwork(SamusBodyArtworkCatalog? value)
    {
        samusBodyArtwork = value;
        flashbackSamus?.TileTransfers.BindArtwork(value);
        babyDiscovery?.Samus.TileTransfers.BindArtwork(value);
    }
    /// <summary>Current host-owned narration content; debugger states retain only playback state.</summary>
    public IntroNarrationPresentation? NarrationPresentation
    {
        get => narrationPresentation;
        set
        {
            narrationPresentation = value;
            objects?.BindNarration(value);
        }
    }
    /// <summary>Current host-owned opening font; restored states must explicitly rebind it.</summary>
    public void BindIntroFont(IntroFontAtlas? value)
    {
        introFont = value;
        if (value is not null)
            ApplyIntroFont(value.Transfer.Span);
    }
    /// <summary>Rebinds the beam DMA left resident under the opening OBJ sheets.</summary>
    public void BindBeamArtwork(BeamTileCatalog? value)
    {
        beamArtwork = value;
        if (value is not null)
            SamusProjectileSystem.LoadBeamTiles(bus, vram, equippedBeams: 0,
                artwork: value);
    }
    /// <summary>Rebinds installed BG and OBJ pixels after restoring emulated state.</summary>
    public void BindCharacterArtwork(IntroCinematicArtworkCatalog? value)
    {
        characterArtwork = value;
        ceresFlight?.BindArtwork(value?.CeresFlight);
        if (value is not null)
        {
            // A restored fade keeps its current CGRAM/accumulators, but later scene
            // targets must use the currently selected installed palette.
            var baseline = new SnesCgram();
            value.Palette.LoadTo(baseline);
            baseline.Colors.CopyTo(introPalette);
            if (Phase is IntroCinematicPhase.Initial or IntroCinematicPhase.WaitForInitialMusicQueue)
                value.Palette.LoadTo(cgram);
            if (Phase >= IntroCinematicPhase.WaitForPageOneMusicQueue &&
                Phase < IntroCinematicPhase.CeresFlight)
            {
                // The divider occupies only rows 24-27. Preserve live typewriter
                // words and caret state in every other row on debugger rebind.
                value.FinalLine.Words.Span.CopyTo(textTilemap.AsSpan(
                    IntroCinematicRomData.Text.FinalLineDestinationStart,
                    IntroCinematicRomData.Text.FinalLineWordCount));
                vram.ExecuteWordTransfer(value.FinalLine.Words.Span,
                    (ushort)(IntroCinematicRomData.Layers.NarrationTilemapWord +
                        IntroCinematicRomData.Text.FinalLineDestinationStart), 1);
            }
            vram.LoadBytes(IntroCinematicRomData.Vram.BackgroundCharacterDestinationByte,
                value.BackgroundCharacters.Transfer.Span);
            vram.LoadBytes(IntroCinematicRomData.Vram.SamusHeadTilemapDestinationByte,
                value.PortraitTilemap.Span);
            // The active eye rectangle overlays the base portrait after its VRAM upload.
            objects?.BindEyeArtwork(value.EyeFrames);
            // $8B:A66F replaces the initial BG3 card with live typewriter words
            // when page one begins. Never overwrite that current state on restore.
            if (InitialNarrationCardResident)
                vram.LoadBytes(IntroCinematicRomData.Vram.NarrationTilemapDestinationByte,
                    value.InitialNarrationTilemap.Span);
            vram.LoadBytes(IntroCinematicRomData.Vram.BackgroundPagesDestinationByte,
                value.BackgroundPages.Span);
            vram.LoadBytes(IntroCinematicRomData.Vram.IntroObjectCharactersDestinationByte,
                value.IntroObjectCharacters.Transfer.Span);
            vram.LoadBytes(IntroCinematicRomData.Vram.CinematicObjectCharactersDestinationByte,
                value.CinematicObjectCharacters.Transfer.Span);
            // $8B:A3AC uploads beam tiles after the opening OBJ sheets; retain that
            // overlap without reloading the beam palette over a restored scene palette.
            SamusProjectileSystem.LoadBeamTiles(bus, vram, equippedBeams: 0,
                beamArtwork);
        }
        else
            objects?.BindEyeArtwork(null);
    }
    private const int ScreenWidth = SnesPpuLayout.ScreenWidthPixels;
    private const int ScreenHeight = SnesPpuLayout.ScreenHeightPixels;

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
    private readonly ushort[] textTilemap =
        new ushort[IntroCinematicRomData.Layers.TextTilemapWordCount];
    private byte[] japaneseBlankCharacter = [];
    private readonly ushort[] introPalette = new ushort[SnesCgram.ColorCount];
    private IntroCinematicObjectSystem? objects;
    private CinematicPaletteFader? paletteFader;
    private SamusState? flashbackSamus;
    /// <summary>$1A57 during the Mother Brain flashback: $8B:AF65 sets it, $8B:B842 clears it.</summary>
    private IntroSamusDisplay flashbackSamusDisplay;
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
    /// <summary>$51: the INIDISP byte the cinematic fades; the intro starts in forced blank.</summary>
    private int inidisp = ScreenFade.ForcedBlank;
    private readonly ScreenFade fade = new();
    private int timer = 8;

    /// <summary>Creates the opening cinematic and performs its initial native graphics and music setup.</summary>
    /// <param name="bus">The cartridge address space used by cinematic actors and projectiles.</param>
    /// <param name="audio">Optional cartridge audio state that owns queued music and sound effects.</param>
    /// <param name="introFont">The installed opening-cinematic font atlas.</param>
    /// <param name="characterArtwork">The installed opening-cinematic BG and OBJ artwork.</param>
    /// <param name="beamArtwork">Optional installed beam graphics used by gameplay flashbacks.</param>
    /// <param name="samusBodyArtwork">Optional installed Samus body artwork used by gameplay flashbacks.</param>
    public IntroCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null,
        IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null,
        SamusBodyArtworkCatalog? samusBodyArtwork = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        this.bus = bus;
        this.audio = audio;
        this.introFont = introFont ?? throw new InvalidOperationException(
            "Opening cinematic requires the installed font atlas.");
        this.characterArtwork = characterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic requires installed artwork.");
        this.beamArtwork = beamArtwork;
        this.samusBodyArtwork = samusBodyArtwork;
        characterArtwork.Palette.LoadTo(cgram);
        cgram.Colors.CopyTo(introPalette);

        byte[] bgCharacters = characterArtwork.BackgroundCharacters.Transfer.ToArray();
        byte[] fontOne = introFont.Transfer.ToArray();
        byte[] samusHeadTilemap = characterArtwork.PortraitTilemap.ToArray();
        byte[] bg1Pages = characterArtwork.BackgroundPages.ToArray();
        byte[] introObjects = characterArtwork.CinematicObjectCharacters.Transfer.ToArray();
        byte[] firstNarrationTilemap = characterArtwork.InitialNarrationTilemap.ToArray();

        RequireMinimum(bgCharacters, IntroCinematicRomData.Vram.BackgroundCharacterBytes,
            "intro BG1/BG2 characters");
        RequireMinimum(fontOne, IntroCinematicRomData.Vram.FontOneBytes, "intro font one");
        RequireMinimum(samusHeadTilemap, IntroCinematicRomData.Vram.SamusHeadTilemapBytes,
            "Samus-head BG2 tilemap");
        RequireMinimum(bg1Pages, IntroCinematicRomData.Vram.BackgroundPageTilemapBytes,
            "intro BG1 page tilemaps");
        RequireMinimum(introObjects, IntroCinematicRomData.Vram.ObjectCharacterBytes,
            "intro OBJ characters");
        RequireMinimum(firstNarrationTilemap, IntroCinematicRomData.Vram.NarrationTilemapBytes,
            "first narration BG3 tilemap");

        // BTS[$1E8E-$1E9D] aliases $7F:8290-$829F while the first font is resident in
        // decompression RAM. $8B:A86A repeats precisely this character when blanking the
        // optional Japanese glyph area; retain the source before host staging is discarded.
        // Literal VMADD values from `$8B:A469-$A529`, converted to physical byte offsets.
        vram.LoadBytes(IntroCinematicRomData.Vram.BackgroundCharacterDestinationByte,
            bgCharacters.AsSpan(0, IntroCinematicRomData.Vram.BackgroundCharacterBytes));
        ApplyIntroFont(fontOne);
        vram.LoadBytes(IntroCinematicRomData.Vram.SamusHeadTilemapDestinationByte,
            samusHeadTilemap.AsSpan(0, IntroCinematicRomData.Vram.SamusHeadTilemapBytes));
        vram.LoadBytes(IntroCinematicRomData.Vram.NarrationTilemapDestinationByte,
            firstNarrationTilemap.AsSpan(0, IntroCinematicRomData.Vram.NarrationTilemapBytes));
        vram.LoadBytes(IntroCinematicRomData.Vram.BackgroundPagesDestinationByte,
            bg1Pages.AsSpan(0, IntroCinematicRomData.Vram.BackgroundPageTilemapBytes));
        vram.LoadBytes(IntroCinematicRomData.Vram.IntroObjectCharactersDestinationByte,
            characterArtwork.IntroObjectCharacters.Transfer.ToArray());
        vram.LoadBytes(IntroCinematicRomData.Vram.CinematicObjectCharactersDestinationByte,
            introObjects.AsSpan(0, IntroCinematicRomData.Vram.ObjectCharacterBytes));

        // $8B:A3AC performs the ordinary beam tile/palette upload before copying the full
        // intro palette. The projectile tile DMA remains resident for both gameplay
        // flashbacks; restore the later intro CGRAM copy after using the shared helper.
        SamusProjectileSystem.LoadBeamTilesAndPalette(bus, vram, cgram,
            equippedBeams: 0, beamArtwork);
        characterArtwork.Palette.LoadTo(cgram);

        // The PPU loads above are invisible: the intro stays in forced blank until its fade.
        // $8B:A395 itself runs as the next dispatch's cinematic function.
        Phase = IntroCinematicPhase.Initial;
    }

    private void ApplyIntroFont(ReadOnlySpan<byte> transfer)
    {
        if (transfer.Length != IntroFontAtlasFormat.ByteCount)
        {
            throw new InvalidDataException(
                $"Opening font contains {transfer.Length} bytes; expected " +
                $"{IntroFontAtlasFormat.ByteCount}.");
        }
        japaneseBlankCharacter = transfer.Slice(
            IntroCinematicRomData.Text.JapaneseBlankSourceOffset,
            IntroCinematicRomData.Text.JapaneseBlankCharacterByteCount).ToArray();
        vram.LoadBytes(IntroCinematicRomData.Vram.FontOneDestinationByte, transfer);
    }

    /// <summary>
    /// True until $8B:A66F replaces the initial BG3 card. The setup phases appended to the
    /// enum precede it in time even though they follow it numerically.
    /// </summary>
    private bool InitialNarrationCardResident =>
        Phase is IntroCinematicPhase.Initial or IntroCinematicPhase.SetupPageOne ||
        Phase < IntroCinematicPhase.WaitForPageOneMusicQueue;

    /// <summary>Gets the current opening-cinematic state-machine phase.</summary>
    public IntroCinematicPhase Phase { get; private set; }

    /// <summary>True after the final narration fade hands control to the Ceres flight.</summary>
    public bool NarrationFinished { get; private set; }

    /// <summary>True when the SPACE COLONY caption and its final fade have completed.</summary>
    public bool CeresFlightFinished => ceresFlight?.Finished ?? false;

    /// <summary>Advances the opening cinematic by one gameplay update.</summary>
    /// <param name="controllerInput">The current raw controller-held word.</param>
    public void Step(ushort controllerInput)
    {
        flashbackProjectiles.BeginImpactAudioFrame(cinematicActive: true);
        // Cinematic functions consume joypad1_newkeys, not the raw held word. Latching at
        // this state boundary preserves a one-frame edge and prevents a held A/Start from
        // skipping both the options menu and the first narration page.
        controller.Latch(controllerInput);
        bool explosionsExistedBeforeThisFrame = flashbackMotherBrainExplosions is not null;
        nmiFrameCounter++;
        switch (Phase)
        {
            case IntroCinematicPhase.Initial:
                // $8B:A59B-$A5A2: stop the music, then load the opening music data.
                audio?.QueueMusicDelayed8(MusicCommand.Stop);
                audio?.QueueMusicDelayed8(
                    MusicCommand.LoadData(IntroCinematicRomData.Music.OpeningDataIndex));
                Phase = IntroCinematicPhase.WaitForInitialMusicQueue;
                break;

            case IntroCinematicPhase.WaitForInitialMusicQueue:
                // QueueMusic_Delayed8 owns an eight-frame delay before HasQueuedMusic clears.
                if (MusicQueueFinished())
                {
                    // $8B:A5A7 seeds both fade words with two.
                    Phase = IntroCinematicPhase.FadeInFirstNarration;
                    fade.SetTiming(IntroCinematicRomData.Fade.NarrationSlowFade, IntroCinematicRomData.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeInFirstNarration:
                if (fade.AdvanceSlowFadeIn(ref inidisp))
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
                    audio?.QueueMusicDelayed8(
                        MusicCommand.LoadData(IntroCinematicRomData.Music.MotherBrainDataIndex));
                    audio?.QueueMusicDelayed(
                        MusicCommand.SelectTrack(IntroCinematicRomData.Music.SceneTrack),
                        MusicCommandDelay.FromDelayedYArgument(
                            IntroCinematicRomData.Music.SceneTrackDelayArgument));
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
                    // $8B:A64C seeds both fade words with two.
                    Phase = IntroCinematicPhase.FadeOutFirstNarration;
                    fade.SetTiming(IntroCinematicRomData.Fade.NarrationSlowFade, IntroCinematicRomData.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeOutFirstNarration:
                // $8B:A663 installs $8B:A66F, which sets up page one in the next dispatch.
                if (fade.AdvanceSlowFadeOut(ref inidisp))
                    Phase = IntroCinematicPhase.SetupPageOne;
                break;

            case IntroCinematicPhase.SetupPageOne:
                SetupFirstIllustratedPage();
                break;

            case IntroCinematicPhase.WaitForPageOneMusicQueue:
                // QueueMusic_DelayedY(track 5, $0E) is the longest command issued by
                // $8B:A66F, so HasQueuedMusic becomes clear after fourteen update slots.
                if (MusicQueueFinished())
                {
                    objects!.StartEnglishPageOne();
                    // $8B:A82B seeds both fade words with two.
                    Phase = IntroCinematicPhase.FadeInPageOne;
                    fade.SetTiming(IntroCinematicRomData.Fade.NarrationSlowFade, IntroCinematicRomData.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeInPageOne:
                if (fade.AdvanceSlowFadeIn(ref inidisp))
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
                // $8B:B72F fades through HandleFadingOut and finishes once forced blank is set.
                fade.FadeOut(ref inidisp);
                if (ScreenFade.IsForcedBlank(inidisp))
                {
                    NarrationFinished = true;
                    // $8B:BCA0 immediately replaces the narration PPU setup with the
                    // Ceres Mode 7 scene. Give that scene its own VRAM, CGRAM, and
                    // brightness owner: the narration has just deliberately reached
                    // INIDISP zero, while $8B:BDE4 restores full brightness only after
                    // the fourteen-frame delayed music command has completed.
                    ceresFlight = new IntroCeresFlightState(bus, audio
                        ?? throw new InvalidOperationException("The Ceres flight waits on the cartridge music queue."),
                        characterArtwork?.CeresFlight);
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
                fade.SetTiming(IntroCinematicRomData.Fade.FinishFade, IntroCinematicRomData.Fade.FinishFade);
                Phase = IntroCinematicPhase.IntroFadeOut;
            }
        }

        // $8B:8E0D runs Samus's handlers, ahead of the sprite pass, only while $1A57 is set.
        if (flashbackSamus is not null && flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            // Intro-demo alpha refreshes the live radius before projectiles and beta movement.
            // Pose commits at the end of the previous frame deliberately leave it unchanged.
            flashbackSamus.RefreshCollisionRadii(bus);

            // $90:E91D runs DemoInputObjectHandler inside Samus's current-state handler,
            // after the cinematic function, so a demo loaded by $8B:AEB8 plays its first
            // entry in the same dispatch, ahead of the Samus pose input that consumes it.
            if (flashbackDemoInput is not null)
                StepMotherBrainDemo();

            StepMotherBrainFlashbackSamus();
        }

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
            // $8B:B842: deleting Mother Brain after the page-two crossfade hides Samus from
            // the next dispatch on.
            if (!flashbackMotherBrain.IsVisible)
                flashbackSamusDisplay = IntroSamusDisplay.Hidden;
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
                motherBrainExploding: flashbackMotherBrain?.ExplosionStarted == true,
                explosionsAllocated: flashbackMotherBrainExplosions is not null);
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

        // Handle_CinematicSpriteObjects ($8B:93EF) runs after the cinematic function, so the
        // actors a scene setup spawns take their first step in that same dispatch.
        babyDiscovery?.Step(nmiFrameCounter, crossfadeCounter);
        scientistCutscene?.Step(bus, crossfadeCounter, introCrossfadeCounter);
    }

    /// <summary>Renders the current cinematic phase into the reusable 256-by-224 RGBA frame buffer.</summary>
    /// <returns>The current cinematic frame, valid until the next render call.</returns>
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
        // Shared Samus handlers read the nonzero cinematic-function word to select
        // intro palette restoration and suppress gameplay-only sound/landing effects.
        flashbackSamus.LiquidPhysics.CinematicFunctionActive = true;
        flashbackSamus.TileTransfers.BindArtwork(samusBodyArtwork);
        flashbackSamus.RefreshCollisionRadii(bus);
        flashbackSamus.InitializeAnimation(bus);
        flashbackSamus.CommitPoseHistory(bus);
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
        MotherBrainLevelData = IntroMotherBrainCollisionDefinitions.CopySourceBytes();
        flashbackLevel = CreateMotherBrainLevel(MotherBrainLevelData);
        flashbackProjectiles.Reset();
        flashbackDemoInput = new DemoInputState();
        flashbackDemoInput.Clear();
        flashbackDemoInput.Enable();
        flashbackDemoInput.LoadObject(bus, IntroMotherBrainInputDefinitions.HeaderStart,
            definitionWord: IntroMotherBrainInputDefinitions.ReadWord);
        // $8B:AF62: Samus is processed, and drawn ahead of the cinematic objects.
        flashbackSamusDisplay = IntroSamusDisplay.SamusFirst;

        // $8B:B018 replaces the target palette with kPalettes_Intro, decomposes every
        // component, clears only the incoming gameplay ranges, and immediately composes.
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.Narration);
        paletteFader.ComposeInto(cgram);

        // The sprite setup stored 127 in both cinematic_var13 and (through sprite object
        // $CE55's setup) cinematic_var4. $B250 tests the old counter, then decrements it.
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
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
        if ((crossfadeCounter & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Narration);
            paletteFader!.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & IntroCinematicRomData.Palette.CounterSignBit) == 0)
            return;

        // Transition completion sets TM=$15 and clears English words 128..767 to tile
        // $002F. The top/bottom ornamental rows survive because the loop starts at $80.
        ClearVisibleNarrationText();
        Phase = IntroCinematicPhase.MotherBrainFlashback;
    }

    private void SetupPageTwoCrossfade()
    {
        // $8B:B35F spawns page-two's bank-$8C BG object, selects the reverse gameplay-to-
        // text palette routine, and restores the already allocated caret object.
        objects!.StartEnglishPageTwo();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
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
        if ((crossfadeCounter & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeInPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Narration);
            paletteFader!.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & IntroCinematicRomData.Palette.CounterSignBit) != 0)
            Phase = completedPhase;
    }

    private void SetupBabyDiscoveryCrossfade()
    {
        objects!.PlaceCaretOffScreen();

        // $8B:AF7B selects BG1SC=$54 (the second cartridge-authored room page), declares a
        // 32x16 collision room, carries Samus into new egg/baby/demo owners, and reuses the same
        // text-to-gameplay palette crossfade as the Mother Brain scene.
        babyDiscovery = new IntroBabyDiscoveryState(bus, audio, flashbackSamus
            ?? throw new InvalidOperationException("Discovery setup requires the preceding flashback Samus state."));
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
        ClearPaletteSpans(IntroCinematicRomData.Palette.Narration);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        Phase = IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    private void StepBabyDiscoveryCrossfade()
    {
        if ((crossfadeCounter & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Narration);
            paletteFader!.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & IntroCinematicRomData.Palette.CounterSignBit) == 0)
            return;

        // $8B:B29F leaves TM=$15 and clears the English text region after the final fade
        // update. BG1SC already points at $54, so the SR388 room becomes the sole BG1 page.
        ClearVisibleNarrationText();
        Phase = IntroCinematicPhase.BabyDiscovery;
    }

    private void SetupPageThreeCrossfade()
    {
        // Egg opcode $B33E selects page three. On the next cinematic-function call $B370
        // spawns its bank-$8C text object and enters the same gameplay-to-text palette
        // routine used after Mother Brain. The SR388 actors continue running beneath it.
        objects!.StartEnglishPageThree();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
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
        ClearPaletteSpans(IntroCinematicRomData.Palette.Discovery);
        paletteFader.ComposeInto(cgram);

        // The native intro alternates two adjacent WRAM counters. Page-three setup left
        // CinematicFunctionTimer at $007F; this setup writes the separate intro counter.
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        introCrossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
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
        if ((crossfadeCounter & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Discovery);
            paletteFader!.ComposeInto(cgram);
        }

        crossfadeCounter = unchecked((ushort)(crossfadeCounter - 1));
        if ((crossfadeCounter & IntroCinematicRomData.Palette.CounterSignBit) == 0)
            return;

        ClearVisibleNarrationText();
        Phase = completedPhase;
    }

    private void SetupPageFourCrossfade()
    {
        // Actor opcode $B346 selects page four. $B381 starts its text stream and seeds the
        // cinematic-function counter, while the reverse routine deliberately consumes the
        // still-$007F IntroCrossFadeTimer set by the delivery scene.
        objects!.StartEnglishPageFour();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
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
            FadeInPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Discovery);
            paletteFader!.ComposeInto(cgram);
        }

        introCrossfadeCounter = unchecked((ushort)(introCrossfadeCounter - 1));
        if ((introCrossfadeCounter & IntroCinematicRomData.Palette.CounterSignBit) != 0)
            Phase = completedPhase;
    }

    private void SetupBabyMetroidExamination()
    {
        objects!.PlaceCaretOffScreen();

        // $B123 selects BG1SC=$5C, starts the page at Y=-24, and spawns definition $CE67.
        // Its setup is otherwise the same two-counter scientist crossfade as delivery.
        scientistCutscene = IntroScientistCutsceneState.CreateExamination(audio);
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.Discovery);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        introCrossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        Phase = IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    private void StepBabyMetroidExaminationCrossfade() =>
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidExamination);

    private void SetupPageFiveCrossfade()
    {
        objects!.StartEnglishPageFive();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        Phase = IntroCinematicPhase.PageFiveCrossfade;
    }

    private void StepPageFiveCrossfade() =>
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFiveText);

    private void SetupPageSix()
    {
        // English takes B1F4's direct fall-through into B207: no palette transition. Only
        // the English text region, caret, eye stream, and final page object are replaced.
        introCrossfadeCounter = IntroCinematicRomData.Palette.CrossfadeInitialCounter;
        objects!.StartEnglishPageSix();
        scientistCutscene = null;
        Phase = IntroCinematicPhase.PageSixText;
    }

    private void StepMotherBrainDemo()
    {
        flashbackDemoInput!.Step(bus, specialInstruction: HandleMotherBrainDemoInstruction,
            instructionWord: IntroMotherBrainInputDefinitions.ReadWord);

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

        // $90:E70D/$90:E71C: both the demo and locked handlers end with
        // ResetMovementAndPoseChangeVariables, so a later missile inherits only the
        // movement recorded in its own dispatch.
        flashbackSamus!.ClearPoseTransitionShotDirection();
        SamusProjectileInheritance.ClearMovement(bus);
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
        BlockMoveResult? hurtEndingProbe = null;
        bool transitionAccepted = false;
        bool groundedDemoStepped = false;
        if (samus.KnockbackActive)
        {
            // `$90:E83C` dispatches the installed `$90:DF38` movement handler. Its shared
            // vertical calculation now carries the eleven-frame Rinka arc through its apex.
            SamusKnockbackMovement.Step(bus, level, samus, nmiFrameCounter);
        }
        else if (samus.Pose is SamusPoseIds.KnockbackRightPose or SamusPoseIds.KnockbackLeftPose)
        {
            hurtEndingProbe = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
                bus, level, samus, nmiFrameCounter);
        }
        else if (flashbackDemoInput?.Enabled == true &&
            (SamusState.IsLeftFacingStandingPose(samus.Pose) || SamusState.IsLeftFacingRunningPose(samus.Pose)))
        {
            // The demo's later run/jump records use the same alpha/beta order as the
            // discovery scene. This coordinator already animates and commits its pose.
            IntroSamusDemoMovement.StepGroundedLeft(bus, level, samus, demoInput,
                flashbackDemoInput.NewlyPressed, nmiFrameCounter);
            groundedDemoStepped = true;
        }
        else if (samus.Pose == SamusPoseIds.SpinJumpLeftPose)
        {
            fallingMovement = SamusAerialMovement.StepSpinJump(bus, level, samus,
                demoInput, nmiFrameCounter, flashbackDemoInput?.NewlyPressed ?? 0);
        }
        else if (SamusState.IsLeftFacingNormalJumpPose(samus.Pose))
        {
            fallingMovement = SamusAerialMovement.StepNormalJump(bus, level, samus,
                demoInput, nmiFrameCounter);
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
        if (!groundedDemoStepped)
            samus.AnimateNoFx(bus, demoInput, nmiFrameCounter);

        // Delay opcodes `$F8/$FD` publish a prospective pose; they are not ordinary frame
        // delays. `$90:E849` consumes that publication before the byte following the opcode
        // can ever be misread as another animation duration. This completes landing art's
        // native A5 -> 02 transition instead of walking beyond its ROM delay list.
        bool animationTransitionApplied = samus.ApplyPendingVerifiedAnimationTransition(bus);
        animationTransitionApplied |= SamusKnockbackMovement.TryFinishExpiredHitInterruption(bus, samus);
        transitionAccepted |= animationTransitionApplied;

        if (!animationTransitionApplied && hurtEndingProbe is { IsUnobstructedDownwardMovement: true })
        {
            samus.ApplyWalkedOffFloorTransition(bus, level, samus.SelectFallingPoseForCurrentAim(bus), nmiFrameCounter);
            animationTransitionApplied = transitionAccepted = true;
        }

        // The normal new-state handler resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation list all come from the same bank-$91 implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true })
        {
            samus.ApplyAerialLanding(bus, wasSpinning: samus.Pose == SamusPoseIds.SpinJumpLeftPose, demoInput);
            transitionAccepted = true;
        }

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
            transitionAccepted = true;
        }

        // The intro uses the ordinary transition epilogue after movement, animation,
        // and hurt interruption. Publish only the final accepted pose, once per slot;
        // an unchanged animation frame must not erase the preceding transition history.
        if (transitionAccepted)
            samus.CommitPoseHistory(bus);

        // `$90:E84D` runs the ordinary Samus palette handler even in intro-demo state.
        // This supplies the alternating hurt colors and eventual suit-palette restoration;
        // invincibility flicker remains independently enforced by Samus.Draw.
        SamusHurtFlashPalette.Update(bus, cgram, samus, demoInput, samusHurtColors);

        // `$8B:8E0D` ages the two hit words after both Samus state handlers, but before the
        // cinematic-object walker can publish a new Rinka collision later in this frame.
        samus.DecrementHurtTimers();
    }

    private DemoInputInstructionResult HandleMotherBrainDemoInstruction(
        DemoInputState demo,
        ushort instructionPointer,
        ushort argumentPointer)
    {
        if (instructionPointer != IntroCinematicRomData.Flashback.ExpectedEndInstruction)
            return DemoInputInstructionResult.NotHandled(argumentPointer);

        // $91:8739 locks Samus in pose two, reinitializes the pose/animation bookkeeping,
        // disables demo publication, and then returns to the bytecode interpreter. The next
        // physical word is shared delete opcode $8427, so the object is removed in the same
        // handler call. State-handler function pointers are not independently dispatched by
        // this scoped frontend yet; freezing input after the exact pose change has the same
        // observable contract for the remaining Mother Brain explosion frames.
        flashbackSamus!.Pose = SamusPoseIds.FacingLeftNormalPose;
        flashbackSamus.RefreshCollisionRadii(bus);
        // $91:874B: a Samus already in pose two keeps her running animation.
        flashbackSamus.SetAnimationFrameIfPoseChanged(bus);
        flashbackSamus.CommitPoseHistory(bus);
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
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        OamBuffer oam = PrepareMotherBrainOam();

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

    // Preparation remains on the simulation/display boundary: trail drawing advances
    // trail state, so a consumer must never call it to redraw an already captured frame.
    private OamBuffer PrepareMotherBrainOam()
    {
        // Both direct rendering and detached capture enter here once per display.
        // The intro's standard OBJ upload owns these same trail regions; its separate
        // compressed cinematic sheet starts later and must not be replaced.
        if (trailArtworkRefreshPending)
        {
            trailArtwork!.Tiles!.LoadTo(vram);
            trailArtworkRefreshPending = false;
        }
        var oam = new OamBuffer();
        oam.BeginFrame();
        // $8B:8E2D: a negative $1A57 draws Samus and both projectile passes before the
        // cinematic objects; once $B842 clears it, only the objects remain.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            flashbackSamus!.Draw(bus, oam, layer1X: 0, layer1Y: 0, nmiFrameCounter);
            flashbackProjectiles.DrawLiveProjectiles(bus, oam, 0, 0, nmiFrameCounter, ProjectileCompositions);
            flashbackProjectiles.HandleTrailsAndDraw(bus, oam, 0, 0, timeIsFrozen: false, TrailArtwork);
            flashbackProjectiles.DrawExplosions(bus, oam, 0, 0, ProjectileCompositions);
        }
        flashbackRinkas?.Draw(bus, oam, characterArtwork?.RinkaSprites);
        flashbackMotherBrainExplosions?.Draw(oam,
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro Mother Brain explosions require installed character artwork.")).MotherBrainExplosionSprites);
        if (flashbackMotherBrain!.IsVisible && flashbackMotherBrain.SpriteMapPointer != 0)
        {
            // cinematic_var15=$FFFF makes DrawIntroSprites draw Samus first and cinematic
            // actors afterward. Retaining that OAM insertion order preserves overlap wins.
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro Mother Brain sprites require installed character artwork.")).MotherBrainSprites.Draw(
                flashbackMotherBrain.SpriteMapPointer, oam,
                IntroMotherBrainSpriteState.XPosition,
                IntroMotherBrainSpriteState.YPosition,
                IntroMotherBrainSpriteState.PaletteBits);
        }
        oam.FinalizeFrame();

        return oam;
    }

    private Rgba32[] RenderBabyDiscovery()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        OamBuffer oam = PrepareBabyDiscoveryOam();

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

    private OamBuffer PrepareBabyDiscoveryOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        // $8B:8E2D orders Samus against the cinematic objects by $1A57's sign.
        IntroSamusDisplay display = babyDiscovery!.SamusDisplay;
        if (display == IntroSamusDisplay.SamusFirst)
            DrawBabyDiscoverySamus(oam);
        babyDiscovery.DrawActors(oam, characterArtwork?.EggEffectSprites,
            characterArtwork?.DiscoveryActorSprites);
        if (display == IntroSamusDisplay.ObjectsFirst)
            DrawBabyDiscoverySamus(oam);
        oam.FinalizeFrame();

        return oam;
    }

    private void DrawBabyDiscoverySamus(OamBuffer oam)
    {
        // This cinematic deliberately keeps layer1_x_pos at zero. Samus starts at $178,
        // outside the 256-pixel viewport, and the demo makes her enter from the right; the
        // $54 BG1 screen-base selects different art, not a hidden +$100 camera coordinate.
        babyDiscovery!.Samus.TileTransfers.TransferToVram(bus, vram);
        babyDiscovery.Samus.Draw(bus, oam, layer1X: 0, layer1Y: 0, nmiFrameCounter);
    }

    private Rgba32[] RenderScientistCutscene()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        OamBuffer oam = PrepareScientistOam();

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

    private OamBuffer PrepareScientistOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        scientistCutscene!.Draw(bus, oam, characterArtwork?.ScientistSprites);
        oam.FinalizeFrame();
        return oam;
    }

    private void CompositeScientistRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
            vram,
            cgram,
            tilemapBaseWord: scientistCutscene?.TilemapBaseWord ??
                IntroCinematicRomData.Layers.ScientistTilemapWord,
            characterBaseWord: 0,
            horizontalScroll: scientistCutscene?.BackgroundX ?? 0,
            verticalScroll: scientistCutscene?.BackgroundY ?? 0,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    private void CompositeBabyDiscoveryRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
            vram,
            cgram,
            tilemapBaseWord: IntroCinematicRomData.Layers.SceneBg2TilemapWord,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: GameplayFlashbackBg1VerticalScroll,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    private void CompositeMotherBrainRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
            vram,
            cgram,
            tilemapBaseWord: IntroCinematicRomData.Layers.SceneBg1TilemapWord,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: flashbackMotherBrain?.BackgroundVerticalScroll ??
                GameplayFlashbackBg1VerticalScroll,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    private Rgba32[] RenderFirstNarration()
    {
        // Initial SetupPpu_Intro sets TM=$04: only BG3 is visible. BG3SC=$4C and BG34NBA=$04
        // select tilemap word $4C00 and 2-bpp character word $4000 respectively.
        // Opaque BG3 with no priority filter writes all 28 rows, i.e. the whole frame.
        Rgba32[] pixels = frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight];
        SnesBgTilemapRenderer.Render2Bpp(
            pixels,
            vram,
            cgram,
            tilemapBaseWord: IntroCinematicRomData.Layers.NarrationTilemapWord,
            characterBaseWord: IntroCinematicRomData.Layers.FontCharacterBaseWord,
            rowCount: IntroCinematicRomData.Layers.NarrationRowCount);
        return pixels;
    }

    private Rgba32[] RenderFirstIllustratedPage()
    {
        // TM=$16 enables BG2, BG3, and OBJ. BG2 is the 4-bpp Samus portrait at SC=$48;
        // BG3 is the progressively written 2-bpp narration at SC=$4C. Both vertical scroll
        // registers are eight, so source scanline eight is the first visible output line.
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        OamBuffer oam = PrepareIllustratedPageOam();
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

    private OamBuffer PrepareIllustratedPageOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (objects!.SpriteMapPointer != 0)
        {
            // The persistent caret begins at (8,24), moves to (8,$F8) during illustrated
            // crossfades, and is restored when the next narration page starts.
            // The spritemap itself lives in bank $8C and OBSEL=$03 selects word $6000 as
            // the OBJ character base, matching the initial $9A:D200 -> VMADD $6000 DMA.
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro caret sprites require installed character artwork.")).CaretSprites.Draw(
                objects.SpriteMapPointer, oam, objects.CaretX, objects.CaretY,
                IntroCinematicRomData.Objects.ScientistPalette.Raw);
        }
        oam.FinalizeFrame();

        return oam;
    }

    private void CompositePortraitPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
            vram,
            cgram,
            tilemapBaseWord: IntroCinematicRomData.Layers.PortraitTilemapWord,
            characterBaseWord: 0,
            horizontalScroll: 0,
            verticalScroll: 8,
            width: ScreenWidth,
            height: ScreenHeight,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    // Raster scratch reused across draws; never part of saved state (restores reallocate it).
    [NonSerialized] private Rgba32[]? textScratch;
    [NonSerialized] private Rgba32[]? objScratch;

    private void CompositeTextPriority(Span<Rgba32> pixels, bool priority)
    {
        // The renderer skips tiles of the other priority, so clear the reused plane first.
        Rgba32[] fullText = textScratch ??= new Rgba32[ScreenWidth * 32 * 8];
        fullText.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(
            fullText,
            vram,
            cgram,
            tilemapBaseWord: IntroCinematicRomData.Layers.NarrationTilemapWord,
            characterBaseWord: IntroCinematicRomData.Layers.FontCharacterBaseWord,
            rowCount: 32,
            transparentColorZero: true,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, fullText.AsSpan(8 * ScreenWidth, ScreenWidth * ScreenHeight));
    }

    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        Rgba32[] sprites = objScratch ??= new Rgba32[ScreenWidth * ScreenHeight];
        SnesObjRenderer.Render(
            sprites,
            oam,
            vram,
            cgram,
            obsel: 3,
            width: ScreenWidth,
            height: ScreenHeight,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, sprites);
    }

    private void SetupFirstIllustratedPage()
    {
        // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times to
        // $7E:4000, then DMA copies the resulting $600 bytes to VMADD $4180 (byte $8300).
        // English still performs this clear; omitting it exposes stale font glyphs in the
        // otherwise empty lower margin.
        var blankJapaneseCharacters =
            new byte[IntroCinematicRomData.Vram.JapaneseBlankCharactersByteCount];
        for (int offset = 0; offset < blankJapaneseCharacters.Length; offset += japaneseBlankCharacter.Length)
            japaneseBlankCharacter.CopyTo(blankJapaneseCharacters, offset);
        vram.LoadBytes(
            IntroCinematicRomData.Vram.JapaneseBlankCharactersDestinationByte,
            blankJapaneseCharacters);

        // ClearCinematicBgObjects($2F) fills the complete $7E:3000 staging tilemap. The
        // native border then replaces four 32-tile rows at the top and bottom.
        Array.Fill(textTilemap, IntroCinematicRomData.Text.Blank.Raw);
        for (int index = 0;
             index < IntroCinematicRomData.Text.GameplayBlankStartIndex;
             index++)
        {
            textTilemap[index] = IntroCinematicRomData.Text.JapaneseBlank.Raw;
            textTilemap[index + IntroCinematicRomData.Text.JapaneseBlankBottomDelta] =
                IntroCinematicRomData.Text.JapaneseBlank.Raw;
        }

        // $8B:A72B maps Japanese subtitle glyph staging into BG3 rows 24-27.
        (characterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic requires installed artwork."))
            .FinalLine.Words.Span.CopyTo(textTilemap.AsSpan(
                IntroCinematicRomData.Text.FinalLineDestinationStart,
                IntroCinematicRomData.Text.FinalLineWordCount));

        // `menu.menu_tilemap` begins $600 bytes into the same WRAM union. Its byte offset
        // $11E therefore aliases words 911/912 of the staging map; both receive $1C29.
        textTilemap[IntroCinematicRomData.Text.FinalBlankLeftIndex] =
            IntroCinematicRomData.Text.FinalBlank.Raw;
        textTilemap[IntroCinematicRomData.Text.FinalBlankRightIndex] =
            IntroCinematicRomData.Text.FinalBlank.Raw;

        vram.ExecuteWordTransfer(
            textTilemap,
            IntroCinematicRomData.Layers.NarrationTilemapWord,
            1);
        objects = new IntroCinematicObjectSystem(
            bus,
            vram,
            textTilemap,
            audio,
            narrationPresentation,
            characterArtwork?.EyeFrames);
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(
            MusicCommand.LoadData(IntroCinematicRomData.Music.DiscoveryDataIndex));
        audio?.QueueMusicDelayed(
            MusicCommand.SelectTrack(IntroCinematicRomData.Music.SceneTrack),
            MusicCommandDelay.FromDelayedYArgument(
                IntroCinematicRomData.Music.SceneTrackDelayArgument));
        timer = IntroCinematicRomData.Music.DiscoveryInitialTimer;
        Phase = IntroCinematicPhase.WaitForPageOneMusicQueue;
    }

    /// <summary>
    /// Production follows bank $80's real HasQueuedMusic result. Standalone actor tests do
    /// not own an APU queue, so they retain the previously explicit local countdown.
    /// </summary>
    private bool MusicQueueFinished() => audio is null ? --timer <= 0 : !audio.HasQueuedMusic;

    /// <summary>Clears the gameplay rows of BG3 and publishes the same native DMA range.</summary>
    private void ClearVisibleNarrationText()
    {
        Array.Fill(
            textTilemap,
            IntroCinematicRomData.Text.Blank.Raw,
            startIndex: IntroCinematicRomData.Text.GameplayBlankStartIndex,
            count: IntroCinematicRomData.Text.GameplayBlankWordCount);
        vram.ExecuteWordTransfer(
            textTilemap.AsSpan(0, IntroCinematicRomData.Layers.VisibleTextTransferWordCount),
            IntroCinematicRomData.Layers.NarrationTilemapWord,
            1);
    }

    private void ClearPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            paletteFader!.Clear(span.ByteOffset, span.ByteCount);
    }

    private void FadeInPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            paletteFader!.FadeIn(span.ByteOffset, span.ByteCount);
    }

    private void FadeOutPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            paletteFader!.FadeOut(span.ByteOffset, span.ByteCount);
    }

    private void ApplyMasterBrightness(Span<Rgba32> pixels)
    {
        for (int pixel = 0; pixel < pixels.Length; pixel++)
        {
            Rgba32 color = pixels[pixel];
            int brightness = ScreenFade.Displayed(inidisp);
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

    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
    }
}

/// <summary>Ordered phases of the opening narration, flashbacks, laboratory scenes, and Ceres flight.</summary>
public enum IntroCinematicPhase
{
    /// <summary>Waits for the initial opening music commands to finish queuing.</summary>
    WaitForInitialMusicQueue,
    /// <summary>Fades in the first narration card.</summary>
    FadeInFirstNarration,
    /// <summary>Holds the “last Metroid” narration card.</summary>
    LastMetroidIsInCaptivity,
    /// <summary>Holds the “galaxy is at peace” narration card.</summary>
    GalaxyIsAtPeace,
    /// <summary>Waits for the Mother Brain flashback music commands.</summary>
    WaitForSecondMusicQueue,
    /// <summary>Holds the first narration card for its authored four seconds.</summary>
    FourSecondHold,
    /// <summary>Fades out the first narration card.</summary>
    FadeOutFirstNarration,
    /// <summary>Waits for the first illustrated page's music queue.</summary>
    WaitForPageOneMusicQueue,
    /// <summary>Fades in the first illustrated narration page.</summary>
    FadeInPageOne,
    /// <summary>Types and displays the first illustrated page.</summary>
    PageOneText,
    /// <summary>Waits for input after the first illustrated page.</summary>
    PageOneAwaitingInput,
    /// <summary>Crossfades from page one into the Mother Brain gameplay flashback.</summary>
    MotherBrainCrossfade,
    /// <summary>Runs the Mother Brain gameplay flashback.</summary>
    MotherBrainFlashback,
    /// <summary>Crossfades from the Mother Brain flashback to narration page two.</summary>
    PageTwoCrossfade,
    /// <summary>Types and displays narration page two.</summary>
    PageTwoText,
    /// <summary>Waits for input after narration page two.</summary>
    PageTwoAwaitingInput,
    /// <summary>Crossfades from page two into the baby Metroid discovery flashback.</summary>
    BabyDiscoveryCrossfade,
    /// <summary>Runs the SR388 baby Metroid discovery flashback.</summary>
    BabyDiscovery,
    /// <summary>Crossfades from the discovery flashback to narration page three.</summary>
    PageThreeCrossfade,
    /// <summary>Types and displays narration page three.</summary>
    PageThreeText,
    /// <summary>Waits for input after narration page three.</summary>
    PageThreeAwaitingInput,
    /// <summary>Crossfades from page three into the baby Metroid delivery scene.</summary>
    BabyMetroidDeliveryCrossfade,
    /// <summary>Runs the Ceres baby Metroid delivery scene.</summary>
    BabyMetroidDelivery,
    /// <summary>Crossfades from the delivery scene to narration page four.</summary>
    PageFourCrossfade,
    /// <summary>Types and displays narration page four.</summary>
    PageFourText,
    /// <summary>Waits for input after narration page four.</summary>
    PageFourAwaitingInput,
    /// <summary>Crossfades from page four into the baby Metroid examination scene.</summary>
    BabyMetroidExaminationCrossfade,
    /// <summary>Runs the Ceres scientist examination scene.</summary>
    BabyMetroidExamination,
    /// <summary>Crossfades from the examination scene to narration page five.</summary>
    PageFiveCrossfade,
    /// <summary>Types and displays narration page five.</summary>
    PageFiveText,
    /// <summary>Waits for input after narration page five.</summary>
    PageFiveAwaitingInput,
    /// <summary>Types and displays the final narration page.</summary>
    PageSixText,
    /// <summary>Fades out the completed narration sequence.</summary>
    IntroFadeOut,
    /// <summary>Runs the SPACE COLONY Ceres approach sequence.</summary>
    CeresFlight,
    /// <summary>$8B:A395: the setup dispatch that queues the opening music.</summary>
    /// <remarks>Debugger states store phases numerically, so this follows the existing members.</remarks>
    Initial,
    /// <summary>$8B:A66F: the dispatch after the first narration's fade-out that sets up page one.</summary>
    SetupPageOne,
}
