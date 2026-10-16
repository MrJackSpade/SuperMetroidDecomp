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
internal sealed class CeresDestructionCinematicState
{
    /// <summary>Supplies the scene's native instruction and color-math source words.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Shared queue for Ceres music and the final station blast sound.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Retains scene tilemaps and OBJ characters across native phase transfers.</summary>
    internal readonly SnesVram vram = new();
    /// <summary>Receives the Ceres base palette and later engine-flicker program.</summary>
    internal readonly SnesCgram cgram = new();
    /// <summary>Contiguous Ceres scene, front-gunship, and clear Mode-7 map source regions.</summary>
    private byte[] ceresTilemaps;
    /// <summary>Installed art rebound after debugger restoration rather than serialized with scene state.</summary>
    [NonSerialized] private IntroCinematicArtworkCatalog? artwork;
    /// <summary>Host-owned sprite compositions for Ceres and Zebes actors.</summary>
    [NonSerialized] private IIntroCinematicSpritePresentation? spriteArtwork;
    /// <summary>Live asteroid, blast, planet, star, and title objects in update order.</summary>
    internal readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Native OBJ slot ownership for Ceres actors when explosions reuse freed slots.</summary>
    private readonly Dictionary<IntroDiscoverySprite, int> ceresActorSlots = [];
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
    /// <summary>Native 8.8 Mode-7 scale used to construct the scene matrix.</summary>
    internal ushort zoom = CeresDestructionRomData.Motion.IdentityScale;
    /// <summary>Mode-7 rotation angle sampled from the compiled signed sine table.</summary>
    private SnesAngle angle;
    /// <summary>PPU master brightness stepped by the slow scene fades.</summary>
    private byte brightness;
    /// <summary>Alternating-call delay between brightness changes.</summary>
    private int fadeCounter = 1;
    /// <summary>Phase-specific hold or mosaic size countdown.</summary>
    private int phaseTimer;
    /// <summary>Fallback music wait when no shared audio queue is present.</summary>
    private int musicQueueTimer = 14;
    /// <summary>Completed <c>$8B:C11B</c> NMI waits; setup follows the last wait.</summary>
    private int initialNmiWaits;
    /// <summary>Shared cinematic frame word incremented after phase dispatch and before actor handling.</summary>
    private ushort cinematicFrameCounter;
    /// <summary>Calls elapsed for the staggered and repeating Ceres explosion spawner.</summary>
    private int explosionSpawnerFrame;
    /// <summary>Cycles the authored offsets for repeating small explosions.</summary>
    private int explosionOffsetIndex;
    /// <summary>Native countdown carried between repeating explosion spawn calls.</summary>
    private ushort explosionRepeatCountdown;
    /// <summary>Selects Mode 7 or the planet-reveal Mode 1 tilemap path.</summary>
    private bool usesMode7 = true;
    /// <summary>Controls whether BG1 is inserted between OBJ priorities during Mode-7 rendering.</summary>
    private SnesMainScreenLayers mainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;
    /// <summary>State $25 route when the escape timer expires while Samus remains aboard Ceres.</summary>
    private readonly bool withSamus;

