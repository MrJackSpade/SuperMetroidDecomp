using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;

namespace SuperMetroid.Core.Frontend;

/// <summary>Initial native Mode 7 leg of Samus's flight toward Ceres station.</summary>
internal sealed class IntroCeresFlightState
{
    private const int PaletteAddress = 0x8ce5e9;
    private const int Mode7CharacterAddress = 0x95a82f;
    private const int Mode7TilemapAddress = 0x96fe69;
    private const int ObjectCharacterAddress = 0x96d10a;
    private const ushort SpaceColonyTilemapWord = 0x5c00;
    private const ushort SpaceColonyCharacterWord = 0x6000;

    // `$8C:D629` writes one English character into BG1 every $10 frames. These are
    // complete SNES tilemap words: bit $2000 selects BG1's high-priority plane and the
    // low ten bits select the already-resident character from the cinematic OBJ set.
    private static readonly (int Column, ushort Tile)[] SpaceColonyLetters =
    [
        (0x0a, 0x21ed), // S
        (0x0b, 0x21ee), // P
        (0x0c, 0x21ef), // A
        (0x0d, 0x21f7), // C
        (0x0e, 0x21f8), // E
        (0x10, 0x21f7), // C
        (0x11, 0x21f9), // O
        (0x12, 0x21fa), // L
        (0x13, 0x21f9), // O
        (0x14, 0x21fb), // N
        (0x15, 0x21b9), // Y
    ];

    private readonly ISnesAddressSpace bus;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly byte[] tilemap;
    private readonly ushort[] spaceColonyTilemap = new ushort[0x400];
    private readonly IntroDiscoverySprite stars = new(
        xPosition: 0x0070,
        yPosition: 0x0057,
        paletteBits: SnesObjPalettes.Index4.PaletteBits,
        instructionPointer: CinematicCodePointers.Lists.CeresStars)
    {
        GeneralTimer = 0xfc00,
    };

    private ushort backgroundX = 0xffb8;
    private ushort backgroundXSubPosition;
    private ushort backgroundY = 0xff98;
    private ushort backgroundYSubPosition;
    private ushort zoom = 0x0200;
    private SnesAngle angle = SnesAngle.FromTableIndex(0xe0);
    private int musicQueueTimer = 14;
    private byte brightness;
    private IntroDiscoverySprite[] rearViewActors = [];
    private byte fixedColorRed = 0x20;
    private byte fixedColorGreen = 0x40;
    private byte fixedColorBlue = 0x80;
    private int spaceColonyLetterIndex;
    private int spaceColonyTimer;
    private bool spaceColonyHoldStarted;
    private int fadeDelay;

