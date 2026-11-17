using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The downward gate species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DownwardGateBehavior
{
    /// <summary>Shared finite projectile pool holding moving and closed gate actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Binds gate room-object requests to the current room's projectile owner.</summary>
    internal DownwardGateBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Applies one gate request to the room's enemy projectiles.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request)
    {

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
        bool closed = request.Kind == DownwardGateProjectileKind.Closed;
        projectile.StartAnimation(new DownwardGateProjectileAnimation(closed
            ? DownwardGateProjectileSegment.Closed
            : DownwardGateProjectileSegment.Moving));
        int blockX = request.RoomObjectBlock.X;
        int blockY = request.RoomObjectBlock.Y;
        projectile.Position = projectile.Position.WithWhole(new(CoordinateAxis.Wrap(
            blockX * RoomGridConversions.BlockPixels), CoordinateAxis.Wrap(
            blockY * RoomGridConversions.BlockPixels +
            (request.Kind == DownwardGateProjectileKind.Closed
                ? DownwardGateDefinitions.ClosedPositionOffsetPixels
                : 0))));

        // The gate's block ties the actor to its gate; the gate's room object searches for it when the gate needs to open again.
        projectile.State = new DownwardGateState(request.RoomObjectBlock,
            closed ? DownwardGateProjectileAnimations.ClosedStage : DownwardGateProjectileAnimations.SlidingDownStage);
    }

    /// <summary>Finds the matching gate actor from the pool's end and takes it on to its next stage, a closed gate rising.</summary>
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

        AdvanceDownwardGateStage(projectile);
    }

    /// <summary>
    /// Moves the gate at its speed, a whole pixel an update, and shows its next place each time it has moved a block.
    /// </summary>
    internal static void RunDownwardGateProjectileMovement(EnemyProjectile projectile)
    {
        DownwardGateState gate = projectile.StateOf<DownwardGateState>();
        int moved = gate.PixelsSinceBlock + projectile.Velocity.Y.MagnitudeWholePixels;
        if (moved >= RoomGridConversions.BlockPixels)
        {
            // The next stage's animation shows its first frame on this same enemy-projectile pass.
            AdvanceDownwardGateStage(projectile);
            moved = 0;
        }
        gate.PixelsSinceBlock = moved;
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Takes a gate on to the next stage of its travel, starting the animation that stage shows.</summary>
    private static void AdvanceDownwardGateStage(EnemyProjectile projectile)
    {
        DownwardGateState gate = projectile.StateOf<DownwardGateState>();
        gate.Stage = DownwardGateProjectileAnimations.NextStage.TryGetValue(gate.Stage, out DownwardGateProjectileSegment next)
            ? next
            : throw new InvalidOperationException($"A downward gate at stage {gate.Stage} has no stage to go on to.");
        projectile.StartAnimation(new DownwardGateProjectileAnimation(gate.Stage));
    }

    /// <summary>Carries out one cue of the gate's animations.</summary>
    private static void ApplyDownwardGateCue(EnemyProjectile projectile, DownwardGateProjectileCue cue)
    {
        switch (cue)
        {
            case DownwardGateProjectileCue.SetYVelocity setYVelocity:
                projectile.Velocity = projectile.Velocity.WithY(setYVelocity.YVelocity);
                break;
            case DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.StartSliding }:
                SingleMotionProjectileBehavior.StartMoving(projectile);
                break;
            case DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.StopSliding }:
                projectile.HoldStill();
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

    /// <summary>
    /// A downward gate: it slides by its vertical velocity, down while moving and up while closed, from when its animation
    /// starts it sliding until the animation stops it, and handles its animation's cues.
    /// </summary>
    private sealed class GateBehavior() : SingleMotionProjectileBehavior(
        context => RunDownwardGateProjectileMovement(context.Projectile), initial: null), IEnemyProjectileCues<DownwardGateProjectileCue>
    {
        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, DownwardGateProjectileCue cue) => ApplyDownwardGateCue(context.Projectile, cue);
    }
}
