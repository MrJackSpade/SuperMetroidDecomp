using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Area-dependent initializer properties for Ridley's fireball and directional afterburn.</summary>
internal static class RidleyProjectileDamageDefinitions
{
    /// <summary>The fireball afterburn's damage by area. All property flags are identical; only the
    /// damage differs. The afterburn selects Norfair, Tourian, or the default row for every other area.</summary>
    internal static readonly Dictionary<AreaId, int> DamageByArea = new()
    {
        [AreaId.Crateria] = 3,
        [AreaId.Brinstar] = 3,
        [AreaId.Norfair] = 60,
        [AreaId.WreckedShip] = 3,
        [AreaId.Maridia] = 3,
        [AreaId.Tourian] = 80,
        [AreaId.Ceres] = 3,
    };
}
