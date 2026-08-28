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
    private const ushort MetareeShotAi = 0x8b0f;
    private const ushort FirefleaTouchAi = 0x8e6b;
    private const ushort FirefleaPowerBombAi = 0x8e83;
    private const ushort FirefleaShotAi = 0x8e89;
    private const ushort MochtroidTouchAi = 0xa953;
    private const ushort MochtroidShotAi = 0xa9a8;
    private const ushort YardTouchAi = 0xd3b0;
    private const ushort YardShotAi = 0xd469;
    private const ushort BeetomTouchAi = 0xbe2e;
    private const ushort BeetomShotAi = 0xbeac;
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
            bool isFireflea = slot.EnemyDefinitionPointer == FirefleaDefinition &&
                slot.Definition.TouchAiPointer == FirefleaTouchAi;
            bool isPlatform = IsPlatformDefinition(slot.EnemyDefinitionPointer) &&
                slot.Definition.TouchAiPointer == PlatformNoOpTouchAi;
            bool isBeetom = slot.EnemyDefinitionPointer == BeetomDefinition &&
                slot.Definition.TouchAiPointer == BeetomTouchAi;
            bool usesTranslatedTouchAi = slot.Definition.TouchAiPointer == CommonNormalEnemyTouchAi ||
                isPlatform ||
                isFireflea ||
                isBeetom ||
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

            if (isPlatform)
            {
                // `$A3:9F07` is a literal RTL. Platform solidity and the asymmetric rider
                // test live in other handlers; ordinary body overlap must neither injure
                // Samus nor synthesize knockback here.
            }
            else if (slot.EnemyDefinitionPointer == MochtroidDefinition)
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
            else if (isBeetom)
            {
                ResolveBeetomTouch(
                    slot,
                    RequireBeetomState(slot),
                    samus,
                    controllerInput);
            }
            else if (isFireflea)
            {
                ResolveFirefleaTouch(slot, samus, controllerInput);
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
            bool isMetaree = enemy.EnemyDefinitionPointer == MetareeDefinition &&
                enemy.Definition.ShotAiPointer == MetareeShotAi;
            bool isFireflea = enemy.EnemyDefinitionPointer == FirefleaDefinition &&
                enemy.Definition.ShotAiPointer == FirefleaShotAi;
            bool isTripper = enemy.EnemyDefinitionPointer == TripperDefinition &&
                enemy.Definition.ShotAiPointer == TripperShotAi;
            bool isBeetom = enemy.EnemyDefinitionPointer == BeetomDefinition &&
                enemy.Definition.ShotAiPointer == BeetomShotAi;
            bool usesTranslatedShotAi = enemy.Definition.ShotAiPointer == CommonNormalEnemyShotAi ||
                enemy.EnemyDefinitionPointer == SkreeDefinition &&
                enemy.Definition.ShotAiPointer == SkreeShotAi ||
                isMetaree ||
                isFireflea ||
                isTripper ||
                isBeetom ||
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
                    if (isTripper)
                    {
                        // Tripper's private tail runs after common shot AI and replaces the
                        // current frame with a direction-specific two-piece frozen map.
                        enemy.SpritemapPointer = RequirePlatformState(enemy).XMovement ==
                            PlatformHorizontalMovement.Left
                                ? TripperFrozenMovingLeftSpritemap
                                : TripperFrozenMovingRightSpritemap;
                    }
                    if (isBeetom)
                        ResolveBeetomShotAfterCommon(enemy, RequireBeetomState(enemy));
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
                        else if (isMetaree)
                        {
                            // `$A3:8B0F` saves these graphics words around common shot AI,
                            // uses them for four metal debris actors on death, then clears
                            // the dead body. Spawning before the clears preserves that exact
                            // graphics index without inventing projectile-local assets.
                            SpawnMetareeParticleBurst(enemy);
                            enemy.VramTilesIndex = 0;
                            enemy.PaletteIndex = 0;
                        }
                        if (isFireflea)
                            AdvanceFirefleaDarknessLevel();
                        enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                    }
                }

                // Beetom's private tail runs after common shot AI for every accepted hit,
                // including immune/zero-damage vulnerability results.
                if (isBeetom)
                    ResolveBeetomShotAfterCommon(enemy, RequireBeetomState(enemy));

                hitCount++;
                break;
            }
        }
        return hitCount;
    }

    /// <summary>
    /// Ports <c>Process_Enemy_PowerBomb_Interaction</c> at $A0:A306 for one expansion
    /// sample. The caller supplies the high byte of the live power-bomb radius; native code
    /// uses it as the horizontal radius and derives a three-quarter-height vertical ellipse.
    /// </summary>
    public int ResolveOrdinaryPowerBombHits(
        ISnesAddressSpace bus,
        ushort explosionX,
        ushort explosionY,
        byte explosionRadius)
    {
        ArgumentNullException.ThrowIfNull(bus);
        EnsureLoaded();
        if (explosionRadius == 0)
            return 0;

        int horizontalRadius = explosionRadius;
        // `$A0:A31A-$A31D` retains carry from the first LSR through ADC. Expressing each
        // step makes odd-radius rounding agree with the 65C816 rather than using 0.75f.
        int carry = horizontalRadius & 1;
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + carry) >> 1;
        int reactionCount = 0;

        // The native pass walks all 32 physical slots from $07C0 down to zero, independent
        // of the ordinary active list. That matters while a power bomb reaches off-screen
        // actors and then sets their process-off-screen property below.
        for (int slotIndex = MaximumEnemyCount - 1; slotIndex >= 0; slotIndex--)
        {
            RoomEnemySlot enemy = _slots[slotIndex];
            if (enemy.EnemyDefinitionPointer is 0 or 0xdaff ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(EnemyProperties.Deleted))
            {
                continue;
            }

            byte vulnerability = ReadProjectileVulnerability(bus, enemy, 0x0500);
            if ((vulnerability & 0x7f) == 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(explosionX - enemy.XPosition)));
            int yDistance = Math.Abs(unchecked((short)(explosionY - enemy.YPosition)));
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            ushort reactionPointer = enemy.Definition.PowerBombReactionPointer;
            bool isFireflea = enemy.EnemyDefinitionPointer == FirefleaDefinition &&
                reactionPointer == FirefleaPowerBombAi;
            if (reactionPointer != 0 && !isFireflea)
            {
                throw new NotSupportedException(
                    $"Enemy ${enemy.EnemyDefinitionPointer:X4} power-bomb reaction " +
                    $"${enemy.Definition.Bank:X2}:{reactionPointer:X4} is not translated.");
            }

            // `$FF` reaches the reaction dispatcher because the outer admission check masks
            // bit seven, then common AI explicitly returns without damage. It still receives
            // property $0800 afterward, an observable quirk preserved below.
            if (vulnerability != 0xff)
            {
                int damage = 100 * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    enemy.InvincibilityTimer = 48;
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
                        if (isFireflea)
                            AdvanceFirefleaDarknessLevel();
                    }
                }
            }

            enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
            reactionCount++;
        }

        return reactionCount;
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
