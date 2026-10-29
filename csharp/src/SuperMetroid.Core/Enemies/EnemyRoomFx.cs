using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room's liquid effect, which enemy AI drives directly: liquid motion commands from Ridley and the Chozo statue.</summary>
[RoomLifetime]
internal sealed class EnemyRoomFx
{
    /// <summary>Current room's effect state; excluded from serialized enemy state and rebound after restoration.</summary>
    [field: NonSerialized] internal RoomLayer3FxState? Layer3Fx { get; private set; }

    /// <summary>
    /// Binds the room's effect state before gameplay and after debugger restoration. Isolated
    /// enemy audits may omit this owner and inspect the published diagnostic values.
    /// </summary>
    internal void BindRoomFx(RoomLayer3FxState roomFx) => Layer3Fx = roomFx;

    /// <summary>Applies a liquid target, packed velocity, and delay at an AI command boundary so the FX timer is not reset each frame.</summary>
    internal void PublishLiquidMotion(int target, int velocity, int delay) =>
        // Execute at the native AI command, not from retained state each frame:
        // repeated application would continually reset the liquid handler's delay.
        Layer3Fx?.SetLiquidMotion(targetYPosition: target, packedYVelocity: velocity, timer: delay);
}
