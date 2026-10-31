using SuperMetroid.Core.Samus.Drawing;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Palettes;
using SuperMetroid.Core.Samus.Poses;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>
/// Opening-cinematic state $1E, beginning with “The last Metroid is in captivity”.
/// </summary>
/// <remarks>
/// Construction ports the complete graphics setup at $8B:A395. The visible state machine
/// continues through the first narration card, the complete first illustrated/typewriter
/// page, and the native palette transition into the Mother Brain gameplay flashback.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own flashback level and Samus projectiles for the scene's lifetime; no gameplay room is loaded.")]
public sealed class IntroCinematicState : IFrontendScreen
{
    /// <inheritdoc/>
    void IFrontendScreen.BindArtwork(GameArtwork previous, GameArtwork artwork, SuperMetroidGameState state)
    {
        Ensure.NotNull(previous);
        Ensure.NotNull(artwork);
        if (!ReferenceEquals(previous.MapPresentation, artwork.MapPresentation))
        {
            NarrationPresentation = artwork.MapPresentation?.IntroNarration;
            BindIntroFont(artwork.MapPresentation?.IntroFont);
            BindSamusHurtColors(artwork.MapPresentation?.SamusHurtColors);
        }
        if (!ReferenceEquals(previous.IntroCinematicArt, artwork.IntroCinematicArt))
            BindCharacterArtwork(artwork.IntroCinematicArt);
        if (!ReferenceEquals(previous.SamusBodyArt, artwork.SamusBodyArt))
            BindSamusBodyArtwork(artwork.SamusBodyArt);
        if (!ReferenceEquals(previous.ProjectileFrameBindings, artwork.ProjectileFrameBindings))
            ProjectileFrameBindings = artwork.ProjectileFrameBindings;
        if (!ReferenceEquals(previous.SpriteSheets, artwork.SpriteSheets))
            BindSpriteSheets(artwork.SpriteSheets);
    }

