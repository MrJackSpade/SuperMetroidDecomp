using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's goop and the wall turrets' shots in flight (#1760): their frame behaviours, the cues of their animations, the
/// animations a shot or Samus's touch starts, and the power bomb that catches them. Draygon's parts and turrets bind these
/// behaviours to the shots they spawn.
/// </summary>
[RoomLifetime]
internal sealed class DraygonShots
{
    /// <summary>Allocates the drop a shot goop leaves.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonShots(EnemyDeathsAndDrops deathsAndDrops) => _deathsAndDrops = deathsAndDrops;

    /// <summary>
    /// Draygon's goop: Samus's touch sticks it to her and a shot one drops Draygon's eye's chances, through the typed
    /// animations its definition names.
    /// </summary>
    internal DraygonShotBehavior Goop => field ??= new DraygonShotBehavior(_deathsAndDrops, DraygonShotMotion.GoopFlying,
        shot: () => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopShot),
        touch: () => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));

    /// <summary>
    /// A wall turret's shot: it holds still through its muzzle bloom until its animation fires it, and a shot one runs the
    /// shared dust's small explosion, the animation its definition names.
    /// </summary>
    internal EnemyProjectileBehavior WallTurretShot => field ??= new DraygonShotBehavior(_deathsAndDrops, initial: null,
        shot: () => new MiscDustProjectileAnimation(DustExplosionAnimation.SmallExplosion));

    /// <summary>
    /// A goop or turret shot: the goop's flight and its hold on Samus, the turret shot's flight, and their cues. The power
    /// bomb explosion of the projectile pass's bombs frees the shots it catches.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot goop leaves.</param>
    /// <param name="initial">The motion the shot starts with: the goop's flight, or none for the turret shot.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    internal sealed class DraygonShotBehavior(
        EnemyDeathsAndDrops deathsAndDrops,
        DraygonShotMotion? initial,
        Func<EnemyProjectileAnimation> shot,
        Func<EnemyProjectileAnimation>? touch = null)
        : EnemyProjectileMotionBehavior<DraygonShotMotion>(initial, shot, touch), IEnemyProjectileCues<DraygonProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, DraygonShotMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case DraygonShotMotion.GoopStuckToSamus:
                    RunAttachedDraygonGoop(projectile, context.Samus, context.SamusBombs.PowerBombExplosion);
                    break;
                case DraygonShotMotion.TurretShotFlying:
                    // The goop's power-bomb step frees a power-bombed shot's place, then still moves the shot.
                    FreePlaceIfPowerBombed(projectile, context.Samus, context.SamusBombs.PowerBombExplosion);
                    MoveDraygonProjectile(projectile);
                    DeleteDraygonProjectileOutsideRoom(projectile);
                    break;
                case DraygonShotMotion.GoopFlying:
                    RunFlyingDraygonGoop(projectile, context.Samus, context.SamusBombs.PowerBombExplosion);
                    break;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DraygonShotMotion)} {motion}.");
            }
        }

        /// <summary>Carries out a cue of the goop's and turret shot's animations.</summary>
        public void RunCue(EnemyProjectileCueContext context, DraygonProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case DraygonProjectileCue.StickToSamus:
                    AttachDraygonGoopToSamus(projectile, context.Samus, context.SamusBombs.PowerBombExplosion);
                    return;
                case DraygonProjectileCue.SpawnDrops:
                    // The drop is spawned before the deletion frees this projectile's place, so it takes another place.
                    deathsAndDrops.SpawnEnemyDropFromDefinition(projectile.Position.Whole, EnemyDefinitionId.DraygonEye, context.Samus);
                    return;
                case DraygonProjectileCue.Delete:
                    projectile.Delete();
                    return;
                case DraygonProjectileCue.Fire:
                    projectile.StartMotion(DraygonShotMotion.TurretShotFlying);
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DraygonProjectileCue)} {cue}.");
            }
        }

        /// <summary>Advances airborne goop, including the power bomb freeing its place and Samus contact setup.</summary>
        private static void RunFlyingDraygonGoop(
            EnemyProjectile goop,
            SamusState samus,
            SamusPowerBombExplosionState powerBomb)
        {
            // A power-bombed goop frees its place but its update still moves it and
            // tests Samus. Contact is tested before the room boundary, so a goop that touches
            // Samus outside the boundary still plays its touch animation instead of being deleted.
            FreePlaceIfPowerBombed(goop, samus, powerBomb);
            MoveDraygonProjectile(goop);
            int xDistance = CoordinateAxis.Distance(samus.Position.X.Whole, goop.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(samus.Position.Y.Whole, goop.Position.Y.Whole);
            if (xDistance < DraygonArenaDefinitions.GoopTouchReach.X && yDistance < DraygonArenaDefinitions.GoopTouchReach.Y)
            {
                goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));
                return;
            }
            DeleteDraygonProjectileOutsideRoom(goop);
        }

        /// <summary>Attaches goop to Samus, increasing her X-speed divisor until it expires.</summary>
        private static void AttachDraygonGoopToSamus(
            EnemyProjectile goop,
            SamusState samus,
            SamusPowerBombExplosionState powerBomb)
        {
            // The goop's attach step runs the power-bomb catch first, then attaches regardless.
            FreePlaceIfPowerBombed(goop, samus, powerBomb);
            int nextDivisor = samus.SlowdownLevel + 1;
            if (nextDivisor >= DraygonArenaDefinitions.GoopSlowdownLimit)
                return;

            samus.SlowdownLevel = nextDivisor;
            goop.State = new DraygonGoopAttachedState(nextDivisor);
            goop.StartMotion(DraygonShotMotion.GoopStuckToSamus);
            goop.BlocksSamusProjectiles = false;
            goop.CanDamageSamus = false;
            goop.PersistsOnSamusContact = true;
            samus.InvincibilityTimer = WrappingCountdown.Start(0);
            samus.KnockbackTimer = 0;
        }

        /// <summary>Follows Samus with attached goop and counts down its removal timer.</summary>
        /// <param name="goop">The goop stuck to Samus.</param>
        /// <param name="samus">Samus, whom the goop follows.</param>
        /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which frees the goop's place.</param>
        internal static void RunAttachedDraygonGoop(
            EnemyProjectile goop,
            SamusState samus,
            SamusPowerBombExplosionState powerBomb)
        {
            // The power-bomb catch does not end this frame behaviour.
            FreePlaceIfPowerBombed(goop, samus, powerBomb);
            if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
            {
                RemoveAttachedDraygonGoop(goop, samus);
                return;
            }

            DraygonGoopAttachedState attached = goop.StateOf<DraygonGoopAttachedState>();
            goop.Position = goop.Position.WithWhole(new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + attached.StackPosition * DraygonArenaDefinitions.GoopStackSpacing + DraygonArenaDefinitions.GoopStackTop)));
            // The attachment lifetime counts down: the goop falls off when it reaches zero.
            if (--attached.Lifetime == 0)
                RemoveAttachedDraygonGoop(goop, samus);
        }

        /// <summary>
        /// Frees the place of a Draygon goop or wall-turret shot caught in a power bomb's explosion and clears Samus's
        /// slowdown. Inside the explosion's ellipse the shot finishes its update (<see cref="EnemyProjectile.FreePlace"/>):
        /// the calling update goes on with it.
        /// </summary>
        private static void FreePlaceIfPowerBombed(EnemyProjectile projectile, SamusState samus, SamusPowerBombExplosionState powerBomb)
        {
            int horizontalRadius = powerBomb.Blast.ExplosionRadius.WholePixels;
            if (horizontalRadius == 0)
                return;
            // The vertical radius is three quarters of the horizontal: half of it, rounded up, plus the whole, halved down.
            int verticalRadius = (horizontalRadius + (horizontalRadius + 1) / 2) / 2;
            int xDistance = CoordinateAxis.Distance(powerBomb.Position.X, projectile.Position.X.Whole);
            int yDistance = CoordinateAxis.Distance(powerBomb.Position.Y, projectile.Position.Y.Whole);
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                return;
            projectile.FreePlace();
            samus.SlowdownLevel = 0;
        }
    }
    /// <summary>Moves a Draygon projectile along its angle at its speed.</summary>
    internal static void MoveDraygonProjectile(EnemyProjectile projectile) =>
        projectile.StateOf<AngledFlightState>().Advance(projectile);

    /// <summary>Deletes a projectile that has left Draygon's room.</summary>
    private static void DeleteDraygonProjectileOutsideRoom(EnemyProjectile projectile)
    {
        if (IsOutsideDraygonRoom(projectile))
            projectile.Delete();
    }

    /// <summary>Releases one attached goop projectile and restores one X-speed divisor step.</summary>
    private static void RemoveAttachedDraygonGoop(
        EnemyProjectile goop,
        SamusState samus)
    {
        goop.Delete();
        samus.SlowdownLevel = samus.SlowdownLevel == 0
            ? 0
            : samus.SlowdownLevel - 1;
    }

    /// <summary>Tests the projectile against the 512-by-512-pixel Draygon arena bounds.</summary>
    private static bool IsOutsideDraygonRoom(EnemyProjectile projectile) =>
        CoordinateAxis.AsSigned(projectile.Position.X.Whole) < 0 || projectile.Position.X.Whole >= DraygonArenaDefinitions.ArenaSize ||
        CoordinateAxis.AsSigned(projectile.Position.Y.Whole) < 0 || projectile.Position.Y.Whole >= DraygonArenaDefinitions.ArenaSize;
}
