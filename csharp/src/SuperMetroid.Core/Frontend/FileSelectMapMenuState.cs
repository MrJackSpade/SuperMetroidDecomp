using System.Buffers.Binary;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Audio;
using SuperMetroid.Core.Game;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rendering;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Frontend;

/// <summary>Coordinates existing-save map navigation without loading a gameplay room early.</summary>
/// <remarks>
/// Entry, area/room confirmation, scrolling, return and load handoff each retain their
/// own timer and input boundary; rendering never advances those owners.
/// </remarks>
public sealed class FileSelectMapMenuState
{
    /// <summary>Shared menu address space for map owners and sound-triggering navigation.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Receives confirmation, scrolling and transition sound requests during updates.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Saved Zebes area; Ceres bypasses this menu.</summary>
    private readonly int area;
    /// <summary>Per-area used-station words restored from the selected save slot.</summary>
    private readonly ushort[] usedStations = new ushort[FileSelectMapRomData.AreaCount];
    /// <summary>Renders the six-area selection view during entry and area navigation.</summary>
    private readonly FileSelectAreaMapGraphics areaGraphics;
    /// <summary>Renders the selected area's room map, including exploration and markers.</summary>
    internal readonly FileSelectRoomMapGraphics roomGraphics;
    /// <summary>Owns room-map scroll position anchored to the selected save station.</summary>
    internal FileSelectMapScroll scroll;
    /// <summary>Save-station marker positioned from the installed presentation catalog.</summary>
    private FileSelectStationMarker marker;
    /// <summary>Saved station index used to restore the marker and scroll anchor.</summary>
    private readonly ushort stationIndex;
    /// <summary>Suppresses marker display until the room-map entry animation reaches it.</summary>
    private bool markerDrawn;
    /// <summary>Advances map icon arrows and highlight palette independently of rendering.</summary>
    private readonly FileSelectMapAnimations animations;
    /// <summary>Controls whether room-map scrolling arrows are included in a frame.</summary>
    private bool drawArrows;
    /// <summary>Interprets latched input and owns area/room navigation phases.</summary>
    private readonly FileSelectMapNavigation navigation;
    /// <summary>Owns the initial reveal and fade before navigation can consume input.</summary>
    private readonly FileSelectMapEntry entry;
    /// <summary>Active clipping window for the room-to-area return transition.</summary>
    private FileSelectMapWindow? returnWindow;
    /// <summary>Remaining transition updates before load or return handoff.</summary>
    private int pendingFrames;
    /// <summary>Master brightness applied after scene composition during fades.</summary>
    private byte brightness = 15;
    /// <summary>Installed map artwork rebound after debugger restoration instead of serialized with navigation state.</summary>
    [NonSerialized] private AreaMapPresentationCatalog? mapPresentation;

