namespace SuperMetroid.Core.Game;

public sealed partial class RoomEnemySystem
{
    /// <summary>Optional shared Layer 3 effect owner receiving instruction-timed cartridge motion writes.</summary>
    [NonSerialized] private RoomLayer3FxState? _roomFx;

    /// <summary>
    /// Binds shared FX words before gameplay and after debugger restoration. Isolated
    /// enemy audits may omit this owner and inspect the published diagnostic words.
    /// </summary>
    internal void BindRoomFx(RoomLayer3FxState roomFx) => _roomFx = roomFx;

    /// <summary>Updates Ridley's retained liquid-motion values and publishes the same write when an FX owner is bound.</summary>
    /// <param name="state">Ridley state receiving the target, packed velocity, and timer values.</param>
    /// <param name="target">Liquid target Y position selected by the current Ridley phase.</param>
    /// <param name="velocity">Packed vertical velocity written to the liquid effect.</param>
    /// <param name="delay">Delay before the liquid handler applies the motion.</param>
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
