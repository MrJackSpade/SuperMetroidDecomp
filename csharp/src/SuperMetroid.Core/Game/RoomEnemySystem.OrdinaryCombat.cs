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
    private const ushort DefaultEnemyVulnerability = 0xec1c;

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    public bool ResolveOrdinarySamusContact(SamusState samus, ushort controllerInput)
    {
        ArgumentNullException.ThrowIfNull(samus);
        EnsureLoaded();
        if (samus.InvincibilityTimer != 0)
            return false;

        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot slot = SlotFromNativeIndex(nativeIndex);
            if (slot.EnemyDefinitionPointer == CeresRidleyDefinition ||
                slot.Definition.TouchAiPointer != CommonNormalEnemyTouchAi ||
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

            ApplyNormalEnemyTouchDamage(
                samus,
                controllerInput,
                slot.Definition.Damage,
                slot.XPosition);
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
        SamusBombProjectileSystem sharedProjectiles)
    {
        ArgumentNullException.ThrowIfNull(bus);
        ArgumentNullException.ThrowIfNull(projectiles);
        ArgumentNullException.ThrowIfNull(sharedProjectiles);
        EnsureLoaded();

        int hitCount = 0;
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot enemy = SlotFromNativeIndex(nativeIndex);
            if (enemy.EnemyDefinitionPointer == CeresRidleyDefinition ||
                enemy.Definition.ShotAiPointer != CommonNormalEnemyShotAi ||
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
                ushort family = unchecked((ushort)(projectile.Type & 0x0f00));
                if (!projectile.IsActive || family is 0x0300 or 0x0500 or 0x0700 ||
                    !RadiusBoxesOverlap(
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

                byte vulnerability = ReadProjectileVulnerability(bus, enemy, projectile.Type);
                ushort projectileDamage = projectile.Damage;
                if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
                    continue;

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
