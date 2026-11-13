using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's liquid commands (#1395).</summary>
internal static class RidleyLiquidMotion
{
    /// <summary>Applies Ridley's liquid target and delay, with the velocity, to the room's liquid.</summary>
    /// <param name="roomFx">The room's liquid.</param>
    /// <param name="target">The liquid's target height.</param>
    /// <param name="velocity">The liquid's velocity.</param>
    /// <param name="delay">The updates before the liquid moves.</param>
    internal static void Publish(EnemyRoomFx roomFx, int target, SignedEightEight velocity, int delay) =>
        roomFx.PublishLiquidMotion(target, velocity, delay);
}
