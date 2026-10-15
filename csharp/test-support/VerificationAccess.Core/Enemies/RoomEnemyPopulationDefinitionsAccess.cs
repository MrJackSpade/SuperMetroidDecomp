
namespace SuperMetroid.Core.Enemies;

/// <summary>Verification access to <see cref="RoomEnemyPopulationDefinitions"/> members production does not use.</summary>
internal static class RoomEnemyPopulationDefinitionsAccess
{
    /// <summary>Enumerates the native identities of compiled room enemy populations.</summary>
    extension(RoomEnemyPopulationDefinitions)
    {
        /// <summary>Enumerates every compiled population identity for ROM-oracle tests.</summary>
        internal static IEnumerable<ushort> Pointers => RoomEnemyPopulationDefinitions.Definitions.Select(list => list.Pointer);
    }
}
