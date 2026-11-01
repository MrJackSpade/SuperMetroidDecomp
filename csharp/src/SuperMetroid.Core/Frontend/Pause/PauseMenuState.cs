using System.Diagnostics.CodeAnalysis;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Frontend.FileSelect;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Pause.Artwork;
using SuperMetroid.Core.Map;
using SuperMetroid.Core.Hud;
using SuperMetroid.Core.Frontend.Menus;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Installed-asset owner of the pause map/equipment menu.
/// </summary>
/// <remarks>
/// This object deliberately owns only the pause-menu-local display image. The outer
/// <see cref="SuperMetroidGame"/> dispatcher retains the pause game states, while the
/// live <see cref="SamusState"/> remains the sole owner of inventory words. Consequently an
/// equipment-screen A press changes the same bits consumed by movement, rendering, and saving;
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
    private readonly AudioQueues? audio;
    /// <summary>Advances the map highlight colors once per accepted menu update.</summary>
    internal readonly MapPaletteAnimation paletteAnimation;
    /// <summary>Live inventory owner mutated by equipment and reserve controls.</summary>
    private readonly SamusState samus;
    /// <summary>Provides persistent area map and exploration bits.</summary>
    private readonly SharedGameState system;
    /// <summary>Area whose map and pause backdrop are displayed.</summary>
    internal readonly AreaId area;
    /// <summary>Room header map origin used to locate Samus on the area map.</summary>
    private readonly Position16<MapTiles> roomMapPosition;
    /// <summary>Visibility rule for the installed map and its scroll bounds.</summary>
    private readonly MapRevealMode mapRevealMode;
    /// <summary>Host-bound map artwork, rebound after debugger state restoration.</summary>
    [NonSerialized] private AreaMapPresentation? mapPresentation;
    /// <summary>The pause screen: its artwork, tilemaps (including the retained gameplay HUD rows), palette and sprite entries (#1358).</summary>
    internal readonly PauseScreen screen;
    /// <summary>
    /// The equipment page as the game's writes to its tilemap have left it (#1467): labels, overrun tail, wireframe,
    /// reserve display and arrow. The page shows it once uploaded.
    /// </summary>
    internal ShownEquipmentPage Equipment { get; private set; }
    /// <summary>The equipment page as last uploaded, which the equipment page shows; none before the first upload.</summary>
    internal ShownEquipmentPage? ShownEquipment { get; private set; }
    /// <summary>Which of the frame's button labels are lit (#1467); it survives page transitions and artwork rebinds.</summary>
    internal PauseButtonHighlight ButtonHighlight { get; private set; }
    /// <summary>How the map page shows the area map (#1466, #1467), as last loaded.</summary>
    internal ShownAreaMap ShownMap { get; private set; }
    /// <summary>Current map/equipment fade or load phase.</summary>
    internal PauseMenuTransition Transition { get; private set; }
    /// <summary>Counter inserted between brightness changes during fade-in.</summary>
    private int transitionFadeCounter;
    /// <summary>Current page-transition brightness, from black to full intensity.</summary>
    internal int TransitionBrightness { get; private set; } = 15;
    /// <summary>Equipment or reserve category selected by the cursor.</summary>
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
    /// <summary>Current map-position marker frame.</summary>
    internal int MapIndicatorAnimationFrame { get; set; }
    /// <summary>Remaining updates in the map-position marker frame.</summary>
    internal int MapIndicatorAnimationTimer { get; set; }
    /// <summary>Current equipment selector sprite frame.</summary>
    internal int ItemSelectorAnimationFrame { get; set; }
    /// <summary>Remaining updates in the equipment selector frame.</summary>
    internal int ItemSelectorAnimationTimer { get; set; }
    /// <summary>Last selected map marker or equipment selector origin for diagnostics.</summary>
    internal Position16<SpritePixels> LastIndicatorOrigin { get; private set; }
    /// <summary>Pose of the Samus map marker when it was the last indicator drawn, for diagnostics; null after a selector draw.</summary>
    internal MapSpriteId? LastMapIndicatorSprite { get; private set; }
    /// <summary>Category of the equipment selector when it was the last indicator drawn, for diagnostics; null after a map-marker draw.</summary>
    internal PauseEquipmentCategory? LastSelectorCategory { get; private set; }

    /// <summary>Creates the isolated pause display image and initial map/equipment state from live gameplay.</summary>
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
        if (entry.GameplayHud is not null)
        {
            // Pause setup moves the HUD layer's tilemap but never loads a replacement
            // tilemap. The four-row gameplay HUD already resident there is
            // intentionally retained. A fresh host-side display object used to discard that
            // state, removing the entire energy/ammo HUD from pause and exposing map BG1
            // pixels in the area that its BG3 plane normally covers. Pause keeps the HUD's
            // images gameplay displayed (#1351, #1466) and draws them under its palette.
            screen.RetainHud(entry.GameplayHud);
        }

        // GameState_13 finishes pause setup by calling QueueClearingOfFxTilemap at
        // the end of setup, which blanks every BG3 row beneath the four HUD rows. Pause draws
        // only the HUD's own layer (PauseScreen.RetainedHudPictures), so every otherwise-empty map
        // cell shows nothing rather than pause character zero, the visible orange `1` glyph.
        entry.MapPresentation.Palettes.Pause.LoadTo(screen.Palette);

        // LoadPauseScreenBaseTilemaps shows the button page's two label rows over the backdrop's bottom rows, its labels
        // lit or dimmed by the highlight; the frame is the area's backdrop picture, the button rows' picture and each
        // label's picture in its shade (#1467).
        SetPauseButtonLabelMode(PauseButtonHighlight.MapPage);

        // LoadEquipmentScreen copies the template, then writes every label, the wireframe and the reserve display.
        Equipment = BuiltEquipmentPage();
        LoadPauseMapTilemap();
        SelectFirstCollectedEquipment();
        SetupMapScrolling();
    }

    /// <summary>Zero for the map and one for the equipment page.</summary>
    public PauseScreenMode ScreenMode { get; private set; }

    /// <summary>
    /// Runs the pause menu's input after the caller has latched the update's input and applied the
    /// delayed-held filter. Returns true when Start requests unpausing.
    /// The runtime supplies its short frame counter for the colour animation's phase;
    /// standalone diagnostic callers may leave that phase at zero.
    /// </summary>
    public bool Step(ControllerButton delayedHeldInput, ControllerButton newlyPressedInput, ControllerButton? heldInput = null, int frameCounterByte = 0)
    {
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations(frameCounterByte);
        if (ScreenMode == PauseScreenMode.Map && Transition == PauseMenuTransition.None)
            MapArrows.StepArrows(direction => MapScrollBounds.CanScroll(direction, MapScrollPosition));
        ControllerButton delayedPressed = delayedHeldInput;
        ControllerButton newlyPressed = newlyPressedInput;
        if (Transition != PauseMenuTransition.None)
        {
            StepPageTransition();
            return false;
        }

        if (ScreenMode == PauseScreenMode.Map)
        {
            // The map page runs L/R, then Start, then map scrolling. A Start press does not stop
            // the same frame's scroll pulse.
            if ((delayedPressed & ControllerButton.R) != ControllerButton.None)
            {
                audio?.QueueSound(Library1Sound.MenuConfirm, priority: SoundRequestPriority.Normal);
                SetPauseButtonLabelMode(PauseButtonHighlight.EquipmentPage);
                Transition = PauseMenuTransition.MapToEquipmentFadeOut;
                TransitionBrightness = 15;
            }
            bool mapStart = HandleStartButton(delayedPressed);
            // Map arrows read the ordinary held controller word, not the delayed chrome
            // input. An accepted pulse keeps running after release, as in the game.
            if (MapScrollBounds.Step(heldInput ?? delayedHeldInput, ref mapScrollPosition))
                audio?.QueueSound(Library1Sound.MapScroll, priority: SoundRequestPriority.Normal);
            return mapStart;
        }

        // The equipment page runs EquipmentScreenMain, then L/R, then Start, so an A press in the
        // frame that unpauses still toggles the selected item. EquipmentScreenMain consumes
        // the ordinary rising-edge word for D-pad/A; only the shared L/R/Start chrome
        // uses the delayed-held word.
        HandleEquipmentInput(newlyPressed, frameCounterByte);
        // EquipmentScreenMain refreshes the reserve amount independently of selected
        // label edits. Keep this per-frame write without rebuilding the whole tilemap,
        // which would erase the game's same-frame VAR overrun.
        WriteReserveSupplyDigits();
        UploadEquipmentTilemap();
        if ((delayedPressed & ControllerButton.L) != ControllerButton.None)
        {
            audio?.QueueSound(Library1Sound.MenuConfirm, priority: SoundRequestPriority.Normal);
            SetPauseButtonLabelMode(PauseButtonHighlight.MapPage);
            Transition = PauseMenuTransition.EquipmentToMapFadeOut;
            TransitionBrightness = 15;
        }
        return HandleStartButton(delayedPressed);
    }

    /// <summary>
    /// <c>Handle_PauseScreen_StartButton</c>, run last on both stable
    /// pages. It reads the delayed-held word: a fresh press is excluded for three frames by
    /// the controller's delayed-held filter before becoming menu input. True requests the unpause state.
    /// </summary>
    private bool HandleStartButton(ControllerButton delayedPressed)
    {
        if ((delayedPressed & ControllerButton.Start) == ControllerButton.None)
            return false;
        audio?.QueueSound(Library1Sound.MenuConfirm, priority: SoundRequestPriority.Normal);
        SetPauseButtonLabelMode(PauseButtonHighlight.Unpausing);
        return true;
    }

    /// <summary>
    /// Advances the sprite animation drawn by the current pause-menu substate.
    /// </summary>
    /// <remarks>
    /// Rendering itself is intentionally pure. The game advances these counters from its
    /// draw routines, but the desktop may render a framebuffer more than once while paused
    /// (for example, a debugger watch or PNG capture). The dispatcher calls this exactly
    /// once per frame so inspection cannot change game-visible timing.
    /// </remarks>
    public void AdvanceAnimations(int frameCounterByte = 0, PauseAnimationPhase phase = PauseAnimationPhase.Stable)
    {
        // The fade-out paths emit destination labels before map icons.
        mapLabelsBeforeIcons = phase == PauseAnimationPhase.FadingOut || Transition == PauseMenuTransition.MapToEquipmentFadeOut;
        // Only stable map dispatch handles the scroll arrows. Fade paths retain counters but hide arrows.
        CurrentMapArrows?.StepArrows(_ => false);
        pauseFrameCounterByte = frameCounterByte;
        // Stable pause dispatch calls the palette handler afterwards. The
        // outer pause/unpause fades draw sprites without advancing this owner.
        if (phase == PauseAnimationPhase.Stable && paletteAnimation.Step(screen.Palette))
            audio?.QueueSound(Library3Sound.PauseBeep, priority: SoundRequestPriority.Normal);
        if (ScreenMode == PauseScreenMode.Map)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Prepares the sprite list on the simulation owner, then captures without raster work.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // BG fetches start on physical scanline one; OBJ coordinates already use
        // output-space Y. Keep the game's scroll and sprite origins unchanged.
        Position16<TilePlanePixels> mapScroll = new(
            ScreenMode == PauseScreenMode.Map ? MapScrollPosition.X : 0,
            CoordinateAxis.Wrap((ScreenMode == PauseScreenMode.Map ? MapScrollPosition.Y : 0) + ScreenGeometry.BackgroundRowOffset));
        // The map page is a grid of the area map's cell pictures, the equipment page the pictures of what it last uploaded
        // and the frame the area's backdrop and button pictures (#1467).
        DisplayLayer bg1 = ScreenMode == PauseScreenMode.Map
            ? new PictureGridLayer(AreaMapPictures.Grid(ShownMap, screen.Pictures), mapScroll, false)
            : new PicturedPlaneLayer(screen.EquipmentPage(ShownEquipment ?? throw new InvalidOperationException(
                "The equipment page shows before it was uploaded.")), mapScroll, false);
        var bg2 = new PicturedPlaneLayer(screen.Frame(area, ButtonHighlight), new Position16<TilePlanePixels>(0, ScreenGeometry.BackgroundRowOffset), false);
        // The retained HUD is its own layer at the HUD's place, drawn from its images in their priority tiers (#1466).
        var hud = new HudLayer(screen.RetainedHudPictures(), TransparentColorZero: true, Priority: false);
        DisplayLayer[] layers =
        [
            new SpriteDepthLayer(0), hud,
            new SpriteDepthLayer(1), bg2, bg1,
            new SpriteDepthLayer(2), bg2 with { Priority = true }, InFront(bg1),
            new SpriteDepthLayer(3), hud with { Priority = true },
        ];
        return new(screen.Paint(), layers, Transition == PauseMenuTransition.None ? 15 : TransitionBrightness);
    }

    /// <summary><paramref name="layer"/>, the map page's grid or the equipment page, drawing only its front-tier pixels.</summary>
    private static DisplayLayer InFront(DisplayLayer layer) => layer switch
    {
        PictureGridLayer grid => grid with { Priority = true },
        PicturedPlaneLayer page => page with { Priority = true },
        _ => throw new InvalidOperationException($"The pause page is no {layer.GetType().Name}."),
    };

    /// <summary>Assembles map markers or equipment sprites in their page-specific order.</summary>
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
            // Pause shares the boss lists, defeated overlays and downloaded
            // destination lettering with file select. Preserve their per-phase drawing order.
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

                // The fade-out returns at black before the separate load dispatch.
                Transition = Transition == PauseMenuTransition.MapToEquipmentFadeOut
                    ? PauseMenuTransition.MapToEquipmentLoad : PauseMenuTransition.EquipmentToMapLoad;
                return;

            case PauseMenuTransition.MapToEquipmentLoad:
            case PauseMenuTransition.EquipmentToMapLoad:
                if (Transition == PauseMenuTransition.MapToEquipmentLoad)
                {
                    ScreenMode = PauseScreenMode.Equipment;
                    // The selector resets on each equipment-page entry.
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
                // Load dispatches set delay/counter to one; the fade-in spends a
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
        // The game selects the tank mode control whenever reserve capacity exists,
        // including empty tanks. The first beam is only the no-reserve fallback.
        if (samus.MaxReserveEnergy != 0)
        {
            SelectedCategory = PauseEquipmentCategory.Reserves;
            SelectedItem = PauseReserveEnergyDefinitions.ModeItem;
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

    /// <summary>
    /// The equipment page built from its template and the current inventory: every label in its inventory state, no overrun
    /// tail, the equipped wireframe, and the reserve labels and supply digits once reserve capacity exists.
    /// </summary>
    private ShownEquipmentPage BuiltEquipmentPage() => ShownEquipmentPage.Built(
        new PauseInventory(samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems, samus.EquippedItems, samus.HyperBeamEnabled),
        samus.MaxReserveEnergy == 0 ? null : new ShownReserveDisplay(samus.ReserveTankMode, samus.ReserveEnergy));

    /// <summary>
    /// Translates <c>Load_EquipmentScreen_ReserveHealth_Tilemap</c>.
    /// The game displays current reserve supply, not maximum normal Energy Tank
    /// capacity. It leaves the authored template untouched until at least one Reserve Tank
    /// exists, then writes hundreds, tens, and ones as three consecutive BG1 words.
    /// </summary>
    private void WriteReserveSupplyDigits()
    {
        if (samus.MaxReserveEnergy == 0)
            return;
        Equipment = Equipment.WithReserveSupply(samus.ReserveEnergy);
    }

    /// <summary>Writes the pause wireframe for Samus's equipped suit and boots over the page.</summary>
    private void WriteSamusWireframe() => Equipment = Equipment.WithWireframe(PauseEquipmentRules.WireframeKind(samus.EquippedItems));

    /// <summary>Builds the area map from installed presentation and exploration bits into BG1.</summary>
    [MemberNotNull(nameof(ShownMap))]
    private void LoadPauseMapTilemap()
    {
        AreaMapPresentation maps = mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.");
        ShownMap = AreaMapProjection.Project(maps.Get(area), system, maps.HiddenCells.Unrevealed, mapRevealMode);

        // Installed backdrops already contain their authored area lettering.
    }

    /// <summary>Derives map-scroll bounds and initial offset from visible tiles and Samus's map cell.</summary>
    private void SetupMapScrolling()
    {
        // DetermineMapScrollLimits scans either the downloaded map
        // or the persistent explored plane. Expressing the scan in coordinates is exactly
        // equivalent to its byte/bit loops and makes the two-page 64x32 layout explicit.
        bool hasAreaMap = system.HasAreaMap(area);
        IAreaMapView map = (mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.")).Get(area);
        bool IsVisible(int x, int y)
        {
            bool explored = system.IsMapTileExplored(area, new(x, y));
            // The game's scroll bounds select one bit plane, unlike tile rendering, which
            // also displays explored secrets. Unioning the planes moves the map's center
            // after a secret is explored even though the downloaded bounds are unchanged.
            if (mapRevealMode == MapRevealMode.None)
                return hasAreaMap ? map.IsRevealedByMapStation(new(x, y)) : explored;
            return AreaMapVisibility.IsVisible(
                explored,
                hasAreaMap,
                map.IsRevealedByMapStation(new(x, y)),
                map.IsDiscoverable(new(x, y)),
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
        // The game's vertical scans return their defaults before checking the final row
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

        // Every map word below keeps its sixteen-bit wrap where it is computed; the signed
        // tests read those words as the game's branches do.
        int minimumX = WordWrap.Unsigned(left * 8 - (area == AreaId.Maridia ? 24 : 0));
        int maximumX = WordWrap.Unsigned(right * 8);
        int minimumY = WordWrap.Unsigned(top * 8);
        int maximumY = WordWrap.Unsigned(bottom * 8);
        CurrentMapScrollBounds = new PauseMapScroll(new(CoordinateAxis.Wrap(minimumX), CoordinateAxis.Wrap(minimumY)), new(CoordinateAxis.Wrap(maximumX), CoordinateAxis.Wrap(maximumY)));

        // The pause map's scroll setup uses wrapping 16-bit arithmetic throughout. These local
        // helpers wrap before every signed test so an edge-of-map position behaves as the
        // game's does rather than like unbounded integer arithmetic.
        // The player's map cell: the room's map origin plus Samus's room screen, with the
        // map convention's one-row downward offset.
        Position16<MapPixels> player = SamusMapCell().ToMapPixels();
        int horizontal = CoordinateAxis.Wrap(
            minimumX + CoordinateAxis.Wrap(maximumX - minimumX) / 2 - 128);
        int horizontalScreenPosition = CoordinateAxis.Wrap(player.X - horizontal);
        int distanceFromRightClamp = CoordinateAxis.Difference(224, horizontalScreenPosition);
        if (distanceFromRightClamp >= 0)
        {
            int distanceFromLeftClamp = CoordinateAxis.Wrap(32 - horizontalScreenPosition);
            if (CoordinateAxis.AsSigned(distanceFromLeftClamp) >= 0)
                horizontal = CoordinateAxis.Wrap(horizontal - distanceFromLeftClamp);
        }
        else
        {
            horizontal = CoordinateAxis.Wrap(horizontal - distanceFromRightClamp);
        }

        int verticalMiddle = CoordinateAxis.Wrap(
            minimumY + CoordinateAxis.Wrap(maximumY - minimumY) / 2 + 16);
        int verticalCenterOffset = WordWrap.Unsigned((128 - verticalMiddle) & ~7);
        int vertical = CoordinateAxis.Wrap(-verticalCenterOffset);
        int playerMapY = CoordinateAxis.Wrap(player.Y + verticalCenterOffset);
        int distanceFromTopClamp = WordWrap.Signed(64 - playerMapY);
        if (distanceFromTopClamp >= 0)
        {
            vertical = CoordinateAxis.Wrap(vertical - distanceFromTopClamp);
            if (WordWrap.Signed(vertical + 40) < 0)
                vertical = WordWrap.Unsigned(-40);
        }
        MapScrollPosition = new(WordWrap.Unsigned(horizontal), WordWrap.Unsigned(vertical));
    }

    /// <summary>Maps Samus's current room screen to the area map with its one-row Y convention.</summary>
    private Position16<MapTiles> SamusMapCell() =>
        roomMapPosition + new Offset16<MapTiles>(EightEight.WholeOf(samus.Position.X.Whole), (EightEight.WholeOf(samus.Position.Y.Whole)) + 1);

    /// <summary>Draws the animated Samus map marker at the scrolled screen position.</summary>
    private void DrawMapPositionIndicator()
    {
        Position16<SpritePixels> origin = SamusMapCell().ToMapPixels().ToScreen(MapScrollPosition).ToSpritePosition();
        LastIndicatorOrigin = origin;
        MapSpriteId indicator = PauseMapIndicatorAnimation.SpritemapId(MapIndicatorAnimationFrame);
        LastMapIndicatorSprite = indicator;
        LastSelectorCategory = null;
        DrawMenuSpritemap(
            indicator,
            origin,
            ScreenColourGroup.MapMarkers);
    }

    /// <summary>Advances the map marker's timer-before-draw frame cadence.</summary>
    private void StepMapIndicatorAnimation()
    {
        // The timer starts at zero, advances to frame one on the first draw, then
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
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == SamusEquipmentFlags.None && samus.CollectedBeams == SamusBeamFlags.None)
            return;

        // DrawPauseScreenSpriteAnim(3) selects the third timer/frame pair. Its animation
        // list holds three-byte entries (delay, unused, ID offset).
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
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == SamusEquipmentFlags.None && samus.CollectedBeams == SamusBeamFlags.None)
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

    /// <summary>
    /// Runs the three SetPauseScreenButtonLabelPalettes variants and the redraw of the two label rows
    /// that follows each: the frame shows its labels in <paramref name="highlight"/>'s shades from the next capture.
    /// </summary>
    private void SetPauseButtonLabelMode(PauseButtonHighlight highlight) =>
        ButtonHighlight = Enum.IsDefined(highlight) ? highlight : throw new ArgumentOutOfRangeException(nameof(highlight), highlight, "Undefined pause highlight.");

    /// <summary>Draws a map sprite sheet frame in the map's colours <paramref name="colouring"/>.</summary>
    private void DrawMenuSpritemap(MapSpriteId id, Position16<SpritePixels> origin, ScreenColourGroup colouring) =>
        // Map compositions use this shared entry. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        MapSpriteSheet.Draw(MapSheet, id, screen.Objects, origin, colouring);

    /// <summary>Uploads the equipment page as its writes have left it: the equipment page shows it from the next capture.</summary>
    private void UploadEquipmentTilemap() => ShownEquipment = Equipment;

    /// <summary>Gets the inventory bit represented by an item in an equipment category.</summary>
    private static PauseEquipmentBit ReadCategoryMask(PauseEquipmentCategoryDefinition category, int item) =>
        PauseEquipmentRules.Mask(category.Category, item);

    /// <summary>Dispatches reserve input or moves and toggles equipment in the game's update order.</summary>
    private void HandleEquipmentInput(ControllerButton pressed, int frameCounterByte)
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
        if ((pressed & ControllerButton.A) == ControllerButton.None)
            return;
        if (dispatchedCategory == PauseEquipmentCategory.Beams && samus.CollectedBeams == SamusBeamFlags.None)
            return;
        // Moving into the reserve rows on the frame A is pressed toggles with the reserve category, whose equipment and
        // label tables hold no entries: the game takes the scratch words it has just written as the equipment word, its
        // mask and the label to copy, and writes to wherever they then point in work memory. What it overwrites depends
        // on scratch left by unrelated code, which only full memory emulation could reproduce, so the toggle is
        // rejected under the emulation scope limit (#1500).
        if (SelectedCategory == PauseEquipmentCategory.Reserves)
            throw new NotSupportedException(
                "A same-frame equipment toggle into the reserve rows writes work memory through stale scratch words; " +
                "reproducing it needs full memory emulation, which is outside the port's scope.");

        // Dispatch precedes movement. Boots->Plasma therefore retains the Boots copy
        // length and bypasses the Weapons handler's Spazer/Plasma exclusion. Do not use
        // upstream C's 'Fixed var bug' category guard: it is absent from the retail game.
        SamusBeamFlags previousBeams = samus.EquippedBeams;
        var target = PauseEquipmentCategories.Get(SelectedCategory);
        PauseEquipmentBit mask = ReadCategoryMask(target, SelectedItem);
        bool wasEquipped = mask.IsSetIn(samus.EquippedBeams, samus.EquippedItems);
        // The bit belongs to its own category's set, so toggling both sets toggles exactly that upgrade.
        samus.EquippedBeams ^= mask.Beam;
        samus.EquippedItems ^= mask.Item;

        audio?.QueueSound(Library1Sound.MenuConfirm, priority: SoundRequestPriority.Normal);
        UpdateEquipmentLabel(SelectedCategory, SelectedItem,
            PauseEquipmentCategories.Get(dispatchedCategory).LabelWordCount, wasEquipped);
        if (dispatchedCategory == PauseEquipmentCategory.Beams)
        {
            SamusBeamFlags added = samus.EquippedBeams & ~previousBeams;
            if ((added & SamusBeamFlags.Spazer) != SamusBeamFlags.None &&
                (samus.EquippedBeams & SamusBeamFlags.Plasma) != SamusBeamFlags.None)
            {
                samus.EquippedBeams &= ~SamusBeamFlags.Plasma;
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.PlasmaItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
            else if ((added & SamusBeamFlags.Plasma) != SamusBeamFlags.None &&
                (samus.EquippedBeams & SamusBeamFlags.Spazer) != SamusBeamFlags.None)
            {
                samus.EquippedBeams &= ~SamusBeamFlags.Spazer;
                UpdateEquipmentLabel(PauseEquipmentCategory.Beams, PauseEquipmentCategories.SpazerItem,
                    PauseEquipmentCategories.Get(PauseEquipmentCategory.Beams).LabelWordCount, true);
            }
        }
        // The game patches only the chosen label, then refreshes the wireframe. Rebuilding
        // all labels would erase the intentional overlong copy that displays VAR.
        WriteSamusWireframe();
        UploadEquipmentTilemap();
    }

    /// <summary>
    /// Writes one equipment label: <paramref name="wordCount"/> cells, the dispatched category's label width, so the retail
    /// overlong Plasma copy and a beam column's five-cell write over a nine-cell label are written as the game writes them.
    /// </summary>
    private void UpdateEquipmentLabel(PauseEquipmentCategory categoryIndex, int item, int wordCount, bool disabled) =>
        Equipment = Equipment.WithLabelWritten(PauseEquipmentLabelTiles.Label(categoryIndex, item), wordCount,
            disabled ? PauseEquipmentLabelState.Disabled : PauseEquipmentLabelState.Enabled);

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
                // The suit scan checks byte offset ten only AFTER an unsuccessful probe.
                // Thus directly entering the final suit item can succeed, but scanning
                // forward from an earlier missing item must not discover it.
                if (categoryIndex == PauseEquipmentCategory.Suits && step > 0 &&
                    item + step >= category.ItemCount - 1)
                    return false;
                continue;
            }
            SelectedCategory = categoryIndex;
            SelectedItem = item;
            audio?.QueueSound(Library1Sound.MenuCursor, priority: SoundRequestPriority.Normal);
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
        audio?.QueueSound(Library1Sound.MenuCursor, priority: SoundRequestPriority.Normal);
        return true;
    }

    /// <summary>Applies the directional links among beam, suit, boot, and reserve rows.</summary>
    private void MoveEquipmentSelector(ControllerButton pressed)
    {
        bool left = (pressed & ControllerButton.Left) != ControllerButton.None, right = (pressed & ControllerButton.Right) != ControllerButton.None;
        bool up = (pressed & ControllerButton.Up) != ControllerButton.None, down = (pressed & ControllerButton.Down) != ControllerButton.None;
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
                        audio?.QueueSound(Library1Sound.MenuCursor, priority: SoundRequestPriority.Normal);
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
                        else audio?.QueueSound(Library1Sound.MenuCursor, priority: SoundRequestPriority.Normal);
                    }
                }
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseEquipmentCategory)} {SelectedCategory}.");
        }
    }

    /// <summary>Retains only the HUD gameplay last displayed, not one still being written.</summary>
    /// <param name="gameplayHud">The HUD gameplay latched at that frame (#1351, #1466).</param>
    public void SynchronizeAcceptedHud(ShownHud gameplayHud)
    {
        // Pause owns its own screen. Mirror the HUD's drawn rows, leaving its static
        // first row and deliberately cleared FX rows untouched.
        screen.MirrorAcceptedHud(gameplayHud);
    }

    /// <summary>Rebinds edited map and pause artwork while retaining live scroll, palette, and label state.</summary>
    public void BindMapPresentation(AreaMapPresentation? catalog)
    {
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Pause menu requires installed map presentation assets.");
        CurrentMapArrows ??= new FileSelectMapAnimations(catalog.Arrows);
        CurrentMapArrows.BindPresentation(catalog.Arrows);
        paletteAnimation.Bind(catalog.HighlightCycle);
        screen.BindArtwork(catalog);
        // Live highlights and reserve-arrow colors belong to their animation
        // owners. Replacing the static base must not reset their phase.
        catalog.Palettes.Pause.LoadToExcept(screen.Palette, screen.Palette.Colours(ScreenColourGroup.MapHighlight),
            screen.Palette.Colours(ScreenColourGroup.PauseReserveArrowShade), screen.Palette.Colours(ScreenColourGroup.PauseReserveArrowLight));
        // The equipment page is typed state the rebound sheets' pictures show again (#1467): its labels, overrun tail,
        // wireframe and reserve display keep exactly what the game's writes left, and nothing is rewritten.
        if (samus.MaxReserveEnergy != 0)
        {
            // A rebind is not a tank dispatch: re-apply the owner's latched arrow state with the
            // new palettes. Only a bind before anything has latched asks the owner.
            if (reserveArrowLatched)
                ReapplyLatchedReserveArrow();
            else if (SelectedCategory == PauseEquipmentCategory.Reserves)
                UpdateReserveArrow(pauseFrameCounterByte);
        }
        if (ScreenMode != PauseScreenMode.Map) UploadEquipmentTilemap();
        // The live button highlight, including a Start/fade highlight, is state the frame's pictures show again; do not
        // rerun controls or rebuild equipment: that would erase the game's same-frame label overruns.
        // Preserve the serialized scroll position and transition timing. Only refresh
        // BG1 when it currently contains the map; the equipment page shares that layer.
        if (ScreenMode == PauseScreenMode.Map) LoadPauseMapTilemap();
    }

    /// <summary>Whether reserve-arrow ownership has published a state to retain across artwork rebinds.</summary>
    private bool reserveArrowLatched;
    /// <summary>Last requested reserve-arrow state.</summary>
    private ReserveArrowState reserveArrowLatchedState;
    /// <summary>Eight-bit frame phase (zero through 255) retained for reproducing the arrow's current color on rebind.</summary>
    private int reserveArrowLatchedFrame;

    /// <summary>Runs the reserve arrow update after the tank input handler and D-pad response.</summary>
    private void UpdateReserveArrow(int frameCounterByte)
    {
        bool inTanks = SelectedCategory == PauseEquipmentCategory.Reserves;
        ReserveArrowState state =
            inTanks && SelectedReserveItem == PauseReserveItem.Mode && samus.ReserveTankMode == ReserveTankMode.Auto ? ReserveArrowState.Animated
            : inTanks && SelectedReserveItem == PauseReserveItem.Transfer ? ReserveArrowState.Solid
            : ReserveArrowState.Hidden;
        SetReserveArrow(state, frameCounterByte);
    }

    /// <summary>Applies reserve-arrow tile colouring and palette state, then latches it for content rebinds.</summary>
    private void SetReserveArrow(ReserveArrowState state, int frameCounterByte = 0)
    {
        PauseReserveArrowColours presentation = (mapPresentation ?? throw new InvalidOperationException(
            "Reserve arrow requires installed presentation assets.")).PauseReserveArrow;
        presentation.ApplyArrowColors(screen.Palette,
            state == ReserveArrowState.Animated, frameCounterByte);
        Equipment = Equipment.WithArrow(state != ReserveArrowState.Hidden ? ShownReserveArrow.Lit : ShownReserveArrow.Dim);
        reserveArrowLatched = true;
        reserveArrowLatchedState = state;
        reserveArrowLatchedFrame = frameCounterByte;
    }

    /// <summary>Re-applies the latched arrow tiles and colors with the currently bound presentation.</summary>
    private void ReapplyLatchedReserveArrow() =>
        SetReserveArrow(reserveArrowLatchedState, reserveArrowLatchedFrame);

    /// <summary>
    /// Rewrites the reserve mode and tank labels for the current mode, only when reserve capacity exists. Setup keeps
    /// MODE's initial MANUAL text for the zero mode and replaces its four characters once a mode is set.
    /// </summary>
    private void WriteReserveLabels()
    {
        if (samus.MaxReserveEnergy == 0) return;
        Equipment = Equipment.WithReserveMode(samus.ReserveTankMode);
    }

    /// <summary>Accepted eight-bit frame phase (zero through 255) retained so repeated renders cannot advance tank fill flicker.</summary>
    private int pauseFrameCounterByte;

    /// <summary>Draws the reserve strip's full/partial/empty tanks and final cap.</summary>
    private void DrawReserveTanks()
    {
        if (samus.MaxReserveEnergy == 0) return;
        int tank = 0;
        int full = samus.ReserveEnergy / PauseReserveEnergyDefinitions.EnergyPerTank;
        int remainder = samus.ReserveEnergy % PauseReserveEnergyDefinitions.EnergyPerTank;
        for (; tank < full; tank++) Draw(PauseReserveTankVisual.Full, tank);
        if (remainder != 0)
        {
            // A low partial tank flickers its next, partly filled step on and off.
            int steps = remainder / PauseReserveEnergyDefinitions.EnergyPerFillStep;
            if (steps < PauseReserveEnergyDefinitions.FlickerFillStepLimit &&
                remainder % PauseReserveEnergyDefinitions.EnergyPerFillStep != 0 &&
                pauseFrameCounterByte / PauseReserveEnergyDefinitions.FlickerPhaseFrames % 2 == 0)
                steps++;
            Draw(PauseReserveTankParts.FillLevel(steps), tank++);
        }
        for (; tank < samus.MaxReserveEnergy / PauseReserveEnergyDefinitions.EnergyPerTank; tank++)
            Draw(PauseReserveTankVisual.Empty, tank);
        Draw(PauseReserveTankVisual.EndCap, tank);

        void Draw(PauseReserveTankVisual map, int index)
        {
            (mapPresentation ?? throw new InvalidOperationException(
                "Reserve tanks require installed presentation assets."))
                .PauseReserveTanks.Draw(screen.Objects, PauseSheet, map, index);
        }
    }

    /// <summary>Reserve-transfer sound cadence, advanced only by the selected reserve-transfer handler.</summary>
    internal WrappingCountdown ReserveTransferSoundDelay { get; private set; }

    /// <summary>The selected item while the reserve-tank category is selected.</summary>
    private PauseReserveItem SelectedReserveItem => (uint)SelectedItem < (uint)PauseReserveEnergyDefinitions.Items.Length
        ? PauseReserveEnergyDefinitions.Items[SelectedItem]
        : throw new InvalidDataException($"Unsupported reserve subdispatcher {SelectedItem}.");

    /// <summary>Runs the tank subdispatcher before its D-pad response.</summary>
    private void HandleReserveInput(ControllerButton pressed)
    {
        switch (SelectedReserveItem)
        {
            case PauseReserveItem.Mode:
                if ((pressed & ControllerButton.A) == ControllerButton.None || samus.MaxReserveEnergy == 0) return;
                audio?.QueueSound(Library1Sound.MenuCursor, priority: SoundRequestPriority.Normal);
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
            if ((pressed & ControllerButton.A) == ControllerButton.None) return;
            ReserveTransferSoundDelay = WrappingCountdown.Start((samus.ReserveEnergy +
                PauseReserveEnergyDefinitions.RefillSoundInterval - 1) & -PauseReserveEnergyDefinitions.RefillSoundInterval);
        }
        ReserveTransferSoundDelay = ReserveTransferSoundDelay.Decrement().Remaining;
        if (ReserveTransferSoundDelay.CyclePhase(1, PauseReserveEnergyDefinitions.RefillSoundInterval) ==
            PauseReserveEnergyDefinitions.RefillSoundInterval - 1)
            audio?.QueueSound(PauseReserveEnergyDefinitions.RefillSound, PauseReserveEnergyDefinitions.MaximumQueuedSounds);

        int amount = PauseEquipmentRules.ReserveEnergyPerFrame;
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
        SelectedItem = PauseReserveEnergyDefinitions.ModeItem;
    }
}
