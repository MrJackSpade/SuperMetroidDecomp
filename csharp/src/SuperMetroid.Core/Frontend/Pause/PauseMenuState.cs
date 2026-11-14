using System.Collections.Immutable;
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
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Frontend.Pause;

/// <summary>
/// Installed-asset owner of the pause map/equipment menu.
/// </summary>
/// <remarks>
/// This object deliberately owns only the pause-menu-local display image. The outer
/// <see cref="SuperMetroidGame"/> game-state loop retains the pause game states, while the
/// live <see cref="SamusState"/> remains the sole owner of inventory. Consequently an
/// equipment-screen A press changes the same flags consumed by movement, rendering, and saving;
/// there is no separate copy that can diverge from gameplay.
/// </remarks>
internal sealed class PauseMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    [MemberNotNull(nameof(spriteSheets))]
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
    {
        spriteSheets = sheets ?? throw new InvalidOperationException("Pause menu requires the installed sprite sheets.");
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the map's sprites draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>The map sprite sheet the map's markers, labels, arrows and position indicator draw from.</summary>
    private SpriteSheet MapSheet => spriteSheets.Get(SpriteSheetId.MapSprites);

    /// <summary>The pause sheet the equipment selectors and reserve-tank strip draw from.</summary>
    private SpriteSheet PauseSheet => spriteSheets.Get(SpriteSheetId.PauseSprites);


    /// <summary>Queues menu navigation and animation sounds when a live frontend owns audio.</summary>
    private readonly AudioQueues? audio;
    /// <summary>The map's highlight cycle, advanced once per accepted stable menu update.</summary>
    internal readonly MapHighlightAnimation mapHighlight = new();
    /// <summary>Live inventory owner mutated by equipment and reserve controls.</summary>
    private readonly SamusState samus;
    /// <summary>Provides persistent area map and exploration bits.</summary>
    private readonly SharedGameState system;
    /// <summary>Area whose map and pause backdrop are displayed.</summary>
    internal readonly AreaId area;
    /// <summary>Room definition map origin used to locate Samus on the area map.</summary>
    private readonly Position16<MapTiles> roomMapPosition;
    /// <summary>Visibility rule for the installed map and its scroll bounds.</summary>
    private readonly MapRevealMode mapRevealMode;
    /// <summary>Installed map artwork, bound when the game is bound to its installed content.</summary>
    private AreaMapPresentation? mapPresentation;
    /// <summary>The pause screen: its artwork, pictures (including the retained gameplay HUD rows), palette and sprite entries (#1358).</summary>
    internal readonly PauseScreen screen;
    /// <summary>
    /// What the equipment page shows this frame (#1467, #1515): each label in its item's state now, the wireframe of the
    /// equipped suit and boots, the reserve labels and digits once Samus has reserve capacity, and the arrow as the cursor
    /// and the reserve mode light it now (#1597).
    /// </summary>
    internal ShownEquipmentPage EquipmentPage => ShownEquipmentPage.Of(
        new PauseInventory(samus.CollectedBeams, samus.EquippedBeams, samus.CollectedItems, samus.EquippedItems, samus.HyperBeamEnabled),
        samus.MaxReserveEnergy == 0 ? null : new ShownReserveDisplay(samus.ReserveTankMode, samus.ReserveEnergy),
        samus.MaxReserveEnergy == 0 ? ShownReserveArrow.NoReserves
            : ReserveArrow != ReserveArrowState.Hidden ? ShownReserveArrow.Lit : ShownReserveArrow.Dim);
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
    /// <summary>The equipment control the cursor rests on (#1772); its category is the column the cursor is in.</summary>
    internal PauseEquipmentControl SelectedControl { get; private set; } = PauseEquipmentControls.ReserveMode;
    /// <summary>Current map scroll in map pixels.</summary>
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

    /// <summary>Creates the isolated pause display image and initial map/equipment state from live gameplay.</summary>
    public PauseMenuState(PauseMenuEntry entry)
    {

        this.mapPresentation = entry.MapPresentation ?? throw new InvalidOperationException(
            "Pause menu requires installed map and equipment presentation assets.");

        this.samus = Ensure.NotNull(entry.Samus);
        this.system = Ensure.NotNull(entry.System);
        this.audio = entry.Audio;
        CurrentMapArrows = new FileSelectMapAnimations(entry.MapPresentation.Arrows);
        mapHighlight.Bind(entry.MapPresentation.HighlightCycle);
        area = entry.Area;
        _ = AreaIds.Require(entry.Area);
        this.roomMapPosition = entry.RoomMapPosition;
        this.mapRevealMode = Enum.IsDefined(entry.MapRevealMode)
            ? entry.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(entry.MapRevealMode));

        // The pause's setup loads the map, pause interface, menu sprites and HUD artwork; the
        // screen binds that artwork and its capture places the pictures.
        screen = new PauseScreen(entry.MapPresentation, entry.SpriteSheets);
        BindSpriteSheets(entry.SpriteSheets);
        if (entry.GameplayHud is not null)
        {
            // Pause shows the HUD gameplay displayed (#1351, #1466) and draws it under its palette, so the energy and
            // ammo HUD stays over the area it covers instead of the map showing there.
            screen.ShowHud(entry.GameplayHud);
        }

        // Pause setup ends by clearing the effect overlay
        // beneath the HUD. Pause draws
        // only the HUD's own pictures (PauseScreen.ShownHudPictures), so every otherwise-empty map
        // square shows nothing.

        // The pause screen's base page shows the button page's two label rows over the backdrop's bottom rows, its labels
        // lit or dimmed by the highlight; the frame is the area's backdrop picture, the button rows' picture and each
        // label's picture in its shade (#1467).
        SetPauseButtonLabelMode(PauseButtonHighlight.MapPage);

        LoadMapPage();
        SelectFirstCollectedEquipment();
        SetupMapScrolling();
    }

    /// <summary>Whether the map or the equipment page is shown.</summary>
    public PauseScreenMode ScreenMode { get; private set; }

    /// <summary>
    /// Runs the pause menu's input after the caller has latched the update's input and applied the
    /// delayed-held filter. Returns true when Start requests unpausing.
    /// The runtime supplies its short frame counter for the colour animation's phase;
    /// standalone diagnostic callers may leave that phase at zero. <paramref name="bombs"/> are Samus's bomb slots frozen
    /// under the pause, which a same-frame toggle into the first reserve row repaints (#1505).
    /// </summary>
    public bool Step(ControllerButton delayedHeldInput, ControllerButton newlyPressedInput, ControllerButton? heldInput = null, int frameCycle = 0,
        SamusBombProjectileSystem? bombs = null)
    {
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations(frameCycle);
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
                audio?.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
                SetPauseButtonLabelMode(PauseButtonHighlight.EquipmentPage);
                Transition = PauseMenuTransition.MapToEquipmentFadeOut;
                TransitionBrightness = 15;
            }
            bool mapStart = HandleStartButton(delayedPressed);
            // Map arrows read the ordinary held controller buttons, not the delayed chrome
            // input. An accepted pulse keeps running after release, as in the game.
            if (MapScrollBounds.Step(heldInput ?? delayedHeldInput, ref mapScrollPosition))
                audio?.QueueSound(SamusSound.MapScroll, priority: SoundRequestPriority.Normal);
            return mapStart;
        }

        // The equipment page runs its own input, then L/R, then Start, so an A press in the
        // frame that unpauses still toggles the selected item. The equipment page consumes
        // the ordinary newly pressed buttons for D-pad/A; only the shared L/R/Start chrome
        // uses the delayed-held buttons.
        HandleEquipmentInput(newlyPressed, bombs);
        if ((delayedPressed & ControllerButton.L) != ControllerButton.None)
        {
            audio?.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
            SetPauseButtonLabelMode(PauseButtonHighlight.MapPage);
            Transition = PauseMenuTransition.EquipmentToMapFadeOut;
            TransitionBrightness = 15;
        }
        return HandleStartButton(delayedPressed);
    }

    /// <summary>
    /// The pause screen's Start-button handling, run last on both stable
    /// pages. It reads the delayed-held buttons: a fresh press is excluded for three frames by
    /// the controller's delayed-held filter before becoming menu input. True requests the unpause state.
    /// </summary>
    private bool HandleStartButton(ControllerButton delayedPressed)
    {
        if ((delayedPressed & ControllerButton.Start) == ControllerButton.None)
            return false;
        audio?.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
        SetPauseButtonLabelMode(PauseButtonHighlight.Unpausing);
        return true;
    }

    /// <summary>
    /// Advances the sprite animation drawn by the current pause-menu substate.
    /// </summary>
    /// <remarks>
    /// Rendering itself is intentionally pure. The game advances these counters from its
    /// draw steps, but a host may draw the same frame more than once while paused
    /// (for example, a screen capture). The game-state loop calls this exactly
    /// once per frame so extra draws cannot change game-visible timing.
    /// </remarks>
    public void AdvanceAnimations(int frameCycle = 0, PauseAnimationPhase phase = PauseAnimationPhase.Stable)
    {
        // The fade-out paths emit destination labels before map icons.
        mapLabelsBeforeIcons = phase == PauseAnimationPhase.FadingOut || Transition == PauseMenuTransition.MapToEquipmentFadeOut;
        // Only stable map dispatch handles the scroll arrows. Fade paths retain counters but hide arrows.
        CurrentMapArrows?.StepArrows(_ => false);
        pauseFrameCycle = frameCycle;
        // Stable pause dispatch advances the highlight cycle afterwards. The
        // outer pause/unpause fades draw sprites without advancing this owner.
        if (phase == PauseAnimationPhase.Stable && mapHighlight.Step())
            audio?.QueueSound(RoomSound.PauseBeep, priority: SoundRequestPriority.Normal);
        if (ScreenMode == PauseScreenMode.Map)
            StepMapIndicatorAnimation();
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Prepares the sprite list on the simulation owner, then captures without raster work.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // The map page shows the area map from the map row below its scroll position: the position names the row above
        // the screen's top, the row Samus's marker and the map's icons are placed from. The map does not repeat, so a
        // scroll left of or above its corner shows nothing there (#1555).
        Offset16<PlanePixels> mapScroll = MapScrollPosition.AsPlaneOffset() + new Offset16<PlanePixels>(0, PauseScreen.PageRowsHiddenAboveScreen);
        // The map page is a grid of the area map's cell pictures, the equipment page the pictures of what Samus has now
        // (#1515) and the frame the area's backdrop and button pictures (#1467).
        var frame = new PicturedPlaneLayer(screen.Frame(area, ButtonHighlight), Offset16<PlanePixels>.Zero);
        // The map scrolls inside the frame's opening, its border framing it (#1514); the frame's pictures lie their page's
        // rows above the screen's top, and its opening with them. The equipment page's pictures are placed where the
        // screen shows them, so its plane is not scrolled; it lies within the frame and is drawn over it whole.
        DisplayLayer page = ScreenMode == PauseScreenMode.Map
            ? new PictureGridLayer(AreaMapPictures.Grid(ShownMap, screen.Pictures), mapScroll) { Window = MapFrameOpening.Window(new(0, PauseScreen.PageRowsHiddenAboveScreen)) }
            : new PicturedPlaneLayer(screen.EquipmentPage(EquipmentPage, ReserveArrowShading), Offset16<PlanePixels>.Zero);
        // The retained HUD is its own layer at the HUD's place (#1466).
        var hud = new HudLayer(screen.ShownHudPictures(), DrawsOpaque: false);
        // Back to front (#1514): the area's frame, the page inside it, the page's markers, cursors and labels, and the
        // retained HUD over the top band. Each picture and sprite is drawn whole at its layer's place; the per-square and
        // per-sprite-part priority tiers that once interleaved them are a presentation quirk the model does not keep.
        DisplayLayer[] layers = [frame, page, screen.Sprites(), hud];
        return new(screen.Paint(), layers, Transition == PauseMenuTransition.None ? 15 : TransitionBrightness);
    }

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
            CurrentMapArrows?.DrawArrows(screen.Objects, mapHighlight.Shading, MapSheet, PauseMapScrollLayout.ArrowVerticalOffset);
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
                    ResetItemSelectorAnimation();
                    Transition = PauseMenuTransition.MapToEquipmentFadeIn;
                }
                else
                {
                    ScreenMode = PauseScreenMode.Map;
                    LoadMapPage();
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
            SelectedControl = PauseEquipmentControls.ReserveMode;
            return;
        }
        foreach (PauseUpgradeControl control in PauseEquipmentControls.Upgrades)
        {
            if (!control.Upgrade.IsSetIn(samus.CollectedBeams, samus.CollectedItems))
                continue;
            SelectedControl = control;
            return;
        }

        SelectedControl = PauseEquipmentControls.ReserveMode;
    }

    /// <summary>Builds the area map from installed presentation and exploration bits into the map page.</summary>
    [MemberNotNull(nameof(ShownMap))]
    private void LoadMapPage()
    {
        AreaMapPresentation maps = mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.");
        ShownMap = AreaMapProjection.Project(maps.Get(area), system, mapRevealMode);

        // Installed backdrops already contain their authored area lettering.
    }

    /// <summary>Derives map-scroll bounds and initial offset from visible tiles and Samus's map cell.</summary>
    private void SetupMapScrolling()
    {
        // The map's scroll limits come from scanning either the downloaded map
        // or the persistent explored plane. Expressing the scan in coordinates is exactly
        // equivalent to the game's cell-by-cell scan and makes the two-page 64x32 layout explicit.
        bool hasAreaMap = system.HasAreaMap(area);
        IAreaMapView map = (mapPresentation ?? throw new InvalidOperationException(
            "Pause map requires installed map presentation assets.")).Get(area);
        bool IsVisible(int x, int y)
        {
            bool explored = system.IsMapTileExplored(area, new(x, y));
            // The game's scroll bounds select one bit plane, unlike the map's drawing, which
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

        // Every value below is a map pixel or a difference of two, within a few hundred pixels of the 512 by 256 map:
        // the scans keep the left column at or before the right and the top row at or above the bottom, so the
        // halved spans are never negative and no value comes near the coordinate range. Only a negative result (Maridia's
        // margin left of the first column, a scroll above the map) is stored as its wrapped coordinate.
        int minimumX = left * 8 - (area == AreaId.Maridia ? 24 : 0);
        int maximumX = right * 8;
        int minimumY = top * 8;
        int maximumY = bottom * 8;
        CurrentMapScrollBounds = new PauseMapScroll(Position16<MapPixels>.Wrapped(minimumX, minimumY), Position16<MapPixels>.Wrapped(maximumX, maximumY));

        // The player's map cell: the room's map origin plus Samus's room screen, with the
        // map convention's one-row downward offset.
        Position16<MapPixels> player = SamusMapCell().ToMapPixels();
        int horizontal = minimumX + (maximumX - minimumX) / 2 - 128;
        int horizontalScreenPosition = player.X - horizontal;
        int distanceFromRightClamp = 224 - horizontalScreenPosition;
        if (distanceFromRightClamp >= 0)
        {
            int distanceFromLeftClamp = 32 - horizontalScreenPosition;
            if (distanceFromLeftClamp >= 0)
                horizontal -= distanceFromLeftClamp;
        }
        else
        {
            horizontal -= distanceFromRightClamp;
        }

        int verticalMiddle = minimumY + (maximumY - minimumY) / 2 + 16;
        // The centring offset is rounded down to whole eight-pixel map rows.
        int verticalCenterOffset = (128 - verticalMiddle) & ~7;
        int vertical = -verticalCenterOffset;
        int distanceFromTopClamp = 64 - (player.Y + verticalCenterOffset);
        if (distanceFromTopClamp >= 0)
        {
            vertical -= distanceFromTopClamp;
            if (vertical + 40 < 0)
                vertical = -40;
        }
        MapScrollPosition = Position16<MapPixels>.Wrapped(horizontal, vertical);
    }

    /// <summary>Maps Samus's current room screen to the area map with its one-row Y convention.</summary>
    private Position16<MapTiles> SamusMapCell() =>
        roomMapPosition + new Offset16<MapTiles>(RoomGridConversions.PixelToScreen(samus.Position.X.Whole),
            RoomGridConversions.PixelToScreen(samus.Position.Y.Whole) + 1);

    /// <summary>Draws the animated Samus map marker at the scrolled screen position.</summary>
    private void DrawMapPositionIndicator()
    {
        Position16<SpritePixels> origin = SamusMapCell().ToMapPixels().ToScreen(MapScrollPosition).ToSpritePosition();
        MapSpriteId indicator = PauseMapIndicatorAnimation.SpriteId(MapIndicatorAnimationFrame);
        DrawMapSprite(
            indicator,
            origin,
            MapSpriteColouring.Markers);
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

        // The equipment selector's animation holds each of its frames for that frame's own duration.
        ItemSelectorAnimationTimer--;
        if (ItemSelectorAnimationTimer > 0)
            return;

        var selectors = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        ItemSelectorAnimationFrame = selectors.NormalizePhase(ItemSelectorAnimationFrame + 1);
        ItemSelectorAnimationTimer = selectors.Duration(ItemSelectorAnimationFrame);
    }

    /// <summary>Draws the selected equipment item's authored sprite.</summary>
    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == SamusEquipmentFlags.None && samus.CollectedBeams == SamusBeamFlags.None)
            return;

        var selector = (mapPresentation ?? throw new InvalidOperationException(
            "Pause selector requires installed presentation assets.")).PauseSelectors;
        selector.Draw(screen.Objects, PauseSheet, SelectedControl, ItemSelectorAnimationFrame);
    }

    /// <summary>
    /// Runs the three button-label lighting variants and the redraw of the two label rows
    /// that follows each: the frame shows its labels in <paramref name="highlight"/>'s shades from the next capture.
    /// </summary>
    private void SetPauseButtonLabelMode(PauseButtonHighlight highlight) =>
        ButtonHighlight = Enum.IsDefined(highlight) ? highlight : throw new ArgumentOutOfRangeException(nameof(highlight), highlight, "Undefined pause highlight.");

    /// <summary>Draws a map sprite sheet frame in the map's colours <paramref name="colouring"/>.</summary>
    private void DrawMapSprite(MapSpriteId id, Position16<SpritePixels> origin, MapSpriteColouring colouring) =>
        // Map compositions use this shared entry. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        MapSpriteSheet.Draw(MapSheet, id, screen.Objects, origin, MapSpriteScreen.Pause, colouring);

    /// <summary>Handles reserve input or moves and toggles equipment in the game's update order.</summary>
    private void HandleEquipmentInput(ControllerButton pressed, SamusBombProjectileSystem? bombs)
    {
        PauseEquipmentCategory handlingCategory = SelectedControl.Category;
        if (handlingCategory == PauseEquipmentCategory.Reserves)
        {
            HandleReserveInput(pressed);
            MoveEquipmentSelector(pressed);
            return;
        }
        MoveEquipmentSelector(pressed);
        if ((pressed & ControllerButton.A) == ControllerButton.None)
            return;
        if (handlingCategory == PauseEquipmentCategory.Beams && samus.CollectedBeams == SamusBeamFlags.None)
            return;
        // Moving into the reserve rows on the frame A is pressed toggles with the reserve category, whose equipment and
        // label tables hold no entries. On the first row the game repaints a fixed run of projectile values, of which
        // play reads only the last spread bomb's vertical speed (#1505); it changes no equipment or label. The other
        // rows take their tables from scratch left by unrelated code, which only full memory emulation could reproduce,
        // so they stay rejected under the emulation scope limit (#1500).
        if (SelectedControl is not PauseUpgradeControl target)
        {
            if (SelectedControl != PauseEquipmentControls.ReserveMode)
                throw new NotSupportedException(
                    "A same-frame equipment toggle into a reserve row past the first corrupts unrelated game state; " +
                    "reproducing it needs full memory emulation, which is outside the port's scope.");
            audio?.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
            SamusBombProjectileSlot last = (bombs ?? throw new InvalidOperationException(
                "The first reserve row's same-frame toggle needs Samus's bomb slots.")).Slots[^1];
            last.BombSpreadVerticalVelocity = last.BombSpreadVerticalVelocity.WithWholePixels(
                ReserveRowToggleSpreadBombSpeed.Repaint(last.BombSpreadVerticalVelocity.WholePixels));
            return;
        }

        // The handler is chosen before movement: the handler of the category selected before the cursor moved toggles the item
        // the cursor has just moved onto.
        // A press handled from the boots column onto Plasma therefore bypasses the beam handler's Spazer/Plasma
        // exclusion. Do not add a category guard that corrects this: the retail game has none. The page
        // shows each label in its item's state every frame (#1515), so nothing here writes a label.
        SamusBeamFlags previousBeams = samus.EquippedBeams;
        PauseEquipmentBit mask = target.Upgrade;
        // The bit belongs to its own category's set, so toggling both sets toggles exactly that upgrade.
        samus.EquippedBeams ^= mask.Beam;
        samus.EquippedItems ^= mask.Item;

        audio?.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
        if (handlingCategory == PauseEquipmentCategory.Beams)
        {
            SamusBeamFlags added = samus.EquippedBeams & ~previousBeams;
            if ((added & SamusBeamFlags.Spazer) != SamusBeamFlags.None &&
                (samus.EquippedBeams & SamusBeamFlags.Plasma) != SamusBeamFlags.None)
                samus.EquippedBeams &= ~SamusBeamFlags.Plasma;
            else if ((added & SamusBeamFlags.Plasma) != SamusBeamFlags.None &&
                (samus.EquippedBeams & SamusBeamFlags.Spazer) != SamusBeamFlags.None)
                samus.EquippedBeams &= ~SamusBeamFlags.Spazer;
        }
    }

    /// <summary>The direction the cursor searches a column in: down the list, or up it.</summary>
    private enum ColumnSearch
    {
        /// <summary>Towards the bottom of the column.</summary>
        Down,
        /// <summary>Towards the top of the column.</summary>
        Up,
    }

    /// <summary>
    /// Searches <paramref name="column"/> from <paramref name="first"/> (inclusive) in <paramref name="direction"/> and moves
    /// the selector to the first collected upgrade it reaches.
    /// </summary>
    private bool TrySelectEquipment(ImmutableArray<PauseUpgradeControl> column, PauseUpgradeControl first, ColumnSearch direction) =>
        TrySelectEquipmentFrom(column, column.IndexOf(first), direction);

    /// <summary>
    /// Searches <paramref name="column"/>, which holds the selected control, from that control's neighbour in
    /// <paramref name="direction"/> and moves the selector to the first collected upgrade it reaches.
    /// </summary>
    private bool TrySelectNeighbour(ImmutableArray<PauseUpgradeControl> column, ColumnSearch direction)
    {
        int selected = column.IndexOf((PauseUpgradeControl)SelectedControl);
        return TrySelectEquipmentFrom(column, direction == ColumnSearch.Down ? selected + 1 : selected - 1, direction);
    }

    /// <summary>Walks <paramref name="column"/> from the control at <paramref name="start"/> to its end in <paramref name="direction"/>.</summary>
    private bool TrySelectEquipmentFrom(ImmutableArray<PauseUpgradeControl> column, int start, ColumnSearch direction)
    {
        if (column == PauseEquipmentControls.Beams && samus.HyperBeamEnabled)
            return false;
        int step = direction == ColumnSearch.Down ? 1 : -1;
        for (int place = start; place >= 0 && place < column.Length; place += step)
        {
            if (!column[place].Upgrade.IsSetIn(samus.CollectedBeams, samus.CollectedItems))
            {
                // The suit search down the column checks for its last control only AFTER an unsuccessful probe.
                // Thus directly entering the final suit control can succeed, but searching down from an earlier
                // missing upgrade must not discover it.
                if (column == PauseEquipmentControls.Suits && direction == ColumnSearch.Down && place + step >= column.Length - 1)
                    return false;
                continue;
            }
            SelectedControl = column[place];
            audio?.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
            return true;
        }
        return false;
    }

    /// <summary>Moves the selector to reserve mode when Samus has reserve capacity.</summary>
    private bool TrySelectReserves()
    {
        if (samus.MaxReserveEnergy == 0) return false;
        SelectedControl = PauseEquipmentControls.ReserveMode;
        audio?.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
        return true;
    }

    /// <summary>Applies the directional links among beam, suit, boot, and reserve rows.</summary>
    private void MoveEquipmentSelector(ControllerButton pressed)
    {
        bool left = (pressed & ControllerButton.Left) != ControllerButton.None, right = (pressed & ControllerButton.Right) != ControllerButton.None;
        bool up = (pressed & ControllerButton.Up) != ControllerButton.None, down = (pressed & ControllerButton.Down) != ControllerButton.None;
        ImmutableArray<PauseUpgradeControl> beams = PauseEquipmentControls.Beams, suits = PauseEquipmentControls.Suits,
            boots = PauseEquipmentControls.Boots;
        switch (SelectedControl.Category)
        {
            case PauseEquipmentCategory.Beams:
                if (right)
                {
                    if (!TrySelectEquipment(suits, up ? suits[0] : PauseEquipmentControls.BeamsRightSuitEntry, ColumnSearch.Down) && !up)
                        TrySelectEquipment(boots, boots[0], ColumnSearch.Down);
                }
                else if (down) TrySelectNeighbour(beams, ColumnSearch.Down);
                else if (up && !TrySelectNeighbour(beams, ColumnSearch.Up)) TrySelectReserves();
                break;
            case PauseEquipmentCategory.Suits:
                if (left)
                {
                    if (down || !TrySelectReserves()) TrySelectEquipment(beams, beams[0], ColumnSearch.Down);
                }
                else if (up) TrySelectNeighbour(suits, ColumnSearch.Up);
                else if (down && !TrySelectNeighbour(suits, ColumnSearch.Down)) TrySelectEquipment(boots, boots[0], ColumnSearch.Down);
                break;
            case PauseEquipmentCategory.Boots:
                if (left)
                {
                    if (up || !TrySelectEquipment(beams, beams[^1], ColumnSearch.Up)) TrySelectReserves();
                }
                else if (down) TrySelectNeighbour(boots, ColumnSearch.Down);
                else if (up && !TrySelectNeighbour(boots, ColumnSearch.Up))
                    TrySelectEquipment(suits, suits[^1], ColumnSearch.Up);
                break;
            case PauseEquipmentCategory.Reserves:
                if (right)
                {
                    if (down || !TrySelectEquipment(suits, suits[0], ColumnSearch.Down)) TrySelectEquipment(boots, boots[0], ColumnSearch.Down);
                }
                else if (up)
                {
                    if (SelectedControl != PauseEquipmentControls.ReserveMode)
                    {
                        SelectedControl = ReserveNeighbour(ColumnSearch.Up);
                        audio?.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
                    }
                }
                else if (down)
                {
                    if (SelectedControl == PauseEquipmentControls.ReserveTransfer ||
                        samus.ReserveTankMode == ReserveTankMode.Auto)
                        TrySelectEquipment(beams, beams[0], ColumnSearch.Down);
                    else
                    {
                        SelectedControl = ReserveNeighbour(ColumnSearch.Down);
                        if (samus.ReserveEnergy == 0) TrySelectEquipment(beams, beams[0], ColumnSearch.Down);
                        else audio?.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
                    }
                }
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(PauseEquipmentCategory)} {SelectedControl.Category}.");
        }
    }

    /// <summary>The selected reserve control's neighbour in <paramref name="direction"/>.</summary>
    private PauseReserveControl ReserveNeighbour(ColumnSearch direction)
    {
        ImmutableArray<PauseReserveControl> reserves = PauseEquipmentControls.Reserves;
        int selected = reserves.IndexOf((PauseReserveControl)SelectedControl);
        return reserves[direction == ColumnSearch.Down ? selected + 1 : selected - 1];
    }

    /// <summary>Shows the HUD gameplay last displayed, not one still being written.</summary>
    /// <param name="gameplayHud">The HUD gameplay latched at that frame (#1351, #1466, #1626).</param>
    public void SynchronizeAcceptedHud(ShownHud gameplayHud) => screen.ShowHud(gameplayHud);

    /// <summary>Rebinds edited map and pause artwork while retaining live scroll, highlight, and label state.</summary>
    public void BindMapPresentation(AreaMapPresentation? catalog)
    {
        mapPresentation = catalog ?? throw new InvalidOperationException(
            "Pause menu requires installed map presentation assets.");
        CurrentMapArrows ??= new FileSelectMapAnimations(catalog.Arrows);
        CurrentMapArrows.BindPresentation(catalog.Arrows);
        mapHighlight.Bind(catalog.HighlightCycle);
        // Live highlights and reserve-arrow colours are typed state their owners keep; rebinding never resets their phase.
        screen.BindArtwork(catalog);
        // The equipment page, its reserve arrow included, is derived from Samus's and the cursor's state each frame
        // (#1515, #1597), so the rebound sheets' pictures show it again.
        // The live button highlight, including a Start/fade highlight, is state the frame's pictures show again; do not
        // rerun controls.
        // Preserve the live scroll position and transition timing. Only refresh
        // the map page when it is shown; the equipment page shares its layer.
        if (ScreenMode == PauseScreenMode.Map) LoadMapPage();
    }

    /// <summary>
    /// The shading the reserve arrow's picture shows (#1512, #1597): its standard colours while Samus has no reserve
    /// capacity, the pulse phase of the accepted frame while AUTO pulses it, and its solid colours otherwise.
    /// </summary>
    internal PictureShading ReserveArrowShading => samus.MaxReserveEnergy == 0
        ? PictureShading.Standard
        : PictureShading.Shaded(ReserveArrow == ReserveArrowState.Animated
            ? PauseReserveArrowShades.Pulse(PauseCadenceDefinitions.ReserveArrowPulseFrame.Of(pauseFrameCycle))
            : PauseReserveArrowShades.Solid);

    /// <summary>
    /// How the reserve arrow shows now (#1597): pulsing while the cursor is on the reserve mode in AUTO, solid while it is on
    /// the reserve transfer, and dim otherwise.
    /// </summary>
    private ReserveArrowState ReserveArrow
    {
        get
        {
            return SelectedControl == PauseEquipmentControls.ReserveMode && samus.ReserveTankMode == ReserveTankMode.Auto ? ReserveArrowState.Animated
                : SelectedControl == PauseEquipmentControls.ReserveTransfer ? ReserveArrowState.Solid
                : ReserveArrowState.Hidden;
        }
    }

    /// <summary>Accepted frame phase (zero through 255) retained so repeated renders cannot advance tank fill flicker or the arrow's pulse.</summary>
    private int pauseFrameCycle;

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
                !PauseCadenceDefinitions.ReserveFlickerStepHidden.IsSetIn(pauseFrameCycle))
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

    /// <summary>Runs the tank subdispatcher before its D-pad response.</summary>
    private void HandleReserveInput(ControllerButton pressed)
    {
        if (SelectedControl == PauseEquipmentControls.ReserveMode)
        {
            if ((pressed & ControllerButton.A) == ControllerButton.None || samus.MaxReserveEnergy == 0) return;
            audio?.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
            samus.ReserveTankMode = samus.ReserveTankMode == ReserveTankMode.Auto
                ? ReserveTankMode.Manual : ReserveTankMode.Auto;
            return;
        }

        // The manual transfer.

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
        // Preserve that behavior: the reserves empty and the selector returns to the reserve mode switch.
        samus.ReserveEnergy = 0;
        ReserveTransferSoundDelay = WrappingCountdown.Start(0);
        SelectedControl = PauseEquipmentControls.ReserveMode;
    }
}
