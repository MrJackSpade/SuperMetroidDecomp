using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Selects the enemies the enemy pass processes this frame: the processing window, on-screen tests, and the active and
/// interactive enemies, each in <see cref="EnemySlotOrder"/>.
/// </summary>
[RoomLifetime]
internal sealed class EnemyProcessingWindow
{
    /// <summary>Frame policy that can force off-screen enemies into the active list.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>The species' work stepped at the start of the update, before enemy selection.</summary>
    private readonly IStepBeforeEnemySelection _stepBeforeSelection;
    /// <summary>Room load guard for selection against a valid enemy population.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The slots traversed, in slot order, while choosing active enemies.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds processing-window decisions to one room's frame policy and enemy pool.</summary>
    /// <param name="frameContext">The frame policy that can force off-screen enemies into the active list.</param>
    /// <param name="stepBeforeSelection">The species' work that steps at the start of the update, before selection.</param>
    /// <param name="room">The room load guard.</param>
    /// <param name="slotTable">The room's enemy places.</param>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, IStepBeforeEnemySelection stepBeforeSelection, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _frameContext = frameContext;
        _stepBeforeSelection = stepBeforeSelection;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>The enemies scheduled for AI this frame, including forced off-screen actors, in slot order.</summary>
    private readonly List<RoomEnemySlot> _activeEnemies = new();
    /// <summary>The active enemies still eligible for Samus interaction and grapple endpoint scans, in slot order.</summary>
    private readonly List<RoomEnemySlot> _interactiveEnemies = new();

    /// <summary>The enemies scheduled for AI this frame, in slot order.</summary>
    internal IReadOnlyList<RoomEnemySlot> ActiveEnemies => _activeEnemies;

    /// <summary>The active enemies still eligible for Samus interaction, in slot order.</summary>
    internal IReadOnlyList<RoomEnemySlot> InteractiveEnemies => _interactiveEnemies;

    /// <summary>
    /// Steps the species' work that runs before selection (the Morph Ball eye's beam), then decides which enemies to process.
    /// </summary>
    /// <remarks>
    /// Main gameplay builds the window first, before Samus's handlers. The grapple
    /// endpoint scan therefore sees this frame's interactive list, including an actor that
    /// a death explosion respawned at the end of the previous frame. The per-line display effects run
    /// earlier still, at the start of the update: that order is observable on spawn (one-frame pending
    /// initialization) and shutdown (the full beam sees the body's cleared activation flag on
    /// the following frame).
    /// </remarks>
    /// <param name="camera">Camera position (viewport top-left) in whole room pixels.</param>
    public void PrepareEnemyProcessingList(Position16<RoomPixels> camera)
    {
        _room.EnsureLoaded();
        _stepBeforeSelection.StepBeforeEnemySelection();
        DetermineWhichEnemiesToProcess(camera);
    }

    /// <summary>Rebuilds active and interactive lists after clearing deleted slots and applying off-screen policy.</summary>
    internal void DetermineWhichEnemiesToProcess(Position16<RoomPixels> camera)
    {
        _activeEnemies.Clear();
        _interactiveEnemies.Clear();
        foreach (RoomEnemySlot slot in _slotTable.Slots)
        {
            // A place with no enemy, one awaiting its enemy's respawn among them, is not processed.
            if (slot.DefinitionId == EnemyDefinitionId.None)
                continue;
            if (slot.Presence.HasAny(EnemyPresence.Deleted))
            {
                slot.Unbind();
                continue;
            }

            bool active = _frameContext.ProcessAllEnemies ||
                slot.Processing.HasAny(EnemyProcessing.OffScreen) ||
                (slot.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None ||
                IsWithinProcessingWindow(slot, camera);
            if (!active)
                continue;

            _activeEnemies.Add(slot);
            if (!slot.Contact.HasAny(EnemyContact.IgnoresSamus))
                _interactiveEnemies.Add(slot);
        }
    }

    /// <summary>
    /// Whether the enemy lies within the processing window around the camera: its radius reaches the screen's width from
    /// the camera's left edge, and its centre lies from eight pixels above the camera to 248 pixels below it. The window
    /// selects the enemies an update processes, and also the enemies the draw pass reaches and the enemies Rios and
    /// pipe bugs treat as on screen.
    /// </summary>
    internal static bool IsWithinProcessingWindow(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        !CoordinateAxis.Precedes(slot.Radius.X + slot.Position.X.Whole, camera.X) &&
        !CoordinateAxis.Precedes(slot.Radius.X + camera.X + 256, slot.Position.X.Whole) &&
        !CoordinateAxis.Precedes(slot.Position.Y.Whole + 8, camera.Y) &&
        !CoordinateAxis.Precedes(camera.Y + 248, slot.Position.Y.Whole);

    /// <summary>
    /// The center lies within 256 pixels
    /// right of and below the camera on both axes.
    /// </summary>
    internal static bool EnemyCenterIsOnScreen(RoomEnemySlot slot, Position16<RoomPixels> camera) =>
        !CoordinateAxis.Precedes(slot.Position.X.Whole, camera.X) &&
        !CoordinateAxis.Precedes(camera.X + 256, slot.Position.X.Whole) &&
        !CoordinateAxis.Precedes(slot.Position.Y.Whole, camera.Y) &&
        !CoordinateAxis.Precedes(camera.Y + 256, slot.Position.Y.Whole);

    /// <summary>
    /// A negative X, a right edge
    /// left of the camera, or a position at least 256 pixels right of the camera is off-screen.
    /// </summary>
    internal static bool EnemyIsHorizontallyOffScreen(RoomEnemySlot slot, int cameraX)
    {
        if (CoordinateAxis.AsSigned(slot.Position.X.Whole) < 0)
            return true;
        int fromCamera = CoordinateAxis.Wrap(slot.Position.X.Whole + slot.Radius.X - cameraX);
        return CoordinateAxis.AsSigned(fromCamera) < 0 || !CoordinateAxis.Precedes(fromCamera, 256 + slot.Radius.X);
    }
}
