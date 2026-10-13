using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;
using SuperMetroid.Core.Spatial;
using System.Buffers.Binary;

namespace SuperMetroid.Core.Frontend;

/// <summary>
/// Installed-asset owner of bank-$82's pause map/equipment menu.
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
    /// <summary>Address space supplying native menu pose and timing data.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Queues menu navigation and animation sounds when a live frontend owns audio.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Advances the map highlight colors once per accepted menu update.</summary>
    internal readonly MapPaletteAnimation paletteAnimation;
    /// <summary>Live inventory owner mutated by equipment and reserve controls.</summary>
    private readonly SamusState samus;
    /// <summary>Provides persistent area map and exploration bits.</summary>
    private readonly Bank80SystemState system;
    /// <summary>Area whose map and pause backdrop are displayed.</summary>
    private readonly AreaId area;
    /// <summary>Room header map origin used to locate Samus on the area map.</summary>
    private readonly Position16<MapTiles> roomMapPosition;
    /// <summary>Visibility rule for the installed map and its scroll bounds.</summary>
    private readonly MapRevealMode mapRevealMode;
    /// <summary>Host-bound map artwork, rebound after debugger state restoration.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Pause-owned VRAM image, including retained gameplay HUD rows.</summary>
    internal readonly SnesVram vram = new();
    /// <summary>Pause-owned CGRAM image with live palette and reserve-arrow edits.</summary>
    internal readonly SnesCgram cgram = new();
    /// <summary>OAM assembled in native map or equipment drawing order.</summary>
    internal readonly OamBuffer oam = new();
    /// <summary>Mutable BG1 equipment page including current inventory labels and tanks.</summary>
    internal readonly byte[] equipmentTilemap;
    /// <summary>Mutable button-label source whose palette bits survive page transitions.</summary>
    private readonly byte[] pauseButtonTilemap;
    /// <summary>Current map/equipment fade or load phase.</summary>
    internal PauseMenuTransition transition;
    /// <summary>Counter inserted between brightness changes during fade-in.</summary>
    private int transitionFadeCounter;
    /// <summary>Current page-transition brightness, from black to full intensity.</summary>
    internal int transitionBrightness = 15;
    /// <summary>Equipment or reserve category selected by the native cursor.</summary>
    internal PauseEquipmentCategory selectedCategory;
    /// <summary>Selected item index within the current equipment category.</summary>
    internal int selectedItem;
    /// <summary>Current BG1 map scroll in map pixels.</summary>
    internal Position16<MapPixels> mapScrollPosition;
    /// <summary>Bounded map-scroll controller initialized from visible area cells.</summary>
    internal PauseMapScroll mapScroll = null!;
    /// <summary>Animated map arrows and their current visibility state.</summary>
    internal FileSelectMapAnimations? mapArrows;
    /// <summary>Whether elevator labels draw before map icons during a fade-out.</summary>
    private bool mapLabelsBeforeIcons;
    /// <summary>Current bank-$82 map-position marker frame.</summary>
    internal int mapIndicatorAnimationFrame;
    /// <summary>Remaining updates in the map-position marker frame.</summary>
    internal int mapIndicatorAnimationTimer;
    /// <summary>Current equipment selector sprite frame.</summary>
    internal int itemSelectorAnimationFrame;
    /// <summary>Remaining updates in the equipment selector frame.</summary>
    internal int itemSelectorAnimationTimer;
    /// <summary>Tracks the retail Boots-to-Plasma label copy overrun across artwork rebinds.</summary>
    private bool plasmaLabelOverrunActive;
    /// <summary>Last selected map marker or equipment selector origin for diagnostics.</summary>
    internal Position16<OamPixels> lastIndicatorOrigin;
    /// <summary>Last native marker or selector sprite identity for diagnostics.</summary>
    internal ushort lastIndicatorSpritemapId;

    /// <summary>Creates the isolated pause PPU image and initial map/equipment state from live gameplay.</summary>
    public PauseMenuState(ISnesAddressSpace bus, PauseMenuEntry entry)
    {
        this.bus = Ensure.NotNull(bus);
        this.mapPresentation = entry.MapPresentation ?? throw new InvalidOperationException(
            "Pause menu requires installed map and equipment presentation assets.");
        this.samus = Ensure.NotNull(entry.Samus);
        this.system = Ensure.NotNull(entry.System);
        this.audio = entry.Audio;
        mapArrows = new FileSelectMapAnimations(bus, entry.MapPresentation.Arrows);
        paletteAnimation = new MapPaletteAnimation(bus);
        paletteAnimation.Bind(entry.MapPresentation.HighlightCycle);
        area = entry.AreaIndex;
        _ = AreaIds.ToIndex(entry.AreaIndex);
        this.roomMapPosition = entry.RoomMapPosition;
        this.mapRevealMode = Enum.IsDefined(entry.MapRevealMode)
            ? entry.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(entry.MapRevealMode));

        // GameState_13 copies exactly these three cartridge ranges. VMADD is a word
        // address, hence the doubled byte destinations below.
        var pauseVram = new PauseVram(vram);
        entry.MapPresentation.Tiles.LoadTo(pauseVram.MapCharacters);
        entry.MapPresentation.PauseTiles.LoadTo(pauseVram.InterfaceCharacters);
        entry.MapPresentation.Sprites.LoadArtworkTo(pauseVram.ObjectCharacters);
        entry.MapPresentation.HudTiles.LoadTo(vram);
        LoadPauseBackdrop();
        if (entry.GameplayVram is not null)
        {
            // SetupPPUForPauseMenu changes BG3SC to $58 but never uploads a replacement
            // tilemap. The four-row gameplay HUD already resident at VRAM word $5800 is
            // intentionally retained. A fresh host-side VRAM object used to discard that
            // state, removing the entire energy/ammo HUD from pause and exposing map BG1
            // pixels in the area that its BG3 plane normally covers.
            new PauseVram(vram).RetainedHudTilemap.Load(new PauseVram(entry.GameplayVram).RetainedHudTilemap);
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
        new PauseVram(vram).Bg3FxRows.Write(clearedFxTilemap);
        new CgramPalettes(cgram).LoadImage(entry.MapPresentation.Palettes.Pause);

        // LoadPauseScreenBaseTilemaps does *not* leave the bottom two button-label rows
        // solely in the $B6:E000 BG2 image. It keeps a mutable $B6:E400 copy at WRAM
        // $3400, recolors MAP/EQUIPMENT/START there, and queues $80 bytes from $3640 to
        // BG2 word $3B20. Omitting this second source is why the pause-screen chrome looked
        // like missing HUD. Keep the complete mutable source so every native word index
        // below remains directly comparable with bank $82.
        pauseButtonTilemap = entry.MapPresentation.PauseBackdrops.CreateButtonTilemap();
        SetPauseButtonLabelMode(0);

        // $B6:E800 is the mutable equipment template normally copied to $7E:3800.
        // Preserve it as a byte array because the cartridge's offset tables contain WRAM
        // byte addresses rather than tilemap word indexes.
        equipmentTilemap = entry.MapPresentation.PauseEquipmentBase.CreateTilemap();
        RebuildEquipmentTilemap();
        LoadPauseMapTilemap();
        SelectFirstCollectedEquipment();
        SetupMapScrolling();
    }

    /// <summary>Zero for the map and one for the equipment page, matching WRAM $0753.</summary>
    public int ScreenMode { get; private set; }

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
        if (ScreenMode == 0 && transition == PauseMenuTransition.None)
            mapArrows!.StepArrows(direction => mapScroll.CanScroll(direction, mapScrollPosition));
        SnesButton delayedPressed = (SnesButton)delayedHeldInput;
        SnesButton newlyPressed = (SnesButton)newlyPressedInput;
        if (transition != PauseMenuTransition.None)
        {
            StepPageTransition();
            return false;
        }

        if (ScreenMode == 0)
        {
            // `$82:9120`: L/R, then Start, then map scrolling. A Start press does not stop
            // the same frame's scroll pulse.
            if ((delayedPressed & SnesButton.R) != 0)
            {
                audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
                SetPauseButtonLabelMode(2);
                transition = PauseMenuTransition.MapToEquipmentFadeOut;
                transitionBrightness = 15;
            }
            bool mapStart = HandleStartButton(delayedPressed);
            // Map arrows read the ordinary held controller word, not the delayed chrome
            // input. An accepted pulse keeps running after release, as on the cartridge.
            if (mapScroll.Step(heldInput ?? delayedHeldInput, ref mapScrollPosition))
                audio?.QueueSound(SoundEffectLibrary1Sounds.MapScroll, maximumQueued: 6);
            return mapStart;
        }

        // `$82:9142`: EquipmentScreenMain, then L/R, then Start, so an A press in the
        // frame that unpauses still toggles the selected item. EquipmentScreenMain consumes
        // the ordinary $8F rising-edge word for D-pad/A; only the shared L/R/Start chrome
        // uses the delayed-held word at $05DF.
        HandleEquipmentInput(newlyPressed, nmiFrameCounter8);
        // EquipmentScreenMain refreshes the reserve amount independently of selected
        // label edits. Keep this per-frame write without rebuilding the whole tilemap,
        // which would erase the cartridge's same-frame VAR overrun.
        WriteReserveSupplyDigits();
        UploadEquipmentTilemap();
        if ((delayedPressed & SnesButton.L) != 0)
        {
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
            SetPauseButtonLabelMode(0);
            transition = PauseMenuTransition.EquipmentToMapFadeOut;
            transitionBrightness = 15;
        }
        return HandleStartButton(delayedPressed);
    }

    /// <summary>
    /// <c>Handle_PauseScreen_StartButton</c> at <c>$82:A5B7</c>, run last on both stable
    /// pages. It reads the delayed-held word: a fresh press is excluded for three frames by
    /// <c>$80:8146</c> before becoming menu input. True requests game state $10.
    /// </summary>
    private bool HandleStartButton(SnesButton delayedPressed)
    {
        if ((delayedPressed & SnesButton.Start) == 0)
            return false;
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        SetPauseButtonLabelMode(1);
        return true;
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
    public void AdvanceAnimations(byte nmiFrameCounter8 = 0, bool fadingOut = false, bool advancePalette = true)
    {
        // $82:9156 and $82:9353 emit destination labels before map icons during fade-out.
        mapLabelsBeforeIcons = fadingOut || transition == PauseMenuTransition.MapToEquipmentFadeOut;
        // Only stable map dispatch calls $82:B934. Fade paths retain counters but hide arrows.
        mapArrows?.StepArrows(_ => false);
        pauseNmiFrameCounter8 = nmiFrameCounter8;
        // $82:90E8 calls the palette handler after stable pause dispatch. The
        // outer pause/unpause fades draw sprites without advancing this owner.
        if (advancePalette && paletteAnimation.Step(cgram))
            audio?.QueueSound(SoundEffectLibrary3Sounds.MapPaletteLoop, maximumQueued: 6);
        if (ScreenMode == 0)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Reusable OBJ color raster, excluded from saved state.</summary>
    [NonSerialized] private Rgba32[]? objScratchPixels;
    /// <summary>Reusable OBJ priority raster, excluded from saved state.</summary>
    [NonSerialized] private byte[]? objScratchPriorities;
    /// <summary>Reusable BG3 priority plane, rebuilt after state restoration.</summary>
    [NonSerialized] private Rgba32[]? bg3ScratchPlane;

    /// <summary>Composites BG and OBJ priorities in Mode-1 order and applies page-transition brightness.</summary>
    public Rgba32[] Render()
    {
        Rgba32[] output = SnesLayerCompositor.CreateBackdrop(cgram, 256 * 224);
        // SetupPpuForPauseMenu writes BGMODE=$09 and TM=$17. The old host compositor drew
        // complete BG2 followed by complete BG1, which ignored tile priority and allowed
        // the low-priority full-area map to paint over BG2's high-priority holder/chrome.
        // Build OAM first and then follow the literal Mode-1-with-BG3-priority ladder.
        PrepareRenderOam();
        objScratchPixels ??= new Rgba32[256 * 224];
        objScratchPriorities ??= new byte[256 * 224];
        SnesObjRenderer.RenderResolved(oam, vram, cgram, PauseMenuLayout.ObjectSelection, new ObjRasterTargets(objScratchPixels, objScratchPriorities));
        var objects = new ResolvedObjFrame(objScratchPixels, objScratchPriorities);

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
    [PpuLayoutBoundary("codec: pause render packet layers")]
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        ushort mapX = ScreenMode == 0 ? mapScrollPosition.X : (ushort)0;
        // BG fetches start on physical scanline one; OBJ coordinates already use
        // output-space Y. Keep the cartridge scroll and sprite origins unchanged.
        ushort mapY = unchecked((ushort)((ScreenMode == 0 ? mapScrollPosition.Y : 0) + SnesPpuLayout.FirstVisibleBackgroundScanline));
        var bg1 = new Bg4BppRenderLayer(PauseMenuLayout.Bg1TilemapWord, 0, new Position16<BgPlanePixels>(mapX, mapY), new Extent16<TilemapCells>(64, 32), false);
        var bg2 = new Bg4BppRenderLayer(PauseMenuLayout.Bg2TilemapWord, 0, new Position16<BgPlanePixels>(0, SnesPpuLayout.FirstVisibleBackgroundScanline), new Extent16<TilemapCells>(32, 32), false);
        var bg3 = new Bg2BppViewportRenderLayer(GameplayVramLayout.HudTilemapWord,
            GameplayVramLayout.HudCharacterWord,
            SnesPpuLayout.FirstVisibleBackgroundScanline, true, false);
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

    /// <summary>Assembles map markers or equipment sprites in their native page-specific OAM order.</summary>
    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        if (ScreenMode == 0)
        {
            var icons = new FileSelectMapIcons(system, area);
            icons.BindLandmarks(mapPresentation?.Landmarks);
            icons.BindSprites(mapPresentation?.Sprites);
            if (mapLabelsBeforeIcons) icons.DrawElevatorLabels(oam, mapScrollPosition);
            mapArrows?.DrawArrows(oam, mapPresentation?.Sprites, PauseMapScrollLayout.ArrowVerticalOffset);
            DrawMapPositionIndicator();
            // Native pause shares the boss lists, defeated overlays and downloaded
            // destination lettering with file select. Preserve their per-phase OAM order.
            icons.DrawBossMarkers(oam, mapScrollPosition);
            if (!mapLabelsBeforeIcons) icons.DrawElevatorLabels(oam, mapScrollPosition);
        }
        else
        {
            DrawEquipmentItemSelector();
            DrawReserveTanks();
        }
        oam.FinalizeFrame();
    }

    /// <summary>Composites one priority half of the scrolling BG1 map or fixed BG2 backdrop.</summary>
    private void CompositePauseBg(
        Span<Rgba32> output,
        ushort tilemapBaseWord,
        int tilemapWidthInTiles,
        bool priority)
    {
        bool isMapLayer = tilemapBaseWord == PauseMenuLayout.Bg1TilemapWord;
        SnesBgTilemapRenderer.Composite4BppViewport(output, vram, cgram, new BgVramAddresses { TilemapBaseWord = tilemapBaseWord, CharacterBaseWord = 0 }, new BgViewport { Scroll = new Position16<BgPlanePixels>(isMapLayer && ScreenMode == 0 ? mapScrollPosition.X : (ushort)0, unchecked((ushort)((isMapLayer && ScreenMode == 0 ? mapScrollPosition.Y : 0) + SnesPpuLayout.FirstVisibleBackgroundScanline))), Size = SnesPpuLayout.ScreenSize, TilemapSize = new Extent16<TilemapCells>(checked((ushort)tilemapWidthInTiles), 32) }, priority: priority);
    }

    /// <summary>Renders one priority half of the retained gameplay HUD BG3 layer.</summary>
    private void CompositePauseBg3(Span<Rgba32> output, bool priority)
    {
        Rgba32[] plane = bg3ScratchPlane ??= new Rgba32[32 * 8 * 32 * 8];
        // The renderer skips tiles of the other priority, so clear the reused plane first.
        plane.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(plane, vram, cgram, new BgVramAddresses { TilemapBaseWord = 0x5800, CharacterBaseWord = 0x4000 }, rowCount: 32, transparentColorZero: true, priority: priority);
        SnesLayerCompositor.Composite(output, plane.AsSpan(
            SnesPpuLayout.FirstVisibleBackgroundScanline * SnesPpuLayout.ScreenWidthPixels,
            output.Length));
    }

    /// <summary>Copies only OBJ pixels matching one native priority rung.</summary>
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

    /// <summary>Advances map/equipment fade, page load, and brightness phases.</summary>
    private void StepPageTransition()
    {
        switch (transition)
        {
            case PauseMenuTransition.MapToEquipmentFadeOut:
            case PauseMenuTransition.EquipmentToMapFadeOut:
                transitionBrightness--;
                if (transitionBrightness > 0)
                    return;

                // $82:9156/$9186 returns at black before the separate load dispatch.
                transition = transition == PauseMenuTransition.MapToEquipmentFadeOut
                    ? PauseMenuTransition.MapToEquipmentLoad : PauseMenuTransition.EquipmentToMapLoad;
                return;

            case PauseMenuTransition.MapToEquipmentLoad:
            case PauseMenuTransition.EquipmentToMapLoad:
                if (transition == PauseMenuTransition.MapToEquipmentLoad)
                {
                    ScreenMode = 1;
                    // $82:AB47 resets the selector on each equipment-page entry.
                    SelectFirstCollectedEquipment();
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
                transitionFadeCounter = PauseFadeTiming.CounterReload;
                return;

            case PauseMenuTransition.MapToEquipmentFadeIn:
            case PauseMenuTransition.EquipmentToMapFadeIn:
                // Load dispatches set delay/counter to one; $80:894D spends a
                // counter-only update between brightness writes.
                if (transitionFadeCounter-- > 0) return;
                transitionFadeCounter = PauseFadeTiming.CounterReload;
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

    /// <summary>Selects reserves first when available, otherwise the first collected inventory item.</summary>
    private void SelectFirstCollectedEquipment()
    {
        // $82:ABAD-$ABB5 selects the tank mode control whenever reserve capacity exists,
        // including empty tanks. The first beam is only the no-reserve fallback.
        if (samus.MaxReserveEnergy != 0)
        {
            selectedCategory = PauseEquipmentCategory.Reserves;
            selectedItem = PauseReserveTransferRomData.ModeItem;
            return;
        }
        foreach (PauseEquipmentCategory categoryIndex in PauseEquipmentCategories.InventoryCategories)
        {
            PauseEquipmentCategoryDefinition category = PauseEquipmentCategories.Get(categoryIndex);
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

        selectedCategory = PauseEquipmentCategory.Reserves;
        selectedItem = 0;
    }

    /// <summary>Reads collected beam or item bits from live Samus inventory.</summary>
    private ushort GetCollectedBits(PauseEquipmentCategory category) =>
        category == PauseEquipmentCategory.Beams ? samus.CollectedBeams : samus.CollectedItems;

    /// <summary>Reads equipped beam or item bits from live Samus inventory.</summary>
    private ushort GetEquippedBits(PauseEquipmentCategory category) =>
        category == PauseEquipmentCategory.Beams ? samus.EquippedBeams : samus.EquippedItems;

    /// <summary>Rebuilds the equipment page from its template and current inventory state.</summary>
    private void RebuildEquipmentTilemap()
    {
        // Restore the literal base before applying inventory-dependent labels. This makes
        // repeated A toggles idempotent and mirrors re-entering LoadEquipmentScreen...
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause equipment requires installed presentation assets."))
            .PauseEquipmentBase.CreateTilemap().CopyTo(equipmentTilemap, 0);
        mapPresentation.PauseEquipmentLabels.ApplyInventory(equipmentTilemap,
            samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems,
            samus.EquippedItems, samus.HyperBeam != 0);

        plasmaLabelOverrunActive = false;

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
            (mapPresentation ?? throw new InvalidOperationException(
                "Reserve digits require installed presentation assets."))
                .PauseReserveUi.ApplyDigit(equipmentTilemap, digitIndex, decimalPlaceValues[digitIndex]);
        }
    }

    /// <summary>Selects and patches the pause wireframe for Samus's equipped suit.</summary>
    private void WriteSamusWireframe()
    {
        int variant = PauseEquipmentRules.WireframeIndex(samus.EquippedItems);
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause wireframe requires installed presentation assets."))
            .PauseWireframes.ApplyTo(equipmentTilemap, (PauseWireframeKind)variant);
    }

    /// <summary>Builds the area map from installed presentation and exploration bits into BG1.</summary>
    private void LoadPauseMapTilemap()
    {
        IAreaMapView map = (mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.")).Get(area);
        byte[] mapTilemap = AreaMapTilemapBuilder.Build(
            map, system, MapTileWords.PauseBlank, mapRevealMode);
        new PauseVram(vram).Bg1Tilemap.Load(mapTilemap);

        // Installed backdrops already contain their authored area lettering.
    }

    /// <summary>Derives native map-scroll bounds and initial offset from visible tiles and Samus's map cell.</summary>
    private void SetupMapScrolling()
    {
        int areaIndex = AreaIds.ToIndex(area);
        // DetermineMapScrollLimits at $82:9EC4 scans either the downloaded cartridge map
        // or the persistent explored plane. Expressing the scan in coordinates is exactly
        // equivalent to its byte/bit loops and makes the two-page 64x32 layout explicit.
        bool hasAreaMap = system.HasAreaMap(areaIndex);
        IAreaMapView map = (mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.")).Get(area);
        bool IsVisible(int x, int y)
        {
            bool explored = system.IsMapTileExplored(areaIndex, new(checked((ushort)x), checked((ushort)y)));
            // Native scroll bounds select one bit plane, unlike tile rendering, which
            // also displays explored secrets. Unioning the planes moves the map's center
            // after a secret is explored even though the downloaded bounds are unchanged.
            if (mapRevealMode == MapRevealMode.None)
                return hasAreaMap ? map.IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y))) : explored;
            return AreaMapVisibility.IsVisible(
                explored,
                hasAreaMap,
                map.IsRevealedByMapStation(new(checked((ushort)x), checked((ushort)y))),
                map.IsDiscoverable(new(checked((ushort)x), checked((ushort)y))),
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
        mapScroll = new PauseMapScroll(new(minimumX, minimumY), new(maximumX, maximumY));

        // SetupMapScrollingForPauseMenu($80) uses 16-bit ADC/SBC throughout. These local
        // helpers retain wrapping before every signed branch so an edge-of-map position
        // behaves like the 65C816 rather than like unbounded host integer arithmetic.
        // The player's map cell: the room's map origin plus Samus's room screen, with the
        // map convention's one-row downward offset.
        Position16<MapPixels> player = SamusMapCell().ToMapPixels();
        ushort horizontal = unchecked((ushort)(
            minimumX + unchecked((ushort)(maximumX - minimumX)) / 2 - 128));
        ushort horizontalScreenPosition = unchecked((ushort)(player.X - horizontal));
        short distanceFromRightClamp = unchecked((short)(224 - horizontalScreenPosition));
        if (distanceFromRightClamp >= 0)
        {
            ushort distanceFromLeftClamp = unchecked((ushort)(32 - horizontalScreenPosition));
            if (unchecked((short)distanceFromLeftClamp) >= 0)
                horizontal = unchecked((ushort)(horizontal - distanceFromLeftClamp));
        }
        else
        {
            horizontal = unchecked((ushort)(horizontal - distanceFromRightClamp));
        }

        ushort verticalMiddle = unchecked((ushort)(
            minimumY + unchecked((ushort)(maximumY - minimumY)) / 2 + 16));
        ushort verticalCenterOffset = unchecked((ushort)((0x80 - verticalMiddle) & 0xfff8));
        ushort vertical = unchecked((ushort)-verticalCenterOffset);
        ushort playerMapY = unchecked((ushort)(player.Y + verticalCenterOffset));
        short distanceFromTopClamp = unchecked((short)(64 - playerMapY));
        if (distanceFromTopClamp >= 0)
        {
            vertical = unchecked((ushort)(vertical - distanceFromTopClamp));
            if (unchecked((short)(vertical + 40)) < 0)
                vertical = unchecked((ushort)-40);
        }
        mapScrollPosition = new(horizontal, vertical);
    }

    /// <summary>Maps Samus's current room screen to the area map with its one-row Y convention.</summary>
    private Position16<MapTiles> SamusMapCell() =>
        roomMapPosition + new Offset16<MapTiles>(checked((short)(SnesEightEight.WholeOf(samus.Position.X.Whole))), checked((short)((SnesEightEight.WholeOf(samus.Position.Y.Whole)) + 1)));

    /// <summary>Draws the animated Samus map marker at the scrolled screen position.</summary>
    private void DrawMapPositionIndicator()
    {
        Position16<OamPixels> origin = SamusMapCell().ToMapPixels().ToScreen(mapScrollPosition).ToOam();
        lastIndicatorOrigin = origin;
        MapSpriteId indicator = PauseMapIndicatorAnimation.SpritemapId(mapIndicatorAnimationFrame);
        lastIndicatorSpritemapId = (ushort)indicator;
        DrawMenuSpritemap(
            indicator,
            origin,
            ReadPauseSpritePaletteBits());
    }

    /// <summary>Advances the map marker's native timer-before-draw frame cadence.</summary>
    private void StepMapIndicatorAnimation()
    {
        // The native timer starts at zero, advances to frame one on the first draw, then
        // decrements after reloading. This order is intentionally not a conventional
        // "draw frame zero for N ticks" animation helper.
        if (mapIndicatorAnimationTimer == 0)
        {
            mapIndicatorAnimationFrame = (mapIndicatorAnimationFrame + 1) & (PauseMapIndicatorAnimation.FrameCount - 1);
            mapIndicatorAnimationTimer = PauseMapIndicatorAnimation.FrameDelay(mapIndicatorAnimationFrame);
        }
        mapIndicatorAnimationTimer--;
    }

    /// <summary>Restarts the equipment selector at its first authored frame.</summary>
    private void ResetItemSelectorAnimation()
    {
        itemSelectorAnimationFrame = 0;
        itemSelectorAnimationTimer = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets."))
            .PauseSelectors.InitialDurationTicks;
    }

    /// <summary>Advances the equipment selector only while inventory offers a selectable item.</summary>
    private void StepItemSelectorAnimation()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        // DrawPauseScreenSpriteAnim(3) selects the third timer/frame pair. Its animation
        // list is a cartridge pointer, with three-byte entries (delay, unused, ID offset).
        itemSelectorAnimationTimer--;
        if (itemSelectorAnimationTimer > 0)
            return;

        var selectors = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        itemSelectorAnimationFrame = selectors.NormalizePhase(itemSelectorAnimationFrame + 1);
        itemSelectorAnimationTimer = selectors.Duration(itemSelectorAnimationFrame);
    }

    /// <summary>Draws the selected equipment item's authored sprite and records its origin.</summary>
    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        var selector = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        var point = selector.Anchor(selectedCategory, selectedItem);
        lastIndicatorOrigin = point;
        // Diagnostic identity denotes the native category binding, not an
        // author-supplied ROM index. Custom phases may use different artwork.
        lastIndicatorSpritemapId = PauseSelectorDefinitions.NativeSpriteId(selectedCategory);
        selector.Draw(oam, selectedCategory, selectedItem, itemSelectorAnimationFrame);
    }

    /// <summary>Gets the native OBJ palette bits shared by pause map markers.</summary>
    private static ushort ReadPauseSpritePaletteBits() => PauseMenuLayout.MapMarkerPaletteBits;

    /// <summary>
    /// Ports the three SetPauseScreenButtonLabelPalettes variants at $82:A628-$A84C and
    /// immediately performs UpdatePauseMenuLRStartVramTilemap's $80-byte upload.
    /// </summary>
    private void SetPauseButtonLabelMode(int mode)
    {
        // These indexes are words relative to native WRAM $3000. The mutable cartridge
        // template begins at $3400, or word index $200; subtract that origin before
        // indexing the local byte array.
        void SetPalette(int nativeWordIndex, int wordCount, ushort paletteBits)
        {
            int localWordIndex = nativeWordIndex - PauseMenuLayout.ButtonSourceWordOrigin;
            for (int word = 0; word < wordCount; word++)
            {
                int byteOffset = (localWordIndex + word) * 2;
                ushort tile = unchecked((ushort)(
                    SnesLittleEndian.Word(pauseButtonTilemap[byteOffset], pauseButtonTilemap[byteOffset + 1])));
                tile = unchecked((ushort)((tile & 0xe3ff) | paletteBits));
                pauseButtonTilemap[byteOffset] = unchecked((byte)tile);
                pauseButtonTilemap[byteOffset + 1] = SnesLittleEndian.HighByte(tile);
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

        new PauseVram(vram).ButtonRows.Load(
            pauseButtonTilemap.AsSpan(
                PauseMenuLayout.ButtonRowsSourceOffset,
                PauseMenuLayout.ButtonRowsByteCount));
    }

    /// <summary>Draws an installed map sprite composition with its selected palette bits.</summary>
    private void DrawMenuSpritemap(MapSpriteId id, Position16<OamPixels> origin, ushort paletteBits)
    {
        // Map compositions use this shared entry. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        (mapPresentation ?? throw new InvalidOperationException(
            "Menu spritemap requires installed presentation assets."))
            .Sprites.Draw(id, oam, origin, paletteBits);
    }

    /// <summary>Uploads the current mutable equipment page to BG1 VRAM.</summary>
    private void UploadEquipmentTilemap() =>
        new PauseVram(vram).Bg1Tilemap.Load(equipmentTilemap);

    /// <summary>Gets the inventory bit represented by an item in an equipment category.</summary>
    private static ushort ReadCategoryMask(PauseEquipmentCategoryDefinition category, int item) =>
        PauseEquipmentRules.Mask(category.Category, item);

    /// <summary>Dispatches reserve input or moves and toggles equipment in native update order.</summary>
    private void HandleEquipmentInput(SnesButton pressed, byte nmiFrameCounter8)
    {
        PauseEquipmentCategory dispatchedCategory = selectedCategory;
        if (dispatchedCategory == PauseEquipmentCategory.Reserves)
        {
            HandleReserveInput(pressed);
            MoveEquipmentSelector(pressed);
            UpdateReserveArrow(nmiFrameCounter8);
            return;
        }
        MoveEquipmentSelector(pressed);
        if ((pressed & SnesButton.A) == 0)
            return;
        if (dispatchedCategory == PauseEquipmentCategory.Beams && samus.CollectedBeams == 0)
            return;
        if (selectedCategory == PauseEquipmentCategory.Reserves)
            throw new NotSupportedException("Same-frame equipment toggle into reserve tables requires native out-of-table WRAM behavior.");

        // Dispatch precedes movement. Boots->Plasma therefore retains the Boots copy
        // length and bypasses the Weapons handler's Spazer/Plasma exclusion. Do not use
        // upstream C's 'Fixed var bug' category guard: it is absent from retail ROM.
        ushort previousBeams = samus.EquippedBeams;
        var target = PauseEquipmentCategories.Get(selectedCategory);
        ushort mask = ReadCategoryMask(target, selectedItem);
        bool wasEquipped = (GetEquippedBits(selectedCategory) & mask) != 0;
        if (selectedCategory == PauseEquipmentCategory.Beams)
            samus.EquippedBeams ^= mask;
        else
            samus.EquippedItems ^= mask;

        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuConfirm, maximumQueued: 6);
        UpdateEquipmentLabel(selectedCategory, selectedItem,
            PauseEquipmentCategories.Get(dispatchedCategory).LabelWordCount, wasEquipped);
        if (dispatchedCategory == PauseEquipmentCategory.Beams)
        {
            SamusBeamFlags added = (SamusBeamFlags)(samus.EquippedBeams & ~previousBeams);
            if ((added & SamusBeamFlags.Spazer) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Plasma) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Plasma);
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
            else if ((added & SamusBeamFlags.Plasma) != 0 &&
                (samus.EquippedBeams & (ushort)SamusBeamFlags.Spazer) != 0)
            {
                samus.EquippedBeams &= unchecked((ushort)~(ushort)SamusBeamFlags.Spazer);
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.SpazerItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
        }
        // Native patches only the chosen label, then refreshes the wireframe. Rebuilding
        // all labels would erase the intentional overlong copy that displays VAR.
        WriteSamusWireframe();
        UploadEquipmentTilemap();
    }

    /// <summary>Patches one equipment label, retaining the retail overlong Plasma copy when selected.</summary>
    private void UpdateEquipmentLabel(PauseEquipmentCategory categoryIndex, int item, int wordCount, bool disabled)
    {
        var category = PauseEquipmentCategories.Get(categoryIndex);
        if (categoryIndex == PauseEquipmentCategory.Beams &&
            item == PauseEquipmentCategories.PlasmaItem && wordCount > category.LabelWordCount)
            plasmaLabelOverrunActive = true;
        (mapPresentation ?? throw new InvalidOperationException(
            "Equipment labels require installed presentation assets."))
            .PauseEquipmentLabels.ApplyLabel(
                equipmentTilemap, categoryIndex, item, wordCount, disabled);
    }

    /// <summary>Scans collected items in one category and moves the selector to the first eligible item.</summary>
    private bool TrySelectEquipment(PauseEquipmentCategory categoryIndex, int start, int step)
    {
        if (categoryIndex == PauseEquipmentCategory.Beams && samus.HyperBeam != 0)
            return false;
        var category = PauseEquipmentCategories.Get(categoryIndex);
        for (int item = start; item >= 0 && item < category.ItemCount; item += step)
        {
            if ((GetCollectedBits(categoryIndex) & ReadCategoryMask(category, item)) == 0)
            {
                // $82:B4B7 checks byte offset ten only AFTER an unsuccessful probe.
                // Thus directly entering the final suit item can succeed, but scanning
                // forward from an earlier missing item must not discover it.
                if (categoryIndex == PauseEquipmentCategory.Suits && step > 0 &&
                    item + step >= category.ItemCount - 1)
                    return false;
                continue;
            }
            selectedCategory = categoryIndex;
            selectedItem = item;
            audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
            return true;
        }
        return false;
    }

    /// <summary>Moves the selector to reserve mode when Samus has reserve capacity.</summary>
    private bool TrySelectReserves()
    {
        if (samus.MaxReserveEnergy == 0) return false;
        selectedCategory = PauseEquipmentCategory.Reserves;
        selectedItem = 0;
        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
        return true;
    }

    /// <summary>Applies native directional links among beam, suit, boot, and reserve rows.</summary>
    private void MoveEquipmentSelector(SnesButton pressed)
    {
        bool left = (pressed & SnesButton.Left) != 0, right = (pressed & SnesButton.Right) != 0;
        bool up = (pressed & SnesButton.Up) != 0, down = (pressed & SnesButton.Down) != 0;
        PauseEquipmentCategory beams = PauseEquipmentCategory.Beams, suits = PauseEquipmentCategory.Suits, boots = PauseEquipmentCategory.Boots;
        switch (selectedCategory)
        {
            case PauseEquipmentCategory.Beams:
                if (right)
                {
                    if (!TrySelectEquipment(suits, up ? 0 : PauseEquipmentCategories.BeamRightSuitItem, 1) && !up)
                        TrySelectEquipment(boots, 0, 1);
                }
                else if (down) TrySelectEquipment(beams, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(beams, selectedItem - 1, -1)) TrySelectReserves();
                break;
            case PauseEquipmentCategory.Suits:
                if (left)
                {
                    if (down || !TrySelectReserves()) TrySelectEquipment(beams, 0, 1);
                }
                else if (up) TrySelectEquipment(suits, selectedItem - 1, -1);
                else if (down && !TrySelectEquipment(suits, selectedItem + 1, 1)) TrySelectEquipment(boots, 0, 1);
                break;
            case PauseEquipmentCategory.Boots:
                if (left)
                {
                    if (up || !TrySelectEquipment(beams, PauseEquipmentCategories.PlasmaItem, -1)) TrySelectReserves();
                }
                else if (down) TrySelectEquipment(boots, selectedItem + 1, 1);
                else if (up && !TrySelectEquipment(boots, selectedItem - 1, -1))
                    TrySelectEquipment(suits, PauseEquipmentCategories.Get(suits).ItemCount - 1, -1);
                break;
            case PauseEquipmentCategory.Reserves:
                if (right)
                {
                    if (down || !TrySelectEquipment(suits, 0, 1)) TrySelectEquipment(boots, 0, 1);
                }
                else if (up)
                {
                    if (SelectedReserveItem != PauseReserveItem.Mode)
                    {
                        selectedItem--;
                        audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                    }
                }
                else if (down)
                {
                    if (SelectedReserveItem == PauseReserveItem.Transfer ||
                        samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode)
                        TrySelectEquipment(beams, 0, 1);
                    else
                    {
                        selectedItem++;
                        if (samus.ReserveEnergy == 0) TrySelectEquipment(beams, 0, 1);
                        else audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                    }
                }
                break;
        }
    }

    /// <summary>Retains only HUD DMA accepted by NMI, not the pending WRAM tilemap.</summary>
    public void SynchronizeAcceptedHud(SnesVram gameplayVram)
    {
        // Pause owns an isolated PPU image. Mirror the native mutable HUD range,
        // leaving its static first row and deliberately cleared FX rows untouched.
        new PauseVram(vram).AcceptedHudRows.Load(new PauseVram(gameplayVram).AcceptedHudRows);
    }

    /// <summary>Rebinds edited map and pause artwork while retaining live scroll, palette, and label state.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        AreaMapPresentationCatalog? previousCatalog = mapPresentation;
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Pause menu requires installed map presentation assets.");
        mapArrows ??= new FileSelectMapAnimations(bus, catalog.Arrows);
        mapArrows.BindPresentation(catalog.Arrows);
        paletteAnimation.Bind(catalog.HighlightCycle);
        catalog.PauseTiles.LoadTo(new PauseVram(vram).InterfaceCharacters);
        catalog.Sprites.LoadArtworkTo(new PauseVram(vram).ObjectCharacters);
        // Live highlights and reserve-arrow colors belong to their animation
        // owners. Replacing the static base must not reset their phase.
        new CgramPalettes(cgram).LoadImageExcept(catalog.Palettes.Pause, FrontendPaletteLayout.MapHighlightCycle,
            FrontendPaletteLayout.PauseReserveArrowColor6, FrontendPaletteLayout.PauseReserveArrowColor11);
        catalog.Tiles.LoadTo(new PauseVram(vram).MapCharacters);
        catalog.HudTiles.LoadTo(vram);
        LoadPauseBackdrop();
        RefreshPauseButtonArtwork();
        // Refresh the authored base and rebuild the now-semantic inventory layer. The
        // explicit overrun bit retains the retail Boots-to-Plasma VAR artifact without
        // preserving stale pixels at a label's old editable destination.
        bool labelsChanged = previousCatalog is null ||
            previousCatalog.PauseEquipmentLabels.ContentIdentity != catalog.PauseEquipmentLabels.ContentIdentity;
        if (labelsChanged)
        {
            catalog.PauseEquipmentBase.RebindBeforeInventoryRefreshInto(equipmentTilemap);
            catalog.PauseEquipmentLabels.ApplyInventory(equipmentTilemap,
                samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems,
                samus.EquippedItems, samus.HyperBeam != 0);
            if (plasmaLabelOverrunActive)
                catalog.PauseEquipmentLabels.ApplyLabel(equipmentTilemap,
                    PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Boots).LabelWordCount,
                    disabled: false);
        }
        else catalog.PauseEquipmentBase.RebindBaseInto(
            equipmentTilemap, previousCatalog!.PauseEquipmentLabels);
        // Reapply only the wireframe patch, in its native footprint. The surrounding
        // mutable labels include intentional cartridge overruns and must not be rebuilt.
        WriteSamusWireframe();
        // Reserve presentation has its own bounded footprints and can safely adopt the
        // rebound content without reconstructing unrelated inventory labels.
        if (samus.MaxReserveEnergy != 0)
        {
            WriteReserveLabels();
            WriteReserveSupplyDigits();
            // A rebind is not a tank dispatch: re-apply the owner's latched arrow state with the
            // new palettes. Only a bind before anything has latched asks the owner.
            if (reserveArrowLatched)
                ReapplyLatchedReserveArrow();
            else if (selectedCategory == PauseEquipmentCategory.Reserves)
                UpdateReserveArrow(pauseNmiFrameCounter8);
        }
        if (ScreenMode != 0) UploadEquipmentTilemap();
        // The serialized live button palette rows still own their overlay on
        // the refreshed backdrop, including a Start/fade highlight.
        // Do not rerun controls or rebuild equipment: that would erase native
        // same-frame label overruns and change the state being restored.
        new PauseVram(vram).ButtonRows.Load(
            pauseButtonTilemap.AsSpan(PauseMenuLayout.ButtonRowsSourceOffset, PauseMenuLayout.ButtonRowsByteCount));
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == 0) LoadPauseMapTilemap();
    }

    /// <summary>Loads the selected area's installed pause backdrop into BG2.</summary>
    private void LoadPauseBackdrop()
    {
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause backdrop requires installed presentation assets."))
            .PauseBackdrops.LoadTo(new PauseVram(vram).Bg2Tilemap, area);
    }

    /// <summary>Adopts replacement button art while preserving palette bits from the live menu.</summary>
    private void RefreshPauseButtonArtwork()
    {
        byte[] replacement = (mapPresentation ?? throw new InvalidOperationException(
            "Pause buttons require installed presentation assets."))
            .PauseBackdrops.CreateButtonTilemap();
        // Only these palette bits are owned by the live menu. Carry them forward
        // word-by-word (including restored historical states) rather than inferring
        // a mode from ScreenMode, which lags the button highlight during fades.
        foreach (var span in PauseMenuLayout.ButtonLabelSpans)
        for (int index = 0; index < span.Count; index++)
        {
            int offset = (span.Word - PauseMenuLayout.ButtonSourceWordOrigin + index) * 2;
            var previous = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(pauseButtonTilemap.AsSpan(offset)));
            var current = new SnesBgTilemapWord(BinaryPrimitives.ReadUInt16LittleEndian(replacement.AsSpan(offset)));
            BinaryPrimitives.WriteUInt16LittleEndian(replacement.AsSpan(offset), current.WithPaletteIndex(previous.PaletteIndex).Raw);
        }
        replacement.CopyTo(pauseButtonTilemap, 0);
    }

    /// <summary>Whether reserve-arrow ownership has published a state to retain across artwork rebinds.</summary>
    private bool reserveArrowLatched;
    /// <summary>Last requested visibility of the reserve arrow.</summary>
    private bool reserveArrowLatchedEnabled;
    /// <summary>Whether the last arrow color request used its animated palette.</summary>
    private bool reserveArrowLatchedAnimated;
    /// <summary>NMI frame retained for reproducing the arrow's current color on rebind.</summary>
    private byte reserveArrowLatchedFrame;

    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(byte nmiFrameCounter8)
    {
        bool inTanks = selectedCategory == PauseEquipmentCategory.Reserves;
        bool animated = inTanks && SelectedReserveItem == PauseReserveItem.Mode &&
            samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode;
        bool enabled = animated || inTanks && SelectedReserveItem == PauseReserveItem.Transfer;
        SetReserveArrow(enabled, animated, nmiFrameCounter8);
    }

    /// <summary>Applies reserve-arrow tile and palette state, then latches it for content rebinds.</summary>
    private void SetReserveArrow(bool enabled, bool animated = false, byte nmiFrameCounter8 = 0)
    {
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve arrow requires installed presentation assets.")).PauseReserveUi;
        presentation.ApplyArrowColors(cgram, animated, nmiFrameCounter8,
            PauseReserveArrowRomData.Color6Index, PauseReserveArrowRomData.Color11Index);
        presentation.ApplyArrowTilePalettes(equipmentTilemap, enabled);
        reserveArrowLatched = true;
        reserveArrowLatchedEnabled = enabled;
        reserveArrowLatchedAnimated = animated;
        reserveArrowLatchedFrame = nmiFrameCounter8;
    }

    /// <summary>Re-applies the latched arrow tiles and colors with the currently bound presentation.</summary>
    private void ReapplyLatchedReserveArrow() =>
        SetReserveArrow(reserveArrowLatchedEnabled, reserveArrowLatchedAnimated, reserveArrowLatchedFrame);

    /// <summary>Writes reserve mode and tank labels only when reserve capacity exists.</summary>
    private void WriteReserveLabels()
    {
        // Absence of capacity leaves the original blank template intact. Restoring
        // the template before rebuilding also removes labels if inventory is changed.
        if (samus.MaxReserveEnergy == 0) return;
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve labels require installed presentation assets.")).PauseReserveUi;
        presentation.ApplyLabel(equipmentTilemap, "Mode");
        presentation.ApplyLabel(equipmentTilemap, "ReserveTank");
        // Native setup retains the initial MANUAL label for the zero/uninitialized
        // mode and patches its first four characters only after a nonzero mode exists.
        if (samus.ReserveTankMode != 0)
            presentation.ApplyLabel(equipmentTilemap,
                samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode ? "Auto" : "Manual",
                preserveAttributes: true);
    }

    /// <summary>Accepted NMI phase retained so repeated renders cannot advance tank fill flicker.</summary>
    private byte pauseNmiFrameCounter8;

    /// <summary>Ports $82:B2AA's full/partial/empty strip and final cap.</summary>
    private void DrawReserveTanks()
    {
        if (samus.MaxReserveEnergy == 0) return;
        int tank = 0;
        int full = samus.ReserveEnergy / PauseReserveTankRomData.EnergyPerTank;
        int remainder = samus.ReserveEnergy % PauseReserveTankRomData.EnergyPerTank;
        for (; tank < full; tank++) Draw(PauseReserveTankVisual.Full, tank);
        if (remainder != 0)
        {
            int quotient = remainder / PauseReserveTankRomData.EnergyPerFillStep;
            int tableOffset = quotient * sizeof(ushort);
            if (tableOffset < PauseReserveTankRomData.FlickerDoubledQuotientLimit &&
                remainder % PauseReserveTankRomData.EnergyPerFillStep != 0 &&
                (pauseNmiFrameCounter8 & PauseReserveTankRomData.FlickerFrameMask) == 0)
                tableOffset += sizeof(ushort);
            if (samus.ReserveEnergy >= PauseReserveTankRomData.EnergyPerTank)
                tableOffset += PauseReserveTankRomData.SecondTableOffset;
            Draw(PauseReserveTankDefinitions.PartialMap(tableOffset / sizeof(ushort)), tank++);
        }
        for (; tank < samus.MaxReserveEnergy / PauseReserveTankRomData.EnergyPerTank; tank++)
            Draw(PauseReserveTankVisual.Empty, tank);
        Draw(PauseReserveTankVisual.EndCap, tank);

        void Draw(PauseReserveTankVisual map, int index)
        {
            (mapPresentation ?? throw new InvalidOperationException(
                "Reserve tanks require installed presentation assets."))
                .PauseReserveTanks.Draw(oam, map, index);
        }
    }

    /// <summary>Native $0757 sound cadence, advanced only by the selected reserve-transfer handler.</summary>
    internal NativeCountdown reserveTransferSoundDelay;

    /// <summary>The selected item while the reserve-tank category is selected.</summary>
    private PauseReserveItem SelectedReserveItem => Enum.IsDefined((PauseReserveItem)selectedItem)
        ? (PauseReserveItem)selectedItem
        : throw new InvalidDataException($"Unsupported native reserve subdispatcher {selectedItem}.");

    /// <summary>Runs the tank subdispatcher before its D-pad response, as at $82:AC70.</summary>
    private void HandleReserveInput(SnesButton pressed)
    {
        switch (SelectedReserveItem)
        {
            case PauseReserveItem.Mode:
                if ((pressed & SnesButton.A) == 0 || samus.MaxReserveEnergy == 0) return;
                audio?.QueueSound(SoundEffectLibrary1Sounds.MenuCursor, maximumQueued: 6);
                samus.ReserveTankMode = samus.ReserveTankMode == PauseReserveLabelRomData.AutoMode
                    ? PauseReserveTransferRomData.ManualMode : PauseReserveLabelRomData.AutoMode;
                WriteReserveLabels();
                return;
            case PauseReserveItem.Transfer:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseReserveItem)} {selectedItem}.");
        }

        if (reserveTransferSoundDelay.IsZero)
        {
            if ((pressed & SnesButton.A) == 0) return;
            reserveTransferSoundDelay = NativeCountdown.FromRaw(unchecked((ushort)((samus.ReserveEnergy +
                PauseReserveTransferRomData.SoundCadenceMask) & ~PauseReserveTransferRomData.SoundCadenceMask)));
        }
        reserveTransferSoundDelay = reserveTransferSoundDelay.Decrement().Remaining;
        if ((reserveTransferSoundDelay.RawValue & PauseReserveTransferRomData.SoundCadenceMask) ==
            PauseReserveTransferRomData.SoundCadenceMask)
            audio?.QueueSound(PauseReserveTransferRomData.RefillSound, PauseReserveTransferRomData.MaximumQueuedSounds);

        ushort amount = PauseEquipmentRules.ReserveEnergyPerFrame;
        samus.Health = unchecked((ushort)(samus.Health + amount));
        if (unchecked((short)(samus.Health - samus.MaxHealth)) >= 0)
            samus.Health = samus.MaxHealth;
        else
        {
            samus.ReserveEnergy = unchecked((ushort)(samus.ReserveEnergy - amount));
            if (unchecked((short)samus.ReserveEnergy) > 0) return;
            if (samus.ReserveEnergy != 0)
                samus.Health = unchecked((ushort)(samus.Health + samus.ReserveEnergy));
        }

        // Retail exhausts ALL reserves even when only a small top-up was needed.
        // Preserve that behavior and clear both bytes of the packed selector.
        samus.ReserveEnergy = 0;
        reserveTransferSoundDelay = NativeCountdown.Start(0);
        selectedCategory = PauseEquipmentCategory.Reserves;
        selectedItem = PauseReserveTransferRomData.ModeItem;
    }
}

/// <summary>Map/equipment page fade and load phases owned by the pause menu.</summary>
internal enum PauseMenuTransition
{
    /// <summary>No page change is in progress; normal input and rendering run.</summary>
    None,
    /// <summary>Dim the map before loading equipment graphics.</summary>
    MapToEquipmentFadeOut,
    /// <summary>Brighten the newly loaded equipment page.</summary>
    MapToEquipmentFadeIn,
    /// <summary>Dim equipment before restoring the map.</summary>
    EquipmentToMapFadeOut,
    /// <summary>Brighten the restored map page.</summary>
    EquipmentToMapFadeIn,
    /// <summary>Swap map BG1 for equipment BG1 while the screen is black.</summary>
    MapToEquipmentLoad,
    /// <summary>Restore map BG1 after the equipment page has faded out.</summary>
    EquipmentToMapLoad,
}
