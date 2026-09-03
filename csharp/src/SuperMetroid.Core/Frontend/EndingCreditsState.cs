using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Bank $8B's state-$27 ending owner, from <c>$8B:D443</c> through the credits and
/// post-credit result screen.
/// </summary>
/// <remarks>
/// This is deliberately a cinematic state machine rather than a video or a host-authored
/// slideshow. Backgrounds, character art, palettes, spritemaps, animation durations, and
/// credits rows all come from the cartridge. The named phases merely replace the native
/// <c>cinematic_function</c> address, making the same coroutine boundaries debuggable in C#.
/// </remarks>
internal sealed partial class EndingCreditsState
{
    private const int Intro3PaletteAddress = 0x8cede9;
    private const int Intro4PaletteAddress = 0x8cefe9;
    private const int Intro5PaletteAddress = 0x8ce9e9;
    private const int Intro6PaletteAddress = 0x8cebe9;

    private const int EscapeMapAAddress = 0x98bcd6;
    private const int EscapeMapBAddress = 0x98ed4f;
    private const int EscapeMapCAddress = 0x999101;
    private const int EscapeCharactersAAddress = 0x99d17e;
    private const int EscapeCharactersBAddress = 0x99d65b;
    private const int EscapeCharactersCAddress = 0x99d932;
    private const int EndingObjectCharactersAddress = 0x99d17e;
    private const int EndingObjectCharacters70Address = 0x98b5c1;
    private const int EndingObjectCharacters74Address = 0x98b857;
    private const int EndingObjectCharacters78Address = 0x98baed;
    private const int EndingObjectCharacters7cAddress = 0x98bccd;
    private const int EndingFontCharactersAddress = 0x97e7de;

    private const int WaitingForCreditsCharactersAddress = 0x979803;
    private const int SuitlessSamusCharactersAddress = 0x97b957;
    private const int ShootingScreenCharactersAddress = 0x97d7fc;
    private const int WaitingForCreditsTilemapAddress = 0x9796f4;
    private const int PostCreditsMode7Address = 0x97f987;
    private const int PostCreditsTileFragmentAAddress = 0x99da9f;
    private const int PostCreditsTileFragmentBAddress = 0x99dab1;

    private const ushort ExplosionFadePaletteOpcode = 0xf284;
    private const ushort SpawnExplosionSilhouetteOpcode = 0xf295;
    private const ushort StartZebesExplosionOpcode = 0xf2b7;
    private const ushort ExplosionFinaleOpcode = 0xf2fa;
    private const ushort EndZebesExplosionOpcode = 0xf32b;
    private const ushort SpawnCompletedTextOpcode = 0xf3b0;
    private const ushort SpawnClearTimeOpcode = 0xf3ce;
    private const ushort SpawnHoursTensOpcode = 0xf41b;
    private const ushort SpawnHoursUnitsOpcode = 0xf424;
    private const ushort SpawnColonOpcode = 0xf42d;
    private const ushort SpawnMinutesTensOpcode = 0xf436;
    private const ushort SpawnMinutesUnitsOpcode = 0xf43f;
    private const ushort TransitionToCreditsOpcode = 0xf448;

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly ushort gameTimeHours;
    private readonly ushort gameTimeMinutes;
    private readonly EndingInventorySnapshot inventory;
    private readonly bool japaneseText;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly List<EndingSprite> sprites = [];
    private readonly ushort[] postCreditsTilemap = new ushort[0x400];

    private CreditsObjectState? credits;
    private EndingBackgroundTextState? postCreditsText;
    private ushort cinematicFrame;
    private int phaseTimer;
    private int fadeCounter;
    private byte brightness;
    private ushort mode7X;
    private ushort mode7XSubposition;
    private ushort mode7Y;
    private ushort mode7Zoom = 0x0100;
    private SnesAngle mode7Angle;
    private ushort planetMotionIndex;
    private ushort postCreditsVerticalScroll;
    private short planetVelocityWhole;
    private ushort planetVelocityFraction;
    private bool creditsAssetsLoaded;

