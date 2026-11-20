using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Rooms.RoomObjects;

namespace SuperMetroid.Core.Enemies.Projectiles;

/// <summary>The downward gates: each gate moving down to close, closed, and rising again when woken.</summary>
[RoomLifetime]
internal sealed class DownwardGateBehavior
{
    /// <summary>Shared finite projectile pool holding moving and closed gate actors.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>
    /// Each gate's own projectile, by the gate's room block (#2455): the last one spawned for it, which its wake takes on.
    /// A room has one gate per block.
    /// </summary>
    private readonly Dictionary<Position16<RoomBlocks>, DownwardGateProjectile> _gateProjectiles = [];

    /// <summary>Binds gate room-object requests to the current room's projectile owner.</summary>
    internal DownwardGateBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Applies one gate request to the room's enemy projectiles.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request)
    {
        switch (request)
        {
            case DownwardGateProjectileSpawn spawn:
                SpawnDownwardGateProjectile(spawn);
                return;
            case DownwardGateProjectileWake wake:
                WakeDownwardGateProjectile(wake.RoomObjectBlock);
                return;
            default:
                throw new ArgumentOutOfRangeException(
                    nameof(request), request, "Unknown downward gate projectile request.");
        }
    }

    /// <summary>Allocates a gate actor at its room object's block and ties it to its gate's block for waking.</summary>
    private void SpawnDownwardGateProjectile(
        DownwardGateProjectileSpawn request)
    {
        RoomEnemyProjectileKind kind = request.Kind switch
        {
            DownwardGateProjectileKind.Closed => RoomEnemyProjectileKind.DownwardGateClosed,
            DownwardGateProjectileKind.Moving => RoomEnemyProjectileKind.DownwardGateMoving,
            _ => throw new InvalidDataException($"Downward gate requested undefined projectile {request.Kind}."),
        };

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // The projectile spawn's deliberate finite-pool failure.

        bool closed = request.Kind == DownwardGateProjectileKind.Closed;
        // The gate's block ties the actor to its gate, which keeps it to wake it when the gate opens again.
        var gate = new DownwardGateState(request.RoomObjectBlock,
            closed ? DownwardGateProjectileAnimations.ClosedStage : DownwardGateProjectileAnimations.SlidingDownStage);
        var behavior = new GateBehavior(gate);
        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, behavior, graphics: default);
        projectile.StartAnimation(new DownwardGateProjectileAnimation(closed
            ? DownwardGateProjectileSegment.Closed
            : DownwardGateProjectileSegment.Moving, behavior));
        int blockX = request.RoomObjectBlock.X;
        int blockY = request.RoomObjectBlock.Y;
        projectile.Position = projectile.Position.WithWhole(new(
            blockX * RoomGridConversions.BlockPixels,
            blockY * RoomGridConversions.BlockPixels +
            (request.Kind == DownwardGateProjectileKind.Closed
                ? DownwardGateDefinitions.ClosedPositionOffsetPixels
                : 0)));

        _gateProjectiles[request.RoomObjectBlock] = new DownwardGateProjectile(projectile, behavior);
    }

    /// <summary>Takes the gate's own projectile on to its next stage, a closed gate rising.</summary>
    /// <exception cref="InvalidDataException">The gate's projectile has gone or now holds something else.</exception>
    private void WakeDownwardGateProjectile(Position16<RoomBlocks> roomObjectBlock)
    {
        if (!_gateProjectiles.TryGetValue(roomObjectBlock, out DownwardGateProjectile? gate) ||
            !gate.Projectile.IsActive ||
            gate.Projectile.Kind is not (RoomEnemyProjectileKind.DownwardGateMoving or RoomEnemyProjectileKind.DownwardGateClosed))
        {
            throw new InvalidDataException(
                $"Downward gate room object at block {roomObjectBlock} has no associated gate projectile.");
        }

        gate.Behavior.AdvanceStage(gate.Projectile);
    }

    /// <summary>
    /// Moves the gate at its speed, a whole pixel an update, and shows its next place each time it has moved a block.
    /// </summary>
    /// <param name="projectile">The gate's projectile.</param>
    /// <param name="gate">What the gate's actor keeps.</param>
    /// <param name="cues">The gate's behaviour, which owns its animations' cues.</param>
    internal static void RunDownwardGateProjectileMovement(EnemyProjectile projectile, DownwardGateState gate,
        IEnemyProjectileCues<DownwardGateProjectileCue> cues)
    {
        for (int pixel = 0; pixel < projectile.Velocity.Y.MagnitudeWholePixels; pixel++)
            gate.BlockTravel.Step();
        if (gate.BlockTravel.IsOver)
        {
            // The next stage's animation shows its first frame on this same enemy-projectile pass; the travel starts again
            // from the block, whatever the update moved past it.
            AdvanceDownwardGateStage(projectile, gate, cues);
            gate.BlockTravel.Restart(RoomGridConversions.BlockPixels);
        }
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Takes a gate on to the next stage of its travel, starting the animation that stage shows.</summary>
    private static void AdvanceDownwardGateStage(EnemyProjectile projectile, DownwardGateState gate, IEnemyProjectileCues<DownwardGateProjectileCue> cues)
    {
        gate.Stage = DownwardGateProjectileAnimations.NextStage.TryGetValue(gate.Stage, out DownwardGateProjectileSegment next)
            ? next
            : throw new InvalidOperationException($"A downward gate at stage {gate.Stage} has no stage to go on to.");
        projectile.StartAnimation(new DownwardGateProjectileAnimation(gate.Stage, cues));
    }

    /// <summary>
    /// A downward gate: it slides by its vertical velocity, down while moving and up while closed, from when its animation
    /// starts it sliding until the animation stops it, and handles its animation's cues.
    /// </summary>
    /// <param name="gate">What the gate's actor keeps: its gate's block, its stage and its travel through the block.</param>
    internal sealed class GateBehavior(DownwardGateState gate)
        : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<DownwardGateProjectileCue>
    {
        /// <summary>Takes the gate on to the next stage of its travel, starting the animation that stage shows.</summary>
        /// <param name="projectile">The gate's projectile.</param>
        internal void AdvanceStage(EnemyProjectile projectile) => AdvanceDownwardGateStage(projectile, gate, this);

        /// <inheritdoc/>
        private protected override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            RunDownwardGateProjectileMovement(context.Projectile, gate, this);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, DownwardGateProjectileCue cue)
        {
            EnemyProjectile projectile = context.Projectile;
            switch (cue)
            {
                case DownwardGateProjectileCue.SetYVelocity setYVelocity:
                    projectile.Velocity = projectile.Velocity.WithY(setYVelocity.YVelocity);
                    break;
                case DownwardGateProjectileCue.Signal { Kind: DownwardGateProjectileCueKind.StartSliding }:
                    StartMotion(projectile, SingleMotion.Moving);
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
    }

    /// <summary>A gate's actor and what it keeps, which its gate wakes when it opens again.</summary>
    /// <param name="Projectile">The gate's actor.</param>
    /// <param name="Behavior">The actor's behaviour, which keeps its stage and owns its animations' cues.</param>
    private sealed record DownwardGateProjectile(EnemyProjectile Projectile, GateBehavior Behavior);
}
