using SuperMetroid.Core.Rooms.Backgrounds;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.CeresCinematics;
using SuperMetroid.Core.Frontend.CeresCinematics.Artwork;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.Core.Frontend.Intro;

/// <summary>Initial native Mode 7 leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    /// <summary>Holds the music queue whose drain gates the initial display.</summary>
    internal readonly CartridgeAudioState audio;
    /// <summary>The flight's colors, loaded from the installed palette (#1358).</summary>
    internal readonly ScreenPalette palette = new();
    /// <summary>The front-star or rear-view actors' sprite entries over the flight OBJ page (#1358).</summary>
    internal readonly ScreenSprites objects;
    /// <summary>Installed flight artwork the frame is drawn from, rebound after debugger restoration.</summary>
    [field: NonSerialized] internal CeresFlightArtworkCatalog? Artwork { get; private set; }
    /// <summary>The floor's cells for the front or the rear view of the bound artwork, until the view or the artwork changes.</summary>
    [NonSerialized] private byte[]? floorMap;
    /// <summary>Whether <see cref="floorMap"/> lays out the rear view.</summary>
    [NonSerialized] private bool floorMapIsRear;
    /// <summary>Installed sprite compositions, rebound without restarting the scene.</summary>
    [NonSerialized] private CinematicSpriteSheet? spriteArtwork;
    /// <summary>Optional edited rear-view actor positions.</summary>
    [NonSerialized] private CeresFlightActorLayout? actorLayout;
    /// <summary>Mode 1 caption tiles progressively written during the SPACE COLONY title.</summary>
    internal ushort[] SpaceColonyTilemap { get; } = new ushort[32 * 32];
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
    internal IntroDiscoverySprite[] RearViewActors { get; private set; } = [];
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
    /// <param name="sheets">The installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's tiles; required.</param>
    public IntroCeresFlightState(CartridgeAudioState audio,
        CeresFlightArtworkCatalog? artwork = null, bool japaneseText = false, InstalledSpriteSheets? sheets = null)
    {
        this.japaneseText = japaneseText;
        this.audio = Ensure.NotNull(audio);
        BindSheets(sheets ?? throw new InvalidOperationException("Ceres approach requires the installed sprite sheets."));
        actorLayout = artwork?.Actors;

        CeresFlightArtworkCatalog content = artwork ?? throw new InvalidOperationException(
            "Ceres approach requires installed cinematic artwork.");
        RequireMinimum(content.FloorCharacters.Length, CeresFlightRomData.Transfers.FloorCharacterByteCount,
            "gunship/Ceres Mode 7 characters");
        RequireMinimum(content.FloorMaps.Length, CeresFlightRomData.Transfers.FloorMapByteCount,
            "gunship front/rear Mode 7 tilemaps");
        RequireMinimum(content.ObjectCharacters.Length, CeresFlightRomData.Transfers.ObjectCharacterByteCount,
            "space/Ceres OBJ characters");
        Artwork = content;

        // $BCCD uploads the Mode 7 characters, the blank-filled front-view map and the OBJ characters;
        // the scene binds that artwork and its capture composes them.
        objects = new ScreenSprites();
        content.Palette.LoadTo(palette);
        // The PPU loads above are invisible: the screen stays in forced blank until $8B:BDE4.
        // $8B:BCA0 itself runs as the next dispatch's cinematic function.
        Phase = IntroCeresFlightPhase.Initial;
    }

    /// <summary>Rebinds the installed sheets: the Ceres flight sheet the stars and rear-view actors draw from, and the caption's tiles.</summary>
    public void BindSheets(InstalledSpriteSheets? sheets)
    {
        spriteArtwork = sheets is null ? null : new CinematicSpriteSheet(sheets.Get(SpriteSheetId.CeresFlightSprites), CinematicSpriteFamily.CeresFlight);
        captionTiles = sheets is null ? null : CeresScreenTiles.Flight(sheets);
        floorTiles = sheets is null ? null : CeresScreenTiles.FlightFloor(sheets);
    }

    /// <summary>What the Mode 7 view's tiles show (#1392); artwork, never saved.</summary>
    [NonSerialized] private Mode7TilePictures? floorTiles;

    /// <summary>What the caption's characters show (#1392); artwork, never saved.</summary>
    [NonSerialized] private ScreenLayerPictures? captionTiles;

    /// <summary>Reapplies current external art and palette without resetting the live flight phase.</summary>
    public void BindArtwork(CeresFlightArtworkCatalog? artwork)
    {
        actorLayout = artwork?.Actors;
        Artwork = artwork;
        floorMap = null;
        if (artwork is null) return;
        artwork.Palette.LoadTo(palette);
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
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        var layers = new List<RenderLayer> { new ObjPriorityRenderLayer(0) };
        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished)
        {
            // The caption draws from the flight's object characters' pre-coloured tiles (#1392).
            var caption = new TileRenderLayer(ScreenTileMap.Of(SpaceColonyTilemap, new Extent16<TilemapCells>(32, 32),
                captionTiles ?? throw new InvalidOperationException("The Ceres flight's caption requires the installed sprite sheets.")),
                new Position16<BgPlanePixels>(0, 0), false);
            layers.Add(caption);
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(caption with { Priority = true });
        }
        else
        {
            var (a, b, c, d) = FloorRotationMatrix.Calculate(angle, zoom);
            layers.Add(new Mode7RenderLayer(new(a, b, c, d,
                new Offset16<Mode7Pixels>(CeresFlightRenderDefinitions.CenterX, CeresFlightRenderDefinitions.CenterY),
                background.Whole.AsSignedRegister()),
                AffineFloor.Of(FloorMap(), floorTiles ?? throw new InvalidOperationException("The Ceres flight's Mode 7 view requires the installed sprite sheets."))));
            layers.Add(new ObjPriorityRenderLayer(1));
        }
        layers.Add(new ObjPriorityRenderLayer(2));
        layers.Add(new ObjPriorityRenderLayer(3));
        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
        {
            byte flash = checked((byte)flashIntensity);
            layers.Add(new FixedColorAddRenderLayer(flash, flash, flash));
        }
        return new(new FramePaint(palette.Colors, objects.Draws), layers.ToArray(), checked((byte)brightness));
    }

    /// <summary>Places the front stars or surviving rear-view actors as this frame's sprite entries.</summary>
    private void PrepareRenderSprites()
    {
        objects.BeginFrame();
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (IntroDiscoverySprite actor in RearViewActors)
                actor.Draw(objects, installedArt: spriteArtwork);
        else stars.Draw(objects, installedArt: spriteArtwork);
        objects.FinishFrame();
    }

    /// <summary>Whether the scene shows its rear view: once <c>$BE22</c> has queued the rear-view map.</summary>
    internal bool ShowsRearView => Phase is not (IntroCeresFlightPhase.Initial or IntroCeresFlightPhase.WaitForMusicQueue or
        IntroCeresFlightPhase.FlyingIntoCamera);

    /// <summary>The floor's cells: the blank tile everywhere, with the front or the rear view's map over the start.</summary>
    private byte[] FloorMap()
    {
        CeresFlightArtworkCatalog content = Artwork ?? throw new InvalidOperationException(
            "Ceres approach rendering requires installed cinematic artwork.");
        bool rear = ShowsRearView;
        if (floorMap is null || floorMapIsRear != rear)
        {
            var cells = new byte[AffineFloor.CellCount];
            cells.AsSpan(0, CeresFlightRomData.Transfers.FloorMapFillWordCount).Fill(CeresFlightRomData.Transfers.FloorBlankMapTile);
            int slice = rear ? CeresFlightRomData.Transfers.FloorMapSliceByteCount : 0;
            content.FloorMaps.Span.Slice(slice, CeresFlightRomData.Transfers.FloorMapSliceByteCount).CopyTo(cells);
            floorMap = cells;
            floorMapIsRear = rear;
        }
        return floorMap;
    }

    /// <summary>Loads the rear map, places five actors, and primes the white fixed-color flash.</summary>
    private void SetupRearView()
    {
        // $BE22 queues the second $300-byte map immediately behind the front view in the
        // same decompressed stream; the rear view's phase selects it in the composed image.
        background = new(new(unchecked((ushort)-32), 0), new(unchecked((ushort)-128), 0));
        angle = SnesAngle.FromTableIndex(32);

        // These five slots are the exact $BE3B-$BE5C spawn order. Their visual lists
        // remain cartridge streams; the compiled definitions supply callback identity,
        // initializer results and the physical motion applied by StepRearViewActors.
        RearViewActors = Enumerable.Range(0, CeresFlightActorDefinitions.RearViewActorCount)
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
        Array.Clear(SpaceColonyTilemap);
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

    /// <summary>Writes one authored title glyph to the caption tilemap.</summary>
    private void WriteNextSpaceColonyLetter()
    {
        (int column, int tile) = SpaceColonyCaptionDefinitions.Letter(spaceColonyLetterIndex++);
        const int CaptionRow = 24;
        SpaceColonyTilemap[CaptionRow * 32 + column] = checked((ushort)tile);
    }

    /// <summary>Writes the alternate Japanese caption below the English title.</summary>
    private void WriteSpaceColonyJapaneseCaption()
    {
        ReadOnlySpan<ushort> tiles = SpaceColonyCaptionDefinitions.JapaneseTiles;
        for (int index = 0; index < tiles.Length; index++)
            SpaceColonyTilemap[(SpaceColonyCaptionDefinitions.JapaneseRow + index / SpaceColonyCaptionDefinitions.JapaneseWidth) * 32 +
                SpaceColonyCaptionDefinitions.JapaneseColumn + index % SpaceColonyCaptionDefinitions.JapaneseWidth] = tiles[index];
    }

    /// <summary>Lowers brightness every other dispatch until forced blank permits the Ceres handoff.</summary>
    private void StepFadeOut()
    {
        // `$C0A2` seeds both fade words with one. `$8B:C0C5` calls HandleFadingOut, which
        // therefore removes one brightness level every other frame and ends with the screen blanked.
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
        for (int index = 0; index < RearViewActors.Length; index++)
        {
            IntroDiscoverySprite actor = RearViewActors[index];
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

    /// <summary>Rejects installed art streams shorter than the native transfer size.</summary>
    private static void RequireMinimum(int length, int minimum, string name)
    {
        if (length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${length:X}, expected ${minimum:X}.");
    }
}
