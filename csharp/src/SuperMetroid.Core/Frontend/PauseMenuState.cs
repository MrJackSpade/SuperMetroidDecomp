using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rom;
using SuperMetroid.Core.Runtime;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Cartridge-backed owner of bank-$82's pause map/equipment menu.
/// </summary>
/// <remarks>
/// This object deliberately owns only the pause-menu-local WRAM/PPU image. The outer
/// <see cref="SuperMetroidGame"/> dispatcher retains native game states $0C-$12, while the
/// live <see cref="SamusState"/> remains the sole owner of inventory words. Consequently an
/// equipment-screen A press changes the same bits consumed by movement, rendering, and SRAM;
/// there is no host-only copy that can diverge from gameplay.
/// </remarks>
internal sealed partial class PauseMenuState
{
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly MapPaletteAnimation paletteAnimation;
    private readonly SamusState samus;
    private readonly Bank80SystemState system;
    private readonly AreaId area;
    private readonly byte roomMapX;
    private readonly byte roomMapY;
    private readonly MapRevealMode mapRevealMode;
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly OamBuffer oam = new();
    private readonly byte[] equipmentTilemap;
    private readonly byte[] pauseButtonTilemap;
    private PauseMenuTransition transition;
    private int transitionBrightness = 15;
    private int selectedCategory;
    private int selectedItem;
    private ushort mapHorizontalScroll;
    private ushort mapVerticalScroll;
    private PauseMapScroll mapScroll = null!;
    private int mapIndicatorAnimationFrame;
    private int mapIndicatorAnimationTimer;
    private int itemSelectorAnimationFrame;
    private int itemSelectorAnimationTimer;
    private ushort lastIndicatorOriginX;
    private ushort lastIndicatorOriginY;
    private ushort lastIndicatorSpritemapId;

    public PauseMenuState(
        ISnesAddressSpace bus,
        SamusState samus,
        Bank80SystemState system,
        AreaId areaIndex,
        byte roomMapX,
        byte roomMapY,
        CartridgeAudioState? audio = null,
        SnesVram? gameplayVram = null,
        MapRevealMode mapRevealMode = MapRevealMode.None,
        AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.mapPresentation = mapPresentation;
        this.samus = samus ?? throw new ArgumentNullException(nameof(samus));
        this.system = system ?? throw new ArgumentNullException(nameof(system));
        this.audio = audio;
        paletteAnimation = new MapPaletteAnimation(bus);
        paletteAnimation.Bind(mapPresentation?.HighlightCycle);
        area = areaIndex;
        _ = AreaIds.ToIndex(areaIndex);
        this.roomMapX = roomMapX;
        this.roomMapY = roomMapY;
        this.mapRevealMode = Enum.IsDefined(mapRevealMode)
            ? mapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(mapRevealMode));

