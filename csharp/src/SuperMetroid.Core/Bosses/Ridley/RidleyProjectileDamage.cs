using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>The damage of Ridley's fireball and afterburn in the area the fight takes place in.</summary>
internal static partial class RidleyProjectileDamage
{
    /// <summary>The fireball and afterburn damage in <paramref name="area"/>.</summary>
    internal static int ForArea(AreaId area) => RidleyProjectileDamageDefinitions.DamageByArea[area];
}