    public EndingCreditsState(
        ISnesAddressSpace bus,
        CartridgeAudioState audio,
        ushort gameTimeHours,
        ushort gameTimeMinutes,
        EndingInventorySnapshot inventory = default,
        bool japaneseText = false)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        this.gameTimeHours = gameTimeHours;
        this.gameTimeMinutes = gameTimeMinutes;
        this.inventory = inventory;
        this.japaneseText = japaneseText;
        Phase = EndingCreditsPhase.SetupEscapeFromZebes;
    }

    public EndingCreditsPhase Phase { get; private set; }
    public byte Brightness => brightness;
    public ushort CinematicFrame => cinematicFrame;
    public ushort CreditsVerticalScroll => credits?.VerticalScroll ?? 0;
    public bool CreditsFinished => credits?.Finished ?? false;
    public EndingReward EndingReward => gameTimeHours < 3
        ? EndingReward.Suitless
        : gameTimeHours < 10
            ? EndingReward.Helmetless
            : EndingReward.Armored;

    /// <summary>Runs one state-$27 dispatcher call.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case EndingCreditsPhase.SetupEscapeFromZebes:
                SetupEscapeSceneA();
                break;

            case EndingCreditsPhase.WaitForEscapeMusic:
                if (!audio.HasQueuedMusic)
                {
                    brightness = 0;
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeInEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeInEscapeSceneA:
                StepEscapeClouds(sceneB: false);
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneA;
                break;

            case EndingCreditsPhase.EscapeSceneA:
                StepEscapeClouds(sceneB: false);
                mode7X = unchecked((ushort)(mode7X - ((cinematicFrame & 1) == 0 ? 1 : 0)));
                mode7Y = unchecked((ushort)(mode7Y + 2));
                if (mode7Y >= 0x0180)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneA;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneA:
                StepEscapeClouds(sceneB: false);
                if (StepFastFadeOut())
                    SetupEscapeSceneB();
                break;

            case EndingCreditsPhase.FadeInEscapeSceneB:
                StepEscapeClouds(sceneB: true);
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.EscapeSceneB;
                break;

            case EndingCreditsPhase.EscapeSceneB:
                StepEscapeClouds(sceneB: true);
                mode7X = unchecked((ushort)(mode7X - ((cinematicFrame & 1) == 0 ? 1 : 0)));
                mode7Y = unchecked((ushort)(mode7Y + 3));
                if (mode7Y >= 0x0180)
                {
                    fadeCounter = 1;
                    Phase = EndingCreditsPhase.FadeOutEscapeSceneB;
                }
                break;

            case EndingCreditsPhase.FadeOutEscapeSceneB:
                StepEscapeClouds(sceneB: true);
                if (StepFastFadeOut())
                    SetupZebesExplosion();
                break;

            case EndingCreditsPhase.FadeInZebesExplosion:
                StepSprites();
                mode7Y = unchecked((ushort)(mode7Y + 4));
                if (StepFastFadeIn())
                    Phase = EndingCreditsPhase.ZebesExplosionPaletteCrossfade;
                break;

            case EndingCreditsPhase.ZebesExplosionPaletteCrossfade:
                StepSprites();
                mode7Y = unchecked((ushort)(mode7Y + 4));
                if (--phaseTimer <= 0)
                {
                    phaseTimer = 16;
                    Phase = EndingCreditsPhase.ZebesExplosionTileUpload;
                }
                break;

            case EndingCreditsPhase.ZebesExplosionTileUpload:
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
                    audio.QueueMusicDelayed8(MusicCommand.LoadData(0x3c));
                    audio.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x000e));
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
                    phaseTimer = 32;
                    Phase = EndingCreditsPhase.PostCreditsShootingStars;
                }
                break;

            case EndingCreditsPhase.PostCreditsShootingStars:
                if (--phaseTimer <= 0)
                {
                    // Function 132 installs the complete $8C:DC9B result panel into rows
                    // nine through seventeen before the following 180-frame hold.
                    CopyPostCreditsWords(0xdc9b, destination: 288, count: 288);
                    UploadPostCreditsTilemap();
                    phaseTimer = 180;
                    Phase = EndingCreditsPhase.PostCreditsWaitingSamus;
                }
                break;

            case EndingCreditsPhase.PostCreditsWaitingSamus:
                if (--phaseTimer <= 0)
                {
                    SpawnEndingRewardActors();
                    phaseTimer = 180;
                    Phase = EndingCreditsPhase.PostCreditsReward;
                }
                break;

            case EndingCreditsPhase.PostCreditsReward:
                StepSprites();
                if (--phaseTimer <= 0)
                {
                    Array.Fill(postCreditsTilemap, (ushort)0x007f, startIndex: 288, count: 288);
                    CopyPostCreditsWords(0xdedb, destination: 384, count: 64);
                    UploadPostCreditsTilemap();
                    postCreditsText = new EndingBackgroundTextState(
                        bus,
                        postCreditsTilemap,
                        instructionPointer: 0xdfdb,
                        inventory,
                        japaneseText);
                    Phase = EndingCreditsPhase.ItemPercentage;
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
                    postCreditsText = new EndingBackgroundTextState(
                        bus,
                        postCreditsTilemap,
                        instructionPointer: 0xe0af,
                        inventory,
                        japaneseText);
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

        cinematicFrame++;
    }

    private void SetupEscapeSceneA()
    {
        cgram.LoadFromBus(bus, Intro3PaletteAddress);
        LoadMode7(EscapeMapAAddress, EscapeCharactersAAddress);
        LoadEndingObjectCharacters();
        sprites.Clear();
        SpawnSprite(0x0140, 0x00c0, 0x0a00, 0xed0d, EndingSpriteRole.CloudRightA);
        SpawnSprite(0xffc0, 0x0040, 0x0a00, 0xed15, EndingSpriteRole.CloudLeftA);
        SpawnSprite(0x0140, 0x01c0, 0x0a00, 0xed0d, EndingSpriteRole.CloudRightB);
        SpawnSprite(0xffc0, 0xff40, 0x0a00, 0xed15, EndingSpriteRole.CloudLeftB);
        mode7X = 0x0020;
        mode7Y = 0x0040;
        mode7Zoom = 0x0100;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        postCreditsVerticalScroll = 0;
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        audio.QueueMusicDelayed8(MusicCommand.LoadData(0x33));
        audio.QueueMusicDelayed(MusicCommand.SelectTrack(5), MusicCommandDelay.FromDelayedYArgument(0x000e));
        Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    private void SetupEscapeSceneB()
    {
        LoadMode7(EscapeMapBAddress, EscapeCharactersBAddress);
        LoadEndingObjectCharacters();
        sprites.Clear();
        SpawnSprite(0xffa0, 0x0080, 0x0a00, 0xeced, EndingSpriteRole.CloudTopA);
        SpawnSprite(0xffa0, 0x00c0, 0x0a00, 0xecf5, EndingSpriteRole.CloudTopB);
        SpawnSprite(0x0120, 0x0120, 0x0a00, 0xecfd, EndingSpriteRole.CloudBottomA);
        SpawnSprite(0x0120, 0x0160, 0x0a00, 0xed05, EndingSpriteRole.CloudBottomB);
        mode7X = 0x0020;
        mode7Y = 0x0040;
        brightness = 0;
        fadeCounter = 1;
        Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    private void SetupZebesExplosion()
    {
        LoadMode7(EscapeMapCAddress, EscapeCharactersCAddress);
        LoadEndingObjectCharacters();
        cgram.LoadFromBus(bus, Intro6PaletteAddress + 0x0100, 128, 128);
        sprites.Clear();
        SpawnSprite(0x0080, 0x0080, 0x0e00, 0xeb0f, EndingSpriteRole.ExplodingZebes);
        SpawnSprite(0x0080, 0x0080, 0x0a00, 0xeb59, EndingSpriteRole.ExplosionLava);
        SpawnSprite(0x0080, 0x0080, 0x0e00, 0xeb3d, EndingSpriteRole.ExplosionGlow);
        SpawnSprite(0x0080, 0x0080, 0x0e00, 0xeb51, EndingSpriteRole.ExplosionStars);
        mode7X = 0;
        mode7Y = 0x0040;
        mode7Zoom = 0x0100;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = 64;
        Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    private void SetupPlanetEscape()
    {
        mode7X = unchecked((ushort)-72);
        mode7Y = unchecked((ushort)-104);
        mode7Zoom = 0x0c00;
        mode7Angle = SnesAngle.NormalizeTableIndex(-112);
        phaseTimer = 192;
        planetMotionIndex = 0;
        Phase = EndingCreditsPhase.PlanetEscapeFast;
    }

    private void SetupCredits()
    {
        LoadCreditsAndPostCreditsAssets();
        credits = new CreditsObjectState(bus);
        credits.UploadTilemap(vram);
        Array.Fill(postCreditsTilemap, (ushort)0x007f);
        sprites.Clear();
        brightness = 15;
        Phase = EndingCreditsPhase.Credits;
    }

    private void SetupPostCreditsBlank()
    {
        // F6FE copies Intro4 colors $04-$FF, disables text glow, forces blank, and arms
        // function 129 for sixty calls. Preserve colors zero through three as native does.
        cgram.LoadFromBus(bus, Intro4PaletteAddress + 8, 252, 4);
        brightness = 0;
        UploadPostCreditsTilemap();
        phaseTimer = 60;
        Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    private void LoadMode7(int mapAddress, int characterAddress)
    {
        byte[] map = RomDataReader.Decompress(bus, mapAddress, maximumOutputBytes: 0x8000);
        byte[] characters = RomDataReader.Decompress(bus, characterAddress, maximumOutputBytes: 0x8000);
        RequireMinimum(map, 0x4000, "ending Mode-7 map");
        RequireMinimum(characters, 0x4000, "ending Mode-7 characters");
        vram.Clear();
        vram.LoadMode7MapBytes(map.AsSpan(0, 0x4000));
        vram.LoadMode7CharacterBytes(characters.AsSpan(0, 0x4000));
    }

    private void LoadEndingObjectCharacters()
    {
        byte[] main = RomDataReader.Decompress(bus, EndingObjectCharactersAddress, maximumOutputBytes: 0x8000);
        // The native $6000-byte DMA begins at $7F:8000, but the $99:D17E stream itself
        // expands to $4000 bytes. Its final $2000 source bytes are the already-cleared
        // $7F:C000-$DFFF work-RAM tail. VRAM was likewise cleared above, so copying the
        // authored $4000-byte prefix reproduces the complete observable transfer.
        RequireMinimum(main, 0x4000, "ending OBJ characters");
        vram.LoadBytes(0x8000, main.AsSpan(0, 0x4000));
        LoadObjectFragment(EndingObjectCharacters70Address, 0xe000);
        LoadObjectFragment(EndingObjectCharacters74Address, 0xe800);
        LoadObjectFragment(EndingObjectCharacters78Address, 0xf000);
        LoadObjectFragment(EndingObjectCharacters7cAddress, 0xf800);
        byte[] font = RomDataReader.Decompress(bus, EndingFontCharactersAddress, maximumOutputBytes: 0x4000);
        RequireMinimum(font, 0x1000, "ending font characters");
        vram.LoadBytes(0xa000, font.AsSpan(0, 0x1000));
    }

    private void LoadObjectFragment(int sourceAddress, int destinationByte)
    {
        byte[] fragment = RomDataReader.Decompress(bus, sourceAddress, maximumOutputBytes: 0x1000);
        RequireMinimum(fragment, 0x0800, "ending OBJ fragment");
        vram.LoadBytes(destinationByte, fragment.AsSpan(0, 0x0800));
    }

    private void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        cgram.LoadFromBus(bus, Intro5PaletteAddress, 128, 0);
        byte[] font = RomDataReader.Decompress(bus, EndingFontCharactersAddress, maximumOutputBytes: 0x4000);
        byte[] waiting = RomDataReader.Decompress(bus, WaitingForCreditsCharactersAddress, maximumOutputBytes: 0x4000);
        byte[] shooting = RomDataReader.Decompress(bus, ShootingScreenCharactersAddress, maximumOutputBytes: 0x8000);
        byte[] waitingMap = RomDataReader.Decompress(bus, WaitingForCreditsTilemapAddress, maximumOutputBytes: 0x1000);
        byte[] mode7 = RomDataReader.Decompress(bus, PostCreditsMode7Address, maximumOutputBytes: 0x8000);
        byte[] fragmentA = RomDataReader.Decompress(bus, PostCreditsTileFragmentAAddress, maximumOutputBytes: 0x1000);
        byte[] fragmentB = RomDataReader.Decompress(bus, PostCreditsTileFragmentBAddress, maximumOutputBytes: 0x1000);
        RequireMinimum(font, 0x1000, "credits font");
        RequireMinimum(waiting, 0x2000, "waiting-Samus characters");
        RequireMinimum(shooting, 0x4000, "post-credits OBJ characters");
        RequireMinimum(waitingMap, 0x0800, "waiting-Samus tilemap");
        RequireMinimum(mode7, 0x4000, "post-credits Mode-7 characters");
        RequireMinimum(fragmentA, 0x0100, "post-credits tile fragment A");
        RequireMinimum(fragmentB, 0x0800, "post-credits tile fragment B");

        vram.Clear();
        vram.LoadBytes(0x8000, font.AsSpan(0, 0x1000));
        vram.LoadBytes(0xa000, waiting.AsSpan(0, 0x2000));
        vram.LoadBytes(0xc000, shooting.AsSpan(0, 0x4000));
        vram.LoadBytes(0x9800, waitingMap.AsSpan(0, 0x0800));
        vram.LoadBytes(0x4000, fragmentA.AsSpan(0, 0x0100));
        vram.LoadBytes(0x4800, fragmentB.AsSpan(0, 0x0800));

        // Function 126 selects either the suitless (<3h) or armored (>=3h) Mode-7
        // character source. Keeping both decompressions here makes the branch explicit.
        byte[] resultCharacters = EndingReward == EndingReward.Suitless
            ? RomDataReader.Decompress(bus, SuitlessSamusCharactersAddress, maximumOutputBytes: 0x8000)
            : mode7;
        RequireMinimum(resultCharacters, 0x4000, "post-credits reward characters");
        vram.LoadMode7CharacterBytes(resultCharacters.AsSpan(0, 0x4000));
        creditsAssetsLoaded = true;
    }

    private void StepEscapeClouds(bool sceneB)
    {
        foreach (EndingSprite wrapper in sprites)
        {
            IntroDiscoverySprite sprite = wrapper.Sprite;
            switch (wrapper.Role)
            {
                case EndingSpriteRole.CloudRightA:
                case EndingSpriteRole.CloudRightB:
                    if (mode7Y >= 0x0060)
                    {
                        sprite.XPosition = unchecked((ushort)(sprite.XPosition - 2));
                        sprite.YPosition--;
                    }
                    break;
                case EndingSpriteRole.CloudLeftA:
                case EndingSpriteRole.CloudLeftB:
                    if (mode7Y >= 0x0060)
                    {
                        sprite.XPosition = unchecked((ushort)(sprite.XPosition + 2));
                        sprite.YPosition++;
                    }
                    break;
                case EndingSpriteRole.CloudTopA:
                case EndingSpriteRole.CloudTopB:
                    if (mode7Zoom < 0x00b0)
                        sprite.XPosition++;
                    break;
                case EndingSpriteRole.CloudBottomA:
                case EndingSpriteRole.CloudBottomB:
                    if (mode7Zoom < 0x00b0)
                        sprite.XPosition--;
                    break;
            }
            sprite.Step(bus);
        }
    }

    private void StepSprites()
    {
        // New actors are appended by opcode callbacks. A snapshot count matches the native
        // descending slot walk: a newly spawned lower slot begins on the following frame.
        int count = sprites.Count;
        for (int index = 0; index < count; index++)
        {
            EndingSprite wrapper = sprites[index];
            wrapper.Sprite.Step(bus, (opcode, cursor) =>
                HandleSpriteOpcode(wrapper, opcode, cursor));
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    private ushort? HandleSpriteOpcode(EndingSprite owner, ushort opcode, ushort cursor)
    {
        switch (opcode)
        {
            case ExplosionFadePaletteOpcode:
                // F284 starts a palette-FX object. The palette interpreter remains a
                // separate subsystem; the actor's list cursor still advances immediately.
                return cursor;

            case SpawnExplosionSilhouetteOpcode:
                SpawnSprite(0x0080, 0x0080, 0x0a00, 0xeb69, EndingSpriteRole.ExplosionSilhouette);
                cgram.SetColor(0, 0x7fff);
                return cursor;

            case StartZebesExplosionOpcode:
                SpawnSprite(0x0080, 0x0080, 0x0e00, 0xeb71, EndingSpriteRole.ExplosionStarsRight);
                SpawnSprite(0xff80, 0x0080, 0x0e00, 0xeb81, EndingSpriteRole.ExplosionStarsLeft);
                return cursor;

            case ExplosionFinaleOpcode:
                SpawnSprite(0x0080, 0x0080, 0x0c00, 0xeb89, EndingSpriteRole.ExplosionAfterglow);
                return cursor;

            case EndZebesExplosionOpcode:
                phaseTimer = 120;
                Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return cursor;

            case SpawnCompletedTextOpcode:
                SpawnSprite(0x0080, 0x0060, 0x0400, 0xebd7, EndingSpriteRole.CompletedSuccessfullyText);
                return cursor;

            case SpawnClearTimeOpcode:
                SpawnSprite(0x0080, 0x00a0, 0x0200, 0xec35, EndingSpriteRole.ClearTimeText);
                return cursor;

            case SpawnHoursTensOpcode:
                SpawnDigit(gameTimeHours / 10, 0x009c);
                return cursor;

            case SpawnHoursUnitsOpcode:
                SpawnDigit(gameTimeHours % 10, 0x00a4);
                return cursor;

            case SpawnColonOpcode:
                SpawnSprite(0x00ac, 0x00a0, 0, 0xecd1, EndingSpriteRole.ClearTimeDigit);
                return cursor;

            case SpawnMinutesTensOpcode:
                SpawnDigit(gameTimeMinutes / 10, 0x00b4);
                return cursor;

            case SpawnMinutesUnitsOpcode:
                SpawnDigit(gameTimeMinutes % 10, 0x00bc);
                return cursor;

            case TransitionToCreditsOpcode:
                fadeCounter = 1;
                Phase = EndingCreditsPhase.FadeOutToCredits;
                return cursor;

            default:
                throw new InvalidDataException(
                    $"Ending sprite opcode $8B:{opcode:X4} at $8B:{unchecked((ushort)(cursor - 2)):X4} is untranslated.");
        }
    }

    private void SpawnDigit(int digit, ushort x)
    {
        int normalized = Math.Clamp(digit, 0, 9);
        SpawnSprite(x, 0x00a0, 0, unchecked((ushort)(0xec81 + normalized * 8)), EndingSpriteRole.ClearTimeDigit);
    }

    private void SpawnEndingRewardActors()
    {
        sprites.Clear();
        switch (EndingReward)
        {
            case EndingReward.Suitless:
                SpawnSprite(0x0078, 0x0088, 0x0a00, 0xed1d, EndingSpriteRole.RewardSamus);
                SpawnSprite(0x0078, 0x0088, 0x0a00, 0xed25, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Helmetless:
                SpawnSprite(0x0078, 0x0098, 0x0c00, 0xedb1, EndingSpriteRole.RewardSamus);
                SpawnSprite(0x0079, 0x006b, 0x0a00, 0xedc1, EndingSpriteRole.RewardSamus);
                break;
            default:
                SpawnSprite(0x0078, 0x0098, 0x0c00, 0xedb1, EndingSpriteRole.RewardSamus);
                SpawnSprite(0x007c, 0x006c, 0x0c00, 0xedb9, EndingSpriteRole.RewardSamus);
                break;
        }
    }

    private void CopyPostCreditsWords(ushort sourcePointer, int destination, int count)
    {
        if (destination < 0 || count < 0 || destination + count > postCreditsTilemap.Length)
            throw new ArgumentOutOfRangeException(nameof(destination));
        for (int index = 0; index < count; index++)
        {
            postCreditsTilemap[destination + index] = RomDataReader.ReadWordFixedBank(
                bus,
                0x8c0000 | unchecked((ushort)(sourcePointer + index * 2)));
        }
    }

    private void UploadPostCreditsTilemap() =>
        vram.ExecuteWordTransfer(postCreditsTilemap, destinationWord: 0x4c00, wordIncrement: 1);

    private void SpawnSprite(
        ushort x,
        ushort y,
        ushort palette,
        ushort instructionPointer,
        EndingSpriteRole role)
    {
        sprites.Add(new EndingSprite(
            new IntroDiscoverySprite(x, y, palette, instructionPointer),
            role));
    }

    private void StepPlanetEscapeFast()
    {
        if (phaseTimer > 0)
            phaseTimer--;
        mode7Angle = mode7Angle.AddTableUnits(-4);
        AddSignedFixed(ref mode7X, ref mode7XSubposition, PlanetFastMotion[planetMotionIndex]);
        planetMotionIndex = unchecked((ushort)((planetMotionIndex + 1) & 0x0f));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 8));
        if (mode7Zoom < 0x05b0)
        {
            planetMotionIndex = 0;
            Phase = EndingCreditsPhase.PlanetEscapeSlow;
        }
    }

    private void StepPlanetEscapeSlow()
    {
        if (mode7Angle != SnesAngle.FromTableIndex(0xe0))
            mode7Angle = mode7Angle.AddTableUnits(-1);
        AddSignedFixed(ref mode7X, ref mode7XSubposition, PlanetSlowMotion[planetMotionIndex]);
        planetMotionIndex = unchecked((ushort)((planetMotionIndex + 1) & 7));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 2));
        if (mode7Zoom < 0x04a0)
        {
            planetVelocityWhole = 0;
            planetVelocityFraction = 0x8000;
            Phase = EndingCreditsPhase.PlanetEscapeAccelerating;
        }
    }

    private void StepPlanetEscapeAccelerating()
    {
        int velocity = (planetVelocityWhole << 16) | planetVelocityFraction;
        velocity = unchecked(velocity - 0x0000_0100);
        planetVelocityWhole = unchecked((short)(velocity >> 16));
        planetVelocityFraction = unchecked((ushort)velocity);
        AddSignedFixed(ref mode7X, ref mode7XSubposition, velocity);
        if (mode7Zoom < 0x0180 && (cinematicFrame & 3) == 0 &&
            mode7Angle != SnesAngle.FromTableIndex(0x10))
        {
            mode7Angle = mode7Angle.AddTableUnits(2);
        }
        if (mode7Zoom < 0x0020)
        {
            SpawnSprite(0x0080, 0x0060, 0x0400, 0xeb91, EndingSpriteRole.OperationWasText);
            Phase = EndingCreditsPhase.OperationSuccessfulText;
        }
        else
        {
            mode7Zoom = unchecked((ushort)(mode7Zoom - 4));
        }
    }

    private bool StepFastFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    private bool StepFastFadeOut()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 1;
            brightness = (byte)Math.Max(0, brightness - 1);
        }
        return brightness == 0;
    }

    private bool StepSlowFadeIn()
    {
        if (fadeCounter-- <= 0)
        {
            fadeCounter = 3;
            brightness = (byte)Math.Min(15, brightness + 1);
        }
        return brightness == 15;
    }

    private static void AddSignedFixed(ref ushort whole, ref ushort fraction, int delta)
    {
        uint value = ((uint)whole << 16) | fraction;
        value = unchecked(value + (uint)delta);
        whole = unchecked((ushort)(value >> 16));
        fraction = unchecked((ushort)value);
    }

    private static void RequireMinimum(byte[] data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

    private static readonly int[] PlanetFastMotion =
    [
        0x0000_8000, 0x0000_8000, 0x0000_8000, 0x0000_8000,
        unchecked((int)0xffff_8000), unchecked((int)0xffff_8000), 0x0000_8000, 0x0000_8000,
        0x0000_8000, unchecked((int)0xffff_8000), unchecked((int)0xffff_8000), 0x0000_8000,
        0x0000_8000, 0x0000_8000, unchecked((int)0xffff_8000), unchecked((int)0xffff_8000),
    ];

    private static readonly int[] PlanetSlowMotion =
    [
        0x0001_0000, 0x0001_0000, 0x0001_0000, unchecked((int)0xffff_0000),
        unchecked((int)0xffff_0000), 0x0001_0000, 0x0001_0000, unchecked((int)0xffff_0000),
    ];
}

