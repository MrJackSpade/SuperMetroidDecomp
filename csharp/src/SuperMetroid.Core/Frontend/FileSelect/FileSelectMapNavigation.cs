using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>Normal (non-debug) input ownership for the saved-game area/room-map sequence.</summary>
/// <remarks>
/// Rendering and fades are separate owners. Requests stay pending until the frontend
/// completes the requested transition; calling Step again never implicitly skips it.
/// </remarks>
public sealed class FileSelectMapNavigation
{
    /// <summary>The saved Zebes area, retained while area-map directions are suppressed in normal mode.</summary>
    private readonly AreaId area;
    /// <summary>The buttons held on the previous update, used to detect new confirm and cancel presses.</summary>
    private ControllerButton previousInput;
    /// <summary>The installed label positions, when bound.</summary>
    private SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelPositions? labels;
    /// <summary>Rebinds room-map labels before a pending window is constructed.</summary>
    internal void BindLabels(SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelPositions? content) => labels = content;

    /// <summary>Creates normal area/room-map navigation for a saved area.</summary>
    /// <param name="area">The saved area, one of the six Zebes areas.</param>
    /// <param name="initialHeldInput">Controller state to latch so already-held buttons do not create edges.</param>
    public FileSelectMapNavigation(AreaId area, ControllerButton initialHeldInput = ControllerButton.None)
    {
        if (!AreaIds.IsZebes(area))
            throw new ArgumentOutOfRangeException(nameof(area), area, "Only the six Zebes areas have world-map selections.");
        this.area = area;
        previousInput = initialHeldInput;
    }

    /// <summary>Gets the current mutually exclusive navigation or transition-request phase.</summary>
    public FileSelectMapNavigationPhase Phase { get; private set; } = FileSelectMapNavigationPhase.Area;

    /// <summary>Gets the room-map window after preparation begins, or <see langword="null"/> on the area map.</summary>
    public FileSelectMapWindow? Window { get; private set; }
    /// <summary>The map window, which exists once the window phase starts.</summary>
    public FileSelectMapWindow ActiveWindow => Window ??
        throw new InvalidOperationException("The file-select map window has not been prepared.");
    /// <summary>Entry/fade owners consume controller samples without executing menu actions.</summary>
    public void LatchInputWithoutNavigation(ControllerButton heldInput) => previousInput = heldInput;

    /// <summary>
    /// Consumes one held-controller sample. Edges are latched even during transitions:
    /// pressing Confirm while the window expands must not queue a future room confirmation.
    /// </summary>
    public void Step(ControllerButton heldInput)
    {
        // Same edge rule as ControllerInputState.NewlyPressedButtons, over the held buttons.
        ControllerButton pressed = heldInput & ~previousInput;
        previousInput = heldInput;
        bool cancel = (pressed & ControllerButton.B) != ControllerButton.None;
        bool confirm = (pressed & (ControllerButton.Start | ControllerButton.A)) != ControllerButton.None;
        switch (Phase)
        {
            case FileSelectMapNavigationPhase.Area:
                // The game's direction handling changes areas only with debug enabled.
                // Its non-debug branch replaces the working input word with zero when
                // a direction/Select edge exists, also suppressing a simultaneous confirm.
                if ((pressed & (ControllerButton.Up | ControllerButton.Down | ControllerButton.Left |
                        ControllerButton.Right | ControllerButton.Select)) != ControllerButton.None) break;
                // Otherwise B wins over A/Start, and selecting keeps the saved area.
                if (cancel) Phase = FileSelectMapNavigationPhase.OptionsRequested;
                else if (confirm) Phase = FileSelectMapNavigationPhase.PreparingWindow;
                break;
            case FileSelectMapNavigationPhase.PreparingWindow:
                Window = new FileSelectMapWindow(area, labels);
                Phase = FileSelectMapNavigationPhase.ExpandingWindow;
                break;
            case FileSelectMapNavigationPhase.ExpandingWindow:
                if (ActiveWindow.Step()) Phase = FileSelectMapNavigationPhase.InitializingRoom;
                break;
            case FileSelectMapNavigationPhase.InitializingRoom:
                // The room map is installed after the expanding-window endpoint;
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
            case FileSelectMapNavigationPhase.EnteringArea:
            default:
                throw new InvalidOperationException($"Unknown map navigation phase {Phase}.");
        }
    }

    /// <summary>Called after the frontend finishes the return-to-area sequence.</summary>
    public void CompleteAreaReturn()
    {
        if (Phase != FileSelectMapNavigationPhase.AreaReturnRequested)
            throw new InvalidOperationException("No return-to-area transition is pending.");
        Window = null;
        Phase = FileSelectMapNavigationPhase.Area;
    }
}
