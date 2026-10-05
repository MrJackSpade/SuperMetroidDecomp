using SuperMetroid.Core.Game;

namespace SuperMetroid.Core.Runtime;

/// <summary>Persistent encounter state included in the opt-in save-load reset; Tourian is excluded.</summary>
internal static class TesterEncounterResetDefinitions
{
    /// <summary>$7E:D828 area defeat bytes for bosses, minibosses and Torizos outside Tourian.</summary>
    internal static ReadOnlySpan<AreaId> Areas =>
        [AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Ceres];

    /// <summary>Bank-$8F eye-door population arguments $45/$5C/$85/$9B/$A8; these five door bits persist enemy defeat.</summary>
    internal static ReadOnlySpan<ushort> EyeDoorBits => [0x0045, 0x005c, 0x0085, 0x009b, 0x00a8];
}