using SuperMetroid.Core.Frontend.Artwork;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Coordinates existing-save map navigation without loading a gameplay room early.</summary>
/// <remarks>
/// Entry, area/room confirmation, scrolling, return and load handoff each retain their
/// own timer and input boundary; rendering never advances those owners.
/// </remarks>
public sealed class FileSelectMapMenuState : IMenuFrontendScreen, IComposedFrontendScene<LayeredScene>
{
    /// <inheritdoc/>
    public void BindSpriteSheets(InstalledSpriteSheets sheets)
    {
        areaGraphics.BindSheets(sheets);
        roomGraphics.BindSheets(sheets);
    }

    /// <summary>Receives confirmation, scrolling and transition sound requests during updates.</summary>
    private readonly AudioQueues audio;
    /// <summary>Saved Zebes area; Ceres bypasses this menu.</summary>
    private readonly AreaId area;
    /// <summary>The selected save's progress: its used stations label the area map's areas.</summary>
    private readonly PersistentProgress savedProgress;
    /// <summary>Renders the six-area selection view during entry and area navigation.</summary>
    private readonly FileSelectAreaMapGraphics areaGraphics;
    /// <summary>Renders the selected area's room map, including exploration and markers.</summary>
    private readonly FileSelectRoomMapGraphics roomGraphics;
    /// <summary>Owns room-map scroll position anchored to the selected save station.</summary>
    private MapScroll Scroll { get; set; }
    /// <summary>Save-station marker positioned from the installed presentation catalog.</summary>
    private FileSelectStationMarker marker;
    /// <summary>The saved station, which restores the marker and scroll anchor.</summary>
    private readonly LoadStation station;
    /// <summary>Advances map icon arrows and highlight colours independently of rendering.</summary>
    private readonly FileSelectMapAnimations animations;
    /// <summary>
    /// The phase the last update began in, whose step ran on it: an update that began on the room map stepped its arrows
    /// and its marker, so the frame shows them; the room map's first update, which began as the room was set up, shows
    /// neither, and the update that leaves the room map for a load or a return still shows both.
    /// </summary>
    private FileSelectMapNavigationPhase lastUpdateBeganIn = FileSelectMapNavigationPhase.EnteringArea;
    /// <summary>Reads latched input and owns area/room navigation phases.</summary>
    private readonly FileSelectMapNavigation navigation;
    /// <summary>Owns the initial reveal and fade before navigation can consume input.</summary>
    private readonly FileSelectMapEntry entry;
    /// <summary>Active clipping window for the room-to-area return transition.</summary>
    private FileSelectMapWindow? returnWindow;
    /// <summary>
    /// The wait of the current load or return, from the update that requested it: a return's setup of
    /// <see cref="FileSelectMapDefinitions.ReturnSetupFrames"/>, or a load's prelude, fade and black wait together; over
    /// for every other phase.
    /// </summary>
    private UpdateCountdown requestWait = UpdateCountdown.Ended();
    /// <summary>The screen's brightness: fully lit, and one level darker on every update of the exit fades, stopping at black.</summary>
    private readonly ScreenFade fade = new(DisplayBrightness.Full);

    /// <summary>Initializes existing-save map entry from the saved Zebes area/station, exploration, boss, map-station, and used-station data without loading a gameplay room.</summary>
    /// <param name="audio">Owner receiving menu confirmation, scroll, colour-loop, and transition sound requests.</param>
    /// <param name="random">The session's random number generator, which the saved progress the map reads shares with the rest of the session.</param>
    /// <param name="slot">Selected save slot; its area must be one of the six Zebes identities and its station supplies the load/scroll anchor.</param>
    /// <param name="initialHeldInput">Controller buttons already held on entry, latched to prevent a held confirmation button becoming a fresh navigation press.</param>
    /// <param name="screenContent">The installed screen content, whose area maps the screen draws.</param>
    /// <param name="spriteSheets">The installed sprite sheets the map's sprites draw from, rebound with the other artwork.</param>
    /// <exception cref="ArgumentOutOfRangeException">The save area is outside Zebes; Ceres bypasses this menu.</exception>
    public FileSelectMapMenuState(AudioQueues audio, SharedRandom random,
        SuperMetroidSaveSlot slot, ControllerButton initialHeldInput, InstalledScreenContent screenContent,
        InstalledSpriteSheets spriteSheets)
    {
        this.audio = Ensure.NotNull(audio);
        Ensure.NotNull(slot);
        if (!AreaIds.IsZebes(slot.Checkpoint.Area))
            throw new ArgumentOutOfRangeException(nameof(slot), "Ceres does not use the Zebes save-selection map.");
        area = slot.Checkpoint.Area;
        station = slot.Checkpoint;
        var system = new SharedGameState(random);
        system.Progress.CopyFrom(slot.Progression);
        savedProgress = system.Progress;
        Ensure.NotNull(screenContent);
        InstalledSpriteSheets sheets = Ensure.NotNull(spriteSheets);
        areaGraphics = new FileSelectAreaMapGraphics(area, screenContent, sheets);
        roomGraphics = new FileSelectRoomMapGraphics(system, area, screenContent, sheets);
        InstalledScreenContent = screenContent;
        Scroll = CreateScrollForCurrentContent();
        marker = new FileSelectStationMarker(station, screenContent.Maps.SaveMarkers);
        navigation = new FileSelectMapNavigation(area, screenContent.Maps.Labels, initialHeldInput);
        animations = new FileSelectMapAnimations(screenContent.Maps.Arrows, screenContent.Maps.HighlightCycle);
        entry = new FileSelectMapEntry();
    }

