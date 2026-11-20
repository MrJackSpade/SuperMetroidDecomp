using SuperMetroid.Core.Enemies;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Samus.Movement;
using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Bosses.Draygon;

/// <summary>
/// Draygon's goop and the wall turrets' shots in flight (#1760): how they move each update, the cues of their animations, the
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
    /// <param name="flight">The goop's flight along its angle.</param>
    internal EnemyProjectileBehavior Goop(AngledFlightState flight) => new FlyingShotBehavior(_deathsAndDrops, DraygonShotMotion.GoopFlying,
        GoopShot, GoopTouch, flight);

    /// <summary>The animation a shot goop starts: it drops Draygon's eye's chances.</summary>
    private static DraygonProjectileAnimation GoopShot() => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopShot);

    /// <summary>The animation touching Samus starts on a goop: it sticks to her.</summary>
    private static DraygonProjectileAnimation GoopTouch() => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch);

    /// <summary>
    /// A wall turret's shot: it holds still through its muzzle bloom until its animation fires it, and a shot one runs the
    /// shared dust's small explosion, the animation its definition names.
    /// </summary>
    /// <param name="flight">The shot's flight toward where Samus was.</param>
    internal EnemyProjectileBehavior WallTurretShot(AngledFlightState flight) => new FlyingShotBehavior(_deathsAndDrops, initial: null,
        () => new DustExplosionProjectileAnimation(DustExplosionAnimation.SmallExplosion), touch: null, flight);

    /// <summary>
    /// A goop or turret shot: the goop's flight and its hold on Samus, the turret shot's flight, and their cues. The power
    /// bomb explosion of the projectile pass's bombs frees the shots it catches.
    /// </summary>
    /// <param name="deathsAndDrops">The drops a shot goop leaves.</param>
    /// <param name="initial">The motion the shot starts with: the goop's flight, or none for the turret shot.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    internal abstract class DraygonShotBehavior(
        EnemyDeathsAndDrops deathsAndDrops,
        DraygonShotMotion? initial,
        Func<EnemyProjectileAnimation> shot,
        Func<EnemyProjectileAnimation>? touch = null)
        : EnemyProjectileMotionBehavior<DraygonShotMotion>(initial, shot, touch), IEnemyProjectileCues<DraygonProjectileCue>
    {

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
        private protected static void RunFlyingDraygonGoop(
            EnemyProjectile goop,
            AngledFlightState flight,
            SamusState samus,
            SamusPowerBombExplosionState powerBomb)
        {
            // A power-bombed goop frees its place but its update still moves it and
            // tests Samus. Contact is tested before the room boundary, so a goop that touches
            // Samus outside the boundary still plays its touch animation instead of being deleted.
            FreePlaceIfPowerBombed(goop, samus, powerBomb);
            MoveDraygonProjectile(goop, flight);
            int xDistance = DistanceAcrossRoomEdge.Between(samus.Position.X.Whole, goop.Position.X.Whole);
            int yDistance = DistanceAcrossRoomEdge.Between(samus.Position.Y.Whole, goop.Position.Y.Whole);
            if (xDistance < DraygonArenaDefinitions.GoopTouchReach.X && yDistance < DraygonArenaDefinitions.GoopTouchReach.Y)
            {
                goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));
                return;
            }
            DeleteDraygonProjectileOutsideRoom(goop);
        }

        /// <summary>Attaches goop to Samus, increasing her X-speed divisor until it expires.</summary>
        private void AttachDraygonGoopToSamus(
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
            goop.Rebind(new StuckGoopBehavior(deathsAndDrops, shot, touch, new DraygonGoopAttachedState(nextDivisor)));
            goop.StartMotion(DraygonShotMotion.GoopStuckToSamus);
            goop.BlocksSamusProjectiles = false;
            goop.CanDamageSamus = false;
            goop.PersistsOnSamusContact = true;
            samus.ClearHurtTimers();
        }

        /// <summary>Follows Samus with attached goop, which falls off on its lifetime's last update.</summary>
        /// <param name="goop">The goop stuck to Samus.</param>
        /// <param name="attached">What the stuck goop keeps: its place in the stack and its lifetime.</param>
        /// <param name="samus">Samus, whom the goop follows.</param>
        /// <param name="powerBomb">The power bomb explosion of the pass's bombs, which frees the goop's place.</param>
        internal static void RunAttachedDraygonGoop(
            EnemyProjectile goop,
            DraygonGoopAttachedState attached,
            SamusState samus,
            SamusPowerBombExplosionState powerBomb)
        {
            // The power-bomb catch does not stop the goop's flight on this update.
            FreePlaceIfPowerBombed(goop, samus, powerBomb);
            if (samus.HorizontalSpeed.BodyAttack != SamusBodyAttack.None)
            {
                RemoveAttachedDraygonGoop(goop, samus);
                return;
            }

            // The stack's top lies 12 pixels above Samus's centre, which stays more than her radius below the arena's
            // solid ceiling, so a stuck goop stays in the room.
            goop.Position = goop.Position.WithWhole(new(samus.Position.X.Whole, samus.Position.Y.Whole + attached.StackPosition * DraygonArenaDefinitions.GoopStackSpacing + DraygonArenaDefinitions.GoopStackTop));
            if (attached.Lifetime.StepRunsOut())
                RemoveAttachedDraygonGoop(goop, samus);
        }

        /// <summary>
        /// Frees the place of a Draygon goop or wall-turret shot caught in a power bomb's explosion and clears Samus's
        /// slowdown. Inside the explosion's ellipse the shot finishes its update (<see cref="EnemyProjectile.FreePlace"/>):
        /// the calling update goes on with it.
        /// </summary>
        private protected static void FreePlaceIfPowerBombed(EnemyProjectile projectile, SamusState samus, SamusPowerBombExplosionState powerBomb)
        {
            int horizontalRadius = powerBomb.Blast.ExplosionRadius.WholePixels;
            if (horizontalRadius == 0)
                return;
            // The vertical radius is three quarters of the horizontal: half of it, rounded up, plus the whole, halved down.
            int verticalRadius = (horizontalRadius + (horizontalRadius + 1) / 2) / 2;
            int xDistance = DistanceAcrossRoomEdge.Between(powerBomb.Position.X, projectile.Position.X.Whole);
            int yDistance = DistanceAcrossRoomEdge.Between(powerBomb.Position.Y, projectile.Position.Y.Whole);
            if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
                return;
            projectile.FreePlace();
            samus.SlowdownLevel = 0;
        }
    }

    /// <summary>A goop or a wall turret's shot in flight along its angle.</summary>
    /// <param name="deathsAndDrops">The drops a shot goop leaves.</param>
    /// <param name="initial">The motion the shot starts with: the goop's flight, or none for the turret shot.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="touch">The animation touching Samus starts, or null when the kind has none of its own.</param>
    /// <param name="flight">The shot's flight.</param>
    private sealed class FlyingShotBehavior(EnemyDeathsAndDrops deathsAndDrops, DraygonShotMotion? initial, Func<EnemyProjectileAnimation> shot,
        Func<EnemyProjectileAnimation>? touch, AngledFlightState flight)
        : DraygonShotBehavior(deathsAndDrops, initial, shot, touch)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, DraygonShotMotion motion)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (motion)
            {
                case DraygonShotMotion.TurretShotFlying:
                    // The goop's power-bomb step frees a power-bombed shot's place, then still moves the shot.
                    FreePlaceIfPowerBombed(projectile, context.Samus, context.SamusBombs.PowerBombExplosion);
                    MoveDraygonProjectile(projectile, flight);
                    DeleteDraygonProjectileOutsideRoom(projectile);
                    break;
                case DraygonShotMotion.GoopFlying:
                    RunFlyingDraygonGoop(projectile, flight, context.Samus, context.SamusBombs.PowerBombExplosion);
                    break;
                case DraygonShotMotion.GoopStuckToSamus:
                    throw new InvalidOperationException($"A flying Draygon shot does not move by {nameof(DraygonShotMotion)} {motion}; a stuck goop does.");
                default:
                    throw new InvalidOperationException($"A flying Draygon shot does not move by {nameof(DraygonShotMotion)} {motion}.");
            }
        }
    }

    /// <summary>A goop stuck to Samus, which keeps its place in the stack and its lifetime.</summary>
    /// <param name="deathsAndDrops">The drops a shot goop leaves.</param>
    /// <param name="shot">The animation a destroying shot starts.</param>
    /// <param name="touch">The animation touching Samus starts.</param>
    /// <param name="attached">What the stuck goop keeps.</param>
    private sealed class StuckGoopBehavior(EnemyDeathsAndDrops deathsAndDrops, Func<EnemyProjectileAnimation> shot,
        Func<EnemyProjectileAnimation>? touch, DraygonGoopAttachedState attached)
        : DraygonShotBehavior(deathsAndDrops, DraygonShotMotion.GoopStuckToSamus, shot, touch)
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, DraygonShotMotion motion)
        {
            if (motion != DraygonShotMotion.GoopStuckToSamus)
                throw new InvalidOperationException($"A stuck goop does not move by {nameof(DraygonShotMotion)} {motion}.");
            RunAttachedDraygonGoop(context.Projectile, attached, context.Samus, context.SamusBombs.PowerBombExplosion);
        }
    }
    /// <summary>Moves a Draygon projectile along its angle at its speed.</summary>
    internal static void MoveDraygonProjectile(EnemyProjectile projectile, AngledFlightState flight) =>
        flight.Advance(projectile);

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

    /// <summary>
    /// Tests the projectile against the 512-by-512-pixel Draygon arena bounds: a place before the room, counted back from
    /// the far end of the room's coordinates (<see cref="OffRoomPlaces"/>), lies past the arena's far edges too.
    /// </summary>
    private static bool IsOutsideDraygonRoom(EnemyProjectile projectile) =>
        projectile.Position.X.Whole >= DraygonArenaDefinitions.ArenaSize || projectile.Position.Y.Whole >= DraygonArenaDefinitions.ArenaSize;
}
