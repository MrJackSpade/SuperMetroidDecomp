using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Fx;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room's layer-3 FX words that enemy AI drives directly: liquid motion commands from Ridley and the Chozo statue.</summary>
internal sealed class EnemyRoomFx
{
    /// <summary>Current room's layer-3 FX owner; excluded from serialized enemy state and rebound after restoration.</summary>
    [NonSerialized] internal RoomLayer3FxState? Layer3Fx;

    /// <summary>
    /// Binds shared FX words before gameplay and after debugger restoration. Isolated
    /// enemy audits may omit this owner and inspect the published diagnostic words.
    /// </summary>
    internal void BindRoomFx(RoomLayer3FxState roomFx) => Layer3Fx = roomFx;

    /// <summary>Applies Ridley's liquid target, packed velocity, and delay at the AI command boundary so the FX timer is not reset each frame.</summary>
    internal void PublishRidleyLiquidMotion(RidleyEnemyState state, ushort target, ushort velocity, ushort delay)
    {
        state.FxTargetYPosition = target;
        state.FxYSubVelocity = velocity;
        state.FxTimer = delay;
        // Execute at the native AI command, not from retained state each frame:
        // repeated application would continually reset the liquid handler's delay.
        Layer3Fx?.ApplyCartridgeMotionWrites(targetYPosition: target, packedYVelocity: velocity, timer: delay);
    }
}
