using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;

namespace SuperMetroid.Core.Frontend;

/// <summary>Initial native Mode 7 leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    /// <summary>Scratch OBJ layer reused for compositing; rendering clears it before drawing actors.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    /// <summary>Reusable composed frame; callers may retain its contents only until the next render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Cartridge address space used to step and draw cinematic actor instruction lists.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Shared audio state whose queued music gates the transition from forced blank.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>PPU video memory for the Mode 7 maps and object graphics used by this sequence.</summary>
    private readonly SnesVram vram = new();
    /// <summary>PPU palette memory used to render the flight and title scenes.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Source tilemap bytes retained so artwork can be rebound for the active view.</summary>
    private byte[] tilemap;
    /// <summary>Optional sprite presentation metadata bound from the installed cinematic artwork.</summary>
    [NonSerialized] private CeresFlightSpritePresentation? spriteArtwork;
    /// <summary>Optional actor placement metadata used to construct the rear-view cast.</summary>
    [NonSerialized] private CeresFlightActorLayout? actorLayout;
    /// <summary>32-by-32 word map built incrementally as the SPACE COLONY caption appears.</summary>
    private readonly ushort[] spaceColonyTilemap = new ushort[0x400];
    /// <summary>Front-view star actor whose accelerating movement also drives the Mode 7 scroll.</summary>
    private readonly IntroDiscoverySprite stars =
        CreateActor(CeresFlightActorDefinitions.FrontStars);

    /// <summary>Whole-pixel component of the signed Mode 7 horizontal scroll position.</summary>
    private ushort backgroundX = 0xffb8;
    /// <summary>Fractional component paired with <see cref="backgroundX"/> for 8.8 motion updates.</summary>
    private ushort backgroundXSubPosition;
    /// <summary>Whole-pixel component of the signed Mode 7 vertical scroll position.</summary>
    private ushort backgroundY = 0xff98;
    /// <summary>Fractional component paired with <see cref="backgroundY"/> for 8.8 motion updates.</summary>
    private ushort backgroundYSubPosition;
    /// <summary>Mode 7 scale used to construct the PPU affine matrix.</summary>
    private ushort zoom = 0x0200;
    /// <summary>Mode 7 rotation angle used when calculating the affine matrix.</summary>
    private SnesAngle angle = SnesAngle.FromTableIndex(0xe0);
    /// <summary>Current INIDISP brightness level, from forced blank through full brightness.</summary>
    private byte brightness;
    /// <summary>Actors retained behind the station caption and advanced during the rear-view flight.</summary>
    private IntroDiscoverySprite[] rearViewActors = [];
    /// <summary>SNES fixed-color selector and red component value used during the rear-view flash.</summary>
    private byte fixedColorRed = 0x20;
    /// <summary>SNES fixed-color selector and green component value used during the rear-view flash.</summary>
    private byte fixedColorGreen = 0x40;
    /// <summary>SNES fixed-color selector and blue component value used during the rear-view flash.</summary>
    private byte fixedColorBlue = 0x80;
    /// <summary>Index of the next SPACE COLONY glyph to add to the caption map.</summary>
    private int spaceColonyLetterIndex;
    /// <summary>Frames remaining before the next caption glyph or hold transition.</summary>
    private int spaceColonyTimer;
    /// <summary>Tracks whether the completed caption has entered its one-time display hold.</summary>
    private bool spaceColonyHoldStarted;
    /// <summary>Alternating-frame delay used while the final brightness fade runs.</summary>
    private int fadeDelay;

    /// <param name="bus">Address space the star and actor instruction lists step and draw through.</param>
    /// <param name="audio">The shared music queue that $8B:BDE4 waits on.</param>
    /// <param name="artwork">Installed cinematic artwork; the approach cannot start without it.</param>
    public IntroCeresFlightState(ISnesAddressSpace bus, CartridgeAudioState audio,
        CeresFlightArtworkCatalog? artwork = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
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
        vram.LoadMode7CharacterBytes(characters.AsSpan(0,
            CeresFlightRomData.Vram.Mode7CharacterByteCount));
        vram.FillMode7MapBytes(CeresFlightRomData.Vram.Mode7BlankMapTile,
            CeresFlightRomData.Vram.Mode7MapFillWordCount);
        vram.LoadMode7MapBytes(tilemap.AsSpan(0,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        vram.LoadBytes(CeresFlightRomData.Vram.ObjectCharacterDestinationByte,
            objectCharacters.AsSpan(0, CeresFlightRomData.Vram.ObjectCharacterByteCount));
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
        vram.LoadMode7CharacterBytes(artwork.Mode7Characters.Span);
        vram.FillMode7MapBytes(CeresFlightRomData.Vram.Mode7BlankMapTile,
            CeresFlightRomData.Vram.Mode7MapFillWordCount);
        int mapOffset = Phase is IntroCeresFlightPhase.Initial or
            IntroCeresFlightPhase.WaitForMusicQueue or
            IntroCeresFlightPhase.FlyingIntoCamera
            ? 0 : CeresFlightRomData.Vram.Mode7MapSliceByteCount;
        vram.LoadMode7MapBytes(tilemap.AsSpan(mapOffset,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        vram.LoadBytes(CeresFlightRomData.Vram.ObjectCharacterDestinationByte,
            artwork.ObjectCharacters.Span);
        artwork.Palette.LoadTo(cgram);
    }

    /// <summary>Current cinematic dispatch stage, including its forced-blank and music wait states.</summary>
    public IntroCeresFlightPhase Phase { get; private set; }

    /// <summary>True after the native SPACE COLONY hold and final fade reach forced blank.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    /// <summary>Advances the active cinematic stage by one gameplay dispatch.</summary>
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
                if (zoom >= 0x0020)
                {
                    zoom = unchecked((ushort)(zoom - 0x0010));
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
        }

        if (Phase == IntroCeresFlightPhase.FlyingIntoCamera)
            StepStarsAndBackground();

        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or
            IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut)
            StepRearViewActors();
        else if (Phase is not (IntroCeresFlightPhase.StartGameAtCeres or IntroCeresFlightPhase.Finished))
            stars.Step(bus, instructionWord: CeresFlightSpriteInstructionDefinitions.ReadWord);
    }

    /// <summary>Composes the current flight or title image into the scene's reusable frame buffer.</summary>
    /// <returns>The rendered pixels, valid until this state renders again.</returns>
    public Rgba32[] Render()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224, frameBuffer ??= new Rgba32[256 * 224]);

        OamBuffer oam = PrepareRenderOam();

        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.StartGameAtCeres or
            IntroCeresFlightPhase.Finished)
        {
            RenderSpaceColonyTitle(pixels, oam);
        }
        else
        {
            (short matrixA, short matrixB, short matrixC, short matrixD) = CalculateMatrix();

            // Mode 7 has one BG priority between OBJ priorities zero and one in this PPU mode.
            CompositeObjPriority(pixels, oam, 0);
            SnesMode7Renderer.CompositeViewport(
                pixels,
                vram,
                cgram,
                matrixA,
                matrixB,
                matrixC,
                matrixD,
                centerX: CeresFlightRenderDefinitions.CenterX,
                centerY: CeresFlightRenderDefinitions.CenterY,
                horizontalOffset: unchecked((short)backgroundX),
                verticalOffset: unchecked((short)backgroundY));
            CompositeObjPriority(pixels, oam, 1);
            CompositeObjPriority(pixels, oam, 2);
            CompositeObjPriority(pixels, oam, 3);
        }

        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
            ApplyRearViewFixedColorMath(pixels);

        if (brightness < 15)
        {
            for (int index = 0; index < pixels.Length; index++)
            {
                Rgba32 color = pixels[index];
                pixels[index] = new Rgba32(
                    (byte)(color.R * brightness / 15),
                    (byte)(color.G * brightness / 15),
                    (byte)(color.B * brightness / 15),
                    255);
            }
        }
        return pixels;
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
                CeresFlightRomData.Layers.SpaceColonyCharacterWord, 0, 0, 32, 32, false);
            layers.Add(caption);
            layers.Add(new ObjPriorityRenderLayer(1));
            layers.Add(caption with { Priority = true });
        }
        else
        {
            var (a, b, c, d) = CalculateMatrix();
            layers.Add(new Mode7RenderLayer(new(a, b, c, d,
                CeresFlightRenderDefinitions.CenterX, CeresFlightRenderDefinitions.CenterY,
                unchecked((short)backgroundX), unchecked((short)backgroundY))));
            layers.Add(new ObjPriorityRenderLayer(1));
        }
        layers.Add(new ObjPriorityRenderLayer(2));
        layers.Add(new ObjPriorityRenderLayer(3));
        if (Phase == IntroCeresFlightPhase.FlyingTowardCeres)
            layers.Add(new FixedColorAddRenderLayer((byte)(fixedColorRed & 31),
                (byte)(fixedColorGreen & 31), (byte)(fixedColorBlue & 31)));
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers.ToArray(),
            MenuRenderDefinitions.ObjectSelection, brightness);
    }

    /// <summary>Builds OAM entries for the actors visible in the current cinematic view.</summary>
    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (IntroDiscoverySprite actor in rearViewActors)
                actor.Draw(bus, oam, installedArt: spriteArtwork);
        else stars.Draw(bus, oam, installedArt: spriteArtwork);
        oam.FinalizeFrame();
        return oam;
    }

    /// <summary>Switches from the approaching camera view to the rear gunship view and initializes its actors.</summary>
    private void SetupRearView()
    {
        // $BE22 queues the second $300-byte map immediately behind the front view in the
        // same decompressed stream. Only low bytes change; the shared character plane and
        // the surrounding tile-$8C star field remain resident.
        vram.LoadMode7MapBytes(tilemap.AsSpan(CeresFlightRomData.Vram.Mode7MapSliceByteCount,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        backgroundX = 0xffe0;
        backgroundXSubPosition = 0;
        backgroundY = 0xff80;
        backgroundYSubPosition = 0;
        angle = SnesAngle.FromTableIndex(0x20);

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
                    definition = definition with
                    {
                        X = checked((ushort)placement.X),
                        Y = checked((ushort)placement.Y),
                    };
                }
                return CreateActor(definition);
            })
            .ToArray();

        // CGADSUB=$31 adds the fixed colour to BG1, OBJ, and backdrop. $BE09 begins at
        // white and $BFDA removes one five-bit component step per frame until black.
        fixedColorRed = 0x3f;
        fixedColorGreen = 0x5f;
        fixedColorBlue = 0x9f;
        Phase = IntroCeresFlightPhase.FlyingTowardCeres;
    }

    /// <summary>Creates a cinematic actor with its native instruction list, timer, and active callback.</summary>
    /// <param name="definition">The cartridge-derived initial placement and dispatch data for the actor.</param>
    /// <returns>An actor ready for instruction stepping.</returns>
    private static IntroDiscoverySprite CreateActor(CeresFlightActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.X,
            definition.Y,
            definition.Attributes,
            definition.InstructionList)
        {
            GeneralTimer = definition.InitialTimer,
        };
        actor.PreInstructionPointerForDiscovery(definition.ActivePreInstruction);
        return actor;
    }

    /// <summary>Fades the rear-view color flash and advances the gunship's scroll and zoom until the caption begins.</summary>
    private void StepFlyingTowardCeres()
    {
        // The fixed-colour flash fades by one SNES component step per frame, clamped at
        // the selector-only register values $20/$40/$80 (component zero).
        fixedColorRed = (byte)Math.Max(0x20, fixedColorRed - 1);
        fixedColorGreen = (byte)Math.Max(0x40, fixedColorGreen - 1);
        fixedColorBlue = (byte)Math.Max(0x80, fixedColorBlue - 1);

        // $BFDA drifts BG1 left by 0.2000 and grows the rear-of-gunship Mode 7 image in
        // two native bands: +$10 until $0C00, then +$20 until $2000.
        AddSignedSixteenSixteen(ref backgroundX, ref backgroundXSubPosition, unchecked((int)0xffff_e000));
        if (zoom < 0x0c00)
            zoom = unchecked((ushort)(zoom + 0x0010));
        else if (zoom < 0x2000)
            zoom = unchecked((ushort)(zoom + 0x0020));
        else
            SetupSpaceColonyTitle();
    }

    /// <summary>Initializes the Mode 1 caption map and starts the station-title sequence.</summary>
    private void SetupSpaceColonyTitle()
    {
        // `$8B:C03E` switches from Mode 7 to Mode 1, assigns BG1SC=$5C/BG1NBA=$06,
        // and starts BG object list `$8C:D629`. The rear-view cinematic actors are not
        // deleted, so Ceres and both asteroid layers remain behind the title text.
        Array.Clear(spaceColonyTilemap);
        spaceColonyLetterIndex = 0;
        spaceColonyHoldStarted = false;
        WriteNextSpaceColonyLetter();
        spaceColonyTimer = 0x10;
        Phase = IntroCeresFlightPhase.SpaceColonyTitle;
    }

    /// <summary>Times caption glyphs and the final hold before handing the scene to its fade.</summary>
    private void StepSpaceColonyTitle()
    {
        if (--spaceColonyTimer > 0)
            return;

        if (spaceColonyLetterIndex < SpaceColonyCaptionDefinitions.LetterCount)
        {
            WriteNextSpaceColonyLetter();
            spaceColonyTimer = 0x10;
            return;
        }

        // The final instruction in the English list holds the completed caption for $80
        // frames before `$8B:C0A2` installs the ordinary fade owner.
        if (!spaceColonyHoldStarted)
        {
            spaceColonyHoldStarted = true;
            spaceColonyTimer = 0x80;
            return;
        }

        fadeDelay = 1;
        Phase = IntroCeresFlightPhase.FadeOut;
    }

    /// <summary>Adds the next caption glyph to the tilemap and uploads its word to VRAM.</summary>
    private void WriteNextSpaceColonyLetter()
    {
        (int column, ushort tile) = SpaceColonyCaptionDefinitions.Letter(spaceColonyLetterIndex++);
        const int CaptionRow = 0x18;
        spaceColonyTilemap[CaptionRow * 32 + column] = tile;
        vram.ExecuteWordTransfer(spaceColonyTilemap,
            CeresFlightRomData.Layers.SpaceColonyTilemapWord, 1);
    }

    /// <summary>Applies the native alternating-frame brightness fade and advances when forced blank is reached.</summary>
    private void StepFadeOut()
    {
        // `$C0A2` seeds both fade words with one. `$8B:C0C5` calls HandleFadingOut, which
        // therefore removes one INIDISP level every other frame and ends at forced blank.
        if (fadeDelay-- > 0)
            return;
        fadeDelay = 1;
        brightness = (byte)Math.Max(0, brightness - 1);
        if (brightness == 0)
            Phase = IntroCeresFlightPhase.StartGameAtCeres;
    }

    /// <summary>Composites the caption and actor layers in Mode 1 priority order.</summary>
    /// <param name="pixels">Destination viewport receiving the completed title scene.</param>
    /// <param name="oam">Prepared actor entries interleaved with the caption's BG1 priorities.</param>
    private void RenderSpaceColonyTitle(Span<Rgba32> pixels, OamBuffer oam)
    {
        // Mode 1's ordinary ordering is sufficient here because every caption word uses
        // BG1 high priority. Keeping the empty low plane in the ladder documents why the
        // station/asteroid OBJ actors remain visible everywhere outside the glyphs.
        CompositeObjPriority(pixels, oam, 0);
        CompositeSpaceColonyPriority(pixels, priority: false);
        CompositeObjPriority(pixels, oam, 1);
        CompositeSpaceColonyPriority(pixels, priority: true);
        CompositeObjPriority(pixels, oam, 2);
        CompositeObjPriority(pixels, oam, 3);
    }

    /// <summary>Draws one priority plane of the SPACE COLONY background tilemap.</summary>
    /// <param name="pixels">Destination viewport to receive the selected background pixels.</param>
    /// <param name="priority">Selects low-priority pixels when false and high-priority pixels when true.</param>
    private void CompositeSpaceColonyPriority(Span<Rgba32> pixels, bool priority)
    {
        SnesBgTilemapRenderer.Composite4BppViewport(
            pixels,
            vram,
            cgram,
            CeresFlightRomData.Layers.SpaceColonyTilemapWord,
            CeresFlightRomData.Layers.SpaceColonyCharacterWord,
            horizontalScroll: 0,
            verticalScroll: 0,
            width: 256,
            height: 224,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    /// <summary>Applies each rear-view actor's native horizontal motion and advances its instruction list.</summary>
    private void StepRearViewActors()
    {
        for (int index = 0; index < rearViewActors.Length; index++)
        {
            IntroDiscoverySprite actor = rearViewActors[index];
            CeresFlightActorDefinition definition =
                CeresFlightActorDefinitions.RearViewActor(index);
            if (actor.PreInstructionPointer != definition.ActivePreInstruction)
            {
                throw new InvalidDataException(
                    $"Ceres flight actor {index} names invalid pre-instruction " +
                    $"$8B:{actor.PreInstructionPointer:X4}.");
            }
            if (definition.WrapX)
                AddWrappedX(actor, unchecked((ushort)definition.HorizontalDelta));
            else
                AddSignedX(actor, definition.HorizontalDelta);
            actor.Step(bus, instructionWord: CeresFlightSpriteInstructionDefinitions.ReadWord);
        }
    }

    /// <summary>Moves an actor horizontally and wraps its whole-pixel coordinate at the 9-bit screen boundary.</summary>
    /// <param name="actor">Actor whose fixed-point X position is updated.</param>
    /// <param name="fractionalDelta">Unsigned 8.8 movement amount used by the wrapped actor path.</param>
    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        ushort whole = actor.XPosition;
        ushort sub = actor.XSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(ref whole, ref sub, 0, fractionalDelta);
        actor.XPosition = (ushort)(whole & 0x01ff);
        actor.XSubPosition = sub;
    }

    /// <summary>Moves an actor horizontally by a signed 16.16 delta without screen wrapping.</summary>
    /// <param name="actor">Actor whose fixed-point X position is updated.</param>
    /// <param name="fixedDelta">Signed 16.16 displacement applied to the actor.</param>
    private static void AddSignedX(IntroDiscoverySprite actor, int fixedDelta)
    {
        ushort whole = actor.XPosition;
        ushort sub = actor.XSubPosition;
        AddSignedSixteenSixteen(ref whole, ref sub, fixedDelta);
        actor.XPosition = whole;
        actor.XSubPosition = sub;
    }

    /// <summary>Adds a signed 16.16 displacement to a whole/subposition pair with native carry behavior.</summary>
    /// <param name="whole">Reference to the whole-pixel word of the position.</param>
    /// <param name="sub">Reference to the fractional word of the position.</param>
    /// <param name="fixedDelta">Signed 16.16 displacement to add.</param>
    private static void AddSignedSixteenSixteen(ref ushort whole, ref ushort sub, int fixedDelta)
    {
        ushort deltaSub = unchecked((ushort)fixedDelta);
        ushort deltaWhole = unchecked((ushort)(fixedDelta >> 16));
        IntroCinematicMotion.AddSixteenSixteen(ref whole, ref sub, deltaWhole, deltaSub);
    }

    /// <summary>Adds the current five-bit fixed-color components to each rear-view pixel, clamping at white.</summary>
    /// <param name="pixels">Pixels modified in place using the SNES fixed-color values.</param>
    private void ApplyRearViewFixedColorMath(Span<Rgba32> pixels)
    {
        int addRed = fixedColorRed & 0x1f;
        int addGreen = fixedColorGreen & 0x1f;
        int addBlue = fixedColorBlue & 0x1f;
        for (int index = 0; index < pixels.Length; index++)
        {
            Rgba32 color = pixels[index];
            pixels[index] = new Rgba32(
                ExpandFiveBit(Math.Min(31, ReduceToFiveBit(color.R) + addRed)),
                ExpandFiveBit(Math.Min(31, ReduceToFiveBit(color.G) + addGreen)),
                ExpandFiveBit(Math.Min(31, ReduceToFiveBit(color.B) + addBlue)),
                255);
        }
    }

    /// <summary>Converts an eight-bit color component to its rounded five-bit SNES range.</summary>
    /// <param name="component">Eight-bit channel value.</param>
    /// <returns>The corresponding value from 0 through 31.</returns>
    private static int ReduceToFiveBit(byte component) => (component * 31 + 127) / 255;

    /// <summary>Expands a five-bit SNES color component to eight bits by replicating its high bits.</summary>
    /// <param name="component">Five-bit channel value.</param>
    /// <returns>The expanded eight-bit channel value.</returns>
    private static byte ExpandFiveBit(int component) =>
        (byte)((component << 3) | (component >> 2));

    /// <summary>Accelerates the front stars and applies their shared velocity to both Mode 7 scroll axes.</summary>
    private void StepStarsAndBackground()
    {
        // BEBE adds $0080 to a signed 8.8 speed, then applies that same growing delta to
        // the star actor and both Mode 7 offsets. This is why the ship rush feels diagonal.
        if (stars.PreInstructionPointer !=
            CeresFlightActorDefinitions.FrontStars.ActivePreInstruction)
        {
            throw new InvalidDataException(
                $"Ceres front stars name invalid pre-instruction " +
                $"$8B:{stars.PreInstructionPointer:X4}.");
        }
        stars.GeneralTimer = unchecked((ushort)(stars.GeneralTimer +
            CeresFlightActorDefinitions.FrontStarAcceleration));
        ushort velocity = stars.GeneralTimer;
        ushort starX = stars.XPosition;
        ushort starXSub = stars.XSubPosition;
        ushort starY = stars.YPosition;
        ushort starYSub = stars.YSubPosition;
        IntroCinematicMotion.AddEightEight(ref starX, ref starXSub, velocity);
        IntroCinematicMotion.AddEightEight(ref starY, ref starYSub, velocity);
        stars.XPosition = starX;
        stars.XSubPosition = starXSub;
        stars.YPosition = starY;
        stars.YSubPosition = starYSub;
        IntroCinematicMotion.AddEightEight(ref backgroundX, ref backgroundXSubPosition, velocity);
        IntroCinematicMotion.AddEightEight(ref backgroundY, ref backgroundYSubPosition, velocity);
    }

    /// <summary>Builds the signed Mode 7 affine matrix from the current angle and zoom.</summary>
    /// <returns>Matrix coefficients in the PPU's signed fixed-point representation.</returns>
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

    /// <summary>Reads one signed sine-table entry used by the native Mode 7 matrix calculation.</summary>
    /// <param name="tableIndex">Eight-bit index into the cartridge-compatible trigonometry table.</param>
    /// <returns>The signed sine component at that index.</returns>
    private static short ReadSine(byte tableIndex) => EnemyTrigonometryTables.SignedSine(tableIndex);

    /// <summary>Multiplies a signed matrix component by the scale and keeps the native result bits.</summary>
    /// <param name="component">Signed sine or cosine component.</param>
    /// <param name="scalar">Mode 7 scale factor.</param>
    /// <returns>The scaled signed coefficient.</returns>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));

    /// <summary>Renders one OAM priority band into a scratch layer and composites it over the scene.</summary>
    /// <param name="pixels">Destination viewport receiving the actor pixels.</param>
    /// <param name="oam">Prepared OAM entries for the current cinematic view.</param>
    /// <param name="priority">OAM priority band to render, from zero through three.</param>
    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        // Name both optional arguments. A positional integer following `obsel` binds to
        // the renderer's width parameter, not its priority filter, and would request a
        // nonsensical zero-to-three-pixel framebuffer during the first Ceres frame.
        Rgba32[] layer = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(layer, oam, vram, cgram, obsel: 3, priority: priority);
        SnesLayerCompositor.Composite(pixels, layer);
    }

    /// <summary>Rejects an extracted artwork stream that cannot cover the data consumed by the flight state.</summary>
    /// <param name="bytes">Expanded source stream to validate.</param>
    /// <param name="minimum">Required minimum stream length.</param>
    /// <param name="name">Asset label included in the failure message.</param>
    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected ${minimum:X}.");
    }
}

/// <remarks>Debugger states store these numerically, so new members are appended.</remarks>
internal enum IntroCeresFlightPhase
{
    /// <summary>$8B:BDE4: forced blank until the music queue drains.</summary>
    WaitForMusicQueue,
    /// <summary>Zooms the front-view starfield toward the camera before establishing the rear view.</summary>
    FlyingIntoCamera,
    /// <summary>Shows the rear gunship approach while the fixed-color flash fades and the scene zooms.</summary>
    FlyingTowardCeres,
    /// <summary>Reveals and holds the SPACE COLONY caption over the rear-view actors.</summary>
    SpaceColonyTitle,
    /// <summary>Fades the completed title scene to forced blank.</summary>
    FadeOut,
    /// <summary>$8B:C100 has run; game state $1F follows.</summary>
    Finished,
    /// <summary>$8B:BCA0: the PPU setup dispatch that queues the flight's music.</summary>
    Initial,
    /// <summary>$8B:C0C5 reached forced blank and installed $8B:C100 for the next dispatch.</summary>
    StartGameAtCeres,
}