    /// <summary>Current public navigation phase, reporting EnteringArea until the separately owned initial reveal/fade completes, then exposing the navigation owner's phase.</summary>
    public FileSelectMapNavigationPhase Phase => entry.IsComplete ? navigation.Phase : FileSelectMapNavigationPhase.EnteringArea;
    /// <summary>
    /// How the map hands off to its host (#2562): its load or its options, kept once it reaches one; absent while the map
    /// runs, before it has reached either (#2587).
    /// </summary>
    public FileSelectMapOutcome? Outcome { get; private set; }

    /// <summary>Binds the installed map artwork when the game is bound to its installed content, without restarting navigation.</summary>
    public void BindScreenContent(InstalledScreenContent catalog)
    {
        InstalledScreenContent = Ensure.NotNull(catalog);
        navigation.BindLabels(catalog.Maps.Labels);
        areaGraphics.BindScreenContent(catalog);
        animations.BindHighlight(catalog.Maps.HighlightCycle);
        animations.BindPresentation(catalog.Maps.Arrows);
        roomGraphics.BindScreenContent(catalog);
        marker.BindPosition(station, catalog.Maps.SaveMarkers);
    }

    /// <summary>The installed map artwork the map draws and moves over.</summary>
    private InstalledScreenContent InstalledScreenContent { get; set; }

    /// <summary>Starts room-map scrolling at the anchor for the saved area and station.</summary>
    private MapScroll CreateScrollForCurrentContent()
    {
        InstalledScreenContent catalog = InstalledScreenContent;
        var anchor = FileSelectMapLoadAnchors.Get(station);
        return MapScroll.Centred(MapFramingDefinitions.FileSelect,
            MapScrollLimits.OfRevealedPlane(catalog.Get(area), roomGraphics.MapSystem), anchor);
    }

    /// <summary>Advances one menu update, retaining input latching during entry and the game's ordering of icon and colour animation, scrolling, navigation, transition sounds, and delayed load/options handoff.</summary>
    /// <param name="input">Current held controller buttons; edge reading is owned by navigation, not by rendering.</param>
    public void Step(ControllerButton input)
    {
        if (!entry.IsComplete)
        {
            navigation.LatchInputWithoutNavigation(input);
            entry.Step();
            return;
        }
        FileSelectMapNavigationPhase before = Phase;
        lastUpdateBeganIn = before;
        if (before == FileSelectMapNavigationPhase.Room) animations.StepArrows(Scroll.CanScroll);
        // The game draws/advances map icons before testing input, including the frame
        // that requests load/cancel and the following load fade.
        if (before is FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested)
        {
            if (animations.StepHighlight()) Queue(RoomSound.PauseBeep);
            marker.Step();
        }
        if (before == FileSelectMapNavigationPhase.Room && Scroll.Step(input))
            Queue(SamusSound.MapScroll);
        navigation.Step(input);
        if (Phase != before)
        {
            if (Phase == FileSelectMapNavigationPhase.AreaReturnRequested)
                requestWait = UpdateCountdown.Lasting(FileSelectMapDefinitions.ReturnSetupFrames);
            else if (Phase == FileSelectMapNavigationPhase.LoadRequested)
                requestWait = UpdateCountdown.Lasting(
                    FileSelectMapDefinitions.LoadPreludeFrames + FileSelectMapDefinitions.LoadFadeFrames + FileSelectMapDefinitions.LoadBlackFrames);
            else
                requestWait = UpdateCountdown.Ended();
            if (Phase is FileSelectMapNavigationPhase.PreparingWindow or FileSelectMapNavigationPhase.LoadRequested)
                Queue(SamusSound.MenuConfirm);
            if (Phase == FileSelectMapNavigationPhase.ExpandingWindow) Queue(SamusSound.MapExpand);
            if (Phase == FileSelectMapNavigationPhase.AreaReturnRequested) Queue(SamusSound.MapReturn);
            if (Phase == FileSelectMapNavigationPhase.Room)
            {
                Scroll = CreateScrollForCurrentContent();
                marker = new FileSelectStationMarker(station, InstalledScreenContent.Maps.SaveMarkers);
                animations.ResetHighlight();
            }
            return;
        }
        switch (Phase)
        {
            case FileSelectMapNavigationPhase.AreaReturnRequested:
                if (!requestWait.Step()) break;
                if (returnWindow is null)
                {
                    returnWindow = FileSelectMapWindow.CreateReturn(area, InstalledScreenContent.Maps.Labels);
                    Queue(SamusSound.MapReturn);
                }
                else if (returnWindow.Step())
                {
                    navigation.CompleteAreaReturn();
                    returnWindow = null;
                }
                break;
            case FileSelectMapNavigationPhase.OptionsRequested:
                if (fade.Darken())
                    Outcome = FileSelectMapOutcome.Options;
                break;
            case FileSelectMapNavigationPhase.LoadRequested:
                requestWait.Step();
                if (requestWait.Age <= FileSelectMapDefinitions.LoadPreludeFrames) break;
                if (!fade.IsBlack)
                {
                    fade.StepDarker();
                    break;
                }
                if (requestWait.IsOver)
                    Outcome = FileSelectMapOutcome.Load;
                break;
            // Navigation itself advances these phases; no menu countdown runs.
            case FileSelectMapNavigationPhase.Area:
            case FileSelectMapNavigationPhase.PreparingWindow:
            case FileSelectMapNavigationPhase.ExpandingWindow:
            case FileSelectMapNavigationPhase.InitializingRoom:
            case FileSelectMapNavigationPhase.Room:
            case FileSelectMapNavigationPhase.EnteringArea:
                break;
            default:
                throw new InvalidOperationException($"Undefined file-select map phase {Phase}.");
        }
    }

