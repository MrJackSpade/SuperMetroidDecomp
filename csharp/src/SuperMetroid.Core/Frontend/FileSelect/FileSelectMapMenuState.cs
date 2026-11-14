using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Saves;
using SuperMetroid.Core.Frontend.Map.Artwork;
using SuperMetroid.Core.Frontend.Menus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Coordinates existing-save map navigation without loading a gameplay room early.</summary>
/// <remarks>
/// Entry, area/room confirmation, scrolling, return and load handoff each retain their
/// own timer and input boundary; rendering never advances those owners.
/// </remarks>
public sealed class FileSelectMapMenuState : IMenuFrontendScreen
{
    /// <inheritdoc/>
    public void BindSpriteSheets(InstalledSpriteSheets? sheets)
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
    internal readonly FileSelectRoomMapGraphics roomGraphics;
    /// <summary>Owns room-map scroll position anchored to the selected save station.</summary>
    internal FileSelectMapScroll Scroll { get; private set; }
    /// <summary>Save-station marker positioned from the installed presentation catalog.</summary>
    private FileSelectStationMarker marker;
    /// <summary>The saved station, which restores the marker and scroll anchor.</summary>
    private readonly LoadStation station;
    /// <summary>Suppresses marker display until the room-map entry animation reaches it.</summary>
    private bool markerDrawn;
    /// <summary>Advances map icon arrows and highlight palette independently of rendering.</summary>
    private readonly FileSelectMapAnimations animations;
    /// <summary>Controls whether room-map scrolling arrows are included in a frame.</summary>
    private bool drawArrows;
    /// <summary>Reads latched input and owns area/room navigation phases.</summary>
    private readonly FileSelectMapNavigation navigation;
    /// <summary>Owns the initial reveal and fade before navigation can consume input.</summary>
    private readonly FileSelectMapEntry entry;
    /// <summary>Active clipping window for the room-to-area return transition.</summary>
    private FileSelectMapWindow? returnWindow;
    /// <summary>Remaining transition updates before load or return handoff.</summary>
    private int pendingFrames;
    /// <summary>Master brightness from fifteen (full) down to zero (black) during the exit fades; each step is guarded by a positive level.</summary>
    private int brightness = 15;
    /// <summary>Installed map artwork, bound when the game is bound to its installed content.</summary>
    private AreaMapPresentation? mapPresentation;

