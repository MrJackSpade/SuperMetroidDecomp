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

    public FileSelectMapNavigation(ISnesAddressSpace bus, int area, ushort initialHeldInput = 0)
    {
        this.bus = bus ?? throw new ArgumentNullException(nameof(bus));
        if ((uint)area >= FileSelectMapRomData.AreaCount)
            throw new ArgumentOutOfRangeException(nameof(area));
        this.area = area;
        previousInput = initialHeldInput;
    }

    public FileSelectMapNavigationPhase Phase { get; private set; } = FileSelectMapNavigationPhase.Area;
    public FileSelectMapWindow? Window { get; private set; }

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
                Window = new FileSelectMapWindow(bus, area);
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
    Area,
    PreparingWindow,
    ExpandingWindow,
    InitializingRoom,
    Room,
    AreaReturnRequested,
    OptionsRequested,
    LoadRequested,
}