    /// <summary>Initializes existing-save map entry from the saved Zebes area/station, exploration, boss, map-station, and used-station data without loading a gameplay room.</summary>
    /// <param name="bus">Runtime address space shared by menu systems.</param>
    /// <param name="audio">Owner receiving menu confirmation, scroll, palette-loop, and transition sound requests.</param>
    /// <param name="slot">Selected save slot; its area must be one of the six Zebes identities and its station supplies the compiled load/scroll anchor.</param>
    /// <param name="initialHeldInput">Native controller-button word already held on entry, latched to prevent a held confirmation button becoming a fresh navigation press.</param>
    /// <param name="mapPresentation">Required installed map artwork/layout catalog, rebound separately after debugger restoration.</param>
    /// <exception cref="ArgumentOutOfRangeException">The save area is outside Zebes; Ceres bypasses this menu.</exception>
    /// <exception cref="InvalidOperationException">Map presentation assets are absent.</exception>
    public FileSelectMapMenuState(ISnesAddressSpace bus, CartridgeAudioState audio,
        SuperMetroidSaveSlot slot, ushort initialHeldInput, AreaMapPresentationCatalog? mapPresentation = null)
    {
        this.bus = Ensure.NotNull(bus);
        this.audio = Ensure.NotNull(audio);
        Ensure.NotNull(slot);
        area = slot.Area;
        stationIndex = slot.SaveStation;
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(slot), "Ceres does not use the Zebes save-selection map.");
        var system = new Bank80SystemState();
        system.LoadMapStationBytes(slot.MapStationBytes);
        system.LoadBossBytes(slot.BossBytes);
        system.LoadExploredMapBytes(slot.ExploredMapBytes);
        for (int index = 0; index < usedStations.Length; index++)
            usedStations[index] = BinaryPrimitives.ReadUInt16LittleEndian(slot.UsedSaveStationBytes.AsSpan(index * 2));
        var typedArea = (AreaId)area;
        if (mapPresentation is null)
            throw new InvalidOperationException("File-select map requires installed map presentation assets.");
        areaGraphics = FileSelectAreaMapGraphics.FromPresentation(bus, area, mapPresentation);
        roomGraphics = new FileSelectRoomMapGraphics(bus, system, typedArea, mapPresentation: mapPresentation);
        this.mapPresentation = mapPresentation;
        scroll = CreateScrollForCurrentContent();
        marker = new FileSelectStationMarker(bus, typedArea, slot.SaveStation, mapPresentation?.SaveMarkers);
        navigation = new FileSelectMapNavigation(bus, area, initialHeldInput);
        navigation.BindLabels(mapPresentation?.Labels);
        animations = new FileSelectMapAnimations(bus, mapPresentation?.Arrows);
        animations.BindPalette(mapPresentation?.HighlightCycle);
        entry = new FileSelectMapEntry(mapPresentation?.Palettes ?? throw new InvalidOperationException(
            "File-select map requires installed map palettes."));
    }

    /// <summary>Current public navigation phase, reporting EnteringArea until the separately owned initial reveal/fade completes, then exposing the navigation owner's phase.</summary>
    public FileSelectMapNavigationPhase Phase => entry.IsComplete ? navigation.Phase : FileSelectMapNavigationPhase.EnteringArea;
    /// <summary>Latched host handoff request after confirmation, two prelude updates, fifteen brightness steps, and thirty-two black updates; the navigation phase alone does not authorize loading gameplay.</summary>
    public bool LoadRequested { get; private set; }
    /// <summary>Latched host request to replace this menu with game options once its cancel/options fade reaches zero brightness; no options state is created by this owner.</summary>
    public bool OptionsRequested { get; private set; }

    /// <summary>Rebinds host presentation after restoration without restarting navigation.</summary>
    internal void BindMapPresentation(AreaMapPresentationCatalog? catalog)
    {
        mapPresentation = catalog;
        navigation.BindLabels(catalog?.Labels);
        areaGraphics.BindLabels(catalog?.Labels);
        areaGraphics.BindPalettes(catalog?.Palettes);
        areaGraphics.BindScreens(catalog?.Screens, catalog?.WorldArtwork);
        areaGraphics.BindSprites(catalog?.Sprites);
        entry.BindPalettes(catalog?.Palettes);
        animations.BindPalette(catalog?.HighlightCycle);
        animations.BindPresentation(catalog?.Arrows);
        roomGraphics.BindMapPresentation(catalog);
        marker.BindPosition(bus, (AreaId)area, stationIndex, catalog?.SaveMarkers);
    }

    /// <summary>Starts room-map scrolling at the compiled anchor for the saved area and station.</summary>
    private FileSelectMapScroll CreateScrollForCurrentContent()
    {
        var typedArea = (AreaId)area;
        AreaMapPresentationCatalog catalog = mapPresentation ?? throw new InvalidOperationException(
            "File-select map requires installed map presentation assets.");
        var anchor = FileSelectMapLoadAnchors.Get(typedArea, stationIndex);
        return new FileSelectMapScroll(bus, catalog.Get(typedArea), roomGraphics.MapSystem, anchor);
    }

    /// <summary>Advances one menu update, retaining input latching during entry and native ordering of icon/palette animation, scrolling, navigation, transition sounds, and delayed load/options handoff.</summary>
    /// <param name="input">Current held native controller-button word; edge interpretation is owned by navigation, not by rendering.</param>
    public void Step(ushort input)
    {
        if (!entry.IsComplete)
        {
            navigation.LatchInputWithoutNavigation(input);
            entry.Step();
            return;
        }
        FileSelectMapNavigationPhase before = Phase;
        drawArrows = before == FileSelectMapNavigationPhase.Room;
        if (drawArrows) animations.StepArrows(scroll.CanScroll);
        // Native draws/advances map icons before testing input, including the frame
        // that requests load/cancel and the following load fade.
        if (before is FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested)
        {
            if (animations.StepPalette(roomGraphics.Cgram)) Queue(SoundEffectLibrary3Sounds.MapPaletteLoop);
            marker.Step();
            markerDrawn = true;
        }
        if (before == FileSelectMapNavigationPhase.Room && scroll.Step(input))
            Queue(SoundEffectLibrary1Sounds.MapScroll);
        navigation.Step(input);
        if (Phase != before)
        {
            pendingFrames = 0;
            if (Phase is FileSelectMapNavigationPhase.PreparingWindow or FileSelectMapNavigationPhase.LoadRequested)
                Queue(SoundEffectLibrary1Sounds.MenuConfirm);
            if (Phase == FileSelectMapNavigationPhase.ExpandingWindow) Queue(SoundEffectLibrary1Sounds.MapExpand);
            if (Phase == FileSelectMapNavigationPhase.AreaReturnRequested) Queue(SoundEffectLibrary1Sounds.MapReturn);
            if (Phase == FileSelectMapNavigationPhase.Room)
            {
                scroll = CreateScrollForCurrentContent();
                marker = new FileSelectStationMarker(bus, (AreaId)area, stationIndex, mapPresentation?.SaveMarkers);
                markerDrawn = false;
                animations.ResetPalette();
            }
            return;
        }
        switch (Phase)
        {
            case FileSelectMapNavigationPhase.AreaReturnRequested:
                if (++pendingFrames < FileSelectMapRomData.ReturnSetupFrames) break;
                if (returnWindow is null)
                {
                    returnWindow = FileSelectMapWindow.CreateReturn(bus, area, mapPresentation?.Labels);
                    Queue(SoundEffectLibrary1Sounds.MapReturn);
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
                if (pendingFrames <= FileSelectMapRomData.LoadPreludeFrames) break;
                if (brightness > 0) { brightness--; break; }
                LoadRequested = pendingFrames >= FileSelectMapRomData.LoadPreludeFrames + 15 + FileSelectMapRomData.LoadBlackFrames;
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

    /// <summary>Reusable composed frame buffer; callers must copy its contents before another render.</summary>
    [NonSerialized] private Rgba32[]? frameBuffer;

    /// <summary>Draws the current 256-by-224 entry, area, room, or clipped transition scene and applies current master brightness without consuming input or advancing any timers.</summary>
    /// <returns>An owned RGBA frame valid only until this menu renders again; callers retaining a frame must copy it.</returns>
    public Rgba32[] Render()
    {
        Rgba32[] composed = frameBuffer ??= new Rgba32[FrontendFrame.Width * FrontendFrame.Height];
        if (!entry.IsComplete) return entry.Render(areaGraphics.Render(usedStations), composed);
        Rgba32[] pixels = Phase switch
        {
            FileSelectMapNavigationPhase.ExpandingWindow or FileSelectMapNavigationPhase.InitializingRoom =>
                FileSelectMapWindowCompositor.Composite(areaGraphics.Render(usedStations, false),
                    roomGraphics.RenderFrameOnly(), navigation.Window!, composed),
            FileSelectMapNavigationPhase.Room when !markerDrawn => roomGraphics.RenderBackgrounds(scroll.Scroll),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested =>
                roomGraphics.Render(scroll.Scroll, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 =>
                roomGraphics.Render(scroll.Scroll, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapRomData.ReturnSetupFrames - 1 =>
                FileSelectMapWindowCompositor.CompositeInitialEntryWindow(
                    areaGraphics.Render(usedStations), roomGraphics.RenderFrameOnly(), composed),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null =>
                // Return setup calls $81:A5B3 (CGADSUB=$25); unlike the forward
                // expansion's $05, this retains backdrop addition outside the window.
                FileSelectMapWindowCompositor.Composite(areaGraphics.Render(usedStations),
                    roomGraphics.RenderFrameOnly(), returnWindow, composed),
            FileSelectMapNavigationPhase.AreaReturnRequested => roomGraphics.RenderFrameOnly(),
            FileSelectMapNavigationPhase.Area or FileSelectMapNavigationPhase.PreparingWindow or
                FileSelectMapNavigationPhase.OptionsRequested or FileSelectMapNavigationPhase.EnteringArea => areaGraphics.Render(usedStations),
            _ => throw new InvalidOperationException($"Undefined file-select map phase {Phase}."),
        };
        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    /// <summary>Requests a menu sound through the audio owner's six-entry queue limit.</summary>
    private void Queue(SoundEffectId sound) => audio.QueueSound(sound, maximumQueued: 6);

    /// <summary>Captures both map-memory owners and resolved window edges at the display boundary.</summary>
    public LayeredRenderSnapshot CaptureRenderSnapshot()
    {
        LayeredRenderSnapshot Area(bool backdrop = true) => areaGraphics.CaptureRenderSnapshot(usedStations, backdrop);
        LayeredRenderSnapshot Frame() => roomGraphics.CaptureRenderSnapshot(frameOnly: true);
        LayeredRenderSnapshot Room(bool icons = true) => roomGraphics.CaptureRenderSnapshot(scroll.Scroll,
            icons ? marker : null, drawArrows ? animations : null);
        if (!entry.IsComplete)
        {
            LayeredRenderSnapshot areaScene = Area();
            Bgr555[] palette = areaScene.Memory.Cgram.ToArray(); palette[0] = Bgr555.Black;
            var black = new LayeredRenderSnapshot(new PpuMemorySnapshot(areaScene.Memory.Vram, palette,
                areaScene.Memory.Oam, 0), [], areaScene.ObjectSelection, SnesPpuLayout.MaximumMasterBrightness);
            return entry.Phase == FileSelectMapEntryPhase.Revealing
                ? Insert(black, areaScene, entry.Left, entry.Top, entry.Right + 1, entry.Bottom) : black;
        }
        LayeredRenderSnapshot scene = Phase switch
        {
            FileSelectMapNavigationPhase.ExpandingWindow or FileSelectMapNavigationPhase.InitializingRoom =>
                Window(Area(false), Frame(), navigation.Window!),
            FileSelectMapNavigationPhase.Room when !markerDrawn => Room(false),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested => Room(),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 => Room(),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapRomData.ReturnSetupFrames - 1 =>
                Insert(Frame(), Area(), FileSelectMapRomData.EntryWindowLeft, FileSelectMapRomData.EntryWindowTop,
                    FileSelectMapRomData.EntryWindowRight + 1, SnesPpuLayout.ScreenHeightPixels - FileSelectMapRomData.EntryWindowTop),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null => Window(Area(), Frame(), returnWindow),
            FileSelectMapNavigationPhase.AreaReturnRequested => Frame(),
            FileSelectMapNavigationPhase.Area or FileSelectMapNavigationPhase.PreparingWindow or
                FileSelectMapNavigationPhase.OptionsRequested or FileSelectMapNavigationPhase.EnteringArea => Area(),
            _ => throw new InvalidOperationException($"Undefined file-select map phase {Phase}."),
        };
        return new(scene.Memory, scene.Layers, scene.ObjectSelection, brightness);
    }

    /// <summary>Projects the native clipped room/area transition into render layers, including completed and off-screen windows.</summary>
    private static LayeredRenderSnapshot Window(LayeredRenderSnapshot areaScene, LayeredRenderSnapshot roomScene, FileSelectMapWindow window)
    {
        if (window.IsComplete) return window.IsReturning ? areaScene : roomScene;
        int top = (byte)window.Top;
        int bottom = Math.Min(SnesPpuLayout.ScreenHeightPixels, top + Math.Max(1, (int)unchecked((byte)(window.Bottom - window.Top))));
        int left = (byte)window.Left, right = (byte)window.Right;
        // The legacy HDMA projection performs no row copies for a wholly off-screen
        // interval or crossed horizontal edges; resolve that before contract validation.
        if (left > right || top >= bottom) return areaScene;
        return Insert(areaScene, roomScene, left, top, right + 1, bottom);
    }

    /// <summary>Overlays an inner scene inside half-open screen bounds while retaining the basis scene's PPU memory and brightness.</summary>
    private static LayeredRenderSnapshot Insert(LayeredRenderSnapshot basis, LayeredRenderSnapshot inside,
        int left, int top, int right, int bottom)
    {
        RenderLayer[] layers = [.. basis.Layers, new WindowedSceneRenderLayer(inside, left, top, right, bottom)];
        return new(basis.Memory, layers, basis.ObjectSelection, basis.Brightness);
    }
}
