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
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly ushort gameTimeHours;
    private readonly ushort gameTimeMinutes;
    private readonly EndingInventorySnapshot inventory;
    private readonly bool japaneseText;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly List<EndingSprite> sprites = [];
    private readonly ushort[] postCreditsTilemap =
        new ushort[EndingCreditsRomData.Rendering.TilemapWords];

    private CreditsObjectState? credits;
    private EndingBackgroundTextState? postCreditsText;
    private ushort cinematicFrame;
    private int phaseTimer;
    private int fadeCounter;
    private byte brightness;
    private ushort mode7X;
    private ushort mode7XSubposition;
    private ushort mode7Y;
    private ushort mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
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
    public EndingReward EndingReward =>
        gameTimeHours < EndingCreditsRomData.Rewards.SuitlessMaximumHoursExclusive
        ? EndingReward.Suitless
        : gameTimeHours < EndingCreditsRomData.Rewards.HelmetlessMaximumHoursExclusive
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
                if (mode7Y >= EndingCreditsRomData.Motion.EscapeEndY)
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
                if (mode7Y >= EndingCreditsRomData.Motion.EscapeEndY)
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
                    PrepareFlyawayUploads();
                    Phase = EndingCreditsPhase.ZebesExplosionTileUpload;
                }
                break;

            case EndingCreditsPhase.ZebesExplosionTileUpload:
                UploadFlyawayChunk(16 - phaseTimer);
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
                    audio.QueueMusicDelayed8(MusicCommand.LoadData(
                        EndingCreditsRomData.Music.EndingDataIndex));
                    audio.QueueMusicDelayed(
                        MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
                        MusicCommandDelay.FromDelayedYArgument(
                            EndingCreditsRomData.Music.DelayArgument));
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
                    CopyPostCreditsWords(
                        EndingCreditsRomData.Instructions.ResultPanel,
                        EndingCreditsRomData.Text.ResultPanelDestination,
                        EndingCreditsRomData.Text.ResultPanelWords);
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
                    Array.Fill(
                        postCreditsTilemap,
                        EndingCreditsRomData.Rendering.BlankTile,
                        EndingCreditsRomData.Text.ResultPanelDestination,
                        EndingCreditsRomData.Text.ResultPanelWords);
                    CopyPostCreditsWords(
                        EndingCreditsRomData.Instructions.ItemPercentagePanel,
                        EndingCreditsRomData.Text.ItemPercentagePanelDestination,
                        EndingCreditsRomData.Text.ItemPercentagePanelWords);
                    UploadPostCreditsTilemap();
                    // E58A clears every cinematic sprite before spawning the percentage
                    // BG object. Reward actors must not survive underneath either message.
                    sprites.Clear();
                    postCreditsText = new EndingBackgroundTextState(
                        bus,
                        postCreditsTilemap,
                        instructionPointer: EndingCreditsRomData.Instructions.ItemPercentageText,
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
                        instructionPointer: EndingCreditsRomData.Instructions.SeeYouNextMissionText,
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
        cgram.LoadFromBus(bus, EndingCreditsRomData.Assets.EscapePalette);
        LoadMode7(
            EndingCreditsRomData.Assets.EscapeMapA,
            EndingCreditsRomData.Assets.EscapeCharactersA);
        LoadEscapeCloudCharacters();
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightTop, EndingSpriteRole.CloudRightA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftTop, EndingSpriteRole.CloudLeftA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudRightBottom, EndingSpriteRole.CloudRightB);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeACloudLeftBottom, EndingSpriteRole.CloudLeftB);
        mode7X = EndingCreditsRomData.Motion.EscapeInitialX;
        mode7Y = EndingCreditsRomData.Motion.EscapeInitialY;
        mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        postCreditsVerticalScroll = 0;
        audio.QueueMusicDelayed8(MusicCommand.Stop);
        audio.QueueMusicDelayed8(MusicCommand.LoadData(
            EndingCreditsRomData.Music.EscapeDataIndex));
        audio.QueueMusicDelayed(
            MusicCommand.SelectTrack(EndingCreditsRomData.Music.Track),
            MusicCommandDelay.FromDelayedYArgument(EndingCreditsRomData.Music.DelayArgument));
        Phase = EndingCreditsPhase.WaitForEscapeMusic;
    }

    private void SetupEscapeSceneB()
    {
        LoadMode7(
            EndingCreditsRomData.Assets.EscapeMapB,
            EndingCreditsRomData.Assets.EscapeCharactersB);
        LoadEscapeCloudCharacters();
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopA, EndingSpriteRole.CloudTopA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudTopB, EndingSpriteRole.CloudTopB);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomA, EndingSpriteRole.CloudBottomA);
        SpawnSprite(EndingCreditsRomData.Sprites.EscapeBCloudBottomB, EndingSpriteRole.CloudBottomB);
        mode7X = EndingCreditsRomData.Motion.EscapeInitialX;
        mode7Y = EndingCreditsRomData.Motion.EscapeInitialY;
        brightness = 0;
        fadeCounter = 1;
        Phase = EndingCreditsPhase.FadeInEscapeSceneB;
    }

    private void SetupZebesExplosion()
    {
        LoadMode7(
            EndingCreditsRomData.Assets.ExplosionMap,
            EndingCreditsRomData.Assets.ExplosionCharacters);
        LoadEndingObjectCharacters();
        cgram.LoadFromBus(
            bus,
            EndingCreditsRomData.Assets.ExplosionPalette +
                EndingCreditsRomData.Rendering.PaletteSecondHalfOffset,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            EndingCreditsRomData.Rendering.PaletteHalfBytes);
        sprites.Clear();
        SpawnSprite(EndingCreditsRomData.Sprites.ExplodingZebes, EndingSpriteRole.ExplodingZebes);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionLava, EndingSpriteRole.ExplosionLava);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionGlow, EndingSpriteRole.ExplosionGlow);
        SpawnSprite(EndingCreditsRomData.Sprites.ExplosionStars, EndingSpriteRole.ExplosionStars);
        mode7X = 0;
        mode7Y = EndingCreditsRomData.Motion.EscapeInitialY;
        mode7Zoom = EndingCreditsRomData.Motion.IdentityScale;
        mode7Angle = SnesAngle.Zero;
        brightness = 0;
        fadeCounter = 1;
        phaseTimer = 64;
        Phase = EndingCreditsPhase.FadeInZebesExplosion;
    }

    private void SetupPlanetEscape()
    {
        // Func120 clears the explosion flash's backdrop and transparent palette entries.
        cgram.SetColor(0, 0);
        cgram.SetColor(16, 0);
        cgram.SetColor(128, 0);
        mode7X = unchecked((ushort)-72);
        mode7Y = unchecked((ushort)-104);
        mode7Zoom = EndingCreditsRomData.Motion.PlanetEscapeInitialScale;
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
        Array.Fill(postCreditsTilemap, EndingCreditsRomData.Rendering.BlankTile);
        sprites.Clear();
        brightness = 15;
        Phase = EndingCreditsPhase.Credits;
    }

    private void SetupPostCreditsBlank()
    {
        // F6FE copies Intro4 colors $04-$FF, disables text glow, forces blank, and arms
        // function 129 for sixty calls. Preserve colors zero through three as native does.
        cgram.LoadFromBus(
            bus,
            EndingCreditsRomData.Assets.PostCreditsPalette +
                EndingCreditsRomData.Rendering.PostCreditsPaletteSourceOffset,
            EndingCreditsRomData.Rendering.PostCreditsPaletteBytes,
            EndingCreditsRomData.Rendering.PostCreditsPaletteDestination);
        brightness = 0;
        UploadPostCreditsTilemap();
        phaseTimer = 60;
        Phase = EndingCreditsPhase.PostCreditsBlank;
    }

    private void LoadMode7(int mapAddress, int characterAddress)
    {
        byte[] map = RomDataReader.Decompress(
            bus, mapAddress, EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] characters = RomDataReader.Decompress(
            bus, characterAddress, EndingCreditsRomData.Rendering.DecompressionLimit);
        RequireMinimum(map, EndingCreditsRomData.Rendering.Mode7Bytes,
            "ending Mode-7 map");
        RequireMinimum(characters, EndingCreditsRomData.Rendering.Mode7Bytes,
            "ending Mode-7 characters");
        vram.Clear();
        // Native mode-1 DMA writes a packed map/character seed twice to $0000/$2000
        // words. The following single-port $2119 DMA replaces only the high bytes.
        vram.LoadBytes(0, characters.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
        vram.LoadBytes(EndingCreditsRomData.Rendering.Mode7Bytes, characters.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
        vram.LoadMode7CharacterBytes(map.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
    }

    private void LoadEscapeCloudCharacters()
    {
        byte[] clouds = RomDataReader.Decompress(bus, EndingCreditsRomData.Assets.EscapeCloudCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        RequireMinimum(clouds, EndingCreditsRomData.Rendering.Mode7Bytes, "escape cloud characters");
        vram.LoadBytes(EndingCreditsRomData.Rendering.PostCreditsObjectDestination,
            clouds.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
    }

    private void LoadEndingObjectCharacters()
    {
        byte[] main = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.EndingObjectCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        // $8B:D8C1 uploads the complete explosion object sheet from $7F:8000.
        RequireMinimum(main, EndingCreditsRomData.Rendering.ExplosionObjectBytes,
            "ending OBJ characters");
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.ObjectCharactersDestination,
            main.AsSpan(0, EndingCreditsRomData.Rendering.ExplosionObjectBytes));
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters70,
            EndingCreditsRomData.Rendering.Fragment70Destination);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters74,
            EndingCreditsRomData.Rendering.Fragment74Destination);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters78,
            EndingCreditsRomData.Rendering.Fragment78Destination);
        LoadObjectFragment(
            EndingCreditsRomData.Assets.EndingObjectCharacters7C,
            EndingCreditsRomData.Rendering.Fragment7CDestination);
        byte[] font = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        RequireMinimum(font, EndingCreditsRomData.Rendering.ObjectFragmentLimit,
            "ending font characters");
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.FontCharactersDestination,
            font.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentLimit));
    }

    private void LoadObjectFragment(int sourceAddress, int destinationByte)
    {
        byte[] fragment = RomDataReader.Decompress(
            bus, sourceAddress, EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        RequireMinimum(fragment, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "ending OBJ fragment");
        vram.LoadBytes(
            destinationByte,
            fragment.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));
    }

    private void LoadCreditsAndPostCreditsAssets()
    {
        if (creditsAssetsLoaded)
            return;

        cgram.LoadFromBus(
            bus,
            EndingCreditsRomData.Assets.CreditsPalette,
            EndingCreditsRomData.Rendering.PaletteHalfBytes,
            0);
        byte[] font = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.EndingFontCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        byte[] waiting = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.WaitingForCreditsCharacters,
            EndingCreditsRomData.Rendering.Mode7Bytes);
        byte[] shooting = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.ShootingScreenCharacters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] waitingMap = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.WaitingForCreditsTilemap,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        byte[] mode7 = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.PostCreditsMode7Characters,
            EndingCreditsRomData.Rendering.DecompressionLimit);
        byte[] fragmentA = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentA,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        byte[] fragmentB = RomDataReader.Decompress(
            bus,
            EndingCreditsRomData.Assets.PostCreditsTileFragmentB,
            EndingCreditsRomData.Rendering.ObjectFragmentLimit);
        RequireMinimum(font, EndingCreditsRomData.Rendering.FontCharacterBytes,
            "credits font");
        RequireMinimum(waiting, EndingCreditsRomData.Rendering.WaitingCharacterBytes,
            "waiting-Samus characters");
        RequireMinimum(shooting, EndingCreditsRomData.Rendering.ShootingCharacterBytes,
            "post-credits OBJ characters");
        RequireMinimum(waitingMap, EndingCreditsRomData.Rendering.WaitingTilemapBytes,
            "waiting-Samus tilemap");
        RequireMinimum(mode7, EndingCreditsRomData.Rendering.Mode7Bytes,
            "post-credits Mode-7 characters");
        RequireMinimum(fragmentA, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes,
            "post-credits tile fragment A");
        RequireMinimum(fragmentB, EndingCreditsRomData.Rendering.ObjectFragmentBytes,
            "post-credits tile fragment B");

        vram.Clear();
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.ObjectCharactersDestination,
            font.AsSpan(0, EndingCreditsRomData.Rendering.FontCharacterBytes));
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.FontCharactersDestination,
            waiting.AsSpan(0, EndingCreditsRomData.Rendering.WaitingCharacterBytes));
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.PostCreditsObjectDestination,
            shooting.AsSpan(0, EndingCreditsRomData.Rendering.ShootingCharacterBytes));
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.WaitingTilemapDestination,
            waitingMap.AsSpan(0, EndingCreditsRomData.Rendering.WaitingTilemapBytes));
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.PostCreditsFragmentADestination,
            fragmentA.AsSpan(0, EndingCreditsRomData.Rendering.PostCreditsFragmentABytes));
        vram.LoadBytes(
            EndingCreditsRomData.Rendering.PostCreditsFragmentBDestination,
            fragmentB.AsSpan(0, EndingCreditsRomData.Rendering.ObjectFragmentBytes));

        // Function 126 selects suitless (<3h) or suited (>=3h) OBJ characters.
        // The suited branch reuses the waiting-scene decompression, not the later Mode-7 sheet.
        byte[] resultCharacters = EndingReward == EndingReward.Suitless
            ? RomDataReader.Decompress(
                bus,
                EndingCreditsRomData.Assets.SuitlessSamusCharacters,
                EndingCreditsRomData.Rendering.DecompressionLimit)
            : waiting;
        RequireMinimum(resultCharacters, EndingCreditsRomData.Rendering.Mode7Bytes,
            "post-credits reward characters");
        // $8B:E027/E04B use mode-1 DMA to both ports, not a single Mode-7 lane.
        vram.LoadBytes(0, resultCharacters.AsSpan(0, EndingCreditsRomData.Rendering.Mode7Bytes));
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
                    if (mode7Y >= EndingCreditsRomData.Motion.CloudMotionStartY)
                    {
                        sprite.XPosition = unchecked((ushort)(sprite.XPosition - 2));
                        sprite.YPosition--;
                    }
                    break;
                case EndingSpriteRole.CloudLeftA:
                case EndingSpriteRole.CloudLeftB:
                    if (mode7Y >= EndingCreditsRomData.Motion.CloudMotionStartY)
                    {
                        sprite.XPosition = unchecked((ushort)(sprite.XPosition + 2));
                        sprite.YPosition++;
                    }
                    break;
                case EndingSpriteRole.CloudTopA:
                case EndingSpriteRole.CloudTopB:
                    if (mode7Zoom < EndingCreditsRomData.Motion.CloudSceneBScaleLimit)
                        sprite.XPosition++;
                    break;
                case EndingSpriteRole.CloudBottomA:
                case EndingSpriteRole.CloudBottomB:
                    if (mode7Zoom < EndingCreditsRomData.Motion.CloudSceneBScaleLimit)
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
            StepEndingSpritePreInstruction(wrapper);
            wrapper.Sprite.Step(bus, (opcode, cursor) =>
                HandleSpriteOpcode(wrapper, opcode, cursor));
        }
        sprites.RemoveAll(wrapper => !wrapper.Sprite.IsActive);
    }

    private ushort? HandleSpriteOpcode(EndingSprite owner, ushort opcode, ushort cursor)
    {
        switch (opcode)
        {
            case CinematicCodePointers.Ending_Instruction_FadeExplosionPalette:
                // F284 starts a palette-FX object. The palette interpreter remains a
                // separate subsystem; the actor's list cursor still advances immediately.
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnExplosionSilhouette:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionSilhouette,
                    EndingSpriteRole.ExplosionSilhouette);
                cgram.SetColor(0, EndingCreditsRomData.Rendering.WhiteColor);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_StartZebesExplosion:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsRight,
                    EndingSpriteRole.ExplosionStarsRight);
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionStarsLeft,
                    EndingSpriteRole.ExplosionStarsLeft);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_ExplosionFinale:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ExplosionAfterglow,
                    EndingSpriteRole.ExplosionAfterglow);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_EndZebesExplosion:
                phaseTimer = 120;
                Phase = EndingCreditsPhase.WaitForPlanetEscapeMusic;
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnCompletedText:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.CompletedSuccessfullyText,
                    EndingSpriteRole.CompletedSuccessfullyText);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnClearTime:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeText,
                    EndingSpriteRole.ClearTimeText);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnHoursTens:
                SpawnDigit(gameTimeHours / 10, EndingCreditsRomData.Text.HoursTensX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnHoursUnits:
                SpawnDigit(gameTimeHours % 10, EndingCreditsRomData.Text.HoursUnitsX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnColon:
                SpawnSprite(
                    EndingCreditsRomData.Sprites.ClearTimeColon,
                    EndingSpriteRole.ClearTimeDigit);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnMinutesTens:
                SpawnDigit(gameTimeMinutes / 10, EndingCreditsRomData.Text.MinutesTensX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_SpawnMinutesUnits:
                SpawnDigit(gameTimeMinutes % 10, EndingCreditsRomData.Text.MinutesUnitsX);
                return cursor;

            case CinematicCodePointers.Ending_Instruction_TransitionToCredits:
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
        SpawnSprite(
            x,
            EndingCreditsRomData.Sprites.ClearTimeDigitY,
            0,
            unchecked((ushort)(
                EndingCreditsRomData.Sprites.ClearTimeDigitInstructionBase +
                normalized * EndingCreditsRomData.Sprites.ClearTimeDigitInstructionStride)),
            EndingSpriteRole.ClearTimeDigit);
    }

    private void SpawnEndingRewardActors()
    {
        sprites.Clear();
        switch (EndingReward)
        {
            case EndingReward.Suitless:
                SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.SuitlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            case EndingReward.Helmetless:
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.HelmetlessRewardHead, EndingSpriteRole.RewardSamus);
                break;
            default:
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardBody, EndingSpriteRole.RewardSamus);
                SpawnSprite(EndingCreditsRomData.Sprites.ArmoredRewardHead, EndingSpriteRole.RewardSamus);
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
                EndingCreditsRomData.Instructions.Bank.AddWithinBank(
                    unchecked((ushort)(sourcePointer + index * sizeof(ushort)))));
        }
    }

    private void UploadPostCreditsTilemap() =>
        vram.ExecuteWordTransfer(
            postCreditsTilemap,
            EndingCreditsRomData.Rendering.PostCreditsTilemapWord,
            wordIncrement: 1);

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
        if (role == EndingSpriteRole.ExplosionStarsLeft)
            sprites[^1].Sprite.PreInstructionPointerForDiscovery(EndingSpritePreInstructions.WaitForFlyaway);
    }

    private void SpawnSprite(EndingSpriteDefinition definition, EndingSpriteRole role) =>
        SpawnSprite(
            definition.X,
            definition.Y,
            definition.Attributes.Raw,
            definition.InstructionPointer,
            role);

    private void StepPlanetEscapeFast()
    {
        if (phaseTimer > 0)
            phaseTimer--;
        mode7Angle = mode7Angle.AddTableUnits(-4);
        AddSignedFixed(
            ref mode7X,
            ref mode7XSubposition,
            EndingCreditsRomData.Motion.PlanetFastPattern[planetMotionIndex]);
        planetMotionIndex = unchecked((ushort)(
            (planetMotionIndex + 1) &
            (EndingCreditsRomData.Motion.PlanetFastPattern.Length - 1)));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 8));
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetFastEndScale)
        {
            planetMotionIndex = 0;
            Phase = EndingCreditsPhase.PlanetEscapeSlow;
        }
    }

    private void StepPlanetEscapeSlow()
    {
        if (mode7Angle != EndingCreditsRomData.Motion.PlanetSlowTargetAngle)
            mode7Angle = mode7Angle.AddTableUnits(-1);
        AddSignedFixed(
            ref mode7X,
            ref mode7XSubposition,
            EndingCreditsRomData.Motion.PlanetSlowPattern[planetMotionIndex]);
        planetMotionIndex = unchecked((ushort)(
            (planetMotionIndex + 1) &
            (EndingCreditsRomData.Motion.PlanetSlowPattern.Length - 1)));
        mode7Zoom = unchecked((ushort)(mode7Zoom - 2));
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetSlowEndScale)
        {
            planetVelocityWhole = 0;
            planetVelocityFraction = EndingCreditsRomData.Motion.InitialAccelerationFraction;
            Phase = EndingCreditsPhase.PlanetEscapeAccelerating;
        }
    }

    private void StepPlanetEscapeAccelerating()
    {
        int velocity = (planetVelocityWhole << 16) | planetVelocityFraction;
        velocity = unchecked(
            velocity - EndingCreditsRomData.Motion.AccelerationDelta16Point16);
        planetVelocityWhole = unchecked((short)(velocity >> 16));
        planetVelocityFraction = unchecked((ushort)velocity);
        AddSignedFixed(ref mode7X, ref mode7XSubposition, velocity);
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetTurnStartScale &&
            (cinematicFrame & 3) == 0 &&
            mode7Angle != EndingCreditsRomData.Motion.PlanetExitTargetAngle)
        {
            mode7Angle = mode7Angle.AddTableUnits(2);
        }
        if (mode7Zoom < EndingCreditsRomData.Motion.PlanetExitScale)
        {
            SpawnSprite(
                EndingCreditsRomData.Sprites.OperationWasText,
                EndingSpriteRole.OperationWasText);
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
        SnesFixedPosition result = SnesSignedSixteenSixteen
            .FromRaw(delta)
            .AddTo(whole, fraction);
        whole = result.Whole;
        fraction = result.Fraction;
    }

    private static void RequireMinimum(byte[] data, int minimum, string name)
    {
        if (data.Length < minimum)
            throw new InvalidDataException($"{name} expanded to ${data.Length:X}, expected at least ${minimum:X}.");
    }

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
