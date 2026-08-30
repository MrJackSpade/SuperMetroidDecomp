using static SuperMetroid.Core.Hardware.SnesAddressMath;
using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

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
    private const ushort MetroidTouchAi = 0xedeb;
    private const ushort MetroidShotAi = 0xef07;
    private const ushort MetroidPowerBombAi = 0xf042;
    private const ushort ZebetiteTouchAi = 0xfda7;
    private const ushort ZebetiteShotAi = 0xfdac;
    private const ushort YardTouchAi = 0xd3b0;
    private const ushort YardShotAi = 0xd469;
    private const ushort BeetomTouchAi = 0xbe2e;
    private const ushort BeetomShotAi = 0xbeac;
    private const ushort PowampTouchAi = 0xc5be;
    private const ushort PowampShotAi = 0xc5ef;
    private const ushort PowampPowerBombAi = 0xc63f;
    private const ushort WorkRobotTouchAi = 0xd174;
    private const ushort WorkRobotNoPowerShotAi = 0xd18d;
    private const ushort WorkRobotShotAi = 0xd192;
    private const ushort BullShotAi = 0xdb14;
    private const ushort FakeKraidTouchAi = 0x9c22;
    private const ushort FakeKraidShotAi = 0x9c39;
    private const ushort SpacePiratePowerBombAi = 0x8767;
    private const ushort SpacePirateTouchAi = 0x876c;
    private const ushort SpacePirateShotAi = 0x8779;
    private const ushort MamaTurtleTouchAi = 0x9281;
    private const ushort BabyTurtleTouchAi = 0x929f;
    private const ushort BabyTurtleShotAi = 0x930f;
    private const ushort YappingMawTouchAi = 0xa799;
    private const ushort YappingMawShotAi = 0xa7bd;
    private const ushort GoldNinjaVulnerableHitboxShotAi = 0x87c8;
    private const ushort GoldNinjaInvincibleHitboxShotAi = 0x883e;
    private const ushort CrocomireHeaderTouchAi = 0xb950;
    private const ushort CrocomireClawTouchAi = 0xb93d;
    private const ushort CrocomireNoOpHitboxShotAi = 0xb951;
    private const ushort CrocomireDustHitboxShotAi = 0xb968;
    private const ushort CrocomireMouthShotAi = 0xba05;
    private const ushort CrocomireAlternateDustHitboxShotAi = 0xbab4;
    private const ushort CrocomirePowerBombAi = 0xb992;
    private const ushort SporeSpawnTouchAi = 0xedec;
    private const ushort SporeSpawnShotAi = 0xed5a;
    private const ushort SporeSpawnDudHitboxShotAi = 0x8046;
    private const ushort SporeSpawnNoOpHitboxTouchAi = 0x804c;
    private const ushort CeresSteamTouchAi = 0xf03f;
    private const ushort CeresSteamNoOpShotAi = 0x804c;
    private const ushort RidleyExtendedTouchAi = 0xdf59;
    private const ushort RidleyShotAi = 0xdf8a;
    private const ushort RidleyPowerBombAi = 0xdfb2;
    private const ushort MotherBrainBodyShotAi = 0xb503;
    private const ushort MotherBrainHeadShotAi = 0xb507;
    private const ushort DeadTorizoTouchAndShotAi = 0xd433;
    private const ushort DeadTorizoPowerBombAi = 0xd42a;
    private const ushort DefaultEnemyVulnerability = 0xec1c;

    /// <summary>Runs the common radius-based Samus/enemy touch pass for translated actors.</summary>
    public bool ResolveOrdinarySamusContact(
        SamusState samus,
        ushort controllerInput,
        RoomLevelData? level = null)
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
            bool isPowamp = slot.EnemyDefinitionPointer == PowampDefinition &&
                slot.Definition.TouchAiPointer == PowampTouchAi;
            bool isWorkRobot = IsWorkRobotDefinition(slot.EnemyDefinitionPointer) &&
                slot.Definition.TouchAiPointer == WorkRobotTouchAi;
            bool isFakeKraid = slot.EnemyDefinitionPointer == FakeKraidDefinition &&
                slot.Definition.TouchAiPointer == FakeKraidTouchAi;
            bool isOrdinarySpacePirate =
                IsOrdinarySpacePirateDefinition(slot.EnemyDefinitionPointer) &&
                slot.Definition.TouchAiPointer == SpacePirateTouchAi;
            bool isMamaTurtle = slot.EnemyDefinitionPointer == MamaTurtleDefinition &&
                slot.Definition.TouchAiPointer == MamaTurtleTouchAi;
            bool isBabyTurtle = slot.EnemyDefinitionPointer == BabyTurtleDefinition &&
                slot.Definition.TouchAiPointer == BabyTurtleTouchAi;
            bool isMagdollite = slot.EnemyDefinitionPointer == MagdolliteDefinition &&
                slot.Definition.TouchAiPointer == MagdolliteTouchAi;
            bool isRinka = slot.EnemyDefinitionPointer == RinkaDefinition &&
                slot.Definition.TouchAiPointer == RinkaTouchAi;
            bool isMaridiaLargeSnail =
                slot.EnemyDefinitionPointer == MaridiaLargeSnailDefinition &&
                slot.Definition.TouchAiPointer == MaridiaLargeSnailNonDamagingTouchAi;
            bool isDragon = slot.EnemyDefinitionPointer == DragonDefinition &&
                slot.Definition.TouchAiPointer == DragonTouchAi;
            bool isVerticalShutter = IsVerticalShutterDefinition(slot.EnemyDefinitionPointer) &&
                slot.Definition.TouchAiPointer == VerticalShutterTouchAi;
            bool isHorizontalShutter =
                slot.EnemyDefinitionPointer == ShootableHorizontalShutterDefinition &&
                slot.Definition.TouchAiPointer == HorizontalShutterTouchAi;
            bool isMetroid = slot.EnemyDefinitionPointer == MetroidDefinition &&
                slot.Definition.TouchAiPointer == MetroidTouchAi;
            bool isZebetite = slot.EnemyDefinitionPointer == ZebetiteDefinition &&
                slot.Definition.TouchAiPointer == ZebetiteTouchAi;
            bool isEvir = slot.EnemyDefinitionPointer == EvirDefinition &&
                slot.Parameter1 == 0 &&
                slot.Definition.TouchAiPointer == EvirTouchAi;
            bool isYappingMaw = slot.EnemyDefinitionPointer == YappingMawDefinition &&
                slot.Definition.TouchAiPointer == YappingMawTouchAi;
            bool isBotwoon = slot.EnemyDefinitionPointer == BotwoonDefinition &&
                slot.Definition.TouchAiPointer == BotwoonTouchAi;
            bool isSporeSpawn = slot.EnemyDefinitionPointer == SporeSpawnDefinition &&
                slot.Definition.TouchAiPointer == SporeSpawnTouchAi;
            bool isCeresSteam = slot.EnemyDefinitionPointer == CeresSteamDefinition &&
                slot.Definition.TouchAiPointer == CeresSteamTouchAi;
            bool isBombTorizo = slot.EnemyDefinitionPointer == BombTorizoDefinition &&
                slot.Definition.TouchAiPointer == BombTorizoTouchAi;
            bool isGoldenTorizo = slot.EnemyDefinitionPointer == GoldenTorizoDefinition &&
                slot.Definition.TouchAiPointer == BombTorizoTouchAi;
            bool isTorizo = isBombTorizo || isGoldenTorizo;
            bool isShaktool = slot.EnemyDefinitionPointer == ShaktoolDefinition &&
                slot.Definition.TouchAiPointer == ShaktoolTouchAi;
            bool isCrocomire = slot.EnemyDefinitionPointer == CrocomireDefinition &&
                slot.Definition.TouchAiPointer == CrocomireHeaderTouchAi;
            bool isCrocomireTongue = slot.EnemyDefinitionPointer == CrocomireTongueDefinition &&
                slot.Definition.TouchAiPointer == CommonNormalEnemyTouchAi;
            bool isPhantoon = slot.EnemyDefinitionPointer == PhantoonBodyDefinition &&
                slot.Definition.TouchAiPointer == PhantoonTouchHitboxCallback;
            bool isDraygonBody = slot.EnemyDefinitionPointer == DraygonBodyDefinition &&
                slot.Definition.TouchAiPointer == DraygonTouchAi;
            bool isDeadTorizo = slot.EnemyDefinitionPointer == DeadTorizoDefinition &&
                slot.Definition.TouchAiPointer == DeadTorizoTouchAndShotAi;
            // A handful of utility/terrain enemies intentionally point touch AI at an RTL
            // in their own bank. Detect the native opcode instead of adding a name-specific
            // exception for every inert actor. The collision is still reported, but the
            // callback performs no damage, knockback, or actor reaction.
            bool isLiteralNoOpTouchAi = slot.Definition.TouchAiPointer != 0 &&
                _bus!.ReadByte(
                    (slot.Definition.Bank << 16) |
                    slot.Definition.TouchAiPointer) == 0x6b;
            bool usesTranslatedTouchAi = slot.Definition.TouchAiPointer == CommonNormalEnemyTouchAi ||
                isPlatform ||
                isFireflea ||
                isBeetom ||
                isPowamp ||
                isWorkRobot ||
                isFakeKraid ||
                isOrdinarySpacePirate ||
                isMamaTurtle ||
                isBabyTurtle ||
                isMagdollite ||
                isRinka ||
                isMaridiaLargeSnail ||
                isDragon ||
                isVerticalShutter ||
                isHorizontalShutter ||
                isMetroid ||
                isZebetite ||
                isEvir ||
                isYappingMaw ||
                isBotwoon ||
                isSporeSpawn ||
                isCeresSteam ||
                isTorizo ||
                isShaktool ||
                isCrocomire ||
                isCrocomireTongue ||
                isPhantoon ||
                isDraygonBody ||
                isDeadTorizo ||
                isLiteralNoOpTouchAi ||
                slot.EnemyDefinitionPointer == MochtroidDefinition &&
                slot.Definition.TouchAiPointer == MochtroidTouchAi ||
                slot.EnemyDefinitionPointer == YardDefinition &&
                slot.Definition.TouchAiPointer == YardTouchAi;

            // Native touch collision does not interpret property $0100 as a collision bit;
            // it suppresses rendering only. Hibashi relies on that distinction: its second
            // slot remains invisible while its instruction stream moves a live hitbox.
            // Both Ridley definitions use a hand-authored extended body plus a separately
            // solved tail tip. ResolveRidleySamusContact owns that combined ordering; letting
            // this ordinary pass see either definition would either flatten the body to the
            // header's dummy 8x8 radius or apply body damage after an earlier tail hit.
            if (IsRidleyDefinition(slot.EnemyDefinitionPointer) ||
                !usesTranslatedTouchAi ||
                slot.SpritemapPointer == 0 ||
                slot.Properties.HasAny(EnemyProperties.Deleted))
            {
                continue;
            }

            bool usesExtendedHitboxes = (
                    isOrdinarySpacePirate || isMaridiaLargeSnail || isTorizo ||
                    isCrocomire || isCrocomireTongue || isSporeSpawn || isCeresSteam ||
                    isPhantoon || isDraygonBody) &&
                slot.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap);
            bool overlapsSamus;
            ushort hitboxTouchAi = slot.Definition.TouchAiPointer;
            if (usesExtendedHitboxes)
            {
                overlapsSamus = TryFindExtendedHitboxCallback(
                    slot,
                    samus.XPosition,
                    samus.YPosition,
                    samus.Kinematics.XRadius,
                    samus.Kinematics.YRadius,
                    selectShotCallback: false,
                    out hitboxTouchAi);
            }
            else
            {
                overlapsSamus = RadiusBoxesOverlap(
                    slot.XPosition,
                    slot.YPosition,
                    slot.XRadius,
                    slot.YRadius,
                    samus.XPosition,
                    samus.YPosition,
                    samus.Kinematics.XRadius,
                    samus.Kinematics.YRadius);
            }

            if (!overlapsSamus)
            {
                continue;
            }

            // `$A9:D433` is shared by touch and shot. Contact does not enter common damage;
            // it makes the corpse solid and starts the exact rotting table immediately.
            if (isDeadTorizo)
            {
                TriggerDeadTorizoRotting(slot);
                return true;
            }

            // Space Pirate extended maps currently use the shared `$B2:876C` touch
            // callback in every retail hitbox. Keep the pointer check explicit: silently
            // treating a later family-specific attack box as ordinary body contact would
            // recreate the exact radius-flattening bug this path is intended to remove.
            if (isOrdinarySpacePirate && usesExtendedHitboxes &&
                hitboxTouchAi != SpacePirateTouchAi)
            {
                throw new NotSupportedException(
                    $"Space Pirate hitbox touch AI $B2:{hitboxTouchAi:X4} is not translated.");
            }

            if (isTorizo && (!usesExtendedHitboxes || hitboxTouchAi != BombTorizoTouchAi))
            {
                throw new NotSupportedException(
                    $"Torizo hitbox touch AI $AA:{hitboxTouchAi:X4} is not translated.");
            }

            if (isPhantoon)
            {
                if (!usesExtendedHitboxes)
                {
                    throw new InvalidDataException(
                        "Phantoon requires his authored extended touch hitboxes.");
                }
                if (hitboxTouchAi == PhantoonNoOpHitboxCallback)
                    return true;
                if (hitboxTouchAi != PhantoonTouchHitboxCallback)
                {
                    throw new NotSupportedException(
                        $"Phantoon hitbox touch AI $A7:{hitboxTouchAi:X4} is not translated.");
                }

                ResolveNormalEnemyTouch(
                    slot,
                    samus,
                    controllerInput,
                    skipDeathAnimation: true);
                return true;
            }

            if (isDraygonBody)
            {
                if (!usesExtendedHitboxes)
                {
                    throw new InvalidDataException(
                        "Draygon requires his authored extended touch hitboxes.");
                }
                if (hitboxTouchAi == DraygonNoOpHitboxTouchAi)
                    return true; // The vulnerable eye rectangle is harmless to Samus.
                if (hitboxTouchAi != DraygonTouchAi)
                {
                    throw new NotSupportedException(
                        $"Draygon hitbox touch AI $A5:{hitboxTouchAi:X4} is not translated.");
                }

                ResolveNormalEnemyTouch(
                    slot,
                    samus,
                    controllerInput,
                    skipDeathAnimation: true);
                ResolveDraygonReaction(slot, samus);
                return true;
            }

            if (isSporeSpawn)
            {
                if (!usesExtendedHitboxes)
                {
                    throw new InvalidDataException(
                        "Spore Spawn requires its authored extended-spritemap hitboxes.");
                }
                if (hitboxTouchAi == SporeSpawnNoOpHitboxTouchAi)
                    return true;
                if (hitboxTouchAi != SporeSpawnTouchAi)
                {
                    throw new NotSupportedException(
                        $"Spore Spawn hitbox touch AI $A5:{hitboxTouchAi:X4} is not translated.");
                }

                ResolveNormalEnemyTouch(
                    slot,
                    samus,
                    controllerInput,
                    skipDeathAnimation: true);
                ResolveSporeSpawnDeathAfterCommon(slot);
                return true;
            }

            if (isCeresSteam)
            {
                // Every visible steam frame stores `$A6:F03F` in its authored hitbox.
                // The last two dispersal frames use an empty hitbox list and cannot reach
                // this branch. Insisting on both facts keeps collision tied to ROM geometry.
                if (!usesExtendedHitboxes || hitboxTouchAi != CeresSteamTouchAi)
                {
                    throw new InvalidDataException(
                        $"Ceres steam requires extended touch AI $A6:{CeresSteamTouchAi:X4}; " +
                        $"extended={usesExtendedHitboxes}, selected=$A6:{hitboxTouchAi:X4}, " +
                        $"map=$A6:{slot.SpritemapPointer:X4}.");
                }

                // `$A6:F03F` restores health immediately before entering common touch AI.
                // Header damage is deliberately zero: contact still installs the common
                // invincibility/knockback state, while Screw/Speed cannot destroy the vent.
                slot.Health = CeresSteamIndestructibleHealth;
                ResolveNormalEnemyTouch(slot, samus, controllerInput);
                return true;
            }

            if (isCrocomire || isCrocomireTongue)
            {
                if (!usesExtendedHitboxes)
                {
                    throw new InvalidDataException(
                        "Crocomire components require extended-spritemap collision.");
                }
                if (hitboxTouchAi == CrocomireHeaderTouchAi)
                    return true; // $A4:B950 is a literal RTL.
                if (hitboxTouchAi is not (
                        CrocomireClawTouchAi or CommonNormalEnemyTouchAi))
                {
                    throw new NotSupportedException(
                        $"Crocomire hitbox touch AI $A4:{hitboxTouchAi:X4} is not translated.");
                }

                ResolveNormalEnemyTouch(slot, samus, controllerInput);
                if (hitboxTouchAi == CrocomireClawTouchAi)
                {
                    CrocomireEnemyState crocomire = _crocomire ??
                        throw new InvalidOperationException("Crocomire claw has no body owner.");
                    crocomire.FightFlags |= 0x4000;
                    samus.Kinematics.ExtraXDisplacement = unchecked((ushort)-4);
                }
                return true;
            }

            if (isMaridiaLargeSnail)
            {
                if (!usesExtendedHitboxes)
                {
                    throw new InvalidDataException(
                        "Maridia Large Snail lost its required extended-spritemap property.");
                }
                if (hitboxTouchAi is not (
                        MaridiaLargeSnailDamagingTouchAi or
                        MaridiaLargeSnailNonDamagingTouchAi))
                {
                    throw new NotSupportedException(
                        $"Maridia Large Snail hitbox touch AI " +
                        $"$A2:{hitboxTouchAi:X4} is not translated.");
                }

                // $D388 enters common touch damage and falls directly through $D38C's
                // asymmetric shove. $D38C frames execute only the shove, which is why the
                // closed shell can carry/push Samus without inflicting the header's 100 HP.
                if (hitboxTouchAi == MaridiaLargeSnailDamagingTouchAi)
                    ResolveNormalEnemyTouch(slot, samus, controllerInput);
                ResolveMaridiaLargeSnailTouchAfterCommon(slot, samus);
                return true;
            }

            if (isOrdinarySpacePirate && slot.FrozenTimer != 0)
            {
                // `$B2:876C` returns before common touch AI while frozen. The overlap was
                // still found, but neither Samus nor the Pirate receives contact damage.
            }
            else if (isPlatform)
            {
                // `$A3:9F07` is a literal RTL. Platform solidity and the asymmetric rider
                // test live in other handlers; ordinary body overlap must neither injure
                // Samus nor synthesize knockback here.
            }
            else if (isLiteralNoOpTouchAi)
            {
                // The bank-local callback is literally RTL. Keep this after the platform
                // branch so platform-specific documentation remains attached to its family,
                // while all other inert actors share the real dispatcher semantics.
            }
            else if (isVerticalShutter)
            {
                // $F09D does not enter common contact damage. Body overlap is itself the
                // trigger, and $F0B6 decides whether this population mode may start moving.
                ReactVerticalShutter(slot, _shutterCameraX, _shutterCameraY);
            }
            else if (isHorizontalShutter)
            {
                // $F3D8 provides manual ejection only after a permanently stopped shutter.
                // The main AI's subsequent nonzero contact-damage-index check reuses the
                // shot reaction, allowing Screw/Speed contact to trigger modes three/four.
                TouchHorizontalShutter(slot, samus, controllerInput);
                if (contactDamageIndex != 0)
                    ReactHorizontalShutter(slot);
            }
            else if (slot.EnemyDefinitionPointer == MochtroidDefinition)
            {
                ResolveMochtroidTouch(
                    slot,
                    RequireMochtroidState(slot),
                    samus,
                    controllerInput);
            }
            else if (isMetroid)
            {
                ResolveMetroidTouch(slot, samus);
            }
            else if (isYappingMaw)
            {
                // $A8:A799 replaces common contact damage entirely. A qualifying overlap
                // transfers Samus input/position ownership to the mouth state machine.
                ResolveYappingMawTouch(RequireYappingMawState(slot), samus);
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
            else if (isPowamp)
            {
                // init1 is zero only for the body before fatal shot damage. Balloons are
                // normally absent from the interactive list because population property
                // $0400 excludes them, but retain the handler's literal guard here too.
                if (slot.Parameter2 == 0)
                    ResolvePowampTouch(slot, samus, controllerInput);
            }
            else if (isWorkRobot)
            {
                ResolveWorkRobotTouch(slot, samus);
            }
            else if (isFireflea)
            {
                ResolveFirefleaTouch(slot, samus, controllerInput);
            }
            else if (isMamaTurtle)
            {
                ResolveMamaTurtleTouch(
                    slot,
                    RequireMamaTurtleState(slot),
                    samus,
                    controllerInput);
            }
            else if (isBabyTurtle)
            {
                ResolveBabyTurtleTouch(
                    slot,
                    RequireBabyTurtleState(slot),
                    samus,
                    level);
            }
            else
            {
                ushort healthBefore = slot.Health;
                ResolveNormalEnemyTouch(
                    slot,
                    samus,
                    controllerInput,
                    skipDeathAnimation: isRinka || isZebetite || isBotwoon || isTorizo);
                if (isMagdollite)
                    ResolveMagdolliteCombatAfterCommon(slot);
                if (isRinka)
                    ResolveRinkaCombatAfterCommon(slot);
                if (isDragon)
                    ResolveDragonCombatAfterCommon(slot);
                if (isEvir)
                    ResolveEvirCombatAfterCommon(slot);
                if (isBotwoon)
                    ResolveBotwoonCombatAfterCommon(slot);
                if (isTorizo && slot.Health == 0)
                    BeginBombTorizoDeath(slot, RequireBombTorizoState(slot));
                if (isFakeKraid && healthBefore != 0 && slot.Health == 0)
                    RequestFakeKraidDeathDrop(slot);
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
            bool isPowamp = enemy.EnemyDefinitionPointer == PowampDefinition &&
                enemy.Definition.ShotAiPointer == PowampShotAi;
            bool isWorkRobot = IsWorkRobotDefinition(enemy.EnemyDefinitionPointer) &&
                enemy.Definition.ShotAiPointer is WorkRobotShotAi or WorkRobotNoPowerShotAi;
            bool isBull = enemy.EnemyDefinitionPointer == BullDefinition &&
                enemy.Definition.ShotAiPointer == BullShotAi;
            bool isSpark = enemy.EnemyDefinitionPointer == SparkDefinition &&
                enemy.Definition.ShotAiPointer == SparkShotAi;
            bool isBlueBrinstarFaceBlock =
                enemy.EnemyDefinitionPointer == BlueBrinstarFaceBlockDefinition &&
                enemy.Definition.ShotAiPointer == BlueBrinstarFaceBlockShotAi;
            bool isFakeKraid = enemy.EnemyDefinitionPointer == FakeKraidDefinition &&
                enemy.Definition.ShotAiPointer == FakeKraidShotAi;
            bool isOrdinarySpacePirate =
                IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
                enemy.Definition.ShotAiPointer == SpacePirateShotAi;
            bool isBabyTurtle = enemy.EnemyDefinitionPointer == BabyTurtleDefinition &&
                enemy.Definition.ShotAiPointer == BabyTurtleShotAi;
            bool isOwtch = enemy.EnemyDefinitionPointer == OwtchDefinition &&
                enemy.Definition.ShotAiPointer == OwtchShotAi;
            bool isKago = enemy.EnemyDefinitionPointer == KagoDefinition &&
                enemy.Definition.ShotAiPointer == KagoShotAi;
            bool isMagdollite = enemy.EnemyDefinitionPointer == MagdolliteDefinition &&
                enemy.Definition.ShotAiPointer == MagdolliteShotAi;
            bool isRinka = enemy.EnemyDefinitionPointer == RinkaDefinition &&
                enemy.Definition.ShotAiPointer == RinkaShotAi;
            bool isMaridiaLargeSnail =
                enemy.EnemyDefinitionPointer == MaridiaLargeSnailDefinition &&
                enemy.Definition.ShotAiPointer == MaridiaLargeSnailShotAi;
            bool isGRipperOrRipper2 =
                enemy.EnemyDefinitionPointer is GRipperDefinition or Ripper2Definition &&
                enemy.Definition.ShotAiPointer == GRipperRipper2ShotAi;
            bool isDragon = enemy.EnemyDefinitionPointer == DragonDefinition &&
                enemy.Definition.ShotAiPointer == DragonShotAi;
            bool isReactionOnlyVerticalShutter =
                enemy.EnemyDefinitionPointer is
                    ShootableVerticalShutterDefinition or KamerVerticalPlatformDefinition &&
                enemy.Definition.ShotAiPointer == ShootableVerticalShutterShotAi;
            bool isDestroyableVerticalShutter =
                enemy.EnemyDefinitionPointer == DestroyableVerticalShutterDefinition &&
                enemy.Definition.ShotAiPointer == DestroyableVerticalShutterShotAi;
            bool isHorizontalShutter =
                enemy.EnemyDefinitionPointer == ShootableHorizontalShutterDefinition &&
                enemy.Definition.ShotAiPointer == HorizontalShutterShotAi;
            bool isMetroid = enemy.EnemyDefinitionPointer == MetroidDefinition &&
                enemy.Definition.ShotAiPointer == MetroidShotAi;
            bool isZebetite = enemy.EnemyDefinitionPointer == ZebetiteDefinition &&
                enemy.Definition.ShotAiPointer == ZebetiteShotAi;
            bool isEvir = enemy.EnemyDefinitionPointer == EvirDefinition &&
                enemy.Parameter1 == 0 &&
                enemy.Definition.ShotAiPointer == EvirShotAi;
            bool isYappingMaw = enemy.EnemyDefinitionPointer == YappingMawDefinition &&
                enemy.Definition.ShotAiPointer == YappingMawShotAi;
            bool isKiHunter = IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                enemy.Definition.ShotAiPointer == KiHunterShotAi;
            bool isBotwoon = enemy.EnemyDefinitionPointer == BotwoonDefinition &&
                enemy.Definition.ShotAiPointer == BotwoonShotAi;
            bool isSporeSpawn = enemy.EnemyDefinitionPointer == SporeSpawnDefinition &&
                enemy.Definition.ShotAiPointer == SporeSpawnShotAi;
            bool isCeresSteam = enemy.EnemyDefinitionPointer == CeresSteamDefinition &&
                enemy.Definition.ShotAiPointer == CeresSteamNoOpShotAi;
            bool isNorfairRidley = enemy.EnemyDefinitionPointer == NorfairRidleyDefinition &&
                enemy.Definition.ShotAiPointer == RidleyShotAi;
            bool isBombTorizo = enemy.EnemyDefinitionPointer == BombTorizoDefinition &&
                enemy.Definition.ShotAiPointer == BombTorizoShotAi;
            bool isGoldenTorizo = enemy.EnemyDefinitionPointer == GoldenTorizoDefinition &&
                enemy.Definition.ShotAiPointer == GoldenTorizoShotAi;
            bool isTorizo = isBombTorizo || isGoldenTorizo;
            bool isShaktool = enemy.EnemyDefinitionPointer == ShaktoolDefinition &&
                enemy.Definition.ShotAiPointer == ShaktoolShotAi;
            bool isCrocomire = enemy.EnemyDefinitionPointer == CrocomireDefinition &&
                enemy.Definition.ShotAiPointer == 0;
            bool isCrocomireTongue =
                enemy.EnemyDefinitionPointer == CrocomireTongueDefinition &&
                enemy.Definition.ShotAiPointer == CommonNormalEnemyShotAi;
            bool isDraygonBody = enemy.EnemyDefinitionPointer == DraygonBodyDefinition &&
                enemy.Definition.ShotAiPointer == DraygonShotAi;
            bool isMotherBrainBody = enemy.EnemyDefinitionPointer == MotherBrainBodyDefinition &&
                enemy.Definition.ShotAiPointer == MotherBrainBodyShotAi;
            bool isMotherBrainHead = enemy.EnemyDefinitionPointer == MotherBrainHeadDefinition &&
                enemy.Definition.ShotAiPointer == MotherBrainHeadShotAi;
            bool isDeadTorizo = enemy.EnemyDefinitionPointer == DeadTorizoDefinition &&
                enemy.Definition.ShotAiPointer == DeadTorizoTouchAndShotAi;
            // Several retail helper/projectile definitions point their shot callback at a
            // literal RTL in their own enemy bank. The bank-$A0 collision walker still runs
            // its projectile prelude before dispatching that no-op callback: supers request
            // quake and ordinary non-plasma shots receive direction bit $10, but no impact,
            // vulnerability lookup, damage, freeze, or death follows. Recognize the opcode
            // from ROM instead of maintaining another speculative list of enemy names.
            bool isLiteralNoOpShotAi = enemy.Definition.ShotAiPointer != 0 &&
                bus.ReadByte(
                    (enemy.Definition.Bank << 16) |
                    enemy.Definition.ShotAiPointer) == 0x6b;
            bool usesTranslatedShotAi = enemy.Definition.ShotAiPointer == CommonNormalEnemyShotAi ||
                enemy.EnemyDefinitionPointer == SkreeDefinition &&
                enemy.Definition.ShotAiPointer == SkreeShotAi ||
                isMetaree ||
                isFireflea ||
                isTripper ||
                isBeetom ||
                isPowamp ||
                isWorkRobot ||
                isBull ||
                isSpark ||
                isBlueBrinstarFaceBlock ||
                isFakeKraid ||
                isOrdinarySpacePirate ||
                isBabyTurtle ||
                isOwtch ||
                isKago ||
                isMagdollite ||
                isRinka ||
                isMaridiaLargeSnail ||
                isGRipperOrRipper2 ||
                isDragon ||
                isReactionOnlyVerticalShutter ||
                isDestroyableVerticalShutter ||
                isHorizontalShutter ||
                isMetroid ||
                isZebetite ||
                isEvir ||
                isYappingMaw ||
                isKiHunter ||
                isBotwoon ||
                isSporeSpawn ||
                isCeresSteam ||
                isNorfairRidley ||
                isTorizo ||
                isShaktool ||
                isCrocomire ||
                isCrocomireTongue ||
                isDraygonBody ||
                isMotherBrainBody ||
                isMotherBrainHead ||
                isDeadTorizo ||
                isLiteralNoOpShotAi ||
                enemy.EnemyDefinitionPointer == MochtroidDefinition &&
                enemy.Definition.ShotAiPointer == MochtroidShotAi ||
                isYard;

            // The shot pass likewise admits visually invisible actors, but rejects the
            // engine's explicit empty spritemap sentinel $804D and property $0400. Keeping
            // these meanings separate mirrors the bank-$A0 list/collision machinery.
            if (enemy.EnemyDefinitionPointer == CeresRidleyDefinition ||
                !usesTranslatedShotAi ||
                enemy.SpritemapPointer is 0 or 0x804d ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(
                    EnemyProperties.Deleted |
                    EnemyProperties.IgnoreSamusCollision))
            {
                continue;
            }

            // Powered definition $E8FF explicitly returns before common shot AI while
            // Phantoon's area-boss bit is clear. The projectile therefore does not begin an
            // impact, and the deactivated-looking body does not enter its recoil sequence.
            if (enemy.EnemyDefinitionPointer == WorkRobotDefinition &&
                !(_isAreaBossDefeated?.Invoke() ?? false))
            {
                continue;
            }

            // Powamp writes one to body init1 as soon as a lethal shot starts its private
            // 32-frame death. Later overlaps return before common shot AI, so they must not
            // consume or explode the projectile while the body is still visibly deflating.
            if (isPowamp && enemy.Parameter2 != 0)
                continue;

            foreach (SamusProjectileSlot projectile in projectiles.Slots)
            {
                // `$0C18,x` is still the live beam/missile type when the enemy collision
                // handler selects vulnerability. Starting the impact below deliberately
                // rewrites that slot to explosion family `$0700`, so retain the incoming
                // word instead of accidentally asking the vulnerability table about the
                // newly-created visual effect.
                ushort projectileType = projectile.Type;
                ushort projectileDamage = projectile.Damage;
                ushort projectileDirection = projectile.Direction;
                ushort family = unchecked((ushort)(projectileType & 0x0f00));
                if (!projectile.IsActive)
                    continue;

                // Bomb, power-bomb, and pseudo-screw actors share the projectile slot array,
                // but ordinary enemy shot AI never interprets them as beam/missile records.
                // Yard is the one translated exception: its private $A3:D469 handler treats
                // bomb-family impacts as a physical kick, so those actors must reach its
                // custom branch below. Keep this as an explicit guard instead of embedding it
                // in the overlap expression; doing so makes the native dispatch boundary clear.
                if (!isYard && !isMetroid &&
                    (family == 0x0300 || family == 0x0500 || family == 0x0700))
                {
                    continue;
                }

                bool usesExtendedHitboxes = (
                        isOrdinarySpacePirate || isMaridiaLargeSnail || isTorizo ||
                        isCrocomire || isCrocomireTongue || isSporeSpawn || isCeresSteam ||
                        isNorfairRidley || isDraygonBody) &&
                    enemy.ExtraProperties.HasAny(EnemyExtraProperties.UsesExtendedSpritemap);
                bool overlapsProjectile;
                ushort hitboxShotAi = enemy.Definition.ShotAiPointer;
                if (usesExtendedHitboxes)
                {
                    overlapsProjectile = TryFindExtendedHitboxCallback(
                        enemy,
                        projectile.XPosition,
                        projectile.YPosition,
                        projectile.XRadius,
                        projectile.YRadius,
                        selectShotCallback: true,
                        out hitboxShotAi);
                }
                else
                {
                    overlapsProjectile = RadiusBoxesOverlap(
                        enemy.XPosition,
                        enemy.YPosition,
                        enemy.XRadius,
                        enemy.YRadius,
                        projectile.XPosition,
                        projectile.YPosition,
                        projectile.XRadius,
                        projectile.YRadius);
                }

                if (!overlapsProjectile)
                {
                    continue;
                }

                if (isDeadTorizo)
                {
                    // Its private callback runs after the ordinary collision prelude but
                    // before common impact/vulnerability code. The shot is marked, not
                    // converted into an explosion, and the corpse never loses health.
                    projectiles.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    TriggerDeadTorizoRotting(enemy);
                    hitCount++;
                    break;
                }

                if (isMotherBrainBody)
                {
                    // Body callback `$A9:B503` is exactly CreateDudShot. The common radius
                    // collision prelude still owns Super-Missile quake before that callback.
                    projectiles.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    CreateEnemyProjectileDudShot(projectile);
                    hitCount++;
                    break;
                }

                if (isMotherBrainHead)
                {
                    if (!ResolveMotherBrainHeadShot(
                        bus,
                        enemy,
                        projectile,
                        projectiles,
                        sharedProjectiles,
                        projectileType,
                        projectileDamage))
                    {
                        continue;
                    }
                    hitCount++;
                    break;
                }

                if (isNorfairRidley &&
                    (!usesExtendedHitboxes || hitboxShotAi != RidleyShotAi))
                {
                    throw new InvalidDataException(
                        $"Lower Norfair Ridley requires extended shot AI " +
                        $"$A6:{RidleyShotAi:X4}; extended={usesExtendedHitboxes}, " +
                        $"selected=$A6:{hitboxShotAi:X4}, map=$A6:{enemy.SpritemapPointer:X4}.");
                }

                if (isDraygonBody)
                {
                    if (!usesExtendedHitboxes)
                    {
                        throw new InvalidDataException(
                            "Draygon requires his authored extended shot hitboxes.");
                    }
                    if (hitboxShotAi == DraygonDudHitboxShotAi)
                    {
                        // Body, claws, and shell use shared `$A0:8046`; only the first
                        // eye rectangle in maps $1A/$2D reaches Draygon's damage callback.
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            (enemy.Properties & 0x1000) != 0 ||
                                (projectile.Type & 0x0008) == 0);
                        CreateEnemyProjectileDudShot(projectile);
                        hitCount++;
                        break;
                    }
                    if (hitboxShotAi != DraygonShotAi)
                    {
                        throw new NotSupportedException(
                            $"Draygon hitbox shot AI $A5:{hitboxShotAi:X4} is not translated.");
                    }
                }

                if (usesExtendedHitboxes && family == 0x0200)
                {
                    // The multibox prelude requests this earthquake before dispatching the
                    // hitbox callback, even when that callback is the no-op shell region.
                    EarthquakeTimer = 30;
                    EarthquakeType = 18;
                }

                // Golden's definition installs $D667, but every authored extended body
                // hitbox calls shared $C97C; that wrapper dispatches to $D667 by area.
                if (isTorizo &&
                    (!usesExtendedHitboxes || hitboxShotAi != BombTorizoShotAi))
                {
                    throw new NotSupportedException(
                        $"Torizo hitbox shot AI $AA:{hitboxShotAi:X4} is not translated.");
                }

                if (isMaridiaLargeSnail)
                {
                    if (!usesExtendedHitboxes)
                    {
                        throw new InvalidDataException(
                            "Maridia Large Snail lost its required extended-spritemap property.");
                    }
                    if (hitboxShotAi == MaridiaLargeSnailNoOpHitboxAi)
                    {
                        // Native multibox collision marks the projectile before calling the
                        // no-op. It does not run normal shot AI, create an impact, play $57,
                        // or consult vulnerability for this protected shell component.
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            (enemy.Properties & 0x1000) != 0 ||
                                (projectile.Type & 0x0008) == 0);
                        hitCount++;
                        break;
                    }
                    if (hitboxShotAi != MaridiaLargeSnailShotAi)
                    {
                        throw new NotSupportedException(
                            $"Maridia Large Snail hitbox shot AI " +
                            $"$A2:{hitboxShotAi:X4} is not translated.");
                    }
                }

                if (isSporeSpawn)
                {
                    if (!usesExtendedHitboxes)
                    {
                        throw new InvalidDataException(
                            "Spore Spawn requires its authored extended-spritemap hitboxes.");
                    }
                    if (hitboxShotAi == SporeSpawnDudHitboxShotAi)
                    {
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            (enemy.Properties & 0x1000) != 0 ||
                                (projectile.Type & 0x0008) == 0);
                        CreateEnemyProjectileDudShot(projectile);
                        hitCount++;
                        break;
                    }
                    if (hitboxShotAi != SporeSpawnShotAi)
                    {
                        throw new NotSupportedException(
                            $"Spore Spawn hitbox shot AI $A5:{hitboxShotAi:X4} is not translated.");
                    }
                    if (!SporeSpawnAcceptsProjectile(projectileType))
                    {
                        // The extended collision walker has already selected the vulnerable
                        // core. $ED5A rejects this projectile family before common damage.
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            (enemy.Properties & 0x1000) != 0 ||
                                (projectile.Type & 0x0008) == 0);
                        hitCount++;
                        break;
                    }
                }

                if (isCeresSteam)
                {
                    // All active plume hitboxes use the definition's literal `$804C` RTL.
                    // The shared extended collision walker still marks the projectile (and
                    // requests Super-Missile quake) before that callback returns; it does
                    // not create an impact, consult vulnerability, or damage the steam.
                    if (!usesExtendedHitboxes || hitboxShotAi != CeresSteamNoOpShotAi)
                    {
                        throw new InvalidDataException(
                            $"Ceres steam requires extended shot AI " +
                            $"$A6:{CeresSteamNoOpShotAi:X4}.");
                    }

                    projectiles.ApplyExtendedEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    hitCount++;
                    break;
                }

                if ((isCrocomire || isCrocomireTongue) &&
                    hitboxShotAi != CommonNormalEnemyShotAi)
                {
                    if (!usesExtendedHitboxes)
                    {
                        throw new InvalidDataException(
                            "Crocomire components require extended-spritemap collision.");
                    }

                    bool markCollision = (enemy.Properties & 0x1000) != 0 ||
                        (projectile.Type & 0x0008) == 0;
                    projectiles.ApplyExtendedEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        markCollision);
                    ResolveCrocomireHitboxShot(
                        enemy,
                        projectile,
                        hitboxShotAi);
                    hitCount++;
                    break;
                }

                // Owtch's private $A2:A579 callback returns before common shot AI unless
                // signed(state - 1) is negative. The bank-$A0 collision prelude has already
                // marked ordinary non-plasma shots as collided at this point, but it has not
                // converted them into an impact animation. Preserve that split so the shell
                // is genuinely immune while moving right, sinking, buried, or rising.
                if (isOwtch && !OwtchAcceptsOrdinaryShot(RequireOwtchState(enemy)))
                {
                    if ((enemy.Properties & 0x1000) != 0 || (projectile.Type & 0x0008) == 0)
                        projectile.Direction = unchecked((ushort)(projectile.Direction | 0x0010));
                    hitCount++;
                    break;
                }

                if (isOrdinarySpacePirate && usesExtendedHitboxes)
                {
                    PirateHitboxShotAction pirateAction = SelectPirateHitboxShotAction(
                        bus,
                        enemy,
                        projectile,
                        hitboxShotAi);
                    if (pirateAction != PirateHitboxShotAction.Normal)
                    {
                        // `$A0:9CD1-$9CD6` marks a non-plasma projectile as having entered
                        // an extended hitbox before the selected callback runs. Normal hits
                        // get the equivalent lifecycle transition from TryStartEnemyImpact;
                        // rejected and reflected hits return before that shared host path.
                        // Enemy property bit $1000 also forces the mark in native code, but
                        // its broader meaning is not yet proven, so it deliberately remains
                        // a documented raw bit rather than a prematurely named enum member.
                        bool forceCollisionState = (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0;
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            forceCollisionState);
                    }
                    if (pirateAction == PirateHitboxShotAction.Ignore)
                    {
                        // `$B2:883E` deliberately returns for a just-fired Super Missile
                        // whose link variable is still zero. The multibox collision walk
                        // nevertheless exits after invoking that callback.
                        hitCount++;
                        break;
                    }
                    if (pirateAction == PirateHitboxShotAction.Reflect)
                    {
                        enemy.InvincibilityTimer = 10;
                        projectile.Direction = (projectile.Direction & 0x000f) switch
                        {
                            (ushort)SamusProjectileDirection.Left =>
                                (ushort)SamusProjectileDirection.UpRight,
                            (ushort)SamusProjectileDirection.Right =>
                                (ushort)SamusProjectileDirection.UpLeft,
                            _ => (ushort)SamusProjectileDirection.DownFacingLeft,
                        };
                        projectiles.ReflectFromEnemy(bus, projectile.SlotIndex);
                        LastSpacePirateSoundEffect = 0x0066;
                        hitCount++;
                        break;
                    }
                }

                // Spark's private `$A8:E70E` handler never enters common shot AI. It only
                // clears direction bit $10 on the colliding Samus projectile, allowing the
                // beam/missile actor to continue rather than becoming an impact animation.
                // This must occur before TryStartEnemyImpact, which would irreversibly
                // rewrite the projectile family to explosion `$0700`.
                if (isSpark || isBlueBrinstarFaceBlock)
                {
                    projectile.Direction = unchecked((ushort)(projectile.Direction & 0xffef));
                    hitCount++;
                    break;
                }

                if (isReactionOnlyVerticalShutter)
                {
                    // $F0A2 goes directly to the shutter reaction instead of normal shot
                    // AI. The bank-$A0 collision walker has nevertheless already marked the
                    // colliding projectile, including Super Missile quake side effects.
                    projectiles.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    ReactVerticalShutter(enemy, _shutterCameraX, _shutterCameraY);
                    hitCount++;
                    break;
                }

                if (isMetroid)
                {
                    if (projectile.PackedDirection.HasLowByteLifecycleState)
                        continue;
                    if (enemy.FrozenTimer != 0)
                    {
                        if (family != (ushort)SamusProjectileFamily.Missile &&
                            family != (ushort)SamusProjectileFamily.SuperMissile)
                        {
                            // Frozen Metroid rejects every beam and bomb family. Bank-$A0
                            // has already accepted the overlap, so non-plasma shots retain
                            // only the collision mark and never enter normal damage AI.
                            projectiles.ApplyEnemyCollisionPrelude(
                                projectile.SlotIndex,
                                (enemy.Properties & 0x1000) != 0 ||
                                    (projectile.Type & 0x0008) == 0);
                            hitCount++;
                            break;
                        }

                        if (!projectiles.TryStartEnemyImpact(
                                bus,
                                sharedProjectiles,
                                projectile.SlotIndex))
                        {
                            continue;
                        }

                        byte frozenVulnerability = ReadProjectileVulnerability(
                            bus,
                            enemy,
                            projectileType);
                        int frozenDamage = (projectileDamage >> 1) *
                            (frozenVulnerability & 0x7f);
                        if (frozenVulnerability != 0xff && frozenDamage != 0)
                        {
                            ushort hurtTime = enemy.HurtAiTime == 0
                                ? (ushort)4
                                : enemy.HurtAiTime;
                            enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
                            enemy.AiHandlerBits |= 0x0002;
                            enemy.Health = frozenDamage >= enemy.Health
                                ? (ushort)0
                                : unchecked((ushort)(enemy.Health - frozenDamage));
                        }

                        if (enemy.Health == 0)
                        {
                            enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                            EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                            FinishMetroidDeath(enemy, samus, requestDrops: true);
                        }

                        hitCount++;
                        break;
                    }

                    // Custom Metroid shot AI does not call normal shot AI. Preserve the
                    // collision prelude, then apply recoil/ice or attached power-bomb escape
                    // without inventing vulnerability-based health damage.
                    projectiles.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    ResolveMetroidNonFrozenShot(
                        enemy,
                        projectile.Type,
                        projectile.Damage,
                        projectiles.Slots[0].XPosition,
                        projectiles.Slots[0].YPosition,
                        samus);
                    hitCount++;
                    break;
                }

                if (isLiteralNoOpShotAi)
                {
                    projectiles.ApplyEnemyCollisionPrelude(
                        projectile.SlotIndex,
                        (enemy.Properties & 0x1000) != 0 ||
                            (projectile.Type & 0x0008) == 0);
                    hitCount++;
                    break;
                }

                if (isTorizo)
                {
                    TorizoEnemyState torizoState = RequireBombTorizoState(enemy);
                    if (enemy.FlashTimer != 0 || isBombTorizo && torizoState.ShotGuard != 0)
                    {
                        // Both callbacks return during a damage flash. Bomb Torizo also
                        // rejects shots while its awakening guard is set; Golden Torizo's
                        // nonzero guard instead routes through common damage below.
                        projectiles.ApplyExtendedEnemyCollisionPrelude(
                            projectile.SlotIndex,
                            (enemy.Properties & 0x1000) != 0 ||
                                (projectile.Type & 0x0008) == 0);
                        hitCount++;
                        break;
                    }

                    if (isGoldenTorizo && torizoState.ShotGuard == 0 &&
                        (enemy.Parameter2 & 0x1000) == 0)
                    {
                        torizoState.CapturedProjectileFamily = family;
                        if (family == (ushort)SamusProjectileFamily.Missile)
                        {
                            projectiles.ApplyExtendedEnemyCollisionPrelude(
                                projectile.SlotIndex,
                                (enemy.Properties & 0x1000) != 0 ||
                                    (projectile.Type & 0x0008) == 0);
                            projectile.Direction &= 0xffef;
                            torizoState.Function = TorizoFunctionIdle;
                            enemy.InstructionTimer = 1;
                            enemy.CurrentInstruction = (enemy.Parameter1 & 0x8000) != 0
                                ? (ushort)0xd2ad
                                : (ushort)0xd1f1;
                            hitCount++;
                            break;
                        }

                        if (family == (ushort)SamusProjectileFamily.SuperMissile)
                        {
                            if (samus is not null &&
                                BombTorizoFunction12IsNonNegative(enemy, samus))
                            {
                                projectiles.ApplyExtendedEnemyCollisionPrelude(
                                    projectile.SlotIndex,
                                    (enemy.Properties & 0x1000) != 0 ||
                                        (projectile.Type & 0x0008) == 0);
                                enemy.Parameter2 |= 0x1000;
                                torizoState.Function = TorizoFunctionIdle;
                                projectile.Direction |= 0x0010;
                                enemy.InstructionTimer = 1;
                                enemy.CurrentInstruction = (enemy.Parameter1 & 0x2000) != 0
                                    ? (enemy.Parameter1 & 0x8000) != 0
                                        ? (ushort)0xceff
                                        : (ushort)0xce43
                                    : (enemy.Parameter1 & 0x8000) != 0
                                        ? (ushort)0xcea5
                                        : (ushort)0xcde1;
                                hitCount++;
                                break;
                            }
                        }
                        else
                        {
                            // Beams and other common-damage families arm the high-health
                            // counterattack branch before entering normal shot AI.
                            enemy.Parameter2 |= 0x2000;
                        }
                    }
                }

                if (!projectiles.TryStartEnemyImpact(bus, sharedProjectiles, projectile.SlotIndex))
                    continue;

                if (isDraygonBody)
                    AdvanceDraygonShotAcceleration(RequireCompleteDraygonState(enemy));

                ushort enemyHealthBefore = enemy.Health;

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
                    if (isPowamp)
                        ResolvePowampShotAfterCommon(enemy);
                    if (isWorkRobot)
                        ResolveWorkRobotShotAfterCommon(enemy, samus);
                    if (isBull)
                    {
                        BullEnemyState bullState = RequireBullState(enemy);
                        bullState.PreviousHealth = enemyHealthBefore;
                        ResolveBullImmuneShot(enemy, bullState, projectileDirection);
                    }
                    if (isBabyTurtle)
                        ResolveBabyTurtleShotAfterCommon(RequireBabyTurtleState(enemy));
                    if (isKago)
                        ResolveKagoShotAfterCommon(enemy, RequireKagoState(enemy));
                    if (isMagdollite)
                        ResolveMagdolliteCombatAfterCommon(enemy);
                    if (isMaridiaLargeSnail)
                        ResolveMaridiaLargeSnailShotAfterCommon();
                    if (isGRipperOrRipper2)
                        ResolveGRipperRipper2ShotAfterCommon(enemy);
                    if (isDragon)
                        ResolveDragonCombatAfterCommon(enemy);
                    if (isZebetite)
                        ResolveZebetiteShotAfterCommon(enemy);
                    // Evir's bank-$A8 shot tail runs after every accepted common shot,
                    // including the $FF ice result. That is where the body copies the
                    // newly installed frozen timer to its arms and attached (idle) spit.
                    if (isEvir)
                        ResolveEvirCombatAfterCommon(enemy);
                    if (isYappingMaw)
                        ResolveYappingMawShotAfterCommon(
                            enemy,
                            RequireYappingMawState(enemy),
                            samus);
                    if (isKiHunter)
                        ResolveKiHunterShotAfterCommon(enemy);
                    if (isBotwoon)
                    {
                        RequireBotwoonState(enemy).PreviousHealth = enemyHealthBefore;
                        ResolveBotwoonCombatAfterCommon(enemy);
                    }
                    if (isSporeSpawn)
                        ResolveSporeSpawnShotAfterCommon(enemy);
                    if (isTorizo && enemy.Health == 0)
                        BeginBombTorizoDeath(enemy, RequireBombTorizoState(enemy));
                    if (isShaktool)
                        ResolveShaktoolShotAfterCommon(enemy);
                    if (isDestroyableVerticalShutter)
                        ReactVerticalShutter(enemy, _shutterCameraX, _shutterCameraY);
                    if (isHorizontalShutter)
                        ReactHorizontalShutter(enemy);
                    if (isDraygonBody)
                        ResolveDraygonReaction(enemy, samus);
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
                    if (enemy.Health == 0 && !isPowamp && !isRinka && !isHorizontalShutter &&
                        !isZebetite && !isBotwoon && !isSporeSpawn && !isTorizo &&
                        !isNorfairRidley && !isDraygonBody)
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
                        if (isOrdinarySpacePirate)
                        {
                            // The shared Pirate shot tail clears native variable B before
                            // requesting death animation variant four. Wall AI uses B only as
                            // a debug jump destination, so its fatal clear is observable too.
                            enemy.VariableB = 0;
                        }
                        enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                    }
                }

                // Beetom's private tail runs after common shot AI for every accepted hit,
                // including immune/zero-damage vulnerability results.
                if (isBeetom)
                    ResolveBeetomShotAfterCommon(enemy, RequireBeetomState(enemy));
                if (isPowamp)
                    ResolvePowampShotAfterCommon(enemy);
                if (isWorkRobot)
                    ResolveWorkRobotShotAfterCommon(enemy, samus);
                if (isBull)
                {
                    BullEnemyState bullState = RequireBullState(enemy);
                    bullState.PreviousHealth = enemyHealthBefore;
                    if (enemy.Health == enemyHealthBefore)
                        ResolveBullImmuneShot(enemy, bullState, projectileDirection);
                }
                if (isFakeKraid && enemyHealthBefore != 0 && enemy.Health == 0)
                    RequestFakeKraidDeathDrop(enemy);
                if (isBabyTurtle)
                    ResolveBabyTurtleShotAfterCommon(RequireBabyTurtleState(enemy));
                if (isKago)
                    ResolveKagoShotAfterCommon(enemy, RequireKagoState(enemy));
                if (isMagdollite)
                    ResolveMagdolliteCombatAfterCommon(enemy);
                if (isRinka)
                    ResolveRinkaCombatAfterCommon(enemy);
                if (isMaridiaLargeSnail)
                    ResolveMaridiaLargeSnailShotAfterCommon();
                if (isGRipperOrRipper2)
                    ResolveGRipperRipper2ShotAfterCommon(enemy);
                if (isDragon)
                    ResolveDragonCombatAfterCommon(enemy);
                if (isZebetite)
                    ResolveZebetiteShotAfterCommon(enemy);
                if (isEvir)
                    ResolveEvirCombatAfterCommon(enemy);
                if (isYappingMaw)
                    ResolveYappingMawShotAfterCommon(
                        enemy,
                        RequireYappingMawState(enemy),
                        samus);
                if (isKiHunter)
                    ResolveKiHunterShotAfterCommon(enemy);
                if (isBotwoon)
                {
                    RequireBotwoonState(enemy).PreviousHealth = enemyHealthBefore;
                    ResolveBotwoonCombatAfterCommon(enemy);
                }
                if (isSporeSpawn)
                    ResolveSporeSpawnShotAfterCommon(enemy);
                if (isTorizo && enemy.Health == 0)
                    BeginBombTorizoDeath(enemy, RequireBombTorizoState(enemy));
                if (isShaktool)
                    ResolveShaktoolShotAfterCommon(enemy);
                if (isDestroyableVerticalShutter)
                    ReactVerticalShutter(enemy, _shutterCameraX, _shutterCameraY);
                if (isHorizontalShutter)
                    ReactHorizontalShutter(enemy);
                if (isNorfairRidley)
                    ResolveNorfairRidleyShotAfterCommon(enemy);
                if (isDraygonBody)
                    ResolveDraygonReaction(enemy, samus);

                hitCount++;
                break;
            }
        }
        return hitCount;
    }

    /// <summary>
    /// Ports the Metroid branch reached through <c>EnemyBombCollHandler</c> at $A0:A236.
    /// This is deliberately a bomb-slot pass rather than an extension of the ordinary
    /// projectile loop: the cartridge stores bombs in physical projectile slots five
    /// through nine, admits them only after their shared variable/fuse word reaches zero,
    /// and then dispatches the enemy's normal shot callback with that bomb collision index.
    /// </summary>
    public int ResolveMetroidBombHits(
        SamusBombProjectileSystem bombs,
        SamusProjectileSystem ordinaryProjectiles,
        SamusState? samus = null)
    {
        ArgumentNullException.ThrowIfNull(bombs);
        ArgumentNullException.ThrowIfNull(ordinaryProjectiles);
        EnsureLoaded();

        int hitCount = 0;
        foreach (ushort nativeIndex in _interactiveEnemyIndexes)
        {
            RoomEnemySlot enemy = SlotFromNativeIndex(nativeIndex);
            if (enemy.EnemyDefinitionPointer != MetroidDefinition ||
                enemy.Definition.ShotAiPointer != MetroidShotAi ||
                enemy.SpritemapPointer == 0 ||
                enemy.InvincibilityTimer != 0 ||
                enemy.Properties.HasAny(EnemyProperties.Deleted))
            {
                continue;
            }

            foreach (SamusBombProjectileSlot bomb in bombs.Slots)
            {
                ushort family = unchecked((ushort)(bomb.Type & 0x0f00));

                // `$A0:A24E-$A265` admits an exploding family-$0500 normal bomb. The
                // high-bit alternative is retained because reflected bomb-like actors use
                // the same native collision gate, even though retail Metroids normally see
                // the ordinary family branch. A nonzero BombTimer is still fuse time and
                // cannot touch an enemy yet.
                if (bomb.Type == 0 ||
                    bomb.BombTimer != 0 ||
                    (family != SamusBombProjectileSystem.NormalBombType &&
                     (bomb.Type & 0x8000) == 0))
                {
                    continue;
                }

                if (!RadiusBoxesOverlap(
                        enemy.XPosition,
                        enemy.YPosition,
                        enemy.XRadius,
                        enemy.YRadius,
                        bomb.XPosition,
                        bomb.YPosition,
                        bomb.XRadius,
                        bomb.YRadius))
                {
                    continue;
                }

                // EnemyBombCollHandler marks the colliding bomb before dispatch. Metroid
                // shot AI then ignores all bombs while frozen, detaches an attached body
                // for family $0500, or applies its peculiar slot-zero recoil otherwise.
                bomb.Direction = unchecked((ushort)(bomb.Direction | 0x0010));
                if (enemy.FrozenTimer == 0)
                {
                    SamusProjectileSlot recoilOrigin = ordinaryProjectiles.Slots[0];
                    ResolveMetroidNonFrozenShot(
                        enemy,
                        bomb.Type,
                        bomb.Damage,
                        recoilOrigin.XPosition,
                        recoilOrigin.YPosition,
                        samus);
                }

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
        byte explosionRadius,
        SamusState? samus = null)
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

            // The radius actor is family `$0300` (power bomb), not the ordinary bomb
            // family `$0500`. Using `$0500` silently indexed byte 14 and made every enemy
            // whose bomb and power-bomb vulnerabilities differ behave incorrectly.
            byte vulnerability = ReadProjectileVulnerability(
                bus,
                enemy,
                (ushort)SamusProjectileFamily.PowerBomb);
            if ((vulnerability & 0x7f) == 0)
                continue;

            int xDistance = Math.Abs(unchecked((short)(explosionX - enemy.XPosition)));
            int yDistance = Math.Abs(unchecked((short)(explosionY - enemy.YPosition)));
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                continue;

            ushort reactionPointer = enemy.Definition.PowerBombReactionPointer;
            bool isFireflea = enemy.EnemyDefinitionPointer == FirefleaDefinition &&
                reactionPointer == FirefleaPowerBombAi;
            bool isPowamp = enemy.EnemyDefinitionPointer == PowampDefinition &&
                reactionPointer == PowampPowerBombAi;
            bool isFakeKraid = enemy.EnemyDefinitionPointer == FakeKraidDefinition &&
                reactionPointer == FakeKraidShotAi;
            bool isMagdollite = enemy.EnemyDefinitionPointer == MagdolliteDefinition &&
                reactionPointer == MagdollitePowerBombAi;
            bool isRinka = enemy.EnemyDefinitionPointer == RinkaDefinition &&
                reactionPointer == RinkaPowerBombAi;
            bool isDragon = enemy.EnemyDefinitionPointer == DragonDefinition &&
                reactionPointer == DragonPowerBombAi;
            bool isEvir = enemy.EnemyDefinitionPointer == EvirDefinition &&
                enemy.Parameter1 == 0 &&
                reactionPointer == EvirPowerBombAi;
            bool isKiHunter = IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) &&
                reactionPointer == KiHunterShotAi;
            bool isBotwoon = enemy.EnemyDefinitionPointer == BotwoonDefinition &&
                reactionPointer == BotwoonPowerBombAi;
            bool isCrocomire = enemy.EnemyDefinitionPointer == CrocomireDefinition &&
                reactionPointer == CrocomirePowerBombAi;
            // Several banks install a one-byte `RTL` callback when an enemy must receive
            // the native power-bomb collision prelude but deliberately take no damage.
            // Recognize the executable contract itself instead of maintaining a bespoke
            // definition list: Etecoon and the growing shutter both use bank-local $804C,
            // and any future retail header pointing at a literal RTL has identical meaning.
            bool isLiteralNoOpReaction = reactionPointer != 0 &&
                bus.ReadByte((enemy.Definition.Bank << 16) | reactionPointer) == 0x6b;
            bool isVerticalShutterReaction =
                IsVerticalShutterDefinition(enemy.EnemyDefinitionPointer) &&
                reactionPointer == VerticalShutterPowerBombAi;
            bool isHorizontalShutterReaction =
                enemy.EnemyDefinitionPointer == ShootableHorizontalShutterDefinition &&
                reactionPointer == HorizontalShutterPowerBombAi;
            bool isSpacePiratePowerBombReaction =
                IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
                reactionPointer == SpacePiratePowerBombAi;
            bool isMetroid = enemy.EnemyDefinitionPointer == MetroidDefinition &&
                reactionPointer == MetroidPowerBombAi;
            bool isNorfairRidley = enemy.EnemyDefinitionPointer == NorfairRidleyDefinition &&
                reactionPointer == RidleyPowerBombAi;
            bool isDraygonBody = enemy.EnemyDefinitionPointer == DraygonBodyDefinition &&
                reactionPointer == DraygonPowerBombAi;
            bool isDeadTorizo = enemy.EnemyDefinitionPointer == DeadTorizoDefinition &&
                reactionPointer == DeadTorizoPowerBombAi;
            if (isRinka && enemy.Properties.HasAny(EnemyProperties.Invisible))
                continue;
            if (reactionPointer != 0 && !isFireflea && !isPowamp && !isFakeKraid &&
                !isMagdollite && !isRinka &&
                !isDragon &&
                !isEvir &&
                !isKiHunter &&
                !isBotwoon &&
                !isCrocomire &&
                !isLiteralNoOpReaction &&
                !isVerticalShutterReaction &&
                !isHorizontalShutterReaction &&
                !isSpacePiratePowerBombReaction &&
                !isMetroid &&
                !isNorfairRidley &&
                !isDraygonBody &&
                !isDeadTorizo)
            {
                throw new NotSupportedException(
                    $"Enemy ${enemy.EnemyDefinitionPointer:X4} power-bomb reaction " +
                    $"${enemy.Definition.Bank:X2}:{reactionPointer:X4} is not translated.");
            }

            // These headers install private callbacks instead of falling through common
            // power-bomb damage. A literal RTL intentionally does nothing; the other two
            // callbacks only run their trigger state machines. All still receive the native
            // process-off-screen bit after a qualifying ellipse overlap.
            if (isLiteralNoOpReaction || isVerticalShutterReaction || isHorizontalShutterReaction)
            {
                if (isVerticalShutterReaction)
                    ReactVerticalShutter(enemy, _shutterCameraX, _shutterCameraY);
                if (isHorizontalShutterReaction)
                    ReactHorizontalShutter(enemy);
                enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
                reactionCount++;
                continue;
            }

            if (isCrocomire)
            {
                ResolveCrocomirePowerBombReaction(enemy);
                enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
                reactionCount++;
                continue;
            }

            if (isDeadTorizo)
            {
                TriggerDeadTorizoPowerBomb(enemy);
                enemy.Properties = enemy.Properties.With(EnemyProperties.ProcessOffScreen);
                reactionCount++;
                continue;
            }

            // `$FF` reaches the reaction dispatcher because the outer admission check masks
            // bit seven, then common AI explicitly returns without damage. It still receives
            // property $0800 afterward, an observable quirk preserved below.
            if (vulnerability != 0xff)
            {
                int damage = 100 * (vulnerability & 0x7f);
                if (damage != 0)
                {
                    ushort healthBefore = enemy.Health;
                    enemy.InvincibilityTimer = 48;
                    ushort hurtTime = enemy.HurtAiTime == 0 ? (ushort)4 : enemy.HurtAiTime;
                    enemy.FlashTimer = unchecked((ushort)(hurtTime + 8));
                    enemy.AiHandlerBits = unchecked((ushort)(enemy.AiHandlerBits | 0x0002));
                    enemy.Health = damage >= enemy.Health
                        ? (ushort)0
                        : unchecked((ushort)(enemy.Health - damage));
                    if (enemy.Health == 0 && !isRinka && !isBotwoon && !isNorfairRidley &&
                        !isDraygonBody)
                    {
                        enemy.Properties = enemy.Properties.With(EnemyProperties.Deleted);
                        EnemiesKilled = unchecked((ushort)(EnemiesKilled + 1));
                        if (isFireflea)
                            AdvanceFirefleaDarknessLevel();
                        if (isMetroid)
                            FinishMetroidDeath(enemy, samus, requestDrops: false);
                    }
                    if (isFakeKraid && healthBefore != 0 && enemy.Health == 0)
                        RequestFakeKraidDeathDrop(enemy);
                }
            }

            if (isPowamp && enemy.Parameter1 == 0)
                ResolvePowampPowerBombAfterCommon(enemy);
            if (isMagdollite)
                ResolveMagdolliteCombatAfterCommon(enemy);
            if (isRinka)
                ResolveRinkaCombatAfterCommon(enemy);
            if (isDragon)
                ResolveDragonCombatAfterCommon(enemy);
            if (isEvir)
                ResolveEvirCombatAfterCommon(enemy);
            if (isKiHunter)
                ResolveKiHunterShotAfterCommon(enemy);
            if (isBotwoon)
                ResolveBotwoonCombatAfterCommon(enemy);
            if (isNorfairRidley)
                ResolveNorfairRidleyPowerBombAfterCommon(enemy);
            if (isDraygonBody)
                ResolveDraygonReaction(enemy, samus);

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
    /// Walks `$A0:9A5A/$A0:9B7F`'s bank-local extended-spritemap structure and returns the
    /// callback belonging to the first overlapping hitbox. One displayed enemy frame may
    /// contain several independently offset ordinary spritemaps; every component points at
    /// a hitbox list whose records are signed bounds plus touch/shot function pointers.
    /// </summary>
    private bool TryFindExtendedHitboxCallback(
        RoomEnemySlot enemy,
        ushort targetX,
        ushort targetY,
        ushort targetXRadius,
        ushort targetYRadius,
        bool selectShotCallback,
        out ushort callback)
    {
        callback = 0;

        // Native multibox collision only accepts negative 16-bit spritemap pointers. The
        // common empty map `$804F` has a zero component count and naturally returns false.
        if ((enemy.SpritemapPointer & 0x8000) == 0)
            return false;

        int bank = enemy.Definition.Bank << 16;
        int extendedMap = bank | enemy.SpritemapPointer;
        // `$A0:9A5A/$9B7F` load only the low byte. The high byte carries drawing metadata;
        // Ceres steam, for example, stores `$1001` for one component. Treating the whole
        // word as 4097 components walks into adjacent ROM and eventually selects garbage
        // callbacks such as `$F880` instead of the authored `$F03F/$804C` pair.
        int componentCount = _bus!.ReadByte(extendedMap);
        ushort targetLeft = unchecked((ushort)(targetX - targetXRadius));
        ushort targetRight = unchecked((ushort)(targetX + targetXRadius));
        ushort targetTop = unchecked((ushort)(targetY - targetYRadius));
        ushort targetBottom = unchecked((ushort)(targetY + targetYRadius));

        for (int componentIndex = 0; componentIndex < componentCount; componentIndex++)
        {
            int component = AddWithinBank(extendedMap, 2 + componentIndex * 8);
            ushort componentX = unchecked((ushort)(
                enemy.XPosition + ReadWord(_bus!, component)));
            ushort componentY = unchecked((ushort)(
                enemy.YPosition + ReadWord(_bus!, AddWithinBank(component, 2))));
            ushort hitboxListPointer = ReadWord(_bus!, AddWithinBank(component, 6));
            int hitboxList = bank | hitboxListPointer;
            int hitboxCount = ReadWord(_bus!, hitboxList);

            for (int hitboxIndex = 0; hitboxIndex < hitboxCount; hitboxIndex++)
            {
                int hitbox = AddWithinBank(hitboxList, 2 + hitboxIndex * 12);
                ushort left = unchecked((ushort)(
                    componentX + ReadWord(_bus!, hitbox)));
                ushort top = unchecked((ushort)(
                    componentY + ReadWord(_bus!, AddWithinBank(hitbox, 2))));
                ushort right = unchecked((ushort)(
                    componentX + ReadWord(_bus!, AddWithinBank(hitbox, 4))));
                ushort bottom = unchecked((ushort)(
                    componentY + ReadWord(_bus!, AddWithinBank(hitbox, 6))));

                // These asymmetric signed comparisons are literal translations. They
                // retain the cartridge's inclusive left/bottom and exclusive right/top
                // edges instead of replacing them with a friendlier host rectangle API.
                bool overlaps = selectShotCallback
                    ? !IsNegative16(targetRight - left) &&
                      IsNegative16(targetLeft - right) &&
                      !IsNegative16(targetBottom - top) &&
                      IsNegative16(targetTop - bottom)
                    : IsNegative16(left - targetRight) &&
                      !IsNegative16(right - targetLeft) &&
                      IsNegative16(top - targetBottom) &&
                      !IsNegative16(bottom - targetTop);
                if (!overlaps)
                    continue;

                callback = ReadWord(
                    _bus!,
                    AddWithinBank(hitbox, selectShotCallback ? 10 : 8));
                return true;
            }
        }

        return false;
    }

    /// <summary>
    /// Dispatches the three shot callbacks stored in Space Pirate hitbox records. Only the
    /// gold Ninja gives `$87C8/$883E` special meaning; every other Pirate deliberately falls
    /// through to normal shot AI even when it displays one of the shared Ninja maps.
    /// </summary>
    private static PirateHitboxShotAction SelectPirateHitboxShotAction(
        ISnesAddressSpace bus,
        RoomEnemySlot enemy,
        SamusProjectileSlot projectile,
        ushort hitboxShotAi)
    {
        if (hitboxShotAi == SpacePirateShotAi)
            return PirateHitboxShotAction.Normal;
        if (hitboxShotAi is not (
                GoldNinjaVulnerableHitboxShotAi or GoldNinjaInvincibleHitboxShotAi))
        {
            throw new NotSupportedException(
                $"Space Pirate hitbox shot AI $B2:{hitboxShotAi:X4} is not translated.");
        }
        if (enemy.EnemyDefinitionPointer != GoldNinjaSpacePirateDefinition)
            return PirateHitboxShotAction.Normal;

        ushort family = unchecked((ushort)(projectile.Type & 0x0f00));
        if (hitboxShotAi == GoldNinjaInvincibleHitboxShotAi)
        {
            if (family == (ushort)SamusProjectileFamily.SuperMissile && projectile.Variable == 0)
                return PirateHitboxShotAction.Ignore;
            return family < (ushort)SamusProjectileFamily.PowerBomb
                ? PirateHitboxShotAction.Reflect
                : PirateHitboxShotAction.Ignore;
        }

        if (family >= (ushort)SamusProjectileFamily.PowerBomb)
            return PirateHitboxShotAction.Ignore;

        ushort vulnerabilityPointer = enemy.Definition.VulnerabilityPointer != 0
            ? enemy.Definition.VulnerabilityPointer
            : DefaultEnemyVulnerability;
        int vulnerabilityOffset = family == (ushort)SamusProjectileFamily.Beam
            ? projectile.Type & 0x000f
            : 11 + (family >> 8);
        int multiplier = bus.ReadByte(
            0xb40000 | unchecked((ushort)(vulnerabilityPointer + vulnerabilityOffset))) & 0x0f;
        return multiplier is not (0 or 15)
            ? PirateHitboxShotAction.Normal
            : PirateHitboxShotAction.Reflect;
    }

    private enum PirateHitboxShotAction : byte
    {
        Normal,
        Ignore,
        Reflect,
    }

    /// <summary>
    /// Ports $A0:A477-$A531 for one already-overlapping ordinary actor. A zero contact index
    /// damages Samus; Speed Booster, shinespark, Screw Attack, and pseudo-Screw instead read
    /// their dedicated bytes from the enemy's vulnerability record and damage the actor.
    /// </summary>
    private void ResolveNormalEnemyTouch(
        RoomEnemySlot enemy,
        SamusState samus,
        ushort controllerInput,
        bool skipDeathAnimation = false)
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

        // Rinka's bank-$A2 callback calls this common damage body explicitly, inspects the
        // resulting zero health, then chooses either generic respawn death or its private
        // hidden two-frame Mother Brain respawn. Do not clear the slot before that tail.
        if (skipDeathAnimation)
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
