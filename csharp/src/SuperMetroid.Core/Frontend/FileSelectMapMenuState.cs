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
public sealed partial class FileSelectMapMenuState
{
    /// <summary>CPU address space used to initialize map data and resolve station destinations.</summary>
    private readonly ISnesAddressSpace bus;
    /// <summary>Audio owner receiving map navigation and animation sound requests.</summary>
    private readonly CartridgeAudioState audio;
    /// <summary>Selected Zebes area whose station map and room transitions are displayed.</summary>
    private readonly int area;
    /// <summary>Native used-save-station bitfields for all areas, copied from the selected save slot.</summary>
    private readonly ushort[] usedStations = new ushort[FileSelectMapRomData.AreaCount];
    /// <summary>Compiled area-map graphics and palettes for the selected area.</summary>
    private readonly FileSelectAreaMapGraphics areaGraphics;
    /// <summary>Compiled room-map graphics, collision metadata, and PPU state for the selected area.</summary>
    private readonly FileSelectRoomMapGraphics roomGraphics;
    /// <summary>Current room-map scroll position and limits anchored to the saved station.</summary>
    private FileSelectMapScroll scroll;
    /// <summary>Draws and animates the selected save-station marker in room view.</summary>
    private FileSelectStationMarker marker;
    /// <summary>Native save-station index used as the initial map anchor and load destination.</summary>
    private readonly ushort stationIndex;
    /// <summary>Whether the marker has already been advanced for the current room frame.</summary>
    private bool markerDrawn;
    /// <summary>Arrow and palette-cycle animation state for room-map navigation.</summary>
    private readonly FileSelectMapAnimations animations;
    /// <summary>Whether directional scrolling arrows should be composed in the current room phase.</summary>
    private bool drawArrows;
    /// <summary>Input and window-transition owner for area, room, return, and load navigation.</summary>
    private readonly FileSelectMapNavigation navigation;
    /// <summary>Initial entry reveal and fade owner, which completes before navigation begins.</summary>
    private readonly FileSelectMapEntry entry;
    /// <summary>Optional clipped room-to-area return window while the return transition is underway.</summary>
    private FileSelectMapWindow? returnWindow;
    /// <summary>Phase-local delay counter for return setup and load handoff timing.</summary>
    private int pendingFrames;
    /// <summary>Current master brightness applied after menu-layer composition.</summary>
    private byte brightness = 15;
    /// <summary>Optional installed map artwork and layout catalog rebound after state restoration.</summary>
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
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        this.audio = audio ?? throw new ArgumentNullException(nameof(audio));
        ArgumentNullException.ThrowIfNull(slot);
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

    /// <summary>Creates room-map scrolling state using the current area graphics and saved-station anchor.</summary>
    /// <returns>Scroll owner configured for the selected area's map bounds.</returns>
    /// <exception cref="InvalidOperationException">The map presentation catalog is not installed.</exception>
    private FileSelectMapScroll CreateScrollForCurrentContent()
    {
        var typedArea = (AreaId)area;
        AreaMapPresentationCatalog catalog = mapPresentation ?? throw new InvalidOperationException(
            "File-select map requires installed map presentation assets.");
        var anchor = FileSelectMapLoadAnchors.Get(typedArea, stationIndex);
        return new FileSelectMapScroll(bus, catalog.Get(typedArea), roomGraphics.MapSystem, anchor.X, anchor.Y);
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
        }
    }

    // Final frame for composed phases; a returned frame is valid until this menu renders again.
    /// <summary>Reusable composed output buffer overwritten by the next render.</summary>
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
            FileSelectMapNavigationPhase.Room when !markerDrawn => roomGraphics.RenderBackgrounds(scroll.Horizontal, scroll.Vertical),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested =>
                roomGraphics.Render(scroll.Horizontal, scroll.Vertical, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 =>
                roomGraphics.Render(scroll.Horizontal, scroll.Vertical, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapRomData.ReturnSetupFrames - 1 =>
                FileSelectMapWindowCompositor.CompositeInitialEntryWindow(
                    areaGraphics.Render(usedStations), roomGraphics.RenderFrameOnly(), composed),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null =>
                // Return setup calls $81:A5B3 (CGADSUB=$25); unlike the forward
                // expansion's $05, this retains backdrop addition outside the window.
                FileSelectMapWindowCompositor.Composite(areaGraphics.Render(usedStations),
                    roomGraphics.RenderFrameOnly(), returnWindow, composed),
            FileSelectMapNavigationPhase.AreaReturnRequested => roomGraphics.RenderFrameOnly(),
            _ => areaGraphics.Render(usedStations),
        };
        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    /// <summary>Queues a map-menu sound through the shared cartridge audio owner.</summary>
    /// <param name="sound">Sound identity requested by the current navigation or animation transition.</param>
    private void Queue(SoundEffectId sound) => audio.QueueSound(sound, maximumQueued: 6);
}
