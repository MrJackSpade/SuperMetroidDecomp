using SuperMetroid.Core.Enemies;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Ridley's liquid commands (#1395).</summary>
internal static class RidleyLiquidMotion
{
    /// <summary>Records Ridley's liquid target and delay and applies them, with the velocity, to the room's liquid.</summary>
    internal static void Publish(EnemyRoomFx roomFx, RidleyEnemyState state, ushort target, SnesSignedEightEight velocity, ushort delay)
    {
        state.FxTargetYPosition = target;
        state.FxTimer = delay;
        roomFx.PublishLiquidMotion(target, velocity.RawValue, delay);
    }
}
