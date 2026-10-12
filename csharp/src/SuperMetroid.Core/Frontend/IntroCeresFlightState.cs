using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Spatial;

namespace SuperMetroid.Core.Frontend;

/// <summary>Initial native Mode 7 leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    // OBJ layer reused across renders; the span overload clears it first. Never saved state.
    /// <summary>Stores object layer scratch.</summary>
    [NonSerialized] private Rgba32[]? objectLayerScratch;
    // Final frame, reused by every render: a returned frame is valid until this scene renders again.
    /// <summary>Stores frame buffer.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Stores bus.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Stores audio.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Stores vram.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Stores cgram.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>Stores tilemap.</summary>
    private byte[] tilemap;
    /// <summary>Stores sprite artwork.</summary>
    [NonSerialized] private CeresFlightSpritePresentation? spriteArtwork;
    /// <summary>Stores actor layout.</summary>
    [NonSerialized] private CeresFlightActorLayout? actorLayout;
    /// <summary>Stores space colony tilemap.</summary>
    private readonly ushort[] spaceColonyTilemap = new ushort[0x400];
    /// <summary>Stores stars.</summary>
    private readonly IntroDiscoverySprite stars =
        CreateActor(CeresFlightActorDefinitions.FrontStars);

    /// <summary>Stores background.</summary>
    private FixedPosition<Mode7Pixels> background = new(new(0xffb8, 0), new(0xff98, 0));
    /// <summary>Stores zoom.</summary>
    private ushort zoom = 0x0200;
    /// <summary>Stores angle.</summary>
    private SnesAngle angle = SnesAngle.FromTableIndex(0xe0);
    /// <summary>Stores brightness.</summary>
    private byte brightness;
    /// <summary>Stores rear view actors.</summary>
    private IntroDiscoverySprite[] rearViewActors = [];
    /// <summary>Stores fixed color red.</summary>
    private byte fixedColorRed = 0x20;
    /// <summary>Stores fixed color green.</summary>
    private byte fixedColorGreen = 0x40;
    /// <summary>Stores fixed color blue.</summary>
    private byte fixedColorBlue = 0x80;
    /// <summary>Stores space colony letter index.</summary>
    private int spaceColonyLetterIndex;
    /// <summary>Stores space colony timer.</summary>
    private int spaceColonyTimer;
    /// <summary>Stores space colony hold started.</summary>
    private bool spaceColonyHoldStarted;
    /// <summary>Stores space colony japanese caption drawn.</summary>
    private bool spaceColonyJapaneseCaptionDrawn;
    /// <summary>Stores japanese text.</summary>
    private readonly bool japaneseText;
    /// <summary>Stores fade delay.</summary>
    private int fadeDelay;

    /// <param name="bus">Address space the star and actor instruction lists step and draw through.</param>
    /// <param name="audio">The shared music queue that $8B:BDE4 waits on.</param>
    /// <param name="artwork">Installed cinematic artwork; the approach cannot start without it.</param>
    /// <param name="japaneseText">Native <c>AltText</c>, which adds the katakana SPACE COLONY caption.</param>
    public IntroCeresFlightState(ISnesAddressSpace bus, CartridgeAudioState audio,
        CeresFlightArtworkCatalog? artwork = null, bool japaneseText = false)
    {
        this.bus = Ensure.NotNull(bus);
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

    /// <summary>Gets or sets phase.</summary>
    public IntroCeresFlightPhase Phase { get; private set; }

    /// <summary>True after the native SPACE COLONY hold and final fade reach forced blank.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    /// <summary>step.</summary>
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
            stars.Step(instructionWord: CeresFlightSpriteInstructionDefinitions.ReadWord);
    }

    /// <summary>render.</summary>
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
            SnesMode7Renderer.CompositeViewport(pixels, vram, cgram, new Mode7RenderRegisters { MatrixA = matrixA, MatrixB = matrixB, MatrixC = matrixC, MatrixD = matrixD, Center = new Offset16<Mode7Pixels>(CeresFlightRenderDefinitions.CenterX, CeresFlightRenderDefinitions.CenterY), Scroll = background.Whole.AsSignedRegister() });
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
            layers.Add(new FixedColorAddRenderLayer((byte)(fixedColorRed & 31),
                (byte)(fixedColorGreen & 31), (byte)(fixedColorBlue & 31)));
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers.ToArray(),
            MenuRenderDefinitions.ObjectSelection, brightness);
    }

    /// <summary>prepare render oam.</summary>
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

    /// <summary>setup rear view.</summary>
    private void SetupRearView()
    {
        // $BE22 queues the second $300-byte map immediately behind the front view in the
        // same decompressed stream. Only low bytes change; the shared character plane and
        // the surrounding tile-$8C star field remain resident.
        new Mode7Vram(vram).LoadMap(tilemap.AsSpan(CeresFlightRomData.Vram.Mode7MapSliceByteCount,
            CeresFlightRomData.Vram.Mode7MapSliceByteCount));
        background = new(new(0xffe0, 0), new(0xff80, 0));
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
                    definition = definition with { Position = placement.Position };
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

    /// <summary>create actor.</summary>
    private static IntroDiscoverySprite CreateActor(CeresFlightActorDefinition definition)
    {
        var actor = new IntroDiscoverySprite(
            definition.Position,
            definition.Attributes,
            definition.InstructionList)
        {
            GeneralTimer = definition.InitialTimer,
        };
        actor.PreInstructionPointerForDiscovery(definition.ActivePreInstruction);
        return actor;
    }

    /// <summary>step flying toward ceres.</summary>
    private void StepFlyingTowardCeres()
    {
        // The fixed-colour flash fades by one SNES component step per frame, clamped at
        // the selector-only register values $20/$40/$80 (component zero).
        fixedColorRed = (byte)Math.Max(0x20, fixedColorRed - 1);
        fixedColorGreen = (byte)Math.Max(0x40, fixedColorGreen - 1);
        fixedColorBlue = (byte)Math.Max(0x80, fixedColorBlue - 1);

        // $BFDA drifts BG1 left by 0.2000 and grows the rear-of-gunship Mode 7 image in
        // two native bands: +$10 until $0C00, then +$20 until $2000.
        background = background.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(background.X, unchecked((int)0xffff_e000)));
        if (zoom < 0x0c00)
            zoom = unchecked((ushort)(zoom + 0x0010));
        else if (zoom < 0x2000)
            zoom = unchecked((ushort)(zoom + 0x0020));
        else
            SetupSpaceColonyTitle();
    }

    /// <summary>setup space colony title.</summary>
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
        spaceColonyTimer = 0x10;
        Phase = IntroCeresFlightPhase.SpaceColonyTitle;
    }

    /// <summary>step space colony title.</summary>
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
            spaceColonyTimer = 0x80;
            return;
        }

        fadeDelay = 1;
        Phase = IntroCeresFlightPhase.FadeOut;
    }

    /// <summary>write next space colony letter.</summary>
    private void WriteNextSpaceColonyLetter()
    {
        (int column, ushort tile) = SpaceColonyCaptionDefinitions.Letter(spaceColonyLetterIndex++);
        const int CaptionRow = 0x18;
        spaceColonyTilemap[CaptionRow * 32 + column] = tile;
        new CeresVram(vram).SpaceColonyTilemap.Write(spaceColonyTilemap);
    }

    /// <summary>write space colony japanese caption.</summary>
    private void WriteSpaceColonyJapaneseCaption()
    {
        ReadOnlySpan<ushort> tiles = SpaceColonyCaptionDefinitions.JapaneseTiles;
        for (int index = 0; index < tiles.Length; index++)
            spaceColonyTilemap[(SpaceColonyCaptionDefinitions.JapaneseRow + index / SpaceColonyCaptionDefinitions.JapaneseWidth) * 32 +
                SpaceColonyCaptionDefinitions.JapaneseColumn + index % SpaceColonyCaptionDefinitions.JapaneseWidth] = tiles[index];
        new CeresVram(vram).SpaceColonyTilemap.Write(spaceColonyTilemap);
    }

    /// <summary>step fade out.</summary>
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

    /// <summary>render space colony title.</summary>
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

    /// <summary>composite space colony priority.</summary>
    private void CompositeSpaceColonyPriority(Span<Rgba32> pixels, bool priority)
    {
        SnesBgTilemapRenderer.Composite4BppViewport(pixels, vram, cgram, new BgVramAddresses { TilemapBaseWord = CeresFlightRomData.Layers.SpaceColonyTilemapWord, CharacterBaseWord = CeresFlightRomData.Layers.SpaceColonyCharacterWord }, new BgViewport { Scroll = new Position16<BgPlanePixels>(0, 0), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(32, 32) }, priority: priority);
    }

    /// <summary>step rear view actors.</summary>
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
            actor.Step(instructionWord: CeresFlightSpriteInstructionDefinitions.ReadWord);
        }
    }

    /// <summary>add wrapped x.</summary>
    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        SnesFixedPosition x = IntroCinematicMotion.AddSixteenSixteen(actor.Position.X, 0, fractionalDelta);
        actor.Position = actor.Position.WithX(x.WithWhole((ushort)(x.Whole & 0x01ff)));
    }

    /// <summary>add signed x.</summary>
    private static void AddSignedX(IntroDiscoverySprite actor, int fixedDelta)
    {
        actor.Position = actor.Position.WithX(IntroCinematicMotion.AddSignedSixteenSixteen(actor.Position.X, fixedDelta));
    }

    /// <summary>apply rear view fixed color math.</summary>
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

    /// <summary>reduce to five bit.</summary>
    private static int ReduceToFiveBit(byte component) => (component * 31 + 127) / 255;

    /// <summary>expand five bit.</summary>
    private static byte ExpandFiveBit(int component) =>
        (byte)((component << 3) | (component >> 2));

    /// <summary>step stars and background.</summary>
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
        var velocity = new SnesSignedEightEight(stars.GeneralTimer);
        stars.Position = new(
            IntroCinematicMotion.AddEightEight(stars.Position.X, velocity),
            IntroCinematicMotion.AddEightEight(stars.Position.Y, velocity));
        background = new(
            IntroCinematicMotion.AddEightEight(background.X, velocity),
            IntroCinematicMotion.AddEightEight(background.Y, velocity));
    }

    /// <summary>calculate matrix.</summary>
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

    /// <summary>read sine.</summary>
    private static short ReadSine(byte tableIndex) => EnemyTrigonometryTables.SignedSine(tableIndex);

    /// <summary>scale.</summary>
    private static short Scale(short component, ushort scalar) =>
        unchecked((short)(SnesEightEight.Multiply(component, (short)scalar)));

    /// <summary>composite obj priority.</summary>
    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        // Name both optional arguments. A positional integer following `obsel` binds to
        // the renderer's width parameter, not its priority filter, and would request a
        // nonsensical zero-to-three-pixel framebuffer during the first Ceres frame.
        Rgba32[] layer = objectLayerScratch ??= new Rgba32[SnesPpuLayout.ScreenWidthPixels * SnesPpuLayout.ScreenHeightPixels];
        SnesObjRenderer.Render(layer, oam, vram, cgram, obsel: 3, priority: priority);
        SnesLayerCompositor.Composite(pixels, layer);
    }

    /// <summary>require minimum.</summary>
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
    /// <summary>Identifies flying into camera.</summary>
    FlyingIntoCamera,
    /// <summary>Identifies flying toward ceres.</summary>
    FlyingTowardCeres,
    /// <summary>Identifies space colony title.</summary>
    SpaceColonyTitle,
    /// <summary>Identifies fade out.</summary>
    FadeOut,
    /// <summary>$8B:C100 has run; game state $1F follows.</summary>
    Finished,
    /// <summary>$8B:BCA0: the PPU setup dispatch that queues the flight's music.</summary>
    Initial,
    /// <summary>$8B:C0C5 reached forced blank and installed $8B:C100 for the next dispatch.</summary>
    StartGameAtCeres,
}
