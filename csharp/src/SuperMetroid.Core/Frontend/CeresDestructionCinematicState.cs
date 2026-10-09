using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

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
internal sealed partial class CeresDestructionCinematicState
{
    /// <summary>Cartridge address space used by actors and the room palette effect.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Optional audio queue whose pending commands gate the scene transitions.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Scene-local video memory populated by the cinematic's asset transfers.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Scene-local color memory used by the palette fade and rendered layers.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Combined Mode 7 maps for the Ceres approach and destruction shots.</summary>
    private byte[] ceresTilemaps;
    /// <summary>Host-owned scene assets, rebound separately after restoring debugger state.</summary>
    [NonSerialized] private IntroCinematicArtworkCatalog? artwork;
    /// <summary>Host-owned sprite-list presentation used to resolve cinematic actor graphics.</summary>
    [NonSerialized] private IIntroCinematicSpritePresentation? spriteArtwork;
    /// <summary>Actors currently owned by the Ceres destruction or Zebes reveal scene.</summary>
    private readonly List<IntroDiscoverySprite> actors = [];
    /// <summary>Maps the initial Ceres actors to their fixed sprite presentation slots.</summary>
    private readonly Dictionary<IntroDiscoverySprite, int> ceresActorSlots = [];
    /// <summary>Explosion state for the space station's final power-bomb blast.</summary>
    private readonly SamusPowerBombExplosionState stationExplosion = new();
    /// <summary>Engine flicker palette program active during the Zebes portion.</summary>
    private RoomPaletteFxSystem? paletteFx;
    /// <summary>Host-owned colors needed to execute the engine flicker palette program.</summary>
    [NonSerialized] private IPaletteFxColorSource? paletteFxColors;
    /// <summary>Zebes planet actor whose position controls the later approach phases.</summary>
    private IntroDiscoverySprite? zebesPlanetActor;
    /// <summary>Completion star actor hidden until the planet title has finished.</summary>
    private IntroDiscoverySprite? zebesCompletionStarActor;
    /// <summary>Planet title actor removed when the reveal transitions into the approach.</summary>
    private IntroDiscoverySprite? zebesTitleActor;

    /// <summary>16-bit whole-pixel component of the Mode 7 horizontal camera position.</summary>
    private ushort backgroundX = unchecked((ushort)-44);
    /// <summary>Fractional component paired with <see cref="backgroundX"/> for 16.16 movement.</summary>
    private ushort backgroundXSubPosition;
    /// <summary>16-bit whole-pixel component of the Mode 7 vertical camera position.</summary>
    private ushort backgroundY = unchecked((ushort)-112);
    /// <summary>Fractional component paired with <see cref="backgroundY"/> for 16.16 movement.</summary>
    private ushort backgroundYSubPosition;
    /// <summary>Mode 7 scale used by the Ceres and Zebes camera sequences.</summary>
    private ushort zoom = CeresDestructionRomData.Motion.IdentityScale;
    /// <summary>Mode 7 rotation advanced by the authored cinematic phases.</summary>
    private SnesAngle angle;
    /// <summary>Master display brightness in the SNES range from zero through fifteen.</summary>
    private byte brightness;
    /// <summary>Alternating-update divider used by the slow brightness ramps.</summary>
    private int fadeCounter = 1;
    /// <summary>Authored hold duration for explosion and close-Zebes scenes.</summary>
    private int phaseTimer;
    /// <summary>Fallback queue countdown used only when no host audio state is supplied.</summary>
    private int musicQueueTimer = 14;
    // $8B:C11B's NMI waits completed so far; setup runs after the last one.
    private int initialNmiWaits;
    /// <summary>Shared cinematic update word used to time explosion and mosaic effects.</summary>
    private ushort cinematicFrameCounter;
    /// <summary>Elapsed frame count used to schedule the repeated Ceres explosion spawner.</summary>
    private int explosionSpawnerFrame;
    /// <summary>Index into the authored sequence of offsets for repeated explosion effects.</summary>
    private int explosionOffsetIndex;
    /// <summary>Frames remaining before the next repeated explosion is spawned.</summary>
    private ushort explosionRepeatCountdown;
    /// <summary>Whether the active scene is rendered through the Mode 7 background path.</summary>
    private bool usesMode7 = true;
    /// <summary>PPU main-screen planes enabled for the current cinematic scene.</summary>
    private SnesMainScreenLayers mainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;