internal enum EndingCreditsPhase
{
    SetupEscapeFromZebes,
    WaitForEscapeMusic,
    FadeInEscapeSceneA,
    EscapeSceneA,
    FadeOutEscapeSceneA,
    FadeInEscapeSceneB,
    EscapeSceneB,
    FadeOutEscapeSceneB,
    FadeInZebesExplosion,
    ZebesExplosionPaletteCrossfade,
    ZebesExplosionTileUpload,
    ZebesExplosionAnimation,
    WaitForPlanetEscapeMusic,
    WaitForPlanetEscapeMusicQueue,
    PlanetEscapeFast,
    PlanetEscapeSlow,
    PlanetEscapeAccelerating,
    OperationSuccessfulText,
    FadeOutToCredits,
    Credits,
    PostCreditsBlank,
    PostCreditsFadeIn,
    PostCreditsShootingStars,
    PostCreditsWaitingSamus,
    PostCreditsReward,
    ItemPercentage,
    ItemPercentageScrollDown,
    SeeYouNextMission,
}

internal enum EndingReward
{
    Suitless,
    Helmetless,
    Armored,
}

internal enum EndingSpriteRole
{
    CloudRightA,
    CloudLeftA,
    CloudRightB,
    CloudLeftB,
    CloudTopA,
    CloudTopB,
    CloudBottomA,
    CloudBottomB,
    ExplodingZebes,
    ExplosionLava,
    ExplosionGlow,
    ExplosionStars,
    ExplosionSilhouette,
    ExplosionStarsRight,
    ExplosionStarsLeft,
    ExplosionAfterglow,
    OperationWasText,
    CompletedSuccessfullyText,
    ClearTimeText,
    ClearTimeDigit,
    RewardSamus,
}

internal sealed record EndingSprite(IntroDiscoverySprite Sprite, EndingSpriteRole Role);
