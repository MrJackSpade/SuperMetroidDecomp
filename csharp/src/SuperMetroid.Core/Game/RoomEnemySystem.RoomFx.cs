namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    [NonSerialized] private RoomLayer3FxState? _roomFx;

    /// <summary>
    /// Binds shared FX words before gameplay and after debugger restoration. Isolated
    /// enemy audits may omit this owner and inspect the published diagnostic words.
    /// </summary>
    internal void BindRoomFx(RoomLayer3FxState roomFx) => _roomFx = roomFx;

    private void PublishRidleyLiquidMotion(RidleyEnemyState state, ushort target, ushort velocity, ushort delay)
    {
        state.FxTargetYPosition = target;
        state.FxYSubVelocity = velocity;
        state.FxTimer = delay;
        // Execute at the native AI command, not from retained state each frame:
        // repeated application would continually reset the liquid handler's delay.
        _roomFx?.ApplyCartridgeMotionWrites(targetYPosition: target, packedYVelocity: velocity, timer: delay);
    }
}