    /// <param name="bus">Cartridge address space read by the scene's actors and transfers.</param>
    /// <param name="audio">The shared music and sound queue.</param>
    /// <param name="fixedColors">Installed power-bomb fixed colors for the station explosion.</param>
    /// <param name="artwork">Installed cinematic artwork; the scene cannot start without it.</param>
    /// <param name="paletteFxColors">Installed palette-FX colors for the engine-flicker program.</param>
    /// <param name="withSamus">
    /// Game state $25 rather than $22: the escape timer expired before Samus left, so the
    /// gunship never appears and the fade-out ends the game instead of flying to Zebes.
    /// </param>
    public CeresDestructionCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        PowerBombFixedColorCatalog? fixedColors = null,
        IntroCinematicArtworkCatalog? artwork = null,
        IPaletteFxColorSource? paletteFxColors = null,
        bool withSamus = false)
    {
        this.bus = Ensure.NotNull(bus);
        this.withSamus = withSamus;
        this.audio = audio;
        this.paletteFxColors = paletteFxColors;
        this.artwork = artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");
        spriteArtwork = new CeresSceneSpritePresentation(
            artwork.CeresFlight.Sprites, artwork.CeresDestruction.Sprites);
        stationExplosion.PresentationColors = fixedColors;
        ceresTilemaps = LoadCeresTilemaps();
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
        if (value is null) return;
        ceresTilemaps = LoadCeresTilemaps();
        if (Phase <= CeresDestructionPhase.FlyToZebesInitial)
            value.CeresFlight.Palette.LoadTo(cgram);
        if (Phase <= CeresDestructionPhase.FadeOutCeres)
        {
            new Mode7Vram(vram).LoadCharacters(value.CeresFlight.Mode7Characters.Span);
            new CeresVram(vram).ObjectCharacters.Load(value.CeresFlight.ObjectCharacters.Span);
            new CeresVram(vram).ObjectCharacters.Load(value.IntroObjectCharacters.Transfer.Span[..CeresDestructionRomData.Vram.SharedObjectCharacterBytes]);
            int offset = Phase < CeresDestructionPhase.FlyingAwayFromExplosion
                ? CeresDestructionRomData.Vram.CeresSceneTilemapOffset : 0;
            new Mode7Vram(vram).LoadMap(ceresTilemaps.AsSpan(offset,
                CeresDestructionRomData.Vram.CeresSceneTilemapBytes));
            if (Phase >= CeresDestructionPhase.FlyingAwayFromExplosion)
                LoadFinalExplosionMaps();
        }
        else
        {
            new Mode7Vram(vram).LoadCharacters(value.CeresFlight.Mode7Characters.Span);
            new Mode7Vram(vram).LoadMap(ceresTilemaps.AsSpan(
                CeresDestructionRomData.Vram.MapHalfBytes,
                CeresDestructionRomData.Vram.MapHalfBytes));
            new CeresVram(vram).ZebesTilemap.Load(value.CeresDestruction.ZebesMap.Transfer.Span);
            new CeresVram(vram).ObjectCharacters.Load(value.CeresDestruction.ZebesCharacters.Transfer.Span);
        }
    }

    /// <summary>Joins installed Ceres flight and destruction maps into the native contiguous transfer source.</summary>
    private byte[] LoadCeresTilemaps()
    {
        IntroCinematicArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Ceres destruction requires installed cinematic artwork.");
        var maps = new byte[CeresDestructionRomData.Vram.CeresMinimumTilemapBytes];
        content.CeresFlight.Mode7Maps.Span.CopyTo(maps);
        content.CeresDestruction.CeresMaps.Span.CopyTo(
            maps.AsSpan(content.CeresFlight.Mode7Maps.Length));
        return maps;
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
        stationExplosion.StepFrame(bus);
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
                    explosionRepeatCountdown = CeresExplosionDefinitions.RepeatingCountdownSeed;
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
                    zoom = unchecked((ushort)(
                        zoom - CeresDestructionRomData.Motion.SlowScaleStep));
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
                    zoom = unchecked((ushort)(zoom + 4));
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
                    zoom = unchecked((ushort)(zoom + CeresDestructionRomData.Motion.SlowScaleStep));
                    angle = angle.AddTableUnits(-1);
                }
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesC:
                background = background.WithY(IntroCinematicMotion.AddSignedSixteenSixteen(background.Y, CeresDestructionRomData.Motion.EighthPixel16Point16));
                background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, CeresDestructionRomData.Motion.EighthPixel16Point16));
                if (zoom < CeresDestructionRomData.Motion.FarZebesScaleLimit)
                {
                    zoom = unchecked((ushort)(zoom + CeresDestructionRomData.Motion.FastScaleStep));
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
                paletteFx = new RoomPaletteFxSystem();
                paletteFx.SpawnDefinition(bus, CeresDestructionRomData.PaletteFx.EngineFlicker, 0);
            }
            paletteFx.Step(bus, cgram, paletteFxColors ?? throw new InvalidOperationException(
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
        byte[] characters = content.CeresFlight.Mode7Characters.ToArray();
        byte[] objectCharacters = content.CeresFlight.ObjectCharacters.ToArray();
        RequireMinimum(characters, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Ceres destruction Mode-7 characters");
        // The stream is four adjacent native work-RAM regions, not merely the two Ceres
        // screens used by this setup call. `$8B:C345` later consumes the front-gunship
        // screen at +$000 and the clear screen at +$C00 without decompressing again.
        RequireMinimum(ceresTilemaps, CeresDestructionRomData.Vram.CeresMinimumTilemapBytes,
            "Ceres cinematic tilemaps");
        RequireMinimum(objectCharacters, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Ceres cinematic OBJ characters");

        // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair. The
        // intro uses offsets zero and $300, so reusing either intro slice produces a
        // coherent but completely incorrect destruction shot.
        new Mode7Vram(vram).LoadCharacters(
            characters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));
        new Mode7Vram(vram).FillMap(
            CeresDestructionRomData.Vram.InitialMode7MapCharacter,
            CeresDestructionRomData.Vram.Mode7MapCapacityBytes);
        new Mode7Vram(vram).LoadMap(ceresTilemaps.AsSpan(
            CeresDestructionRomData.Vram.CeresSceneTilemapOffset,
            CeresDestructionRomData.Vram.CeresSceneTilemapBytes));
        new CeresVram(vram).ObjectCharacters.Load(
            objectCharacters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));

        // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard
        // cinematic sheet at $9A:D200. Preserve that overlap instead of displaying the
        // decompressed source's stale characters for explosion spritemaps.
        new CeresVram(vram).ObjectCharacters.Load(
            content.IntroObjectCharacters.Transfer.Span
                [..CeresDestructionRomData.Vram.SharedObjectCharacterBytes]);
        content.CeresFlight.Palette.LoadTo(cgram);

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
        ceresActorSlots.Add(actors[0], CeresDestructionRomData.Sprites.AsteroidSlot);
        ceresActorSlots.Add(actors[1], CeresDestructionRomData.Sprites.SmallAsteroidSlot);
        ceresActorSlots.Add(actors[2], CeresDestructionRomData.Sprites.VortexSlot);
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    /// <summary>Stages the planet-reveal Mode-1 map and resets camera, angle, fade, and mosaic state.</summary>
    private void SetupZebesReveal()
    {
        IntroCinematicArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Zebes reveal requires installed cinematic artwork.");
        byte[] zebesTilemap = content.CeresDestruction.ZebesMap.Transfer.ToArray();
        byte[] zebesCharacters = content.CeresDestruction.ZebesCharacters.Transfer.ToArray();
        RequireMinimum(zebesTilemap, CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes,
            "Zebes reveal tilemap");
        RequireMinimum(zebesCharacters, CeresDestructionRomData.Vram.Mode7CharacterBytes,
            "Zebes reveal characters");

        // C699 starts in Mode 1. It also stages the rear-gunship low-byte map at VRAM
        // zero; that dormant map becomes visible only when C7CA later selects Mode 7.
        new Mode7Vram(vram).LoadMap(ceresTilemaps.AsSpan(
            CeresDestructionRomData.Vram.MapHalfBytes,
            CeresDestructionRomData.Vram.MapHalfBytes));
        new CeresVram(vram).ZebesTilemap.Load(
            zebesTilemap.AsSpan(0, CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes));
        new CeresVram(vram).ObjectCharacters.Load(
            zebesCharacters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));

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
        brightness = (byte)Math.Min(15, brightness + 1);
    }

    /// <summary>Lowers brightness on alternating calls and reports the first fully black frame.</summary>
    private bool StepSlowFadeOut()
    {
        if (fadeCounter-- > 0)
            return false;
        fadeCounter = 1;
        brightness = (byte)Math.Max(0, brightness - 1);
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
        actor.PreInstructionPointerForDiscovery(definition.ActivePreInstruction);
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
    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
        {
            throw new InvalidDataException(
                $"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
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
                if (actor.PreInstructionPointer != definition.ActivePreInstruction)
                {
                    throw new InvalidDataException(
                        $"Ceres destruction actor {index} names invalid pre-instruction " +
                        $"$8B:{actor.PreInstructionPointer:X4}.");
                }
                if (definition.WrapX)
                    AddWrappedX(actor, unchecked((ushort)definition.HorizontalDelta));
            }
            else
                MoveExplosion(actor);

            actor.Step();
            if (!actor.IsActive)
            {
                ceresActorSlots.Remove(actor);
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
    private void LoadFinalExplosionMaps()
    {
        int upperSource = withSamus ? CeresDestructionRomData.Vram.ClearMapSourceOffset : 0;
        var mode7 = new Mode7Vram(vram);
        mode7.LoadMap(ceresTilemaps.AsSpan(upperSource, CeresDestructionRomData.Vram.MapHalfBytes));
        mode7.LoadMap(
            ceresTilemaps.AsSpan(
                CeresDestructionRomData.Vram.ClearMapSourceOffset,
                CeresDestructionRomData.Vram.MapHalfBytes),
            cell: CeresDestructionRomData.Vram.ClearMapDestinationWord);
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
        cinematicExplosion.PreInstructionPointerForDiscovery(
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
        actor.PreInstructionPointerForDiscovery(definition.PreInstruction);

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
            if (ceresActorSlots.ContainsValue(slot)) continue;
            ceresActorSlots.Add(actor, slot);
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
                CeresDestructionRomData.Motion.GunshipYVelocity,
                CeresDestructionRomData.Motion.GunshipYSubvelocity));
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
                if (definition.SlidePreInstruction == 0)
                {
                    throw new InvalidDataException(
                        "PLANET ZEBES title actor remained active when scene sliding began.");
                }
                actor.PreInstructionPointerForDiscovery(definition.SlidePreInstruction);
                // Zebes accelerates by $40 in 8.8; all four star sheets use $20. Star
                // sheet five is the native completion owner at C8F2. Identify the actors
                // by ownership, not their mutable list index: native actors delete as they
                // cross -$80, so indexes necessarily shift during this loop.
                ushort acceleration = definition.SlideAcceleration;
                actor.GeneralTimer = unchecked((ushort)(actor.GeneralTimer + acceleration));
                SubtractEightEightY(actor, actor.GeneralTimer);
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
        OamBuffer oam = PrepareRenderOam();
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
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers,
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    /// <summary>Captures the active station blast's scanline color addition as a render layer.</summary>
    private ScanlineColorAddRenderLayer? CaptureStationExplosion() =>
        SnesGameplayFrameRenderer.CapturePowerBombColorMath(PowerBombRenderSnapshot.Capture(stationExplosion),
            layer1: Position16<RoomPixels>.Zero, firstVisibleScanline: 0);

    /// <summary>Draws live actors into OAM, retaining native slot order when Ceres explosions recycle slots.</summary>
    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        // Same-priority OBJ overlap is decided by OAM order. Allocation order is
        // not stable slot order after an earlier explosion dies and its slot is reused.
        IEnumerable<IntroDiscoverySprite> drawActors = Phase <= CeresDestructionPhase.FadeOutCeres
            ? actors.OrderByDescending(actor => ceresActorSlots[actor]) : actors;
        foreach (IntroDiscoverySprite actor in drawActors)
            actor.Draw(oam, installedArt: spriteArtwork);
        oam.FinalizeFrame();
        return oam;
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
    internal static short ReadSine(byte index) => EnemyTrigonometryTables.SignedSine(index);

    /// <summary>Applies native 8.8 matrix scaling with signed 16-bit truncation.</summary>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, (short)scalar)));
}
