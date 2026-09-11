using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed implementation of game state <c>$22</c>: Ceres explodes and the
/// camera subsequently follows Samus's gunship toward Zebes.
/// </summary>
/// <remarks>
/// Bank <c>$8B:C11B-$CADF</c> owns this sequence. The state deliberately retains the
/// native phase boundaries, counters, 16.16 camera motion, Mode-7 matrix values, and ROM
/// sprite lists. That makes a debugger watch useful: there is no host-authored video or
/// timer that merely happens to end at the same gameplay room.
/// </remarks>
internal sealed partial class CeresDestructionCinematicState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly byte[] ceresTilemaps;
    private readonly List<IntroDiscoverySprite> actors = [];
    private readonly SamusPowerBombExplosionState stationExplosion = new();
    private IntroDiscoverySprite? zebesPlanetActor;
    private IntroDiscoverySprite? zebesCompletionStarActor;

    private ushort backgroundX = unchecked((ushort)-44);
    private ushort backgroundXSubPosition;
    private ushort backgroundY = unchecked((ushort)-112);
    private ushort backgroundYSubPosition;
    private ushort zoom = CeresDestructionRomData.Motion.IdentityScale;
    private SnesAngle angle;
    private byte brightness;
    private int fadeCounter = 1;
    private int phaseTimer;
    private int musicQueueTimer = 14;
    private ushort cinematicFrameCounter;
    private int explosionSpawnerFrame;
    private int explosionOffsetIndex;
    private bool usesMode7 = true;
    private SnesMainScreenLayers mainScreenLayers = SnesMainScreenLayers.Bg1 | SnesMainScreenLayers.Obj;

    public CeresDestructionCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        // State $25 selects the common cinematic bank and destruction track eight.
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(
            MusicCommand.LoadData(CeresDestructionRomData.Music.CeresDataIndex));
        audio?.QueueMusicDelayed(
            MusicCommand.SelectTrack(CeresDestructionRomData.Music.CeresTrack),
            MusicCommandDelay.FromDelayedYArgument(CeresDestructionRomData.Music.DelayArgument));
        ceresTilemaps = RomDataReader.Decompress(
            bus,
            CeresDestructionRomData.Assets.CeresTilemaps,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        SetupCeresDestruction();
    }

    public CeresDestructionPhase Phase { get; private set; }

    public bool Finished => Phase == CeresDestructionPhase.Finished;

    public ushort Zoom => zoom;

    public byte Brightness => brightness;

    public ushort BackgroundX => backgroundX;

    public ushort BackgroundY => backgroundY;

    internal int ActiveActorCount => actors.Count;

    /// <summary>
    /// Reads the low-byte Mode-7 map selected by the cinematic's current native transfers.
    /// </summary>
    internal byte ReadMode7MapByte(int mapByteIndex)
    {
        if ((uint)mapByteIndex >= CeresDestructionRomData.Vram.Mode7MapCapacityBytes)
            throw new ArgumentOutOfRangeException(nameof(mapByteIndex));
        return vram.ReadByte(mapByteIndex * 2);
    }

    /// <summary>Executes one call through the native state-$22 cinematic dispatcher.</summary>
    public void Step()
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

            case CeresDestructionPhase.WaitForZebesMusicQueue:
                if (MusicQueueFinished())
                    Phase = CeresDestructionPhase.FadeInZebes;
                break;

            case CeresDestructionPhase.FadeInAndDrift:
                StepInitialDrift();
                zoom++;
                StepSlowFadeIn();
                if (brightness == 15)
                    Phase = CeresDestructionPhase.ApproachExplosion;
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
                if (StepSlowFadeOut())
                    SetupZebesReveal();
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

        if (Phase <= CeresDestructionPhase.FadeOutCeres)
            StepCeresActors();

        // GameState_37 increments the shared cinematic frame word after calling the
        // current function but before object handling. The mosaic test therefore observes
        // this pre-increment value on its following dispatcher call.
        cinematicFrameCounter++;
    }

    private void SetupCeresDestruction()
    {
        byte[] characters = RomDataReader.Decompress(
            bus,
            CeresDestructionRomData.Assets.Mode7Characters,
            maximumOutputBytes: CeresDestructionRomData.Vram.Mode7CharacterBytes);
        byte[] objectCharacters = RomDataReader.Decompress(
            bus,
            CeresDestructionRomData.Assets.CeresObjectCharacters,
            maximumOutputBytes: CeresDestructionRomData.Vram.Mode7CharacterBytes);
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
            ReadBusBytes(
                CeresDestructionRomData.Assets.SharedObjectCharacters,
                CeresDestructionRomData.Vram.SharedObjectCharacterBytes));
        cgram.LoadFromBus(bus, CeresDestructionRomData.Assets.Palette);

        actors.Clear();
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.InitialAsteroids));
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.InitialSmallAsteroids));
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.InitialVortex));
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    private void SetupZebesReveal()
    {
        byte[] zebesTilemap = RomDataReader.Decompress(
            bus,
            CeresDestructionRomData.Assets.ZebesTilemap,
            maximumOutputBytes: CeresDestructionRomData.Vram.CompressedTilemapLimit);
        byte[] zebesCharacters = RomDataReader.Decompress(
            bus,
            CeresDestructionRomData.Assets.ZebesCharacters,
            maximumOutputBytes: CeresDestructionRomData.Vram.Mode7CharacterBytes);
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
        // The Ceres/Zebes interstitial at $8B:D6D7 has its own bank-$33 data set and
        // waits for all three commands before beginning the mosaic fade.
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(
            MusicCommand.LoadData(CeresDestructionRomData.Music.ZebesDataIndex));
        audio?.QueueMusicDelayed(
            MusicCommand.SelectTrack(CeresDestructionRomData.Music.ZebesTrack),
            MusicCommandDelay.FromDelayedYArgument(CeresDestructionRomData.Music.DelayArgument));
        musicQueueTimer = 14;
        Phase = CeresDestructionPhase.WaitForZebesMusicQueue;
    }

    private void SetupZebesMode7Actors()
    {
        usesMode7 = true;
        backgroundX = CeresDestructionRomData.Sprites.ZebesInitialBackgroundX;
        backgroundY = unchecked((ushort)-104);
        angle = CeresDestructionRomData.Motion.ApproachAngle;
        zoom = CeresDestructionRomData.Motion.IdentityScale;
        actors.Clear();
        zebesPlanetActor = CreateActor(CeresDestructionRomData.Sprites.ZebesPlanet);
        actors.Add(zebesPlanetActor);
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.UpperLeftStar));
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.UpperRightStar));
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.LowerLeftStar));
        zebesCompletionStarActor = CreateActor(CeresDestructionRomData.Sprites.LowerRightStar);
        actors.Add(zebesCompletionStarActor);
        actors.Add(CreateActor(CeresDestructionRomData.Sprites.PlanetTitle));
        Phase = CeresDestructionPhase.PlanetZebesTitle;
    }

    private void StepInitialDrift()
    {
        AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition,
            CeresDestructionRomData.Motion.SixteenthPixel16Point16);
        AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition,
            CeresDestructionRomData.Motion.NegativeQuarterPixel16Point16);
    }

    private void StepSlowFadeIn()
    {
        if (fadeCounter-- > 0)
            return;
        fadeCounter = 1;
        brightness = (byte)Math.Min(15, brightness + 1);
    }

    private bool StepSlowFadeOut()
    {
        if (fadeCounter-- > 0)
            return false;
        fadeCounter = 1;
        brightness = (byte)Math.Max(0, brightness - 1);
        return brightness == 0;
    }

    private bool StepZebesMosaic()
    {
        if ((cinematicFrameCounter & 3) != 0)
            return false;
        phaseTimer = unchecked((byte)(
            phaseTimer - CeresDestructionRomData.Timing.MosaicFadeStep));
        return (phaseTimer & CeresDestructionRomData.Timing.MosaicSizeMask) == 0;
    }

    private bool MusicQueueFinished() =>
        audio is null ? --musicQueueTimer <= 0 : !audio.HasQueuedMusic;

    private byte[] ReadBusBytes(int address, int count)
    {
        var result = new byte[count];
        for (int index = 0; index < result.Length; index++)
            result[index] = bus.ReadByte(address + index);
        return result;
    }

    private static IntroDiscoverySprite CreateActor(CeresCinematicActorDefinition definition) =>
        new(definition.X, definition.Y, definition.PaletteBits, definition.InstructionPointer);

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

    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
        {
            throw new InvalidDataException(
                $"The {name} stream expanded to ${bytes.Length:X}, expected at least ${minimum:X}.");
        }
    }
}

internal enum CeresDestructionPhase
{
    WaitForMusicQueue,
    FadeInAndDrift,
    ApproachExplosion,
    FlyingAwayFromExplosion,
    HoldAfterExplosion,
    FadeOutCeres,
    WaitForZebesMusicQueue,
    FadeInZebes,
    RemoveZebesMosaic,
    PlanetZebesTitle,
    FlyingTowardZebesA,
    FlyingTowardZebesB,
    FlyingTowardZebesC,
    HoldCloseZebes,
    SlideZebesSceneAway,
    Finished,
}
