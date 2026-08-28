using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Game;

/// <summary>
/// Shared ordinary-enemy contact and projectile collision translated from bank $A0. Enemy
/// families opt in through their ROM header's common touch/shot pointers; damage remains
/// driven by header and vulnerability data rather than actor-specific host constants.
/// </summary>
public sealed partial class RoomEnemySystem
{
    private const ushort CommonNormalEnemyTouchAi = 0x8023;
    private const ushort CommonNormalEnemyShotAi = 0x802d;
    private const ushort SkreeShotAi = 0xc7f5;
    private const ushort MochtroidTouchAi = 0xa953;
    private const ushort MochtroidShotAi = 0xa9a8;
    private const ushort YardTouchAi = 0xd3b0;
    private const ushort YardShotAi = 0xd469;
    private const ushort DefaultEnemyVulnerability = 0xec1c;

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    public bool ResolveOrdinarySamusContact(SamusState samus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(samus);
        EnsureLoaded();
        ushort contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == 0 && samus.InvincibilityTimer != 0)
            return false;
        if (contactDamageIndex != 0)
            samus.InvincibilityTimer = 0;

        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            bool usesTranslatedTouchAi = slot.Definition.TouchAiPointer == CommonNormalEnemyTouchAi ||
                slot.EnemyDefinitionPointer == MochtroidDefinition &&
                slot.Definition.TouchAiPointer == MochtroidTouchAi ||
                slot.EnemyDefinitionPointer == YardDefinition &&
                slot.Definition.TouchAiPointer == YardTouchAi;
            if (slot.EnemyDefinitionPointer == CeresRidleyDefinition ||
                !usesTranslatedTouchAi ||
                slot.SpritemapPointer == 0 ||
                slot.Properties.HasAny(EnemyProperties.Invisible | EnemyProperties.Deleted))
            {
                continue;
            }

            if (!RadiusBoxesOverlap(
                    slot.XPosition,
                    slot.YPosition,
                    slot.XRadius,
                    slot.YRadius,
                    samus.XPosition,
                    samus.YPosition,
                    samus.Kinematics.XRadius,
                    samus.Kinematics.YRadius))
            {
                continue;
            }

