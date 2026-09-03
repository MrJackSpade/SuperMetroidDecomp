using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
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
    private const int PaletteAddress = 0x8ce5e9;
    private const int Mode7CharacterAddress = 0x95a82f;
    private const int CeresTilemapAddress = 0x96fe69;
    private const int CeresObjectCharacterAddress = 0x96d10a;
    private const int ZebesTilemapAddress = 0x978adb;
    private const int ZebesCharacterAddress = 0x96ec76;
    private const int SignedSineTableAddress = 0xa0b443;

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly byte[] ceresTilemaps;
    private readonly List<IntroDiscoverySprite> actors = [];
    private IntroDiscoverySprite? zebesPlanetActor;
    private IntroDiscoverySprite? zebesCompletionStarActor;

    private ushort backgroundX = unchecked((ushort)-44);
    private ushort backgroundXSubPosition;
    private ushort backgroundY = unchecked((ushort)-112);
    private ushort backgroundYSubPosition;
    private ushort zoom = 0x0100;
    private byte angle;
    private byte brightness;
    private int fadeCounter = 1;
    private int phaseTimer;
    private int musicQueueTimer = 14;
    private ushort cinematicFrameCounter;
    private int explosionSpawnerFrame;
    private int explosionOffsetIndex;
    private bool usesMode7 = true;

    public CeresDestructionCinematicState(
        ISnesAddressSpace bus,
        CartridgeAudioState? audio = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio;
        // State $25 selects the common cinematic bank and destruction track eight.
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(MusicCommand.LoadData(0x2d));
        audio?.QueueMusicDelayed(MusicCommand.SelectTrack(8), MusicCommandDelay.FromDelayedYArgument(0x0e));
        ceresTilemaps = RomDataReader.Decompress(bus, CeresTilemapAddress, maximumOutputBytes: 0x1000);
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
        if ((uint)mapByteIndex >= 0x4000)
            throw new ArgumentOutOfRangeException(nameof(mapByteIndex));
        return vram.ReadByte(mapByteIndex * 2);
    }

    /// <summary>Executes one call through the native state-$22 cinematic dispatcher.</summary>
    public void Step()
    {
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
                StepSlowFadeIn();
                if (brightness == 15)
                    Phase = CeresDestructionPhase.ApproachExplosion;
                break;

            case CeresDestructionPhase.ApproachExplosion:
                StepInitialDrift();
                if (zoom < 0x0280)
                {
                    zoom++;
                }
                else
                {
                    // `$8B:C345` snaps the scale to $0300, creates the final blast, and
                    // installs the flying-away function on this exact handler call.
                    zoom = 0x0300;
                    SpawnFinalCeresExplosion();
                    Phase = CeresDestructionPhase.FlyingAwayFromExplosion;
                }
                break;

            case CeresDestructionPhase.FlyingAwayFromExplosion:
                backgroundX = unchecked((ushort)(backgroundX + 2));
                angle = unchecked((byte)(angle - 1));
                if (zoom < 0x0010)
                {
                    phaseTimer = 0x00c0;
                    Phase = CeresDestructionPhase.HoldAfterExplosion;
                }
                else
                {
                    zoom = unchecked((ushort)(zoom - 0x0010));
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
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition, 0x0000_2000);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition, unchecked((int)0xffff_8000));
                if (zoom < 0x0480)
                    zoom = unchecked((ushort)(zoom + 4));
                else
                    Phase = CeresDestructionPhase.FlyingTowardZebesB;
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesB:
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition, 0x0000_2000);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition, unchecked((int)0xffff_8000));
                if (unchecked((short)backgroundX) < -128)
                {
                    Phase = CeresDestructionPhase.FlyingTowardZebesC;
                }
                else
                {
                    zoom = unchecked((ushort)(zoom + 0x0010));
                    angle = unchecked((byte)(angle - 1));
                }
                StepZebesActors(slidingAway: false);
                break;

            case CeresDestructionPhase.FlyingTowardZebesC:
                AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition, 0x0000_2000);
                AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition, 0x0000_2000);
                if (zoom < 0x2000)
                {
                    zoom = unchecked((ushort)(zoom + 0x0020));
                }
                else
                {
                    phaseTimer = 0x0040;
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
        byte[] characters = RomDataReader.Decompress(bus, Mode7CharacterAddress, maximumOutputBytes: 0x4000);
        byte[] objectCharacters = RomDataReader.Decompress(bus, CeresObjectCharacterAddress, maximumOutputBytes: 0x4000);
        RequireMinimum(characters, 0x4000, "Ceres destruction Mode-7 characters");
        // The stream is four adjacent native work-RAM regions, not merely the two Ceres
        // screens used by this setup call. `$8B:C345` later consumes the front-gunship
        // screen at +$000 and the clear screen at +$C00 without decompressing again.
        RequireMinimum(ceresTilemaps, 0x0f00, "Ceres cinematic tilemaps");
        RequireMinimum(objectCharacters, 0x4000, "Ceres cinematic OBJ characters");

        // C11B selects bytes $600-$BFF: the third/fourth 24-row Ceres map pair. The
        // intro uses offsets zero and $300, so reusing either intro slice produces a
        // coherent but completely incorrect destruction shot.
        vram.LoadMode7CharacterBytes(characters.AsSpan(0, 0x4000));
        vram.FillMode7MapBytes(0x8c, 0x4000);
        vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(0x0600, 0x0600));
        vram.LoadBytes(0xc000, objectCharacters.AsSpan(0, 0x4000));

        // The final C11B DMA overwrites the first $1A00 OBJ bytes with the standard
        // cinematic sheet at $9A:D200. Preserve that overlap instead of displaying the
        // decompressed source's stale characters for explosion spritemaps.
        vram.LoadBytes(0xc000, ReadBusBytes(0x9ad200, 0x1a00));
        cgram.LoadFromBus(bus, PaletteAddress);

        actors.Clear();
        actors.Add(new IntroDiscoverySprite(0x0050, 0x009f, 0x0800, 0xcc3f));
        actors.Add(new IntroDiscoverySprite(0x0080, 0x0060, 0x0800, 0xcc4f));
        actors.Add(new IntroDiscoverySprite(0x0070, 0x0057, 0x0800, 0xcc57));
        Phase = CeresDestructionPhase.WaitForMusicQueue;
    }

    private void SetupZebesReveal()
    {
        byte[] zebesTilemap = RomDataReader.Decompress(bus, ZebesTilemapAddress, maximumOutputBytes: 0x1000);
        byte[] zebesCharacters = RomDataReader.Decompress(bus, ZebesCharacterAddress, maximumOutputBytes: 0x4000);
        RequireMinimum(zebesTilemap, 0x0800, "Zebes reveal tilemap");
        RequireMinimum(zebesCharacters, 0x4000, "Zebes reveal characters");

        // C699 starts in Mode 1. It also stages the rear-gunship low-byte map at VRAM
        // zero; that dormant map becomes visible only when C7CA later selects Mode 7.
        vram.LoadMode7MapBytes(ceresTilemaps.AsSpan(0x0300, 0x0300));
        vram.LoadBytes(0xb800, zebesTilemap.AsSpan(0, 0x0800));
        vram.LoadBytes(0xc000, zebesCharacters.AsSpan(0, 0x4000));

        actors.Clear();
        backgroundX = 0;
        backgroundXSubPosition = 0;
        backgroundY = 0;
        backgroundYSubPosition = 0;
        angle = 0;
        zoom = 0x0100;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = 0x81; // The complete MOSAIC register, including BG-enable bit zero.
        usesMode7 = false;
        // The Ceres/Zebes interstitial at $8B:D6D7 has its own bank-$33 data set and
        // waits for all three commands before beginning the mosaic fade.
        audio?.QueueMusicDelayed8(MusicCommand.Stop);
        audio?.QueueMusicDelayed8(MusicCommand.LoadData(0x33));
        audio?.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x0e));
        musicQueueTimer = 14;
        Phase = CeresDestructionPhase.WaitForZebesMusicQueue;
    }

    private void SetupZebesMode7Actors()
    {
        usesMode7 = true;
        backgroundX = 0x0080;
        backgroundY = unchecked((ushort)-104);
        angle = 0x20;
        zoom = 0x0100;
        actors.Clear();
        zebesPlanetActor = new IntroDiscoverySprite(0x0088, 0x006f, 0x0e00, 0xccab);
        actors.Add(zebesPlanetActor);
        actors.Add(new IntroDiscoverySprite(0x0030, 0x002f, 0x0800, 0xcd83));
        actors.Add(new IntroDiscoverySprite(0x00d0, 0x002f, 0x0800, 0xcd8b));
        actors.Add(new IntroDiscoverySprite(0x0030, 0x00cf, 0x0800, 0xcd93));
        zebesCompletionStarActor = new IntroDiscoverySprite(0x00d0, 0x00cf, 0x0800, 0xcd9b);
        actors.Add(zebesCompletionStarActor);
        actors.Add(new IntroDiscoverySprite(0x0080, 0x00ba, 0x0000, 0xccbb));
        Phase = CeresDestructionPhase.PlanetZebesTitle;
    }

    private void StepInitialDrift()
    {
        AddSignedSixteenSixteen(ref backgroundY, ref backgroundYSubPosition, 0x0000_1000);
        AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition, unchecked((int)0xffff_c000));
        zoom++;
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
        phaseTimer = unchecked((byte)(phaseTimer - 0x10));
        return (phaseTimer & 0xf0) == 0;
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
