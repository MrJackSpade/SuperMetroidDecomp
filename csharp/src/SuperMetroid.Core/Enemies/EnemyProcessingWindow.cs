using SuperMetroid.Core.Enemies.MorphBallEye;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// Selects the enemies EnemyMain processes this frame: the processing window, on-screen tests, and the active and
/// interactive native index lists.
/// </summary>
internal sealed class EnemyProcessingWindow
{
    /// <summary>Frame policy that can force off-screen enemies into the active list.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>HDMA behavior stepped before enemy selection in the main-loop prologue.</summary>
    private readonly MorphBallEyeBehavior _morphBallEyeBehavior;
    /// <summary>Room load guard for selection against a valid enemy population.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Physical slots traversed in native order while choosing active enemies.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds processing-window decisions to one room's frame policy and enemy pool.</summary>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, MorphBallEyeBehavior morphBallEyeBehavior, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _frameContext = frameContext;
        _morphBallEyeBehavior = morphBallEyeBehavior;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Native indexes scheduled for AI this frame, including forced off-screen actors.</summary>
    internal readonly List<ushort> _activeEnemyIndexes = new();
    /// <summary>Active native indexes still eligible for Samus interaction and grapple endpoint scans.</summary>
    internal readonly List<ushort> _interactiveEnemyIndexes = new();

    /// <summary>
    /// Runs the bank-$88 morph-ball eye beam HDMA object, then
    /// <c>Determine_Which_Enemies_to_Process</c> ($A0:8EB6).
    /// </summary>
    /// <remarks>
    /// Game state eight calls $A0:8EB6 first ($82:8B47), before Samus's handlers. The grapple
    /// endpoint scan therefore sees this frame's interactive list, including an actor that
    /// `$86:EF10` respawned at the end of the previous frame. The HDMA objects run earlier
    /// still, in the main-loop prologue: that order is observable on spawn (one-frame pending
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
        _activeEnemyIndexes.Clear();
        _interactiveEnemyIndexes.Clear();
        foreach (RoomEnemySlot slot in _slotTable._slots)
        {
            if (slot.EnemyDefinitionPointer is 0 or EnemyDefinitionId.Respawn)
                continue;
            if (slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                slot.EnemyDefinitionPointer = 0;
                continue;
            }

            bool active = _frameContext._processAllEnemies ||
                slot.Properties.HasAny(EnemyProperties.ProcessOffScreen) ||
                (slot.AiHandlerBits & EnemyAiHandlerMasks.Frozen) != 0 ||
                EnemyIsWithinProcessingWindow(slot, camera);
            if (!active)
                continue;

            _activeEnemyIndexes.Add(slot.NativeIndex);
            if (!slot.Properties.HasAny(EnemyProperties.IgnoreSamusCollision))
                _interactiveEnemyIndexes.Add(slot.NativeIndex);
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
        !EnemyMath.IsNegative16(camera.X + 0x0100 - slot.Position.X.Whole) &&
        !EnemyMath.IsNegative16(slot.Position.Y.Whole - camera.Y) &&
        !EnemyMath.IsNegative16(camera.Y + 0x0100 - slot.Position.Y.Whole);

    /// <summary>
    /// <c>CheckIfEnemyIsHorizontallyOffScreen</c> ($A0:C18E): a negative X, a right edge
    /// left of the camera, or a position at least $100 right of the camera is off-screen.
    /// </summary>
    internal static bool EnemyIsHorizontallyOffScreen(RoomEnemySlot slot, ushort cameraX)
    {
        if (EnemyMath.IsNegative16(slot.Position.X.Whole))
            return true;
        ushort fromCamera = unchecked((ushort)(slot.Position.X.Whole + slot.Radius.X - cameraX));
        return EnemyMath.IsNegative16(fromCamera) || !EnemyMath.IsNegative16(fromCamera - 0x100 - slot.Radius.X);
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