            if (slot.EnemyDefinitionPointer == MochtroidDefinition)
            {
                ResolveMochtroidTouch(
                    slot,
                    RequireMochtroidState(slot),
                    samus,
                    controllerInput);
            }
            else if (slot.EnemyDefinitionPointer == YardDefinition)
            {
                ResolveYardTouch(
                    slot,
                    RequireYardState(slot),
                    samus,
                    controllerInput);
            }
            else
            {
                ResolveNormalEnemyTouch(slot, samus, controllerInput);
            }
            return true;
        }
        return false;
    }

    /// <summary>
    /// Runs common normal-enemy shot AI for ordinary radius-based translated actors. One
    /// projectile may resolve per enemy per pass, matching the native collision-handler exit.
    /// </summary>
    public int ResolveOrdinaryProjectileHits(
        ISnesAddressSpace bus,
        SamusProjectileSystem projectiles,
        SamusBombProjectileSystem sharedProjectiles,
        SamusState? samus = null)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        EnsureLoaded();

        int hitCount = 0;
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot enemy = SlotFromNativeIndex(nativeIndex);
            bool isYard = enemy.EnemyDefinitionPointer == YardDefinition &&
                enemy.Definition.ShotAiPointer == YardShotAi;
            bool usesTranslatedShotAi = enemy.Definition.ShotAiPointer == CommonNormalEnemyShotAi ||
                enemy.EnemyDefinitionPointer == SkreeDefinition &&
                enemy.Definition.ShotAiPointer == SkreeShotAi ||
                enemy.EnemyDefinitionPointer == MochtroidDefinition &&
                enemy.Definition.ShotAiPointer == MochtroidShotAi ||
                isYard;
            if (enemy.EnemyDefinitionPointer == CeresRidleyDefinition ||
                !usesTranslatedShotAi ||
                enemy.SpritemapPointer == 0 ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(
                    EnemyProperties.Invisible |
                    EnemyProperties.Deleted |
                    EnemyProperties.IgnoreSamusCollision))
            {
                continue;
            }

            foreach (SamusProjectileSlot projectile in projectiles.Slots)
            {
                // `$0C18,x` is still the live beam/missile type when the enemy collision
                // handler selects vulnerability. Starting the impact below deliberately
                // rewrites that slot to explosion family `$0700`, so retain the incoming
                // word instead of accidentally asking the vulnerability table about the
                // newly-created visual effect.
                ushort projectileType = projectile.Type;
                ushort projectileDamage = projectile.Damage;
                ushort family = unchecked((ushort)(projectileType & 0x0f00));
                if (!projectile.IsActive)
                    continue;

                // Bomb, power-bomb, and pseudo-screw actors share the projectile slot array,
                // but ordinary enemy shot AI never interprets them as beam/missile records.
                // Yard is the one translated exception: its private $A3:D469 handler treats
                // bomb-family impacts as a physical kick, so those actors must reach its
                // custom branch below. Keep this as an explicit guard instead of embedding it
                // in the overlap expression; doing so makes the native dispatch boundary clear.
                if (!isYard &&
                    (family == 0x0300 || family == 0x0500 || family == 0x0700))
                {
                    continue;
                }

                if (!RadiusBoxesOverlap(
                        enemy.XPosition,
                        enemy.YPosition,
                        enemy.XRadius,
                        enemy.YRadius,
                        projectile.XPosition,
                        projectile.YPosition,
                        projectile.XRadius,
                        projectile.YRadius))
                {
                    continue;
                }

                if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
                    continue;

                // Yard's custom shot AI sends super-missile/power-bomb families through
                // normal vulnerability damage, but every other colliding shot merely kicks
                // the shell into the air. This branch must occur after projectile impact,
                // exactly where bank $A0 has already accepted the collision index.
                if (isYard && family is not (0x0300 or 0x0500))
                {
                    if (samus is null)
                    {
                        throw new InvalidOperationException(
                            "Yard beam launch requires the active Samus actor.");
                    }
                    ResolveYardBeamLaunch(enemy, RequireYardState(enemy), samus);
                    hitCount++;
                    break;
                }

                byte vulnerability = ReadProjectileVulnerability(bus, enemy, projectileType);

                if (vulnerability == 0xff)
                {
                    enemy.FrozenTimer = 400;
                    enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0004));
                    enemy.InvincibilityTimer = 10;
                    hitCount++;
                    break;
                }

                int damage = (projectileDamage >> 1) * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    ushort hurtTime = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                    enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
                    enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
                    enemy.Health = damage >= enemy.Health
                        ? (ushort)0
                        : unchecked((ushort)(enemy.Health - damage));
                    if (enemy.Health == 0)
                    {
                        // $A3:C7F5 adds Skree's four debris actors after the shared normal
                        // shot handler reports death, before the common death animation
                        // releases the enemy slot.
                        if (enemy.EnemyDefinitionPointer == SkreeDefinition)
                            SpawnSkreeParticleBurst(enemy);
                        enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                    }
                }

                hitCount++;
                break;
            }
        }
        return hitCount;
    }

    private static byte ReadProjectileVulnerability(
        ISnesAddressSpace bus,
        RoomEnemySlot enemy,
        ushort projectileType)
    {
        ushort pointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : DefaultEnemyVulnerability;
        int family = projectileType & 0x0f00;
        int byteOffset = family switch
        {
            0x0000 => projectileType & 0x000f,
            0x0100 => 12,
            0x0200 => 13,
            0x0500 => 14,
            0x0300 => 15,
            _ => throw new NotSupportedException(
                $"Projectile family ${family:X3} has no translated vulnerability field."),
        };
        return bus.ReadByte(0xb40000 | unchecked((ushort)(pointer + byteOffset)));
    }

    /// <summary>
    /// Ports $A0:A477-$A531 for one already-overlapping ordinary actor. A zero contact index
    /// damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their dedicated bytes from the enemy's vulnerability record and damage the actor.
    /// </summary>
    private void ResolveNormalEnemyTouch(
        RoomEnemySlot enemy,
        SamusState samus,
        ushort controllerInput)
    {
        ushort contactDamageIndex = samus.HorizontalSpeed.ContactDamageIndex;
        if (contactDamageIndex == 0)
        {
            ApplyNormalEnemyTouchDamage(
                samus,
                controllerInput,
                enemy.Definition.Damage,
                enemy.XPosition);
            return;
        }

        ushort baseDamage = contactDamageIndex switch
        {
            1 => 500,  // Speed Booster
            2 => 300,  // Shinespark
            3 => 2000, // Screw Attack
            _ => 200,  // Pseudo-Screw and the native fallback
        };
        int vulnerabilityOffset = contactDamageIndex <= 3
            ? contactDamageIndex + 15
            : contactDamageIndex + 16;
        ushort vulnerabilityPointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : DefaultEnemyVulnerability;
        byte vulnerability = _bus!.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + vulnerabilityOffset)));
        int damage = (baseDamage >> 1) * (vulnerability & 0x7f);
        if (damage == 0)
            return;

        // Touch damage uses the raw hurt-AI duration (default four), unlike projectile and
        // power-bomb paths which add their own visible-flash tail. Samus's timers are also
        // explicitly cleared because this branch represents Samus attacking the enemy.
        enemy.FlashTimer = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
        enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
        samus.InvincibilityTimer = 0;
        samus.KnockbackTimer = 0;
        enemy.Health = damage >= enemy.Health
            ? (ushort)0
            : unchecked((ushort)(enemy.Health - damage));
        if (enemy.Health != 0)
            return;

        enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
    }

    private static bool RadiusBoxesOverlap(
        ushort firstX,
        ushort firstY,
        ushort firstXRadius,
        ushort firstYRadius,
        ushort secondX,
        ushort secondY,
        ushort secondXRadius,
        ushort secondYRadius) =>
        Math.Abs(unchecked((short)(firstX - secondX))) < firstXRadius + secondXRadius &&
        Math.Abs(unchecked((short)(firstY - secondY))) < firstYRadius + secondYRadius;
}
