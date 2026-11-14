using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>The room's liquid effect, which enemy AI drives directly: liquid motion requests from Ridley and the Chozo statue.</summary>
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

    /// <summary>Applies a liquid target, velocity, and delay when the AI requests it, so the FX timer is not reset each frame.</summary>
    internal void PublishLiquidMotion(int target, SignedEightEight velocity, int delay) =>
        // Apply it when the AI requests it, not from retained state each frame:
        // repeated application would continually reset the liquid's rise delay.
        FxOverlay?.SetLiquidMotion(targetLevel: target, levelSpeed: velocity, moveDelay: delay);
}
