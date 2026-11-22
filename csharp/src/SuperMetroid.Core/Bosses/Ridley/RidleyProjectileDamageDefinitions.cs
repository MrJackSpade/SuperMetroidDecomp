using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Bosses.Ridley;

/// <summary>How much Ridley's fireball and directional afterburn hurt Samus, by area.</summary>
internal static partial class RidleyProjectileDamageDefinitions
{
    /// <summary>The fireball and afterburn's damage by area: 60 in Norfair, 80 in Tourian and 3 everywhere else; nothing
    /// else about them changes with the area.</summary>
    internal static readonly IdentityTable<AreaId, int> DamageByArea = new(new Dictionary<AreaId, int>
    {
        [AreaId.Crateria] = 3,
        [AreaId.Brinstar] = 3,
        [AreaId.Norfair] = 60,
        [AreaId.WreckedShip] = 3,
        [AreaId.Maridia] = 3,
        [AreaId.Tourian] = 80,
        [AreaId.Ceres] = 3,
    });
}
