using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate-specific shot reactions in Pirate hitbox records.</summary>
internal static class PirateHitboxShotActions
{
    /// <summary>
    /// Dispatches the common and Pirate-specific shot reactions in hitbox records. Only the
    /// gold Ninja gives the two Ninja body reactions special meaning; every other Pirate deliberately falls
    /// through to normal shot AI even when it displays one of the shared Ninja maps.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateHitboxShotAction(
        RoomEnemySlot enemy,
        SamusProjectile projectile,
        EnemyReaction? hitboxShotAi)
    {
        // The empty frame still has a real point hitbox for a pirate. Its common reaction
        // bypasses the Ninja-specific armor tests; it must not be rejected as unknown.
        if (hitboxShotAi is EnemyReaction.SpacePirateShot or EnemyReaction.NormalEnemyShot)
            return PirateHitboxShotAction.Normal;
        if (hitboxShotAi is not (
                EnemyReaction.GoldNinjaVulnerableHitboxShot or EnemyReaction.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate hitbox shot AI {hitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
        }
        if (enemy.DefinitionId != EnemyDefinitionId.PirateGoldNinja)
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = projectile.Type.Family;
        if (hitboxShotAi == EnemyReaction.GoldNinjaInvincibleHitboxShot)
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
        EnemyReaction? hitboxShotAi)
    {
        if (hitboxShotAi is not (
                EnemyReaction.SpacePirateShot or
                EnemyReaction.GoldNinjaVulnerableHitboxShot or
                EnemyReaction.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate normal-bomb hitbox AI {hitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
        }

        return enemy.DefinitionId == EnemyDefinitionId.PirateGoldNinja
            ? PirateHitboxShotAction.Ignore
            : PirateHitboxShotAction.Normal;
    }

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
