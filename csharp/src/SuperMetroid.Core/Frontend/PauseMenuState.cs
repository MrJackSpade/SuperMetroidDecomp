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
internal sealed class PauseMenuState
{
    private const ushort Bg1TilemapWord = 0x3000;
    private const ushort Bg2TilemapWord = 0x3800;
    private const ushort BlankEquipmentTilemapPointer = 0xc01a;
    private const ushort DisabledEquipmentPaletteBits = 0x0c00;
    private const ushort TilePaletteMask = 0x1c00;
    private const int MenuSpritemapPointerTableAddress = 0x82c569;
    private const byte PauseObjectSelection = 0x01;

    // UpdateSamusPositionIndicatorAnimation at $82:B9FC uses these four literal frames.
    // Keeping the uneven 8/4/8/4 cadence matters: the two narrow middle frames are the
    // transition between the left- and right-facing halves of the map marker.
    private static readonly ushort[] MapIndicatorSpritemapIds = [0x5f, 0x60, 0x61, 0x60];
    private static readonly int[] MapIndicatorFrameDelays = [8, 4, 8, 4];

    private static readonly EquipmentCategoryDefinition[] EquipmentCategories =
    [
        // Category zero is reserve tanks. Its two special controls do not exist on the
        // early-game route because maximum reserve energy is zero.
        new(0, 0, 0, 0, 0),
        new(0x82c06c, 0x82c08c, 0x82c04c, 5, 5), // Beams.
        new(0x82c076, 0x82c096, 0x82c056, 6, 9), // Suits/misc, including Morph and Bombs.
        new(0x82c082, 0x82c0a2, 0x82c062, 3, 9), // Boots.
    ];

    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState? audio;
    private readonly SamusState samus;
    private readonly Bank80SystemState system;
    private readonly byte areaIndex;
    private readonly byte roomMapX;
    private readonly byte roomMapY;
    private readonly SnesVram vram = new();
    private readonly SnesCgram cgram = new();
    private readonly OamBuffer oam = new();
    private readonly byte[] equipmentTilemap;
    private PauseMenuTransition transition;
    private int transitionBrightness = 15;
    private int selectedCategory;
    private int selectedItem;
    private ushort mapHorizontalScroll;
    private ushort mapVerticalScroll;
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
        byte areaIndex,
        byte roomMapX,
        byte roomMapY,
        CartridgeAudioState? audio = null)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.samus = samus ?? throw new ArgumentNullException(nameof(samus));
        this.system = system ?? throw new ArgumentNullException(nameof(system));
        this.audio = audio;
        this.areaIndex = areaIndex < 7 ? areaIndex : (byte)0;
        this.roomMapX = roomMapX;
        this.roomMapY = roomMapY;

        // GameState_13 copies exactly these three cartridge ranges. VMADD is a word
        // address, hence the doubled byte destinations below.
        vram.LoadBytes(0x0000, RomDataReader.ReadFixedBank(bus, 0xb68000, 0x4000));
        vram.LoadBytes(0x4000, RomDataReader.ReadFixedBank(bus, 0xb6c000, 0x2000));
        vram.LoadBytes(0x8000, RomDataReader.ReadFixedBank(bus, 0x9ab200, 0x2000));
        vram.LoadBytes(Bg2TilemapWord * 2, RomDataReader.ReadFixedBank(bus, 0xb6e000, 0x0800));
        cgram.LoadFromBus(bus, 0xb6f000);

        // $B6:E800 is the mutable equipment template normally copied to $7E:3800.
        // Preserve it as a byte array because the cartridge's offset tables contain WRAM
        // byte addresses rather than tilemap word indexes.
        equipmentTilemap = RomDataReader.ReadFixedBank(bus, 0xb6e800, 0x0800);
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
    /// Runs state-$0F menu input after the caller has latched NMI input and invoked the
    /// bank-$80 delayed-held filter. Returns true when Start requests game state $10.
    /// </summary>
    public bool Step(ushort delayedHeldInput, ushort newlyPressedInput)
    {
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations();
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
            audio?.QueueSound(library: 1, soundId: 0x38, maximumQueued: 6);
            return true;
        }

        if (ScreenMode == 0)
        {
            if ((delayedPressed & SnesButton.R) != 0)
            {
                audio?.QueueSound(library: 1, soundId: 0x38, maximumQueued: 6);
                transition = PauseMenuTransition.MapToEquipmentFadeOut;
                transitionBrightness = 15;
            }
            return false;
        }

        if ((delayedPressed & SnesButton.L) != 0)
        {
            audio?.QueueSound(library: 1, soundId: 0x38, maximumQueued: 6);
            transition = PauseMenuTransition.EquipmentToMapFadeOut;
            transitionBrightness = 15;
            return false;
        }

        // EquipmentScreenMain consumes the ordinary $8F rising-edge word for D-pad/A;
        // only the shared L/R/Start chrome uses the delayed-held word at $05DF.
        HandleEquipmentInput(newlyPressed);
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
    public void AdvanceAnimations()
    {
        if (ScreenMode == 0)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Composes the cartridge's BG2 frame and current BG1 page at 256x224.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        Rgba32[] bg2 = SnesBgTilemapRenderer.Render4BppViewport(
            vram, cgram, Bg2TilemapWord, 0, 0, 0, 256, 224, 32, 32);
        Rgba32[] bg1 = SnesBgTilemapRenderer.Render4BppViewport(
            vram,
            cgram,
            Bg1TilemapWord,
            0,
            ScreenMode == 0 ? mapHorizontalScroll : (ushort)0,
            ScreenMode == 0 ? mapVerticalScroll : (ushort)0,
            256,
            224,
            64,
            32);
        SnesLayerCompositor.Composite(output, bg2);
        SnesLayerCompositor.Composite(output, bg1);

        // SetupPpuForPauseMenu writes OBSEL=$01. Pause indicators use the same bank-$82
        // spritemap table as file select, but the different object base points at the
        // $B6:8000 pause characters loaded into VRAM word $0000/$2000 above.
        oam.BeginFrame();
        if (ScreenMode == 0)
            DrawMapPositionIndicator();
        else
            DrawEquipmentItemSelector();
        oam.FinalizeFrame();
        SnesLayerCompositor.Composite(
            output,
            SnesObjRenderer.Render(oam, vram, cgram, PauseObjectSelection));

        // Menu subindexes 2/4/5/7 fade the current page during an L/R transition. The
        // outer game-state fades are applied by SuperMetroidGame because they also cover
        // the gameplay frame before setup and after restoration.
        if (transition != PauseMenuTransition.None)
            MasterBrightnessFilter.Apply(output, transitionBrightness);
        return output;
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

    private void HandleEquipmentInput(SnesButton pressed)
    {
        if (selectedCategory is < 1 or > 3)
            return;

        EquipmentCategoryDefinition category = EquipmentCategories[selectedCategory];
        ushort collected = GetCollectedBits(selectedCategory);

        if ((pressed & SnesButton.Up) != 0)
        {
            for (int item = selectedItem - 1; item >= 0; item--)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedItem = item;
                audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                return;
            }
        }
        else if ((pressed & SnesButton.Down) != 0)
        {
            for (int item = selectedItem + 1; item < category.ItemCount; item++)
            {
                if ((collected & ReadCategoryMask(category, item)) == 0)
                    continue;
                selectedItem = item;
                audio?.QueueSound(library: 1, soundId: 0x37, maximumQueued: 6);
                return;
            }
        }
        else if ((pressed & SnesButton.A) != 0)
        {
            ushort mask = ReadCategoryMask(category, selectedItem);
            if ((collected & mask) == 0)
                return;

            audio?.QueueSound(library: 1, soundId: 0x38, maximumQueued: 6);

            // EquipmentScreenCategory_ButtonResponse toggles the live equipped word, then
            // recolors exactly this label. Rebuilding all labels is equivalent and avoids
            // retaining a second authoritative equipment state in the pause object.
            if (selectedCategory == 1)
                samus.EquippedBeams ^= mask;
            else
                samus.EquippedItems ^= mask;
            RebuildEquipmentTilemap();
            UploadEquipmentTilemap();
        }
    }

    private void SelectFirstCollectedEquipment()
    {
        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            EquipmentCategoryDefinition category = EquipmentCategories[categoryIndex];
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
        RomDataReader.ReadFixedBank(bus, 0xb6e800, equipmentTilemap.Length)
            .CopyTo(equipmentTilemap, 0);

        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
        {
            EquipmentCategoryDefinition category = EquipmentCategories[categoryIndex];
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
                    : BlankEquipmentTilemapPointer;
                CopyBank82Words(source, equipmentTilemap.AsSpan(destinationOffset, byteCount));
                if ((collected & mask) != 0 && (equipped & mask) == 0)
                    RecolorLabel(equipmentTilemap.AsSpan(destinationOffset, byteCount));
            }
        }

        WriteSamusWireframe();
    }

    private void WriteSamusWireframe()
    {
        ushort desired = (ushort)(samus.EquippedItems & 0x0101);
        ushort sourcePointer = 0;
        for (int index = 0; index < 4; index++)
        {
            if (RomDataReader.ReadWordFixedBank(bus, 0x82b257 + index * 2) != desired)
                continue;
            sourcePointer = RomDataReader.ReadWordFixedBank(bus, 0x82b25f + index * 2);
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
        // kPauseMenuMapTilemaps is a table of long pointers to literal 64x32 tilemaps.
        // `$82:943D` dereferences words from that ROM image while applying explored-map
        // visibility; it does not call the decompressor. Treating $B5:9000 as compressed
        // therefore walks unrelated data looking for an impossible command terminator.
        int mapPointer = RomDataReader.ReadLongFixedBank(bus, 0x82964a + areaIndex * 3);
        byte[] mapTilemap = RomDataReader.ReadFixedBank(bus, mapPointer, 0x1000);
        ushort mapDataPointer = RomDataReader.ReadWordFixedBank(
            bus,
            0x829717 + areaIndex * 2);
        int mapDataAddress = 0x820000 | mapDataPointer;
        bool hasAreaMap = system.HasAreaMap(areaIndex);

        // `$82:943D` combines three independent cartridge structures: the literal 64x32
        // tilemap, the one-bit "a room exists here" map, and the persistent one-bit
        // exploration plane. A downloaded map reveals existing-but-unvisited rooms with
        // palette $0400; without one, only visited cells survive. Applying this when the
        // page is loaded also means a save/reload cannot turn the pause screen into the
        // fully revealed debug map used by the first implementation.
        for (int tilemapIndex = 0; tilemapIndex < 0x800; tilemapIndex++)
        {
            int pageIndex = tilemapIndex & 0x3ff;
            int mapX = pageIndex % 32 + (tilemapIndex >= 0x400 ? 32 : 0);
            int mapY = pageIndex / 32;
            bool explored = system.IsMapTileExplored(areaIndex, mapX, mapY);
            bool exists = ReadMapBit(mapDataAddress, mapX, mapY);
            int byteOffset = tilemapIndex * 2;
            ushort word = unchecked((ushort)(
                mapTilemap[byteOffset] | (mapTilemap[byteOffset + 1] << 8)));
            if (explored)
                word &= 0xfbff;
            else if (!hasAreaMap || !exists)
                word = 0x001f;
            mapTilemap[byteOffset] = unchecked((byte)word);
            mapTilemap[byteOffset + 1] = unchecked((byte)(word >> 8));
        }
        vram.LoadBytes(Bg1TilemapWord * 2, mapTilemap);

        // The area name is a 24-byte bank-$82 tilemap fragment copied to VMADD $38AA.
        ushort labelPointer = RomDataReader.ReadWordFixedBank(bus, 0x82965f + areaIndex * 2);
        vram.LoadBytes(0x38aa * 2, RomDataReader.ReadFixedBank(bus, 0x820000 | labelPointer, 0x18));
    }

    private void SetupMapScrolling()
    {
        // DetermineMapScrollLimits at $82:9EC4 scans either the downloaded cartridge map
        // or the persistent explored plane. Expressing the scan in coordinates is exactly
        // equivalent to its byte/bit loops and makes the two-page 64x32 layout explicit.
        bool useCartridgeMap = system.HasAreaMap(areaIndex);
        ushort mapDataPointer = RomDataReader.ReadWordFixedBank(bus, 0x829717 + areaIndex * 2);
        int mapDataAddress = 0x820000 | mapDataPointer;
        bool IsVisible(int x, int y) => useCartridgeMap
            ? ReadMapBit(mapDataAddress, x, y)
            : system.IsMapTileExplored(areaIndex, x, y);

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
        for (int y = 0; y < 32; y++)
        {
            if (!Enumerable.Range(0, 64).Any(x => IsVisible(x, y)))
                continue;
            top = y;
            break;
        }
        for (int y = 31; y >= 0; y--)
        {
            if (!Enumerable.Range(0, 64).Any(x => IsVisible(x, y)))
                continue;
            bottom = y;
            break;
        }

        ushort minimumX = unchecked((ushort)(left * 8 - (areaIndex == 4 ? 24 : 0)));
        ushort maximumX = unchecked((ushort)(right * 8));
        ushort minimumY = unchecked((ushort)(top * 8));
        ushort maximumY = unchecked((ushort)(bottom * 8));

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
        lastIndicatorSpritemapId = MapIndicatorSpritemapIds[mapIndicatorAnimationFrame];
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
            mapIndicatorAnimationTimer = MapIndicatorFrameDelays[mapIndicatorAnimationFrame];
        }
        mapIndicatorAnimationTimer--;
    }

    private void ResetItemSelectorAnimation()
    {
        itemSelectorAnimationFrame = 0;
        itemSelectorAnimationTimer = bus.ReadByte(0x82c10c);
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

        ushort animationPointer = RomDataReader.ReadWordFixedBank(bus, 0x82c0ec);
        itemSelectorAnimationFrame++;
        byte duration = bus.ReadByte(
            0x820000 | ((animationPointer + itemSelectorAnimationFrame * 3) & 0xffff));
        if (duration == 0xff)
        {
            itemSelectorAnimationFrame = 0;
            duration = bus.ReadByte(0x820000 | animationPointer);
        }
        itemSelectorAnimationTimer = duration;
    }

    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        ushort positionListPointer = RomDataReader.ReadWordFixedBank(
            bus,
            0x82c18e + selectedCategory * 2);
        int positionAddress = 0x820000 | ((positionListPointer + selectedItem * 4) & 0xffff);
        ushort x = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, positionAddress) - 1));
        ushort y = unchecked((ushort)(RomDataReader.ReadWordFixedBank(bus, positionAddress + 2) - 1));

        ushort animationPointer = RomDataReader.ReadWordFixedBank(bus, 0x82c0ec);
        int animationEntry = 0x820000 | ((animationPointer + itemSelectorAnimationFrame * 3) & 0xffff);
        byte spritemapOffset = bus.ReadByte(animationEntry + 2);

        // The third variable pointer used by DrawPauseScreenSpriteAnim is WRAM $0755, the
        // packed equipment selector. The important 65C816 detail is operand width: unlike
        // the timer/frame dereferences above, the source dereference is an eight-bit load.
        // It therefore selects by the low-byte category only; using the whole $0302 Bombs
        // selector walks into the following map-icon data and invents spritemap ID $00CA.
        ushort animationVariantPointer = RomDataReader.ReadWordFixedBank(bus, 0x82c0da);
        if (animationVariantPointer != 0x0755)
        {
            throw new InvalidDataException(
                $"Pause item-selector animation variable is ${animationVariantPointer:X4}, " +
                "expected native WRAM $0755.");
        }
        ushort baseTablePointer = RomDataReader.ReadWordFixedBank(bus, 0x82c1e8);
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
        RomDataReader.ReadWordFixedBank(bus, 0x82c100);

    private void DrawMenuSpritemap(ushort id, ushort x, ushort y, ushort paletteBits)
    {
        ushort pointer = RomDataReader.ReadWordFixedBank(
            bus,
            MenuSpritemapPointerTableAddress + id * 2);
        oam.AddOnScreenSpritemap(bus, 0x820000 | pointer, x, y, paletteBits);
    }

    private bool ReadMapBit(int mapDataAddress, int mapX, int mapY)
    {
        int horizontalPageOffset = (mapX & 0x20) != 0 ? 0x80 : 0;
        int byteColumn = (mapX & 0x1f) >> 3;
        int byteIndex = horizontalPageOffset + mapY * 4 + byteColumn;
        return (bus.ReadByte(mapDataAddress + byteIndex) & (0x80 >> (mapX & 7))) != 0;
    }

    private void UploadEquipmentTilemap() =>
        vram.LoadBytes(Bg1TilemapWord * 2, equipmentTilemap);

    private ushort ReadCategoryMask(EquipmentCategoryDefinition category, int item) =>
        RomDataReader.ReadWordFixedBank(bus, category.BitmaskTableAddress + item * 2);

    private void CopyBank82Words(ushort sourcePointer, Span<byte> destination)
    {
        int source = 0x820000 | sourcePointer;
        for (int index = 0; index < destination.Length; index++)
            destination[index] = bus.ReadByte(0x820000 | ((source + index) & 0xffff));
    }

    private static void RecolorLabel(Span<byte> bytes)
    {
        for (int offset = 0; offset < bytes.Length; offset += 2)
        {
            ushort word = (ushort)(bytes[offset] | (bytes[offset + 1] << 8));
            word = (ushort)((word & ~TilePaletteMask) | DisabledEquipmentPaletteBits);
            bytes[offset] = unchecked((byte)word);
            bytes[offset + 1] = unchecked((byte)(word >> 8));
        }
    }

    private readonly record struct EquipmentCategoryDefinition(
        int OffsetTableAddress,
        int TilemapPointerTableAddress,
        int BitmaskTableAddress,
        int ItemCount,
        int LabelWordCount);
}

internal enum PauseMenuTransition
{
    None,
    MapToEquipmentFadeOut,
    MapToEquipmentFadeIn,
    EquipmentToMapFadeOut,
    EquipmentToMapFadeIn,
}
