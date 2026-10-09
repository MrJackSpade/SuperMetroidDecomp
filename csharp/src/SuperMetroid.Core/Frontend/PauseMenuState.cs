using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Runtime;

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
internal sealed partial class PauseMenuState
{
    /// <summary>Address space used by pause-owned PPU operations and animation data reads.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Optional sound queue for menu actions and map animation events.</summary>
    private readonly CartridgeAudioState? audio;
    /// <summary>Cartridge-timed map palette cycle applied to the pause CGRAM image.</summary>
    private readonly MapPaletteAnimation paletteAnimation;
    /// <summary>Live gameplay inventory and position state shared with the pause page.</summary>
    private readonly SamusState samus;
    /// <summary>Area map reveal and exploration state used to build and scroll the map.</summary>
    private readonly Bank80SystemState system;
    /// <summary>Area whose map and pause presentation are currently displayed.</summary>
    private readonly AreaId area;
    /// <summary>Room's horizontal origin in the 64 by 32 area map.</summary>
    private readonly byte roomMapX;
    /// <summary>Room's vertical origin in the 64 by 32 area map.</summary>
    private readonly byte roomMapY;
    /// <summary>Rule controlling which unexplored or secret map tiles are revealed.</summary>
    private readonly MapRevealMode mapRevealMode;
    /// <summary>Installed map, sprite, and equipment presentation assets for this pause session.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;
    /// <summary>Pause-local VRAM image, including the retained gameplay HUD tilemap.</summary>
    private readonly SnesVram vram = new();
    /// <summary>Pause-local palette image used by the software PPU compositor.</summary>
    private readonly SnesCgram cgram = new();
    /// <summary>OAM entries assembled for pause map markers and equipment selectors.</summary>
    private readonly OamBuffer oam = new();
    /// <summary>Mutable equipment-page tilemap rebuilt from its installed base and inventory.</summary>
    private readonly byte[] equipmentTilemap;
    /// <summary>Mutable MAP/EQUIPMENT/START label tilemap uploaded as the page changes.</summary>
    private readonly byte[] pauseButtonTilemap;
    /// <summary>Current fade or page-load phase between the map and equipment screens.</summary>
    private PauseMenuTransition transition;
    /// <summary>Counter spacing successive brightness updates during a page fade-in.</summary>
    private int transitionFadeCounter;
    /// <summary>Current pause brightness value, clamped to the native range 0 through 15.</summary>
    private int transitionBrightness = 15;
    /// <summary>Selected equipment category, using the cartridge category index.</summary>
    private int selectedCategory;
    /// <summary>Selected item index within <see cref="selectedCategory"/>.</summary>
    private int selectedItem;
    /// <summary>Current horizontal BG scroll in the cartridge's wrapped screen coordinate.</summary>
    private ushort mapHorizontalScroll;
    /// <summary>Current vertical BG scroll in the cartridge's wrapped screen coordinate.</summary>
    private ushort mapVerticalScroll;
    /// <summary>Scroll limits and direction checks calculated from visible area-map tiles.</summary>
    private PauseMapScroll mapScroll = null!;
    /// <summary>Cartridge-timed arrow animation owner used while the map page is stable.</summary>
    private FileSelectMapAnimations? mapArrows;
    /// <summary>Whether destination labels precede map icons during the outgoing page fade.</summary>
    private bool mapLabelsBeforeIcons;
    /// <summary>Current pause-map Samus marker animation phase.</summary>
    private int mapIndicatorAnimationFrame;
    /// <summary>Remaining updates before the Samus marker advances to its next phase.</summary>
    private int mapIndicatorAnimationTimer;
    /// <summary>Current equipment selector sprite animation phase.</summary>
    private int itemSelectorAnimationFrame;
    /// <summary>Remaining updates before the equipment selector advances phase.</summary>
    private int itemSelectorAnimationTimer;
    /// <summary>Tracks the same-frame Plasma label spill behavior for diagnostics.</summary>
    private bool plasmaLabelOverrunActive;
    /// <summary>Last screen-space X origin used for the pause marker or selector.</summary>
    private ushort lastIndicatorOriginX;
    /// <summary>Last screen-space Y origin used for the pause marker or selector.</summary>
    private ushort lastIndicatorOriginY;
    /// <summary>Native visual identity last selected for the map marker or equipment selector.</summary>
    private ushort lastIndicatorSpritemapId;

