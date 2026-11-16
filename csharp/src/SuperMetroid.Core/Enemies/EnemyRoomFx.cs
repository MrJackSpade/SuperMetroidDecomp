using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The room's liquid effect, which enemy AI drives directly: liquid motion requests from Ridley and the Chozo statue, and
/// the acid surface Crocomire's smoke rises from.
/// </summary>
/// <param name="fxOverlay">The room's effect state, which lives as long as the room and its enemies.</param>
[RoomLifetime]
internal sealed class EnemyRoomFx(RoomFxOverlayState fxOverlay)
{
    /// <summary>The room's effect state.</summary>
    internal RoomFxOverlayState FxOverlay { get; } = Ensure.NotNull(fxOverlay);

    /// <summary>Applies a liquid target, velocity, and delay when the AI requests it, so the FX timer is not reset each frame.</summary>
    internal void PublishLiquidMotion(int target, SignedEightEight velocity, int delay) =>
        // Apply it when the AI requests it, not from retained state each frame:
        // repeated application would continually reset the liquid's rise delay.
        FxOverlay.MoveLiquidTo(target, velocity, delay);
}
