using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The Cacatac species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CacatacBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Cacatac];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new CacatacEntity(this, slot, common);

    /// <summary>Supplies shared room RNG for the three-in-256 spike attack gate.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Allocates spike projectiles with source position and sprite attributes.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds Cacatac patrol and spike behavior to room RNG and projectile allocation.</summary>
    internal CacatacBehavior(EnemyRoomBinding room, EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _room = room;
        _projectiles = projectiles;
    }

    /// <summary>Carries out one cue of Cacatac's animations.</summary>
    private CacatacSegment? ApplyCacatacCue(RoomEnemySlot slot, CacatacEnemyState state, CacatacCue cue)
    {
        switch (cue.Kind)
        {
            case CacatacCueKind.RestorePatrol:
                RestoreCacatacPatrol(state);
                break;
            case CacatacCueKind.PlaySpikeSound:
                PlayCacatacSpikeSound();
                break;
            case CacatacCueKind.SpawnSpike:
                SpawnCacatacSpike(slot, cue.Direction);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(CacatacCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Initializes the Cacatac.</summary>
    internal static void InitializeCacatac(RoomEnemySlot slot)
    {
        CacatacSpawnSettings settings = slot.SpawnSettings<CacatacSpawnSettings>();
        CacatacDirection direction = settings.Direction;

        bool upsideUp = settings.Mounting == CacatacMounting.Floor;
        slot.Frame = EnemyFrameId.Empty;

        var travel = new Offset16<RoomPixels>(settings.TravelDistance, 0);
        Position16<RoomPixels> spawn = slot.Position.Whole;
        var state = new CacatacEnemyState
        {
            RightVelocity = settings.Speed,
            LeftVelocity = SignedSixteenSixteen.Zero - settings.Speed,
            Direction = direction,
            Function = direction switch
            {
                CacatacDirection.Left => CacatacEnemyFunction.MovingLeft,
                CacatacDirection.Right => CacatacEnemyFunction.MovingRight,
                CacatacDirection.InitialStop => CacatacEnemyFunction.Stopped,
                _ => throw new InvalidOperationException($"Undefined Cacatac direction {direction}."),
            },
            RightPatrolLimit = spawn + travel,
            LeftPatrolLimit = spawn - travel,
            UpsideUp = upsideUp,
        };
        slot.SetSpeciesState(state);
        SetCacatacAnimation(slot, state, upsideUp ? CacatacSegment.UpsideUpIdle : CacatacSegment.UpsideDownIdle);
    }

    /// <summary>Runs a Cacatac's per-frame behaviour through its three functions.</summary>
    private void RunCacatacMain(RoomEnemySlot slot, CacatacEnemyState state)
    {
        switch (state.Function)
        {
            case CacatacEnemyFunction.MovingLeft:
                MoveCacatac(slot, state, direction: HorizontalDirection.Left);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.MovingRight:
                MoveCacatac(slot, state, direction: HorizontalDirection.Right);
                MaybeStartCacatacAttack(slot, state);
                return;
            case CacatacEnemyFunction.Stopped:
                return;
            default:
                throw new InvalidDataException(
                    $"Cacatac function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>
    /// Moves the Cacatac along its patrol: the 16.16 velocity is added to X with the
    /// fraction's overflow into the whole, then the wrapped signed distance to the patrol limit decides the turn.
    /// </summary>
    private static void MoveCacatac(
        RoomEnemySlot slot,
        CacatacEnemyState state,
        HorizontalDirection direction)
    {
        slot.Position = slot.Position.AdvanceX(direction == HorizontalDirection.Right ? state.RightVelocity : state.LeftVelocity);

        if (direction != HorizontalDirection.Right)
        {
            if ((slot.Position.Whole - state.LeftPatrolLimit).X >= 0)
                return;
            state.Function = CacatacEnemyFunction.MovingRight;
            state.Direction = CacatacDirection.Right;
            return;
        }

        if ((slot.Position.Whole - state.RightPatrolLimit).X < 0)
            return;
        state.Function = CacatacEnemyFunction.MovingLeft;
        state.Direction = CacatacDirection.Left;
    }

    /// <summary>The 3/256 random attack gate.</summary>
    private void MaybeStartCacatacAttack(RoomEnemySlot slot, CacatacEnemyState state)
    {
        if (!CacatacAttackDefinitions.Attacks.IsTakenBy(_room.Random.Generate(), advancedBy: slot.FrameCounter))
            return;

        state.Function = CacatacEnemyFunction.Stopped;
        SetCacatacAnimation(slot, state, state.UpsideUp ? CacatacSegment.UpsideUpAttack : CacatacSegment.UpsideDownAttack);
    }

    /// <summary>Animation cue: restore patrol from the stored direction.</summary>
    private static void RestoreCacatacPatrol(CacatacEnemyState state)
    {
        // The game tests the direction against zero, not against one. Its special initial direction two therefore
        // becomes moving-right once the first idle step executes.
        state.Function = state.Direction == CacatacDirection.Left
            ? CacatacEnemyFunction.MovingLeft
            : CacatacEnemyFunction.MovingRight;
    }

    /// <summary>Animation cue: publish the spike sound.</summary>
    private void PlayCacatacSpikeSound() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.CacatacSpikeSound, priority: SoundRequestPriority.Normal);

    /// <summary>Starts a patrol or attack animation on the next animation tick.</summary>
    internal static void SetCacatacAnimation(RoomEnemySlot slot, CacatacEnemyState state, CacatacSegment program)
    {
        state.Playback = EnemyAnimationPlayback.Start(slot, CacatacAnimations.StartingWith(program));
    }

    /// <summary>Registers the spike's ten direction-table movers and viewport cull.</summary>
    internal static void AddCacatacProjectiles(EnemyProjectileHandlerTable table)
    {
        table.AddFrameBehavior(context =>
        {
            // Cacatac spike: ten direction-table movers.
            RunCacatacSpikeFrameBehavior(context.Projectile, context.Camera);
        }, EnemyProjectileFrameBehavior.CacatacSpikeFlight);
    }

    /// <summary>Runs Cacatac's main AI.</summary>
    internal void RunCacatacMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunCacatacMain(slot, slot.RequireSpeciesState<CacatacEnemyState>());
    }

    /// <summary>Advances Cacatac's typed animation.</summary>
    internal void AdvanceCacatacTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, CacatacAnimations.Segments,
            static (CacatacEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) => ApplyCacatacCue(slot, state, cue) is { } target
                ? new AnimationContinuation<CacatacSegment>(CacatacAnimations.StartingWith(target), AnimationPlace.StartOfAnimation)
                : null);

    /// <summary>
    /// Spawns a spike projectile from the Cacatac and initializes it.
    /// The source actor supplies both origin subpixels and the sheet and colours its spikes draw in.
    /// </summary>
    internal void SpawnCacatacSpike(RoomEnemySlot source, CacatacSpikeDirection direction)
    {
        // Rejects an odd or out-of-table direction before allocating, as the selector check does.
        CacatacProjectileSegment program = CacatacSpikes.Animation(direction);

        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            SpriteGraphics.SpawnedBy(source));
        projectile.InstallAnimation(new CacatacProjectileAnimation(program));
        projectile.Position = source.Position;
        projectile.Velocity = CacatacSpikes.Velocity(direction);
    }

    /// <summary>
    /// Runs the spike's per-frame behaviour: the spike moves along its velocity, horizontally then vertically, then is culled
    /// once outside the view.
    /// </summary>
    internal static void RunCacatacSpikeFrameBehavior(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        DeleteEnemyProjectileIfOutsideInclusiveViewport(projectile, camera);
    }

    /// <summary>
    /// Shared clone of the signed four-edge viewport cull. Equality with the right or
    /// bottom edge remains alive; only strictly outside coordinates delete the projectile.
    /// </summary>
    internal static void DeleteEnemyProjectileIfOutsideInclusiveViewport(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        int right = CoordinateAxis.Wrap(camera.X + 256);
        int bottom = CoordinateAxis.Wrap(camera.Y + 256);
        bool outside = CoordinateAxis.Difference(projectile.Position.X.Whole, camera.X) < 0 ||
            CoordinateAxis.Difference(right, projectile.Position.X.Whole) < 0 ||
            CoordinateAxis.Difference(projectile.Position.Y.Whole, camera.Y) < 0 ||
            CoordinateAxis.Difference(bottom, projectile.Position.Y.Whole) < 0;
        if (outside)
            projectile.Delete();
    }
}