    /// <summary>Creates the Ceres destruction sequence and installs its initial graphics and actors.</summary>
    /// <param name="bus">Cartridge address space used by the scene's actors and palette program.</param>
    /// <param name="audio">Optional audio state used to observe the actual queued music commands.</param>
    /// <param name="fixedColors">Optional host palette colors used by the station explosion.</param>
    /// <param name="artwork">Installed Ceres and Zebes graphics and tilemap assets.</param>
    /// <param name="paletteFxColors">Optional installed colors for the later engine-flicker effect.</param>
    public CeresDestructionCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null,
        PowerBombFixedColorCatalog? fixedColors = null,
        IntroCinematicArtworkCatalog? artwork = null,
        IPaletteFxColorSource? paletteFxColors = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
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
    /// retain it for lazy owner creation and older debugger-state reconstruction.
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
            vram.LoadMode7CharacterBytes(value.CeresFlight.Mode7Characters.Span);
            vram.LoadBytes(CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
                value.CeresFlight.ObjectCharacters.Span);
            vram.LoadBytes(CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
                value.IntroObjectCharacters.Transfer.Span[..CeresDestructionRomData.Vram.SharedObjectCharacterBytes]);
            int offset = Phase < CeresDestructionPhase.FlyingAwayFromExplosion
                ? CeresDestructionRomData.Vram.CeresSceneTilemapOffset : 0;
            vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(offset,
                CeresDestructionRomData.Vram.CeresSceneTilemapBytes));
            if (Phase >= CeresDestructionPhase.FlyingAwayFromExplosion)
                vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(
                    CeresDestructionRomData.Vram.ClearMapSourceOffset,
                    CeresDestructionRomData.Vram.MapHalfBytes),
                    CeresDestructionRomData.Vram.ClearMapDestinationWord);
        }
        else
        {
            vram.LoadMode7CharacterBytes(value.CeresFlight.Mode7Characters.Span);
            vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(
                CeresDestructionRomData.Vram.MapHalfBytes,
                CeresDestructionRomData.Vram.MapHalfBytes));
            vram.LoadBytes(CeresDestructionRomData.Vram.ZebesTilemapDestinationByte,
                value.CeresDestruction.ZebesMap.Transfer.Span);
            vram.LoadBytes(CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
                value.CeresDestruction.ZebesCharacters.Transfer.Span);
        }
    }

    /// <summary>Combines flight and destruction map assets into the staging layout expected by VRAM uploads.</summary>
    /// <returns>Contiguous Ceres tilemap bytes used by phase-specific transfers.</returns>
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

    /// <summary>Current native-equivalent phase of the Ceres and Zebes sequence.</summary>
    public CeresDestructionPhase Phase { get; private set; }

    /// <summary>Whether the final Zebes pan has completed and control can leave this cinematic.</summary>
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
            // $8B:C2B8-$C2DF queue the cinematic bank and state $22's track last.
            audio?.QueueMusicDelayed8(MusicCommand.Stop);
            audio?.QueueMusicDelayed8(
                MusicCommand.LoadData(CeresDestructionRomData.Music.CeresDataIndex));
            audio?.QueueMusicDelayed(
                MusicCommand.SelectTrack(CeresDestructionRomData.Music.CeresTrack),
                MusicCommandDelay.FromDelayedYArgument(CeresDestructionRomData.Music.DelayArgument));
        }
        else
        {
            StepPhase();
        }
        StepWrapperTail();
    }

    /// <summary>Advances the active scene phase, including actors, camera motion, and palette counters.</summary>
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
                backgroundX = unchecked((ushort)(backgroundX + 2));
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
                if (StepSlowFadeOut())
                    Phase = CeresDestructionPhase.FlyToZebesInitial;
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
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition,
                    CeresDestructionRomData.Motion.EighthPixel16Point16);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition,
                    CeresDestructionRomData.Motion.NegativeHalfPixel16Point16);
                if (zoom < CeresDestructionRomData.Motion.ZebesApproachScaleLimit)
                    zoom = unchecked((ushort)(zoom + 4));
                else
                    Phase = CeresDestructionPhase.FlyingTowardZebesB;
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesB:
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition,
                    CeresDestructionRomData.Motion.EighthPixel16Point16);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition,
                    CeresDestructionRomData.Motion.NegativeHalfPixel16Point16);
                if (unchecked((short)backgroundX) < -128)
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
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition,
                    CeresDestructionRomData.Motion.EighthPixel16Point16);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition,
                    CeresDestructionRomData.Motion.EighthPixel16Point16);
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
        }
    }

    /// <summary>The state-$22 wrapper's work after the cinematic function returns ($8B:A367).</summary>
    private void StepWrapperTail()
    {
        if (Phase <= CeresDestructionPhase.FadeOutCeres)
            StepCeresActors();

        // GameState_37 runs PaletteFxHandler after cinematic objects, including the
        // frame which spawns the program. Older debugger states lack this owner;
        // resume their visible effect rather than leaving the engines permanently lit.
        if (Phase > CeresDestructionPhase.FlyToZebesInitial)
        {
            if (paletteFx is null)
            {
                paletteFx = new RoomPaletteFxSystem();
                paletteFx.SpawnDefinition(bus, CeresDestructionRomData.PaletteFx.EngineFlicker, 0);
            }
            paletteFx.Step(bus, cgram,
                paletteFxColors ?? throw new InvalidOperationException(
                    "Ceres palette FX requires installed palette colors."), 0, 0, false, false);
            if (paletteFx.SoundRequests.Count != 0 || paletteFx.MusicRequests.Count != 0)
                throw new InvalidDataException("Ceres engine palette program unexpectedly requested audio.");
        }

        // GameState_37 increments the shared cinematic frame word after calling the
        // current function but before object handling. The mosaic test therefore observes
        // this pre-increment value on its following dispatcher call.
        cinematicFrameCounter++;
    }

    /// <summary>Loads the initial Ceres maps and actors, then starts the music-queue wait phase.</summary>
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
        vram.LoadMode7CharacterBytes(
            characters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));
        vram.FillMode7MapBytes(
            CeresDestructionRomData.Vram.InitialMode7MapCharacter,
            CeresDestructionRomData.Vram.Mode7MapCapacityBytes);
        vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(
            CeresDestructionRomData.Vram.CeresSceneTilemapOffset,
            CeresDestructionRomData.Vram.CeresSceneTilemapBytes));
        vram.LoadBytes(
            CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
            objectCharacters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));

        // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard
        // cinematic sheet at $9A:D200. Preserve that overlap instead of displaying the
        // decompressed source's stale characters for explosion spritemaps.
        vram.LoadBytes(
            CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
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
                definition = definition with
                {
                    X = checked((ushort)placement.X),
                    Y = checked((ushort)placement.Y),
                };
            }
            actors.Add(CreateActor(definition));
        }
        ceresActorSlots.Clear();
        ceresActorSlots.Add(actors[0], CeresDestructionRomData.Sprites.AsteroidSlot);
        ceresActorSlots.Add(actors[1], CeresDestructionRomData.Sprites.SmallAsteroidSlot);
        ceresActorSlots.Add(actors[2], CeresDestructionRomData.Sprites.VortexSlot);
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    /// <summary>Loads Zebes reveal graphics and resets the camera and fade state for its mosaic entrance.</summary>
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
        vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(
            CeresDestructionRomData.Vram.MapHalfBytes,
            CeresDestructionRomData.Vram.MapHalfBytes));
        vram.LoadBytes(
            CeresDestructionRomData.Vram.ZebesTilemapDestinationByte,
            zebesTilemap.AsSpan(0, CeresDestructionRomData.Vram.ZebesTilemapMinimumBytes));
        vram.LoadBytes(
            CeresDestructionRomData.Vram.ObjectCharacterDestinationByte,
            zebesCharacters.AsSpan(0, CeresDestructionRomData.Vram.Mode7CharacterBytes));

        actors.Clear();
        backgroundX = 0;
        backgroundXSubPosition = 0;
        backgroundY = 0;
        backgroundYSubPosition = 0;
        angle = SnesAngle.Zero;
        zoom = CeresDestructionRomData.Motion.IdentityScale;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = CeresDestructionRomData.Timing.InitialMosaicRegister;
        usesMode7 = false;
    }

    /// <summary>Enters the Mode 7 title scene and creates the planet, decoration, star, and title actors.</summary>
    private void SetupZebesMode7Actors()
    {
        usesMode7 = true;
        backgroundX = CeresDestructionRomData.Sprites.ZebesInitialBackgroundX;
        backgroundY = unchecked((ushort)-104);
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

    /// <summary>Applies the authored slow 16.16 camera drift during the opening Ceres approach.</summary>
    private void StepInitialDrift()
    {
        AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition,
            CeresDestructionRomData.Motion.SixteenthPixel16Point16);
        AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition,
            CeresDestructionRomData.Motion.NegativeQuarterPixel16Point16);
    }

    /// <summary>Raises master brightness by one every other update until full brightness.</summary>
    private void StepSlowFadeIn()
    {
        if (fadeCounter-- > 0)
            return;
        fadeCounter = 1;
        brightness = (byte)Math.Min(15, brightness + 1);
    }

    /// <summary>Lowers master brightness by one every other update until forced black.</summary>
    /// <returns><see langword="true"/> when brightness reaches zero.</returns>
    private bool StepSlowFadeOut()
    {
        if (fadeCounter-- > 0)
            return false;
        fadeCounter = 1;
        brightness = (byte)Math.Max(0, brightness - 1);
        return brightness == 0;
    }

    /// <summary>Reduces the mosaic register on every fourth update until the effect is cleared.</summary>
    /// <returns><see langword="true"/> when no mosaic-size bits remain.</returns>
    private bool StepZebesMosaic()
    {
        if ((cinematicFrameCounter & 3) != 0)
            return false;
        phaseTimer = unchecked((byte)(
            phaseTimer - CeresDestructionRomData.Timing.MosaicFadeStep));
        return (phaseTimer & CeresDestructionRomData.Timing.MosaicSizeMask) == 0;
    }

    /// <summary>Reports completion of queued music, using a local timer only without host audio.</summary>
    /// <returns><see langword="true"/> when the opening track commands have finished queuing.</returns>
    private bool MusicQueueFinished() =>
        audio is null ? --musicQueueTimer <= 0 : !audio.HasQueuedMusic;

    /// <summary>Constructs an actor from its cartridge definition and initializes its active pre-instruction.</summary>
    /// <param name="definition">Position, attributes, instruction list, and pre-instruction for the actor.</param>
    /// <returns>The initialized scene actor.</returns>
    private static IntroDiscoverySprite CreateActor(
        CeresDestructionActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.X,
            definition.Y,
            definition.Attributes,
            definition.InstructionList);
        actor.PreInstructionPointerForDiscovery(definition.ActivePreInstruction);
        return actor;
    }

    /// <summary>Creates a Zebes actor from its indexed definition and optional host placement.</summary>
    /// <param name="index">Actor index in the Zebes reveal definition table.</param>
    /// <returns>The initialized actor with any installed placement override applied.</returns>
    private IntroDiscoverySprite CreateZebesActor(int index)
    {
        CeresDestructionActorDefinition definition =
            CeresDestructionActorDefinitions.ZebesActor(index);
        if (artwork?.CeresDestruction.RevealActors is { } layout)
        {
            CeresRevealActorPlacement placement = layout[index];
            definition = definition with
            {
                X = checked((ushort)placement.X),
                Y = checked((ushort)placement.Y),
            };
        }
        return CreateActor(definition);
    }

    /// <summary>Adds a signed fixed-point delta to the split whole and fractional camera words.</summary>
    /// <param name="whole">Whole-pixel word updated in place.</param>
    /// <param name="sub">Fractional word updated in place with carry or borrow.</param>
    /// <param name="fixedDelta">Signed 16.16 movement amount.</param>
    private static void AddSignedSixteenSixteen(
        ref ushort whole,
        ref ushort sub,
        int fixedDelta)
    {
        IntroCinematicMotion.AddSixteenSixteen(
            ref whole,
            ref sub,
            unchecked((ushort)(fixedDelta >> 16)),
            unchecked((ushort)fixedDelta));
    }

    /// <summary>Rejects an extracted asset stream shorter than the bytes consumed by the cinematic.</summary>
    /// <param name="bytes">Expanded asset data to validate.</param>
    /// <param name="minimum">Minimum required byte count.</param>
    /// <param name="name">Asset description included in the invalid-data error.</param>
    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
        {
            throw new InvalidDataException(
                $"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
        }
    }
}

