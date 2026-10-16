using SuperMetroid.Core.Input;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Areas.Ceres;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.Corpses;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.MaridiaSnails;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.SpacePirates;
using SuperMetroid.Core.Enemies.WorkRobots;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Progression;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;
using SuperMetroid.Core.Bosses.Draygon.Artwork;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies;

/// <summary>
/// The collision capabilities of species not yet converted to their own entity (#1312 stage 3), moved unchanged from
/// <see cref="EnemyCollision"/>'s species lists. A converting species declares its own and leaves this catalog; it is
/// deleted with <see cref="TableBackedEnemyEntity"/>.
/// </summary>
internal static class EnemyCapabilityCatalog
{
    /// <summary>The capabilities <paramref name="definition"/> takes.</summary>
    /// <param name="definition">The occupant's definition.</param>
    internal static EnemyCapabilities For(EnemyDefinitionId definition) =>
        (UsesExtendedSamusHitboxes(definition) ? EnemyCapabilities.ExtendedSamusHitboxes : EnemyCapabilities.None) |
        (UsesExtendedProjectileHitboxes(definition) ? EnemyCapabilities.ExtendedProjectileHitboxes : EnemyCapabilities.None);

    /// <summary>Whether the definition's Samus contact enters the extended-spritemap walker.</summary>
    private static bool UsesExtendedSamusHitboxes(EnemyDefinitionId definition) =>
        WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(definition) ||
         definition is
            EnemyDefinitionId.Oum or
            EnemyDefinitionId.BombTorizo or
            EnemyDefinitionId.GoldenTorizo or
            EnemyDefinitionId.Crocomire or
            EnemyDefinitionId.CrocomireTongue or
            EnemyDefinitionId.SporeSpawn or
            EnemyDefinitionId.Steam or
            EnemyDefinitionId.KraidArm or
            EnemyDefinitionId.KraidFoot or
            EnemyDefinitionId.PhantoonBody or
            EnemyDefinitionId.DraygonBody;

    /// <summary>Whether the definition's projectile collision enters the extended-spritemap walker.</summary>
    private static bool UsesExtendedProjectileHitboxes(EnemyDefinitionId definition) =>
        WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(definition) ||
         definition is
            EnemyDefinitionId.Oum or
            EnemyDefinitionId.BombTorizo or
            EnemyDefinitionId.GoldenTorizo or
            EnemyDefinitionId.Crocomire or
            EnemyDefinitionId.CrocomireTongue or
            EnemyDefinitionId.SporeSpawn or
            EnemyDefinitionId.Steam or
            EnemyDefinitionId.Kraid or
            EnemyDefinitionId.KraidArm or
            EnemyDefinitionId.KraidFoot or
            EnemyDefinitionId.Ridley or
            EnemyDefinitionId.DraygonBody or
            EnemyDefinitionId.MotherBrainBody or
            EnemyDefinitionId.MotherBrainHead;

