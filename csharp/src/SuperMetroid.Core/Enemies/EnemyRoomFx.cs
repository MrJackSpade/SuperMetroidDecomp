using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room's liquid effect, which enemy AI drives directly: liquid motion commands from Ridley and the Chozo statue.</summary>
[RoomLifetime]
internal sealed class EnemyRoomFx
{
    /// <summary>The current room's effect state, bound when the room loads.</summary>
    internal RoomFxOverlayState? FxOverlay { get; private set; }

    /// <summary>
    /// Binds the room's effect state before gameplay and when a saved game state is restored.
    /// A lone enemy without a room leaves it unbound.
    /// </summary>
    internal void BindRoomFx(RoomFxOverlayState roomFx) => FxOverlay = roomFx;

    /// <summary>Applies a liquid target, packed velocity, and delay at an AI command boundary so the FX timer is not reset each frame.</summary>
    internal void PublishLiquidMotion(int target, int velocity, int delay) =>
        // Execute at the AI command, not from retained state each frame:
        // repeated application would continually reset the liquid handler's delay.
        FxOverlay?.SetLiquidMotion(targetYPosition: target, packedYVelocity: velocity, timer: delay);
}
