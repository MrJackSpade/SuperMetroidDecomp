using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Areas.Escape;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Fx.PaletteFx;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Frontend.Intro.Artwork;
using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Samus.Artwork;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.CeresCinematics;

/// <summary>
/// Installed-asset implementation of game state <c>$22</c>: Ceres explodes and the
/// camera subsequently follows Samus's gunship toward Zebes.
/// </summary>
/// <remarks>
/// Bank <c>$8B:C11B-$CADF</c> owns this sequence. The state deliberately retains the
/// native phase boundaries, counters, 16.16 camera motion, Mode-7 matrix values, and
/// compiled cartridge sprite-list instructions. That makes a debugger watch useful:
/// there is no host-authored video or timer that merely happens to end at the
/// same gameplay room.
/// </remarks>
[RoomScopeBoundary("A cinematic scene that owns its own palette FX for the scene's lifetime; no gameplay room is loaded.")]
internal sealed class CeresDestructionCinematicState
{
    /// <summary>Shared queue for Ceres music and the final station blast sound.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>The scene's graphics transfers in the order the phases made them; the capture replays them over the bound artwork (#1358).</summary>
    private readonly List<CeresDestructionUpload> uploads = [];
    /// <summary>The scene's colors: the Ceres base palette and later engine-flicker program (#1358).</summary>
    internal readonly ScreenPalette palette = new();
    /// <summary>The live actors' sprite entries over the OBJ characters the uploads leave (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>The replayed memory image of the bound artwork, until an upload or a rebind changes it.</summary>
    [NonSerialized] private byte[]? image;
    /// <summary>Installed art rebound after debugger restoration rather than serialized with scene state.</summary>
    [NonSerialized] private IntroCinematicArtworkCatalog? artwork;
    /// <summary>Host-owned sprite compositions for Ceres and Zebes actors.</summary>
    [NonSerialized] private IIntroCinematicSpritePresentation? spriteArtwork;
    /// <summary>Live asteroid, blast, planet, star, and title objects in update order.</summary>
    internal readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>
    /// Native OBJ slot ownership for Ceres actors when explosions reuse freed slots. Entries name their actor by
    /// reference, which a debugger restore preserves; a hash table of actors would not survive it.
    /// </summary>
    private readonly List<CeresActorSlot> ceresActorSlots = [];
    /// <summary>Runs the final station blast through the ordinary power-bomb color curve.</summary>
    private readonly SamusPowerBombExplosionState stationExplosion = new();
    /// <summary>Engine-flicker palette program created at the Zebes reveal boundary.</summary>
    internal RoomPaletteFxSystem? paletteFx;
    /// <summary>Installed color provider rebound after debugger restoration.</summary>
    [NonSerialized] private IPaletteFxColorSource? paletteFxColors;
    /// <summary>Planet actor whose sliding pre-instruction uses the larger acceleration.</summary>
    internal IntroDiscoverySprite? zebesPlanetActor;
    /// <summary>Star actor whose offscreen deletion completes the cinematic.</summary>
    private IntroDiscoverySprite? zebesCompletionStarActor;
    /// <summary>Title actor whose instruction starts the Mode-7 flight toward Zebes.</summary>
    private IntroDiscoverySprite? zebesTitleActor;

    /// <summary>Signed 16.16 camera offset applied to the Mode-7 Ceres or Zebes scene.</summary>
    internal FixedPosition<Mode7Pixels> background = new(new(unchecked((ushort)-44), 0), new(unchecked((ushort)-112), 0));
    /// <summary>
    /// Mode-7 scene scale in 1/256 units (256 is identity). Every producer keeps it within zero through
    /// <see cref="CeresDestructionRomData.Motion.FarZebesScaleLimit"/> plus one fast step, and the only decrease
    /// is guarded by the minimum-scale test, so it never wraps.
    /// </summary>
    internal int zoom = CeresDestructionRomData.Motion.IdentityScale;
    /// <summary>Mode-7 rotation angle sampled from the compiled signed sine table.</summary>
    private SnesAngle angle;
    /// <summary>Master brightness from zero (black) to fifteen (full); the slow fades clamp every step to that range.</summary>
    private int brightness;
    /// <summary>Alternating-call delay between brightness changes.</summary>
    private int fadeCounter = 1;
    /// <summary>Phase-specific hold or mosaic size countdown.</summary>
    private int phaseTimer;
    /// <summary>Fallback music wait when no shared audio queue is present.</summary>
    private int musicQueueTimer = 14;
    /// <summary>Completed <c>$8B:C11B</c> NMI waits; setup follows the last wait.</summary>
    private int initialNmiWaits;
    /// <summary>
    /// Cinematic updates elapsed, incremented after phase dispatch and before actor handling. Only its phase
    /// within a four-update cycle is read, which a wider counter preserves exactly.
    /// </summary>
    private int cinematicFrameCounter;
    /// <summary>Calls elapsed for the staggered and repeating Ceres explosion spawner.</summary>
    private int explosionSpawnerFrame;
    /// <summary>Cycles the authored offsets for repeating small explosions.</summary>
    private int explosionOffsetIndex;
    /// <summary>Countdown carried between repeating explosion spawn calls; it fires on reaching zero or below.</summary>
    private NativeCountdown explosionRepeatCountdown;
    /// <summary>Selects Mode 7 or the planet-reveal Mode 1 tilemap path.</summary>
    private bool usesMode7 = true;
    /// <summary>Controls whether BG1 is inserted between OBJ priorities during Mode-7 rendering.</summary>
    private SnesMainScreenLayers mainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;
    /// <summary>State $25 route when the escape timer expires while Samus remains aboard Ceres.</summary>
    private readonly bool withSamus;

