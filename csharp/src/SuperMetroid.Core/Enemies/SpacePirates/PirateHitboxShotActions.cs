using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate-specific shot reactions of Pirate hitboxes.</summary>
internal static partial class PirateHitboxShotActions
{
    /// <summary>
    /// Runs the gold Ninja's hitbox shot reactions: its two body reactions reflect or ignore what does it no damage, and
    /// the common and Pirate reactions, the only others a Pirate hitbox names (<see cref="SpacePirateHitboxes"/>), run the
    /// ordinary shot reaction. Only the gold Ninja gives the two Ninja body reactions special meaning: every other Pirate
    /// takes the ordinary shot reaction even when it shows one of the frames it shares with the Ninja (#2453).
    /// </summary>
    internal static PirateHitboxShotAction SelectGoldNinjaHitboxShotAction(
        RoomEnemyOccupant enemy,
        SamusProjectileOccupant held,
        EnemyReaction hitboxShot)
    {
        SamusProjectile projectile = held.Projectile;
        if (SpacePirateReactions.GoldNinjaOrdinary.Contains(hitboxShot))
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = held.Type.Family;
        if (SpacePirateReactions.InvincibleArmour.Contains(hitboxShot))
        {
            if (family == SamusProjectileFamily.SuperMissile &&
                held.KindState is { AwaitsIgnitionAsFired: true })
                return PirateHitboxShotAction.Ignore;
            return BeamAndMissileVulnerabilities.TryGetValue(family, out ReflectableShot? reflectable)
                ? PirateHitboxShotAction.Reflect(reflectable.Family)
                : PirateHitboxShotAction.Ignore;
        }

        // Only beams and missiles have a vulnerability the Gold Ninja reads; it ignores bombs and explosions.
        if (!BeamAndMissileVulnerabilities.TryGetValue(family, out ReflectableShot? shot))
            return PirateHitboxShotAction.Ignore;

        EnemyWeaponVulnerability vulnerability = shot.VulnerabilityOf(EnemyVulnerabilities.Of(Ensure.NotNull(enemy).Definition), held);
        // The Gold Ninja reflects what does it no damage, freezing beams included.
        return !vulnerability.Multiplier.IsNone && !vulnerability.FreezesWithoutDamage
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect(shot.Family);
    }


    /// <summary>
    /// Whether a shot or bomb whose hit runs through <paramref name="reaction"/> and hurts a Pirate other than the gold
    /// Ninja while it is not frozen shows the enemy-hit burst: the common reaction (a Pirate's empty frame) and the
    /// Pirates' own show it, while both Ninja body reactions run the Pirates' damage that shows none.
    /// </summary>
    /// <param name="reaction">The reaction the hit runs: its hitbox's, or the definition's when the Pirate collides by its radius.</param>
    internal static bool ShowsHitBurst(EnemyReaction reaction) => SpacePirateReactions.ShowingHitBurst.Contains(reaction);

    /// <summary>Whether a bomb's hurting hit shows the hit burst on an ordinary Pirate: where the reaction it selects shows it.</summary>
    internal static bool ShowsHitBurst(EnemyBombHitContext context) => ShowsHitBurst(Ensure.NotNull(context).SelectedShot);

    /// <summary>Whether a shot's hurting hit shows the hit burst on an ordinary Pirate: where its hitbox's reaction shows it.</summary>
    internal static bool ShowsHitBurst(EnemyShotHitContext context) => ShowsHitBurst(Ensure.NotNull(context).HitboxShot);

    /// <summary>
    /// Whether a shot or bomb whose hit runs through <paramref name="reaction"/> and hurts the gold Ninja while it is not
    /// frozen shows the enemy-hit burst: only the common reaction (its empty frame) shows it, since its own reaction, like
    /// both Ninja body reactions, runs the Pirates' damage that shows none.
    /// </summary>
    /// <param name="reaction">The reaction the hit runs: its hitbox's, or the definition's when the Ninja collides by its radius.</param>
    internal static bool GoldNinjaShowsHitBurst(EnemyReaction reaction) => Ensure.NotNull(reaction).IsOrdinary;

    /// <summary>Whether a bomb's hurting hit shows the hit burst on the gold Ninja: where the reaction it selects shows it.</summary>
    internal static bool GoldNinjaShowsHitBurst(EnemyBombHitContext context) => GoldNinjaShowsHitBurst(Ensure.NotNull(context).SelectedShot);

    /// <summary>Whether a shot's hurting hit shows the hit burst on the gold Ninja: where its hitbox's reaction shows it.</summary>
    internal static bool GoldNinjaShowsHitBurst(EnemyShotHitContext context) => GoldNinjaShowsHitBurst(Ensure.NotNull(context).HitboxShot);

    /// <summary>
    /// The families the gold ninja reflects or is hurt by, a beam, missile or Super Missile: each as the family a reflection
    /// carries and the vulnerability the ninja reads for it.
    /// </summary>
    private static IReadOnlyDictionary<SamusProjectileFamily, ReflectableShot> BeamAndMissileVulnerabilities => field ??=
        new Dictionary<SamusProjectileFamily, ReflectableShot>
        {
            [SamusProjectileFamily.Beam] = new(ReflectableShotFamily.Beam, static (vulnerabilities, held) => vulnerabilities.ForBeam(held.Type.BeamCombination)),
            [SamusProjectileFamily.Missile] = new(ReflectableShotFamily.Missile, static (vulnerabilities, held) => vulnerabilities.Ordnance.Missile),
            [SamusProjectileFamily.SuperMissile] = new(ReflectableShotFamily.SuperMissile, static (vulnerabilities, held) => vulnerabilities.Ordnance.SuperMissile),
        };

    /// <summary>A family the gold ninja reflects, and the vulnerability it reads for it.</summary>
    /// <param name="Family">The family a reflection of the shot carries.</param>
    /// <param name="VulnerabilityOf">The ninja's vulnerability to the shot.</param>
    private sealed partial record ReflectableShot(ReflectableShotFamily Family, Func<EnemyVulnerabilities, SamusProjectileOccupant, EnemyWeaponVulnerability> VulnerabilityOf);

    /// <summary>
    /// Outcome chosen from a Pirate component's shot reaction before ordinary damage processing (#2553): the Pirate's
    /// normal shot reaction, the hit consumed with no damage, or the shot reflected, carrying its reflectable family.
    /// </summary>
    internal sealed partial record PirateHitboxShotAction
    {
        /// <summary>Only the outcomes below exist.</summary>
        private PirateHitboxShotAction()
        {
        }

        /// <summary>Continue through the Pirate's normal shot reaction.</summary>
        internal static PirateHitboxShotAction Normal { get; } = new() { RunsNormalReaction = true };

        /// <summary>Consume the hit without damage or projectile reflection.</summary>
        internal static PirateHitboxShotAction Ignore { get; } = new();

        /// <summary>Reflect the projectile, of <paramref name="family"/>, from the authored hitbox region.</summary>
        internal static PirateHitboxShotAction Reflect(ReflectableShotFamily family) => new() { Reflected = family };

        /// <summary>Whether the hit continues through the Pirate's normal shot reaction.</summary>
        internal bool RunsNormalReaction { get; private init; }

        /// <summary>The family of the shot the hitbox reflects; null when it reflects none.</summary>
        internal ReflectableShotFamily? Reflected { get; private init; }
    }
}