    /// <summary>Whether <paramref name="callback"/> is one of <paramref name="enemy"/>'s translated private shot callbacks.</summary>
    /// <param name="enemy">The occupant.</param>
    /// <param name="callback">The named callback the pointer decodes to, or null.</param>
    /// <param name="callbackPointer">The raw bank-local callback pointer.</param>
    internal static bool OwnsPrivateShotCallback(RoomEnemySlot enemy, EnemyInteractionCallback? callback, ushort callbackPointer) =>
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyTurtle && callback == EnemyInteractionCallback.BabyTurtleShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Owtch && callback == EnemyInteractionCallback.OwtchShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Rinka && callback == EnemyInteractionCallback.RinkaShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Oum &&
            callback == EnemyInteractionCallback.MaridiaLargeSnailShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.GRipper or EnemyDefinitionId.Ripper2 &&
            callback == EnemyInteractionCallback.GRipperRipper2Shot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Dragon && callback == EnemyInteractionCallback.DragonShot ||
        enemy.EnemyDefinitionPointer is
                EnemyDefinitionId.ShutterShootable or EnemyDefinitionId.Kamer &&
            callback == EnemyInteractionCallback.ShootableVerticalShutterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.ShutterDestroyable &&
            callback == EnemyInteractionCallback.DestroyableVerticalShutterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Metaree && callback == EnemyInteractionCallback.MetareeShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Fireflea && callback == EnemyInteractionCallback.FirefleaShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Tripper && callback == EnemyInteractionCallback.TripperShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Mochtroid && callback == EnemyInteractionCallback.MochtroidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Skree && callback == EnemyInteractionCallback.SkreeShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Yard && callback == EnemyInteractionCallback.YardShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Evir && callback == EnemyInteractionCallback.EvirShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.YappingMaw && callback == EnemyInteractionCallback.YappingMawShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Kago && callback == EnemyInteractionCallback.KagoShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Magdollite && callback == EnemyInteractionCallback.MagdolliteShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Beetom && callback == EnemyInteractionCallback.BeetomShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Powamp && callback == EnemyInteractionCallback.PowampShot ||
        WorkRobotBehavior.IsWorkRobotDefinition(enemy.EnemyDefinitionPointer) &&
            callback is EnemyInteractionCallback.WorkRobotShot or EnemyInteractionCallback.WorkRobotNoPowerShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Bull && callback == EnemyInteractionCallback.BullShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Spark && callback == EnemyInteractionCallback.SparkShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.FaceBlock &&
            callback == EnemyInteractionCallback.BlueBrinstarFaceBlockShot ||
        KiHunterBehavior.IsKiHunterBodyDefinition(enemy.EnemyDefinitionPointer) && callback == EnemyInteractionCallback.KiHunterShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MiniKraid && callback == EnemyInteractionCallback.FakeKraidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Zebetite && callback == EnemyInteractionCallback.ZebetiteShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseTorizo &&
            callback == EnemyInteractionCallback.DeadTorizoTouchAndShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.CorpseSidehopper &&
            callback == EnemyInteractionCallback.DeadSidehopperShot ||
        DeadTourianBehavior.HasDeadTourianCorpseShotCallback(enemy) &&
            callbackPointer == enemy.Definition.ShotAiPointer ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.DraygonBody &&
            callback is EnemyInteractionCallback.DraygonShot or EnemyInteractionCallback.DudShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.SporeSpawn &&
            callback is EnemyInteractionCallback.SporeSpawnShot or EnemyInteractionCallback.DudShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.PhantoonBody &&
            callback == EnemyInteractionCallback.PhantoonShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainBody &&
            callback == EnemyInteractionCallback.MotherBrainBodyShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.MotherBrainHead &&
            callback == EnemyInteractionCallback.MotherBrainHeadShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.BombTorizo &&
            callback is EnemyInteractionCallback.BombTorizoShot or EnemyInteractionCallback.TorizoStandUpSitDownShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.GoldenTorizo &&
            callback is EnemyInteractionCallback.GoldenTorizoShot or EnemyInteractionCallback.BombTorizoShot or
                EnemyInteractionCallback.TorizoStandUpSitDownShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Shaktool && callback == EnemyInteractionCallback.ShaktoolShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.KraidArm &&
            callback == EnemyInteractionCallback.KraidArmShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.RidleyCeres or EnemyDefinitionId.Ridley &&
            callback == EnemyInteractionCallback.RidleyShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.BabyMetroid && callback == EnemyInteractionCallback.ShitroidShot ||
        enemy.EnemyDefinitionPointer == EnemyDefinitionId.Botwoon && callback == EnemyInteractionCallback.BotwoonShot ||
        enemy.EnemyDefinitionPointer is EnemyDefinitionId.Crocomire or EnemyDefinitionId.CrocomireTongue &&
            Enum.IsDefined((CrocomireHitboxShotCallback)callbackPointer) ||
        WallSpacePirateBehavior.IsOrdinarySpacePirateDefinition(enemy.EnemyDefinitionPointer) &&
            callback is EnemyInteractionCallback.SpacePirateShot or
                EnemyInteractionCallback.GoldNinjaVulnerableHitboxShot or
                EnemyInteractionCallback.GoldNinjaInvincibleHitboxShot;
}
