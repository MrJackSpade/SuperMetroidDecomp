
namespace SuperMetroid.Core.Progression;

/// <summary>
/// Ranking predicates over the game's projectile family order (<see cref="SamusProjectileFamilyDefinitions.Order"/>) and
/// each family's row in the per-family projectile tables. The cartridge's ordered family comparisons ("explosions and
/// later", "bombs and later") are these predicates; a family's value is never compared.
/// </summary>
public static class SamusProjectileFamilies
{
    /// <summary>The ranking predicates and table row of a family.</summary>
    extension(SamusProjectileFamily family)
    {
        /// <summary>The family's position in the game's family order.</summary>
        private int Rank => Array.IndexOf(SamusProjectileFamilyDefinitions.Order, family) is var rank and >= 0
            ? rank
            : throw new ArgumentOutOfRangeException(nameof(family), family, "Undefined projectile family.");

        /// <summary>Whether the family ranks at or after <paramref name="threshold"/>.</summary>
        public bool IsAtOrAfter(SamusProjectileFamily threshold) => family.Rank >= threshold.Rank;

        /// <summary>Whether the family is an explosion a shot left behind: the families ranked from the beam explosion on.</summary>
        public bool IsExplosion => family.IsAtOrAfter(SamusProjectileFamily.BeamExplosion);

        /// <summary>Whether the family is a bomb or an explosion: the families ranked from the Power Bomb on.</summary>
        public bool IsBombOrExplosion => family.IsAtOrAfter(SamusProjectileFamily.PowerBomb);

        /// <summary>Whether the family is a fired shot: a beam, missile or Super Missile.</summary>
        public bool IsBeamOrMissile => !family.IsBombOrExplosion;

        /// <summary>The family's row in the per-family projectile tables (the non-beam selector and cooldown rows).</summary>
        internal int TableRow => SamusProjectileFamilyDefinitions.TableRows[family.Rank];
    }
}
