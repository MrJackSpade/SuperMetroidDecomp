using SuperMetroid.Core.Input;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>How the player's input moves through the saved game's area and room maps at file select, as it does without the game's debug mode.</summary>
/// <remarks>
/// Rendering and fades are separate owners. Requests stay pending until the frontend
/// completes the requested transition; calling Step again never implicitly skips it.
/// </remarks>
public sealed class FileSelectMapNavigation
{
    /// <summary>The saved Zebes area, which the area map shows: without the game's debug mode its directions never change areas.</summary>
    private readonly ZebesArea area;
    /// <summary>The controller as the map last read it, whose newly pressed buttons confirm and cancel.</summary>
    private readonly ControllerInputState controller = new();
    /// <summary>The installed world-map label positions the room map's window opens from.</summary>
    private SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelPositions labels;
    /// <summary>Rebinds room-map labels before a pending window is constructed.</summary>
    internal void BindLabels(SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelPositions content) => labels = Ensure.NotNull(content);

    /// <summary>Creates the area and room maps' navigation for a saved area.</summary>
    /// <param name="area">The saved area, one of the six Zebes areas.</param>
    /// <param name="labels">The installed world-map label positions the room map's window opens from.</param>
    /// <param name="initialHeldInput">Controller state to latch so already-held buttons do not create edges.</param>
    public FileSelectMapNavigation(ZebesArea area, SuperMetroid.Core.Frontend.Map.Artwork.WorldMapLabelPositions labels,
        ControllerButton initialHeldInput)
    {
        this.labels = Ensure.NotNull(labels);
        this.area = area;
        controller.Latch(initialHeldInput);
    }

    /// <summary>Gets the current mutually exclusive navigation or transition-request phase.</summary>
    public FileSelectMapNavigationPhase Phase { get; private set; } = FileSelectMapNavigationPhase.Area;

    /// <summary>Gets the room-map window after preparation begins, or <see langword="null"/> on the area map.</summary>
    public FileSelectMapWindow? Window { get; private set; }
    /// <summary>The map window, which exists once the window phase starts.</summary>
    public FileSelectMapWindow ActiveWindow => Window ??
        throw new InvalidOperationException("The file-select map window has not been prepared.");
    /// <summary>Entry/fade owners consume controller samples without executing menu actions.</summary>
    public void LatchInputWithoutNavigation(ControllerButton heldInput) => controller.Latch(heldInput);

    /// <summary>
    /// Consumes one held-controller sample. Edges are latched even during transitions:
    /// pressing Confirm while the window expands must not queue a future room confirmation.
    /// </summary>
    public void Step(ControllerButton heldInput)
    {
        controller.Latch(heldInput);
        ControllerButton pressed = controller.NewlyPressed;
        bool cancel = (pressed & ControllerButton.B) != ControllerButton.None;
        bool confirm = (pressed & (ControllerButton.Start | ControllerButton.A)) != ControllerButton.None;
        if (Phase == FileSelectMapNavigationPhase.Area)
        {
            // The area map changes areas only with the game's debug mode enabled. Without it, a newly pressed
            // direction or Select discards the update's input, a confirm pressed with it included.
            if ((pressed & (ControllerButton.Up | ControllerButton.Down | ControllerButton.Left |
                    ControllerButton.Right | ControllerButton.Select)) != ControllerButton.None) return;
            // Otherwise B wins over A/Start, and selecting keeps the saved area.
            if (cancel) Phase = FileSelectMapNavigationPhase.OptionsRequested;
            else if (confirm) Phase = FileSelectMapNavigationPhase.PreparingWindow;
        }
        else if (Phase == FileSelectMapNavigationPhase.PreparingWindow)
        {
            Window = new FileSelectMapWindow(area, labels);
            Phase = FileSelectMapNavigationPhase.ExpandingWindow;
        }
        else if (Phase == FileSelectMapNavigationPhase.ExpandingWindow)
        {
            if (ActiveWindow.Step()) Phase = FileSelectMapNavigationPhase.InitializingRoom;
        }
        else if (Phase == FileSelectMapNavigationPhase.InitializingRoom)
        {
            // The room map is installed after the expanding-window endpoint;
            // the endpoint itself still shows only the frame, not room-map cells.
            Phase = FileSelectMapNavigationPhase.Room;
        }
        else if (Phase == FileSelectMapNavigationPhase.Room)
        {
            if (cancel) Phase = FileSelectMapNavigationPhase.AreaReturnRequested;
            else if (confirm) Phase = FileSelectMapNavigationPhase.LoadRequested;
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
