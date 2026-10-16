using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

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
    /// <summary>Reusable final frame; a returned image remains valid until the next cinematic render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;
    /// <summary>Host-owned trail artwork; the flashback projectile pages bind its characters.</summary>
    [field: NonSerialized]     public ProjectileTrailCatalog? TrailArtwork
    {
        get;
        set { field = value; BindProjectileObjects(); }
    }
    /// <summary>The intro OBJ page with the power beam's and both trails' characters, which flashback projectiles draw from.</summary>
    [NonSerialized] private ProjectileObjectPages? projectilePages;
    /// <summary>
    /// Recomposes <see cref="projectilePages"/>: `$8B:A3AC` leaves the power beam's characters and the standard upload
    /// leaves the trail characters over the intro and cinematic OBJ sheets for both gameplay flashbacks.
    /// </summary>
    private void BindProjectileObjects()
    {
        if (characterArtwork is not { } sheets || beamArtwork is not { } beams || TrailArtwork?.Tiles is not { } trails)
        {
            projectilePages = null;
            return;
        }
        var id = new SpriteArtworkId(SpriteArtworkKind.IntroObjects);
        SpriteArtwork page = ObjCharacterPage.With(id, ObjCharacterPage.Create(id, sheets.IntroObjectCharacters.Transfer.Span),
            sheets.CinematicObjectCharacters.Transfer.Span, ProjectileObjectPageDefinitions.IntroCinematicObjectsFirstCharacter);
        projectilePages = new ProjectileObjectPages(page, beams, trails);
    }
    /// <summary>Current timed-projectile composition selected by the host.</summary>
    [field: NonSerialized]
    public ProjectileSpriteCatalog? ProjectileCompositions { get; set; }
    /// <summary>Host-owned frame artwork for the Mother Brain flashback projectile slots.</summary>
    public ProjectileFrameBindingCatalog? ProjectileFrameBindings
    {
        get => flashbackProjectiles.Animation.FrameBindings;
        set => flashbackProjectiles.Animation.FrameBindings = value;
    }

    /// <summary>Installed first-font transfer rebound after debugger restoration.</summary>
    [NonSerialized] private IntroFontAtlas? introFont;
    /// <summary>Installed background, object, and palette content for the intro scenes.</summary>
    [NonSerialized] internal IntroCinematicArtworkCatalog? characterArtwork;
    /// <summary>Installed beam tiles left resident for gameplay flashbacks.</summary>
    [NonSerialized] private BeamTileCatalog? beamArtwork;
    /// <summary>Installed Samus body graphics rebound into active flashback actors.</summary>
    [NonSerialized] private SamusBodyArtworkCatalog? samusBodyArtwork;
    /// <summary>Host-owned hurt-flash colors consumed by the shared Samus palette handler.</summary>
    [NonSerialized] private SamusHurtColorCatalog? samusHurtColors;
    /// <summary>Editable suit-flash colors used by the cinematic's ordinary Samus hurt handler.</summary>
    public void BindSamusHurtColors(SamusHurtColorCatalog? value) => samusHurtColors = value;
    /// <summary>Rebinds installed Samus pixels to active and later intro flashbacks.</summary>
    public void BindSamusBodyArtwork(SamusBodyArtworkCatalog? value)
    {
        samusBodyArtwork = value;
        flashbackSamus?.Presentation.TileTransfers.BindArtwork(value);
        babyDiscovery?.Samus.Presentation.TileTransfers.BindArtwork(value);
    }
    /// <summary>Current host-owned narration content; debugger states retain only playback state.</summary>
    [field: NonSerialized]     public IntroNarrationPresentation? NarrationPresentation
    {
        get;
        set
        {
            field = value;
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
        BindProjectileObjects();
    }
    /// <summary>Rebinds installed BG and OBJ pixels after restoring emulated state.</summary>
    public void BindCharacterArtwork(IntroCinematicArtworkCatalog? value)
    {
        characterArtwork = value;
        BindProjectileObjects();
        ceresFlight?.BindArtwork(value?.CeresFlight);
        if (value is not null)
        {
            // A restored fade keeps its current CGRAM/accumulators, but later scene
            // targets must use the currently selected installed palette.
            var baseline = new SnesCgram();
            value.Palette.LoadTo(baseline);
            new CgramPalettes(baseline).Image.CopyTo(introPalette);
            if (Phase is IntroCinematicPhase.Initial or IntroCinematicPhase.WaitForInitialMusicQueue)
                value.Palette.LoadTo(cgram);
            if (Phase is >= IntroCinematicPhase.WaitForPageOneMusicQueue and
                < IntroCinematicPhase.CeresFlight)
            {
                // The divider occupies only rows 24-27. Preserve live typewriter
                // words and caret state in every other row on debugger rebind.
                value.FinalLine.Words.Span.CopyTo(textTilemap.AsSpan(
                    IntroCinematicRomData.Text.FinalLineDestinationStart,
                    IntroCinematicRomData.Text.FinalLineWordCount));
                new IntroVram(vram).NarrationTilemap.Write(value.FinalLine.Words.Span,
                    IntroCinematicRomData.Text.FinalLineDestinationStart);
            }
            var intro = new IntroVram(vram);
            intro.BackgroundCharacters.Load(value.BackgroundCharacters.Transfer.Span);
            intro.PortraitTilemap.Load(value.PortraitTilemap.Span);
            // The active eye rectangle overlays the base portrait after its VRAM upload.
            objects?.BindEyeArtwork(value.EyeFrames);
            // $8B:A66F replaces the initial BG3 card with live typewriter words
            // when page one begins. Never overwrite that current state on restore.
            if (InitialNarrationCardResident)
                intro.NarrationTilemap.Load(value.InitialNarrationTilemap.Span);
            intro.BackgroundPages.Load(value.BackgroundPages.Span);
            intro.IntroObjectCharacters.Load(value.IntroObjectCharacters.Transfer.Span);
            intro.CinematicObjectCharacters.Load(value.CinematicObjectCharacters.Transfer.Span);
        }
        else
            objects?.BindEyeArtwork(null);
    }
    /// <summary>Full SNES viewport width used by the cinematic's software planes.</summary>
    private const int ScreenWidth = SnesPpuLayout.ScreenWidthPixels;
    /// <summary>Visible SNES viewport height used by the cinematic's software planes.</summary>
    private const int ScreenHeight = SnesPpuLayout.ScreenHeightPixels;

    /// <summary>
    /// BG1VOFS inherited by both gameplay flashbacks from the first illustrated page ($8B:A66F).
    /// Their actor coordinates remain screen-relative and are not shifted with the background.
    /// </summary>
    internal const ushort GameplayFlashbackBg1VerticalScroll = 8;

    /// <summary>Supplies native cinematic, actor, and projectile data.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Owns queued cinematic music and sound effects when installed.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Retains intro graphics and tilemaps across narration and flashback scenes.</summary>
    internal readonly SnesVram vram = new();
    /// <summary>Holds the palette ranges changed by scene crossfades.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Tracks the current physical button edges alongside scripted demo input.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Runs ordinary beam projectiles during gameplay-style flashbacks.</summary>
    internal readonly SamusProjectileSystem flashbackProjectiles = new();
    /// <summary>Runs the shared bomb owner, including projectile cooldown, in flashbacks.</summary>
    private readonly SamusBombProjectileSystem flashbackBombProjectiles = new();
    /// <summary>Staging words for progressively written BG3 narration pages.</summary>
    private readonly ushort[] textTilemap =
        new ushort[IntroCinematicRomData.Layers.TextTilemapWordCount];
    /// <summary>Font glyph repeated to blank the optional Japanese subtitle character area.</summary>
    private byte[] japaneseBlankCharacter = [];
    /// <summary>Target palette restored by transitions back from gameplay flashbacks.</summary>
    private readonly Bgr555[] introPalette = new Bgr555[CgramPalettes.ColorCount];
    /// <summary>Persistent bank-$8C actor and caret walker for illustrated pages.</summary>
    internal IntroCinematicObjectSystem? objects;
    /// <summary>The intro's sprite objects, which exist from the first text page onward.</summary>
    internal IntroCinematicObjectSystem Objects => objects ??
        throw new InvalidOperationException("The intro's sprite objects have not been created; the text pages create them.");
    /// <summary>Applies the selected Japanese subtitle treatment to narration pages.</summary>
    private readonly IntroJapaneseSubtitles subtitles;
    /// <summary>Interpolates selected CGRAM spans during text and scene crossfades.</summary>
    private CinematicPaletteFader? paletteFader;
    /// <summary>The intro's palette fader, which exists while the intro's scenes run.</summary>
    private CinematicPaletteFader PaletteFader => paletteFader ??
        throw new InvalidOperationException("The intro's palette fader has not been created.");
    /// <summary>Samus gameplay state shared by the Mother Brain and SR388 flashbacks.</summary>
    internal SamusState? flashbackSamus;
    /// <summary>Samus in the Mother Brain flashback, which exists while the flashback runs.</summary>
    internal SamusState FlashbackSamus => flashbackSamus ??
        throw new InvalidOperationException("The Mother Brain flashback has not introduced Samus.");
    /// <summary>$1A57 during the Mother Brain flashback: $8B:AF65 sets it, $8B:B842 clears it.</summary>
    private IntroSamusDisplay flashbackSamusDisplay;
    /// <summary>Mother Brain animation state active only in the first gameplay flashback.</summary>
    internal IntroMotherBrainSpriteState? flashbackMotherBrain;
    /// <summary>Mother Brain's sprite in the flashback, which exists while the flashback runs.</summary>
    internal IntroMotherBrainSpriteState FlashbackMotherBrain => flashbackMotherBrain ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Explosion actors shown after the Mother Brain demo input ends.</summary>
    internal IntroMotherBrainExplosionSystem? flashbackMotherBrainExplosions;
    /// <summary>Rinka actors and collision for the Mother Brain scene.</summary>
    internal IntroRinkaSystem? flashbackRinkas;
    /// <summary>Egg and baby actors in the SR388 discovery flashback.</summary>
    internal IntroBabyDiscoveryState? babyDiscovery;
    /// <summary>The Baby Metroid discovery scene, which exists while that scene runs.</summary>
    internal IntroBabyDiscoveryState BabyDiscovery => babyDiscovery ??
        throw new InvalidOperationException("The Baby Metroid discovery scene is not running.");
    /// <summary>Laboratory actors shared by delivery and examination scenes.</summary>
    internal IntroScientistCutsceneState? scientistCutscene;
    /// <summary>The scientist cutscene, which exists while that scene runs.</summary>
    internal IntroScientistCutsceneState ScientistCutscene => scientistCutscene ??
        throw new InvalidOperationException("The scientist cutscene is not running.");
    /// <summary>Final SPACE COLONY flight sequence after the narrated pages.</summary>
    internal IntroCeresFlightState? ceresFlight;
    /// <summary>The flight to Ceres, which exists while that scene runs.</summary>
    internal IntroCeresFlightState CeresFlight => ceresFlight ??
        throw new InvalidOperationException("The Ceres flight is not running.");
    /// <summary>Scripted controller stream for the Mother Brain gameplay demonstration.</summary>
    internal DemoInputState? flashbackDemoInput;
    /// <summary>The flashback's scripted controller input, which exists while the flashback runs.</summary>
    internal DemoInputState FlashbackDemoInput => flashbackDemoInput ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Collision data for the gameplay-style flashback room.</summary>
    internal RoomLevelData? flashbackLevel;
    /// <summary>The flashback room's collision, which exists while the flashback runs.</summary>
    internal RoomLevelData FlashbackLevel => flashbackLevel ??
        throw new InvalidOperationException("The Mother Brain flashback room is not loaded.");
    /// <summary>Cinematic-function countdown for the active palette transition.</summary>
    private NativeCountdown crossfadeCounter;
    /// <summary>Alternate transition countdown retained between scientist and text phases.</summary>
    private NativeCountdown introCrossfadeCounter;
    /// <summary>Frame counter passed to shared Samus systems during flashback updates.</summary>
    private ushort nmiFrameCounter;
    /// <summary>$51: the INIDISP byte the cinematic fades; the intro starts in forced blank.</summary>
    internal int inidisp = ScreenFade.ForcedBlank;
    /// <summary>Controls master INIDISP fades at cinematic entry and exit.</summary>
    private readonly ScreenFade fade = new();
    /// <summary>Fallback music-queue delay when no cartridge audio state is installed.</summary>
    private int timer = 8;

    /// <summary>Creates the opening cinematic and performs its initial native graphics and music setup.</summary>
    /// <param name="bus">The cartridge address space used by cinematic actors and projectiles.</param>
    /// <param name="audio">Optional cartridge audio state that owns queued music and sound effects.</param>
    /// <param name="introFont">The installed opening-cinematic font atlas.</param>
    /// <param name="characterArtwork">The installed opening-cinematic BG and OBJ artwork.</param>
    /// <param name="beamArtwork">Optional installed beam graphics used by gameplay flashbacks.</param>
    /// <param name="samusBodyArtwork">Optional installed Samus body artwork used by gameplay flashbacks.</param>
    /// <param name="japaneseText">The options menu's Japanese text setting (native <c>AltText</c>).</param>
    public IntroCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        IntroFontAtlas? introFont = null,
        IntroCinematicArtworkCatalog? characterArtwork = null,
        BeamTileCatalog? beamArtwork = null,
        SamusBodyArtworkCatalog? samusBodyArtwork = null,
        bool japaneseText = false)
    {
        Ensure.NotNull(bus);
        this.bus = bus;
        subtitles = new IntroJapaneseSubtitles(japaneseText);
        this.audio = audio;
        this.introFont = introFont ?? throw new InvalidOperationException(
            "Opening cinematic requires the installed font atlas.");
        this.characterArtwork = characterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic requires installed artwork.");
        this.beamArtwork = beamArtwork;
        BindProjectileObjects();
        this.samusBodyArtwork = samusBodyArtwork;
        characterArtwork.Palette.LoadTo(cgram);
        new CgramPalettes(cgram).Image.CopyTo(introPalette);

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
        var intro = new IntroVram(vram);
        intro.BackgroundCharacters.Load(bgCharacters.AsSpan(0, IntroCinematicRomData.Vram.BackgroundCharacterBytes));
        ApplyIntroFont(fontOne);
        intro.PortraitTilemap.Load(samusHeadTilemap.AsSpan(0, IntroCinematicRomData.Vram.SamusHeadTilemapBytes));
        intro.NarrationTilemap.Load(firstNarrationTilemap.AsSpan(0, IntroCinematicRomData.Vram.NarrationTilemapBytes));
        intro.BackgroundPages.Load(bg1Pages.AsSpan(0, IntroCinematicRomData.Vram.BackgroundPageTilemapBytes));
        intro.IntroObjectCharacters.Load(characterArtwork.IntroObjectCharacters.Transfer.ToArray());
        intro.CinematicObjectCharacters.Load(introObjects.AsSpan(0, IntroCinematicRomData.Vram.ObjectCharacterBytes));

        // $8B:A3AC performs the ordinary beam palette load before copying the full intro palette, which
        // replaces it; the flashback projectile pages bind the beam's characters.
        SamusProjectileSystem.LoadBeamPalette(bus, cgram, equippedBeams: 0, beamArtwork?.Palettes);
        characterArtwork.Palette.LoadTo(cgram);

        // The PPU loads above are invisible: the intro stays in forced blank until its fade.
        // $8B:A395 itself runs as the next dispatch's cinematic function.
        Phase = IntroCinematicPhase.Initial;
    }

    /// <summary>Validates the installed font, captures its blank Japanese glyph, and loads the first VRAM font bank.</summary>
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
        new IntroVram(vram).FontOne.Load(transfer);
    }

    /// <summary>
    /// True until $8B:A66F replaces the initial BG3 card. The setup phases appended to the
    /// enum precede it in time even though they follow it numerically.
    /// </summary>
    private bool InitialNarrationCardResident =>
        Phase is IntroCinematicPhase.Initial or IntroCinematicPhase.SetupPageOne or
        < IntroCinematicPhase.WaitForPageOneMusicQueue;

    /// <summary>Gets the current opening-cinematic state-machine phase.</summary>
    public IntroCinematicPhase Phase { get; private set; }

    /// <summary>True after the final narration fade hands control to the Ceres flight.</summary>
    public bool NarrationFinished { get; private set; }

    /// <summary>True when the SPACE COLONY caption and its final fade have completed.</summary>
    public bool CeresFlightFinished => ceresFlight?.Finished ?? false;

    /// <summary>Advances the opening cinematic by one gameplay update.</summary>
    /// <param name="controllerInput">The current raw controller-held word.</param>
    public void Step(SnesButton controllerInput)
    {
        flashbackProjectiles.ImpactAudio.BeginImpactAudioFrame(cinematicActive: true);
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
                    Objects.StartEnglishPageOne();
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
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != 0)
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
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != 0)
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
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != 0)
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
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != 0)
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
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != 0)
                    SetupPageSix();
                break;

            case IntroCinematicPhase.StartPageSix:
                StartPageSix();
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
                        characterArtwork?.CeresFlight, subtitles.Enabled);
                    Phase = IntroCinematicPhase.CeresFlight;
                }
                break;

            case IntroCinematicPhase.CeresFlight:
                CeresFlight.Step();
                break;
        }

        if (objects is not null)
        {
            objects.Step(controller.NewlyPressed);
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
            flashbackSamus.PoseMetadata.RefreshCollisionRadii(bus);

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
                crossfadeCounter.RawValue);
            // $8B:B842: deleting Mother Brain after the page-two crossfade hides Samus from
            // the next dispatch on.
            if (!flashbackMotherBrain.IsVisible)
                flashbackSamusDisplay = IntroSamusDisplay.Hidden;
            if (flashbackProjectiles.EnemyImpacts.TryImpactIntroMotherBrainMissile(bus, flashbackBombProjectiles))
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
        flashbackMotherBrain?.Step();
        if (explosionsExistedBeforeThisFrame)
        {
            // The eight actors are allocated while object zero is already being processed,
            // so their first generic-handler call occurs on the following frame. Hit count
            // four is reached on the spawn frame, after the handler passed their free slots.
            flashbackMotherBrainExplosions?.Step(crossfadeCounter.RawValue);
        }

        // Handle_CinematicSpriteObjects ($8B:93EF) runs after the cinematic function, so the
        // actors a scene setup spawns take their first step in that same dispatch.
        babyDiscovery?.Step(nmiFrameCounter, crossfadeCounter.RawValue);
        scientistCutscene?.Step(crossfadeCounter.RawValue, introCrossfadeCounter.RawValue);
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

    /// <summary>Installs Mother Brain room collision, actors, Samus demo state, and the incoming gameplay palette.</summary>
    internal void SetupMotherBrainFlashback()
    {
        // Shared native setup $8B:B018 moves the persistent narration caret to Y=$F8.
        // It remains alive and will be restored by the next text-page setup.
        Objects.PlaceCaretOffScreen();

        // $8B:AEB8 changes BG1SC to $50. The corresponding 32x32 visual map was already
        // uploaded by the initial $96:FF14 decompression; $8C:BEC3 below is gameplay
        // collision data and must never be expanded as a visual block map.
        flashbackSamus = new SamusState
        {
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(155, 115)),
            SelectedHudItem = 1,
            Missiles = 900,
            MaxMissiles = 900,
        };
        // Shared Samus handlers read the nonzero cinematic-function word to select
        // intro palette restoration and suppress gameplay-only sound/landing effects.
        flashbackSamus.LiquidPhysics.CinematicFunctionActive = true;
        flashbackSamus.Presentation.TileTransfers.BindArtwork(samusBodyArtwork);
        flashbackSamus.PoseMetadata.RefreshCollisionRadii(bus);
        flashbackSamus.Presentation.InitializeAnimation(bus);
        flashbackSamus.CommitPoseHistory(bus);
        flashbackSamus.Presentation.PrimeGraphics(bus);
        // The native NMI consumes the freshly selected top/bottom definitions before the
        // next displayed OAM image. Apply that dedicated DMA now so the first host-rendered
        // flashback frame names initialized OBJ characters rather than stale intro art.
        flashbackSamus.Presentation.TileTransfers.TransferToVram(bus, vram);
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
        flashbackDemoInput.Load(IntroDemoInputObjectDefinitions.IntroMotherBrainFlashback, initializationParameter: 0);
        // $8B:AF62: Samus is processed, and drawn ahead of the cinematic objects.
        flashbackSamusDisplay = IntroSamusDisplay.SamusFirst;

        // $8B:B018 replaces the target palette with kPalettes_Intro, decomposes every
        // component, clears only the incoming gameplay ranges, and immediately composes.
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.Narration);
        paletteFader.ComposeInto(cgram);

        // The sprite setup stored 127 in both cinematic_var13 and (through sprite object
        // $CE55's setup) cinematic_var4. $B250 tests the old counter, then decrements it.
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.MotherBrainCrossfade;
    }

    /// <summary>
    /// Exact 448-byte $8C:BEC3 room-collision image copied by the Mother Brain setup.
    /// Null until page one has accepted its input edge.
    /// </summary>
    public byte[]? MotherBrainLevelData { get; private set; }

    /// <summary>Advances 32 component updates of the text-to-gameplay fade before exposing the Mother Brain room.</summary>
    private void StepMotherBrainCrossfade()
    {
        // $8B:B250 updates only when the pre-decrement counter is divisible by four.
        // Values 124..0 therefore yield exactly 32 component updates.
        if ((crossfadeCounter.RawValue & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Narration);
            PaletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = crossfadeCounter.Decrement().Remaining;
        if (!crossfadeCounter.IsNegative)
            return;

        // Transition completion sets TM=$15 and clears English words 128..767 to tile
        // $002F. The top/bottom ornamental rows survive because the loop starts at $80.
        ClearVisibleNarrationText();
        Phase = IntroCinematicPhase.MotherBrainFlashback;
    }

    /// <summary>Starts the second narration page while restoring its caret and fading back from gameplay colors.</summary>
    private void SetupPageTwoCrossfade()
    {
        // $8B:B35F spawns page-two's bank-$8C BG object, selects the reverse gameplay-to-
        // text palette routine, and restores the already allocated caret object.
        Objects.StartEnglishPageTwo();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.PageTwoCrossfade;
    }

    /// <summary>Completes the Mother Brain-to-page-two palette transition.</summary>
    private void StepPageTwoCrossfade()
    {
        // $8B:B3F4 is the exact inverse of the earlier transition: text/portrait ranges
        // fade in while Mother Brain gameplay, projectile, and OBJ ranges fade out.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageTwoText);
    }

    /// <summary>Fades gameplay and projectile ranges out while restoring portrait and text ranges.</summary>
    private void StepReverseGameplayToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((crossfadeCounter.RawValue & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeInPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Narration);
            PaletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = crossfadeCounter.Decrement().Remaining;
        if (crossfadeCounter.IsNegative)
            Phase = completedPhase;
    }

    /// <summary>Replaces Mother Brain actors with SR388 egg and baby owners and selects the second gameplay room page.</summary>
    internal void SetupBabyDiscoveryCrossfade()
    {
        Objects.PlaceCaretOffScreen();

        // $8B:AF7B selects BG1SC=$54 (the second cartridge-authored room page), declares a
        // 32x16 collision room, carries Samus into new egg/baby/demo owners, and reuses the same
        // text-to-gameplay palette crossfade as the Mother Brain scene.
        babyDiscovery = new IntroBabyDiscoveryState(bus, audio, flashbackSamus
            ?? throw new InvalidOperationException("Discovery setup requires the preceding flashback Samus state."));
        babyDiscovery.Samus.Presentation.TileTransfers.TransferToVram(bus, vram);

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
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    /// <summary>Reuses the text-to-gameplay fade before showing the SR388 discovery room alone.</summary>
    internal void StepBabyDiscoveryCrossfade()
    {
        if ((crossfadeCounter.RawValue & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Narration);
            PaletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = crossfadeCounter.Decrement().Remaining;
        if (!crossfadeCounter.IsNegative)
            return;

        // $8B:B29F leaves TM=$15 and clears the English text region after the final fade
        // update. BG1SC already points at $54, so the SR388 room becomes the sole BG1 page.
        ClearVisibleNarrationText();
        Phase = IntroCinematicPhase.BabyDiscovery;
    }

    /// <summary>Starts the third narration page while SR388 actors continue updating beneath its text fade.</summary>
    private void SetupPageThreeCrossfade()
    {
        // Egg opcode $B33E selects page three. On the next cinematic-function call $B370
        // spawns its bank-$8C text object and enters the same gameplay-to-text palette
        // routine used after Mother Brain. The SR388 actors continue running beneath it.
        Objects.StartEnglishPageThree();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.PageThreeCrossfade;
    }

    /// <summary>Runs the shared gameplay-to-text transition for the SR388 discovery scene.</summary>
    private void StepPageThreeCrossfade()
    {
        // $B3F4 is shared byte-for-byte by pages two and three. The named outer phase keeps
        // rendering the SR388 actors until its final decrement switches TM back to text.
        StepReverseGameplayToTextCrossfade(IntroCinematicPhase.PageThreeText);
    }

    /// <summary>Starts the first laboratory scene with its rightward BG1 pan and separate scientist fade timer.</summary>
    internal void SetupBabyMetroidDelivery()
    {
        // Scientist crossfades use the sibling native setup at $8B:B151, which performs
        // the same off-screen caret move before changing the visible layer configuration.
        Objects.PlaceCaretOffScreen();

        // $B0F2 selects BG1SC=$58, starts it 32 pixels right with vertical scroll eight,
        // seeds IntroCrossFadeTimer, and spawns definition $CE61 before configuring the
        // scientist palette transition. Its tilemap is already in the $96:FF14 payload.
        scientistCutscene = IntroScientistCutsceneState.CreateDelivery();
        babyDiscovery = null;

        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.Discovery);
        paletteFader.ComposeInto(cgram);

        // The native intro alternates two adjacent WRAM counters. Page-three setup left
        // CinematicFunctionTimer at $007F; this setup writes the separate intro counter.
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        introCrossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.BabyMetroidDeliveryCrossfade;
    }

    /// <summary>Fades page-three text and portrait ranges into the delivery laboratory palette.</summary>
    internal void StepBabyMetroidDeliveryCrossfade()
    {
        // $B2D2 fades the portrait/text ranges away while bringing in laboratory BG/OBJ
        // palettes. Only CinematicFunctionTimer is decremented on this direction.
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidDelivery);
    }

    /// <summary>Advances the text-to-laboratory palette spans using the cinematic-function countdown.</summary>
    private void StepTextToScientistCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((crossfadeCounter.RawValue & IntroCinematicRomData.Palette.StepEveryFourFramesMask) == 0)
        {
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicRomData.Palette.Discovery);
            PaletteFader.ComposeInto(cgram);
        }

        crossfadeCounter = crossfadeCounter.Decrement().Remaining;
        if (!crossfadeCounter.IsNegative)
            return;

        ClearVisibleNarrationText();
        Phase = completedPhase;
    }

    /// <summary>Starts page four and the alternate-timer fade back from the delivery laboratory.</summary>
    private void SetupPageFourCrossfade()
    {
        // Actor opcode $B346 selects page four. $B381 starts its text stream and seeds the
        // cinematic-function counter, while the reverse routine deliberately consumes the
        // still-$007F IntroCrossFadeTimer set by the delivery scene.
        Objects.StartEnglishPageFour();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.PageFourCrossfade;
    }

    /// <summary>Restores text and portrait colors while fading the delivery laboratory out.</summary>
    private void StepPageFourCrossfade()
    {
        // $B458 is the scientist-specific inverse: text/portrait colors fade in as the
        // laboratory BG and its nine OBJ colors fade out. It consumes the alternate counter.
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFourText);
    }

    /// <summary>Advances the laboratory-to-text palette ranges using the retained intro countdown.</summary>
    private void StepScientistToTextCrossfade(IntroCinematicPhase completedPhase)
    {
        if ((introCrossfadeCounter.RawValue & 3) == 0)
        {
            FadeInPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicRomData.Palette.Discovery);
            PaletteFader.ComposeInto(cgram);
        }

        introCrossfadeCounter = introCrossfadeCounter.Decrement().Remaining;
        if (introCrossfadeCounter.IsNegative)
            Phase = completedPhase;
    }

    /// <summary>Starts the second laboratory scene on its vertically offset BG1 page.</summary>
    private void SetupBabyMetroidExamination()
    {
        Objects.PlaceCaretOffScreen();

        // $B123 selects BG1SC=$5C, starts the page at Y=-24, and spawns definition $CE67.
        // Its setup is otherwise the same two-counter scientist crossfade as delivery.
        scientistCutscene = IntroScientistCutsceneState.CreateExamination();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.Discovery);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        introCrossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    /// <summary>Reuses the text-to-scientist fade for the examination scene.</summary>
    private void StepBabyMetroidExaminationCrossfade() =>
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidExamination);

    /// <summary>Starts page five after the baby examination scene and selects the reverse palette transition.</summary>
    private void SetupPageFiveCrossfade()
    {
        Objects.StartEnglishPageFive();
        paletteFader = new CinematicPaletteFader(introPalette);
        ClearPaletteSpans(IntroCinematicRomData.Palette.GameplayClear);
        paletteFader.ComposeInto(cgram);
        crossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.PageFiveCrossfade;
    }

    /// <summary>Reuses the scientist-to-text fade for the fifth narration page.</summary>
    private void StepPageFiveCrossfade() =>
        StepScientistToTextCrossfade(IntroCinematicPhase.PageFiveText);

    /// <summary>Seeds the final narration page's timer, preserving the English/Japanese dispatch difference.</summary>
    private void SetupPageSix()
    {
        // $8B:B1EB seeds IntroCrossFadeTimer. English takes B1F4's direct fall-through into
        // B207; Japanese text selects B207 as the next dispatch's cinematic function.
        introCrossfadeCounter = NativeCountdown.Start(IntroCinematicRomData.Palette.CrossfadeInitialCounter);
        if (subtitles.Enabled)
            Phase = IntroCinematicPhase.StartPageSix;
        else
            StartPageSix();
    }

    /// <summary>Replaces the text stream, caret, and eye animation without a palette transition.</summary>
    private void StartPageSix()
    {
        // $8B:B207 has no palette transition. Only the English text region, caret, eye
        // stream, and final page object are replaced.
        Objects.StartEnglishPageSix();
        scientistCutscene = null;
        Phase = IntroCinematicPhase.PageSixText;
    }

    /// <summary>Runs the scripted Samus gameplay, projectiles, collisions, and palette updates for the Mother Brain flashback.</summary>
    internal void StepMotherBrainDemo()
    {
        FlashbackDemoInput.Step(DemoInputState.RunReturnOnlyPreInstruction, HandleMotherBrainDemoCue);

        // HandleHUDSpecificBehaviorAndProjectiles calls the shared cooldown owner before
        // its ordinary-projectile half. Standing pose two cannot place bombs, but running
        // the real owner is still required for missile admission timing.
        flashbackBombProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = FlashbackLevel }, FlashbackSamus, FlashbackDemoInput.Held, FlashbackDemoInput.NewlyPressed);
        flashbackProjectiles.StepFrame(new SamusTerrain { Bus = bus, Level = FlashbackLevel }, FlashbackSamus, new ProjectileButtonInput { ControllerInput = FlashbackDemoInput.Held, ControllerNewInput = FlashbackDemoInput.NewlyPressed, ControllerPreviousNewInput = FlashbackDemoInput.PublishedPreviousNewlyPressed }, layer1: Position16<RoomPixels>.Zero, sharedProjectiles: flashbackBombProjectiles);
        if (flashbackProjectiles.LastFrameResult.QueuedSoundEffect is { } soundEffect)
        {
            audio?.QueueSound(
                soundEffect,
                flashbackProjectiles.LastFrameResult.QueuedSoundMaximum);
        }

        // $90:E70D/$90:E71C: both the demo and locked handlers end with
        // ResetMovementAndPoseChangeVariables, so a later missile inherits only the
        // movement recorded in its own dispatch.
        FlashbackSamus.PoseTransitions.ClearPoseTransitionShotDirection();
        SamusProjectileInheritance.ClearMovement(bus);
    }

    /// <summary>
    /// Runs the movement/animation/interruption/palette slice of the native intro-demo
    /// handler at <c>$90:E833-$90:E84D</c>, followed by the timer owner at <c>$8B:8E1A</c>.
    /// </summary>
    private void StepMotherBrainFlashbackSamus()
    {
        SamusState samus = FlashbackSamus;
        RoomLevelData level = FlashbackLevel;
        SnesButton demoInput = flashbackDemoInput?.Held ?? 0;

        // Accepted NMI transfers the definitions selected by the previous Samus draw. This
        // is what makes pose/frame changes use their corresponding ROM graphics rather than
        // interpreting stale standing tiles as a hurt body on the next visible frame.
        samus.Presentation.TileTransfers.TransferToVram(bus, vram);

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
        else if (samus.Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose)
        {
            hurtEndingProbe = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
                bus, level, samus, nmiFrameCounter);
        }
        else if (flashbackDemoInput?.Enabled == true &&
            (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose)))
        {
            // The demo's later run/jump records use the same alpha/beta order as the
            // discovery scene. This coordinator already animates and commits its pose.
            IntroSamusDemoMovement.StepGroundedLeft(bus, level, samus, demoInput,
                flashbackDemoInput.NewlyPressed, nmiFrameCounter);
            groundedDemoStepped = true;
        }
        else if (samus.Pose == SamusPoseId.SpinJumpLeftPose)
        {
            fallingMovement = SamusAerialMovement.StepSpinJump(bus, level, samus,
                demoInput, nmiFrameCounter, flashbackDemoInput?.NewlyPressed ?? 0);
        }
        else if (SamusPoseMetadata.IsLeftFacingNormalJumpPose(samus.Pose))
        {
            fallingMovement = SamusAerialMovement.StepNormalJump(bus, level, samus,
                demoInput, nmiFrameCounter);
        }
        else if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose)
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
            samus.Presentation.AnimateNoFx(bus, new SamusAnimationStep { ControllerInput = demoInput, NmiFrameCounter = nmiFrameCounter });

        // Delay opcodes `$F8/$FD` publish a prospective pose; they are not ordinary frame
        // delays. `$90:E849` consumes that publication before the byte following the opcode
        // can ever be misread as another animation duration. This completes landing art's
        // native A5 -> 02 transition instead of walking beyond its ROM delay list.
        bool animationTransitionApplied = samus.PoseTransitions.ApplyPendingVerifiedAnimationTransition(bus);
        animationTransitionApplied |= SamusKnockbackMovement.TryFinishExpiredHitInterruption(bus, samus);
        transitionAccepted |= animationTransitionApplied;

        if (!animationTransitionApplied && hurtEndingProbe is { IsUnobstructedDownwardMovement: true })
        {
            samus.PoseTransitions.ApplyWalkedOffFloorTransition(bus, level, samus.PoseTransitions.SelectFallingPoseForCurrentAim(bus), nmiFrameCounter);
            animationTransitionApplied = transitionAccepted = true;
        }

        // The normal new-state handler resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation list all come from the same bank-$91 implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true })
        {
            samus.PoseTransitions.ApplyAerialLanding(bus, wasSpinning: samus.Pose == SamusPoseId.SpinJumpLeftPose, demoInput);
            transitionAccepted = true;
        }

        // `$90:E842` consumes the timer published by Rinka zero on the preceding cinematic-
        // sprite pass. Keeping this after animation matches SamusNewStateHandler_IntroDemo:
        // the hit frame finishes its standing animation before command one installs $53/$54.
        if (!samus.KnockbackActive &&
            !samus.KnockbackTimer.IsZero &&
            samus.KnockbackDirection == 0)
        {
            SamusKnockbackMovement.Start(bus, samus, demoInput, new KnockbackLaunch { KnockbackXDirection = samus.KnockbackXDirection, KnockbackTimer = samus.KnockbackTimer.RawValue });
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

    /// <summary>Runs the Mother Brain demo's end routine, freezing input after its prescribed Samus pose change.</summary>
    private void HandleMotherBrainDemoCue(DemoInputCue cue)
    {
        if (cue != DemoInputCue.EndIntroMotherBrainFlashback)
            throw new InvalidOperationException($"The Mother Brain flashback demo calls no {cue} routine.");

        // $91:8739 locks Samus in pose two, reinitializes the pose/animation bookkeeping,
        // disables demo publication, and then returns to the bytecode interpreter. The next
        // physical word is shared delete opcode $8427, so the object is removed in the same
        // handler call. State-handler function pointers are not independently dispatched by
        // this scoped frontend yet; freezing input after the exact pose change has the same
        // observable contract for the remaining Mother Brain explosion frames.
        FlashbackSamus.Pose = SamusPoseId.FacingLeftNormalPose;
        FlashbackSamus.PoseMetadata.RefreshCollisionRadii(bus);
        // $91:874B: a Samus already in pose two keeps her running animation.
        FlashbackSamus.Presentation.SetAnimationFrameIfPoseChanged(bus);
        FlashbackSamus.CommitPoseHistory(bus);
        FlashbackDemoInput.Disable();
    }

    /// <summary>Builds the flashback's 224-word collision room with native-cleared BTS storage.</summary>
    private static RoomLevelData CreateMotherBrainLevel(ReadOnlySpan<byte> source)
    {
        const int width = 16;
        const int height = 16;
        var foreground = new ushort[width * height];
        for (int byteOffset = 0; byteOffset < source.Length; byteOffset += 2)
        {
            foreground[byteOffset / 2] = unchecked((ushort)(
                SnesLittleEndian.Word(source[byteOffset], source[byteOffset + 1])));
        }

        // $8B:AF30 clears the complete 512-byte word-addressable BTS allocation. BG2 and
        // block-definition data are irrelevant to bank-$94 collision in this visual room;
        // their arrays exist only to satisfy the shared RoomLevelData ownership contract.
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height), new RoomLevelPlanes(foreground, new byte[width * height], new ushort[width * height]), Array.Empty<byte>());
    }

    /// <summary>Composes the Mother Brain room, cinematic actors, Samus, and projectiles in SNES priority order.</summary>
    internal Rgba32[] RenderMotherBrainFlashback()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        SpriteFrameBuffer oam = PrepareMotherBrainOam();

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

    /// <summary>
    /// Prepares Mother Brain OAM once per display; trail drawing advances state, so a captured frame cannot be redrawn through this path.
    /// </summary>
    internal SpriteFrameBuffer PrepareMotherBrainOam()
    {
        // Both direct rendering and detached capture enter here once per display.
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        OamBuffer oam = sprites.Oam;
        // $8B:8E2D: a negative $1A57 draws Samus and both projectile passes before the
        // cinematic objects; once $B842 clears it, only the objects remain.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            FlashbackSamus.Presentation.Draw(bus, sprites, layer1: Position16<RoomPixels>.Zero, nmiFrameCounter);
            // The flashback's VRAM holds the power beam's characters whatever beam its Samus fires.
            var projectileArtwork = new ProjectileDrawArtwork(ProjectileCompositions, projectilePages, EquippedBeams: 0);
            flashbackProjectiles.DrawLiveProjectiles(bus, sprites, Position16<RoomPixels>.Zero, nmiFrameCounter, projectileArtwork);
            flashbackProjectiles.AdvanceTrails(bus, timeIsFrozen: false, TrailArtwork);
            flashbackProjectiles.DrawTrails(sprites, Position16<RoomPixels>.Zero, TrailArtwork, projectileArtwork);
            flashbackProjectiles.DrawExplosions(bus, sprites, Position16<RoomPixels>.Zero, projectileArtwork);
        }
        flashbackRinkas?.Draw(oam, characterArtwork?.RinkaSprites);
        flashbackMotherBrainExplosions?.Draw(oam,
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro Mother Brain explosions require installed character artwork.")).MotherBrainExplosionSprites);
        if (FlashbackMotherBrain.IsVisible && FlashbackMotherBrain.SpriteMapPointer != 0)
        {
            // cinematic_var15=$FFFF makes DrawIntroSprites draw Samus first and cinematic
            // actors afterward. Retaining that OAM insertion order preserves overlap wins.
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro Mother Brain sprites require installed character artwork.")).MotherBrainSprites.Draw(
                FlashbackMotherBrain.SpriteMapPointer, oam,
                IntroMotherBrainSpriteState.Position.ToOam(),
                IntroMotherBrainSpriteState.PaletteBits);
        }
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Composes the SR388 room with its egg, baby, Samus, and projectile layers.</summary>
    internal Rgba32[] RenderBabyDiscovery()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        SpriteFrameBuffer oam = PrepareBabyDiscoveryOam();

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

    /// <summary>Orders Samus and discovery actors by the cinematic display flag before finalizing OBJ data.</summary>
    private SpriteFrameBuffer PrepareBabyDiscoveryOam()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        OamBuffer oam = sprites.Oam;
        // $8B:8E2D orders Samus against the cinematic objects by $1A57's sign.
        IntroSamusDisplay display = BabyDiscovery.SamusDisplay;
        if (display == IntroSamusDisplay.SamusFirst)
            DrawBabyDiscoverySamus(sprites);
        BabyDiscovery.DrawActors(oam, characterArtwork?.EggEffectSprites,
            characterArtwork?.DiscoveryActorSprites);
        if (display == IntroSamusDisplay.ObjectsFirst)
            DrawBabyDiscoverySamus(sprites);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Draws Samus in screen-relative coordinates as the demo brings her in from the right.</summary>
    private void DrawBabyDiscoverySamus(SpriteFrameBuffer sprites)
    {
        // This cinematic deliberately keeps layer1_x_pos at zero. Samus starts at $178,
        // outside the 256-pixel viewport, and the demo makes her enter from the right; the
        // $54 BG1 screen-base selects different art, not a hidden +$100 camera coordinate.
        BabyDiscovery.Samus.Presentation.TileTransfers.TransferToVram(bus, vram);
        BabyDiscovery.Samus.Presentation.Draw(bus, sprites, layer1: Position16<RoomPixels>.Zero, nmiFrameCounter);
    }

    /// <summary>Composes the delivery or examination laboratory with its scrolling BG1 and actor priority.</summary>
    internal Rgba32[] RenderScientistCutscene()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        SpriteFrameBuffer oam = PrepareScientistOam();

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

    /// <summary>Collects scientist-scene objects for the current cinematic display.</summary>
    private SpriteFrameBuffer PrepareScientistOam()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        OamBuffer oam = sprites.Oam;
        ScientistCutscene.Draw(oam, characterArtwork?.ScientistSprites);
        sprites.FinalizeFrame();
        return sprites;
    }

    /// <summary>Overlays only the requested laboratory BG1 priority tiles, leaving transparent pixels untouched.</summary>
    private void CompositeScientistRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = scientistCutscene?.TilemapBaseWord ??
                IntroCinematicRomData.Layers.ScientistTilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = scientistCutscene?.Background.AsBgPlaneScroll() ?? default, Size = new Extent16<ScreenPixels>(ScreenWidth, ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
    }

    /// <summary>Overlays only the requested SR388 BG1 priority tiles, leaving transparent pixels untouched.</summary>
    private void CompositeBabyDiscoveryRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.SceneBg2TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, GameplayFlashbackBg1VerticalScroll), Size = new Extent16<ScreenPixels>(ScreenWidth, ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
    }

    /// <summary>Overlays only the requested Mother Brain BG1 priority tiles, leaving transparent pixels untouched.</summary>
    private void CompositeMotherBrainRoomPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.SceneBg1TilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, flashbackMotherBrain?.BackgroundVerticalScroll ??
                GameplayFlashbackBg1VerticalScroll), Size = new Extent16<ScreenPixels>(ScreenWidth, ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
    }

    /// <summary>Draws the full-screen BG3 opening card while TM enables no other planes.</summary>
    private Rgba32[] RenderFirstNarration()
    {
        // Initial SetupPpu_Intro sets TM=$04: only BG3 is visible. BG3SC=$4C and BG34NBA=$04
        // select tilemap word $4C00 and 2-bpp character word $4000 respectively.
        // Opaque BG3 with no priority filter writes all 28 rows, i.e. the whole frame.
        Rgba32[] pixels = frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight];
        SnesBgTilemapRenderer.Render2Bpp(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.NarrationTilemapWord, CharacterBaseWord = IntroCinematicRomData.Layers.FontCharacterBaseWord }, rowCount: IntroCinematicRomData.Layers.NarrationRowCount);
        return pixels;
    }

    /// <summary>Composes portrait BG2, progressive narration BG3, and OBJ with their eight-pixel source scroll.</summary>
    private Rgba32[] RenderFirstIllustratedPage()
    {
        // TM=$16 enables BG2, BG3, and OBJ. BG2 is the 4-bpp Samus portrait at SC=$48;
        // BG3 is the progressively written 2-bpp narration at SC=$4C. Both vertical scroll
        // registers are eight, so source scanline eight is the first visible output line.
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, ScreenWidth * ScreenHeight, frameBuffer ??= new Rgba32[ScreenWidth * ScreenHeight]);
        SpriteFrameBuffer oam = PrepareIllustratedPageOam();
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

    /// <summary>Collects the persistent caret and illustrated-page cinematic objects in display order.</summary>
    internal SpriteFrameBuffer PrepareIllustratedPageOam()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        OamBuffer oam = sprites.Oam;
        if (Objects.SpriteMapPointer != 0)
        {
            // The persistent caret begins at (8,24), moves to (8,$F8) during illustrated
            // crossfades, and is restored when the next narration page starts.
            // The spritemap itself lives in bank $8C and OBSEL=$03 selects word $6000 as
            // the OBJ character base, matching the initial $9A:D200 -> VMADD $6000 DMA.
            (characterArtwork ?? throw new InvalidOperationException(
                "Intro caret sprites require installed character artwork.")).CaretSprites.Draw(
                Objects.SpriteMapPointer, oam, Objects.Caret.ToOam(),
                IntroCinematicRomData.Objects.ScientistPalette.Raw);
        }
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Overlays portrait BG2 tiles of one priority without erasing earlier layers.</summary>
    private void CompositePortraitPriority(Span<Rgba32> pixels, bool priority)
    {
        // Composited directly; transparent pixels leave the frame untouched, as a plane would.
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.PortraitTilemapWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 8), Size = new Extent16<ScreenPixels>(ScreenWidth, ScreenHeight), TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
    }

    /// <summary>Reusable raster plane omitted from saved state and reallocated after restoration.</summary>
    [NonSerialized] private Rgba32[]? textScratch;
    /// <summary>Reusable OBJ raster plane omitted from saved state and rebuilt after restoration.</summary>
    [NonSerialized] private Rgba32[]? objScratch;

    /// <summary>Clears reused raster scratch before overlaying narration BG3 tiles of one priority.</summary>
    private void CompositeTextPriority(Span<Rgba32> pixels, bool priority)
    {
        // The renderer skips tiles of the other priority, so clear the reused plane first.
        Rgba32[] fullText = textScratch ??= new Rgba32[ScreenWidth * 32 * 8];
        fullText.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(fullText, vram, cgram, new BgVramAddresses { TilemapBaseWord = IntroCinematicRomData.Layers.NarrationTilemapWord, CharacterBaseWord = IntroCinematicRomData.Layers.FontCharacterBaseWord }, rowCount: 32, transparentColorZero: true, priority: priority);
        SnesLayerCompositor.Composite(pixels, fullText.AsSpan(8 * ScreenWidth, ScreenWidth * ScreenHeight));
    }

    /// <summary>Overlays OBJ sprites at one SNES priority level into the cinematic frame.</summary>
    private void CompositeObjPriority(Span<Rgba32> pixels, SpriteFrameBuffer objects, int priority)
    {
        Rgba32[] sprites = objScratch ??= new Rgba32[ScreenWidth * ScreenHeight];
        SnesObjRenderer.Render(
            sprites,
            ObjSpriteImage.Of(objects),
            vram,
            cgram,
            obsel: 3,
            size: new Extent16<ScreenPixels>(ScreenWidth, ScreenHeight),
            priority: priority);
        SnesLayerCompositor.Composite(pixels, sprites);
    }

    /// <summary>Builds the portrait page, clears optional Japanese glyphs, and seeds progressive narration and caret state.</summary>
    internal void SetupFirstIllustratedPage()
    {
        // BlankOut_JapanText_Tiles writes the same 16-byte 2-bpp character 96 times to
        // $7E:4000, then DMA copies the resulting $600 bytes to VMADD $4180 (byte $8300).
        // English still performs this clear; omitting it exposes stale font glyphs in the
        // otherwise empty lower margin.
        var blankJapaneseCharacters =
            new byte[IntroCinematicRomData.Vram.JapaneseBlankCharactersByteCount];
        for (int offset = 0; offset < blankJapaneseCharacters.Length; offset += japaneseBlankCharacter.Length)
            japaneseBlankCharacter.CopyTo(blankJapaneseCharacters, offset);
        new IntroVram(vram).JapaneseBlankCharacters.Load(blankJapaneseCharacters);

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

        new IntroVram(vram).NarrationTilemap.Write(textTilemap);
        objects = new IntroCinematicObjectSystem(
            bus,
            vram,
            textTilemap,
            subtitles,
            audio,
            NarrationPresentation,
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
        new IntroVram(vram).NarrationTilemap.Write(
            textTilemap.AsSpan(0, IntroCinematicRomData.Layers.VisibleTextTransferWordCount));
    }

    /// <summary>Clears selected CGRAM spans before an incoming scene palette begins crossfading.</summary>
    private void ClearPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            PaletteFader.Clear(span.ByteOffset, span.ByteCount);
    }

    /// <summary>Raises selected CGRAM components toward the prepared palette target for the incoming scene.</summary>
    private void FadeInPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            PaletteFader.FadeIn(span.ByteOffset, span.ByteCount);
    }

    /// <summary>Reduces selected palette components during a scene-to-text or text-to-scene fade.</summary>
    private void FadeOutPaletteSpans(IntroCinematicRomData.Palette.Regions spans)
    {
        foreach (IntroPaletteSpan span in spans)
            PaletteFader.FadeOut(span.ByteOffset, span.ByteCount);
    }

    /// <summary>Applies the current INIDISP brightness to final pixels, emitting opaque black at zero.</summary>
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

    /// <summary>Rejects truncated installed graphics before their native-sized VRAM transfers.</summary>
    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Prepares the current display once and owns all inputs needed by a detached renderer.</summary>
    internal LayeredRenderSnapshot CaptureTranslatedRenderSnapshot()
    {
        if (ceresFlight is not null && Phase == IntroCinematicPhase.CeresFlight)
            return ceresFlight.CaptureRenderSnapshot();

        SpriteFrameBuffer oam;
        Bg4BppRenderLayer room;
        if (Phase is IntroCinematicPhase.MotherBrainCrossfade or
            IntroCinematicPhase.MotherBrainFlashback or IntroCinematicPhase.PageTwoCrossfade)
        {
            oam = PrepareMotherBrainOam();
            room = Plane(IntroCinematicRomData.Layers.SceneBg1TilemapWord, new(0,
                flashbackMotherBrain?.BackgroundVerticalScroll ?? GameplayFlashbackBg1VerticalScroll));
        }
        else if (Phase is IntroCinematicPhase.BabyDiscoveryCrossfade or
            IntroCinematicPhase.BabyDiscovery or IntroCinematicPhase.PageThreeCrossfade)
        {
            oam = PrepareBabyDiscoveryOam();
            room = Plane(IntroCinematicRomData.Layers.SceneBg2TilemapWord, new(0, GameplayFlashbackBg1VerticalScroll));
        }
        else if (Phase is IntroCinematicPhase.BabyMetroidDeliveryCrossfade or
            IntroCinematicPhase.BabyMetroidDelivery or IntroCinematicPhase.PageFourCrossfade or
            IntroCinematicPhase.BabyMetroidExaminationCrossfade or
            IntroCinematicPhase.BabyMetroidExamination or IntroCinematicPhase.PageFiveCrossfade)
        {
            oam = PrepareScientistOam();
            room = Plane(ScientistCutscene.TilemapBaseWord, ScientistCutscene.Background.AsBgPlaneScroll());
        }
        else if (objects is null)
        {
            oam = new SpriteFrameBuffer();
            oam.BeginFrame();
            oam.FinalizeFrame();
            // The first card is opaque and unscrolled; later text is transparent
            // and cropped one tile down. Do not conflate those two PPU setups.
            return new(PpuMemorySnapshot.Capture(vram, cgram, oam),
                new RenderLayer[] { new Bg2BppViewportRenderLayer(
                    IntroCinematicRomData.Layers.NarrationTilemapWord,
                    IntroCinematicRomData.Layers.FontCharacterBaseWord, 0, false, null) },
                MenuRenderDefinitions.ObjectSelection, ScreenFade.Displayed(inidisp));
        }
        else
        {
            oam = PrepareIllustratedPageOam();
            room = Plane(IntroCinematicRomData.Layers.PortraitTilemapWord, new(0, GameplayFlashbackBg1VerticalScroll));
        }

        var text = new Bg2BppViewportRenderLayer(IntroCinematicRomData.Layers.NarrationTilemapWord,
            IntroCinematicRomData.Layers.FontCharacterBaseWord, GameplayFlashbackBg1VerticalScroll, true, false);
        // Mode 1 with high-priority BG3: preserve both the OBJ winner and the
        // insertion points of text/room priority planes during palette crossfades.
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), new RenderLayer[]
        {
            new ObjPriorityRenderLayer(0), text, new ObjPriorityRenderLayer(1), room,
            new ObjPriorityRenderLayer(2), room with { Priority = true },
            new ObjPriorityRenderLayer(3), text with { Priority = true },
        }, MenuRenderDefinitions.ObjectSelection, ScreenFade.Displayed(inidisp));
    }

    /// <summary>Defines a four-bit background plane at the selected tilemap and source scroll.</summary>
    private static Bg4BppRenderLayer Plane(ushort tilemap, Position16<BgPlanePixels> scroll) =>
        new(tilemap, 0, scroll, new Extent16<TilemapCells>(32, 32), false);
}
