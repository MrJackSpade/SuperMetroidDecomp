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
    private readonly ISnesAddressSpace bus;
    private readonly CartridgeAudioState audio;
    private readonly int area;
    private readonly ushort[] usedStations = new ushort[FileSelectMapRomData.AreaCount];
    private readonly FileSelectAreaMapGraphics areaGraphics;
    private readonly FileSelectRoomMapGraphics roomGraphics;
    private FileSelectMapScroll scroll;
    private FileSelectStationMarker marker;
    private readonly Func<FileSelectMapScroll> createScroll;
    private readonly ushort stationIndex;
    private bool markerDrawn;
    private readonly FileSelectMapAnimations animations;
    private bool drawArrows;
    private readonly FileSelectMapNavigation navigation;
    private readonly FileSelectMapEntry entry;
    private FileSelectMapWindow? returnWindow;
    private int pendingFrames;
    private byte brightness = 15;

    public FileSelectMapMenuState(ISnesAddressSpace bus, CartridgeAudioState audio,
        SuperMetroidSaveSlot slot, ushort initialHeldInput)
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
        LoadStationEntry station = LoadStationEntry.Load(bus, typedArea, checked((byte)slot.SaveStation));
        CartridgeRoomHeader room = CartridgeRoomHeader.Load(bus, station.RoomPointer);
        areaGraphics = new FileSelectAreaMapGraphics(bus, area);
        roomGraphics = new FileSelectRoomMapGraphics(bus, system, typedArea);
        createScroll = () => new FileSelectMapScroll(bus, AreaMapRomData.Load(bus, typedArea), system,
            (ushort)(8 * (room.MapX + (station.SamusX >> 8))),
            (ushort)(8 * (room.MapY + (station.SamusY >> 8) + 1)));
        scroll = createScroll();
        marker = new FileSelectStationMarker(bus, typedArea, slot.SaveStation);
        navigation = new FileSelectMapNavigation(bus, area, initialHeldInput);
        animations = new FileSelectMapAnimations(bus);
        entry = new FileSelectMapEntry(bus);
    }

    public FileSelectMapNavigationPhase Phase => entry.IsComplete ? navigation.Phase : FileSelectMapNavigationPhase.EnteringArea;
    public bool LoadRequested { get; private set; }
    public bool OptionsRequested { get; private set; }

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
                scroll = createScroll();
                marker = new FileSelectStationMarker(bus, (AreaId)area, stationIndex);
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
                    returnWindow = FileSelectMapWindow.CreateReturn(bus, area);
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

    public Rgba32[] Render()
    {
        if (!entry.IsComplete) return entry.Render(areaGraphics.Render(usedStations));
        Rgba32[] pixels = Phase switch
        {
            FileSelectMapNavigationPhase.ExpandingWindow or FileSelectMapNavigationPhase.InitializingRoom =>
                FileSelectMapWindowCompositor.Composite(areaGraphics.Render(usedStations, false),
                    roomGraphics.RenderFrameOnly(), navigation.Window!),
            FileSelectMapNavigationPhase.Room when !markerDrawn => roomGraphics.RenderBackgrounds(scroll.Horizontal, scroll.Vertical),
            FileSelectMapNavigationPhase.Room or FileSelectMapNavigationPhase.LoadRequested =>
                roomGraphics.Render(scroll.Horizontal, scroll.Vertical, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == 0 =>
                roomGraphics.Render(scroll.Horizontal, scroll.Vertical, marker, drawArrows ? animations : null),
            FileSelectMapNavigationPhase.AreaReturnRequested when pendingFrames == FileSelectMapRomData.ReturnSetupFrames - 1 =>
                FileSelectMapWindowCompositor.CompositeInitialEntryWindow(
                    areaGraphics.Render(usedStations), roomGraphics.RenderFrameOnly()),
            FileSelectMapNavigationPhase.AreaReturnRequested when returnWindow is not null =>
                // Return setup calls $81:A5B3 (CGADSUB=$25); unlike the forward
                // expansion's $05, this retains backdrop addition outside the window.
                FileSelectMapWindowCompositor.Composite(areaGraphics.Render(usedStations),
                    roomGraphics.RenderFrameOnly(), returnWindow),
            FileSelectMapNavigationPhase.AreaReturnRequested => roomGraphics.RenderFrameOnly(),
            _ => areaGraphics.Render(usedStations),
        };
        MasterBrightnessFilter.Apply(pixels, brightness);
        return pixels;
    }

    private void Queue(SoundEffectId sound) => audio.QueueSound(sound, maximumQueued: 6);
}
