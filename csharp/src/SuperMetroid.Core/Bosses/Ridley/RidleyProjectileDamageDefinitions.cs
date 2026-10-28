using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>Area-dependent initializer properties for Ridley's fireball and directional afterburn.</summary>
internal static class RidleyProjectileDamageDefinitions
{
    /// <summary>$86:9408/940A/940C, Set_RidleysFireball_Afterburn_Damage values
    /// $5003/$503C/$5050. All property flags are identical; only low-twelve-bit
    /// damage differs. $86:932F selects Norfair, Tourian, or the default row for every other area.</summary>
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
