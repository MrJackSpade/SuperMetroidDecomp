using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using System.Buffers.Binary;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Pause;

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
internal sealed class PauseMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Pause menu requires the installed sprite sheets.");
        screen.BindScreenTiles(sheets);
    }

    /// <summary>The installed sprite sheets the map's sprites draw from; artwork, never saved.</summary>
    [NonSerialized] private InstalledSpriteSheets spriteSheets;

    /// <summary>The map sprite sheet the map's markers, labels, arrows and position indicator draw from.</summary>
    private SpriteSheet MapSheet => spriteSheets.Get(SpriteSheetId.MapSprites);

    /// <summary>The pause sheet the equipment selectors and reserve-tank strip draw from.</summary>
    private SpriteSheet PauseSheet => spriteSheets.Get(SpriteSheetId.PauseSprites);


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
    /// <summary>The pause screen: its artwork, tilemaps (including the retained gameplay HUD rows), palette and sprite entries (#1358).</summary>
    internal readonly PauseScreen screen;
    /// <summary>Mutable BG1 equipment page including current inventory labels and tanks.</summary>
    internal readonly byte[] equipmentTilemap;
    /// <summary>Mutable button-label source whose palette bits survive page transitions.</summary>
    private readonly byte[] pauseButtonTilemap;
    /// <summary>Current map/equipment fade or load phase.</summary>
    internal PauseMenuTransition Transition { get; private set; }
    /// <summary>Counter inserted between brightness changes during fade-in.</summary>
    private int transitionFadeCounter;
    /// <summary>Current page-transition brightness, from black to full intensity.</summary>
    internal int TransitionBrightness { get; private set; } = 15;
    /// <summary>Equipment or reserve category selected by the native cursor.</summary>
    internal PauseEquipmentCategory SelectedCategory { get; private set; }
    /// <summary>Selected item index within the current equipment category.</summary>
    internal int SelectedItem { get; private set; }
    /// <summary>Current BG1 map scroll in map pixels.</summary>
    private Position16<MapPixels> mapScrollPosition;
    /// <summary>The map screen's scroll position.</summary>
    internal Position16<MapPixels> MapScrollPosition { get => mapScrollPosition; set => mapScrollPosition = value; }
    /// <summary>Bounded map-scroll controller initialized from visible area cells.</summary>
    internal PauseMapScroll? CurrentMapScrollBounds { get; set; }
    /// <summary>The map-scroll bounds, which exist once the map page is set up.</summary>
    internal PauseMapScroll MapScrollBounds => CurrentMapScrollBounds ??
        throw new InvalidOperationException("The pause map scroll has not been set up.");
    /// <summary>Animated map arrows and their current visibility state.</summary>
    internal FileSelectMapAnimations? CurrentMapArrows { get; set; }
    /// <summary>The map arrows, which exist once the map page is set up.</summary>
    internal FileSelectMapAnimations MapArrows => CurrentMapArrows ??
        throw new InvalidOperationException("The pause map arrows have not been set up.");
    /// <summary>Whether elevator labels draw before map icons during a fade-out.</summary>
    private bool mapLabelsBeforeIcons;
    /// <summary>Current bank-$82 map-position marker frame.</summary>
    internal int MapIndicatorAnimationFrame { get; set; }
    /// <summary>Remaining updates in the map-position marker frame.</summary>
    internal int MapIndicatorAnimationTimer { get; set; }
    /// <summary>Current equipment selector sprite frame.</summary>
    internal int ItemSelectorAnimationFrame { get; set; }
    /// <summary>Remaining updates in the equipment selector frame.</summary>
    internal int ItemSelectorAnimationTimer { get; set; }
    /// <summary>Tracks the retail Boots-to-Plasma label copy overrun across artwork rebinds.</summary>
    private bool plasmaLabelOverrunActive;
    /// <summary>Last selected map marker or equipment selector origin for diagnostics.</summary>
    internal Position16<OamPixels> LastIndicatorOrigin { get; private set; }
    /// <summary>Pose of the Samus map marker when it was the last indicator drawn, for diagnostics; null after a selector draw.</summary>
    internal MapSpriteId? LastMapIndicatorSprite { get; private set; }
    /// <summary>Category of the equipment selector when it was the last indicator drawn, for diagnostics; null after a map-marker draw.</summary>
    internal PauseEquipmentCategory? LastSelectorCategory { get; private set; }

    /// <summary>Creates the isolated pause PPU image and initial map/equipment state from live gameplay.</summary>
    public PauseMenuState(PauseMenuEntry entry)
    {

        this.mapPresentation = entry.MapPresentation ?? throw new InvalidOperationException(
            "Pause menu requires installed map and equipment presentation assets.");

        this.samus = Ensure.NotNull(entry.Samus);
        this.system = Ensure.NotNull(entry.System);
        this.audio = entry.Audio;
        CurrentMapArrows = new FileSelectMapAnimations(entry.MapPresentation.Arrows);
        paletteAnimation = new MapPaletteAnimation();
        paletteAnimation.Bind(entry.MapPresentation.HighlightCycle);
        area = entry.AreaIndex;
        _ = AreaIds.ToIndex(entry.AreaIndex);
        this.roomMapPosition = entry.RoomMapPosition;
        this.mapRevealMode = Enum.IsDefined(entry.MapRevealMode)
            ? entry.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(entry.MapRevealMode));

        // GameState_13 uploads the map, pause interface, menu OBJ and HUD characters; the
        // screen binds that artwork and its capture places the characters.
        screen = new PauseScreen(entry.MapPresentation, entry.SpriteSheets);
        BindSpriteSheets(entry.SpriteSheets);
        LoadPauseBackdrop();
        if (entry.GameplayLayer3 is not null)
        {
            // SetupPPUForPauseMenu changes BG3SC to $58 but never uploads a replacement
            // tilemap. The four-row gameplay HUD already resident at VRAM word $5800 is
            // intentionally retained. A fresh host-side VRAM object used to discard that
            // state, removing the entire energy/ammo HUD from pause and exposing map BG1
            // pixels in the area that its BG3 plane normally covers. Gameplay's BG3 plane
            // is the displayed tilemap (#1351), so pause takes those words from it.
            screen.Tilemaps.RetainHud(entry.GameplayLayer3);
        }

        // GameState_13 finishes pause setup by calling QueueClearingOfFxTilemap at
        // `$80:A211`. The accepted NMI fills VRAM words $5880-$5FFF with $184E, preserving
        // the first four HUD rows at $5800-$587F while blanking every row beneath them.
        // This clear is essential because pause replaces BG3's character sheet: an
        // untouched zero tilemap word selects pause character zero, which is the visible
        // orange `1` glyph. Copying only the live HUD without replaying this queued DMA
        // consequently tiled `1` through every otherwise-empty map cell.
        screen.Tilemaps.ClearFx(PauseMenuLayout.Bg3FxClearTile);
        screen.Palette.Load(entry.MapPresentation.Palettes.Pause);

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
    public PauseScreenMode ScreenMode { get; private set; }

    /// <summary>
    /// Runs state-$0F menu input after the caller has latched NMI input and invoked the
    /// bank-$80 delayed-held filter. Returns true when Start requests game state $10.
    /// The runtime supplies its accepted-NMI byte counter for cartridge palette phase;
    /// standalone diagnostic callers may leave that phase at zero.
    /// </summary>
    public bool Step(SnesButton delayedHeldInput, SnesButton newlyPressedInput, SnesButton? heldInput = null, int frameCounterByte = 0)
    {
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations(frameCounterByte);
        if (ScreenMode == PauseScreenMode.Map && Transition == PauseMenuTransition.None)
            MapArrows.StepArrows(direction => MapScrollBounds.CanScroll(direction, MapScrollPosition));
        SnesButton delayedPressed = delayedHeldInput;
        SnesButton newlyPressed = newlyPressedInput;
        if (Transition != PauseMenuTransition.None)
        {
            StepPageTransition();
            return false;
        }

        if (ScreenMode == PauseScreenMode.Map)
        {
            // `$82:9120`: L/R, then Start, then map scrolling. A Start press does not stop
            // the same frame's scroll pulse.
            if ((delayedPressed & SnesButton.R) != 0)
            {
                audio?.QueueSound(Library1Sound.MenuConfirm, maximumQueued: 6);
                SetPauseButtonLabelMode(2);
                Transition = PauseMenuTransition.MapToEquipmentFadeOut;
                TransitionBrightness = 15;
            }
            bool mapStart = HandleStartButton(delayedPressed);
            // Map arrows read the ordinary held controller word, not the delayed chrome
            // input. An accepted pulse keeps running after release, as on the cartridge.
            if (MapScrollBounds.Step(heldInput ?? delayedHeldInput, ref mapScrollPosition))
                audio?.QueueSound(Library1Sound.MapScroll, maximumQueued: 6);
            return mapStart;
        }

        // `$82:9142`: EquipmentScreenMain, then L/R, then Start, so an A press in the
        // frame that unpauses still toggles the selected item. EquipmentScreenMain consumes
        // the ordinary $8F rising-edge word for D-pad/A; only the shared L/R/Start chrome
        // uses the delayed-held word at $05DF.
        HandleEquipmentInput(newlyPressed, frameCounterByte);
        // EquipmentScreenMain refreshes the reserve amount independently of selected
        // label edits. Keep this per-frame write without rebuilding the whole tilemap,
        // which would erase the cartridge's same-frame VAR overrun.
        WriteReserveSupplyDigits();
        UploadEquipmentTilemap();
        if ((delayedPressed & SnesButton.L) != 0)
        {
            audio?.QueueSound(Library1Sound.MenuConfirm, maximumQueued: 6);
            SetPauseButtonLabelMode(0);
            Transition = PauseMenuTransition.EquipmentToMapFadeOut;
            TransitionBrightness = 15;
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
        audio?.QueueSound(Library1Sound.MenuConfirm, maximumQueued: 6);
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
    public void AdvanceAnimations(int frameCounterByte = 0, PauseAnimationPhase phase = PauseAnimationPhase.Stable)
    {
        // $82:9156 and $82:9353 emit destination labels before map icons during fade-out.
        mapLabelsBeforeIcons = phase == PauseAnimationPhase.FadingOut || Transition == PauseMenuTransition.MapToEquipmentFadeOut;
        // Only stable map dispatch calls $82:B934. Fade paths retain counters but hide arrows.
        CurrentMapArrows?.StepArrows(_ => false);
        pauseFrameCounterByte = frameCounterByte;
        // $82:90E8 calls the palette handler after stable pause dispatch. The
        // outer pause/unpause fades draw sprites without advancing this owner.
        if (phase == PauseAnimationPhase.Stable && paletteAnimation.Step(screen.Palette))
            audio?.QueueSound(Library3Sound.PauseBeep, maximumQueued: 6);
        if (ScreenMode == PauseScreenMode.Map)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Prepares OAM on the simulation owner, then captures without raster work.</summary>
    [PpuLayoutBoundary("codec: pause render packet layers")]
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        ushort mapX = ScreenMode == PauseScreenMode.Map ? MapScrollPosition.X : (ushort)0;
        // BG fetches start on physical scanline one; OBJ coordinates already use
        // output-space Y. Keep the cartridge scroll and sprite origins unchanged.
        ushort mapY = unchecked((ushort)((ScreenMode == PauseScreenMode.Map ? MapScrollPosition.Y : 0) + SnesPpuLayout.FirstVisibleBackgroundScanline));
        // Every plane draws from the pause screen's pre-coloured tiles (#1392).
        ScreenLayerPictures planes = screen.PlaneTiles;
        var bg1 = new TileRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg1, new Extent16<TilemapCells>(64, 32), planes),
            new Position16<BgPlanePixels>(mapX, mapY), false);
        var bg2 = new TileRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg2, new Extent16<TilemapCells>(32, 32), planes),
            new Position16<BgPlanePixels>(0, SnesPpuLayout.FirstVisibleBackgroundScanline), false);
        // The HUD plane shows its first screen through the viewport.
        var bg3 = new TileViewportRenderLayer(ScreenTileMap.Of(screen.Tilemaps.Bg3[..PauseTilemaps.ScreenWords], TileViewportRenderLayer.MapSize,
            screen.HudTiles), SnesPpuLayout.FirstVisibleBackgroundScanline, true, false);
        RenderLayer[] layers =
        [
            new ObjPriorityRenderLayer(0), bg3,
            new ObjPriorityRenderLayer(1), bg2, bg1,
            new ObjPriorityRenderLayer(2), bg2 with { Priority = true }, bg1 with { Priority = true },
            new ObjPriorityRenderLayer(3), bg3 with { Priority = true },
        ];
        return new(screen.Paint(), layers, checked((byte)(Transition == PauseMenuTransition.None ? 15 : TransitionBrightness)));
    }

    /// <summary>Assembles map markers or equipment sprites in their native page-specific order.</summary>
    private void PrepareRenderSprites()
    {
        screen.Objects.BeginFrame();
        if (ScreenMode == PauseScreenMode.Map)
        {
            var icons = new FileSelectMapIcons(system, area);
            icons.BindLandmarks(mapPresentation?.Landmarks);
            icons.BindSprites(MapSheet);
            if (mapLabelsBeforeIcons) icons.DrawElevatorLabels(screen.Objects, MapScrollPosition);
            CurrentMapArrows?.DrawArrows(screen.Objects, MapSheet, PauseMapScrollLayout.ArrowVerticalOffset);
            DrawMapPositionIndicator();
            // Native pause shares the boss lists, defeated overlays and downloaded
            // destination lettering with file select. Preserve their per-phase OAM order.
            icons.DrawBossMarkers(screen.Objects, MapScrollPosition);
            if (!mapLabelsBeforeIcons) icons.DrawElevatorLabels(screen.Objects, MapScrollPosition);
        }
        else
        {
            DrawEquipmentItemSelector();
            DrawReserveTanks();
        }
        screen.Objects.FinishFrame();
    }

    /// <summary>Advances map/equipment fade, page load, and brightness phases.</summary>
    private void StepPageTransition()
    {
        switch (Transition)
        {
            case PauseMenuTransition.MapToEquipmentFadeOut:
            case PauseMenuTransition.EquipmentToMapFadeOut:
                TransitionBrightness--;
                if (TransitionBrightness > 0)
                    return;

                // $82:9156/$9186 returns at black before the separate load dispatch.
                Transition = Transition == PauseMenuTransition.MapToEquipmentFadeOut
                    ? PauseMenuTransition.MapToEquipmentLoad : PauseMenuTransition.EquipmentToMapLoad;
                return;

            case PauseMenuTransition.MapToEquipmentLoad:
            case PauseMenuTransition.EquipmentToMapLoad:
                if (Transition == PauseMenuTransition.MapToEquipmentLoad)
                {
                    ScreenMode = PauseScreenMode.Equipment;
                    // $82:AB47 resets the selector on each equipment-page entry.
                    SelectFirstCollectedEquipment();
                    // Equipment setup lights the arrow when reserves are nonempty;
                    // subsequent tank dispatches own animation and selection changes.
                    if (samus.ReserveEnergy != 0) SetReserveArrow(ReserveArrowState.Solid);
                    UploadEquipmentTilemap();
                    ResetItemSelectorAnimation();
                    Transition = PauseMenuTransition.MapToEquipmentFadeIn;
                }
                else
                {
                    ScreenMode = PauseScreenMode.Map;
                    LoadPauseMapTilemap();
                    Transition = PauseMenuTransition.EquipmentToMapFadeIn;
                }
                TransitionBrightness = 0;
                transitionFadeCounter = PauseFadeTiming.CounterReload;
                return;

            case PauseMenuTransition.MapToEquipmentFadeIn:
            case PauseMenuTransition.EquipmentToMapFadeIn:
                // Load dispatches set delay/counter to one; $80:894D spends a
                // counter-only update between brightness writes.
                if (transitionFadeCounter-- > 0) return;
                transitionFadeCounter = PauseFadeTiming.CounterReload;
                TransitionBrightness++;
                if (TransitionBrightness >= 15)
                {
                    TransitionBrightness = 15;
                    Transition = PauseMenuTransition.None;
                }
                return;

            case PauseMenuTransition.None:
            default:
                throw new InvalidDataException($"Invalid pause-page transition {Transition}.");
        }
    }

    /// <summary>Selects reserves first when available, otherwise the first collected inventory item.</summary>
    private void SelectFirstCollectedEquipment()
    {
        // $82:ABAD-$ABB5 selects the tank mode control whenever reserve capacity exists,
        // including empty tanks. The first beam is only the no-reserve fallback.
        if (samus.MaxReserveEnergy != 0)
        {
            SelectedCategory = PauseEquipmentCategory.Reserves;
            SelectedItem = PauseReserveTransferRomData.ModeItem;
            return;
        }
        foreach (PauseEquipmentCategory categoryIndex in PauseEquipmentCategories.InventoryCategories)
        {
            PauseEquipmentCategoryDefinition category = PauseEquipmentCategories.Get(categoryIndex);
            for (int item = 0; item < category.ItemCount; item++)
            {
                if (!ReadCategoryMask(category, item).IsSetIn(samus.CollectedBeams, samus.CollectedItems))
                    continue;
                SelectedCategory = categoryIndex;
                SelectedItem = item;
                return;
            }
        }

        SelectedCategory = PauseEquipmentCategory.Reserves;
        SelectedItem = 0;
    }

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
            samus.EquippedItems, samus.HyperBeamEnabled);

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
        MenuTilemaps.Load(screen.Tilemaps.Bg1, mapTilemap);

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
        CurrentMapScrollBounds = new PauseMapScroll(new(minimumX, minimumY), new(maximumX, maximumY));

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
        ushort verticalCenterOffset = unchecked((ushort)((128 - verticalMiddle) & ~7));
        ushort vertical = unchecked((ushort)-verticalCenterOffset);
        ushort playerMapY = unchecked((ushort)(player.Y + verticalCenterOffset));
        short distanceFromTopClamp = unchecked((short)(64 - playerMapY));
        if (distanceFromTopClamp >= 0)
        {
            vertical = unchecked((ushort)(vertical - distanceFromTopClamp));
            if (unchecked((short)(vertical + 40)) < 0)
                vertical = unchecked((ushort)-40);
        }
        MapScrollPosition = new(horizontal, vertical);
    }

    /// <summary>Maps Samus's current room screen to the area map with its one-row Y convention.</summary>
    private Position16<MapTiles> SamusMapCell() =>
        roomMapPosition + new Offset16<MapTiles>(checked((short)(SnesEightEight.WholeOf(samus.Position.X.Whole))), checked((short)((SnesEightEight.WholeOf(samus.Position.Y.Whole)) + 1)));

    /// <summary>Draws the animated Samus map marker at the scrolled screen position.</summary>
    private void DrawMapPositionIndicator()
    {
        Position16<OamPixels> origin = SamusMapCell().ToMapPixels().ToScreen(MapScrollPosition).ToOam();
        LastIndicatorOrigin = origin;
        MapSpriteId indicator = PauseMapIndicatorAnimation.SpritemapId(MapIndicatorAnimationFrame);
        LastMapIndicatorSprite = indicator;
        LastSelectorCategory = null;
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
        if (MapIndicatorAnimationTimer == 0)
        {
            MapIndicatorAnimationFrame = (MapIndicatorAnimationFrame + 1) & (PauseMapIndicatorAnimation.FrameCount - 1);
            MapIndicatorAnimationTimer = PauseMapIndicatorAnimation.FrameDelay(MapIndicatorAnimationFrame);
        }
        MapIndicatorAnimationTimer--;
    }

    /// <summary>Restarts the equipment selector at its first authored frame.</summary>
    private void ResetItemSelectorAnimation()
    {
        ItemSelectorAnimationFrame = 0;
        ItemSelectorAnimationTimer = (mapPresentation ?? throw new InvalidOperationException(
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
        ItemSelectorAnimationTimer--;
        if (ItemSelectorAnimationTimer > 0)
            return;

        var selectors = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        ItemSelectorAnimationFrame = selectors.NormalizePhase(ItemSelectorAnimationFrame + 1);
        ItemSelectorAnimationTimer = selectors.Duration(ItemSelectorAnimationFrame);
    }

    /// <summary>Draws the selected equipment item's authored sprite and records its origin.</summary>
    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == 0 && samus.CollectedBeams == 0)
            return;

        var selector = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        var point = selector.Anchor(SelectedCategory, SelectedItem);
        LastIndicatorOrigin = point;
        // The diagnostic records the category binding, not the authored artwork,
        // because custom phases may draw different compositions.
        LastSelectorCategory = SelectedCategory;
        LastMapIndicatorSprite = null;
        selector.Draw(screen.Objects, PauseSheet, SelectedCategory, SelectedItem, ItemSelectorAnimationFrame);
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
        // Labels draw bright in BG palette two and dim in palette five.
        const int BrightLabelPalette = 2;
        const int DimLabelPalette = 5;
        void SetPalette(int sourceWordIndex, int wordCount, int palette)
        {
            int localWordIndex = sourceWordIndex - PauseMenuLayout.ButtonSourceWordOrigin;
            for (int word = 0; word < wordCount; word++)
            {
                int byteOffset = (localWordIndex + word) * 2;
                ushort tile = unchecked(
                    SnesLittleEndian.Word(pauseButtonTilemap[byteOffset], pauseButtonTilemap[byteOffset + 1]));
                tile = new SnesBgTilemapWord(tile).WithPaletteIndex(palette);
                pauseButtonTilemap[byteOffset] = unchecked((byte)tile);
                pauseButtonTilemap[byteOffset + 1] = SnesLittleEndian.HighByte(tile);
            }
        }

        switch (mode)
        {
            case 0: // Stable map page: MAP bright, EQUIPMENT/START dim.
                SetPalette(822, 5, BrightLabelPalette);
                SetPalette(854, 5, BrightLabelPalette);
                SetPalette(812, 4, BrightLabelPalette);
                SetPalette(844, 4, BrightLabelPalette);
                SetPalette(805, 5, DimLabelPalette);
                SetPalette(837, 5, DimLabelPalette);
                break;

            case 1: // Start/unpause highlight.
                SetPalette(812, 4, BrightLabelPalette);
                SetPalette(844, 4, BrightLabelPalette);
                SetPalette(805, 5, DimLabelPalette);
                SetPalette(837, 5, DimLabelPalette);
                SetPalette(822, 5, DimLabelPalette);
                SetPalette(854, 5, DimLabelPalette);
                break;

            case 2: // Stable equipment page: EQUIPMENT bright, MAP/START dim.
                SetPalette(805, 5, BrightLabelPalette);
                SetPalette(837, 5, BrightLabelPalette);
                SetPalette(812, 4, BrightLabelPalette);
                SetPalette(844, 4, BrightLabelPalette);
                SetPalette(822, 5, DimLabelPalette);
                SetPalette(854, 5, DimLabelPalette);
                break;

            default:
                throw new ArgumentOutOfRangeException(nameof(mode), mode, "Pause label mode must be 0..2.");
        }

        UploadButtonRows();
    }

    /// <summary>Draws a map sprite sheet frame with its selected palette bits.</summary>
    private void DrawMenuSpritemap(MapSpriteId id, Position16<OamPixels> origin, ushort paletteBits) =>
        // Map compositions use this shared entry. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        MapSpriteSheet.Draw(MapSheet, id, screen.Objects, origin, paletteBits);

    /// <summary>Shows the current mutable equipment page on BG1's first screen, leaving the second as the map left it.</summary>
    private void UploadEquipmentTilemap() =>
        MenuTilemaps.Load(screen.Tilemaps.Bg1[..(equipmentTilemap.Length / sizeof(ushort))], equipmentTilemap);

    /// <summary>Shows the mutable button-label source's two rows on BG2, as the queued $7E:3640 transfer does.</summary>
    private void UploadButtonRows() => MenuTilemaps.Load(screen.Tilemaps.ButtonRows,
        pauseButtonTilemap.AsSpan(PauseMenuLayout.ButtonRowsSourceOffset, PauseMenuLayout.ButtonRowsByteCount));

    /// <summary>Gets the inventory bit represented by an item in an equipment category.</summary>
    private static PauseEquipmentBit ReadCategoryMask(PauseEquipmentCategoryDefinition category, int item) =>
        PauseEquipmentRules.Mask(category.Category, item);

    /// <summary>Dispatches reserve input or moves and toggles equipment in native update order.</summary>
    private void HandleEquipmentInput(SnesButton pressed, int frameCounterByte)
    {
        PauseEquipmentCategory dispatchedCategory = SelectedCategory;
        if (dispatchedCategory == PauseEquipmentCategory.Reserves)
        {
            HandleReserveInput(pressed);
            MoveEquipmentSelector(pressed);
            UpdateReserveArrow(frameCounterByte);
            return;
        }
        MoveEquipmentSelector(pressed);
        if ((pressed & SnesButton.A) == 0)
            return;
        if (dispatchedCategory == PauseEquipmentCategory.Beams && samus.CollectedBeams == 0)
            return;
        if (SelectedCategory == PauseEquipmentCategory.Reserves)
            throw new NotSupportedException("Same-frame equipment toggle into reserve tables requires native out-of-table WRAM behavior.");

        // Dispatch precedes movement. Boots->Plasma therefore retains the Boots copy
        // length and bypasses the Weapons handler's Spazer/Plasma exclusion. Do not use
        // upstream C's 'Fixed var bug' category guard: it is absent from retail ROM.
        SamusBeamFlags previousBeams = samus.EquippedBeams;
        var target = PauseEquipmentCategories.Get(SelectedCategory);
        PauseEquipmentBit mask = ReadCategoryMask(target, SelectedItem);
        bool wasEquipped = mask.IsSetIn(samus.EquippedBeams, samus.EquippedItems);
        // The bit belongs to its own category's set, so toggling both sets toggles exactly that upgrade.
        samus.EquippedBeams ^= mask.Beam;
        samus.EquippedItems ^= mask.Item;

        audio?.QueueSound(Library1Sound.MenuConfirm, maximumQueued: 6);
        UpdateEquipmentLabel(SelectedCategory, SelectedItem,
            PauseEquipmentCategories.Get(dispatchedCategory).LabelWordCount, wasEquipped);
        if (dispatchedCategory == PauseEquipmentCategory.Beams)
        {
            SamusBeamFlags added = samus.EquippedBeams & ~previousBeams;
            if ((added & SamusBeamFlags.Spazer) != 0 &&
                (samus.EquippedBeams & SamusBeamFlags.Plasma) != 0)
            {
                samus.EquippedBeams &= ~SamusBeamFlags.Plasma;
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
            else if ((added & SamusBeamFlags.Plasma) != 0 &&
                (samus.EquippedBeams & SamusBeamFlags.Spazer) != 0)
            {
                samus.EquippedBeams &= ~SamusBeamFlags.Spazer;
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
        if (categoryIndex == PauseEquipmentCategory.Beams && samus.HyperBeamEnabled)
            return false;
        var category = PauseEquipmentCategories.Get(categoryIndex);
        for (int item = start; item >= 0 && item < category.ItemCount; item += step)
        {
            if (!ReadCategoryMask(category, item).IsSetIn(samus.CollectedBeams, samus.CollectedItems))
            {
                // $82:B4B7 checks byte offset ten only AFTER an unsuccessful probe.
                // Thus directly entering the final suit item can succeed, but scanning
                // forward from an earlier missing item must not discover it.
                if (categoryIndex == PauseEquipmentCategory.Suits && step > 0 &&
                    item + step >= category.ItemCount - 1)
                    return false;
                continue;
            }
            SelectedCategory = categoryIndex;
            SelectedItem = item;
            audio?.QueueSound(Library1Sound.MenuCursor, maximumQueued: 6);
            return true;
        }
        return false;
    }

    /// <summary>Moves the selector to reserve mode when Samus has reserve capacity.</summary>
    private bool TrySelectReserves()
    {
        if (samus.MaxReserveEnergy == 0) return false;
        SelectedCategory = PauseEquipmentCategory.Reserves;
        SelectedItem = 0;
        audio?.QueueSound(Library1Sound.MenuCursor, maximumQueued: 6);
        return true;
    }

    /// <summary>Applies native directional links among beam, suit, boot, and reserve rows.</summary>
    private void MoveEquipmentSelector(SnesButton pressed)
    {
        bool left = (pressed & SnesButton.Left) != 0, right = (pressed & SnesButton.Right) != 0;
        bool up = (pressed & SnesButton.Up) != 0, down = (pressed & SnesButton.Down) != 0;
        PauseEquipmentCategory beams = PauseEquipmentCategory.Beams, suits = PauseEquipmentCategory.Suits, boots = PauseEquipmentCategory.Boots;
        switch (SelectedCategory)
        {
            case PauseEquipmentCategory.Beams:
                if (right)
                {
                    if (!TrySelectEquipment(suits, up ? 0 : PauseEquipmentCategories.BeamRightSuitItem, 1) && !up)
                        TrySelectEquipment(boots, 0, 1);
                }
                else if (down) TrySelectEquipment(beams, SelectedItem + 1, 1);
                else if (up && !TrySelectEquipment(beams, SelectedItem - 1, -1)) TrySelectReserves();
                break;
            case PauseEquipmentCategory.Suits:
                if (left)
                {
                    if (down || !TrySelectReserves()) TrySelectEquipment(beams, 0, 1);
                }
                else if (up) TrySelectEquipment(suits, SelectedItem - 1, -1);
                else if (down && !TrySelectEquipment(suits, SelectedItem + 1, 1)) TrySelectEquipment(boots, 0, 1);
                break;
            case PauseEquipmentCategory.Boots:
                if (left)
                {
                    if (up || !TrySelectEquipment(beams, PauseEquipmentCategories.PlasmaItem, -1)) TrySelectReserves();
                }
                else if (down) TrySelectEquipment(boots, SelectedItem + 1, 1);
                else if (up && !TrySelectEquipment(boots, SelectedItem - 1, -1))
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
                        SelectedItem--;
                        audio?.QueueSound(Library1Sound.MenuCursor, maximumQueued: 6);
                    }
                }
                else if (down)
                {
                    if (SelectedReserveItem == PauseReserveItem.Transfer ||
                        samus.ReserveTankMode == ReserveTankMode.Auto)
                        TrySelectEquipment(beams, 0, 1);
                    else
                    {
                        SelectedItem++;
                        if (samus.ReserveEnergy == 0) TrySelectEquipment(beams, 0, 1);
                        else audio?.QueueSound(Library1Sound.MenuCursor, maximumQueued: 6);
                    }
                }
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseEquipmentCategory)} {SelectedCategory}.");
        }
    }

    /// <summary>Retains only the HUD gameplay's last NMI displayed, not the pending WRAM tilemap.</summary>
    /// <param name="gameplayLayer3">The BG3 plane gameplay latched at that NMI (#1351).</param>
    public void SynchronizeAcceptedHud(Layer3Plane gameplayLayer3)
    {
        // Pause owns an isolated PPU image. Mirror the native mutable HUD range,
        // leaving its static first row and deliberately cleared FX rows untouched.
        screen.Tilemaps.MirrorAcceptedHud(gameplayLayer3);
    }

    /// <summary>Rebinds edited map and pause artwork while retaining live scroll, palette, and label state.</summary>
    public void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        AreaMapPresentationCatalog? previousCatalog = mapPresentation;
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Pause menu requires installed map presentation assets.");
        CurrentMapArrows ??= new FileSelectMapAnimations(catalog.Arrows);
        CurrentMapArrows.BindPresentation(catalog.Arrows);
        paletteAnimation.Bind(catalog.HighlightCycle);
        screen.BindArtwork(catalog);
        // Live highlights and reserve-arrow colors belong to their animation
        // owners. Replacing the static base must not reset their phase.
        ColourRun arrow = screen.Palette.BackgroundRow(PauseReserveUiDefinitions.ArrowBackgroundRow);
        screen.Palette.LoadExcept(catalog.Palettes.Pause, screen.Palette.SpriteRow(MapScreenDefinitions.HighlightSpriteRow),
            arrow.Slice(PauseReserveUiDefinitions.ArrowFirstColour, 1), arrow.Slice(PauseReserveUiDefinitions.ArrowSecondColour, 1));
        LoadPauseBackdrop();
        RefreshPauseButtonArtwork();
        // Refresh the authored base and rebuild the now-semantic inventory layer. The
        // explicit overrun bit retains the retail Boots-to-Plasma VAR artifact without
        // preserving stale pixels at a label's old editable destination.
        if (previousCatalog is null ||
            previousCatalog.PauseEquipmentLabels.ContentIdentity != catalog.PauseEquipmentLabels.ContentIdentity)
        {
            catalog.PauseEquipmentBase.RebindBeforeInventoryRefreshInto(equipmentTilemap);
            catalog.PauseEquipmentLabels.ApplyInventory(equipmentTilemap,
                samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems,
                samus.EquippedItems, samus.HyperBeamEnabled);
            if (plasmaLabelOverrunActive)
                catalog.PauseEquipmentLabels.ApplyLabel(equipmentTilemap,
                    PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Boots).LabelWordCount,
                    disabled: false);
        }
        else catalog.PauseEquipmentBase.RebindBaseInto(
            equipmentTilemap, previousCatalog.PauseEquipmentLabels);
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
            else if (SelectedCategory == PauseEquipmentCategory.Reserves)
                UpdateReserveArrow(pauseFrameCounterByte);
        }
        if (ScreenMode != PauseScreenMode.Map) UploadEquipmentTilemap();
        // The serialized live button palette rows still own their overlay on
        // the refreshed backdrop, including a Start/fade highlight.
        // Do not rerun controls or rebuild equipment: that would erase native
        // same-frame label overruns and change the state being restored.
        UploadButtonRows();
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that VRAM.
        if (ScreenMode == PauseScreenMode.Map) LoadPauseMapTilemap();
    }

    /// <summary>Loads the selected area's installed pause backdrop into BG2.</summary>
    private void LoadPauseBackdrop()
    {
        (mapPresentation ?? throw new InvalidOperationException(
            "Pause backdrop requires installed presentation assets."))
            .PauseBackdrops.LoadTo(screen.Tilemaps.Bg2, area);
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
    /// <summary>Last requested reserve-arrow state.</summary>
    private ReserveArrowState reserveArrowLatchedState;
    /// <summary>Eight-bit frame phase (zero through 255) retained for reproducing the arrow's current color on rebind.</summary>
    private int reserveArrowLatchedFrame;

    /// <summary>Runs $82:AD0A after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(int frameCounterByte)
    {
        bool inTanks = SelectedCategory == PauseEquipmentCategory.Reserves;
        ReserveArrowState state =
            inTanks && SelectedReserveItem == PauseReserveItem.Mode && samus.ReserveTankMode == ReserveTankMode.Auto ? ReserveArrowState.Animated
            : inTanks && SelectedReserveItem == PauseReserveItem.Transfer ? ReserveArrowState.Solid
            : ReserveArrowState.Hidden;
        SetReserveArrow(state, frameCounterByte);
    }

    /// <summary>Applies reserve-arrow tile and palette state, then latches it for content rebinds.</summary>
    private void SetReserveArrow(ReserveArrowState state, int frameCounterByte = 0)
    {
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve arrow requires installed presentation assets.")).PauseReserveUi;
        presentation.ApplyArrowColors(screen.Palette.BackgroundRow(PauseReserveUiDefinitions.ArrowBackgroundRow),
            state == ReserveArrowState.Animated, frameCounterByte);
        presentation.ApplyArrowTilePalettes(equipmentTilemap, state != ReserveArrowState.Hidden);
        reserveArrowLatched = true;
        reserveArrowLatchedState = state;
        reserveArrowLatchedFrame = frameCounterByte;
    }

    /// <summary>Re-applies the latched arrow tiles and colors with the currently bound presentation.</summary>
    private void ReapplyLatchedReserveArrow() =>
        SetReserveArrow(reserveArrowLatchedState, reserveArrowLatchedFrame);

    /// <summary>Writes reserve mode and tank labels only when reserve capacity exists.</summary>
    private void WriteReserveLabels()
    {
        // Absence of capacity leaves the original blank template intact. Restoring
        // the template before rebuilding also removes labels if inventory is changed.
        if (samus.MaxReserveEnergy == 0) return;
        PauseReserveUiPresentation presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve labels require installed presentation assets.")).PauseReserveUi;
        presentation.ApplyLabel(equipmentTilemap, PauseReserveLabel.Mode);
        presentation.ApplyLabel(equipmentTilemap, PauseReserveLabel.ReserveTank);
        // Native setup retains the initial MANUAL label for the zero/uninitialized
        // mode and patches its first four characters only after a nonzero mode exists.
        if (samus.ReserveTankMode != ReserveTankMode.None)
            presentation.ApplyLabel(equipmentTilemap,
                samus.ReserveTankMode == ReserveTankMode.Auto ? PauseReserveLabel.Auto : PauseReserveLabel.Manual,
                attributes: LabelAttributes.Preserve);
    }

    /// <summary>Accepted eight-bit frame phase (zero through 255) retained so repeated renders cannot advance tank fill flicker.</summary>
    private int pauseFrameCounterByte;

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
                (pauseFrameCounterByte & PauseReserveTankRomData.FlickerFrameMask) == 0)
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
                .PauseReserveTanks.Draw(screen.Objects, PauseSheet, map, index);
        }
    }

    /// <summary>Native $0757 sound cadence, advanced only by the selected reserve-transfer handler.</summary>
    internal WrappingCountdown ReserveTransferSoundDelay { get; private set; }

    /// <summary>The selected item while the reserve-tank category is selected.</summary>
    private PauseReserveItem SelectedReserveItem => Enum.IsDefined((PauseReserveItem)SelectedItem)
        ? (PauseReserveItem)SelectedItem
        : throw new InvalidDataException($"Unsupported native reserve subdispatcher {SelectedItem}.");

    /// <summary>Runs the tank subdispatcher before its D-pad response, as at $82:AC70.</summary>
    private void HandleReserveInput(SnesButton pressed)
    {
        switch (SelectedReserveItem)
        {
            case PauseReserveItem.Mode:
                if ((pressed & SnesButton.A) == 0 || samus.MaxReserveEnergy == 0) return;
                audio?.QueueSound(Library1Sound.MenuCursor, maximumQueued: 6);
                samus.ReserveTankMode = samus.ReserveTankMode == ReserveTankMode.Auto
                    ? ReserveTankMode.Manual : ReserveTankMode.Auto;
                WriteReserveLabels();
                return;
            case PauseReserveItem.Transfer:
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseReserveItem)} {SelectedItem}.");
        }

        if (ReserveTransferSoundDelay.IsZero)
        {
            if ((pressed & SnesButton.A) == 0) return;
            ReserveTransferSoundDelay = WrappingCountdown.FromRaw(unchecked((ushort)((samus.ReserveEnergy +
                PauseReserveTransferRomData.SoundCadenceMask) & ~PauseReserveTransferRomData.SoundCadenceMask)));
        }
        ReserveTransferSoundDelay = ReserveTransferSoundDelay.Decrement().Remaining;
        if ((ReserveTransferSoundDelay.RawValue & PauseReserveTransferRomData.SoundCadenceMask) ==
            PauseReserveTransferRomData.SoundCadenceMask)
            audio?.QueueSound(PauseReserveTransferRomData.RefillSound, PauseReserveTransferRomData.MaximumQueuedSounds);

        ushort amount = PauseEquipmentRules.ReserveEnergyPerFrame;
        samus.Health += amount;
        if (samus.Health >= samus.MaxHealth)
            samus.Health = samus.MaxHealth;
        else
        {
            // Reserves may undershoot zero by less than one transfer; the shortfall comes back off health.
            samus.ReserveEnergy -= amount;
            if (samus.ReserveEnergy > 0) return;
            if (samus.ReserveEnergy != 0)
                samus.Health += samus.ReserveEnergy;
        }

        // Retail exhausts ALL reserves even when only a small top-up was needed.
        // Preserve that behavior and clear both bytes of the packed selector.
        samus.ReserveEnergy = 0;
        ReserveTransferSoundDelay = WrappingCountdown.Start(0);
        SelectedCategory = PauseEquipmentCategory.Reserves;
        SelectedItem = PauseReserveTransferRomData.ModeItem;
    }
}