        // GameState_13 copies exactly these three cartridge ranges. VMADD is a word
        // address, hence the doubled byte destinations below.
        if (mapPresentation is null) vram.LoadBytes(0x0000, RomDataReader.ReadFixedBank(bus, PauseMenuRomData.BackgroundTiles, 0x4000));
        else
        {
            mapPresentation.Tiles.LoadTo(vram, 0);
            vram.LoadBytes(MapTileAtlasFormat.ByteCount, RomDataReader.ReadFixedBank(bus,
                PauseMenuRomData.BackgroundTiles + MapTileAtlasFormat.ByteCount, 0x4000 - MapTileAtlasFormat.ByteCount));
        }
        vram.LoadBytes(0x4000, RomDataReader.ReadFixedBank(bus, PauseMenuRomData.ObjectTiles, 0x2000));
        if (mapPresentation is null) vram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(bus, PauseMenuRomData.SamusObjectTiles, 0x2000));
        else mapPresentation.HudTiles.LoadTo(vram, HudTileAtlasFormat.DestinationWord * 2);
        vram.LoadBytes(
            PauseMenuLayout.Bg2TilemapWord * 2,
            RomDataReader.ReadFixedBank(bus, PauseMenuRomData.BackgroundTilemap, 0x0800));
        if (gameplayVram is not null)
        {
            // SetupPPUForPauseMenu changes BG3SC to $58 but never uploads a replacement
            // tilemap. The four-row gameplay HUD already resident at VRAM word $5800 is
            // intentionally retained. A fresh host-side VRAM object used to discard that
            // state, removing the entire energy/ammo HUD from pause and exposing map BG1
            // pixels in the area that its BG3 plane normally covers.
            var retainedHudTilemap = new byte[0x0800];
            for (int index = 0; index < retainedHudTilemap.Length; index++)
                retainedHudTilemap[index] = gameplayVram.ReadByte(0xb000 + index);
            vram.LoadBytes(0xb000, retainedHudTilemap);
        }

        // GameState_13 finishes pause setup by calling QueueClearingOfFxTilemap at
        // `$80:A211`. The accepted NMI fills VRAM words $5880-$5FFF with $184E, preserving
        // the first four HUD rows at $5800-$587F while blanking every row beneath them.
        // This clear is essential because pause replaces BG3's character sheet: an
        // untouched zero tilemap word selects pause character zero, which is the visible
        // orange `1` glyph. Copying only the live HUD without replaying this queued DMA
        // consequently tiled `1` through every otherwise-empty map cell.
        var clearedFxTilemap = new ushort[PauseMenuLayout.Bg3FxClearWordCount];
        Array.Fill(clearedFxTilemap, PauseMenuLayout.Bg3FxClearTile);
        vram.ExecuteWordTransfer(
            clearedFxTilemap,
            PauseMenuLayout.Bg3FxClearDestinationWord,
            wordIncrement: 1);
        if (mapPresentation is null) cgram.LoadFromBus(bus, PauseMenuRomData.Palette);
        else for (int color = 0; color < SnesCgram.ColorCount; color++) cgram.SetColor(color, mapPresentation.Palettes.Pause[color]);

        // LoadPauseScreenBaseTilemaps does *not* leave the bottom two button-label rows
        // solely in the $B6:E000 BG2 image. It keeps a mutable $B6:E400 copy at WRAM
        // $3400, recolors MAP/EQUIPMENT/START there, and queues $80 bytes from $3640 to
        // BG2 word $3B20. Omitting this second source is why the pause-screen chrome looked
        // like missing HUD. Keep the complete mutable source so every native word index
        // below remains directly comparable with bank $82.
        pauseButtonTilemap = RomDataReader.ReadFixedBank(bus, PauseMenuRomData.ButtonTilemap, 0x0400);
        SetPauseButtonLabelMode(0);

        // $B6:E800 is the mutable equipment template normally copied to $7E:3800.
        // Preserve it as a byte array because the cartridge's offset tables contain WRAM
        // byte addresses rather than tilemap word indexes.
        equipmentTilemap = RomDataReader.ReadFixedBank(bus, PauseMenuRomData.EquipmentTilemap, 0x0800);
        RebuildEquipmentTilemap();
        LoadPauseMapTilemap();
        SelectFirstCollectedEquipment();
        SetupMapScrolling();
    }

    /// <summary>Zero for the map and one for the equipment page, matching WRAM $0753.</summary>
    public int ScreenMode { get; private set; }

    /// <summary>Low byte of the native category/item selector word.</summary>
    public int SelectedCategory => selectedCategory;

    /// <summary>High byte of the native category/item selector word.</summary>
    public int SelectedItem => selectedItem;

    /// <summary>Number of cartridge OBJ records emitted by the most recent render.</summary>
    public int LastRenderedSpriteCount => oam.LastFinalizedSpriteCount;

    /// <summary>Native BG1 horizontal scroll selected while centering the pause map.</summary>
    public ushort MapHorizontalScroll => mapHorizontalScroll;

    /// <summary>Native BG1 vertical scroll selected while centering the pause map.</summary>
    public ushort MapVerticalScroll => mapVerticalScroll;

    /// <summary>Last screen-space origin passed to the cartridge menu-spritemap loader.</summary>
    public ushort LastIndicatorOriginX => lastIndicatorOriginX;

    /// <summary>Last screen-space Y origin passed to the cartridge menu-spritemap loader.</summary>
    public ushort LastIndicatorOriginY => lastIndicatorOriginY;

    /// <summary>Last bank-$82 menu spritemap ID selected by a pause draw routine.</summary>
    public ushort LastIndicatorSpritemapId => lastIndicatorSpritemapId;

    /// <summary>
    /// Reads one native $7E:3000-relative button-label word after its queued-equivalent
    /// upload. This narrow friend-test seam proves the bottom pause chrome reached VRAM;
    /// callers cannot mutate the private menu PPU.
    /// </summary>
    internal ushort ReadPauseButtonLabelWord(int nativeWordIndex)
    {
        const int firstUploadedNativeWordIndex = 0x0320;
        int uploadedWordOffset = nativeWordIndex - firstUploadedNativeWordIndex;
        if ((uint)uploadedWordOffset >= PauseMenuLayout.ButtonRowsByteCount / 2)
            throw new ArgumentOutOfRangeException(nameof(nativeWordIndex));
        int byteAddress = (PauseMenuLayout.ButtonRowsDestinationWord + uploadedWordOffset) * 2;
        return unchecked((ushort)(
            vram.ReadByte(byteAddress) | (vram.ReadByte(byteAddress + 1) << 8)));
    }

    /// <summary>
    /// Reads the displayed pause-map word for one absolute area-map coordinate. This is a
    /// diagnostic projection of the same BG1 VRAM image rendered by the pause screen.
    /// </summary>
    internal MapTileWord ReadDisplayedMapTile(int mapX, int mapY)
    {
        int wordIndex = AreaMapLayout.GetTilemapWordIndex(mapX, mapY);
        int byteAddress = (PauseMenuLayout.Bg1TilemapWord + wordIndex) * 2;
        return unchecked((ushort)(vram.ReadByte(byteAddress) | (vram.ReadByte(byteAddress + 1) << 8)));
    }

    /// <summary>
    /// Reads one of the three reserve-supply digits from the mutable equipment tilemap.
    /// This exposes rendered menu state to focused tests without exposing mutation.
    /// </summary>
    internal SnesBgTilemapWord ReadReserveSupplyDigit(int digitIndex)
    {
        if ((uint)digitIndex >= PauseMenuLayout.ReserveSupplyDigitCount)
            throw new ArgumentOutOfRangeException(nameof(digitIndex));

        int byteOffset = PauseMenuLayout.ReserveSupplyDigitsByteOffset + digitIndex * 2;
        return unchecked((ushort)(
            equipmentTilemap[byteOffset] | (equipmentTilemap[byteOffset + 1] << 8)));
    }

    /// <summary>
    /// Runs state-$0F menu input after the caller has latched NMI input and invoked the
    /// bank-$80 delayed-held filter. Returns true when Start requests game state $10.
    /// The runtime supplies its accepted-NMI byte counter for cartridge palette phase;
    /// standalone diagnostic callers may leave that phase at zero.
    /// </summary>
    public bool Step(ushort delayedHeldInput, ushort newlyPressedInput, ushort? heldInput = null, byte nmiFrameCounter8 = 0)
    {
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations(nmiFrameCounter8);
        SnesButton delayedPressed = (SnesButton)delayedHeldInput;
        SnesButton newlyPressed = (SnesButton)newlyPressedInput;
        if (transition != PauseMenuTransition.None)
        {
            StepPageTransition();
            return false;
        }

        // HandlePauseScreenStart runs from both stable pages. It is intentionally checked
        // after the delayed-held filter rather than from the raw NMI edge: a fresh press is
        // excluded for three frames by $80:8146 before becoming menu input.
        if ((delayedPressed & SnesButton.Start) != 0)
        {
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
            SetPauseButtonLabelMode(1);
            return true;
        }

        if (ScreenMode == 0)
        {
            // Map arrows read the ordinary held controller word, not the delayed chrome
            // input. An accepted pulse keeps running after release, as on the cartridge.
            if (mapScroll.Step(heldInput ?? delayedHeldInput, ref mapHorizontalScroll, ref mapVerticalScroll))
                audio?.QueueSound(SoundEffectLibrary1Sounds.MapScroll, maximumQueued: 6);
            if ((delayedPressed & SnesButton.R) != 0)
            {
                audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
                SetPauseButtonLabelMode(2);
                transition = PauseMenuTransition.MapToEquipmentFadeOut;
                transitionBrightness = 15;
            }
            return false;
        }

        if ((delayedPressed & SnesButton.L) != 0)
        {
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
            SetPauseButtonLabelMode(0);
            transition = PauseMenuTransition.EquipmentToMapFadeOut;
            transitionBrightness = 15;
            return false;
        }

        // EquipmentScreenMain consumes the ordinary $8F rising-edge word for D-pad/A;
        // only the shared L/R/Start chrome uses the delayed-held word at $05DF.
        HandleEquipmentInput(newlyPressed, nmiFrameCounter8);
        // EquipmentScreenMain refreshes the reserve amount independently of selected
        // label edits. Keep this per-frame write without rebuilding the whole tilemap,
        // which would erase the cartridge's same-frame VAR overrun.
        WriteReserveSupplyDigits();
        UploadEquipmentTilemap();
        return false;
    }

    /// <summary>
    /// Advances the sprite animation drawn by the current native pause-menu substate.
    /// </summary>
    /// <remarks>
    /// Rendering itself is intentionally pure. Bank $82 advances these counters from its
    /// draw routines, but the desktop may render a framebuffer more than once while paused
    /// (for example, a debugger watch or PNG capture). The dispatcher calls this exactly
    /// once per emulated frame so inspection cannot change cartridge-visible timing.
    /// </remarks>
    public void AdvanceAnimations(byte nmiFrameCounter8 = 0)
    {
        pauseNmiFrameCounter8 = nmiFrameCounter8;
        if (paletteAnimation.Step(cgram))
            audio?.QueueSound(SoundEffectLibrary3Sounds.MapPaletteLoop, maximumQueued: 6);
        if (ScreenMode == 0)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Composes the cartridge's BG2 frame and current BG1 page at 256x224.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        // SetupPpuForPauseMenu writes BGMODE=$09 and TM=$17. The old host compositor drew
        // complete BG2 followed by complete BG1, which ignored tile priority and allowed
        // the low-priority full-area map to paint over BG2's high-priority holder/chrome.
        // Build OAM first and then follow the literal Mode-1-with-BG3-priority ladder.
        PrepareRenderOam();
        ResolvedObjFrame objects = SnesObjRenderer.RenderResolved(
            oam, vram, cgram, PauseMenuLayout.ObjectSelection, 256, 224);

        // Back to front for BGMODE=$09:
        // OBJ0, BG3-low, OBJ1, BG2-low, BG1-low, OBJ2, BG2-high, BG1-high,
        // OBJ3, BG3-high. BG3's retained gameplay tilemap supplies the pause HUD.
        CompositeResolvedObjPriority(output, objects, priority: 0);
        CompositePauseBg3(output, priority: false);
        CompositeResolvedObjPriority(output, objects, priority: 1);
        CompositePauseBg(output, PauseMenuLayout.Bg2TilemapWord, 32, priority: false);
        CompositePauseBg(output, PauseMenuLayout.Bg1TilemapWord, 64, priority: false);
        CompositeResolvedObjPriority(output, objects, priority: 2);
        CompositePauseBg(output, PauseMenuLayout.Bg2TilemapWord, 32, priority: true);
        CompositePauseBg(output, PauseMenuLayout.Bg1TilemapWord, 64, priority: true);
        CompositeResolvedObjPriority(output, objects, priority: 3);
        CompositePauseBg3(output, priority: true);

        // Menu subindexes 2/4/5/7 fade the current page during an L/R transition. The
        // outer game-state fades are applied by SuperMetroidGame because they also cover
        // the gameplay frame before setup and after restoration.
        if (transition != PauseMenuTransition.None)
            MasterBrightnessFilter.Apply(output, transitionBrightness);
        return output;
    }

    /// <summary>Prepares OAM on the simulation owner, then captures without raster work.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        ushort mapX = ScreenMode == 0 ? mapHorizontalScroll : (ushort)0;
        ushort mapY = ScreenMode == 0 ? mapVerticalScroll : (ushort)0;
        var bg1 = new Bg4BppRenderLayer(PauseMenuLayout.Bg1TilemapWord, 0, mapX, mapY, 64, 32, false);
        var bg2 = new Bg4BppRenderLayer(PauseMenuLayout.Bg2TilemapWord, 0, 0, 0, 32, 32, false);
        var bg3 = new Bg2BppRenderLayer(SnesPpuLayout.GameplayHudTilemapWord,
            SnesPpuLayout.GameplayHudCharacterBaseWord,
            SnesPpuLayout.ScreenHeightPixels / SnesPpuLayout.BackgroundTileSizePixels, false);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), bg3,
            new ObjPriorityRenderLayer(1), bg2, bg1,
            new ObjPriorityRenderLayer(2), bg2 with { Priority = true }, bg1 with { Priority = true },
            new ObjPriorityRenderLayer(3), bg3 with { Priority = true },
        ];
        return new(PpuMemorySnapshot.Capture(vram, cgram, oam), layers,
            PauseMenuLayout.ObjectSelection, checked((byte)(transition == PauseMenuTransition.None ? 15 : transitionBrightness)));
    }

    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        if (ScreenMode == 0)
        {
            DrawMapPositionIndicator();
            // Native pause draws the same boss lists and defeated overlays as
            // file select, after the player marker. Use the live progression owner.
            new FileSelectMapIcons(bus, system, area).DrawBossMarkers(oam, mapHorizontalScroll, mapVerticalScroll);
        }
        else
        {
            DrawEquipmentItemSelector();
            DrawReserveTanks();
        }
        oam.FinalizeFrame();
    }

    private void CompositePauseBg(
        Span<Rgba32> output,
        ushort tilemapBaseWord,
        int tilemapWidthInTiles,
        bool priority)
    {
        bool isMapLayer = tilemapBaseWord == PauseMenuLayout.Bg1TilemapWord;
        SnesBgTilemapRenderer.Composite4BppViewport(
            output,
            vram,
            cgram,
            tilemapBaseWord,
            characterBaseWord: 0,
            horizontalScroll: isMapLayer && ScreenMode == 0 ? mapHorizontalScroll : (ushort)0,
            verticalScroll: isMapLayer && ScreenMode == 0 ? mapVerticalScroll : (ushort)0,
            width: 256,
            height: 224,
            tilemapWidthInTiles: tilemapWidthInTiles,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    private void CompositePauseBg3(Span<Rgba32> output, bool priority)
    {
        Rgba32[] plane = SnesBgTilemapRenderer.Render2Bpp(
            vram,
            cgram,
            tilemapBaseWord: 0x5800,
            characterBaseWord: 0x4000,
            rowCount: 28,
            transparentColorZero: true,
            priority: priority);
        SnesLayerCompositor.Composite(output, plane);
    }

    private static void CompositeResolvedObjPriority(
        Span<Rgba32> output,
        ResolvedObjFrame objects,
        byte priority)
    {
        for (int pixel = 0; pixel < output.Length; pixel++)
        {
            if (objects.Priorities[pixel] == priority)
                output[pixel] = objects.Pixels[pixel];
        }
    }

    private void StepPageTransition()
    {
        switch (transition)
        {
            case PauseMenuTransition.MapToEquipmentFadeOut:
            case PauseMenuTransition.EquipmentToMapFadeOut:
                transitionBrightness--;
                if (transitionBrightness > 0)
                    return;

                if (transition == PauseMenuTransition.MapToEquipmentFadeOut)
                {
                    ScreenMode = 1;
                    // Equipment setup lights the arrow when reserves are nonempty;
                    // subsequent tank dispatches own animation and selection changes.
                    if (samus.ReserveEnergy != 0) SetReserveArrow(enabled: true);
                    UploadEquipmentTilemap();
                    ResetItemSelectorAnimation();
                    transition = PauseMenuTransition.MapToEquipmentFadeIn;
                }
                else
                {
                    ScreenMode = 0;
                    LoadPauseMapTilemap();
                    transition = PauseMenuTransition.EquipmentToMapFadeIn;
                }
                transitionBrightness = 0;
                return;

            case PauseMenuTransition.MapToEquipmentFadeIn:
            case PauseMenuTransition.EquipmentToMapFadeIn:
                transitionBrightness++;
                if (transitionBrightness >= 15)
                {
                    transitionBrightness = 15;
                    transition = PauseMenuTransition.None;
                }
                return;

            default:
                throw new InvalidDataException($"Invalid pause-page transition {transition}.");
        }
    }

    private void SelectFirstCollectedEquipment()
    {
        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            PauseEquipmentCategoryDefinition category = PauseEquipmentCategories.Definitions[categoryIndex];
            ushort collected = GetCollectedBits(categoryIndex);
            for (int item = 0; item < category.ItemCount; item++)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedCategory = categoryIndex;
                selectedItem = item;
                return;
            }
        }

        selectedCategory = 0;
        selectedItem = 0;
    }

    private ushort GetCollectedBits(int category) =>
        category == 1 ? samus.CollectedBeams : samus.CollectedItems;

    private ushort GetEquippedBits(int category) =>
        category == 1 ? samus.EquippedBeams : samus.EquippedItems;

    private void RebuildEquipmentTilemap()
    {
        // Restore the literal base before applying inventory-dependent labels. This makes
        // repeated A toggles idempotent and mirrors re-entering LoadEquipmentScreen...
        RomDataReader.ReadFixedBank(bus, PauseMenuRomData.EquipmentTilemap, equipmentTilemap.Length)
            .CopyTo(equipmentTilemap, 0);

        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            PauseEquipmentCategoryDefinition category = PauseEquipmentCategories.Definitions[categoryIndex];
            ushort collected = GetCollectedBits(categoryIndex);
            ushort equipped = GetEquippedBits(categoryIndex);
            for (int item = 0; item < category.ItemCount; item++)
            {
                ushort destination = RomDataReader.ReadWordFixedBank(
                    bus,
                    category.OffsetTableAddress + item * 2);
                int destinationOffset = destination - 0x3800;
                int byteCount = category.LabelWordCount * 2;
                if (destinationOffset < 0 || destinationOffset + byteCount > equipmentTilemap.Length)
                {
                    throw new InvalidDataException(
                        $"Equipment label destination ${destination:X4} escapes $7E:3800-$3FFF.");
                }

                ushort mask = ReadCategoryMask(category, item);
                ushort source = (collected & mask) != 0
                    ? RomDataReader.ReadWordFixedBank(bus, category.TilemapPointerTableAddress + item * 2)
                    : PauseMenuRomData.BlankEquipmentTilemap;
                CopyBank82Words(source, equipmentTilemap.AsSpan(destinationOffset, byteCount));
                if ((collected & mask) != 0 && (equipped & mask) == 0)
                    RecolorLabel(equipmentTilemap.AsSpan(destinationOffset, byteCount));
            }
        }

        WriteSamusWireframe();
        WriteReserveLabels();
        WriteReserveSupplyDigits();
    }

    /// <summary>
    /// Translates <c>Load_EquipmentScreen_ReserveHealth_Tilemap</c> at $82:8F70.
    /// The cartridge displays current reserve supply, not maximum normal Energy Tank
    /// capacity. It leaves the authored template untouched until at least one Reserve Tank
    /// exists, then writes hundreds, tens, and ones as three consecutive BG1 words.
    /// </summary>
    private void WriteReserveSupplyDigits()
    {
        if (samus.MaxReserveEnergy == 0)
            return;

        int supply = samus.ReserveEnergy;
        Span<int> decimalPlaceValues = stackalloc int[]
        {
            supply / 100,
            supply % 100 / 10,
            supply % 10,
        };
        for (int digitIndex = 0; digitIndex < decimalPlaceValues.Length; digitIndex++)
        {
            ushort word = unchecked((ushort)(
                PauseMenuLayout.ReserveSupplyDigitZeroTile + decimalPlaceValues[digitIndex]));
            int byteOffset = PauseMenuLayout.ReserveSupplyDigitsByteOffset + digitIndex * 2;
            equipmentTilemap[byteOffset] = unchecked((byte)word);
            equipmentTilemap[byteOffset + 1] = unchecked((byte)(word >> 8));
        }
    }

    private void WriteSamusWireframe()
    {
        ushort desired = (ushort)(samus.EquippedItems & 0x0101);
        ushort sourcePointer = 0;
        for (int index = 0; index < 4; index++)
        {
            if (RomDataReader.ReadWordFixedBank(bus, PauseMenuRomData.EquipmentSetTable + index * 2) != desired)
                continue;
            sourcePointer = RomDataReader.ReadWordFixedBank(
                bus,
                PauseMenuRomData.EquipmentTilemapPatchPointerTable + index * 2);
            break;
        }
        if (sourcePointer == 0)
            throw new InvalidDataException($"Pause wireframe table has no entry for items ${desired:X4}.");

        int sourceAddress = 0x820000 | sourcePointer;
        int destinationOffset = 472;
        for (int row = 0; row < 17; row++)
        {
            for (int column = 0; column < 8; column++)
            {
                ushort word = RomDataReader.ReadWordFixedBank(bus, sourceAddress);
                equipmentTilemap[destinationOffset + column * 2] = unchecked((byte)word);
                equipmentTilemap[destinationOffset + column * 2 + 1] = unchecked((byte)(word >> 8));
                sourceAddress = 0x820000 | ((sourceAddress + 2) & 0xffff);
            }
            destinationOffset += 64;
        }
    }

    private void LoadPauseMapTilemap()
    {
        int areaIndex = AreaIds.ToIndex(area);
        IAreaMapView map = mapPresentation?.Get(area) ?? (IAreaMapView)AreaMapRomData.Load(bus, area);
        byte[] mapTilemap = AreaMapTilemapBuilder.Build(
            map, system, MapTileWords.PauseBlank, mapRevealMode);
        vram.LoadBytes(PauseMenuLayout.Bg1TilemapWord * 2, mapTilemap);

        // The area name is a 24-byte bank-$82 tilemap fragment copied to VMADD $38AA.
        ushort labelPointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.AreaMapLabelPointerTable + areaIndex * 2);
        vram.LoadBytes(0x38aa * 2, RomDataReader.ReadFixedBank(bus, 0x820000 | labelPointer, 0x18));
    }

    private void SetupMapScrolling()
    {
        int areaIndex = AreaIds.ToIndex(area);
        // DetermineMapScrollLimits at $82:9EC4 scans either the downloaded cartridge map
        // or the persistent explored plane. Expressing the scan in coordinates is exactly
        // equivalent to its byte/bit loops and makes the two-page 64x32 layout explicit.
        bool hasAreaMap = system.HasAreaMap(areaIndex);
        IAreaMapView map = mapPresentation?.Get(area) ?? (IAreaMapView)AreaMapRomData.Load(bus, area);
        bool IsVisible(int x, int y)
        {
            bool explored = system.IsMapTileExplored(areaIndex, x, y);
            // Native scroll bounds select one bit plane, unlike tile rendering, which
            // also displays explored secrets. Unioning the planes moves the map's center
            // after a secret is explored even though the downloaded bounds are unchanged.
            if (mapRevealMode == MapRevealMode.None)
                return hasAreaMap ? map.IsRevealedByMapStation(x, y) : explored;
            return AreaMapVisibility.IsVisible(
                explored,
                hasAreaMap,
                map.IsRevealedByMapStation(x, y),
                map.IsDiscoverable(x, y),
                mapRevealMode);
        }

        int left = 26;
        int right = 28;
        int top = 1;
        int bottom = 11;
        for (int x = 0; x < 64; x++)
        {
            if (!Enumerable.Range(0, 32).Any(y => IsVisible(x, y)))
                continue;
            left = x;
            break;
        }
        for (int x = 63; x >= 0; x--)
        {
            if (!Enumerable.Range(0, 32).Any(y => IsVisible(x, y)))
                continue;
            right = x;
            break;
        }
        // Native vertical scans return their defaults before checking the final row
        // in either direction. Match those boundary cases for sparse explored maps.
        for (int y = 0; y < 31; y++)
        {
            if (!Enumerable.Range(0, 64).Any(x => IsVisible(x, y)))
                continue;
            top = y;
            break;
        }
        for (int y = 31; y > 0; y--)
        {
            if (!Enumerable.Range(0, 64).Any(x => IsVisible(x, y)))
                continue;
            bottom = y;
            break;
        }

        ushort minimumX = unchecked((ushort)(left * 8 - (area == AreaId.Maridia ? 24 : 0)));
        ushort maximumX = unchecked((ushort)(right * 8));
        ushort minimumY = unchecked((ushort)(top * 8));
        ushort maximumY = unchecked((ushort)(bottom * 8));
        mapScroll = new PauseMapScroll(minimumX, maximumX, minimumY, maximumY);

        // SetupMapScrollingForPauseMenu($80) uses 16-bit ADC/SBC throughout. These local
        // helpers retain wrapping before every signed branch so an edge-of-map position
        // behaves like the 65C816 rather than like unbounded host integer arithmetic.
        mapHorizontalScroll = unchecked((ushort)(
            minimumX + unchecked((ushort)(maximumX - minimumX)) / 2 - 128));
        ushort playerMapX = unchecked((ushort)(8 * (roomMapX + (samus.XPosition >> 8))));
        ushort horizontalScreenPosition = unchecked((ushort)(playerMapX - mapHorizontalScroll));
        short distanceFromRightClamp = unchecked((short)(224 - horizontalScreenPosition));
        if (distanceFromRightClamp >= 0)
        {
            ushort distanceFromLeftClamp = unchecked((ushort)(32 - horizontalScreenPosition));
            if (unchecked((short)distanceFromLeftClamp) >= 0)
                mapHorizontalScroll = unchecked((ushort)(mapHorizontalScroll - distanceFromLeftClamp));
        }
        else
        {
            mapHorizontalScroll = unchecked((ushort)(mapHorizontalScroll - distanceFromRightClamp));
        }

        ushort verticalMiddle = unchecked((ushort)(
            minimumY + unchecked((ushort)(maximumY - minimumY)) / 2 + 16));
        ushort verticalCenterOffset = unchecked((ushort)((0x80 - verticalMiddle) & 0xfff8));
        mapVerticalScroll = unchecked((ushort)-verticalCenterOffset);
        ushort playerMapY = unchecked((ushort)(
            8 * (roomMapY + (samus.YPosition >> 8) + 1) + verticalCenterOffset));
        short distanceFromTopClamp = unchecked((short)(64 - playerMapY));
        if (distanceFromTopClamp >= 0)
        {
            mapVerticalScroll = unchecked((ushort)(mapVerticalScroll - distanceFromTopClamp));
            if (unchecked((short)(mapVerticalScroll + 40)) < 0)
                mapVerticalScroll = unchecked((ushort)-40);
        }
    }

    private void DrawMapPositionIndicator()
    {
        ushort x = unchecked((ushort)(
            8 * (roomMapX + (samus.XPosition >> 8)) - mapHorizontalScroll));
        ushort y = unchecked((ushort)(
            8 * (roomMapY + (samus.YPosition >> 8) + 1) - mapVerticalScroll));
        lastIndicatorOriginX = x;
        lastIndicatorOriginY = y;
        lastIndicatorSpritemapId = PauseMapIndicatorAnimation.SpritemapIds[mapIndicatorAnimationFrame];
        DrawMenuSpritemap(
            lastIndicatorSpritemapId,
            x,
            y,
            ReadPauseSpritePaletteBits());
    }

    private void StepMapIndicatorAnimation()
    {
        // The native timer starts at zero, advances to frame one on the first draw, then
        // decrements after reloading. This order is intentionally not a conventional
        // "draw frame zero for N ticks" animation helper.
        if (mapIndicatorAnimationTimer == 0)
        {
            mapIndicatorAnimationFrame = (mapIndicatorAnimationFrame + 1) & 3;
            mapIndicatorAnimationTimer = PauseMapIndicatorAnimation.FrameDelays[mapIndicatorAnimationFrame];
        }
        mapIndicatorAnimationTimer--;
    }

    private void ResetItemSelectorAnimation()
    {
        itemSelectorAnimationFrame = 0;
        itemSelectorAnimationTimer = bus.ReadByte(PauseMenuRomData.ItemSelectorAnimationTimer);
    }

    private void StepItemSelectorAnimation()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        // DrawPauseScreenSpriteAnim(3) selects the third timer/frame pair. Its animation
        // list is a cartridge pointer, with three-byte entries (delay, unused, ID offset).
        itemSelectorAnimationTimer--;
        if (itemSelectorAnimationTimer > 0)
            return;

        ushort animationPointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.ItemSelectorAnimationPointer);
        itemSelectorAnimationFrame++;
        byte duration = bus.ReadByte(
            0x820000 | ((animationPointer + itemSelectorAnimationFrame * 3) & 0xffff));
        if (duration == 0xff)
        {
            itemSelectorAnimationFrame = 0;
            duration = bus.ReadByte((int)new SnesAddress(0x82, animationPointer));
        }
        itemSelectorAnimationTimer = duration;
    }

    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        ushort positionListPointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.EquipmentSelectorPositionPointerTable + selectedCategory * 2);
        int positionAddress = 0x820000 | ((positionListPointer + selectedItem * 4) & 0xffff);
        ushort x = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, positionAddress) - 1));
        ushort y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, positionAddress + 2) - 1));

        ushort animationPointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.ItemSelectorAnimationPointer);
        int animationEntry = 0x820000 | ((animationPointer + itemSelectorAnimationFrame * 3) & 0xffff);
        byte spritemapOffset = bus.ReadByte(animationEntry + 2);

        // The third variable pointer used by DrawPauseScreenSpriteAnim is WRAM $0755, the
        // packed equipment selector. The important 65C816 detail is operand width: unlike
        // the timer/frame dereferences above, the source dereference is an eight-bit load.
        // It therefore selects by the low-byte category only; using the whole $0302 Bombs
        // selector walks into the following map-icon data and invents spritemap ID $00CA.
        ushort animationVariantPointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.ItemSelectorAnimationVariantPointer);
        if (animationVariantPointer != 0x0755)
        {
            throw new InvalidDataException(
                $"Pause item-selector animation variable is ${animationVariantPointer:X4}, " +
                "expected native WRAM $0755.");
        }
        ushort baseTablePointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.EquipmentSelectorBaseTablePointer);
        ushort baseSpritemapId = RomDataReader.ReadWordFixedBank(
            bus,
            0x820000 | ((baseTablePointer + selectedCategory * 2) & 0xffff));
        ushort spritemapId = unchecked((ushort)(baseSpritemapId + spritemapOffset));
        if (spritemapId > 0x0064)
        {
            throw new InvalidDataException(
                $"Pause selector category/item ${selectedItem:X2}{selectedCategory:X2} resolved animation " +
                $"${animationPointer:X4}/frame {itemSelectorAnimationFrame}/offset " +
                $"${spritemapOffset:X2} and base table ${baseTablePointer:X4} to " +
                $"invalid menu spritemap ${spritemapId:X4}.");
        }
        lastIndicatorOriginX = x;
        lastIndicatorOriginY = y;
        lastIndicatorSpritemapId = spritemapId;
        DrawMenuSpritemap(
            spritemapId,
            x,
            y,
            ReadPauseSpritePaletteBits());
    }

    private ushort ReadPauseSpritePaletteBits() =>
        RomDataReader.ReadWordFixedBank(bus, PauseMenuRomData.SelectedItemSpritemapPointer);

    /// <summary>
    /// Ports the three SetPauseScreenButtonLabelPalettes variants at $82:A628-$A84C and
    /// immediately performs UpdatePauseMenuLRStartVramTilemap's $80-byte upload.
    /// </summary>
    private void SetPauseButtonLabelMode(int mode)
    {
        // These indexes are words relative to native WRAM $3000. The mutable cartridge
        // template begins at $3400, or word index $200; subtract that origin before
        // indexing the local byte array.
        const int sourceWordOrigin = 0x0200;
        void SetPalette(int nativeWordIndex, int wordCount, ushort paletteBits)
        {
            int localWordIndex = nativeWordIndex - sourceWordOrigin;
            for (int word = 0; word < wordCount; word++)
            {
                int byteOffset = (localWordIndex + word) * 2;
                ushort tile = unchecked((ushort)(
                    pauseButtonTilemap[byteOffset] |
                    (pauseButtonTilemap[byteOffset + 1] << 8)));
                tile = unchecked((ushort)((tile & 0xe3ff) | paletteBits));
                pauseButtonTilemap[byteOffset] = unchecked((byte)tile);
                pauseButtonTilemap[byteOffset + 1] = unchecked((byte)(tile >> 8));
            }
        }

        switch (mode)
        {
            case 0: // Stable map page: MAP bright, EQUIPMENT/START dim.
                SetPalette(822, 5, 0x0800);
                SetPalette(854, 5, 0x0800);
                SetPalette(812, 4, 0x0800);
                SetPalette(844, 4, 0x0800);
                SetPalette(805, 5, 0x1400);
                SetPalette(837, 5, 0x1400);
                break;

            case 1: // Start/unpause highlight.
                SetPalette(812, 4, 0x0800);
                SetPalette(844, 4, 0x0800);
                SetPalette(805, 5, 0x1400);
                SetPalette(837, 5, 0x1400);
                SetPalette(822, 5, 0x1400);
                SetPalette(854, 5, 0x1400);
                break;

            case 2: // Stable equipment page: EQUIPMENT bright, MAP/START dim.
                SetPalette(805, 5, 0x0800);
                SetPalette(837, 5, 0x0800);
                SetPalette(812, 4, 0x0800);
                SetPalette(844, 4, 0x0800);
                SetPalette(822, 5, 0x1400);
                SetPalette(854, 5, 0x1400);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Pause label mode must be 0..2.");
        }

        vram.LoadBytes(
            PauseMenuLayout.ButtonRowsDestinationWord * 2,
            pauseButtonTilemap.AsSpan(
                PauseMenuLayout.ButtonRowsSourceOffset,
                PauseMenuLayout.ButtonRowsByteCount));
    }

    private void DrawMenuSpritemap(ushort id, ushort x, ushort y, ushort paletteBits)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            PauseMenuRomData.SpritemapPointerTable + id * 2);
        oam.AddOnScreenSpritemap(bus, 0x820000 | pointer, x, y, paletteBits);
    }

    private void UploadEquipmentTilemap() =>
        vram.LoadBytes(PauseMenuLayout.Bg1TilemapWord * 2, equipmentTilemap);

    private ushort ReadCategoryMask(PauseEquipmentCategoryDefinition category, int item) =>
        RomDataReader.ReadWordFixedBank(bus, category.BitmaskTableAddress + item * 2);

    private void CopyBank82Words(ushort sourcePointer, Span<byte> destination)
    {
        int source = 0x820000 | sourcePointer;
        for (int index = 0; index < destination.Length; index++)
            destination[index] = bus.ReadByte(
                (int)new SnesAddress(0x82, unchecked((ushort)(source + index))));
    }

    private static void RecolorLabel(Span<byte> bytes)
    {
        for (int offset = 0; offset < bytes.Length; offset += 2)
        {
            var word = new SnesBgTilemapWord(unchecked((ushort)(
                bytes[offset] | (bytes[offset + 1] << 8))))
                .WithPaletteIndex(PauseMenuLayout.DisabledEquipmentPaletteIndex);
            bytes[offset] = unchecked((byte)word.Raw);
            bytes[offset + 1] = unchecked((byte)(word.Raw >> 8));
        }
    }

}

internal enum PauseMenuTransition
{
    None,
    MapToEquipmentFadeOut,
    MapToEquipmentFadeIn,
    EquipmentToMapFadeOut,
    EquipmentToMapFadeIn,
}
