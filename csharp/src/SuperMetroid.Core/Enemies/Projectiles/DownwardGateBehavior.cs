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
    private readonly Dictionary<Position16<RoomBlocks>, EnemyProjectile> _gateProjectiles = [];

    /// <summary>Binds gate room-object requests to the current room's projectile owner.</summary>
    internal DownwardGateBehavior(EnemyProjectileEngine projectiles)
    {
        _projectiles = projectiles;
    }

    /// <summary>Applies one gate request to the room's enemy projectiles.</summary>
    public void ApplyDownwardGateProjectileRequest(
        DownwardGateProjectileRequest request)
    {
        request.React(ApplyDownwardGateProjectileRequestReactions, this);
    }

    /// <summary>What <c>ApplyDownwardGateProjectileRequest</c> does on each <see cref="DownwardGateProjectileRequest"/>, built once (#2553).</summary>
    private static DownwardGateProjectileRequestReactions<DownwardGateBehavior> ApplyDownwardGateProjectileRequestReactions { get; } = new(
        DownwardGateProjectileSpawn: static (spawn, owner) => owner.SpawnDownwardGateProjectile(spawn),
        DownwardGateProjectileWake: static (wake, owner) => owner.WakeDownwardGateProjectile(wake.RoomObjectBlock));

    /// <summary>Allocates a gate actor at its room object's block and ties it to its gate's block for waking.</summary>
    private void SpawnDownwardGateProjectile(
        DownwardGateProjectileSpawn request)
    {
        RoomEnemyProjectileKind kind = request.Kind == DownwardGateProjectileKind.Closed
            ? RoomEnemyProjectileKind.DownwardGateClosed
            // The remaining member, DownwardGateProjectileKind.Moving.
            : RoomEnemyProjectileKind.DownwardGateMoving;

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return; // The projectile spawn's deliberate finite-pool failure.

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(projectile, kind, Gate, graphics: default);
        bool closed = request.Kind == DownwardGateProjectileKind.Closed;
        projectile.StartAnimation(new DownwardGateProjectileAnimation(Gate, closed
            ? DownwardGateProjectileSegment.Closed
            : DownwardGateProjectileSegment.Moving));
        int blockX = request.RoomObjectBlock.X;
        int blockY = request.RoomObjectBlock.Y;
        projectile.Position = projectile.Position.WithWhole(new(
            blockX * RoomGridConversions.BlockPixels,
            blockY * RoomGridConversions.BlockPixels +
            (request.Kind == DownwardGateProjectileKind.Closed
                ? DownwardGateDefinitions.ClosedPositionOffsetPixels
                : 0)));

        // The gate's block ties the actor to its gate, which keeps it to wake it when the gate opens again.
        Gate.Gates.Start(projectile, new DownwardGateState(request.RoomObjectBlock,
            closed ? DownwardGateProjectileAnimations.ClosedStage : DownwardGateProjectileAnimations.SlidingDownStage));
        _gateProjectiles[request.RoomObjectBlock] = projectile;
    }

    /// <summary>Takes the gate's own projectile on to its next stage, a closed gate rising.</summary>
    /// <exception cref="InvalidDataException">The gate's projectile has gone or now holds something else.</exception>
    private void WakeDownwardGateProjectile(Position16<RoomBlocks> roomObjectBlock)
    {
        if (!_gateProjectiles.TryGetValue(roomObjectBlock, out EnemyProjectile? projectile) ||
            !projectile.IsActive ||
            projectile.Kind is not (RoomEnemyProjectileKind.DownwardGateMoving or RoomEnemyProjectileKind.DownwardGateClosed) ||
            Gate.Gates.Of(projectile).RoomObjectBlock != roomObjectBlock)
        {
            throw new InvalidDataException(
                $"Downward gate room object at block {roomObjectBlock} has no associated gate projectile.");
        }

        AdvanceDownwardGateStage(Gate, projectile);
    }

    /// <summary>
    /// Moves the gate at its speed, a whole pixel an update, and shows its next place each time it has moved a block.
    /// </summary>
    private static void RunDownwardGateProjectileMovement(GateBehavior owner, EnemyProjectile projectile)
    {
        DownwardGateState gate = owner.Gates.Of(projectile);
        for (int pixel = 0; pixel < projectile.Velocity.Y.MagnitudeWholePixels; pixel++)
            gate.BlockTravel.Step();
        if (gate.BlockTravel.IsOver)
        {
            // The next stage's animation shows its first frame on this same enemy-projectile pass; the travel starts again
            // from the block, whatever the update moved past it.
            AdvanceDownwardGateStage(owner, projectile);
            gate.BlockTravel.Restart(RoomGridConversions.BlockPixels);
        }
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
    }

    /// <summary>Takes a gate on to the next stage of its travel, starting the animation that stage shows.</summary>
    /// <param name="owner">The gate's behaviour, which carries out its animations' events.</param>
    /// <param name="projectile">The gate.</param>
    private static void AdvanceDownwardGateStage(GateBehavior owner, EnemyProjectile projectile)
    {
        DownwardGateState gate = owner.Gates.Of(projectile);
        gate.Stage = DownwardGateProjectileAnimations.NextStage.TryGetValue(gate.Stage, out DownwardGateProjectileSegment next)
            ? next
            : throw new InvalidOperationException($"A downward gate at stage {gate.Stage} has no stage to go on to.");
        projectile.StartAnimation(new DownwardGateProjectileAnimation(owner, gate.Stage));
    }

    /// <summary>What <c>ApplyDownwardGateCue</c> does for each <see cref="DownwardGateProjectileCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<DownwardGateProjectileCueKind, Action<GateBehavior, EnemyProjectile>> ApplyDownwardGateCueForDownwardGateProjectileCueKind => field ??= new(new Dictionary<DownwardGateProjectileCueKind, Action<GateBehavior, EnemyProjectile>>
    {
        [DownwardGateProjectileCueKind.StartSliding] = static (owner, projectile) => owner.StartMoving(projectile),
        [DownwardGateProjectileCueKind.StopSliding] = static (owner, projectile) => projectile.HoldStill(),
        [DownwardGateProjectileCueKind.Delete] = static (owner, projectile) => projectile.Delete(),
    });

    /// <summary>Carries out one cue of the gate's animations.</summary>
    private static void ApplyDownwardGateCue(GateBehavior owner, EnemyProjectile projectile, DownwardGateProjectileCue cue)
    {
        cue.React(ApplyDownwardGateCueReactions, new ApplyDownwardGateCueContext(projectile, owner));
    }

    /// <summary>What <c>ApplyDownwardGateCue</c> does on each <see cref="DownwardGateProjectileCue"/>, built once (#2553).</summary>
    private static DownwardGateProjectileCueReactions<ApplyDownwardGateCueContext> ApplyDownwardGateCueReactions { get; } = new(
        SetYVelocity: static (setYVelocity, context) => context.Projectile.Velocity = context.Projectile.Velocity.WithY(setYVelocity.YVelocity),
        Signal: static (signal, context) =>
        {
            ApplyDownwardGateCueForDownwardGateProjectileCueKind[signal.Kind](context.Owner, context.Projectile);
        });

    /// <summary>What <c>ApplyDownwardGateCue</c>'s cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Projectile">The <c>projectile</c> <c>ApplyDownwardGateCue</c> was handed.</param>
    /// <param name="Owner">The <c>owner</c> <c>ApplyDownwardGateCue</c> was handed.</param>
    private readonly record struct ApplyDownwardGateCueContext(EnemyProjectile Projectile, GateBehavior Owner);

    /// <summary>A downward gate, moving or closed (#1760).</summary>
    private GateBehavior Gate { get; } = new();

    /// <summary>
    /// A downward gate: it slides by its vertical velocity, down while moving and up while closed, from when its animation
    /// starts it sliding until the animation stops it, and handles its animation's cues.
    /// </summary>
    internal sealed class GateBehavior() : EnemyProjectileMotionBehavior<SingleMotion>(initial: null), IEnemyProjectileCues<DownwardGateProjectileCue>
    {
        /// <summary>Each gate's block and the stage it is at, given as the gate's room object raises it.</summary>
        internal EnemyProjectileStates<DownwardGateState> Gates { get; } = new();

        /// <summary>Starts <paramref name="projectile"/>, a gate, sliding.</summary>
        /// <param name="projectile">A gate this behaviour is bound to.</param>
        internal void StartMoving(EnemyProjectile projectile) => Ensure.NotNull(projectile).StartMotion(this, SingleMotion.Moving);

        /// <inheritdoc/>
        internal override void Move(EnemyProjectileMotionContext context, SingleMotion motion) =>
            RunDownwardGateProjectileMovement(this, context.Projectile);

        /// <inheritdoc/>
        public void RunCue(EnemyProjectileCueContext context, DownwardGateProjectileCue cue) => ApplyDownwardGateCue(this, context.Projectile, cue);
    }
}
