using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The downward gate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DownwardGateBehavior
{
    /// <summary>Enforces that a room is loaded before gate requests are applied.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Shared finite projectile pool holding moving and closed gate actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds gate room-object requests to the current room's projectile owner.</summary>
    internal DownwardGateBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles)
    {
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Applies one gate request to the shared eighteen-slot projectile pool.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request)
    {
        _room.EnsureLoaded();

        switch (request.Operation)
        {
            case DownwardGateProjectileOperation.Spawn:
                SpawnDownwardGateProjectile(request);
                return;
            case DownwardGateProjectileOperation.Wake:
                WakeDownwardGateProjectile(request.RoomObjectBlock);
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request), request.Operation, "Unknown downward gate projectile operation.");
        }
    }

    /// <summary>Allocates a gate actor at its room object's block and ties it to its gate's block for waking.</summary>
    private void SpawnDownwardGateProjectile(
        DownwardGateProjectileRequest request)
    {
        RoomEnemyProjectileKind kind = request.Kind switch
        {
            DownwardGateProjectileKind.Closed => RoomEnemyProjectileKind.DownwardGateClosed,
            DownwardGateProjectileKind.Moving => RoomEnemyProjectileKind.DownwardGateMoving,
            null => throw new InvalidDataException("Downward gate spawn requested no projectile."),
            _ => throw new InvalidDataException($"Downward gate requested undefined projectile {request.Kind}."),
        };

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // The projectile spawn's deliberate finite-pool failure.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Gate, graphics: default);
        projectile.StartAnimation(new DownwardGateProjectileAnimation(request.Kind == DownwardGateProjectileKind.Closed
            ? DownwardGateProjectileSegment.Closed
            : DownwardGateProjectileSegment.Moving));
        int blockX = request.RoomObjectBlock.X;
        int blockY = request.RoomObjectBlock.Y;
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
            blockX * DownwardGateDefinitions.PixelsPerRoomBlock), CoordinateAxis.Wrap(
            blockY * DownwardGateDefinitions.PixelsPerRoomBlock +
            (request.Kind == DownwardGateProjectileKind.Closed
                ? DownwardGateDefinitions.ClosedPositionOffsetPixels
                : 0))));

        // The gate's block ties the actor to its gate; the gate's room object searches for it when the gate needs to open again.
        projectile.State = new DownwardGateState(request.RoomObjectBlock);
    }

    /// <summary>Finds the matching gate actor from the pool's end and wakes its sleeping animation.</summary>
    private void WakeDownwardGateProjectile(Position16<RoomBlocks> roomObjectBlock)
    {
        EnemyProjectile? projectile = null;
        for (int index = _projectiles.EnemyProjectiles.Count - 1; index >= 0; index--)
        {
            EnemyProjectile candidate = _projectiles.EnemyProjectiles[index];
            if (candidate.IsActive &&
                candidate.Kind is (RoomEnemyProjectileKind.DownwardGateMoving or
                    RoomEnemyProjectileKind.DownwardGateClosed) &&
                candidate.StateOf<DownwardGateState>().RoomObjectBlock == roomObjectBlock)
            {
                projectile = candidate;
                break;
            }
        }
        if (projectile is null)
        {
            throw new InvalidDataException(
                $"Downward gate room object at block {roomObjectBlock} has no associated gate projectile.");
        }

        ResumeDownwardGate(projectile);
    }

    /// <summary>Moves the gate one block at a time with signed 1/256-pixel motion.</summary>
    internal static void RunDownwardGateProjectileMovement(EnemyProjectile projectile)
    {
        DownwardGateState gate = projectile.StateOf<DownwardGateState>();
        int speedMagnitude = Math.Abs(projectile.Velocity.Y.InTwoHundredFiftySixthsOfPixel);
        int travelled = gate.TravelSinceBlock + speedMagnitude;
        if (travelled >= DownwardGateDefinitions.OneBlockDistance)
        {
            // Steps the animation past its sleep, which wakes the next visual frame
            // on this same projectile-handler pass.
            ResumeDownwardGate(projectile);
            travelled = 0;
        }
        gate.TravelSinceBlock = travelled;
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
    }

    /// <summary>
    /// Steps a gate past the sleep its animation holds, as stepping its animation one entry on with a one-update timer
    /// does: the animation resumes with the one authored after that sleep.
    /// </summary>
    private static void ResumeDownwardGate(EnemyProjectile projectile)
    {
        DownwardGateProjectileSegment sleeping = projectile.Animation is DownwardGateProjectileAnimation gate
            ? gate.PlayingSegment
            : throw new InvalidOperationException($"A downward gate resumed without its typed program; it runs {projectile.Animation}.");
        projectile.StartAnimation(new DownwardGateProjectileAnimation(DownwardGateProjectileAnimations.Resumption[sleeping]));
    }

    /// <summary>Carries out one cue of the gate's animations.</summary>
    private static void ApplyDownwardGateCue(EnemyProjectile projectile, DownwardGateProjectileCue cue)
    {
        switch (cue)
        {
            case DownwardGateProjectileCue.SetYVelocity setYVelocity:
                projectile.Velocity = projectile.Velocity.WithY(setYVelocity.YVelocity);
                break;
            case DownwardGateProjectileCue.SetFrameBehavior setFrameBehavior:
                projectile.FrameBehavior = setFrameBehavior.FrameBehavior;
                break;
            case DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.ClearFrameBehavior }:
                projectile.FrameBehavior = EnemyProjectileFrameBehavior.Stationary;
                break;
            case DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.Delete }:
                projectile.Delete();
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(DownwardGateProjectileCueKind)} {cue}.");
        }
    }

    /// <summary>A downward gate, moving or closed (#1760).</summary>
    internal static readonly EnemyProjectileBehavior Gate = new GateBehavior();

    /// <summary>A downward gate: it slides down by its velocity once its animation sets it moving, and handles its animation's cues.</summary>
    private sealed class GateBehavior() : SingleMotionProjectileBehavior(
        EnemyProjectileFrameBehavior.DownwardGateDescent,
        context => RunDownwardGateProjectileMovement(context.Projectile)), IEnemyProjectileCues<DownwardGateProjectileCue>
    {
        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, DownwardGateProjectileCue cue) => ApplyDownwardGateCue(context.Projectile, cue);
    }
}
