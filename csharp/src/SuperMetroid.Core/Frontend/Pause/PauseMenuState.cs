using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Frontend.Artwork;
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
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        spriteSheets = Ensure.NotNull(sheets);
        screen.BindScreenPictures(sheets);
    }

    /// <summary>The installed sprite sheets the map's sprites draw from; artwork, never saved.</summary>
    private InstalledSpriteSheets spriteSheets;

    /// <summary>The map sprite sheet the map's markers, labels, arrows and position indicator draw from.</summary>
    private SpriteSheet MapSheet => spriteSheets.Get(SpriteSheetId.MapSprites);

    /// <summary>The pause sheet the equipment selectors and reserve-tank strip draw from.</summary>
    private SpriteSheet PauseSheet => spriteSheets.Get(SpriteSheetId.PauseSprites);


    /// <summary>Queues menu navigation and animation sounds.</summary>
    private readonly AudioQueues audio;
    /// <summary>The map's highlight cycle, advanced once per accepted stable menu update.</summary>
    internal readonly MapHighlightAnimation mapHighlight;
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
    private InstalledScreenContent screenContent;
    /// <summary>The pause screen: its artwork, pictures (including the retained gameplay HUD rows), colours and sprite entries (#1358).</summary>
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
    /// <summary>
    /// The page transition's fade: one level darker every update on the way out, and one level brighter every second
    /// update on the way in after the load.
    /// </summary>
    private readonly ScreenFade transitionFade = new(DisplayBrightness.Full);
    /// <summary>Current page-transition brightness, from black to full intensity.</summary>
    internal int TransitionBrightness => transitionFade.Level;
    /// <summary>The equipment control the cursor rests on (#1772); its category is the column the cursor is in.</summary>
    internal PauseEquipmentControl SelectedControl { get; private set; } = PauseEquipmentControls.ReserveMode;
    /// <summary>The map page's scroll, framed from the visible area cells when the map page is set up.</summary>
    internal MapScroll? CurrentMapScroll { get; private set; }
    /// <summary>The map page's scroll, which exists once the map page is set up.</summary>
    internal MapScroll ActiveMapScroll => CurrentMapScroll ??
        throw new InvalidOperationException("The pause map scroll has not been set up.");
    /// <summary>The map screen's scroll position.</summary>
    internal Offset16<MapPixels> MapScrollPosition => ActiveMapScroll.Scroll;
    /// <summary>Animated map arrows and their current visibility state.</summary>
    internal FileSelectMapAnimations MapArrows { get; }
    /// <summary>The map-position marker's pulse.</summary>
    internal readonly MapIndicatorPulse mapIndicator = new();
    /// <summary>The equipment selector's installed animation, which restarts on each equipment-page entry.</summary>
    internal readonly AnimationPlayer<PauseSelectorSegment, PauseSelectorLook, NoAnimationCue> itemSelector;

    /// <summary>Creates the isolated pause display image and initial map/equipment state from live gameplay.</summary>
    public PauseMenuState(PauseMenuEntry entry)
    {

        Ensure.NotNull(entry);
        screenContent = Ensure.NotNull(entry.ScreenContent);

        this.samus = Ensure.NotNull(entry.Samus);
        this.system = Ensure.NotNull(entry.System);
        this.audio = Ensure.NotNull(entry.Audio);
        MapArrows = new FileSelectMapAnimations(screenContent.Maps.Arrows, screenContent.Maps.HighlightCycle);
        mapHighlight = new MapHighlightAnimation(screenContent.Maps.HighlightCycle);
        PauseSelectorPresentation selectors = screenContent.PauseSelectors;
        itemSelector = AnimationPlayer<PauseSelectorSegment, PauseSelectorLook, NoAnimationCue>.StartShowing(
            selectors.Segments, selectors.Cycling, selectors.InitialDurationTicks);
        area = entry.Area;
        _ = AreaIds.Require(entry.Area);
        this.roomMapPosition = entry.RoomMapPosition;
        this.mapRevealMode = Enum.IsDefined(entry.MapRevealMode)
            ? entry.MapRevealMode
            : throw new ArgumentOutOfRangeException(nameof(entry.MapRevealMode));

        // The pause's setup loads the map, pause interface, menu sprites and HUD artwork; the
        // screen binds that artwork and its capture places the pictures.
        screen = new PauseScreen(entry.ScreenContent, entry.SpriteSheets);
        BindSpriteSheets(entry.SpriteSheets);
        // Pause shows the HUD gameplay displayed (#1351, #1466) and draws it in its colours, so the energy and ammo HUD
        // stays over the area it covers instead of the map showing there.
        screen.ShowHud(entry.GameplayHud);

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
    /// Runs the pause menu's input once the update's input is latched and the
    /// delayed-held filter applied. Returns true when Start requests unpausing.
    /// </summary>
    /// <param name="delayedHeldInput">The held buttons that repeat on this update, which the page chrome reads.</param>
    /// <param name="newlyPressedInput">The buttons newly pressed this update, which the equipment page reads.</param>
    /// <param name="heldInput">The ordinary held buttons, which the map's scroll arrows read.</param>
    /// <param name="frameCycle">The runtime's short frame counter, the colour animation's phase.</param>
    /// <param name="bombs">Samus's bombs frozen under the pause, which a same-frame toggle into the first reserve row repaints (#1505).</param>
    public bool Step(ControllerButton delayedHeldInput, ControllerButton newlyPressedInput, ControllerButton heldInput, int frameCycle,
        SamusBombProjectileSystem bombs)
    {
        Ensure.NotNull(bombs);
        // Stable pause states draw and therefore advance one page-specific sprite animation
        // every frame. Fade states call AdvanceAnimations explicitly from the frontend.
        AdvanceAnimations(frameCycle, PauseAnimationPhase.Stable);
        if (ScreenMode == PauseScreenMode.Map && Transition == PauseMenuTransition.None)
            MapArrows.StepArrows(ActiveMapScroll.CanScroll);
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
                audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
                SetPauseButtonLabelMode(PauseButtonHighlight.EquipmentPage);
                Transition = PauseMenuTransition.MapToEquipmentFadeOut;
                transitionFade.Restart(DisplayBrightness.Full, 0);
            }
            bool mapStart = HandleStartButton(delayedPressed);
            // Map arrows read the ordinary held controller buttons, not the delayed chrome
            // input. An accepted pulse keeps running after release, as in the game.
            if (ActiveMapScroll.Step(heldInput))
                audio.QueueSound(SamusSound.MapScroll, priority: SoundRequestPriority.Normal);
            return mapStart;
        }

        // The equipment page runs its own input, then L/R, then Start, so an A press in the
        // frame that unpauses still toggles the selected item. The equipment page consumes
        // the ordinary newly pressed buttons for D-pad/A; only the shared L/R/Start chrome
        // uses the delayed-held buttons.
        HandleEquipmentInput(newlyPressed, bombs);
        if ((delayedPressed & ControllerButton.L) != ControllerButton.None)
        {
            audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
            SetPauseButtonLabelMode(PauseButtonHighlight.MapPage);
            Transition = PauseMenuTransition.EquipmentToMapFadeOut;
            transitionFade.Restart(DisplayBrightness.Full, 0);
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
        audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
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
    public void AdvanceAnimations(int frameCycle, PauseAnimationPhase phase)
    {
        // Only the stable map update moves the scroll arrows. Fade paths retain counters but hide arrows.
        MapArrows.StepArrows(_ => false);
        pauseFrameCycle = frameCycle;
        // The stable pause update advances the highlight cycle afterwards. The
        // outer pause/unpause fades draw sprites without advancing this owner.
        if (phase == PauseAnimationPhase.Stable && mapHighlight.Step())
            audio.QueueSound(RoomSound.PauseBeep, priority: SoundRequestPriority.Normal);
        if (ScreenMode == PauseScreenMode.Map)
            mapIndicator.Step(_ => { });
        else
            StepItemSelectorAnimation();
    }

    /// <summary>Prepares the sprite list on the simulation owner, then captures without raster work.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        PrepareRenderSprites();
        // The map page shows the area map scrolled with the frame it shows through (#2207): its scroll position names the
        // map pixel Samus's marker and the map's icons are placed from, and the frame's installed pictures lie where the
        // screen shows them against those sprites. The map does not repeat, so a scroll left of or above its corner shows
        // nothing there (#1555).
        Offset16<PlanePixels> frameScroll = screen.FrameScroll(area);
        Offset16<PlanePixels> mapScroll = MapScrollPosition.AsPlaneOffset() + frameScroll;
        // The map page is a grid of the area map's cell pictures, the equipment page the pictures of what Samus has now
        // (#1515) and the frame the area's backdrop and button pictures (#1467).
        var frame = new PicturedPlaneLayer(screen.Frame(area, ButtonHighlight), Offset16<PlanePixels>.Zero);
        // The map scrolls inside the frame's opening, its border framing it (#1514); the opening lies where the frame's
        // installed pictures place it. The equipment page's pictures are installed where the screen shows them, so its
        // plane is not scrolled; it lies within the frame and is drawn over it whole.
        DisplayLayer page = ScreenMode == PauseScreenMode.Map
            ? new PictureGridLayer(AreaMapPictures.Grid(ShownMap, screen.Pictures), mapScroll) { Window = MapFrameOpening.Window(frameScroll) }
            : new PicturedPlaneLayer(screen.EquipmentPage(EquipmentPage, ReserveArrowShading), Offset16<PlanePixels>.Zero);
        // The retained HUD is its own layer at the HUD's place (#1466).
        var hud = new HudLayer(screen.ShownHudPictures(), DrawsOpaque: false);
        // Back to front (#1514): the area's frame, the page inside it, the page's markers, cursors and labels, and the
        // retained HUD over the top band. Each picture and sprite is drawn whole at its layer's place, in its layer's order.
        DisplayLayer[] layers = [frame, page, screen.SpriteLayer(), hud];
        return new(screen.Paint(), layers, Transition == PauseMenuTransition.None ? DisplayBrightness.Full : TransitionBrightness);
    }

    /// <summary>Assembles map markers or equipment sprites in their page-specific order.</summary>
    private void PrepareRenderSprites()
    {
        screen.Sprites.BeginFrame();
        if (ScreenMode == PauseScreenMode.Map)
        {
            var icons = new FileSelectMapIcons(system, area, screenContent.Maps.Stations, screenContent.Maps.Landmarks, MapSheet);
            MapArrows.DrawArrows(screen.Sprites, mapHighlight.Shading, MapSheet, MapFramingDefinitions.Pause.ArrowDrop);
            DrawMapPositionIndicator();
            // Pause shares the boss lists, defeated overlays and downloaded destination lettering with file select. The
            // map's icons and its elevator labels keep one layering in every phase (#2573).
            icons.DrawBossMarkers(screen.Sprites, MapScrollPosition);
            icons.DrawElevatorLabels(screen.Sprites, MapScrollPosition);
        }
        else
        {
            DrawEquipmentItemSelector();
            DrawReserveTanks();
        }
        screen.Sprites.FinishFrame();
    }

    /// <summary>Advances map/equipment fade, page load, and brightness phases.</summary>
    private void StepPageTransition()
    {
        switch (Transition)
        {
            case PauseMenuTransition.MapToEquipmentFadeOut:
            case PauseMenuTransition.EquipmentToMapFadeOut:
                if (!transitionFade.Darken())
                    return;

                // The fade-out returns at black before the separate load update.
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
                transitionFade.Restart(0, PauseFadeTiming.WaitBeforeEachStep);
                return;

            case PauseMenuTransition.MapToEquipmentFadeIn:
            case PauseMenuTransition.EquipmentToMapFadeIn:
                // The load set the fade to wait one update before each brightness step.
                if (transitionFade.Brighten())
                    Transition = PauseMenuTransition.None;
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
            if (!control.Upgrade.IsIn(samus.CollectedBeams, samus.CollectedItems))
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
        InstalledScreenContent maps = screenContent;
        ShownMap = AreaMapProjection.Project(maps.Get(area), system, mapRevealMode);

        // Installed backdrops already contain their authored area lettering.
    }

    /// <summary>Frames the map page from the visible cells' reach and Samus's map cell.</summary>
    private void SetupMapScrolling()
    {
        // The map's scroll limits come from either the downloaded map or the persistent explored plane, never their
        // union, unlike the map's drawing, which also displays explored secrets: unioning the planes would move the map's
        // centre after a secret is explored even though the downloaded limits are unchanged. A reveal assist frames the
        // cells it shows.
        bool hasAreaMap = system.HasAreaMap(area);
        IAreaMapView map = screenContent.Get(area);
        MapScrollLimits limits = mapRevealMode == MapRevealMode.None
            ? MapScrollLimits.OfRevealedPlane(map, system)
            : MapScrollLimits.Scan(cell => AreaMapVisibility.IsVisible(
                system.IsMapTileExplored(area, cell),
                hasAreaMap,
                map.IsRevealedByMapStation(cell),
                map.IsDiscoverable(cell),
                mapRevealMode));
        // The player's map cell: the room's map cell plus Samus's room screen.
        CurrentMapScroll = MapScroll.Centred(MapFramingDefinitions.Pause, limits, SamusMapCell().ToMapPixels());
    }

    /// <summary>The area-map cell of Samus's current room screen.</summary>
    private Position16<MapTiles> SamusMapCell() =>
        roomMapPosition + new Offset16<MapTiles>(RoomGridConversions.PixelToScreen(samus.Position.X.Whole),
            RoomGridConversions.PixelToScreen(samus.Position.Y.Whole));

    /// <summary>Draws the animated Samus map marker at the scrolled screen position.</summary>
    private void DrawMapPositionIndicator()
    {
        Offset16<ScreenPixels> origin = SamusMapCell().ToMapPixels().RelativeToScroll(MapScrollPosition);
        DrawMapSprite(
            mapIndicator.Sprite,
            origin,
            MapSpriteColouring.Markers);
    }

    /// <summary>Restarts the equipment selector on its first look, held for the installed opening hold.</summary>
    private void ResetItemSelectorAnimation()
    {
        PauseSelectorPresentation selectors = screenContent.PauseSelectors;
        itemSelector.Show(selectors.Cycling, selectors.InitialDurationTicks);
    }

    /// <summary>Advances the equipment selector only while inventory offers a selectable item.</summary>
    private void StepItemSelectorAnimation()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == SamusEquipmentFlags.None && samus.CollectedBeams == SamusBeamFlags.None)
            return;

        // The equipment selector's animation holds each of its looks for that look's own hold.
        itemSelector.Step();
    }

    /// <summary>Draws the selected equipment item's authored sprite.</summary>
    private void DrawEquipmentItemSelector()
    {
        if (samus.MaxReserveEnergy == 0 && samus.CollectedItems == SamusEquipmentFlags.None && samus.CollectedBeams == SamusBeamFlags.None)
            return;

        var selector = screenContent.PauseSelectors;
        selector.Draw(screen.Sprites, PauseSheet, SelectedControl, itemSelector.Showing);
    }

    /// <summary>
    /// Runs the three button-label lighting variants and the redraw of the two label rows
    /// that follows each: the frame shows its labels in <paramref name="highlight"/>'s shades from the next capture.
    /// </summary>
    private void SetPauseButtonLabelMode(PauseButtonHighlight highlight) =>
        ButtonHighlight = Enum.IsDefined(highlight) ? highlight : throw new ArgumentOutOfRangeException(nameof(highlight), highlight, "Undefined pause highlight.");

    /// <summary>Draws a map sprite sheet frame in the map's colours <paramref name="colouring"/>.</summary>
    private void DrawMapSprite(MapSpriteId id, Offset16<ScreenPixels> origin, MapSpriteColouring colouring) =>
        // Every map composition draws its map sprites here. Extracted equipment selectors
        // and reserve tanks draw through their semantic presentation owners.
        MapSpriteSheet.Draw(MapSheet, id, screen.Sprites, origin, MapSpriteScreen.Pause, colouring);

    /// <summary>Handles reserve input or moves and toggles equipment in the game's update order.</summary>
    private void HandleEquipmentInput(ControllerButton pressed, SamusBombProjectileSystem bombs)
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
        // Moving into the reserve rows on the frame A is pressed toggles with the reserve category, which lists no
        // equipment and no labels. On the first row the game repaints a fixed run of projectile values, of which play
        // reads only the last spread bomb's vertical speed (#1505); it changes no equipment or label. What the other
        // rows change depends on values unrelated code left behind, which only full memory emulation could reproduce,
        // so they stay rejected under the emulation scope limit (#1500).
        if (SelectedControl is not PauseUpgradeControl target)
        {
            if (SelectedControl != PauseEquipmentControls.ReserveMode)
                throw new NotSupportedException(
                    "A same-frame equipment toggle into a reserve row past the first corrupts unrelated game state; " +
                    "reproducing it needs full memory emulation, which is outside the emulation scope limit (#1500).");
            audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
            // A last place holding no spread bomb has no flight to repaint: no other bomb reads the speed, and the next
            // spread launches with its own (#2459).
            if (bombs.Bombs[^1].SpreadFlight is { } flight)
            {
                flight.VerticalVelocity = flight.VerticalVelocity.WithWholePixels(
                    ReserveRowToggleSpreadBombSpeed.SpeedAfterToggle(flight.VerticalVelocity.WholePixels));
            }
            return;
        }

        // The press belongs to the category selected before the cursor moved, which toggles the item the cursor has just
        // moved onto. A press from the boots column onto Plasma therefore skips the beam column's Spazer and Plasma
        // exclusion, as the game does. The page
        // shows each label in its item's state every frame (#1515), so nothing here writes a label.
        SamusBeamFlags previousBeams = samus.EquippedBeams;
        target.Upgrade.ToggleEquipped(samus);

        audio.QueueSound(SamusSound.MenuConfirm, priority: SoundRequestPriority.Normal);
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
        int step = direction switch
        {
            ColumnSearch.Down => 1,
            ColumnSearch.Up => -1,
            _ => throw new ArgumentOutOfRangeException(nameof(direction), direction, $"Undefined {nameof(ColumnSearch)}."),
        };
        for (int place = start; place >= 0 && place < column.Length; place += step)
        {
            if (!column[place].Upgrade.IsIn(samus.CollectedBeams, samus.CollectedItems))
            {
                // The suit search down the column checks for its last control only AFTER an unsuccessful probe.
                // Thus directly entering the final suit control can succeed, but searching down from an earlier
                // missing upgrade must not discover it.
                if (column == PauseEquipmentControls.Suits && direction == ColumnSearch.Down && place + step >= column.Length - 1)
                    return false;
                continue;
            }
            SelectedControl = column[place];
            audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
            return true;
        }
        return false;
    }

    /// <summary>Moves the selector to reserve mode when Samus has reserve capacity.</summary>
    private bool TrySelectReserves()
    {
        if (samus.MaxReserveEnergy == 0) return false;
        SelectedControl = PauseEquipmentControls.ReserveMode;
        audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
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
                        audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
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
                        else audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
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
    public void BindScreenContent(InstalledScreenContent catalog)
    {
        screenContent = Ensure.NotNull(catalog);
        MapArrows.BindPresentation(catalog.Maps.Arrows);
        mapHighlight.Bind(catalog.Maps.HighlightCycle);
        itemSelector.Rebind(catalog.PauseSelectors.Segments, catalog.PauseSelectors.Cycling);
        // Live highlights and reserve-arrow colours are typed state their owners keep; rebinding never resets their phase.
        screen.BindArtwork(catalog);
        // The equipment page, its reserve arrow included, is derived from Samus's and the cursor's state each frame
        // (#1515, #1597), so the rebound sheets' pictures show it again.
        // The live button highlight, including a Start/fade highlight, is state the frame's pictures show again; do not
        // rerun controls.
        // The scroll position and transition timing stay as they are. Only the map page
        // refreshes, and only when it is shown; the equipment page shares its layer.
        if (ScreenMode == PauseScreenMode.Map) LoadMapPage();
    }

    /// <summary>
    /// The shading the reserve arrow's picture shows (#1512, #1597): its standard colours while Samus has no reserve
    /// capacity, the pulse phase of the accepted frame while AUTO pulses it, and its solid colours otherwise.
    /// </summary>
    internal PictureShading ReserveArrowShading => samus.MaxReserveEnergy == 0
        ? PictureShading.Standard
        : PictureShading.Shaded(ReserveArrow == ReserveArrowState.Animated
            ? PauseReserveArrowShades.Pulse(PauseReserveArrowPulseFrame.PacedBy(pauseFrameCycle))
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
        int full = samus.ReserveEnergy / SamusEnergyDefinitions.EnergyPerTank;
        int remainder = samus.ReserveEnergy % SamusEnergyDefinitions.EnergyPerTank;
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
        for (; tank < samus.MaxReserveEnergy / SamusEnergyDefinitions.EnergyPerTank; tank++)
            Draw(PauseReserveTankVisual.Empty, tank);
        Draw(PauseReserveTankVisual.EndCap, tank);

        void Draw(PauseReserveTankVisual map, int index)
        {
            screenContent
                .PauseReserveTanks.Draw(screen.Sprites, PauseSheet, map, index);
        }
    }

    /// <summary>
    /// The manual reserve transfer's sound cadence, none while no transfer runs (#1985): it lasts the reserve energy rounded
    /// up to a whole sound interval, counted once each transfer update, and the refill sound plays on each update that
    /// starts one of its intervals (<see cref="PauseReserveEnergyDefinitions.RefillSoundStartsInterval"/>). Each update moves
    /// one unit of reserve energy, so the reserves run out before the cadence does; a transfer started with none (the
    /// cursor stays on the transfer when no beam can take it) takes its sound on its only update, as the game's countdown
    /// from zero did.
    /// </summary>
    internal UpdateCountdown? ReserveRefillSound { get; private set; }

    /// <summary>Runs the reserve tank controls' input before their D-pad response.</summary>
    private void HandleReserveInput(ControllerButton pressed)
    {
        if (SelectedControl == PauseEquipmentControls.ReserveMode)
        {
            if ((pressed & ControllerButton.A) == ControllerButton.None || samus.MaxReserveEnergy == 0) return;
            audio.QueueSound(SamusSound.MenuCursor, priority: SoundRequestPriority.Normal);
            samus.ReserveTankMode = samus.ReserveTankMode == ReserveTankMode.Auto
                ? ReserveTankMode.Manual : ReserveTankMode.Auto;
            return;
        }

        // The manual transfer.

        const int interval = PauseReserveEnergyDefinitions.RefillSoundInterval;
        if (ReserveRefillSound is not { } cadence)
        {
            if ((pressed & ControllerButton.A) == ControllerButton.None) return;
            cadence = UpdateCountdown.For((samus.ReserveEnergy + interval - 1) / interval * interval);
            ReserveRefillSound = cadence;
        }
        if (PauseReserveEnergyDefinitions.RefillSoundStartsInterval.IsDue(cadence.UpdatesLeft))
            audio.QueueSound(PauseReserveEnergyDefinitions.RefillSound, PauseReserveEnergyDefinitions.RefillSoundPriority);
        cadence.Step();

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

        // The refill drains every reserve even when only a small top-up was needed.
        // The reserves empty and the selector returns to the reserve mode switch.
        samus.ReserveEnergy = 0;
        ReserveRefillSound = null;
        SelectedControl = PauseEquipmentControls.ReserveMode;
    }
}
