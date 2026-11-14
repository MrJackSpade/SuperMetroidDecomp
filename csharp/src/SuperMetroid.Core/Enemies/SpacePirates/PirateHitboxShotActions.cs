using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate-specific shot reactions of Pirate hitboxes.</summary>
internal static class PirateHitboxShotActions
{
    /// <summary>
    /// Dispatches the common and Pirate-specific shot reactions of hitboxes. Only the
    /// gold Ninja gives the two Ninja body reactions special meaning; every other Pirate deliberately falls
    /// through to normal shot AI even when it displays one of the shared Ninja maps.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateHitboxShotAction(
        RoomEnemySlot enemy,
        SamusProjectile projectile,
        EnemyReaction? hitboxShot)
    {
        // The empty frame still has a real point hitbox for a pirate. Its common reaction
        // bypasses the Ninja-specific armor tests; it must not be rejected as unknown.
        if ((hitboxShot == SpacePirateReactions.Shot || hitboxShot == EnemyReaction.NormalEnemyShot))
            return PirateHitboxShotAction.Normal;
        if ((hitboxShot != SpacePirateReactions.GoldNinjaVulnerableHitboxShot && hitboxShot != SpacePirateReactions.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to shot reaction {hitboxShot?.ToString() ?? "(unnamed)"}.");
        }
        if (enemy.DefinitionId != EnemyDefinitionId.PirateGoldNinja)
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = projectile.Type.Family;
        if (hitboxShot == SpacePirateReactions.GoldNinjaInvincibleHitboxShot)
        {
            if (family == SamusProjectileFamily.SuperMissile && projectile.MissileIgnition == SamusMissileIgnition.Pending)
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
            SamusProjectileFamily.Beam => vulnerabilities.ForBeam(projectile.Type.BeamCombination),
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
        return vulnerability.Multiplier != 0 && !vulnerability.FreezesWithoutDamage
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect;
    }

    /// <summary>
    /// Dispatches a normal bomb through the same three component
    /// reactions as the ordinary projectile path. Non-gold Pirates always jump to normal
    /// Pirate shot handling. Gold Ninjas return from both body reactions because a
    /// normal bomb's family sorts after the Power Bomb family; consequently bombs neither damage
    /// nor reflect from either authored body region.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateNormalBombHitboxShotAction(
        RoomEnemySlot enemy,
        EnemyReaction? hitboxShot)
    {
        if ((hitboxShot != SpacePirateReactions.Shot && hitboxShot != SpacePirateReactions.GoldNinjaVulnerableHitboxShot && hitboxShot != SpacePirateReactions.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate has no response to normal-bomb reaction {hitboxShot?.ToString() ?? "(unnamed)"}.");
        }

        return enemy.DefinitionId == EnemyDefinitionId.PirateGoldNinja
            ? PirateHitboxShotAction.Ignore
            : PirateHitboxShotAction.Normal;
    }

    /// <summary>
    /// Whether a shot or bomb whose hit runs through <paramref name="reaction"/> and hurts <paramref name="pirate"/> while
    /// it is not frozen shows the enemy-hit burst: the common reaction (a Pirate's empty frame) and the Pirates' own show
    /// it, except on the gold Ninja, whose own reaction, like both Ninja body reactions on every Pirate, runs the Pirates'
    /// damage that shows none.
    /// </summary>
    /// <param name="pirate">The Pirate hit.</param>
    /// <param name="reaction">The reaction the hit runs: its hitbox's, or the definition's when the Pirate collides by its radius.</param>
    internal static bool ShowsHitBurst(RoomEnemySlot pirate, EnemyReaction? reaction) =>
        reaction == EnemyReaction.NormalEnemyShot ||
        reaction == SpacePirateReactions.Shot && Ensure.NotNull(pirate).DefinitionId != EnemyDefinitionId.PirateGoldNinja;

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
