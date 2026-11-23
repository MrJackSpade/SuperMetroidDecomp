using Engine.Content;
using Engine.Shared;

namespace Engine.Actors;

/// <summary>
/// The shot a weapon fires while a particular set of equipment is switched on, in place of its own: its damage, its
/// kinds, its cooldown and the projectiles it launches, plain and charged.
/// </summary>
/// <param name="When">The equipment that must all be switched on for this variant to apply.</param>
/// <param name="Damage">The damage of its hit.</param>
/// <param name="Kinds">The kinds its hit carries.</param>
/// <param name="Cooldown">How many updates must pass after a shot; null for the weapon's own.</param>
/// <param name="Shots">The projectiles a plain shot launches.</param>
/// <param name="ChargedShots">The projectiles a charged shot launches.</param>
public sealed record WeaponVariant(
    IReadOnlyList<string> When,
    int Damage,
    IReadOnlyList<HitKind> Kinds,
    int? Cooldown,
    IReadOnlyList<WeaponShot> Shots,
    IReadOnlyList<WeaponShot> ChargedShots)
{
    /// <summary>Reads a variant from a weapon's data file. A shot that gives no projectile uses <paramref name="weaponProjectile"/>.</summary>
    public static WeaponVariant Read(DataNode data, ProjectileSettings weaponProjectile)
    {
        IReadOnlyList<WeaponShot> Shots(string name) => data.List(name).Select(shot => new WeaponShot(
            shot.Has("side") ? shot.Int("side") : 0,
            shot.Has("projectile") ? ProjectileSettings.Read(shot.Get("projectile")) : weaponProjectile)).ToList();

        IReadOnlyList<WeaponShot> shots = data.Has("shots") ? Shots("shots") : [new WeaponShot(0, weaponProjectile)];
        var variant = new WeaponVariant(
            data.List("when").Select(equipment => equipment.AsText()).ToList(),
            data.Int("damage"),
            data.List("kinds").Select(kind => new HitKind(kind.AsText())).ToList(),
            data.Has("cooldown") ? data.Int("cooldown") : null,
            shots,
            data.Has("charged_shots") ? Shots("charged_shots") : shots);
        if (variant.Damage < 0 || variant.Kinds.Count == 0 || variant.Shots.Count == 0 || variant.ChargedShots.Count == 0)
            throw data.Problem("gives a negative damage, a hit that carries no kind, or no projectile to launch");
        return variant;
    }
}
