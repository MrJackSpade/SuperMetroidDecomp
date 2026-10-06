using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>Area-dependent initializer properties for Ridley's fireball and directional afterburn.</summary>
internal static class RidleyProjectileDamageDefinitions
{
    /// <summary>$86:9408/940A/940C, Set_RidleysFireball_Afterburn_Damage values
    /// $5003/$503C/$5050. All property flags are identical; only low-twelve-bit
    /// damage differs. $86:932F selects Norfair, Tourian, or the default row.</summary>
    internal static ushort ForArea(AreaId area) => area switch
    {
        AreaId.Norfair => 60,
        AreaId.Tourian => 80,
        _ => 3,
    };
}