    /// <summary>Requests a menu sound at normal priority.</summary>
    private void Queue(SoundEffectId sound) => audio.QueueSound(sound, priority: SoundRequestPriority.Normal);

    /// <summary>Builds the area or room map scene the phase shows.</summary>
    public LayeredScene ComposeScene()
    {
        LayeredScene Area(AreaMapBackgroundGlow glow = AreaMapBackgroundGlow.WholeScreen) => areaGraphics.ComposeScene(savedProgress, glow);
        LayeredScene Frame() => roomGraphics.ComposeScene(content: RoomMapContent.FrameOnly);
        bool roomMapStepped = lastUpdateBeganIn == FileSelectMapNavigationPhase.Room;
        LayeredScene Room(FileSelectStationMarker? shownMarker) => roomGraphics.ComposeScene(Scroll.Scroll,
            shownMarker, roomMapStepped ? animations : null);
        if (!entry.IsComplete)
        {
            LayeredScene areaScene = Area();
            var black = new LayeredScene(new FramePaint(RgbColor.Black), [], DisplayBrightness.Full);
            return entry.Phase == FileSelectMapEntryPhase.Revealing
                ? Insert(black, areaScene, entry.Left, entry.Top, entry.Right + 1, entry.Bottom) : black;
        }
        LayeredScene scene = Phase switch
        {
            FileSelectMapNavigationPhase.ExpandingWindow or FileSelectMapNavigationPhase.InitializingRoom =>
                Window(Area(AreaMapBackgroundGlow.ForegroundOnly), Frame(), navigation.ActiveWindow),
            FileSelectMapNavigationPhase.Room when !roomMapStepped => Room(null),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested => Room(marker),
            FileSelectMapNavigationPhase.AreaReturnRequested when roomMapStepped => Room(marker),
            FileSelectMapNavigationPhase.AreaReturnRequested when requestWait.UpdatesLeft == 1 =>
                Insert(Frame(), Area(), FileSelectMapDefinitions.EntryWindowLeft, FileSelectMapDefinitions.EntryWindowTop,
                    FileSelectMapDefinitions.EntryWindowRight + 1, ScreenGeometry.ScreenHeightPixels - FileSelectMapDefinitions.EntryWindowTop),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null => Window(Area(), Frame(), returnWindow),
            FileSelectMapNavigationPhase.AreaReturnRequested => Frame(),
            FileSelectMapNavigationPhase.Area or FileSelectMapNavigationPhase.PreparingWindow or
                FileSelectMapNavigationPhase.OptionsRequested or FileSelectMapNavigationPhase.EnteringArea => Area(),
            _ => throw new InvalidOperationException($"Undefined file-select map phase {Phase}."),
        };
        return new(scene.Paint, scene.Layers, fade.Level);
    }

    /// <summary>
    /// Shows the room scene inset in the area scene through the reveal's rectangle (#1468, #1555): a completed reveal shows
    /// the scene it reveals whole, and an empty rectangle none of the room.
    /// </summary>
    private static LayeredScene Window(LayeredScene areaScene, LayeredScene roomScene, FileSelectMapWindow window)
    {
        if (window.IsComplete) return window.IsReturning ? areaScene : roomScene;
        if (window.IsEmpty) return areaScene;
        return Insert(areaScene, roomScene, window.Left, window.Top, window.Right, window.Bottom);
    }

    /// <summary>Overlays an inner scene inside half-open screen bounds while retaining the basis scene's paint and brightness.</summary>
    private static LayeredScene Insert(LayeredScene basis, LayeredScene inside,
        int left, int top, int right, int bottom)
    {
        DisplayLayer[] layers = [.. basis.Layers, new InsetSceneLayer(inside, left, top, right, bottom)];
        return new(basis.Paint, layers, basis.Brightness);
    }
}