    /// <summary>Creates the pause-local PPU image from installed assets and current gameplay state.</summary>
    /// <param name="bus">Address space used to read cartridge animation and palette data.</param>
    /// <param name="samus">Live Samus state whose inventory and position drive the menu.</param>
    /// <param name="system">Area exploration and map-station state used for map visibility.</param>
    /// <param name="areaIndex">Area displayed by the pause map.</param>
    /// <param name="roomMapX">Room origin column in the area map.</param>
    /// <param name="roomMapY">Room origin row in the area map.</param>
    /// <param name="audio">Optional sound queue for pause menu feedback.</param>
    /// <param name="gameplayVram">Optional gameplay VRAM whose existing HUD tilemap is retained.</param>
    /// <param name="mapRevealMode">Visibility policy applied to explored and discoverable tiles.</param>
    /// <param name="mapPresentation">Installed visual assets required to construct the pause display.</param>
    /// <exception cref="ArgumentNullException">A required state or address-space dependency is null.</exception>
    /// <exception cref="ArgumentOutOfRangeException">The reveal mode is not defined.</exception>
    /// <exception cref="InvalidOperationException">Pause presentation assets are unavailable.</exception>
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
        this.mapPresentation = mapPresentation ?? throw new InvalidOperationException(
            "Pause menu requires installed map and equipment presentation assets.");
        this.samus = samus ?? throw new ArgumentNullException(nameof(samus));
        this.system = system ?? throw new ArgumentNullException(nameof(system));
        this.audio = audio;
        mapArrows = new FileSelectMapAnimations(bus, mapPresentation.Arrows);
        paletteAnimation = new MapPaletteAnimation(bus);
        paletteAnimation.Bind(mapPresentation.HighlightCycle);
        area = areaIndex;
        _ = AreaIds.ToIndex(areaIndex);
        this.roomMapX = roomMapX;
        this.roomMapY = roomMapY;
        this.mapRevealMode = Enum.IsDefined(mapRevealMode)
            ? mapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(mapRevealMode));

        // GameState_13 copies exactly these three cartridge ranges. VMADD is a word
        // address, hence the doubled byte destinations below.
        mapPresentation.Tiles.LoadTo(vram, 0);
        mapPresentation.PauseTiles.LoadTo(vram, PauseTileAtlasFormat.DestinationByte);
        mapPresentation.Sprites.LoadArtworkTo(vram, MapSpriteFormat.PauseDestination);
        mapPresentation.HudTiles.LoadTo(vram, HudTileAtlasFormat.DestinationWord * 2);
        LoadPauseBackdrop();
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
        for (int color = 0; color < SnesCgram.ColorCount; color++)
            cgram.SetColor(color, mapPresentation.Palettes.Pause[color]);

        // LoadPauseScreenBaseTilemaps does *not* leave the bottom two button-label rows
        // solely in the $B6:E000 BG2 image. It keeps a mutable $B6:E400 copy at WRAM
        // $3400, recolors MAP/EQUIPMENT/START there, and queues $80 bytes from $3640 to
        // BG2 word $3B20. Omitting this second source is why the pause-screen chrome looked
        // like missing HUD. Keep the complete mutable source so every native word index
        // below remains directly comparable with bank $82.
        pauseButtonTilemap = mapPresentation.PauseBackdrops.CreateButtonTilemap();
        SetPauseButtonLabelMode(0);

        // $B6:E800 is the mutable equipment template normally copied to $7E:3800.
        // Preserve it as a byte array because the cartridge's offset tables contain WRAM
        // byte addresses rather than tilemap word indexes.
        equipmentTilemap = mapPresentation.PauseEquipmentBase.CreateTilemap();
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
            mapArrows!.StepArrows(direction => mapScroll.CanScroll(direction, mapHorizontalScroll, mapVerticalScroll));
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
            if (mapScroll.Step(heldInput ?? delayedHeldInput, ref mapHorizontalScroll, ref mapVerticalScroll))
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

    /// <summary>Composes the cartridge's BG2 frame and current BG1 page at 256x224.</summary>
    // Raster scratch reused across draws; never part of saved state (restores reallocate it).
    [NonSerialized] private Rgba32[]? objScratchPixels;
    /// <summary>Reusable per-pixel OBJ priority buffer for resolving SNES layer order.</summary>
    [NonSerialized] private byte[]? objScratchPriorities;
    /// <summary>Reusable expanded BG3 image for pause composition over the retained HUD.</summary>
    [NonSerialized] private Rgba32[]? bg3ScratchPlane;

    /// <summary>Renders the current pause PPU layers into a 256 by 224 framebuffer.</summary>
    /// <returns>The composed framebuffer in display order, without advancing menu animations.</returns>
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
        SnesObjRenderer.RenderResolved(oam, vram, cgram, PauseMenuLayout.ObjectSelection,
            objScratchPixels, objScratchPriorities, 256, 224);
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
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        PrepareRenderOam();
        ushort mapX = ScreenMode == 0 ? mapHorizontalScroll : (ushort)0;
        // BG fetches start on physical scanline one; OBJ coordinates already use
        // output-space Y. Keep the cartridge scroll and sprite origins unchanged.
        ushort mapY = unchecked((ushort)((ScreenMode == 0 ? mapVerticalScroll : 0) + SnesPpuLayout.FirstVisibleBackgroundScanline));
        var bg1 = new Bg4BppRenderLayer(PauseMenuLayout.Bg1TilemapWord, 0, mapX, mapY, 64, 32, false);
        var bg2 = new Bg4BppRenderLayer(PauseMenuLayout.Bg2TilemapWord, 0, 0, SnesPpuLayout.FirstVisibleBackgroundScanline, 32, 32, false);
        var bg3 = new Bg2BppViewportRenderLayer(SnesPpuLayout.GameplayHudTilemapWord,
            SnesPpuLayout.GameplayHudCharacterBaseWord,
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

    /// <summary>Builds OAM for page-specific markers and selectors before resolving OBJ priorities.</summary>
    private void PrepareRenderOam()
    {
        oam.BeginFrame();
        if (ScreenMode == 0)
        {
            var icons = new FileSelectMapIcons(system, area);
            icons.BindLandmarks(mapPresentation?.Landmarks);
            icons.BindSprites(mapPresentation?.Sprites);
            if (mapLabelsBeforeIcons) icons.DrawElevatorLabels(oam, mapHorizontalScroll, mapVerticalScroll);
            mapArrows?.DrawArrows(oam, mapPresentation?.Sprites, PauseMapScrollLayout.ArrowVerticalOffset);
            DrawMapPositionIndicator();
            // Native pause shares the boss lists, defeated overlays and downloaded
            // destination lettering with file select. Preserve their per-phase OAM order.
            icons.DrawBossMarkers(oam, mapHorizontalScroll, mapVerticalScroll);
            if (!mapLabelsBeforeIcons) icons.DrawElevatorLabels(oam, mapHorizontalScroll, mapVerticalScroll);
        }
        else
        {
            DrawEquipmentItemSelector();
            DrawReserveTanks();
        }
        oam.FinalizeFrame();
    }

    /// <summary>Composites one 4bpp pause background at the requested PPU priority.</summary>
    /// <param name="output">Framebuffer receiving the visible pixels.</param>
    /// <param name="tilemapBaseWord">VRAM word address of the layer's tilemap.</param>
    /// <param name="tilemapWidthInTiles">Tilemap width used to resolve tile entries.</param>
    /// <param name="priority">Whether to draw only high-priority tiles.</param>
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
            verticalScroll: unchecked((ushort)((isMapLayer && ScreenMode == 0 ? mapVerticalScroll : 0) + SnesPpuLayout.FirstVisibleBackgroundScanline)),
            width: 256,
            height: 224,
            tilemapWidthInTiles: tilemapWidthInTiles,
            tilemapHeightInTiles: 32,
            priority: priority);
    }

    /// <summary>Composites the retained gameplay HUD plane using pause's BG3 character data.</summary>
    /// <param name="output">Framebuffer receiving the visible pixels.</param>
    /// <param name="priority">Whether to draw only high-priority BG3 tiles.</param>
    private void CompositePauseBg3(Span<Rgba32> output, bool priority)
    {
        Rgba32[] plane = bg3ScratchPlane ??= new Rgba32[32 * 8 * 32 * 8];
        // The renderer skips tiles of the other priority, so clear the reused plane first.
        plane.AsSpan().Clear();
        SnesBgTilemapRenderer.Render2Bpp(
            plane,
            vram,
            cgram,
            tilemapBaseWord: 0x5800,
            characterBaseWord: 0x4000,
            rowCount: 32,
            transparentColorZero: true,
            priority: priority);
        SnesLayerCompositor.Composite(output, plane.AsSpan(
            SnesPpuLayout.FirstVisibleBackgroundScanline * SnesPpuLayout.ScreenWidthPixels,
            output.Length));
    }

    /// <summary>Copies resolved OBJ pixels belonging to one SNES priority band.</summary>
    /// <param name="output">Framebuffer receiving matching object pixels.</param>
    /// <param name="objects">Resolved object pixels and their per-pixel priorities.</param>
    /// <param name="priority">Priority band to composite.</param>
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

    /// <summary>Advances one fade or page-load phase while preserving native update spacing.</summary>
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

    /// <summary>Selects reserve mode when capacity exists, otherwise the first collected item.</summary>
    private void SelectFirstCollectedEquipment()
    {
        // $82:ABAD-$ABB5 selects the tank mode control whenever reserve capacity exists,
        // including empty tanks. The first beam is only the no-reserve fallback.
        if (samus.MaxReserveEnergy != 0)
        {
            selectedCategory = PauseEquipmentCategories.Reserves;
            selectedItem = PauseReserveTransferRomData.ModeItem;
            return;
        }
        for (int categoryIndex = 1; categoryIndex <= 3; categoryIndex++)
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

        selectedCategory = 0;
        selectedItem = 0;
    }

    /// <summary>Returns the collected inventory mask for a pause equipment category.</summary>
    /// <param name="category">Cartridge category index; category one denotes beams.</param>
    /// <returns>The live collected beam or item bits.</returns>
    private ushort GetCollectedBits(int category) =>
        category == 1 ? samus.CollectedBeams : samus.CollectedItems;

    /// <summary>Returns the equipped inventory mask for a pause equipment category.</summary>
    /// <param name="category">Cartridge category index; category one denotes beams.</param>
    /// <returns>The live equipped beam or item bits.</returns>
    private ushort GetEquippedBits(int category) =>
        category == 1 ? samus.EquippedBeams : samus.EquippedItems;

    /// <summary>Restores the authored equipment base and applies inventory-dependent labels.</summary>
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

    /// <summary>Writes the wireframe variant selected by Samus's currently equipped items.</summary>
    private void WriteSamusWireframe()
    {
        int variant = PauseEquipmentRules.WireframeIndex(samus.EquippedItems);
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause wireframe requires installed presentation assets."))
            .PauseWireframes.ApplyTo(equipmentTilemap, (PauseWireframeKind)variant);
    }

    /// <summary>Builds the selected area's visible pause map and uploads it to BG1.</summary>
    private void LoadPauseMapTilemap()
    {
        IAreaMapView map = (mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.")).Get(area);
        byte[] mapTilemap = AreaMapTilemapBuilder.Build(
            map, system, MapTileWords.PauseBlank, mapRevealMode);
        vram.LoadBytes(PauseMenuLayout.Bg1TilemapWord * 2, mapTilemap);

        // Installed backdrops already contain their authored area lettering.
    }

    /// <summary>Derives visible-map scroll bounds and centers the initial view around Samus.</summary>
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

    /// <summary>Draws the animated Samus marker at its room-relative area-map position.</summary>
    private void DrawMapPositionIndicator()
    {
        ushort x = unchecked((ushort)(
            8 * (roomMapX + (samus.XPosition >> 8)) - mapHorizontalScroll));
        ushort y = unchecked((ushort)(
            8 * (roomMapY + (samus.YPosition >> 8) + 1) - mapVerticalScroll));
        lastIndicatorOriginX = x;
        lastIndicatorOriginY = y;
        lastIndicatorSpritemapId = PauseMapIndicatorAnimation.SpritemapId(mapIndicatorAnimationFrame);
        DrawMenuSpritemap(
            lastIndicatorSpritemapId,
            x,
            y,
            ReadPauseSpritePaletteBits());
    }

    /// <summary>Advances the marker's cartridge-timed animation counter by one update.</summary>
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

    /// <summary>Restarts the equipment selector at its first phase and initial duration.</summary>
    private void ResetItemSelectorAnimation()
    {
        itemSelectorAnimationFrame = 0;
        itemSelectorAnimationTimer = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets."))
            .PauseSelectors.InitialDurationTicks;
    }

    /// <summary>Advances the equipment selector phase when the current duration expires.</summary>
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

    /// <summary>Draws the animated selector at the selected equipment item's installed anchor.</summary>
    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        var selector = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        var point = selector.Anchor(selectedCategory, selectedItem);
        lastIndicatorOriginX = (ushort)point.X;
        lastIndicatorOriginY = (ushort)point.Y;
        // Diagnostic identity denotes the native category binding, not an
        // author-supplied ROM index. Custom phases may use different artwork.
        lastIndicatorSpritemapId = PauseSelectorDefinitions.NativeSpriteId(selectedCategory);
        selector.Draw(oam, selectedCategory, selectedItem, itemSelectorAnimationFrame);
    }

    /// <summary>Returns the OBJ palette bits assigned to the pause-map position marker.</summary>
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

    /// <summary>Draws an installed map-menu composition at a screen-space origin.</summary>
    /// <param name="id">Stable native spritemap identity required by the pause asset catalog.</param>
    /// <param name="x">Horizontal screen coordinate.</param>
    /// <param name="y">Vertical screen coordinate.</param>
    /// <param name="paletteBits">OBJ palette selection bits applied to its OAM entries.</param>
    /// <exception cref="InvalidDataException">The requested composition is not installed.</exception>
    private void DrawMenuSpritemap(ushort id, ushort x, ushort y, ushort paletteBits)
    {
        // Map compositions use this shared entry. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        if (!MapSpriteDefinitions.Contains(id))
            throw new InvalidDataException($"Menu spritemap ${id:X4} is not installed.");
        (mapPresentation ?? throw new InvalidOperationException(
            "Menu spritemap requires installed presentation assets."))
            .Sprites.Draw(id, oam, x, y, paletteBits);
    }

    /// <summary>Uploads the current equipment-page tilemap to the pause BG1 destination.</summary>
    private void UploadEquipmentTilemap() =>
        vram.LoadBytes(PauseMenuLayout.Bg1TilemapWord * 2, equipmentTilemap);

    /// <summary>Gets the inventory bit used to test one item in its category.</summary>
    /// <param name="category">Definition containing the item category and mask layout.</param>
    /// <param name="item">Zero-based item index within the category.</param>
    /// <returns>The bit mask for that item.</returns>
    private static ushort ReadCategoryMask(PauseEquipmentCategoryDefinition category, int item) =>
        PauseEquipmentRules.Mask(category.Category, item);
}

/// <summary>Phases used to switch pause between map and equipment pages with cartridge fades.</summary>
internal enum PauseMenuTransition
{
    /// <summary>No page transition is active.</summary>
    None,
    /// <summary>The map page is fading to black before equipment setup.</summary>
    MapToEquipmentFadeOut,
    /// <summary>The equipment page is fading in after its setup dispatch.</summary>
    MapToEquipmentFadeIn,
    /// <summary>The equipment page is fading to black before map setup.</summary>
    EquipmentToMapFadeOut,
    /// <summary>The map page is fading in after its setup dispatch.</summary>
    EquipmentToMapFadeIn,
    /// <summary>The black-screen dispatch that installs the equipment page.</summary>
    MapToEquipmentLoad,
    /// <summary>The black-screen dispatch that restores the map page.</summary>
    EquipmentToMapLoad,
}
