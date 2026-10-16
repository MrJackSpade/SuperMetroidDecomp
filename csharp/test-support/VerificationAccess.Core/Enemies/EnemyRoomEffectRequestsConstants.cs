using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Rooms.Plms;

namespace SuperMetroid.Core.Enemies;

/// <summary>Cartridge values and helpers of <see cref="EnemyRoomEffectRequests"/> that only verification reads.</summary>
internal static class EnemyRoomEffectRequestsConstants
{
    /// <summary>No requests.</summary>
    public static EnemyRoomEffectRequests None { get; } = new(null, null, [], [], null, 0, [], []);
}
