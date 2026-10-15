namespace SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Rooms.LevelData;

/// <summary>Verification access to <see cref="CeresSteamCollisionDefinitions"/> members production does not use.</summary>
internal static class CeresSteamCollisionDefinitionsAccess
{
    /// <summary>Enumerates native Ceres steam hitbox roots from the compiled collision records.</summary>
    extension(CeresSteamCollisionDefinitions)
    {
        /// <summary>Native roots of the compiled Ceres steam hitbox lists.</summary>
        internal static IEnumerable<ushort> HitboxPointers => CeresSteamCollisionDefinitions.Lists.Pointers;
    }
}
