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
    /// <summary>The enemy pass's view of the frame, whose power bomb explosion frees the shots it catches.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Allocates the drop a shot goop leaves.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Builds the component from the collaborators it uses.</summary>
    internal DraygonShots(EnemyFrameContext frameContext, EnemyDeathsAndDrops deathsAndDrops)
    {
        _frameContext = frameContext;
        _deathsAndDrops = deathsAndDrops;
    }

    /// <summary>
    /// Draygon's goop: Samus's touch sticks it to her and a shot one drops Draygon's eye's chances, through the typed
    /// animations its definition names.
    /// </summary>
    internal EnemyProjectileBehavior Goop => field ??= new DraygonShotBehavior(this,
        shot: () => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopShot),
        touch: () => new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));

    /// <summary>A wall turret's shot: a shot one runs the shared dust's small explosion, the animation its definition names.</summary>
    internal EnemyProjectileBehavior WallTurretShot => field ??= new DraygonShotBehavior(this,
        shot: () => new MiscDustProjectileAnimation(MiscDustProjectileSegment.SmallExplosion));

    /// <summary>A goop or turret shot: the goop's flight and its hold on Samus, the turret shot's flight, and their cues.</summary>
    private sealed class DraygonShotBehavior(
        DraygonShots owner,
        Func<EnemyProjectileAnimation> shot,
        Func<EnemyProjectileAnimation>? touch = null)
        : EnemyProjectileBehavior(shot: shot, touch: touch), IEnemyProjectileCues<DraygonProjectileCue>
    {
        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileFrameBehaviorContext context)
        {
            EnemyProjectile projectile = context.Projectile;
            EnemyProjectileFrameBehavior motion = projectile.FrameBehavior;
            if (motion == EnemyProjectileFrameBehavior.DraygonGoopStuckToSamus)
            {
                owner.RunAttachedDraygonGoop(projectile, context.Samus);
            }
            else if (motion == EnemyProjectileFrameBehavior.DraygonTurretShotFlight)
            {
                // The goop's power-bomb step frees a power-bombed shot's place, then still moves the shot.
                owner.FreePlaceIfPowerBombed(projectile, context.Samus);
                MoveDraygonProjectile(projectile);
                DeleteDraygonProjectileOutsideRoom(projectile);
            }
            else if (motion == EnemyProjectileFrameBehavior.DraygonGoopFlight)
            {
                owner.RunFlyingDraygonGoop(projectile, context.Samus);
            }
            else
            {
                base.Move(context);
            }
        }

        /// <summary>Carries out a cue of the goop's and turret shot's animations.</summary>
        public void RunCue(EnemyProjectileCueContext context, DraygonProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case DraygonProjectileCue.StickToSamus:
                    owner.AttachDraygonGoopToSamus(projectile, context.Samus);
                    return;
                case DraygonProjectileCue.SpawnDrops:
                    // The drop is allocated before the deletion releases this projectile, preserving slot order.
                    owner._deathsAndDrops.SpawnEnemyDropFromDefinition(projectile.Position.Whole, EnemyDefinitionId.DraygonEye);
                    return;
                case DraygonProjectileCue.Delete:
                    projectile.Delete();
                    return;
                case DraygonProjectileCue.Fire:
                    projectile.FrameBehavior = EnemyProjectileFrameBehavior.DraygonTurretShotFlight;
                    return;
                default:
                    throw new InvalidOperationException($"Undefined {nameof(DraygonProjectileCue)} {cue}.");
            }
        }
    }
    /// <summary>Moves a Draygon projectile in 1/65536-pixel space by its angle and speed.</summary>
    internal static void MoveDraygonProjectile(EnemyProjectile projectile) =>
        projectile.StateOf<AngledFlightState>().Advance(projectile);

    /// <summary>Deletes a projectile that has left Draygon's room.</summary>
    private static void DeleteDraygonProjectileOutsideRoom(EnemyProjectile projectile)
    {
        if (IsOutsideDraygonRoom(projectile))
            projectile.Delete();
    }

    /// <summary>Advances airborne goop, including the power bomb freeing its place and Samus contact setup.</summary>
    internal void RunFlyingDraygonGoop(
        EnemyProjectile goop,
        SamusState? samus)
    {
        // A power-bombed goop frees its place but its update still moves it and
        // tests Samus. Contact is tested before the room boundary, so a goop that touches
        // Samus outside the boundary still plays its touch animation instead of being deleted.
        FreePlaceIfPowerBombed(goop, samus);
        MoveDraygonProjectile(goop);
        if (samus is null)
        {
            DeleteDraygonProjectileOutsideRoom(goop);
            return;
        }

        int xDistance = CoordinateAxis.Distance(samus.Position.X.Whole, goop.Position.X.Whole);
        int yDistance = CoordinateAxis.Distance(samus.Position.Y.Whole, goop.Position.Y.Whole);
        if (xDistance < 16 && yDistance < 20)
        {
            goop.StartAnimation(new DraygonProjectileAnimation(DraygonProjectileSegment.GoopTouch));
            return;
        }
        DeleteDraygonProjectileOutsideRoom(goop);
    }

    /// <summary>Attaches goop to Samus, increasing her X-speed divisor until it expires.</summary>
    internal void AttachDraygonGoopToSamus(
        EnemyProjectile goop,
        SamusState? samus)
    {
        // The goop's attach step runs the power-bomb catch first, then attaches regardless.
        FreePlaceIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        int nextDivisor = samus.XSpeedDivisor + 1;
        if (nextDivisor >= 6)
            return;

        samus.XSpeedDivisor = nextDivisor;
        goop.State = new DraygonGoopAttachedState(nextDivisor);
        goop.FrameBehavior =
            EnemyProjectileFrameBehavior.DraygonGoopStuckToSamus;
        goop.BlocksSamusProjectiles = false;
        goop.CanDamageSamus = false;
        goop.PersistsOnSamusContact = true;
        samus.InvincibilityTimer = WrappingCountdown.Start(0);
        samus.KnockbackTimer = 0;
    }

    /// <summary>Follows Samus with attached goop and counts down its removal timer.</summary>
    internal void RunAttachedDraygonGoop(
        EnemyProjectile goop,
        SamusState? samus)
    {
        // The power-bomb catch does not end this frame behaviour.
        FreePlaceIfPowerBombed(goop, samus);
        if (samus is null)
            return;
        if (samus.HorizontalSpeed.ContactDamageIndex != SamusContactDamageIndex.None)
        {
            RemoveAttachedDraygonGoop(goop, samus);
            return;
        }

        DraygonGoopAttachedState attached = goop.StateOf<DraygonGoopAttachedState>();
        goop.Position = goop.Position.WithWhole(new(samus.Position.X.Whole, CoordinateAxis.Wrap(samus.Position.Y.Whole + attached.StackPosition * 4 - 12)));
        // The attachment lifetime counts down: the goop falls off when it reaches zero.
        if (--attached.Lifetime == 0)
            RemoveAttachedDraygonGoop(goop, samus);
    }

    /// <summary>Releases one attached goop projectile and restores one X-speed divisor step.</summary>
    private static void RemoveAttachedDraygonGoop(
        EnemyProjectile goop,
        SamusState samus)
    {
        goop.Delete();
        samus.XSpeedDivisor = samus.XSpeedDivisor == 0
            ? 0
            : samus.XSpeedDivisor - 1;
    }

    /// <summary>
    /// Frees the place of a Draygon goop or wall-turret shot caught in a power bomb's explosion and clears Samus's
    /// slowdown. Inside the explosion's ellipse the shot finishes its update
    /// (<see cref="PowerBombedDraygonShotFinishesItsUpdate"/>): the calling update goes on with it.
    /// </summary>
    internal void FreePlaceIfPowerBombed(EnemyProjectile projectile, SamusState? samus)
    {
        // A projectile pass no enemy pass has preceded has seen no explosion, which is the radius-zero case.
        if (_frameContext.PowerBombExplosion is not { } powerBomb)
            return;
        int horizontalRadius = powerBomb.ExplosionRadius.WholePixels;
        if (horizontalRadius == 0)
            return;
        // The vertical radius is three quarters of the horizontal: half of it, rounded up, plus the whole, halved.
        int verticalRadius = ((horizontalRadius >> 1) + horizontalRadius + (horizontalRadius & 1)) >> 1;
        int xDistance = CoordinateAxis.Distance(powerBomb.Position.X, projectile.Position.X.Whole);
        int yDistance = CoordinateAxis.Distance(powerBomb.Position.Y, projectile.Position.Y.Whole);
        if (xDistance >= horizontalRadius || yDistance >= verticalRadius)
            return;
        PowerBombedDraygonShotFinishesItsUpdate.Catch(projectile, static shot => shot.FreePlace());
        if (samus is not null)
            samus.XSpeedDivisor = 0;
    }

    /// <summary>Tests the projectile against the 512-by-512-pixel Draygon arena bounds.</summary>
    private static bool IsOutsideDraygonRoom(EnemyProjectile projectile) =>
        CoordinateAxis.AsSigned(projectile.Position.X.Whole) < 0 || projectile.Position.X.Whole >= 512 ||
        CoordinateAxis.AsSigned(projectile.Position.Y.Whole) < 0 || projectile.Position.Y.Whole >= 512;
}