    /// <param name="audio">The shared music and sound queue.</param>
    /// <param name="fixedColors">Installed power-bomb fixed colors for the station explosion.</param>
    /// <param name="artwork">Installed cinematic artwork; the scene cannot start without it.</param>
    /// <param name="paletteFxColors">Installed palette-FX colors for the engine-flicker program.</param>
    /// <param name="withSamus">
    /// Game state $25 rather than $22: the escape timer expired before Samus left, so the
    /// gunship never appears and the fade-out ends the game instead of flying to Zebes.
    /// </param>
    public CeresDestructionCinematicState(
        CartridgeAudioState? audio = null,
        PowerBombFixedColorCatalog? fixedColors = null,
        IntroCinematicArtworkCatalog? artwork = null,
        IPaletteFxColorSource? paletteFxColors = null,
        bool withSamus = false)
    {
        this.withSamus = withSamus;
        this.audio = audio;
        this.paletteFxColors = paletteFxColors;
        this.artwork = artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");
        spriteArtwork = new CeresSceneSpritePresentation(
            artwork.CeresFlight.Sprites, artwork.CeresDestruction.Sprites);
        stationExplosion.PresentationColors = fixedColors;
        objects = new ScreenSprites(CeresDestructionScreenImage.ObjectPage(CeresDestructionScreenImage.Compose(this.artwork, uploads, withSamus)),
            ObjectSelectionCodec.Sizes(MenuRenderDefinitions.ObjectSelection));
    }

    /// <summary>
    /// True when the next update resumes the initializer after one of its NMI waits, rather
    /// than entering MainGameLoop.
    /// </summary>
    internal bool ResumesAfterNmiWait =>
        initialNmiWaits is > 0 and <= CeresDestructionRomData.InitialNmiWaits;

    /// <summary>Rebinds current host artwork after restoring a cinematic debugger state.</summary>
    internal void BindFixedColors(PowerBombFixedColorCatalog? colors) =>
        stationExplosion.PresentationColors = colors;

    /// <summary>
    /// Reattaches installed palette colors without restarting the cinematic's
    /// engine-flicker program. The provider is host-owned, just like room FX;
    /// retain it for lazy owner creation.
    /// </summary>
    internal void BindPaletteFxColors(IPaletteFxColorSource? colors)
    {
        paletteFxColors = colors;
    }

    /// <summary>
    /// Rebinds host-owned artwork after restoring a debugger state. Only the native
    /// graphics transfers already reached by the current phase are refreshed; counters,
    /// actors, audio and gameplay timeline remain untouched. The shared Ceres palette
    /// is refreshed only before the later engine-flicker palette program takes ownership.
    /// </summary>
    internal void BindArtwork(IntroCinematicArtworkCatalog? value)
    {
        artwork = value;
        spriteArtwork = value is null ? null : new CeresSceneSpritePresentation(
            value.CeresFlight.Sprites, value.CeresDestruction.Sprites);
        image = null;
        if (value is null) return;
        if (Phase <= CeresDestructionPhase.FlyToZebesInitial)
            value.CeresFlight.Palette.LoadTo(palette);
    }

    /// <summary>Current native dispatcher phase, including the alternate game-over route.</summary>
    public CeresDestructionPhase Phase { get; internal set; }

    /// <summary>True after the completion star leaves the Zebes screen or the terminal route ends.</summary>
    public bool Finished => Phase == CeresDestructionPhase.Finished;

    /// <summary>Executes one call through the native state-$22 cinematic dispatcher.</summary>
    public void Step()
    {
        // $8B:C11B waits for nine NMIs within its first dispatch; each wait is one update
        // that resumes it. After the last it sets up the scene and returns to the state-$22
        // wrapper, whose object handling and frame-counter tail ($8B:A367) then run once.
        if (initialNmiWaits < CeresDestructionRomData.InitialNmiWaits)
        {
            initialNmiWaits++;
            return;
        }
        if (initialNmiWaits == CeresDestructionRomData.InitialNmiWaits)
        {
            initialNmiWaits++;
            SetupCeresDestruction();
            // $8B:C2B8-$C2DF queue the cinematic bank and the state's own track last.
            audio?.QueueMusicDelayed8(MusicCommand.Stop);
            audio?.QueueMusicDelayed8(
                MusicCommand.LoadData(CeresDestructionRomData.Music.CeresDataIndex));
            audio?.QueueMusicDelayed(
                MusicCommand.SelectTrack(withSamus
                    ? CeresDestructionRomData.Music.CeresWithSamusTrack
                    : CeresDestructionRomData.Music.CeresTrack),
                MusicCommandDelay.FromDelayedYArgument(CeresDestructionRomData.Music.DelayArgument));
        }
        else
        {
            StepPhase();
        }
        StepWrapperTail();
    }

