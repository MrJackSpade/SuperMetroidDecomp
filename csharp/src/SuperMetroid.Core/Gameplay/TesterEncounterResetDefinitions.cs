using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Persistent encounter state included in the opt-in save-load reset; Tourian is excluded.</summary>
internal static class TesterEncounterResetDefinitions
{
    /// <summary>$7E:D828 area defeat bytes for bosses, minibosses and Torizos outside Tourian.</summary>
    internal static ReadOnlySpan<AreaId> Areas =>
        [AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Ceres];

    /// <summary>Bank-$8F eye-door population arguments $45/$5C/$85/$9B/$A8: the five door bits that persist enemy defeat.</summary>
    internal static ReadOnlySpan<int> EyeDoorBits => [69, 92, 133, 155, 168];
}