/// <summary>Dispatch states for the Ceres destruction, Zebes reveal, and approach sequence.</summary>
internal enum CeresDestructionPhase
{
    /// <summary>Waits until the initial music commands have entered the audio queue.</summary>
    WaitForMusicQueue,
    /// <summary>Fades in while the camera drifts toward the Ceres explosion.</summary>
    FadeInAndDrift,
    /// <summary>Continues the Ceres approach until the final explosion scale is reached.</summary>
    ApproachExplosion,
    /// <summary>Moves away from the blast while rotating and shrinking the Ceres view.</summary>
    FlyingAwayFromExplosion,
    /// <summary>Holds on the blast before fading out the Ceres scene.</summary>
    HoldAfterExplosion,
    /// <summary>Fades Ceres to black before installing the Zebes reveal.</summary>
    FadeOutCeres,
    /// <summary>Initializes the Zebes scene after the Ceres fade has completed.</summary>
    FlyToZebesInitial,
    /// <summary>Fades in Zebes while progressively removing its mosaic.</summary>
    FadeInZebes,
    /// <summary>Finishes the mosaic reveal and creates the Mode 7 Zebes actors.</summary>
    RemoveZebesMosaic,
    /// <summary>Displays the planet title before beginning the approach camera motion.</summary>
    PlanetZebesTitle,
    /// <summary>First authored camera-motion segment toward Zebes.</summary>
    FlyingTowardZebesA,
    /// <summary>Middle camera-motion segment, including the rotation transition.</summary>
    FlyingTowardZebesB,
    /// <summary>Final camera-motion segment before the close Zebes hold.</summary>
    FlyingTowardZebesC,
    /// <summary>Holds the close Zebes view before sliding the scene away.</summary>
    HoldCloseZebes,
    /// <summary>Moves the Zebes actors offscreen and completes the cinematic.</summary>
    SlideZebesSceneAway,
    /// <summary>Terminal state reached after the Zebes scene has left the display.</summary>
    Finished,
}