    /// <summary>Runs one native cinematic phase after the initial NMI waits and advances the station blast.</summary>
    private void StepPhase()
    {
        // The global bank-$82 frame dispatcher runs HDMA before the cinematic. A
        // newly spawned blast therefore cannot advance until the following call.
        stationExplosion.StepFrame();
        switch (Phase)
        {
            case CeresDestructionPhase.WaitForMusicQueue:
                if (MusicQueueFinished())
                    Phase = CeresDestructionPhase.FadeInAndDrift;
                break;

            case CeresDestructionPhase.FlyToZebesInitial:
                // $8B:C699 sets up the Zebes scene and installs the fade-in ($C79C).
                SetupZebesReveal();
                Phase = CeresDestructionPhase.FadeInZebes;
                break;

            case CeresDestructionPhase.FadeInAndDrift:
                StepInitialDrift();
                zoom++;
                StepSlowFadeIn();
                if (brightness == 15)
                {
                    explosionRepeatCountdown = NativeCountdown.Start(CeresExplosionDefinitions.RepeatingCountdownSeed);
                    Phase = CeresDestructionPhase.ApproachExplosion;
                }
                break;

            case CeresDestructionPhase.ApproachExplosion:
                StepInitialDrift();
                if (zoom < CeresDestructionRomData.Motion.CeresZoomLimit)
                {
                    zoom++;
                }
                else
                {
                    // `$8B:C345` snaps the scale to $0300, creates the final blast, and
                    // installs the flying-away function on this exact handler call.
                    zoom = CeresDestructionRomData.Motion.CeresExplosionScale;
                    SpawnFinalCeresExplosion();
                    Phase = CeresDestructionPhase.FlyingAwayFromExplosion;
                }
                break;

            case CeresDestructionPhase.FlyingAwayFromExplosion:
                background = background.WithX(background.X.WithWhole(unchecked((ushort)(background.X.Whole + 2))));
                angle = angle.AddTableUnits(-1);
                if (zoom < CeresDestructionRomData.Motion.MinimumScale)
                {
                    phaseTimer = CeresDestructionRomData.Timing.ExplosionHoldFrames;
                    Phase = CeresDestructionPhase.HoldAfterExplosion;
                }
                else
                {
                    zoom -= CeresDestructionRomData.Motion.SlowScaleStep;
                }
                break;

            case CeresDestructionPhase.HoldAfterExplosion:
                if (--phaseTimer <= 0)
                {
                    fadeCounter = 1;
                    Phase = CeresDestructionPhase.FadeOutCeres;
                }
                break;

            case CeresDestructionPhase.FadeOutCeres:
                // $8B:C627 installs $C699 once forced blank is reached; it runs next dispatch.
                // State $25 instead stops the music and ends at the death black-out ($C64E).
                if (StepSlowFadeOut())
                {
                    if (withSamus)
                    {
                        audio?.QueueMusicDelayed8(MusicCommand.Stop);
                        Phase = CeresDestructionPhase.Finished;
                    }
                    else
                    {
                        Phase = CeresDestructionPhase.FlyToZebesInitial;
                    }
                }
                break;

            case CeresDestructionPhase.FadeInZebes:
                StepZebesMosaic();
                StepSlowFadeIn();
                if (brightness == 15)
                    Phase = CeresDestructionPhase.RemoveZebesMosaic;
                break;

            case CeresDestructionPhase.RemoveZebesMosaic:
                if (StepZebesMosaic())
                    SetupZebesMode7Actors();
                break;

            case CeresDestructionPhase.PlanetZebesTitle:
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesA:
                background = background.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(background.Y, CeresDestructionRomData.Motion.EighthPixel16Point16));
                background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, CeresDestructionRomData.Motion.NegativeHalfPixel16Point16));
                if (zoom < CeresDestructionRomData.Motion.ZebesApproachScaleLimit)
                    zoom += 4;
                else
                    Phase = CeresDestructionPhase.FlyingTowardZebesB;
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesB:
                background = background.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(background.Y, CeresDestructionRomData.Motion.EighthPixel16Point16));
                background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, CeresDestructionRomData.Motion.NegativeHalfPixel16Point16));
                if (unchecked((short)background.X.Whole) < -128)
                {
                    Phase = CeresDestructionPhase.FlyingTowardZebesC;
                }
                else
                {
                    zoom += CeresDestructionRomData.Motion.SlowScaleStep;
                    angle = angle.AddTableUnits(-1);
                }
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesC:
                background = background.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(background.Y, CeresDestructionRomData.Motion.EighthPixel16Point16));
                background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, CeresDestructionRomData.Motion.EighthPixel16Point16));
                if (zoom < CeresDestructionRomData.Motion.FarZebesScaleLimit)
                {
                    zoom += CeresDestructionRomData.Motion.FastScaleStep;
                }
                else
                {
                    // The cartridge disables the ship's BG1 plane before the hold, not
                    // when the later pan starts. Planet and star OBJ remain enabled.
                    mainScreenLayers = SnesMainScreenLayers.Obj;
                    phaseTimer = CeresDestructionRomData.Timing.ZebesHoldFrames;
                    Phase = CeresDestructionPhase.HoldCloseZebes;
                }
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.HoldCloseZebes:
                StepZebesActors(slidingAway: false);
                if (--phaseTimer <= 0)
                    Phase = CeresDestructionPhase.SlideZebesSceneAway;
                break;

            case CeresDestructionPhase.SlideZebesSceneAway:
                StepZebesActors(slidingAway: true);
                break;
            // The cinematic has ended; its owner advances the game state.
            case CeresDestructionPhase.Finished:
                break;
            default:
                throw new InvalidOperationException($"Undefined CeresDestructionPhase {Phase}.");
        }
    }

    /// <summary>The state-$22 wrapper's work after the cinematic function returns ($8B:A367).</summary>
    private void StepWrapperTail()
    {
        if (Phase <= CeresDestructionPhase.FadeOutCeres)
            StepCeresActors();

        // GameState_37 runs PaletteFxHandler after cinematic objects, including the
        // frame which spawns the program.
        if (Phase > CeresDestructionPhase.FlyToZebesInitial)
        {
            if (paletteFx is null)
            {
                paletteFx = new RoomPaletteFxSystem(new PaletteFxSessionState());
                paletteFx.SpawnDefinition(CeresDestructionRomData.PaletteFx.EngineFlicker, 0);
            }
            paletteFx.Step(palette, paletteFxColors ?? throw new InvalidOperationException(
                    "Ceres palette FX requires installed palette colors."), new PaletteFxConditions { SamusY = 0, EquippedItems = 0, EnemyZeroIsDead = false, AreaMiniBossDefeated = false });
            if (paletteFx.SoundRequests.Count != 0 || paletteFx.MusicRequests.Count != 0)
                throw new InvalidDataException("Ceres engine palette program unexpectedly requested audio.");
        }

        // GameState_37 increments the shared cinematic frame word after calling the
        // current function but before object handling. The mosaic test therefore observes
        // this pre-increment value on its following dispatcher call.
        cinematicFrameCounter++;
    }

    /// <summary>Stages Ceres Mode-7 and OBJ transfers, palettes, and three initial actors after the ninth NMI wait.</summary>
    private void SetupCeresDestruction()
    {
        IntroCinematicArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");
        RequireMinimum(content.CeresFlight.Mode7Characters.Length, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Ceres destruction Mode-7 characters");
        // The stream is four adjacent native work-RAM regions, not merely the two Ceres
        // screens used by this setup call. `$8B:C345` later consumes the front-gunship
        // screen at +$000 and the clear screen at +$C00 without decompressing again.
        RequireMinimum(content.CeresFlight.Mode7Maps.Length + content.CeresDestruction.CeresMaps.Length,
            CeresDestructionRomData.Vram.CeresMinimumTilemapBytes, "Ceres cinematic tilemaps");
        RequireMinimum(content.CeresFlight.ObjectCharacters.Length, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Ceres cinematic OBJ characters");

        // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair. The
        // intro uses offsets zero and $300, so reusing either intro slice produces a
        // coherent but completely incorrect destruction shot.
        Upload(CeresDestructionUpload.CeresScene);
        content.CeresFlight.Palette.LoadTo(palette);

        actors.Clear();
        for (int index = 0; index < CeresDestructionActorDefinitions.InitialActorCount; index++)
        {
            CeresDestructionActorDefinition definition =
                CeresDestructionActorDefinitions.InitialActor(index);
            if (artwork?.CeresDestruction.DestructionActors is { } layout)
            {
                CeresDestructionActorPlacement placement = layout[index];
                definition = definition with { Position = placement.Position };
            }
            actors.Add(CreateActor(definition));
        }
        ceresActorSlots.Clear();
        ceresActorSlots.Add(new CeresActorSlot(actors[0], CeresDestructionRomData.Sprites.AsteroidSlot));
        ceresActorSlots.Add(new CeresActorSlot(actors[1], CeresDestructionRomData.Sprites.SmallAsteroidSlot));
        ceresActorSlots.Add(new CeresActorSlot(actors[2], CeresDestructionRomData.Sprites.VortexSlot));
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    /// <summary>Stages the planet-reveal Mode-1 map and resets camera, angle, fade, and mosaic state.</summary>
    private void SetupZebesReveal()
    {
        IntroCinematicArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Zebes reveal requires installed cinematic artwork.");
        RequireMinimum(content.CeresDestruction.ZebesMap.Transfer.Length, CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes,
            "Zebes reveal tilemap");
        RequireMinimum(content.CeresDestruction.ZebesCharacters.Transfer.Length, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Zebes reveal characters");

        // C699 starts in Mode 1. It also stages the rear-gunship low-byte map at VRAM
        // zero; that dormant map becomes visible only when C7CA later selects Mode 7.
        Upload(CeresDestructionUpload.ZebesReveal);

        actors.Clear();
        background = FixedPosition<Mode7Pixels>.Zero;
        angle = SnesAngle.Zero;
        zoom = CeresDestructionRomData.Motion.IdentityScale;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = CeresDestructionRomData.Timing.InitialMosaicRegister;
        usesMode7 = false;
    }

    /// <summary>Creates the planet, stars, and title sprites for the later Mode-7 flight.</summary>
    private void SetupZebesMode7Actors()
    {
        usesMode7 = true;
        background = new(background.X.WithWhole(CeresDestructionRomData.Sprites.ZebesInitialBackgroundX),
            background.Y.WithWhole(unchecked((ushort)-104)));
        angle = CeresDestructionRomData.Motion.ApproachAngle;
        zoom = CeresDestructionRomData.Motion.IdentityScale;
        actors.Clear();
        zebesPlanetActor = CreateZebesActor(0);
        actors.Add(zebesPlanetActor);
        actors.Add(CreateZebesActor(1));
        actors.Add(CreateZebesActor(2));
        actors.Add(CreateZebesActor(3));
        zebesCompletionStarActor = CreateZebesActor(4);
        actors.Add(zebesCompletionStarActor);
        zebesTitleActor = CreateZebesActor(5);
        actors.Add(zebesTitleActor);
        Phase = CeresDestructionPhase.PlanetZebesTitle;
    }

    /// <summary>Drifts the Ceres camera by one sixteenth pixel down and one quarter pixel left.</summary>
    private void StepInitialDrift()
    {
        background = background.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(background.Y, CeresDestructionRomData.Motion.SixteenthPixel16Point16));
        background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, CeresDestructionRomData.Motion.NegativeQuarterPixel16Point16));
    }

    /// <summary>Raises master brightness one level on alternating calls, capped at fifteen.</summary>
    private void StepSlowFadeIn()
    {
        if (fadeCounter-- > 0)
            return;
        fadeCounter = 1;
        brightness = Math.Min(15, brightness + 1);
    }

    /// <summary>Lowers brightness on alternating calls and reports the first fully black frame.</summary>
    private bool StepSlowFadeOut()
    {
        if (fadeCounter-- > 0)
            return false;
        fadeCounter = 1;
        brightness = Math.Max(0, brightness - 1);
        return brightness == 0;
    }

    /// <summary>Reduces the planet reveal's mosaic size every fourth cinematic frame until clear.</summary>
    private bool StepZebesMosaic()
    {
        if ((cinematicFrameCounter & 3) != 0)
            return false;
        phaseTimer = unchecked((byte)(
            phaseTimer - CeresDestructionRomData.Timing.MosaicFadeStep));
        return (phaseTimer & CeresDestructionRomData.Timing.MosaicSizeMask) == 0;
    }

    /// <summary>Waits for queued cartridge music, using a fixed fallback when audio is absent.</summary>
    private bool MusicQueueFinished() =>
        audio is null ? --musicQueueTimer <= 0 : !audio.HasQueuedMusic;

    /// <summary>Installs an authored sprite's position, attributes, instruction list, and pre-instruction.</summary>
    private static IntroDiscoverySprite CreateActor(
        CeresDestructionActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.Position,
            definition.Attributes,
            definition.Program);
        actor.InstallPreInstruction(definition.ActivePreInstruction);
        return actor;
    }

    /// <summary>Creates one Zebes reveal actor, applying an installed placement override when present.</summary>
    private IntroDiscoverySprite CreateZebesActor(int index)
    {
        CeresDestructionActorDefinition definition =
            CeresDestructionActorDefinitions.ZebesActor(index);
        if (artwork?.CeresDestruction.RevealActors is { } layout)
        {
            CeresRevealActorPlacement placement = layout[index];
            definition = definition with { Position = placement.Position };
        }
        return CreateActor(definition);
    }

    /// <summary>Rejects an extracted artwork stream too short for the native DMA slice.</summary>
    private static void RequireMinimum(int length, int minimum, string name)
    {
        if (length < minimum)
        {
            throw new InvalidDataException(
                $"The {name} stream expanded to ${length:X}, expected at least ${minimum:X}.");
        }
    }

    /// <summary>Spawns authored blast waves and advances live Ceres actors in descending native slot order.</summary>
    private void StepCeresActors()
    {
        explosionSpawnerFrame++;

        // CF33 remains invisible for $80 frames, spawns five staggered small blasts,
        // waits $50 BEFORE installing the repeating-blast pre-instruction, then runs
        // it during the final $40-frame wait. The pre-instruction stops on departure;
        // the instruction list still reaches its four terminal blasts afterward.
        CeresExplosionSpawnEvents events = CeresExplosionDefinitions.EventsAtFrame(explosionSpawnerFrame,
            Phase < CeresDestructionPhase.FlyingAwayFromExplosion, ref explosionRepeatCountdown);
        if (events.Initial)
        {
            for (int index = 0; index < CeresExplosionDefinitions.InitialExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.InitialActor,
                    CeresExplosionDefinitions.InitialExplosion(index));
        }
        if (events.Repeating)
        {
            int offset = explosionOffsetIndex++ &
                (CeresExplosionDefinitions.RepeatingExplosionCount - 1);
            SpawnCeresExplosion(
                CeresExplosionDefinitions.RepeatingActor,
                CeresExplosionDefinitions.RepeatingExplosion(offset));
        }
        if (events.Final)
        {
            for (int index = 0; index < CeresExplosionDefinitions.FinalExplosionCount; index++)
                SpawnCeresExplosion(
                    CeresExplosionDefinitions.FinalWaveActor,
                    CeresExplosionDefinitions.FinalExplosion(index));
        }

        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (index < CeresDestructionActorDefinitions.InitialActorCount)
            {
                CeresDestructionActorDefinition definition =
                    CeresDestructionActorDefinitions.InitialActor(index);
                if (actor.PreInstruction != definition.ActivePreInstruction)
                {
                    throw new InvalidDataException(
                        $"Ceres destruction actor {index} names invalid pre-instruction " +
                        $"{actor.PreInstruction}.");
                }
                if (definition.WrapX)
                    AddWrappedX(actor, unchecked((ushort)definition.HorizontalDelta));
            }
            else
                MoveExplosion(actor);

            actor.Step();
            if (!actor.IsActive)
            {
                ceresActorSlots.RemoveAll(entry => entry.Actor == actor);
                actors.RemoveAt(index);
            }
        }
    }

    /// <summary>
    /// `$8B:C345` queues these two Mode-7 transfers on the same dispatcher call that creates
    /// the final explosion. In state $22 the upper 24 rows become the gunship viewed from the
    /// front; state $25 has no gunship and clears them too. The lower 24 rows are always
    /// cleared. Leaving the original Ceres screens in either half makes the station itself
    /// flee the explosion and later attaches those stale tiles to the Zebes approach.
    /// </summary>
    private void LoadFinalExplosionMaps() => Upload(CeresDestructionUpload.FinalExplosionMaps);

    /// <summary>Records one of the scene's graphics transfers; the next capture replays it.</summary>
    private void Upload(CeresDestructionUpload upload)
    {
        uploads.Add(upload);
        image = null;
    }

    /// <summary>The memory image the scene's transfers leave over the bound artwork, rebinding the OBJ page it shows.</summary>
    internal byte[] Image()
    {
        if (image is not null)
            return image;
        image = CeresDestructionScreenImage.Compose(artwork ?? throw new InvalidOperationException(
            "Ceres destruction rendering requires installed cinematic artwork."), uploads, withSamus);
        objects.BindArtwork(CeresDestructionScreenImage.ObjectPage(image));
        return image;
    }

    /// <summary>Starts the power-bomb blast, queues sound, adds its sprite, and replaces the station maps.</summary>
    private void SpawnFinalCeresExplosion()
    {
        ushort x = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX - background.X.Whole));
        ushort y = unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY - background.Y.Whole));
        // C345 calls the ordinary bank-$88 explosion with screen-space coordinates,
        // before replacing the station tilemap. Keep its ROM curves/colors/lifetime.
        stationExplosion.Spawn(new(x, y));
        audio?.QueueSound(SuperMetroid.Core.Audio.SoundEffectLibrary1Sounds.PowerBombExplosion,
            maximumQueued: 15);
        var cinematicExplosion = new IntroDiscoverySprite(
            new(x, y),
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            CeresExplosionDefinitions.StationBlastActor.Program);
        cinematicExplosion.InstallPreInstruction(
            CeresExplosionDefinitions.StationBlastActor.PreInstruction);
        _ = TryAddCeresActor(cinematicExplosion);

        LoadFinalExplosionMaps();
    }

    /// <summary>Creates a small blast at a camera-relative authored offset with its initial instruction delay.</summary>
    private void SpawnCeresExplosion(
        CeresExplosionActorDefinition definition,
        CeresExplosionPlacement placement)
    {
        var actor = new IntroDiscoverySprite(
            new(unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterX -
                    unchecked((short)background.X.Whole) + placement.Offset.X)),
                unchecked((ushort)(CeresDestructionRomData.Rendering.CeresCenterY -
                    unchecked((short)background.Y.Whole) + placement.Offset.Y))),
            CeresDestructionRomData.Sprites.ExplosionPalette.Raw,
            definition.Program);
        actor.InstallPreInstruction(definition.PreInstruction);

        // The native initializer writes its stagger directly to the instruction timer.
        // GeneralTimer is a different WRAM array used by decrement-and-goto opcodes; using
        // it here would make all five blasts appear immediately despite distinct delays.
        actor.DelayFirstInstruction(placement.DelayFrames);
        _ = TryAddCeresActor(actor);
    }

    /// <summary>Allocates the highest free native sprite slot, preserving the spawner's reserved slot until deletion.</summary>
    private bool TryAddCeresActor(IntroDiscoverySprite actor)
    {
        // Native allocation searches the highest free slot. The invisible spawner
        // owns its slot until its final spawn instruction and deletion have run.
        for (int slot = CeresDestructionRomData.Sprites.AsteroidSlot; slot >= 0; slot--)
        {
            if (slot == CeresDestructionRomData.Sprites.SpawnerSlot &&
                explosionSpawnerFrame <= CeresExplosionDefinitions.SpawnerFinalFrame)
                continue;
            if (ceresActorSlots.Exists(entry => entry.Slot == slot)) continue;
            ceresActorSlots.Add(new CeresActorSlot(actor, slot));
            actors.Add(actor);
            return true;
        }
        // $8B:938A returns carry set when all slots are occupied. These cinematic
        // callers intentionally do not branch on it: this is native capacity
        // behavior, not an unsupported instruction or swallowed runtime failure.
        return false;
    }

    /// <summary>Applies the fleeing gunship's native X subvelocity and signed Y velocity to an explosion sprite.</summary>
    private static void MoveExplosion(IntroDiscoverySprite actor)
    {
        actor.Position = new(
            IntroCinematicMotion.AddSixteenSixteen(
                actor.Position.X,
                0,
                CeresDestructionRomData.Motion.GunshipXSubvelocity),
            IntroCinematicMotion.AddSixteenSixteen(
                actor.Position.Y,
                CeresDestructionRomData.Motion.GunshipYVelocity.WholeWord,
                CeresDestructionRomData.Motion.GunshipYVelocity.Fraction));
    }

    /// <summary>Advances the planet and title objects, applying slide acceleration and finishing on star deletion.</summary>
    private void StepZebesActors(bool slidingAway)
    {
        for (int index = actors.Count - 1; index >= 0; index--)
        {
            IntroDiscoverySprite actor = actors[index];
            if (slidingAway)
            {
                CeresDestructionActorDefinition definition = ReferenceEquals(actor, zebesPlanetActor)
                    ? CeresDestructionActorDefinitions.ZebesActor(0)
                    : ReferenceEquals(actor, zebesCompletionStarActor)
                        ? CeresDestructionActorDefinitions.ZebesActor(4)
                        : ReferenceEquals(actor, zebesTitleActor)
                            ? CeresDestructionActorDefinitions.ZebesActor(5)
                            : CeresDestructionActorDefinitions.ZebesActor(1);
                if (definition.SlidePreInstruction is null)
                {
                    throw new InvalidDataException(
                        "PLANET ZEBES title actor remained active when scene sliding began.");
                }
                actor.InstallPreInstruction(definition.SlidePreInstruction);
                // Zebes accelerates by $40 in 8.8; all four star sheets use $20. Star
                // sheet five is the native completion owner at C8F2. Identify the actors
                // by ownership, not their mutable list index: native actors delete as they
                // cross -$80, so indexes necessarily shift during this loop.
                ushort acceleration = definition.SlideAcceleration;
                actor.GeneralTimer = unchecked((ushort)(actor.GeneralTimer + acceleration));
                SubtractEightEightY(actor, checked((ushort)actor.GeneralTimer));
                if (unchecked((short)actor.WholePosition.Y) < -128)
                {
                    // `$8B:C885/$C8F2/$C95D/$C987` delete each object before its 8-bit
                    // OAM Y coordinate can wrap below the screen. Only star sheet five
                    // additionally installs CADF, handing the cinematic back to game load.
                    if (definition.CompletesScene)
                    {
                        Phase = CeresDestructionPhase.Finished;
                        return;
                    }

                    actor.Delete();
                    actors.RemoveAt(index);
                    continue;
                }
            }

            actor.Step(HandleZebesInstruction);
            if (!actor.IsActive)
                actors.RemoveAt(index);
        }
    }

    /// <summary>Lets the title actor start the camera flight at its authored instruction boundary.</summary>
    private void HandleZebesInstruction(CinematicSpriteCueKind cue)
    {
        switch (cue)
        {
            case CinematicSpriteCueKind.FadeInZebesText:
            case CinematicSpriteCueKind.SpawnJapaneseZebesTextIfNeeded:
            case CinematicSpriteCueKind.FadeOutZebesText:
                return;

            case CinematicSpriteCueKind.StartFlyingToZebes:
                // The title actor, not a host timer, publishes the camera flight exactly
                // where its cartridge instruction list reaches C9C7.
                background = new(background.X.WithWhole(CeresDestructionRomData.Motion.Mode7ExitX),
                    background.Y.WithWhole(unchecked((ushort)-112)));
                angle = CeresDestructionRomData.Motion.ApproachAngle;
                zoom = CeresDestructionRomData.Motion.MinimumScale;
                Phase = CeresDestructionPhase.FlyingTowardZebesA;
                return;

            case CinematicSpriteCueKind.SetPreInstruction:
            case CinematicSpriteCueKind.EndZebesExplosion:
            case CinematicSpriteCueKind.ExplosionFinale:
            case CinematicSpriteCueKind.FadeExplosionPalette:
            case CinematicSpriteCueKind.GreyOut:
            case CinematicSpriteCueKind.Launch:
            case CinematicSpriteCueKind.LaunchHead:
            case CinematicSpriteCueKind.PrepareHead:
            case CinematicSpriteCueKind.Shoot:
            case CinematicSpriteCueKind.SpawnClearTime:
            case CinematicSpriteCueKind.SpawnColon:
            case CinematicSpriteCueKind.SpawnCompletedText:
            case CinematicSpriteCueKind.SpawnExplosionSilhouette:
            case CinematicSpriteCueKind.SpawnFinalExplosionWave:
            case CinematicSpriteCueKind.SpawnHoursTens:
            case CinematicSpriteCueKind.SpawnHoursUnits:
            case CinematicSpriteCueKind.SpawnInitialExplosionWave:
            case CinematicSpriteCueKind.SpawnMinutesTens:
            case CinematicSpriteCueKind.SpawnMinutesUnits:
            case CinematicSpriteCueKind.SpawnParticles:
            case CinematicSpriteCueKind.SpawnRinkas0And1:
            case CinematicSpriteCueKind.SpawnRinkas2And3:
            case CinematicSpriteCueKind.SpawnSuitedJump:
            case CinematicSpriteCueKind.SpawnSuitlessJump:
            case CinematicSpriteCueKind.StartIntroPage2:
            case CinematicSpriteCueKind.StartIntroPage3:
            case CinematicSpriteCueKind.StartIntroPage4:
            case CinematicSpriteCueKind.StartIntroPage5:
            case CinematicSpriteCueKind.StartRinkaMoving:
            case CinematicSpriteCueKind.StartZebesExplosion:
            case CinematicSpriteCueKind.TransitionToCredits:
            default:
                throw new InvalidOperationException($"The Zebes title programs publish no {cue} cue.");
        }
    }

    /// <summary>Adds a fractional asteroid X step and wraps its whole coordinate to the native world mask.</summary>
    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        SnesFixedPosition x = IntroCinematicMotion.AddSixteenSixteen(actor.Position.X, 0, fractionalDelta);
        actor.Position = actor.Position.WithX(x.WithWhole((ushort)(x.Whole & CeresDestructionRomData.Rendering.WorldXMask)));
    }

    /// <summary>Subtracts an unsigned 8.8 slide velocity from the actor's 16.16 Y position with borrow.</summary>
    private static void SubtractEightEightY(IntroDiscoverySprite actor, ushort velocity)
    {
        // The bank-$8B pre-instructions use XBA to split an unsigned 8.8 velocity into
        // whole/subposition words before a 16-bit subtraction with borrow.
        uint fixedPosition = new SnesFixedPosition(actor.Position.Y.Whole, actor.Position.Y.Fraction).RawValue;
        uint fixedVelocity = SnesSixteenSixteen.FromEightEight((uint)velocity);
        fixedPosition = unchecked(fixedPosition - fixedVelocity);
        actor.Position = actor.Position.WithY(new(SnesFixedPosition.WholeOf(unchecked((int)fixedPosition)), unchecked((ushort)fixedPosition)));
    }

    /// <summary>Captures the scene's exact Mode 7/OBJ insertion order or Mode 1 planet plane.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        byte[] shown = Image();
        PrepareRenderSprites();
        RenderLayer[] layers;
        if (usesMode7 && (mainScreenLayers & SnesMainScreenLayers.Bg1) == 0)
        {
            layers = [new ObjPriorityRenderLayer(0), new ObjPriorityRenderLayer(1),
                new ObjPriorityRenderLayer(2), new ObjPriorityRenderLayer(3)];
        }
        else if (usesMode7)
        {
            var (a, b, c, d) = CalculateMatrix();
            var mode7 = new Mode7RenderRegisters(a, b, c, d,
                Phase <= CeresDestructionPhase.FadeOutCeres
                    ? new Offset16<Mode7Pixels>(CeresDestructionRomData.Rendering.CeresCenterX, CeresDestructionRomData.Rendering.CeresCenterY)
                    : new Offset16<Mode7Pixels>(CeresDestructionRomData.Rendering.ZebesCenterX, CeresDestructionRomData.Rendering.ZebesCenterY),
                background.Whole.AsSignedRegister());
            layers = [new ObjPriorityRenderLayer(0), new Mode7RenderLayer(mode7),
                new ObjPriorityRenderLayer(1), new ObjPriorityRenderLayer(2), new ObjPriorityRenderLayer(3)];
        }
        else
        {
            // Preserve the existing non-mosaicked Mode 1 path. Mosaic implementation
            // is a cartridge-correctness change, not an implicit part of this extraction.
            layers = [new Bg4BppRenderLayer(CeresDestructionRomData.Rendering.Mode1TilemapWord,
                CeresDestructionRomData.Rendering.Mode1CharacterWord, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), null)];
        }
        if (CaptureStationExplosion() is { } blast)
            layers = [.. layers, blast];
        return new(new PpuMemorySnapshot(shown, palette.Colors, objects.Draws), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    /// <summary>One Ceres actor's native OBJ slot.</summary>
    /// <param name="Actor">The actor holding the slot.</param>
    /// <param name="Slot">The slot it holds.</param>
    private sealed record CeresActorSlot(IntroDiscoverySprite Actor, int Slot);

    /// <summary>Captures the active station blast's scanline color addition as a render layer.</summary>
    private ScanlineColorAddRenderLayer? CaptureStationExplosion() =>
        SnesGameplayFrameRenderer.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(stationExplosion),
            layer1: Position16<RoomPixels>.Zero, firstVisibleScanline: 0);

    /// <summary>Places live actors as sprite entries, retaining native slot order when Ceres explosions recycle slots.</summary>
    private void PrepareRenderSprites()
    {
        objects.BeginFrame();
        // Same-priority OBJ overlap is decided by OAM order. Allocation order is
        // not stable slot order after an earlier explosion dies and its slot is reused.
        IEnumerable<IntroDiscoverySprite> drawActors = Phase <= CeresDestructionPhase.FadeOutCeres
            ? actors.OrderByDescending(actor => ceresActorSlots.First(entry => entry.Actor == actor).Slot) : actors;
        foreach (IntroDiscoverySprite actor in drawActors)
            actor.Draw(objects, installedArt: spriteArtwork);
        objects.FinishFrame();
    }

    /// <summary>Builds the Mode-7 rotation and scale matrix from signed cartridge sine samples.</summary>
    private (short A, short B, short C, short D) CalculateMatrix()
    {
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return (a, b, unchecked((short)-b), a);
    }

    /// <summary>Reads the compiled signed sine quadrant used by Ceres Mode-7 motion.</summary>
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(new SnesByteAngle(index));

    /// <summary>Applies 8.8 matrix scaling with signed 16-bit truncation; the scale stays below 32768.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, scalar)));
}
