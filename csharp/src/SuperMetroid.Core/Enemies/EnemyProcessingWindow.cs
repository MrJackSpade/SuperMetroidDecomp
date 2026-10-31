using SuperMetroid.Core.Enemies.MorphBallEye;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Selects the enemies EnemyMain processes this frame: the processing window, on-screen tests, and the active and
/// interactive enemies, each in <see cref="EnemySlotOrder"/>.
/// </summary>
[RoomLifetime]
internal sealed class EnemyProcessingWindow
{
    /// <summary>Frame policy that can force off-screen enemies into the active list.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Per-line display effect stepped before enemy selection at the start of the update.</summary>
    private readonly MorphBallEyeBehavior _morphBallEyeBehavior;
    /// <summary>Room load guard for selection against a valid enemy population.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>The slots traversed, in slot order, while choosing active enemies.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds processing-window decisions to one room's frame policy and enemy pool.</summary>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, MorphBallEyeBehavior morphBallEyeBehavior, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _frameContext = frameContext;
        _morphBallEyeBehavior = morphBallEyeBehavior;
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
    /// Runs the morph-ball eye's beam effect, then
    /// <c>Determine_Which_Enemies_to_Process</c> ($A0:8EB6).
    /// </summary>
    /// <remarks>
    /// Game state eight calls $A0:8EB6 first ($82:8B47), before Samus's handlers. The grapple
    /// endpoint scan therefore sees this frame's interactive list, including an actor that
    /// `$86:EF10` respawned at the end of the previous frame. The per-line display effects run
    /// earlier still, at the start of the update: that order is observable on spawn (one-frame pending
    /// initialization) and shutdown (the full beam sees the body's cleared activation word on
    /// the following frame).
    /// </remarks>
    /// <param name="camera">Layer-one camera position (viewport top-left) in whole room pixels.</param>
    public void PrepareEnemyProcessingList(Position16<RoomPixels> camera)
    {
        _room.EnsureLoaded();
        _morphBallEyeBehavior.StepMorphBallEyeBeam();
        DetermineWhichEnemiesToProcess(camera);
    }

    /// <summary>Rebuilds active and interactive lists after clearing deleted slots and applying off-screen policy.</summary>
    internal void DetermineWhichEnemiesToProcess(Position16<RoomPixels> camera)
    {
        _activeEnemies.Clear();
        _interactiveEnemies.Clear();
        foreach (RoomEnemySlot slot in _slotTable.Slots)
        {
            if (slot.DefinitionId is 0 or EnemyDefinitionId.Respawn)
                continue;
            if (slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.Unbind();
                continue;
            }

            bool active = _frameContext.ProcessAllEnemies ||
                slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
                (slot.AiStates & EnemyAiStates.Frozen) != EnemyAiStates.None ||
                EnemyIsWithinProcessingWindow(slot, camera);
            if (!active)
                continue;

            _activeEnemies.Add(slot);
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
                _interactiveEnemies.Add(slot);
        }
    }

    /// <summary>Applies native radius-aware horizontal and asymmetric vertical bounds around the camera.</summary>
    private static bool EnemyIsWithinProcessingWindow(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        !EnemyMath.IsNegative16(slot.Radius.X + slot.Position.X.Whole - camera.X) &&
        !EnemyMath.IsNegative16(slot.Radius.X + camera.X + 256 - slot.Position.X.Whole) &&
        !EnemyMath.IsNegative16(slot.Position.Y.Whole + 8 - camera.Y) &&
        !EnemyMath.IsNegative16(camera.Y + 248 - slot.Position.Y.Whole);

    /// <summary>
    /// <c>CheckIfEnemyCenterIsOnScreen</c> ($A0:AD70): the center lies within $100 pixels
    /// right of and below the camera on both axes.
    /// </summary>
    internal static bool EnemyCenterIsOnScreen(RoomEnemySlot slot, Position16<RoomPixels> camera) =>
        !EnemyMath.IsNegative16(slot.Position.X.Whole - camera.X) &&
        !EnemyMath.IsNegative16(camera.X + 256 - slot.Position.X.Whole) &&
        !EnemyMath.IsNegative16(slot.Position.Y.Whole - camera.Y) &&
        !EnemyMath.IsNegative16(camera.Y + 256 - slot.Position.Y.Whole);

    /// <summary>
    /// <c>CheckIfEnemyIsHorizontallyOffScreen</c> ($A0:C18E): a negative X, a right edge
    /// left of the camera, or a position at least $100 right of the camera is off-screen.
    /// </summary>
    internal static bool EnemyIsHorizontallyOffScreen(RoomEnemySlot slot, int cameraX)
    {
        if (EnemyMath.IsNegative16(slot.Position.X.Whole))
            return true;
        int fromCamera = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - cameraX));
        return EnemyMath.IsNegative16(fromCamera) || !EnemyMath.IsNegative16(fromCamera - 256 - slot.Radius.X);
    }

    /// <summary>Uses the normal-sprite processing window to determine whether all bounds reject a slot.</summary>
    internal static bool EnemyWithNormalSpritesIsOffScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        EnemyMath.IsNegative16(slot.Radius.X + slot.Position.X.Whole - camera.X) ||
        EnemyMath.IsNegative16(slot.Radius.X + camera.X + 256 - slot.Position.X.Whole) ||
        EnemyMath.IsNegative16(slot.Position.Y.Whole + 8 - camera.Y) ||
        EnemyMath.IsNegative16(camera.Y + 248 - slot.Position.Y.Whole);
}
