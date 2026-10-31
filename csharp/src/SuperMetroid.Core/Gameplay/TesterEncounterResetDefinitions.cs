using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Gameplay;

/// <summary>Persistent encounter state included in the opt-in save-load reset; Tourian is excluded.</summary>
internal static class TesterEncounterResetDefinitions
{
    /// <summary>The area defeat flags for bosses, minibosses and Torizos outside Tourian.</summary>
    internal static ReadOnlySpan<AreaId> Areas =>
        [AreaId.Crateria, AreaId.Brinstar, AreaId.Norfair, AreaId.WreckedShip, AreaId.Maridia, AreaId.Ceres];

    /// <summary>The eye doors' five door flags that persist enemy defeat.</summary>
    internal static ReadOnlySpan<int> EyeDoorBits => [69, 92, 133, 155, 168];
}