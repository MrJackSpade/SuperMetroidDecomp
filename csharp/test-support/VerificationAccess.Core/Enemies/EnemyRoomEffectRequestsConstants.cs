namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyRoomEffectRequests"/> that only verification reads.</summary>
internal static class EnemyRoomEffectRequestsConstants
{
    /// <summary>No requests.</summary>
    public static EnemyRoomEffectRequests None { get; } = new(null, null, [], [], null, 0, [], []);
}