    public IntroCeresFlightState(ISnesAddressSpace bus)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));

        byte[] characters = RomDataReader.Decompress(bus, Mode7CharacterAddress, maximumOutputBytes: 0x4000);
        tilemap = RomDataReader.Decompress(bus, Mode7TilemapAddress, maximumOutputBytes: 0x1000);
        byte[] objectCharacters = RomDataReader.Decompress(bus, ObjectCharacterAddress, maximumOutputBytes: 0x4000);
        RequireMinimum(characters, 0x4000, "gunship/Ceres Mode 7 characters");
        RequireMinimum(tilemap, 0x0300, "gunship front Mode 7 tilemap");
        RequireMinimum(objectCharacters, 0x4000, "space/Ceres OBJ characters");

        // $BCCD writes all $4000 character bytes to $2119, fills every corresponding low
        // byte with tile $8C, then overwrites only the front-view map's first $300 bytes.
        vram.LoadMode7CharacterBytes(characters.AsSpan(0, 0x4000));
        vram.FillMode7MapBytes(0x8c, 0x4000);
        vram.LoadMode7MapBytes(tilemap.AsSpan(0, 0x0300));
        vram.LoadBytes(0xc000, objectCharacters.AsSpan(0, 0x4000));
        cgram.LoadFromBus(bus, PaletteAddress);
        Phase = IntroCeresFlightPhase.WaitForMusicQueue;
    }

    public IntroCeresFlightPhase Phase { get; private set; }

    /// <summary>True after the native SPACE COLONY hold and final fade reach forced blank.</summary>
    public bool Finished => Phase == IntroCeresFlightPhase.Finished;

    public void Step()
    {
        switch (Phase)
        {
            case IntroCeresFlightPhase.WaitForMusicQueue:
                // Track five is queued with the native fourteen-frame delayed command.
                if (--musicQueueTimer <= 0)
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
        }

        if (Phase == IntroCeresFlightPhase.FlyingIntoCamera)
            StepStarsAndBackground();

        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or
            IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut)
            StepRearViewActors();
        else if (Phase != IntroCeresFlightPhase.Finished)
            stars.Step(bus);
    }

    public Rgba32[] Render()
    {
        Rgba32[] pixels = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);

        OamBuffer oam = PrepareRenderOam();

        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or
            IntroCeresFlightPhase.FadeOut or
            IntroCeresFlightPhase.Finished)
        {
            RenderSpaceColonyTitle(pixels, oam);
        }
        else
        {
            (short matrixA, short matrixB, short matrixC, short matrixD) = CalculateMatrix();

            // Mode 7 has one BG priority between OBJ priorities zero and one in this PPU mode.
            CompositeObjPriority(pixels, oam, 0);
            Rgba32[] mode7 = SnesMode7Renderer.RenderViewport(
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
            SnesLayerCompositor.Composite(pixels, mode7);
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
        if (Phase is IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut or IntroCeresFlightPhase.Finished)
        {
            var caption = new Bg4BppRenderLayer(SpaceColonyTilemapWord, SpaceColonyCharacterWord, 0, 0, 32, 32, false);
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

    private OamBuffer PrepareRenderOam()
    {
        var oam = new OamBuffer();
        oam.BeginFrame();
        if (Phase is IntroCeresFlightPhase.FlyingTowardCeres or IntroCeresFlightPhase.SpaceColonyTitle or IntroCeresFlightPhase.FadeOut)
            foreach (IntroDiscoverySprite actor in rearViewActors) actor.Draw(bus, oam);
        else stars.Draw(bus, oam);
        oam.FinalizeFrame();
        return oam;
    }

    private void SetupRearView()
    {
        // $BE22 queues the second $300-byte map immediately behind the front view in the
        // same decompressed stream. Only low bytes change; the shared character plane and
        // the surrounding tile-$8C star field remain resident.
        vram.LoadMode7MapBytes(tilemap.AsSpan(0x0300, 0x0300));
        backgroundX = 0xffe0;
        backgroundXSubPosition = 0;
        backgroundY = 0xff80;
        backgroundYSubPosition = 0;
        angle = SnesAngle.FromTableIndex(0x20);

        // These five slots are the exact $BE3B-$BE5C spawn order. Their list pointers and
        // initial positions come from definitions $CF39/$CE85/$CE8B/$CE91/$CF0F; keeping
        // each as the shared bank-$8B interpreter preserves its ROM spritemap selection.
        rearViewActors =
        [
            CreateRearActor(0x0050, 0x009f, SnesObjPalettes.Index4,
                CinematicCodePointers.Lists.CeresExplosionLargeAsteroids),
            CreateRearActor(0x0074, 0x00a0, SnesObjPalettes.Index6,
                CinematicCodePointers.Lists.CeresUnderAttack),
            CreateRearActor(0x0080, 0x0060, SnesObjPalettes.Index4,
                CinematicCodePointers.Lists.CeresSmallAsteroids),
            CreateRearActor(0x00e0, 0x0057, SnesObjPalettes.Index4,
                CinematicCodePointers.Lists.CeresPurpleSpaceVortex),
            CreateRearActor(0xffe0, 0x0057, SnesObjPalettes.Index4,
                CinematicCodePointers.Lists.CeresStars),
        ];

        // CGADSUB=$31 adds the fixed colour to BG1, OBJ, and backdrop. $BE09 begins at
        // white and $BFDA removes one five-bit component step per frame until black.
        fixedColorRed = 0x3f;
        fixedColorGreen = 0x5f;
        fixedColorBlue = 0x9f;
        Phase = IntroCeresFlightPhase.FlyingTowardCeres;
    }

    private static IntroDiscoverySprite CreateRearActor(
        ushort x,
        ushort y,
        SnesObjAttributeWord palette,
        ushort instructionPointer) => new(x, y, palette, instructionPointer);

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

    private void StepSpaceColonyTitle()
    {
        if (--spaceColonyTimer > 0)
            return;

        if (spaceColonyLetterIndex < SpaceColonyLetters.Length)
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

    private void WriteNextSpaceColonyLetter()
    {
        (int column, ushort tile) = SpaceColonyLetters[spaceColonyLetterIndex++];
        const int CaptionRow = 0x18;
        spaceColonyTilemap[CaptionRow * 32 + column] = tile;
        vram.ExecuteWordTransfer(spaceColonyTilemap, SpaceColonyTilemapWord, 1);
    }

    private void StepFadeOut()
    {
        // `$C0A2` seeds both fade words with one. `AdvanceSlowScreenFadeOut` therefore
        // removes one INIDISP level every other frame and ends at forced blank.
        if (fadeDelay-- > 0)
            return;
        fadeDelay = 1;
        brightness = (byte)Math.Max(0, brightness - 1);
        if (brightness == 0)
            Phase = IntroCeresFlightPhase.Finished;
    }

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

    private void CompositeSpaceColonyPriority(Span<Rgba32> pixels, bool priority)
    {
        Rgba32[] caption = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            SpaceColonyTilemapWord,
            SpaceColonyCharacterWord,
            horizontalScroll: 0,
            verticalScroll: 0,
            width: 256,
            height: 224,
            tilemapWidthInTiles: 32,
            tilemapHeightInTiles: 32,
            priority: priority);
        SnesLayerCompositor.Composite(pixels, caption);
    }

    private void StepRearViewActors()
    {
        for (int index = 0; index < rearViewActors.Length; index++)
        {
            IntroDiscoverySprite actor = rearViewActors[index];
            switch (index)
            {
                case 0:
                    AddWrappedX(actor, 0x4000); // $BF35: +0.4000, modulo $200.
                    break;
                case 1:
                    AddWrappedX(actor, 0x1000); // $BF5F: +0.1000, modulo $200.
                    break;
                case 2:
                    AddWrappedX(actor, 0x0800); // $BF89: +0.0800, modulo $200.
                    break;
                case 3:
                case 4:
                    AddSignedX(actor, unchecked((int)0xffff_e000)); // $BFC6: -0.2000.
                    break;
            }
            actor.Step(bus);
        }
    }

    private static void AddWrappedX(IntroDiscoverySprite actor, ushort fractionalDelta)
    {
        ushort whole = actor.XPosition;
        ushort sub = actor.XSubPosition;
        IntroCinematicMotion.AddSixteenSixteen(ref whole, ref sub, 0, fractionalDelta);
        actor.XPosition = (ushort)(whole & 0x01ff);
        actor.XSubPosition = sub;
    }

    private static void AddSignedX(IntroDiscoverySprite actor, int fixedDelta)
    {
        ushort whole = actor.XPosition;
        ushort sub = actor.XSubPosition;
        AddSignedSixteenSixteen(ref whole, ref sub, fixedDelta);
        actor.XPosition = whole;
        actor.XSubPosition = sub;
    }

    private static void AddSignedSixteenSixteen(ref ushort whole, ref ushort sub, int fixedDelta)
    {
        ushort deltaSub = unchecked((ushort)fixedDelta);
        ushort deltaWhole = unchecked((ushort)(fixedDelta >> 16));
        IntroCinematicMotion.AddSixteenSixteen(ref whole, ref sub, deltaWhole, deltaSub);
    }

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

    private static int ReduceToFiveBit(byte component) => (component * 31 + 127) / 255;

    private static byte ExpandFiveBit(int component) =>
        (byte)((component << 3) | (component >> 2));

    private void StepStarsAndBackground()
    {
        // BEBE adds $0080 to a signed 8.8 speed, then applies that same growing delta to
        // the star actor and both Mode 7 offsets. This is why the ship rush feels diagonal.
        stars.GeneralTimer = unchecked((ushort)(stars.GeneralTimer + 0x0080));
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

    private static short ReadSine(byte tableIndex) => EnemyTrigonometryTables.SignedSine(tableIndex);

    private static short Scale(short component, ushort scalar) =>
        unchecked((short)((component * unchecked((short)scalar)) >> 8));

    private void CompositeObjPriority(Span<Rgba32> pixels, OamBuffer oam, int priority)
    {
        // Name both optional arguments. A positional integer following `obsel` binds to
        // the renderer's width parameter, not its priority filter, and would request a
        // nonsensical zero-to-three-pixel framebuffer during the first Ceres frame.
        Rgba32[] layer = SnesObjRenderer.Render(oam, vram, cgram, obsel: 3, priority: priority);
        SnesLayerCompositor.Composite(pixels, layer);
    }

    private static void RequireMinimum(byte[] bytes, int minimum, string name)
    {
        if (bytes.Length < minimum)
            throw new InvalidDataException($"The {name} stream expanded to ${bytes.Length:X}, expected ${minimum:X}.");
    }
}

internal enum IntroCeresFlightPhase
{
    WaitForMusicQueue,
    FlyingIntoCamera,
    FlyingTowardCeres,
    SpaceColonyTitle,
    FadeOut,
    Finished,
}
