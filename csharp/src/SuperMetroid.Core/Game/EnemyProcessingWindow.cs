using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using static SuperMetroid.Core.Hardware.SnesAddressMath;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Selects the enemies EnemyMain processes this frame: the processing window, on-screen tests, and the active and
/// interactive native index lists.
/// </summary>
internal sealed class EnemyProcessingWindow
{
    /// <summary>Stores frame context.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Stores morph ball eye behavior.</summary>
    private readonly MorphBallEyeBehavior _morphBallEyeBehavior;
    /// <summary>Stores room.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Stores slot table.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Initializes a new enemy processing window instance.</summary>
    internal EnemyProcessingWindow(EnemyFrameContext frameContext, MorphBallEyeBehavior morphBallEyeBehavior, EnemyRoomBinding room, EnemySlotTable slotTable)
    {
        _frameContext = frameContext;
        _morphBallEyeBehavior = morphBallEyeBehavior;
        _room = room;
        _slotTable = slotTable;
    }
    /// <summary>Stores active enemy indexes.</summary>
    internal readonly List<ushort> _activeEnemyIndexes = new();
    /// <summary>Stores interactive enemy indexes.</summary>
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

    /// <summary>determine which enemies to process.</summary>
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

    /// <summary>enemy is within processing window.</summary>
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

    /// <summary>enemy with normal sprites is off screen.</summary>
    internal static bool EnemyWithNormalSpritesIsOffScreen(
        RoomEnemySlot slot,
        Position16<RoomPixels> camera) =>
        EnemyMath.IsNegative16(slot.Radius.X + slot.Position.X.Whole - camera.X) ||
        EnemyMath.IsNegative16(slot.Radius.X + camera.X + 256 - slot.Position.X.Whole) ||
        EnemyMath.IsNegative16(slot.Position.Y.Whole + 8 - camera.Y) ||
        EnemyMath.IsNegative16(camera.Y + 248 - slot.Position.Y.Whole);
}