    /// <summary>Initializes existing-save map entry from the saved Zebes area/station, exploration, boss, map-station, and used-station data without loading a gameplay room.</summary>
    /// <param name="audio">Owner receiving menu confirmation, scroll, palette-loop, and transition sound requests.</param>
    /// <param name="random">The session's random number generator, which the saved progress the map reads shares with the rest of the session.</param>
    /// <param name="slot">Selected save slot; its area must be one of the six Zebes identities and its station supplies the compiled load/scroll anchor.</param>
    /// <param name="initialHeldInput">Controller buttons already held on entry, latched to prevent a held confirmation button becoming a fresh navigation press.</param>
    /// <param name="mapPresentation">Required installed map artwork/layout catalog.</param>
    /// <param name="spriteSheets">Required installed sprite sheets the map's sprites draw from, rebound with the other artwork.</param>
    /// <exception cref="ArgumentOutOfRangeException">The save area is outside Zebes; Ceres bypasses this menu.</exception>
    /// <exception cref="InvalidOperationException">Map presentation assets or the sprite sheets are absent.</exception>
    public FileSelectMapMenuState(AudioQueues audio, SharedRandom random,
        SuperMetroidSaveSlot slot, ControllerButton initialHeldInput, AreaMapPresentation? mapPresentation = null,
        InstalledSpriteSheets? spriteSheets = null)
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
        if (mapPresentation is null)
            throw new InvalidOperationException("File-select map requires installed map presentation assets.");
        InstalledSpriteSheets sheets = spriteSheets ?? throw new InvalidOperationException("File-select map requires the installed sprite sheets.");
        areaGraphics = FileSelectAreaMapGraphics.FromPresentation(area, mapPresentation, sheets);
        roomGraphics = new FileSelectRoomMapGraphics(system, area, mapPresentation: mapPresentation, sheets: sheets);
        this.mapPresentation = mapPresentation;
        Scroll = CreateScrollForCurrentContent();
        marker = new FileSelectStationMarker(station, mapPresentation.SaveMarkers);
        navigation = new FileSelectMapNavigation(area, initialHeldInput);
        navigation.BindLabels(mapPresentation.Labels);
        animations = new FileSelectMapAnimations(mapPresentation.Arrows);
        animations.BindHighlight(mapPresentation.HighlightCycle);
        entry = new FileSelectMapEntry();
    }

    /// <summary>Current public navigation phase, reporting EnteringArea until the separately owned initial reveal/fade completes, then exposing the navigation owner's phase.</summary>
    public FileSelectMapNavigationPhase Phase => entry.IsComplete ? navigation.Phase : FileSelectMapNavigationPhase.EnteringArea;
    /// <summary>Latched host handoff request after confirmation, two prelude updates, fifteen brightness steps, and thirty-two black updates; the navigation phase alone does not authorize loading gameplay.</summary>
    public bool LoadRequested { get; private set; }
    /// <summary>Latched host request to replace this menu with game options once its cancel/options fade reaches zero brightness; no options state is created by this owner.</summary>
    public bool OptionsRequested { get; private set; }

    /// <summary>Binds the installed map artwork when the game is bound to its installed content, without restarting navigation.</summary>
    public void BindMapPresentation(AreaMapPresentation? catalog)
    {
        mapPresentation = catalog;
        navigation.BindLabels(catalog?.Labels);
        areaGraphics.BindMapPresentation(catalog);
        animations.BindHighlight(catalog?.HighlightCycle);
        animations.BindPresentation(catalog?.Arrows);
        roomGraphics.BindMapPresentation(catalog);
        marker.BindPosition(station, catalog?.SaveMarkers);
    }

    /// <summary>Starts room-map scrolling at the compiled anchor for the saved area and station.</summary>
    private FileSelectMapScroll CreateScrollForCurrentContent()
    {
        AreaMapPresentation catalog = mapPresentation ?? throw new InvalidOperationException(
            "File-select map requires installed map presentation assets.");
        var anchor = FileSelectMapLoadAnchors.Get(station);
        return new FileSelectMapScroll(catalog.Get(area), roomGraphics.MapSystem, anchor);
    }

    /// <summary>Advances one menu update, retaining input latching during entry and the game's ordering of icon/palette animation, scrolling, navigation, transition sounds, and delayed load/options handoff.</summary>
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
        drawArrows = before == FileSelectMapNavigationPhase.Room;
        if (drawArrows) animations.StepArrows(Scroll.CanScroll);
        // The game draws/advances map icons before testing input, including the frame
        // that requests load/cancel and the following load fade.
        if (before is FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested)
        {
            if (animations.StepHighlight()) Queue(RoomSound.PauseBeep);
            marker.Step();
            markerDrawn = true;
        }
        if (before == FileSelectMapNavigationPhase.Room && Scroll.Step(input))
            Queue(SamusSound.MapScroll);
        navigation.Step(input);
        if (Phase != before)
        {
            pendingFrames = 0;
            if (Phase is FileSelectMapNavigationPhase.PreparingWindow or FileSelectMapNavigationPhase.LoadRequested)
                Queue(SamusSound.MenuConfirm);
            if (Phase == FileSelectMapNavigationPhase.ExpandingWindow) Queue(SamusSound.MapExpand);
            if (Phase == FileSelectMapNavigationPhase.AreaReturnRequested) Queue(SamusSound.MapReturn);
            if (Phase == FileSelectMapNavigationPhase.Room)
            {
                Scroll = CreateScrollForCurrentContent();
                marker = new FileSelectStationMarker(station, mapPresentation?.SaveMarkers);
                markerDrawn = false;
                animations.ResetHighlight();
            }
            return;
        }
        switch (Phase)
        {
            case FileSelectMapNavigationPhase.AreaReturnRequested:
                if (++pendingFrames < FileSelectMapDefinitions.ReturnSetupFrames) break;
                if (returnWindow is null)
                {
                    returnWindow = FileSelectMapWindow.CreateReturn(area, mapPresentation?.Labels);
                    Queue(SamusSound.MapReturn);
                }
                else if (returnWindow.Step())
                {
                    navigation.CompleteAreaReturn();
                    returnWindow = null;
                }
                break;
            case FileSelectMapNavigationPhase.OptionsRequested:
                if (brightness > 0) brightness--;
                OptionsRequested = brightness == 0;
                break;
            case FileSelectMapNavigationPhase.LoadRequested:
                pendingFrames++;
                if (pendingFrames <= FileSelectMapDefinitions.LoadPreludeFrames) break;
                if (brightness > 0) { brightness--; break; }
                LoadRequested = pendingFrames >= FileSelectMapDefinitions.LoadPreludeFrames + 15 + FileSelectMapDefinitions.LoadBlackFrames;
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

    /// <summary>Captures both map-memory owners and resolved window edges at the display boundary.</summary>
    public LayeredScene CaptureRenderSnapshot()
    {
        LayeredScene Area(AreaMapBackgroundGlow glow = AreaMapBackgroundGlow.WholeScreen) => areaGraphics.CaptureRenderSnapshot(savedProgress, glow);
        LayeredScene Frame() => roomGraphics.CaptureRenderSnapshot(content: RoomMapContent.FrameOnly);
        LayeredScene Room(FileSelectStationMarker? shownMarker) => roomGraphics.CaptureRenderSnapshot(Scroll.Scroll,
            shownMarker, drawArrows ? animations : null);
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
            FileSelectMapNavigationPhase.Room when !markerDrawn => Room(null),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested => Room(marker),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 => Room(marker),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapDefinitions.ReturnSetupFrames - 1 =>
                Insert(Frame(), Area(), FileSelectMapDefinitions.EntryWindowLeft, FileSelectMapDefinitions.EntryWindowTop,
                    FileSelectMapDefinitions.EntryWindowRight + 1, ScreenGeometry.ScreenHeightPixels - FileSelectMapDefinitions.EntryWindowTop),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null => Window(Area(), Frame(), returnWindow),
            FileSelectMapNavigationPhase.AreaReturnRequested => Frame(),
            FileSelectMapNavigationPhase.Area or FileSelectMapNavigationPhase.PreparingWindow or
                FileSelectMapNavigationPhase.OptionsRequested or FileSelectMapNavigationPhase.EnteringArea => Area(),
            _ => throw new InvalidOperationException($"Undefined file-select map phase {Phase}."),
        };
        return new(scene.Paint, scene.Layers, brightness);
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
