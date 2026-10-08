using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;

namespace SuperMetroid.Core.Frontend;

/// <summary>Normal (non-debug) input ownership for the saved-game area/room-map sequence.</summary>
/// <remarks>
/// Rendering and fades are separate owners. Requests stay pending until the frontend
/// completes the requested transition; calling Step again never implicitly skips it.
/// </remarks>
public sealed class FileSelectMapNavigation
{
    private readonly ISnesAddressSpace bus;
    private readonly int area;
    private ushort previousInput;
    [NonSerialized] private SuperMetroid.Core.Assets.WorldMapLabelLayout? labels;
    internal void BindLabels(SuperMetroid.Core.Assets.WorldMapLabelLayout? content) => labels = content;

    /// <summary>Creates normal area/room-map navigation for a saved area's native index.</summary>
    /// <param name="bus">Installed address space used when constructing the room-map window.</param>
    /// <param name="area">Zero-based saved area index.</param>
    /// <param name="initialHeldInput">Controller state to latch so already-held buttons do not create edges.</param>
    public FileSelectMapNavigation(ISnesAddressSpace bus, int area, ushort initialHeldInput = 0)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        this.area = area;
        previousInput = initialHeldInput;
    }

    /// <summary>Gets the current mutually exclusive navigation or transition-request phase.</summary>
    public FileSelectMapNavigationPhase Phase { get; private set; } = FileSelectMapNavigationPhase.Area;

    /// <summary>Gets the room-map window after preparation begins, or <see langword="null"/> on the area map.</summary>
    public FileSelectMapWindow? Window { get; private set; }
    /// <summary>Entry/fade owners consume controller samples without executing menu actions.</summary>
    public void LatchInputWithoutNavigation(ushort heldInput) => previousInput = heldInput;

    /// <summary>
    /// Consumes one held-controller sample. Edges are latched even during transitions:
    /// pressing Confirm while the window expands must not queue a future room confirmation.
    /// </summary>
    public void Step(ushort heldInput)
    {
        ushort pressed = (ushort)(heldInput & ~previousInput);
        previousInput = heldInput;
        bool cancel = (pressed & (ushort)SnesButton.B) != 0;
        bool confirm = (pressed & (ushort)(SnesButton.Start | SnesButton.A)) != 0;
        switch (Phase)
        {
            case FileSelectMapNavigationPhase.Area:
                // $81:A800's direction handling changes areas only with enable_debug.
                // Its non-debug branch replaces the working input word with zero when
                // a direction/Select edge exists, also suppressing a simultaneous confirm.
                if ((pressed & (ushort)(SnesButton.Up | SnesButton.Down | SnesButton.Left |
                        SnesButton.Right | SnesButton.Select)) != 0) break;
                // Otherwise B wins over A/Start, and selecting keeps the SRAM area.
                if (cancel) Phase = FileSelectMapNavigationPhase.OptionsRequested;
                else if (confirm) Phase = FileSelectMapNavigationPhase.PreparingWindow;
                break;
            case FileSelectMapNavigationPhase.PreparingWindow:
                Window = new FileSelectMapWindow(bus, area, labels);
                Phase = FileSelectMapNavigationPhase.ExpandingWindow;
                break;
            case FileSelectMapNavigationPhase.ExpandingWindow:
                if (Window!.Step()) Phase = FileSelectMapNavigationPhase.InitializingRoom;
                break;
            case FileSelectMapNavigationPhase.InitializingRoom:
                // $81:AD17 installs the room map after the expanding-window endpoint;
                // the endpoint itself still shows only the frame, not room-map cells.
                Phase = FileSelectMapNavigationPhase.Room;
                break;
            case FileSelectMapNavigationPhase.Room:
                if (cancel) Phase = FileSelectMapNavigationPhase.AreaReturnRequested;
                else if (confirm) Phase = FileSelectMapNavigationPhase.LoadRequested;
                break;
            case FileSelectMapNavigationPhase.AreaReturnRequested:
            case FileSelectMapNavigationPhase.OptionsRequested:
            case FileSelectMapNavigationPhase.LoadRequested:
                break;
            default:
                throw new InvalidOperationException($"Unknown map navigation phase {Phase}.");
        }
    }

    /// <summary>Called after the frontend finishes $81:AF97's return-to-area sequence.</summary>
    public void CompleteAreaReturn()
    {
        if (Phase != FileSelectMapNavigationPhase.AreaReturnRequested)
            throw new InvalidOperationException("No return-to-area transition is pending.");
        Window = null;
        Phase = FileSelectMapNavigationPhase.Area;
    }
}

/// <summary>Exclusive navigation phases; these names are not cartridge dispatcher addresses.</summary>
public enum FileSelectMapNavigationPhase
{
    /// <summary>Accepts area-map confirm or cancel edges.</summary>
    Area,
    /// <summary>Creates the selected area's room-map window.</summary>
    PreparingWindow,
    /// <summary>Advances the window-opening animation.</summary>
    ExpandingWindow,
    /// <summary>Installs the room-map contents after the frame reaches its endpoint.</summary>
    InitializingRoom,
    /// <summary>Accepts room-map load or return edges.</summary>
    Room,
    /// <summary>Waits for the frontend to finish returning to the area map.</summary>
    AreaReturnRequested,
    /// <summary>Requests a transition back to the file-options menu.</summary>
    OptionsRequested,
    /// <summary>Requests loading the selected save file.</summary>
    LoadRequested,
    /// <summary>Represents the frontend-owned transition into an area map.</summary>
    EnteringArea,
}
