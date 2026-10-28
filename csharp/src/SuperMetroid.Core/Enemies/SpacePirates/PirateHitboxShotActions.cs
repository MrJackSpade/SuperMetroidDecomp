using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Progression;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>The Pirate-specific shot callbacks in Pirate hitbox records.</summary>
internal static class PirateHitboxShotActions
{
    /// <summary>
    /// Dispatches the common and Pirate-specific shot callbacks in hitbox records. Only the
    /// gold Ninja gives `$87C8/$883E` special meaning; every other Pirate deliberately falls
    /// through to normal shot AI even when it displays one of the shared Ninja maps.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateHitboxShotAction(
        RoomEnemySlot enemy,
        SamusProjectileSlot projectile,
        EnemyInteractionCallback? hitboxShotAi)
    {
        // The empty extended map still has a real point hitbox. Its common callback
        // bypasses the Ninja-specific armor tests; it must not be rejected as unknown.
        if (hitboxShotAi is EnemyInteractionCallback.SpacePirateShot or EnemyInteractionCallback.NormalEnemyShot)
            return PirateHitboxShotAction.Normal;
        if (hitboxShotAi is not (
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot))
        {
            throw new InvalidDataException(
                $"Space Pirate hitbox shot AI {hitboxShotAi?.ToString() ?? "(unnamed)"} is not translated.");
        }
        if (enemy.DefinitionId != EnemyDefinitionId.PirateGoldNinja)
            return PirateHitboxShotAction.Normal;

        SamusProjectileFamily family = projectile.PackedType.Family;
        if (hitboxShotAi == EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot)
        {
            if (family == SamusProjectileFamily.SuperMissile && projectile.Variable == 0)
                return PirateHitboxShotAction.Ignore;
            return IsBeamOrMissile(family)
                ? PirateHitboxShotAction.Reflect
                : PirateHitboxShotAction.Ignore;
        }

        if (!IsBeamOrMissile(family))
            return PirateHitboxShotAction.Ignore;

        EnemyVulnerabilityTable table = enemy.Definition.Vulnerability;
        int vulnerabilityOffset = family switch
        {
            SamusProjectileFamily.Beam => projectile.PackedType.BeamCombination.TableIndex,
            SamusProjectileFamily.Missile => EnemyVulnerabilityDefinitions.MissileOffset,
            SamusProjectileFamily.SuperMissile =>
                EnemyVulnerabilityDefinitions.SuperMissileOffset,
            SamusProjectileFamily.PowerBomb or
                SamusProjectileFamily.Bomb or
                SamusProjectileFamily.BeamExplosion or
                SamusProjectileFamily.MissileExplosion => throw new InvalidDataException(
                $"Gold Ninja projectile family {family} has no vulnerability field."),
            _ => throw new InvalidDataException(
                $"Gold Ninja projectile family {family} has no vulnerability field."),
        };
        int multiplier = EnemyVulnerabilityDefinitions.LowNibbleMultiplier(EnemyVulnerabilityDefinitions.Read(
            table, vulnerabilityOffset));
        return multiplier is not (0 or 15)
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect;
    }

    /// <summary>
    /// Dispatches a family-$0500 normal bomb through the same three bank-$B2 component
    /// callbacks as the ordinary projectile path. Non-gold Pirates always jump to normal
    /// Pirate shot handling. Gold Ninjas return from both `$87C8` and `$883E` because a
    /// normal bomb sorts after Power Bomb family `$0300`; consequently bombs neither damage
    /// nor reflect from either authored body region.
    /// </summary>
    internal static PirateHitboxShotAction SelectPirateNormalBombHitboxShotAction(
        RoomEnemySlot enemy,
        EnemyInteractionCallback? hitboxShotAi)
    {
        if (hitboxShotAi is not (
                EnemyInteractionCallback.SpacePirateShot or
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or
                EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot))
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

    /// <summary>Outcome chosen from a Pirate component's shot callback before ordinary damage processing.</summary>
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