    /// <summary>Binds the installed sprite sheets the flashbacks' Samus and projectiles draw from.</summary>
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets;
        projectileSheets = sheets is null ? null : ProjectileSheets.IntroFlashbacks(sheets);
        CurrentFlashbackSamus?.Presentation.BodyFrames.BindSheets(spriteSheets);
        CurrentCeresFlight?.BindSheets(sheets);
    }

    /// <summary>The installed sheets the flashbacks' Samus draws from.</summary>
    [NonSerialized] private InstalledSpriteSheets? spriteSheets;

    /// <summary>Draws <paramref name="family"/>'s frames from the intro sheet.</summary>
    private CinematicSpriteSheet IntroSprites(CinematicSpriteFamily family) =>
        new((spriteSheets ?? throw new InvalidOperationException("Intro sprites require the installed sprite sheets."))
            .Get(SpriteSheetId.IntroSprites), family);

    /// <summary>The intro's projectile sheet, which the flashbacks' projectiles, bombs and trails draw from.</summary>
    [NonSerialized] private ProjectileSheets? projectileSheets;

    /// <summary>The session's random-number word, which the demo Samus's animation draws from.</summary>
    public required SharedRandom Random { get; init; }

    /// <summary>The session's floor update residue, bound into the flashback Samus.</summary>
    public SuperMetroid.Core.Compatibility.FloorUpdateResidue? FloorQueueResidue { get; init; }

    /// <summary>Host-owned frame artwork for the Mother Brain flashback projectile slots.</summary>
    public InstalledProjectileFrameBindings? ProjectileFrameBindings
    {
        get => flashbackProjectiles.Animation.FrameBindings;
        set => flashbackProjectiles.Animation.FrameBindings = value;
    }

    /// <summary>Installed first-font transfer rebound after debugger restoration.</summary>
    [field: NonSerialized] internal IntroFontAtlas? IntroFont { get; private set; }
    /// <summary>Installed background, object, and palette content for the intro scenes.</summary>
    [field: NonSerialized] internal IntroCinematicArtwork? CharacterArtwork { get; private set; }
    /// <summary>Installed Samus body graphics rebound into active flashback actors.</summary>
    [NonSerialized] private InstalledSamusBodyArtwork? samusBodyArtwork;
    /// <summary>Host-owned hurt-flash colors consumed by the shared Samus palette handler.</summary>
    [NonSerialized] private InstalledSamusHurtColors? samusHurtColors;
    /// <summary>Editable suit-flash colors used by the cinematic's ordinary Samus hurt handler.</summary>
    public void BindSamusHurtColors(InstalledSamusHurtColors? value) => samusHurtColors = value;
    /// <summary>Rebinds installed Samus pixels to active and later intro flashbacks.</summary>
    public void BindSamusBodyArtwork(InstalledSamusBodyArtwork? value)
    {
        samusBodyArtwork = value;
        CurrentFlashbackSamus?.Presentation.BodyFrames.BindArtwork(value);
        CurrentBabyDiscovery?.Samus.Presentation.BodyFrames.BindArtwork(value);
    }
    /// <summary>Current host-owned narration content; debugger states retain only playback state.</summary>
    [field: NonSerialized]     public IntroNarrationPresentation? NarrationPresentation
    {
        get;
        set
        {
            field = value;
            CurrentObjects?.BindNarration(value);
        }
    }
    /// <summary>Current host-owned opening font; restored states must explicitly rebind it.</summary>
    public void BindIntroFont(IntroFontAtlas? value)
    {
        if (value is not null)
            RequireFont(value);
        IntroFont = value;
    }
    /// <summary>Rebinds installed BG and OBJ pixels after restoring emulated state.</summary>
    public void BindCharacterArtwork(IntroCinematicArtwork? value)
    {
        CharacterArtwork = value;
        CurrentCeresFlight?.BindArtwork(value?.CeresFlight);
        if (value is not null)
        {
            // A restored fade keeps its current CGRAM/accumulators, but later scene
            // targets must use the currently selected installed palette.
            introColours = value.Palette.Colours;
            if (Phase is IntroCinematicPhase.Initial or IntroCinematicPhase.WaitForInitialMusicQueue)
                value.Palette.LoadTo(palette);
            // The first card and the final line are installed pictures the narration plane places (#1467), so a
            // rebind keeps the live letters and caret state as they are.
        }
    }

    /// <summary>
    /// BG1VOFS inherited by both gameplay flashbacks from the first illustrated page ($8B:A66F).
    /// Their actor coordinates remain screen-relative and are not shifted with the background.
    /// </summary>
    internal const ushort GameplayFlashbackBg1VerticalScroll = 8;

    /// <summary>Owns queued cinematic music and sound effects when installed.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The narration text the intro shows across narration and flashback scenes (#1467).</summary>
    internal readonly IntroNarrationText narrationText = new();
    /// <summary>The intro's colors, including the palette ranges changed by scene crossfades (#1358).</summary>
    internal readonly ScreenPalette palette = new();
    /// <summary>The cinematic actors' sprite entries over the intro OBJ page (#1358).</summary>
    internal readonly ScreenSprites objectSprites;
    /// <summary>Tracks the current physical button edges alongside scripted demo input.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>Runs ordinary beam projectiles during gameplay-style flashbacks.</summary>
    internal readonly SamusProjectileSystem flashbackProjectiles;
    /// <summary>The installed services reproducing Samus's glitch reads, handed to the flashbacks' Samus and projectiles.</summary>
    private readonly SamusOverreadServices samusOverreads;
    /// <summary>Runs the shared bomb owner, including projectile cooldown, in flashbacks.</summary>
    private readonly SamusBombProjectileSystem flashbackBombProjectiles = new();
    /// <summary>The installed intro colours, by owner, that transitions back from gameplay flashbacks restore.</summary>
    private ScreenColours introColours;
    /// <summary>Persistent bank-$8C actor and caret walker for illustrated pages.</summary>
    internal IntroCinematicObjectSystem? CurrentObjects { get; private set; }
    /// <summary>The intro's sprite objects, which exist from the first text page onward.</summary>
    internal IntroCinematicObjectSystem Objects => CurrentObjects ??
        throw new InvalidOperationException("The intro's sprite objects have not been created; the text pages create them.");
    /// <summary>Applies the selected Japanese subtitle treatment to narration pages.</summary>
    private readonly IntroJapaneseSubtitles subtitles;
    /// <summary>Interpolates selected CGRAM spans during text and scene crossfades.</summary>
    private CinematicPaletteFader? paletteFader;
    /// <summary>The intro's palette fader, which exists while the intro's scenes run.</summary>
    private CinematicPaletteFader PaletteFader => paletteFader ??
        throw new InvalidOperationException("The intro's palette fader has not been created.");
    /// <summary>Samus gameplay state shared by the Mother Brain and SR388 flashbacks.</summary>
    internal SamusState? CurrentFlashbackSamus { get; private set; }
    /// <summary>Samus in the Mother Brain flashback, which exists while the flashback runs.</summary>
    internal SamusState FlashbackSamus => CurrentFlashbackSamus ??
        throw new InvalidOperationException("The Mother Brain flashback has not introduced Samus.");
    /// <summary>$1A57 during the Mother Brain flashback: $8B:AF65 sets it, $8B:B842 clears it.</summary>
    private IntroSamusDisplay flashbackSamusDisplay;
    /// <summary>Mother Brain animation state active only in the first gameplay flashback.</summary>
    internal IntroMotherBrainSpriteState? CurrentFlashbackMotherBrain { get; private set; }
    /// <summary>Mother Brain's sprite in the flashback, which exists while the flashback runs.</summary>
    internal IntroMotherBrainSpriteState FlashbackMotherBrain => CurrentFlashbackMotherBrain ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Explosion actors shown after the Mother Brain demo input ends.</summary>
    internal IntroMotherBrainExplosionSystem? FlashbackMotherBrainExplosions { get; set; }
    /// <summary>Rinka actors and collision for the Mother Brain scene.</summary>
    internal IntroRinkaSystem? FlashbackRinkas { get; set; }
    /// <summary>Egg and baby actors in the SR388 discovery flashback.</summary>
    internal IntroBabyDiscoveryState? CurrentBabyDiscovery { get; private set; }
    /// <summary>The Baby Metroid discovery scene, which exists while that scene runs.</summary>
    internal IntroBabyDiscoveryState BabyDiscovery => CurrentBabyDiscovery ??
        throw new InvalidOperationException("The Baby Metroid discovery scene is not running.");
    /// <summary>Laboratory actors shared by delivery and examination scenes.</summary>
    internal IntroScientistCutsceneState? CurrentScientistCutscene { get; private set; }
    /// <summary>The scientist cutscene, which exists while that scene runs.</summary>
    internal IntroScientistCutsceneState ScientistCutscene => CurrentScientistCutscene ??
        throw new InvalidOperationException("The scientist cutscene is not running.");
    /// <summary>Final SPACE COLONY flight sequence after the narrated pages.</summary>
    internal IntroCeresFlightState? CurrentCeresFlight { get; set; }
    /// <summary>The flight to Ceres, which exists while that scene runs.</summary>
    internal IntroCeresFlightState CeresFlight => CurrentCeresFlight ??
        throw new InvalidOperationException("The Ceres flight is not running.");
    /// <summary>Scripted controller stream for the Mother Brain gameplay demonstration.</summary>
    internal DemoInputState? CurrentFlashbackDemoInput { get; private set; }
    /// <summary>The flashback's scripted controller input, which exists while the flashback runs.</summary>
    internal DemoInputState FlashbackDemoInput => CurrentFlashbackDemoInput ??
        throw new InvalidOperationException("The Mother Brain flashback is not running.");
    /// <summary>Collision data for the gameplay-style flashback room.</summary>
    internal RoomLevelData? CurrentFlashbackLevel { get; private set; }
    /// <summary>The flashback room's collision, which exists while the flashback runs.</summary>
    internal RoomLevelData FlashbackLevel => CurrentFlashbackLevel ??
        throw new InvalidOperationException("The Mother Brain flashback room is not loaded.");
    /// <summary>Cinematic-function countdown for the active palette transition.</summary>
    private WrappingCountdown crossfadeCounter;
    /// <summary>Alternate transition countdown retained between scientist and text phases.</summary>
    private WrappingCountdown introCrossfadeCounter;
    /// <summary>
    /// Updates the cinematic has run, counted from zero at its creation; the shared Samus systems receive it through
    /// <see cref="FrameCounterWord"/> as their frame-counter word.
    /// </summary>
    private int elapsedUpdates;
    /// <summary>How bright the narration screen is; the intro starts blanked and fades each narration page in and out.</summary>
    private ScreenBrightness brightness = ScreenBrightness.Blanked;
    /// <summary>How bright the narration screen the cinematic publishes is.</summary>
    internal ScreenBrightness Brightness { get => brightness; set => brightness = value; }
    /// <summary>Times the narration's screen fades at cinematic entry, between pages and at exit.</summary>
    private readonly ScreenFade fade = new();
    /// <summary>Fallback music-queue delay when no cartridge audio state is installed.</summary>
    private int timer = 8;

    /// <summary>Creates the opening cinematic and performs its initial native graphics and music setup.</summary>
    /// <param name="samusOverreads">The installed services reproducing Samus's glitch reads, for the flashbacks' Samus and projectiles.</param>
    /// <param name="audio">Optional cartridge audio state that owns queued music and sound effects.</param>
    /// <param name="introFont">The installed opening-cinematic font atlas.</param>
    /// <param name="characterArtwork">The installed opening-cinematic BG and OBJ artwork.</param>
    /// <param name="beamPalettes">Optional installed beam palettes used by gameplay flashbacks.</param>
    /// <param name="samusBodyArtwork">Optional installed Samus body artwork used by gameplay flashbacks.</param>
    /// <param name="japaneseText">The options menu's Japanese text setting (native <c>AltText</c>).</param>
    public IntroCinematicState(
        SamusOverreadServices samusOverreads,
        AudioQueues? audio = null,
        IntroFontAtlas? introFont = null,
        IntroCinematicArtwork? characterArtwork = null,
        InstalledBeamPalettes? beamPalettes = null,
        InstalledSamusBodyArtwork? samusBodyArtwork = null,
        bool japaneseText = false)
    {
        this.samusOverreads = Ensure.NotNull(samusOverreads);
        flashbackProjectiles = new SamusProjectileSystem(new BeamChargeState(), samusOverreads);

        subtitles = new IntroJapaneseSubtitles(japaneseText);
        this.audio = audio;
        IntroFont = introFont ?? throw new InvalidOperationException(
            "Opening cinematic requires the installed font atlas.");
        CharacterArtwork = characterArtwork ?? throw new InvalidOperationException(
            "Opening cinematic requires installed artwork.");
        this.samusBodyArtwork = samusBodyArtwork;
        characterArtwork.Palette.LoadTo(palette);
        introColours = characterArtwork.Palette.Colours;

        RequireMinimum(introFont.CharacterCount * (IntroFontAtlasFormat.ByteCount / IntroFontAtlasFormat.TileCount),
            IntroCinematicDefinitions.Artwork.FontOneBytes, "intro font one");
        RequireFont(introFont);

        // `$8B:A469-$A529` upload the font and OBJ sheets the intro binds; the first narration card, the scene pages and
        // the portrait are installed pictures (#1467).
        objectSprites = new ScreenSprites();

        // $8B:A3AC performs the ordinary beam palette load before copying the full intro palette, which
        // replaces it; the flashback projectile pages bind the beam's characters.
        SamusProjectileSystem.LoadBeamPalette(palette.Colours(ScreenColourGroup.SamusBeams), equippedBeams: 0, beamPalettes);
        characterArtwork.Palette.LoadTo(palette);

        // The loads above are invisible: the intro stays black until its fade.
        // $8B:A395 itself runs as the next dispatch's cinematic function.
        Phase = IntroCinematicPhase.Initial;
    }

    /// <summary>Gets the current opening-cinematic state-machine phase.</summary>
    public IntroCinematicPhase Phase { get; private set; }

    /// <summary>True after the final narration fade hands control to the Ceres flight.</summary>
    public bool NarrationFinished { get; private set; }

    /// <summary>True when the SPACE COLONY caption and its final fade have completed.</summary>
    public bool CeresFlightFinished => CurrentCeresFlight?.Finished ?? false;

    /// <summary>Advances the opening cinematic by one gameplay update.</summary>
    /// <param name="controllerInput">The current raw controller-held word.</param>
    public void Step(ControllerButton controllerInput)
    {
        flashbackProjectiles.ImpactAudio.BeginImpactAudioFrame(context: ImpactAudioContext.Cinematic);
        // Cinematic functions consume joypad1_newkeys, not the raw held word. Latching at
        // this state boundary preserves a one-frame edge and prevents a held A/Start from
        // skipping both the options menu and the first narration page.
        controller.Latch(controllerInput);
        bool explosionsExistedBeforeThisFrame = FlashbackMotherBrainExplosions is not null;
        elapsedUpdates = unchecked(elapsedUpdates + 1);
        switch (Phase)
        {
            case IntroCinematicPhase.Initial:
                // $8B:A59B-$A5A2: stop the music, then load the opening music data.
                audio?.QueueMusic(MusicRequest.Stop);
                audio?.QueueMusic(
                    MusicRequest.LoadDataSet(MusicDataSet.LastMetroidInCaptivity));
                Phase = IntroCinematicPhase.WaitForInitialMusicQueue;
                break;

            case IntroCinematicPhase.WaitForInitialMusicQueue:
                // A request queued with the shortest delay waits eight updates before HasQueuedMusic clears.
                if (MusicQueueFinished())
                {
                    // $8B:A5A7 seeds both fade words with two.
                    Phase = IntroCinematicPhase.FadeInFirstNarration;
                    fade.SetTiming(IntroCinematicDefinitions.Fade.NarrationSlowFade, IntroCinematicDefinitions.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeInFirstNarration:
                if (fade.AdvanceSlowFadeIn(ref brightness))
                {
                    Phase = IntroCinematicPhase.LastMetroidIsInCaptivity;
                    timer = 60;
                }
                break;

            case IntroCinematicPhase.LastMetroidIsInCaptivity:
                if (--timer <= 0)
                {
                    audio?.QueueMusic(MusicRequest.SelectTrack(5));
                    Phase = IntroCinematicPhase.GalaxyIsAtPeace;
                    timer = 200;
                }
                break;

            case IntroCinematicPhase.GalaxyIsAtPeace:
                if (--timer <= 0)
                {
                    // The next music requests wait on the music queue before the
                    // documented four-second hold; the shortest delay is their slot.
                    audio?.QueueMusic(MusicRequest.Stop);
                    audio?.QueueMusic(
                        MusicRequest.LoadDataSet(MusicDataSet.GalaxyAtPeace));
                    audio?.ScheduleMusic(
                        MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.SceneTrack),
                        MusicQueueDelay.AtLeastShortest(
                            IntroCinematicDefinitions.Music.SceneTrackDelay));
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
                    fade.SetTiming(IntroCinematicDefinitions.Fade.NarrationSlowFade, IntroCinematicDefinitions.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeOutFirstNarration:
                // $8B:A663 installs $8B:A66F, which sets up page one in the next dispatch.
                if (fade.AdvanceSlowFadeOut(ref brightness))
                    Phase = IntroCinematicPhase.SetupPageOne;
                break;

            case IntroCinematicPhase.SetupPageOne:
                SetupFirstIllustratedPage();
                break;

            case IntroCinematicPhase.WaitForPageOneMusicQueue:
                // Track 5 scheduled fourteen updates ahead is the longest request issued by
                // $8B:A66F, so HasQueuedMusic becomes clear after fourteen update slots.
                if (MusicQueueFinished())
                {
                    Objects.StartEnglishPageOne();
                    // $8B:A82B seeds both fade words with two.
                    Phase = IntroCinematicPhase.FadeInPageOne;
                    fade.SetTiming(IntroCinematicDefinitions.Fade.NarrationSlowFade, IntroCinematicDefinitions.Fade.NarrationSlowFade);
                }
                break;

            case IntroCinematicPhase.FadeInPageOne:
                if (fade.AdvanceSlowFadeIn(ref brightness))
                    Phase = IntroCinematicPhase.PageOneText;
                break;

            case IntroCinematicPhase.PageOneText:
                break;

            case IntroCinematicPhase.PageOneAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupMotherBrainFlashback();
                break;

            case IntroCinematicPhase.MotherBrainCrossfade:
                StepMotherBrainCrossfade();
                break;

            case IntroCinematicPhase.MotherBrainFlashback:
                if (CurrentFlashbackMotherBrain?.PageTwoRequested == true)
                    SetupPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoCrossfade:
                StepPageTwoCrossfade();
                break;

            case IntroCinematicPhase.PageTwoText:
                break;

            case IntroCinematicPhase.PageTwoAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscoveryCrossfade:
                StepBabyDiscoveryCrossfade();
                break;

            case IntroCinematicPhase.BabyDiscovery:
                if (CurrentBabyDiscovery?.PageThreeRequested == true)
                    SetupPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeCrossfade:
                StepPageThreeCrossfade();
                break;

            case IntroCinematicPhase.PageThreeText:
                break;

            case IntroCinematicPhase.PageThreeAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyMetroidDelivery();
                break;

            case IntroCinematicPhase.BabyMetroidDeliveryCrossfade:
                StepBabyMetroidDeliveryCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidDelivery:
                if (CurrentScientistCutscene?.PageFourRequested == true)
                    SetupPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourCrossfade:
                StepPageFourCrossfade();
                break;

            case IntroCinematicPhase.PageFourText:
                break;

            case IntroCinematicPhase.PageFourAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupBabyMetroidExamination();
                break;

            case IntroCinematicPhase.BabyMetroidExaminationCrossfade:
                StepBabyMetroidExaminationCrossfade();
                break;

            case IntroCinematicPhase.BabyMetroidExamination:
                if (CurrentScientistCutscene?.PageFiveRequested == true)
                    SetupPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveCrossfade:
                StepPageFiveCrossfade();
                break;

            case IntroCinematicPhase.PageFiveText:
                break;

            case IntroCinematicPhase.PageFiveAwaitingInput:
                if (!subtitles.HoldInputWait() && controller.NewlyPressed != ControllerButton.None)
                    SetupPageSix();
                break;

            case IntroCinematicPhase.StartPageSix:
                StartPageSix();
                break;

            case IntroCinematicPhase.PageSixText:
                break;

            case IntroCinematicPhase.IntroFadeOut:
                // $8B:B72F fades through HandleFadingOut and finishes once the screen is black.
                fade.FadeOut(ref brightness);
                if (brightness.IsBlanked)
                {
                    NarrationFinished = true;
                    // $8B:BCA0 immediately replaces the narration PPU setup with the
                    // Ceres Mode 7 scene. Give that scene its own VRAM, CGRAM, and
                    // brightness owner: the narration has just deliberately reached
                    // a blanked screen, while $8B:BDE4 restores full brightness only after
                    // the fourteen-frame delayed music command has completed.
                    CurrentCeresFlight = new IntroCeresFlightState(audio
                        ?? throw new InvalidOperationException("The Ceres flight waits on the cartridge music queue."),
                        CharacterArtwork?.CeresFlight, subtitles.Enabled,
                        spriteSheets ?? throw new InvalidOperationException("The Ceres flight requires the installed sprite sheets."));
                    Phase = IntroCinematicPhase.CeresFlight;
                }
                break;

            case IntroCinematicPhase.CeresFlight:
                CeresFlight.Step();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(IntroCinematicPhase)} {Phase}.");
        }

        if (CurrentObjects is not null)
        {
            CurrentObjects.Step(controller.NewlyPressed);
            if (CurrentObjects.PageOneAwaitingInput)
            {
                if (Phase == IntroCinematicPhase.PageOneText)
                    Phase = IntroCinematicPhase.PageOneAwaitingInput;
            }
            if (CurrentObjects.PageTwoAwaitingInput && Phase == IntroCinematicPhase.PageTwoText)
                Phase = IntroCinematicPhase.PageTwoAwaitingInput;
            if (CurrentObjects.PageThreeAwaitingInput && Phase == IntroCinematicPhase.PageThreeText)
                Phase = IntroCinematicPhase.PageThreeAwaitingInput;
            if (CurrentObjects.PageFourAwaitingInput && Phase == IntroCinematicPhase.PageFourText)
                Phase = IntroCinematicPhase.PageFourAwaitingInput;
            if (CurrentObjects.PageFiveAwaitingInput && Phase == IntroCinematicPhase.PageFiveText)
                Phase = IntroCinematicPhase.PageFiveAwaitingInput;
            if (CurrentObjects.IntroFinishRequested && Phase == IntroCinematicPhase.PageSixText)
            {
                // $B240 selects the ordinary fade-out owner with delay/counter one. The
                // framebuffer on this request frame is still full-brightness page six.
                fade.SetTiming(IntroCinematicDefinitions.Fade.FinishFade, IntroCinematicDefinitions.Fade.FinishFade);
                Phase = IntroCinematicPhase.IntroFadeOut;
            }
        }

        // $8B:8E0D runs Samus's handlers, ahead of the sprite pass, only while $1A57 is set.
        if (CurrentFlashbackSamus is not null && flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            // Intro-demo alpha refreshes the live radius before projectiles and beta movement.
            // Pose commits at the end of the previous frame deliberately leave it unchanged.
            SamusPoseMetadata.RefreshCollisionRadii(CurrentFlashbackSamus);

            // $90:E91D runs DemoInputObjectHandler inside Samus's current-state handler,
            // after the cinematic function, so a demo loaded by $8B:AEB8 plays its first
            // entry in the same dispatch, ahead of the Samus pose input that consumes it.
            if (CurrentFlashbackDemoInput is not null)
                StepMotherBrainDemo();

            StepMotherBrainFlashbackSamus();
        }

        // Game state $25 handles cinematic sprites after the active cinematic function and
        // after Samus/projectiles. Mother Brain can therefore consume a missile at its new
        // position during this same frame, exactly as $8B:B786 does.
        if (CurrentFlashbackMotherBrain is not null)
        {
            CurrentFlashbackMotherBrain.RunPreInstruction(
                palette,
                introColours,
                FrameCounterWord(elapsedUpdates),
                crossfadeCounter.RawValue);
            // $8B:B842: deleting Mother Brain after the page-two crossfade hides Samus from
            // the next dispatch on.
            if (!CurrentFlashbackMotherBrain.IsVisible)
                flashbackSamusDisplay = IntroSamusDisplay.Hidden;
            if (CurrentFlashbackMotherBrain.TakeMissileHit(flashbackProjectiles.EnemyImpacts, flashbackBombProjectiles))
            {
                FlashbackMotherBrainExplosions = new IntroMotherBrainExplosionSystem();
                FlashbackMotherBrainExplosions.SpawnFourthHitExplosions();
            }
        }

        if (FlashbackRinkas is not null && CurrentFlashbackSamus is not null)
        {
            FlashbackRinkas.Step(
                CurrentFlashbackSamus,
                motherBrainExploding: CurrentFlashbackMotherBrain?.ExplosionStarted == true,
                explosionsAllocated: FlashbackMotherBrainExplosions is not null);
        }

        // This ordering makes a newly spawned timer-one object select its first spritemap
        // on the same accepted frame as the page-one input transition.
        CurrentFlashbackMotherBrain?.Step();
        if (explosionsExistedBeforeThisFrame)
        {
            // The eight actors are allocated while object zero is already being processed,
            // so their first generic-handler call occurs on the following frame. Hit count
            // four is reached on the spawn frame, after the handler passed their free slots.
            FlashbackMotherBrainExplosions?.Step(crossfadeCounter.RawValue);
        }

        // Handle_CinematicSpriteObjects ($8B:93EF) runs after the cinematic function, so the
        // actors a scene setup spawns take their first step in that same dispatch.
        CurrentBabyDiscovery?.Step(FrameCounterWord(elapsedUpdates), crossfadeCounter.RawValue);
        CurrentScientistCutscene?.Step(crossfadeCounter.RawValue, introCrossfadeCounter.RawValue);
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
        CurrentFlashbackSamus = new SamusState(samusOverreads.AnimationContinuation)
        {
            FloorQueueResidue = FloorQueueResidue,
            Pose = SamusPoseId.FacingLeftNormalPose,
            Position = FixedPosition<RoomPixels>.FromWhole(new(155, 115)),
            SelectedHudItem = HudItem.Missile,
            Missiles = 900,
            MaxMissiles = 900,
        };
        // Shared Samus handlers read the nonzero cinematic-function word to select
        // intro palette restoration and suppress gameplay-only sound/landing effects.
        CurrentFlashbackSamus.LiquidPhysics.CinematicFunctionActive = true;
        CurrentFlashbackSamus.Presentation.BodyFrames.BindArtwork(samusBodyArtwork);
        CurrentFlashbackSamus.Presentation.BodyFrames.BindSheets(spriteSheets);
        SamusPoseMetadata.RefreshCollisionRadii(CurrentFlashbackSamus);
        SamusPresentation.InitializeAnimation(CurrentFlashbackSamus);
        CurrentFlashbackSamus.CommitPoseHistory();
        CurrentFlashbackSamus.Presentation.PrimeGraphics(CurrentFlashbackSamus);
        // The next displayed frame shows the freshly selected top and bottom definitions;
        // apply them now so the first rendered
        // flashback frame names initialized OBJ characters rather than stale intro art.
        CurrentFlashbackMotherBrain = new IntroMotherBrainSpriteState();
        FlashbackMotherBrainExplosions = null;
        FlashbackRinkas = new IntroRinkaSystem();

        // The flashback's collision room is built from its own terrain blocks, so the
        // visual BG1 screen never stands in for the collision contract.
        CurrentFlashbackLevel = CreateMotherBrainLevel();
        flashbackProjectiles.Reset();
        CurrentFlashbackDemoInput = new DemoInputState();
        CurrentFlashbackDemoInput.Clear();
        CurrentFlashbackDemoInput.Enable();
        CurrentFlashbackDemoInput.Load(IntroDemoInputObjectDefinitions.IntroMotherBrainFlashback);
        // $8B:AF62: Samus is processed, and drawn ahead of the cinematic objects.
        flashbackSamusDisplay = IntroSamusDisplay.SamusFirst;

        // $8B:B018 replaces the target palette with kPalettes_Intro, decomposes every
        // component, clears only the incoming gameplay ranges, and immediately composes.
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.Narration);
        paletteFader.ComposeInto(palette);

        // The sprite setup stored 127 in both cinematic_var13 and (through sprite object
        // $CE55's setup) cinematic_var4. $B250 tests the old counter, then decrements it.
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.MotherBrainCrossfade;
    }

    /// <summary>Advances 32 component updates of the text-to-gameplay fade before exposing the Mother Brain room.</summary>
    private void StepMotherBrainCrossfade()
    {
        // $8B:B250 updates only when the pre-decrement counter is divisible by four.
        // Values 124..0 therefore yield exactly 32 component updates.
        if (crossfadeCounter.RawValue % IntroCinematicDefinitions.Palette.CrossfadeStepFrames == 0)
        {
            FadeOutPaletteSpans(IntroCinematicDefinitions.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicDefinitions.Palette.Narration);
            PaletteFader.ComposeInto(palette);
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
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
        if (crossfadeCounter.RawValue % IntroCinematicDefinitions.Palette.CrossfadeStepFrames == 0)
        {
            FadeInPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicDefinitions.Palette.Narration);
            PaletteFader.ComposeInto(palette);
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
        CurrentBabyDiscovery = new IntroBabyDiscoveryState(Random, CurrentFlashbackSamus
            ?? throw new InvalidOperationException("Discovery setup requires the preceding flashback Samus state."), audio);

        // The old gameplay objects have reached their delete lists. Releasing the scoped
        // references prevents their projectile/collision handlers from running behind the
        // new scene while preserving the shared VRAM and CGRAM they intentionally reuse.
        CurrentFlashbackSamus = null;
        CurrentFlashbackMotherBrain = null;
        FlashbackMotherBrainExplosions = null;
        FlashbackRinkas = null;
        CurrentFlashbackDemoInput = null;
        CurrentFlashbackLevel = null;
        flashbackProjectiles.Reset();

        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.Narration);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.BabyDiscoveryCrossfade;
    }

    /// <summary>Reuses the text-to-gameplay fade before showing the SR388 discovery room alone.</summary>
    internal void StepBabyDiscoveryCrossfade()
    {
        if (crossfadeCounter.RawValue % IntroCinematicDefinitions.Palette.CrossfadeStepFrames == 0)
        {
            FadeOutPaletteSpans(IntroCinematicDefinitions.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicDefinitions.Palette.Narration);
            PaletteFader.ComposeInto(palette);
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
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateDelivery();
        CurrentBabyDiscovery = null;

        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.Discovery);
        paletteFader.ComposeInto(palette);

        // The native intro alternates two adjacent WRAM counters. Page-three setup left
        // CinematicFunctionTimer at $007F; this setup writes the separate intro counter.
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
        introCrossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
        if (crossfadeCounter.RawValue % IntroCinematicDefinitions.Palette.CrossfadeStepFrames == 0)
        {
            FadeOutPaletteSpans(IntroCinematicDefinitions.Palette.Gameplay);
            FadeInPaletteSpans(IntroCinematicDefinitions.Palette.Discovery);
            PaletteFader.ComposeInto(palette);
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
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
            FadeInPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
            FadeOutPaletteSpans(IntroCinematicDefinitions.Palette.Discovery);
            PaletteFader.ComposeInto(palette);
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
        CurrentScientistCutscene = IntroScientistCutsceneState.CreateExamination();
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.Discovery);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
        introCrossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
        Phase = IntroCinematicPhase.BabyMetroidExaminationCrossfade;
    }

    /// <summary>Reuses the text-to-scientist fade for the examination scene.</summary>
    private void StepBabyMetroidExaminationCrossfade() =>
        StepTextToScientistCrossfade(IntroCinematicPhase.BabyMetroidExamination);

    /// <summary>Starts page five after the baby examination scene and selects the reverse palette transition.</summary>
    private void SetupPageFiveCrossfade()
    {
        Objects.StartEnglishPageFive();
        paletteFader = new CinematicPaletteFader(introColours);
        ClearPaletteSpans(IntroCinematicDefinitions.Palette.GameplayClear);
        paletteFader.ComposeInto(palette);
        crossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
        introCrossfadeCounter = WrappingCountdown.Start(IntroCinematicDefinitions.Palette.CrossfadeInitialCounter);
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
        CurrentScientistCutscene = null;
        Phase = IntroCinematicPhase.PageSixText;
    }

    /// <summary>Runs the scripted Samus gameplay, projectiles, collisions, and palette updates for the Mother Brain flashback.</summary>
    internal void StepMotherBrainDemo()
    {
        FlashbackDemoInput.Step(DemoInputState.RunReturnOnlyPreInstruction, HandleMotherBrainDemoCue);

        // HandleHUDSpecificBehaviorAndProjectiles calls the shared cooldown owner before
        // its ordinary-projectile half. Standing pose two cannot place bombs, but running
        // the real owner is still required for missile admission timing.
        flashbackBombProjectiles.StepFrame(new SamusTerrain { Level = FlashbackLevel }, FlashbackSamus, FlashbackDemoInput.Held, FlashbackDemoInput.NewlyPressed);
        flashbackProjectiles.StepFrame(new SamusTerrain { Level = FlashbackLevel }, FlashbackSamus, new ProjectileButtonInput { ControllerInput = FlashbackDemoInput.Held, ControllerNewInput = FlashbackDemoInput.NewlyPressed, ControllerPreviousNewInput = FlashbackDemoInput.PublishedPreviousNewlyPressed }, layer1: Position16<RoomPixels>.Zero, sharedProjectiles: flashbackBombProjectiles);
        if (flashbackProjectiles.LastFrameResult.QueuedSoundEffect is { } soundEffect)
        {
            audio?.QueueSound(
                soundEffect,
                flashbackProjectiles.LastFrameResult.QueuedSoundMaximum
                    ?? throw new InvalidDataException("A projectile sound was queued without its queue limit."));
        }

        // $90:E70D/$90:E71C: both the demo and locked handlers end with
        // ResetMovementAndPoseChangeVariables, so a later missile inherits only the
        // movement recorded in its own dispatch.
        SamusAerialPoseTransitions.DropPendingTransitionShot(FlashbackSamus);
        FlashbackSamus.ProjectileInheritance.ClearMovement();
    }

    /// <summary>
    /// Runs the movement/animation/interruption/palette slice of the native intro-demo
    /// handler at <c>$90:E833-$90:E84D</c>, followed by the timer owner at <c>$8B:8E1A</c>.
    /// </summary>
    private void StepMotherBrainFlashbackSamus()
    {
        SamusState samus = FlashbackSamus;
        RoomLevelData level = FlashbackLevel;
        ControllerButton demoInput = CurrentFlashbackDemoInput?.Held ?? 0;

        // Each frame shows the definitions selected by the previous Samus draw. This
        // is what makes pose/frame changes use their corresponding ROM graphics rather than
        // interpreting stale standing tiles as a hurt body on the next visible frame.

        AerialMovementResult? fallingMovement = null;
        BlockMoveResult? hurtEndingProbe = null;
        bool transitionAccepted = false;
        bool groundedDemoStepped = false;
        if (samus.KnockbackActive)
        {
            // `$90:E83C` dispatches the installed `$90:DF38` movement handler. Its shared
            // vertical calculation now carries the eleven-frame Rinka arc through its apex.
            SamusKnockbackMovement.Step(level, samus, FrameCounterWord(elapsedUpdates));
        }
        else if (samus.Pose is SamusPoseId.KnockbackRightPose or SamusPoseId.KnockbackLeftPose)
        {
            hurtEndingProbe = SamusGroundedMovement.StepKnockbackOrCrystalFlashEnding(
level, samus, FrameCounterWord(elapsedUpdates));
        }
        else if (CurrentFlashbackDemoInput?.Enabled == true &&
            (SamusPoseMetadata.IsLeftFacingStandingPose(samus.Pose) || SamusPoseMetadata.IsLeftFacingRunningPose(samus.Pose)))
        {
            // The demo's later run/jump records use the same alpha/beta order as the
            // discovery scene. This coordinator already animates and commits its pose.
            IntroSamusDemoMovement.StepGroundedLeft(level, samus, demoInput,
                CurrentFlashbackDemoInput.NewlyPressed, FrameCounterWord(elapsedUpdates), Random);
            groundedDemoStepped = true;
        }
        else if (samus.Pose == SamusPoseId.SpinJumpLeftPose)
        {
            fallingMovement = SamusAerialMovement.StepSpinJump(level, samus,
                demoInput, FrameCounterWord(elapsedUpdates), CurrentFlashbackDemoInput?.NewlyPressed ?? 0);
        }
        else if (SamusPoseMetadata.IsLeftFacingNormalJumpPose(samus.Pose))
        {
            fallingMovement = SamusAerialMovement.StepNormalJump(level, samus,
                demoInput, FrameCounterWord(elapsedUpdates));
        }
        else if (samus.Pose is SamusPoseId.FallingRightPose or SamusPoseId.FallingLeftPose)
        {
            // When `$90:DDE9` ends humanoid knockback it selects pose $29/$2A and restores
            // normal movement. The following frames therefore execute the ordinary type-6
            // fall until the cartridge collision layer reports the original laboratory floor.
            fallingMovement = SamusAerialMovement.StepFalling(

                level,
                samus,
                demoInput,
                FrameCounterWord(elapsedUpdates));
        }

        // Intro-demo state is not an animation shortcut. `$90:E83F` advances the same
        // cartridge delay programs as gameplay, including every visible hurt/fall frame.
        if (!groundedDemoStepped)
            SamusPresentation.AnimateNoFx(samus, new SamusAnimationStep { ControllerInput = demoInput, FrameCounter = FrameCounterWord(elapsedUpdates), Random = Random });

        // Delay opcodes `$F8/$FD` publish a prospective pose; they are not ordinary frame
        // delays. `$90:E849` consumes that publication before the byte following the opcode
        // can ever be misread as another animation duration. This completes landing art's
        // native A5 -> 02 transition instead of walking beyond its ROM delay list.
        bool animationTransitionApplied = SamusPoseTransitions.ApplyPendingVerifiedAnimationTransition(samus);
        animationTransitionApplied |= SamusKnockbackMovement.TryFinishExpiredHitInterruption(samus);
        transitionAccepted |= animationTransitionApplied;

        if (!animationTransitionApplied && hurtEndingProbe is { IsUnobstructedDownwardMovement: true })
        {
            SamusPoseTransitions.ApplyWalkedOffFloorTransition(samus, level, SamusPoseTransitions.SelectFallingPoseForCurrentAim(samus), FrameCounterWord(elapsedUpdates));
            animationTransitionApplied = transitionAccepted = true;
        }

        // The normal new-state handler resolves a downward collision only after animation.
        // Apply the shared landing transition here so pose, radii, feet alignment, and the
        // next animation list all come from the same bank-$91 implementation as gameplay.
        if (!animationTransitionApplied && fallingMovement is { Landed: true })
        {
            SamusPoseTransitions.ApplyAerialLanding(samus, wasSpinning: samus.Pose == SamusPoseId.SpinJumpLeftPose, demoInput);
            transitionAccepted = true;
        }

        // `$90:E842` consumes the timer published by Rinka zero on the preceding cinematic-
        // sprite pass. Keeping this after animation matches SamusNewStateHandler_IntroDemo:
        // the hit frame finishes its standing animation before command one installs $53/$54.
        if (!samus.KnockbackActive &&
            !samus.KnockbackTimer.IsZero &&
            samus.KnockbackDirection == Samus.Movement.SamusKnockbackDirection.None)
        {
            SamusKnockbackMovement.Start(samus, demoInput, KnockbackLaunch.FromPendingHit(samus));
            transitionAccepted = true;
        }

        // The intro uses the ordinary transition epilogue after movement, animation,
        // and hurt interruption. Publish only the final accepted pose, once per slot;
        // an unchanged animation frame must not erase the preceding transition history.
        if (transitionAccepted)
            samus.CommitPoseHistory();

        // `$90:E84D` runs the ordinary Samus palette handler even in intro-demo state.
        // This supplies the alternating hurt colors and eventual suit-palette restoration;
        // invincibility flicker remains independently enforced by Samus.Draw.
        SamusHurtFlashPalette.Update(palette.Colours(ScreenColourGroup.SamusSuit), samus, demoInput, samusHurtColors);

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
        SamusPoseMetadata.RefreshCollisionRadii(FlashbackSamus);
        // $91:874B: a Samus already in pose two keeps her running animation.
        SamusPresentation.SetAnimationFrameIfPoseChanged(FlashbackSamus);
        FlashbackSamus.CommitPoseHistory();
        FlashbackDemoInput.Disable();
    }

    /// <summary>Builds the flashback's collision room: its terrain, with every block's behaviour cleared.</summary>
    private static RoomLevelData CreateMotherBrainLevel()
    {
        const int width = IntroMotherBrainCollisionDefinitions.Columns;
        const int height = IntroMotherBrainCollisionDefinitions.RoomRows;
        RoomTerrainBlock[] foreground = IntroMotherBrainCollision.CreateForeground();
        var behavior = new RoomBlockBehavior[width * height];
        behavior.AsSpan().Fill(RoomBlockBehaviorValues.None);

        // The background plane is irrelevant to collision in this visual room; it exists
        // only to satisfy the shared RoomLevelData ownership contract.
        return new RoomLevelData(new Extent16<RoomBlocks>(width, height),
            new RoomLevelPlanes(foreground, behavior, new RoomBlockAppearance[width * height]));
    }

    /// <summary>
    /// Prepares Mother Brain OAM once per display; trail drawing advances state, so a captured frame cannot be redrawn through this path.
    /// </summary>
    internal SpriteFrameBuffer PrepareMotherBrainSprites()
    {
        // Both direct rendering and detached capture enter here once per display.
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        objectSprites.BeginFrame();
        // $8B:8E2D: a negative $1A57 draws Samus and both projectile passes before the
        // cinematic objects; once $B842 clears it, only the objects remain.
        if (flashbackSamusDisplay != IntroSamusDisplay.Hidden)
        {
            FlashbackSamus.Presentation.Draw(FlashbackSamus, sprites, layer1: Position16<RoomPixels>.Zero, FrameCounterWord(elapsedUpdates));
            // The flashback's VRAM holds the power beam's characters whatever beam its Samus fires.
            var projectileArtwork = new ProjectileDrawArtwork(projectileSheets, EquippedBeams: 0);
            flashbackProjectiles.DrawLiveProjectiles(sprites, Position16<RoomPixels>.Zero, FrameCounterWord(elapsedUpdates), projectileArtwork);
            flashbackProjectiles.AdvanceTrails(timeIsFrozen: false);
            flashbackProjectiles.DrawTrails(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
            flashbackProjectiles.DrawExplosions(sprites, Position16<RoomPixels>.Zero, projectileArtwork);
        }
        FlashbackRinkas?.Draw(objectSprites, IntroSprites(CinematicSpriteFamily.IntroRinka));
        FlashbackMotherBrainExplosions?.Draw(objectSprites, IntroSprites(CinematicSpriteFamily.IntroMotherBrainExplosion));
        // The blank pose draws nothing.
        if (FlashbackMotherBrain.IsVisible && FlashbackMotherBrain.Pose is { } motherBrainPose &&
            motherBrainPose != CinematicSpritePose.Blank)
        {
            // cinematic_var15=$FFFF makes DrawIntroSprites draw Samus first and cinematic
            // actors afterward. Retaining that OAM insertion order preserves overlap wins.
            IntroSprites(CinematicSpriteFamily.IntroMotherBrain).Draw(
                motherBrainPose, objectSprites,
                IntroMotherBrainSpriteState.Position.ToSpritePosition(),
                IntroMotherBrainSpriteState.Colouring);
        }
        objectSprites.FinishFrame();
        foreach (SpriteDraw draw in objectSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();

        return sprites;
    }

    /// <summary>Orders Samus and discovery actors by the cinematic display flag before finalizing OBJ data.</summary>
    internal SpriteFrameBuffer PrepareBabyDiscoverySprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        // $8B:8E2D orders Samus against the cinematic objects by $1A57's sign.
        IntroSamusDisplay display = BabyDiscovery.SamusDisplay;
        if (display == IntroSamusDisplay.SamusFirst)
            DrawBabyDiscoverySamus(sprites);
        objectSprites.BeginFrame();
        BabyDiscovery.DrawActors(objectSprites, IntroSprites(CinematicSpriteFamily.IntroEggEffect),
            IntroSprites(CinematicSpriteFamily.IntroDiscoveryActor));
        objectSprites.FinishFrame();
        foreach (SpriteDraw draw in objectSprites.Draws)
            sprites.Add(draw);
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
        BabyDiscovery.Samus.Presentation.Draw(BabyDiscovery.Samus, sprites, layer1: Position16<RoomPixels>.Zero, FrameCounterWord(elapsedUpdates));
    }

    /// <summary>Collects scientist-scene objects for the current cinematic display.</summary>
    internal SpriteFrameBuffer PrepareScientistSprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        objectSprites.BeginFrame();
        ScientistCutscene.Draw(objectSprites, IntroSprites(CinematicSpriteFamily.IntroScientist));
        objectSprites.FinishFrame();
        foreach (SpriteDraw draw in objectSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();
        return sprites;
    }

    /// <summary>Collects the persistent caret and illustrated-page cinematic objects in display order.</summary>
    internal SpriteFrameBuffer PrepareIllustratedPageSprites()
    {
        var sprites = new SpriteFrameBuffer();
        sprites.BeginFrame();
        objectSprites.BeginFrame();
        // The caret's blank pose, shown while it blinks off, draws nothing.
        if (Objects.CaretPose is { } caretPose && caretPose != CinematicSpritePose.Blank)
        {
            // The persistent caret begins at (8,24), moves to (8,$F8) during illustrated
            // crossfades, and is restored when the next narration page starts.
            // OBSEL=$03 selects word $6000 as the OBJ character base, matching the
            // initial $9A:D200 -> VMADD $6000 DMA.
            IntroSprites(CinematicSpriteFamily.IntroCaret).Draw(
                caretPose, objectSprites, Objects.Caret.ToSpritePosition(),
                ScreenColourGroup.IntroCrossFadeObjects);
        }
        objectSprites.FinishFrame();
        foreach (SpriteDraw draw in objectSprites.Draws)
            sprites.Add(draw);
        sprites.FinalizeFrame();

        return sprites;
    }


    /// <summary>Builds the portrait page, clears optional Japanese glyphs, and seeds progressive narration and caret state.</summary>
    internal void SetupFirstIllustratedPage()
    {
        // BlankOut_JapanText_Tiles blanks the Japanese glyphs and ClearCinematicBgObjects($2F) clears the staging map
        // under its border; $8B:A72B then maps the final line into rows 24-27. The pages' plane shows the blank text rows
        // over the installed final line, drawn with the Japanese glyphs blanked (#1467).
        narrationText.ShowPages();
        CurrentObjects = new IntroCinematicObjectSystem(
            narrationText,
            subtitles,
            audio,
            NarrationPresentation);
        audio?.QueueMusic(MusicRequest.Stop);
        audio?.QueueMusic(
            MusicRequest.LoadDataSet(MusicDataSet.Intro));
        audio?.ScheduleMusic(
            MusicRequest.SelectTrack(IntroCinematicDefinitions.Music.SceneTrack),
            MusicQueueDelay.AtLeastShortest(
                IntroCinematicDefinitions.Music.SceneTrackDelay));
        timer = IntroCinematicDefinitions.Music.DiscoveryInitialTimer;
        Phase = IntroCinematicPhase.WaitForPageOneMusicQueue;
    }

    /// <summary>
    /// Production follows bank $80's real HasQueuedMusic result. Standalone actor tests do
    /// not own a music queue, so they retain the previously explicit local countdown.
    /// </summary>
    private bool MusicQueueFinished() => audio is null ? --timer <= 0 : !audio.HasQueuedMusic;

    /// <summary>Clears the narration's text rows and shows them cleared, as the native clear uploads the visible rows.</summary>
    private void ClearVisibleNarrationText()
    {
        narrationText.ClearText();
        narrationText.Publish();
    }

    /// <summary>Clears the selected screen colours before an incoming scene palette begins crossfading.</summary>
    private void ClearPaletteSpans(IReadOnlyList<ScreenColourGroup> groups)
    {
        foreach (ScreenColourGroup group in groups)
            PaletteFader.Clear(group);
    }

    /// <summary>Raises the selected screen colours' components toward the prepared palette target for the incoming scene.</summary>
    private void FadeInPaletteSpans(IReadOnlyList<ScreenColourGroup> groups)
    {
        foreach (ScreenColourGroup group in groups)
            PaletteFader.FadeIn(group);
    }

    /// <summary>Reduces the selected screen colours' components during a scene-to-text or text-to-scene fade.</summary>
    private void FadeOutPaletteSpans(IReadOnlyList<ScreenColourGroup> groups)
    {
        foreach (ScreenColourGroup group in groups)
            PaletteFader.FadeOut(group);
    }

    /// <summary>Rejects truncated installed graphics before their native-sized VRAM transfers.</summary>
    private static void RequireMinimum(int length, int minimum, string name)
    {
        if (length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${length:X}, expected at least ${minimum:X}.");
    }

    /// <summary>Prepares the current display once and owns all inputs needed by a detached renderer.</summary>
    internal LayeredScene CaptureTranslatedRenderSnapshot()
    {
        if (CurrentCeresFlight is not null && Phase == IntroCinematicPhase.CeresFlight)
            return CurrentCeresFlight.CaptureRenderSnapshot();

        SpriteFrameBuffer sprites;
        PicturedPlaneLayer room;
        if (Phase is IntroCinematicPhase.MotherBrainCrossfade or
            IntroCinematicPhase.MotherBrainFlashback or IntroCinematicPhase.PageTwoCrossfade)
        {
            sprites = PrepareMotherBrainSprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.MotherBrainRoom),
                (CurrentFlashbackMotherBrain?.Background ??
                    new Position16<RoomPixels>(0, GameplayFlashbackBg1VerticalScroll)).AsBgPlaneScroll());
        }
        else if (Phase is IntroCinematicPhase.BabyDiscoveryCrossfade or
            IntroCinematicPhase.BabyDiscovery or IntroCinematicPhase.PageThreeCrossfade)
        {
            sprites = PrepareBabyDiscoverySprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePage.BabyDiscoveryRoom), new(0, GameplayFlashbackBg1VerticalScroll));
        }
        else if (Phase is IntroCinematicPhase.BabyMetroidDeliveryCrossfade or
            IntroCinematicPhase.BabyMetroidDelivery or IntroCinematicPhase.PageFourCrossfade or
            IntroCinematicPhase.BabyMetroidExaminationCrossfade or
            IntroCinematicPhase.BabyMetroidExamination or IntroCinematicPhase.PageFiveCrossfade)
        {
            sprites = PrepareScientistSprites();
            room = Plane(IntroScenePictures.PagePlane(SceneSheets, IntroScenePages.Laboratory(ScientistCutscene.Scene)), ScientistCutscene.Background.AsBgPlaneScroll());
        }
        else if (CurrentObjects is null)
        {
            sprites = new SpriteFrameBuffer();
            sprites.BeginFrame();
            sprites.FinalizeFrame();
            // The first card is opaque and unscrolled; later text is transparent
            // and cropped one tile down. Do not conflate those two PPU setups.
            return new(new FramePaint(palette.Shown(), sprites.Draws),
                new DisplayLayer[] { new PicturedPlaneLayer(IntroNarrationPictures.FirstCardPlane(SceneSheets), default, null) { ShowsColourZero = true } },
                Brightness.Level);
        }
        else
        {
            sprites = PrepareIllustratedPageSprites();
            room = Plane(IntroScenePictures.PortraitPlane(SceneSheets, CurrentObjects.EyeFrame), new(0, GameplayFlashbackBg1VerticalScroll));
        }

        PicturedPlaneLayer text = Plane(IntroNarrationPictures.PagesPlane(SceneSheets, narrationText), new(0, GameplayFlashbackBg1VerticalScroll));
        // Mode 1 with high-priority BG3: preserve both the OBJ winner and the
        // insertion points of text/room priority planes during palette crossfades.
        return new(new FramePaint(palette.Shown(), sprites.Draws), new DisplayLayer[]
        {
            new SpriteDepthLayer(0), text, new SpriteDepthLayer(1), room,
            new SpriteDepthLayer(2), room with { Priority = true },
            new SpriteDepthLayer(3), text with { Priority = true },
        }, Brightness.Level);
    }

    /// <summary>Rejects a font atlas that is not the installed font's size.</summary>
    internal static void RequireFont(IntroFontAtlas font)
    {
        if (Ensure.NotNull(font).CharacterCount != IntroFontAtlasFormat.TileCount)
            throw new InvalidDataException($"Opening font contains {font.CharacterCount} characters; expected {IntroFontAtlasFormat.TileCount}.");
    }

    /// <summary>
    /// The shared Samus systems' frame-counter word after <paramref name="elapsedUpdates"/> updates: its low sixteen
    /// bits, which return to zero every 65536 updates exactly as the cartridge's sixteen-bit counter does.
    /// </summary>
    internal static ushort FrameCounterWord(int elapsedUpdates) => unchecked((ushort)elapsedUpdates);

    /// <summary>The scene plane showing <paramref name="pictures"/> at the selected scroll, its back pixels only (#1467).</summary>
    private static PicturedPlaneLayer Plane(PicturedPlane pictures, Position16<TilePlanePixels> scroll) => new(pictures, scroll, false);

    /// <summary>The installed sheets the scene pictures are drawn from.</summary>
    private InstalledSpriteSheets SceneSheets =>
        spriteSheets ?? throw new InvalidOperationException("Opening cinematic rendering requires the installed sprite sheets.");

}
