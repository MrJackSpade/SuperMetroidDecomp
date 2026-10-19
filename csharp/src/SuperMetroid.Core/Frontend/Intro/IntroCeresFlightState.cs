using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Initial native Mode 7 leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    /// <summary>Holds the music queue whose drain gates the initial display.</summary>
    internal readonly CartridgeAudioState audio;
    /// <summary>Contains loaded Mode 7 and OBJ character data for this scene.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Contains the installed Ceres flight palette used by rendering.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Decompressed front and rear Mode 7 map slices selected at the view transition.</summary>
    private byte[] tilemap;
    /// <summary>Installed sprite compositions, rebound without restarting the scene.</summary>
    [NonSerialized] private CeresFlightSpritePresentation? spriteArtwork;
    /// <summary>Optional edited rear-view actor positions.</summary>
    [NonSerialized] private CeresFlightActorLayout? actorLayout;
    /// <summary>Mode 1 caption tiles progressively written during the SPACE COLONY title.</summary>
    private readonly ushort[] spaceColonyTilemap = new ushort[32 * 32];
    /// <summary>Front-view star actor whose timer also drives the diagonal background rush.</summary>
    private readonly IntroDiscoverySprite stars =
        CreateActor(CeresFlightActorDefinitions.FrontStars);

    /// <summary>Mode 7 scroll position advanced with the star actor or rear-view drift.</summary>
    private FixedPosition<Mode7Pixels> background = new(new(unchecked((ushort)-72), 0), new(unchecked((ushort)-104), 0));
    /// <summary>
    /// Mode 7 scale in 8.8 fixed point: 512 at the start, shrunk by 16 per update while at least 32 (ending at 16)
    /// for the front approach, then grown by 16 below 3072 and by 32 below 8192 for the rear view, ending at 8192.
    /// </summary>
    private int zoom = 512;
    /// <summary>Mode 7 rotation index used to derive the four matrix entries.</summary>
    private SnesAngle angle = SnesAngle.FromTableIndex(224);
    /// <summary>
    /// Master brightness from zero (black) to fifteen (full): held at zero until music is ready, set to fifteen, and
    /// lowered one level every other update in the final fade, stopping at zero.
    /// </summary>
    private int brightness;
    /// <summary>Five station and asteroid sprites created when the camera switches to rear view.</summary>
    internal IntroDiscoverySprite[] rearViewActors = [];
    /// <summary>
    /// The intensity of the rear-view flash's fixed color, from zero to 31: set to 31 when the rear view starts and
    /// lowered by one per update, stopping at zero. The native COLDATA writes select red, green and blue separately
    /// but always give them this same intensity.
    /// </summary>
    private int flashIntensity;
    /// <summary>Next caption glyph to add to the title tilemap.</summary>
    private int spaceColonyLetterIndex;
    /// <summary>Sixteen-frame glyph cadence and final $80-frame caption hold.</summary>
    private int spaceColonyTimer;
    /// <summary>Distinguishes the completed caption's final hold from its glyph-writing phase.</summary>
    private bool spaceColonyHoldStarted;
    /// <summary>Tracks the optional one-frame katakana caption record.</summary>
    private bool spaceColonyJapaneseCaptionDrawn;
    /// <summary>Selects the cartridge's alternate Japanese caption record.</summary>
    private readonly bool japaneseText;
    /// <summary>Gates one brightness decrement every other frame in the closing fade.</summary>
    private int fadeDelay;

    /// <summary>Loads installed flight art into PPU memory and leaves the scene in forced blank for the setup dispatch.</summary>
    /// <param name="audio">The shared music queue that $8B:BDE4 waits on.</param>
    /// <param name="artwork">Installed cinematic artwork; the approach cannot start without it.</param>
    /// <param name="japaneseText">Native <c>AltText</c>, which adds the katakana SPACE COLONY caption.</param>
    public IntroCeresFlightState(CartridgeAudioState audio,
        CeresFlightArtworkCatalog? artwork = null, bool japaneseText = false)
    {
        this.japaneseText = japaneseText;
        this.audio = Ensure.NotNull(audio);
        spriteArtwork = artwork?.Sprites;
        actorLayout = artwork?.Actors;

        CeresFlightArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Ceres approach requires installed cinematic artwork.");
        byte[] characters = content.Mode7Characters.ToArray();
        tilemap = content.Mode7Maps.ToArray();
        byte[] objectCharacters = content.ObjectCharacters.ToArray();
        RequireMinimum(characters, CeresFlightRomData.Vram.Mode7CharacterByteCount,
            "gunship/Ceres Mode 7 characters");
        RequireMinimum(tilemap, CeresFlightRomData.Vram.Mode7MapByteCount,
            "gunship front/rear Mode 7 tilemaps");
        RequireMinimum(objectCharacters, CeresFlightRomData.Vram.ObjectCharacterByteCount,
            "space/Ceres OBJ characters");

        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only the front-view map's first $300 bytes.
        new Mode7Vram(vram).LoadCharacters(characters.AsSpan(0,
            CeresFlightRomData.Vram.Mode7CharacterByteCount));
        new Mode7Vram(vram).FillMap(CeresFlightRomData.Vram.Mode7BlankMapTile,
            CeresFlightRomData.Vram.Mode7MapFillWordCount);
        new Mode7Vram(vram).LoadMap(tilemap.AsSpan(0,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        new CeresVram(vram).ObjectCharacters.Load(objectCharacters.AsSpan(0, CeresFlightRomData.Vram.ObjectCharacterByteCount));
        content.Palette.LoadTo(cgram);
        // The PPU loads above are invisible: the screen stays in forced blank until $8B:BDE4.
        // $8B:BCA0 itself runs as the next dispatch's cinematic function.
        Phase = IntroCeresFlightPhase.Initial;
    }

    /// <summary>Reapplies current external art and palette without resetting the live flight phase.</summary>
    public void BindArtwork(CeresFlightArtworkCatalog? artwork)
    {
        spriteArtwork = artwork?.Sprites;
        actorLayout = artwork?.Actors;
        if (artwork is null) return;
        tilemap = artwork.Mode7Maps.ToArray();
        new Mode7Vram(vram).LoadCharacters(artwork.Mode7Characters.Span);
        new Mode7Vram(vram).FillMap(CeresFlightRomData.Vram.Mode7BlankMapTile,
            CeresFlightRomData.Vram.Mode7MapFillWordCount);
        int mapOffset = Phase is IntroCeresFlightPhase.Initial or
            IntroCeresFlightPhase.WaitForMusicQueue or
            IntroCeresFlightPhase.FlyingIntoCamera
            ? 0 : CeresFlightRomData.Vram.Mode7MapSliceByteCount;
        new Mode7Vram(vram).LoadMap(tilemap.AsSpan(mapOffset,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        new CeresVram(vram).ObjectCharacters.Load(artwork.ObjectCharacters.Span);
        artwork.Palette.LoadTo(cgram);
    }

    /// <summary>Current native cinematic dispatch phase, including the final handoff to Ceres gameplay.</summary>
    public IntroCeresFlightPhase Phase { get; private set; }

    /// <summary>True after the native SPACE COLONY hold and final fade reach forced blank.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    /// <summary>Advances the music gate, camera, rear-view actors, title cadence, or final fade for one dispatch.</summary>
    public void Step()
    {
        switch (Phase)
        {
            case IntroCeresFlightPhase.Initial:
                // $8B:BDD2-$BDDF: the flight's music data, then its track, on the shared queue.
                audio.QueueMusicDelayed8(MusicCommand.LoadData(CeresFlightRomData.Music.DataIndex));
                audio.QueueMusicDelayed(
                    MusicCommand.SelectTrack(CeresFlightRomData.Music.Track),
                    MusicCommandDelay.FromDelayedYArgument(CeresFlightRomData.Music.TrackDelayArgument));
                Phase = IntroCeresFlightPhase.WaitForMusicQueue;
                break;

            case IntroCeresFlightPhase.WaitForMusicQueue:
                // $8B:BDE4 enables the display once CheckIfMusicIsQueued reports an empty queue.
                if (!audio.HasQueuedMusic)
                {
                    brightness = 15;
                    Phase = IntroCeresFlightPhase.FlyingIntoCamera;
                }
                break;

            case IntroCeresFlightPhase.FlyingIntoCamera:
                if (zoom >= 32)
                {
                    zoom -= 16;
                }
                else
                {
                    SetupRearView();
                }
                break;

            case IntroCeresFlightPhase.FlyingTowardCeres:
                StepFlyingTowardCeres();
                break;

            case IntroCeresFlightPhase.SpaceColonyTitle:
                StepSpaceColonyTitle();
                break;

            case IntroCeresFlightPhase.FadeOut:
                StepFadeOut();
                break;

            case IntroCeresFlightPhase.StartGameAtCeres:
                // $8B:C100 runs as the dispatch after forced blank; it hands control to state
                // $1F, whose owner also performs this function's area-six checkpoint save.
                Phase = IntroCeresFlightPhase.Finished;
                break;

            case IntroCeresFlightPhase.Finished:
                // The flight has ended; its owner moves to state $1F.
                break;

            default:
                throw new InvalidOperationException($"Undefined Ceres flight phase {Phase}.");
        }

        if (Phase == IntroCeresFlightPhase.FlyingIntoCamera)
            StepStarsAndBackground();

        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or
            IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut)
            StepRearViewActors();
        else if (Phase is not (IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished))
            stars.Step();
    }

    /// <summary>Captures matrix, ordered layers and rear-view fixed-color registers without rasterizing.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        OamBuffer oam = PrepareRenderOam();
        var layers = new List<RenderLayer> { new ObjPriorityRenderLayer(0) };
        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished)
        {
            var caption = new Bg4BppRenderLayer(CeresFlightRomData.Layers.SpaceColonyTilemapWord,
                CeresFlightRomData.Layers.SpaceColonyCharacterWord, new Position16<BgPlanePixels>(0, 0), new Extent16<TilemapCells>(32, 32), false);
            layers.Add(caption);
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(caption with { Priority = true });
        }
        else
        {
            var (a, b, c, d) = CalculateMatrix();
            layers.Add(new Mode7RenderLayer(new(a, b, c, d,
                new Offset16<Mode7Pixels>(CeresFlightRenderDefinitions.CenterX, CeresFlightRenderDefinitions.CenterY),
                background.Whole.AsSignedRegister())));
            layers.Add(new ObjPriorityRenderLayer(1));
        }
        layers.Add(new ObjPriorityRenderLayer(2));
        layers.Add(new ObjPriorityRenderLayer(3));
        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
        {
            byte flash = checked((byte)flashIntensity);
            layers.Add(new FixedColorAddRenderLayer(flash, flash, flash));
        }
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers.ToArray(),
            MenuRenderDefinitions.ObjectSelection, checked((byte)brightness));
    }

    /// <summary>Draws the front stars or surviving rear-view actors into the current OAM pass.</summary>
    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (IntroDiscoverySprite actor in rearViewActors)
                actor.Draw(oam, installedArt: spriteArtwork);
        else stars.Draw(oam, installedArt: spriteArtwork);
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Loads the rear map, places five actors, and primes the white fixed-color flash.</summary>
    private void SetupRearView()
    {
        // $BE22 queues the second $300-byte map immediately behind the front view in the
        // same decompressed stream. Only low bytes change; the shared character plane and
        // the surrounding tile-$8C star field remain resident.
        new Mode7Vram(vram).LoadMap(tilemap.AsSpan(CeresFlightRomData.Vram.Mode7MapSliceByteCount,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        background = new(new(unchecked((ushort)-32), 0), new(unchecked((ushort)-128), 0));
        angle = SnesAngle.FromTableIndex(32);

        // These five slots are the exact $BE3B-$BE5C spawn order. Their visual lists
        // remain cartridge streams; the compiled definitions supply callback identity,
        // initializer results and the physical motion applied by StepRearViewActors.
        rearViewActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount)
            .Select(index =>
            {
                CeresFlightActorDefinition definition =
                    CeresFlightActorDefinitions.RearViewActor(index);
                if (actorLayout is { } layout)
                {
                    CeresFlightActorPlacement placement = layout[index];
                    definition = definition with { Position = placement.Position };
                }
                return CreateActor(definition);
            })
            .ToArray();

        // CGADSUB=$31 adds the fixed colour to BG1, OBJ, and backdrop. $BE09 begins at
        // white and $BFDA removes one five-bit component step per frame until black.
        flashIntensity = 31;
        Phase = IntroCeresFlightPhase.FlyingTowardCeres;
    }

    /// <summary>Creates one cinematic actor with its authored timing and active pre-instruction.</summary>
    private static IntroDiscoverySprite CreateActor(CeresFlightActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.Position,
            definition.Attributes,
            definition.Program)
        {
            GeneralTimer = definition.InitialTimer,
        };
        actor.InstallPreInstruction(definition.ActivePreInstruction);
        return actor;
    }

    /// <summary>Fades fixed color and enlarges the rear-view Mode 7 image until the title begins.</summary>
    private void StepFlyingTowardCeres()
    {
        // The fixed-colour flash fades by one SNES component step per frame, clamped at zero
        // (the selector-only register values $20/$40/$80).
        flashIntensity = Math.Max(0, flashIntensity - 1);

        // $BFDA drifts BG1 left by an eighth of a pixel and grows the rear-of-gunship Mode 7
        // image in two native bands: +16 until 3072, then +32 until 8192.
        background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, -SnesSixteenSixteen.FromWhole(1) / 8));
        if (zoom < 3072)
            zoom += 16;
        else if (zoom < 8192)
            zoom += 32;
        else
            SetupSpaceColonyTitle();
    }

    /// <summary>Switches to the Mode 1 caption and writes its first glyph.</summary>
    private void SetupSpaceColonyTitle()
    {
        // `$8B:C03E` switches from Mode 7 to Mode 1, assigns BG1SC=$5C/BG1NBA=$06,
        // and starts BG object list `$8C:D629`. The rear-view cinematic actors are not
        // deleted, so Ceres and both asteroid layers remain behind the title text.
        Array.Clear(spaceColonyTilemap);
        spaceColonyLetterIndex = 0;
        spaceColonyHoldStarted = false;
        spaceColonyJapaneseCaptionDrawn = false;
        WriteNextSpaceColonyLetter();
        spaceColonyTimer = 16;
        Phase = IntroCeresFlightPhase.SpaceColonyTitle;
    }

    /// <summary>Writes title letters every sixteen frames, optionally adds katakana, then holds before fading.</summary>
    private void StepSpaceColonyTitle()
    {
        if (--spaceColonyTimer > 0)
            return;

        if (spaceColonyLetterIndex < SpaceColonyCaptionDefinitions.LetterCount)
        {
            WriteNextSpaceColonyLetter();
            spaceColonyTimer = 16;
            return;
        }

        // $8B:C096 skips the Japanese caption record for English text. Japanese text draws
        // it on a one-frame record before the hold.
        if (japaneseText && !spaceColonyJapaneseCaptionDrawn)
        {
            spaceColonyJapaneseCaptionDrawn = true;
            WriteSpaceColonyJapaneseCaption();
            spaceColonyTimer = 1;
            return;
        }

        // The list's final record holds the completed caption for $80 frames before
        // `$8B:C0A2` installs the ordinary fade owner.
        if (!spaceColonyHoldStarted)
        {
            spaceColonyHoldStarted = true;
            spaceColonyTimer = 128;
            return;
        }

        fadeDelay = 1;
        Phase = IntroCeresFlightPhase.FadeOut;
    }

    /// <summary>Writes one authored title glyph to the caption tilemap and VRAM.</summary>
    private void WriteNextSpaceColonyLetter()
    {
        (int column, ushort tile) = SpaceColonyCaptionDefinitions.Letter(spaceColonyLetterIndex++);
        const int CaptionRow = 24;
        spaceColonyTilemap[CaptionRow * 32 + column] = tile;
        new CeresVram(vram).SpaceColonyTilemap.Write(spaceColonyTilemap);
    }

    /// <summary>Writes the alternate Japanese caption below the English title.</summary>
    private void WriteSpaceColonyJapaneseCaption()
    {
        ReadOnlySpan<ushort> tiles = SpaceColonyCaptionDefinitions.JapaneseTiles;
        for (int index = 0; index < tiles.Length; index++)
            spaceColonyTilemap[(SpaceColonyCaptionDefinitions.JapaneseRow + index / SpaceColonyCaptionDefinitions.JapaneseWidth) * 32 +
                SpaceColonyCaptionDefinitions.JapaneseColumn + index % SpaceColonyCaptionDefinitions.JapaneseWidth] = tiles[index];
        new CeresVram(vram).SpaceColonyTilemap.Write(spaceColonyTilemap);
    }

    /// <summary>Lowers brightness every other dispatch until forced blank permits the Ceres handoff.</summary>
    private void StepFadeOut()
    {
        // `$C0A2` seeds both fade words with one. `$8B:C0C5` calls HandleFadingOut, which
        // therefore removes one INIDISP level every other frame and ends at forced blank.
        if (fadeDelay-- > 0)
            return;
        fadeDelay = 1;
        brightness = Math.Max(0, brightness - 1);
        if (brightness == 0)
            Phase = IntroCeresFlightPhase.StartGameAtCeres;
    }

    /// <summary>Moves each rear-view actor according to its compiled motion before stepping its sprite list.</summary>
    private void StepRearViewActors()
    {
        for (int index = 0; index < rearViewActors.Length; index++)
        {
            IntroDiscoverySprite actor = rearViewActors[index];
            CeresFlightActorDefinition definition =
                CeresFlightActorDefinitions.RearViewActor(index);
            if (actor.PreInstruction != definition.ActivePreInstruction)
            {
                throw new InvalidDataException(
                    $"Ceres flight actor {index} names invalid pre-instruction " +
                    $"{actor.PreInstruction}.");
            }
            if (definition.WrapX)
                AddWrappedX(actor, unchecked((ushort)definition.HorizontalDelta));
            else
                AddSignedX(actor, definition.HorizontalDelta);
            actor.Step();
        }
    }

    /// <summary>Adds a fractional X delta and wraps the actor within the native 512-pixel span.</summary>
    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        SnesFixedPosition x = IntroCinematicMotion.AddSixteenSixteen(actor.Position.X, 0, fractionalDelta);
        actor.Position = actor.Position.WithX(x.WithWhole((ushort)(x.Whole % 512)));
    }

    /// <summary>Applies signed 16.16 horizontal motion without the front-star wrap.</summary>
    private static void AddSignedX(IntroDiscoverySprite actor, int fixedDelta)
    {
        actor.Position = actor.Position.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(actor.Position.X, fixedDelta));
    }

    /// <summary>Accelerates the front stars and both Mode 7 scroll offsets along the same diagonal.</summary>
    private void StepStarsAndBackground()
    {
        // BEBE adds $0080 to a signed 8.8 speed, then applies that same growing delta to
        // the star actor and both Mode 7 offsets. This is why the ship rush feels diagonal.
        if (stars.PreInstruction !=
            CeresFlightActorDefinitions.FrontStars.ActivePreInstruction)
        {
            throw new InvalidDataException(
                $"Ceres front stars name invalid pre-instruction " +
                $"{stars.PreInstruction}.");
        }
        stars.GeneralTimer = unchecked((ushort)(stars.GeneralTimer +
            CeresFlightActorDefinitions.FrontStarAcceleration));
        var velocity = new SnesSignedEightEight(checked((ushort)stars.GeneralTimer));
        stars.Position = new(
            IntroCinematicMotion.AddEightEight(stars.Position.X, velocity),
            IntroCinematicMotion.AddEightEight(stars.Position.Y, velocity));
        background = new(
            IntroCinematicMotion.AddEightEight(background.X, velocity),
            IntroCinematicMotion.AddEightEight(background.Y, velocity));
    }

    /// <summary>Builds the rotated Mode 7 scale matrix from the signed sine table and current zoom.</summary>
    private (short A, short B, short C, short D) CalculateMatrix()
    {
        // $8532 reads signed 8-bit sine words from $A0:B443 and keeps product bits 8..23.
        // Reading the retail table makes the rotation agree at every one of 256 angles.
        short cosine = ReadSine(angle.AddRaw(SnesAngle.QuarterTurn.RawValue).TableIndex);
        short sine = ReadSine(angle.TableIndex);
        short a = Scale(cosine, zoom);
        short b = Scale(sine, zoom);
        return (a, b, unchecked((short)-b), a);
    }

    /// <summary>Reads the native signed sine sample for a Mode 7 angle index.</summary>
    internal static short ReadSine(byte tableIndex) => EnemyTrigonometryTables.SignedSine(tableIndex);

    /// <summary>Multiplies a signed trigonometric component by the scene's 8.8 scale.</summary>
    private static short Scale(short component, int scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, checked((short)scalar))));

    /// <summary>Rejects installed art streams shorter than the native transfer size.</summary>
    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected ${minimum:X}.");
    }
}
