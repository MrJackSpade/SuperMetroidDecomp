using SuperMetroid.Core.Areas.Tourian;
using SuperMetroid.Core.Bosses.Botwoon;
using SuperMetroid.Core.Bosses.Crocomire;
using SuperMetroid.Core.Bosses.Draygon;
using SuperMetroid.Core.Bosses.Kraid;
using SuperMetroid.Core.Bosses.MotherBrain;
using SuperMetroid.Core.Bosses.Phantoon;
using SuperMetroid.Core.Bosses.Ridley;
using SuperMetroid.Core.Bosses.SporeSpawn;
using SuperMetroid.Core.Bosses.Torizo;
using SuperMetroid.Core.Enemies.EyeDoor;
using SuperMetroid.Core.Enemies.Kago;
using SuperMetroid.Core.Enemies.KiHunters;
using SuperMetroid.Core.Enemies.NoobTube;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>
/// Verification's view of the motion an enemy projectile moves by (#2571): the motion value its behaviour's typed motion
/// holds, for checks that compare it with a decoded native word, and the motion a snapshot import starts.
/// </summary>
internal static class EnemyProjectileMotionValueAccess
{
    /// <summary>The motion views of a projectile.</summary>
    extension(EnemyProjectile self)
    {
        /// <summary>The motion value the projectile moves by, or null while it holds still.</summary>
        /// <exception cref="InvalidOperationException">The projectile moves by a family's motion verification does not know.</exception>
        internal Enum? MotionValue => self.Motion switch
        {
            null => null,
            EnemyProjectileFamilyMotion<SingleMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<BotwoonProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<CrocomireProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<DraygonShotMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<EyeDoorProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<FakeKraidProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<KagoBugMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<KiHunterAcidMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<KraidRockMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<MotherBrainProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<NoobTubePieceMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<PhantoonFlameMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<RidleyFireballMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<SporeSpawnSpawnerMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<TorizoProjectileMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<TourianStatueDecorationMotion> motion => motion.Motion,
            EnemyProjectileFamilyMotion<TourianStatueEffectMotion> motion => motion.Motion,
            var unknown => throw new InvalidOperationException($"Verification knows no motion family {unknown}."),
        };

        /// <summary>Starts <paramref name="motion"/> on the projectile through its behaviour, or holds it still for null.</summary>
        /// <param name="motion">One of the projectile's behaviour's motions, or null.</param>
        /// <exception cref="InvalidOperationException">The motion is of a family verification does not know.</exception>
        internal void StartMotionValue(Enum? motion)
        {
            switch (motion)
            {
                case null: self.HoldStill(); break;
                case SingleMotion typed: StartThroughBehavior(self, typed); break;
                case BotwoonProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case CrocomireProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case DraygonShotMotion typed: StartThroughBehavior(self, typed); break;
                case EyeDoorProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case FakeKraidProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case KagoBugMotion typed: StartThroughBehavior(self, typed); break;
                case KiHunterAcidMotion typed: StartThroughBehavior(self, typed); break;
                case KraidRockMotion typed: StartThroughBehavior(self, typed); break;
                case MotherBrainProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case NoobTubePieceMotion typed: StartThroughBehavior(self, typed); break;
                case PhantoonFlameMotion typed: StartThroughBehavior(self, typed); break;
                case RidleyFireballMotion typed: StartThroughBehavior(self, typed); break;
                case SporeSpawnSpawnerMotion typed: StartThroughBehavior(self, typed); break;
                case TorizoProjectileMotion typed: StartThroughBehavior(self, typed); break;
                case TourianStatueDecorationMotion typed: StartThroughBehavior(self, typed); break;
                case TourianStatueEffectMotion typed: StartThroughBehavior(self, typed); break;
                default: throw new InvalidOperationException($"Verification knows no motion family of {motion}.");
            }
        }
    }

    /// <summary>
    /// Starts <paramref name="motion"/> on <paramref name="projectile"/> through the behaviour bound to it, which the
    /// snapshot's decoded motion names the family of.
    /// </summary>
    /// <exception cref="InvalidOperationException">The projectile's behaviour does not own <typeparamref name="TMotion"/>.</exception>
    private static void StartThroughBehavior<TMotion>(EnemyProjectile projectile, TMotion motion) where TMotion : struct, Enum =>
        projectile.StartMotion(projectile.Behavior as EnemyProjectileMotionBehavior<TMotion> ?? throw new InvalidOperationException(
            $"Enemy projectile {projectile.Kind} in place {projectile.Place} is bound to {projectile.Behavior?.GetType().Name ?? "no behaviour"}, which owns no {typeof(TMotion).Name}."), motion);
}
