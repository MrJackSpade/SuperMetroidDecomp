using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate-specific shot reactions of Pirate hitboxes.</summary>
internal static class PirateHitboxShotActions
{
    /// <summary>
    /// Fails for a reaction no Pirate hitbox names. The empty frame still has a real point hitbox for a pirate, whose
    /// common reaction bypasses the Ninja-specific armor tests; it is not rejected as unknown. Only the gold Ninja
    /// (<see cref="SelectGoldNinjaHitboxShotAction"/>) gives the two Ninja body reactions special meaning: every other
    /// Pirate takes the ordinary shot reaction even when it shows one of the frames it shares with the Ninja (#2453).
    /// </summary>
    internal static void RequirePirateShotReaction(EnemyReaction? hitboxShot)
    {
        if (hitboxShot != SpacePirateReactions.Shot && hitboxShot != EnemyReaction.OrdinaryShot &&
            hitboxShot != SpacePirateReactions.GoldNinjaVulnerableHitboxShot && hitboxShot != SpacePirateReactions.GoldNinjaInvincibleHitboxShot)
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to shot reaction {hitboxShot?.ToString() ?? "(unnamed)"}.");
        }
    }

    /// <summary>
    /// Runs the gold Ninja's hitbox shot reactions: its two body reactions reflect or ignore what does it no damage, and
    /// the common and Pirate reactions run the ordinary shot reaction.
    /// </summary>
    internal static PirateHitboxShotAction SelectGoldNinjaHitboxShotAction(
        RoomEnemyPlace enemy,
        SamusProjectileOccupant held,
        EnemyReaction? hitboxShot)
    {
        SamusProjectile projectile = held.Projectile;
        RequirePirateShotReaction(hitboxShot);
        if (hitboxShot == SpacePirateReactions.Shot || hitboxShot == EnemyReaction.OrdinaryShot)
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = held.Type.Family;
        if (hitboxShot == SpacePirateReactions.GoldNinjaInvincibleHitboxShot)
        {
            if (family == SamusProjectileFamily.SuperMissile &&
                projectile.KindStateAs<SuperMissileShotState>().Ignition is SamusMissileIgnition.Pending)
                return PirateHitboxShotAction.Ignore;
            return IsBeamOrMissile(family)
                ? PirateHitboxShotAction.Reflect
                : PirateHitboxShotAction.Ignore;
        }

        if (!IsBeamOrMissile(family))
            return PirateHitboxShotAction.Ignore;

        EnemyVulnerabilities vulnerabilities = EnemyVulnerabilities.Of(enemy.Definition);
        EnemyWeaponVulnerability vulnerability = family switch
        {
            SamusProjectileFamily.Beam => vulnerabilities.ForBeam(held.Type.BeamCombination),
            SamusProjectileFamily.Missile => vulnerabilities.Ordnance.Missile,
            SamusProjectileFamily.SuperMissile => vulnerabilities.Ordnance.SuperMissile,
            SamusProjectileFamily.PowerBomb or
                SamusProjectileFamily.Bomb or
                SamusProjectileFamily.BeamExplosion or
                SamusProjectileFamily.MissileExplosion => throw new InvalidDataException(
                $"Gold Ninja projectile family {family} has no vulnerability field."),
            _ => throw new InvalidDataException(
                $"Gold Ninja projectile family {family} has no vulnerability field."),
        };
        // The Gold Ninja reflects what does it no damage, freezing beams included.
        return !vulnerability.Multiplier.IsNone && !vulnerability.FreezesWithoutDamage
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect;
    }

    /// <summary>
    /// Fails for a reaction no Pirate hitbox gives a normal bomb: a Gold Ninja's body answers only a Power Bomb, so a normal
    /// bomb neither hurts it nor reflects from it.
    /// </summary>
    internal static void RequireGoldNinjaNormalBombReaction(EnemyReaction? hitboxShot)
    {
        if (hitboxShot != SpacePirateReactions.Shot && hitboxShot != SpacePirateReactions.GoldNinjaVulnerableHitboxShot && hitboxShot != SpacePirateReactions.GoldNinjaInvincibleHitboxShot)
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to normal-bomb reaction {hitboxShot?.ToString() ?? "(unnamed)"}.");
        }
    }

    /// <summary>
    /// Whether a shot or bomb whose hit runs through <paramref name="reaction"/> and hurts a Pirate other than the gold
    /// Ninja while it is not frozen shows the enemy-hit burst: the common reaction (a Pirate's empty frame) and the
    /// Pirates' own show it, while both Ninja body reactions run the Pirates' damage that shows none.
    /// </summary>
    /// <param name="reaction">The reaction the hit runs: its hitbox's, or the definition's when the Pirate collides by its radius.</param>
    internal static bool ShowsHitBurst(EnemyReaction? reaction) =>
        reaction == EnemyReaction.OrdinaryShot || reaction == SpacePirateReactions.Shot;

    /// <summary>
    /// Whether a shot or bomb whose hit runs through <paramref name="reaction"/> and hurts the gold Ninja while it is not
    /// frozen shows the enemy-hit burst: only the common reaction (its empty frame) shows it, since its own reaction, like
    /// both Ninja body reactions, runs the Pirates' damage that shows none.
    /// </summary>
    /// <param name="reaction">The reaction the hit runs: its hitbox's, or the definition's when the Ninja collides by its radius.</param>
    internal static bool GoldNinjaShowsHitBurst(EnemyReaction? reaction) => reaction == EnemyReaction.OrdinaryShot;

    /// <summary>Whether a shot is a beam, missile or Super Missile, the families the gold ninja reflects or is hurt by.</summary>
    private static bool IsBeamOrMissile(SamusProjectileFamily family) =>
        family is SamusProjectileFamily.Beam or SamusProjectileFamily.Missile or SamusProjectileFamily.SuperMissile;

    /// <summary>Outcome chosen from a Pirate component's shot reaction before ordinary damage processing.</summary>
    internal enum PirateHitboxShotAction
    {
        /// <summary>Continue through the Pirate's normal shot reaction.</summary>
        Normal,
        /// <summary>Consume the hit without damage or projectile reflection.</summary>
        Ignore,
        /// <summary>Reflect the projectile from the authored hitbox region.</summary>
        Reflect,
    }
}
