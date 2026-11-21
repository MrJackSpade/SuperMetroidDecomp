
namespace SuperMetroid.Core.Progression;

/// <summary>
/// The properties of a projectile family the game tests (<see cref="SamusProjectileFamilyDefinitions.Roles"/>, #2556): its
/// ordered family comparisons ("explosions and later", "bombs and later") are these predicates; no family is ranked.
/// </summary>
public static partial class SamusProjectileFamilies
{
    /// <summary>The properties of a family.</summary>
    extension(SamusProjectileFamily family)
    {
        /// <summary>What the family is to the rules that test it.</summary>
        private SamusProjectileFamilyRole Role => SamusProjectileFamilyDefinitions.Roles[family];

        /// <summary>Whether the family is an explosion a shot left behind.</summary>
        public bool IsExplosion => family.Role == SamusProjectileFamilyRole.Explosion;

        /// <summary>Whether the family is a bomb or an explosion.</summary>
        public bool IsBombOrExplosion => family.Role != SamusProjectileFamilyRole.FiredShot;

        /// <summary>Whether the family is a fired shot: a beam, missile or Super Missile.</summary>
        public bool IsBeamOrMissile => family.Role == SamusProjectileFamilyRole.FiredShot;
    }
}
