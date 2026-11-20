using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
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
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new CacatacEntity(this, place, common));

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
    private void ApplyCacatacCue(RoomEnemyPlace place, CacatacEnemyState state, CacatacCue cue)
    {
        switch (cue)
        {
            case CacatacCue.Signal { Kind: CacatacCueKind.RestorePatrol }:
                RestoreCacatacPatrol(state);
                break;
            case CacatacCue.Signal { Kind: CacatacCueKind.PlaySpikeSound }:
                PlayCacatacSpikeSound();
                break;
            case CacatacCue.SpawnSpike spike:
                SpawnCacatacSpike(place, spike.Direction);
                break;
            default:
                throw new InvalidOperationException($"Undefined Cacatac cue {cue}.");
        }
    }

    /// <summary>Initializes the Cacatac.</summary>
    internal static void InitializeCacatac(RoomEnemyPlace place)
    {
        CacatacSpawnSettings settings = place.SpawnSettings<CacatacSpawnSettings>();
        bool onFloor = settings.Mounting == CacatacMounting.Floor;
        place.Frame = EnemyFrameId.Empty;

        int spawnX = place.Position.X.Whole;
        if (!Enum.IsDefined(settings.Direction))
            throw new InvalidOperationException($"Undefined Cacatac direction {settings.Direction}.");
        // A Cacatac placed to stop at first stands still, then patrols its direction from its first idle step.
        var state = new CacatacEnemyState(settings.Start switch
        {
            CacatacStart.Patrolling => CacatacAction.Patrolling,
            CacatacStart.Stopped => CacatacAction.Stopped,
            _ => throw new InvalidOperationException($"Undefined {nameof(CacatacStart)} {settings.Start}."),
        })
        {
            PatrolDirection = settings.Direction,
            RightPatrolX = spawnX + settings.TravelDistance,
            LeftPatrolX = spawnX - settings.TravelDistance,
        };
        place.SetSpeciesState(state);
        SetCacatacAnimation(place, state, onFloor ? CacatacSegment.OnFloorIdle : CacatacSegment.OnCeilingIdle);
    }

    /// <summary>Runs a Cacatac for one update through its three actions.</summary>
    private void RunCacatacMain(RoomEnemyPlace place, CacatacEnemyState state)
    {
        switch (state.Action)
        {
            case CacatacAction.Patrolling:
                MoveCacatac(place, state);
                MaybeStartCacatacAttack(place, state);
                return;
            case CacatacAction.Stopped:
                return;
            default:
                throw new InvalidDataException(
                    $"Cacatac has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>
    /// Moves the Cacatac along its patrol by its speed, then turns it once it passes the left limit or reaches or passes
    /// the right one.
    /// </summary>
    private static void MoveCacatac(RoomEnemyPlace place, CacatacEnemyState state)
    {
        bool right = state.PatrolDirection == HorizontalDirection.Right;
        SignedSixteenSixteen speed = place.SpawnSettings<CacatacSpawnSettings>().Speed;
        place.Position = place.Position.AdvanceX(speed.Toward(state.PatrolDirection));

        if (!right)
        {
            if (place.Position.X.Whole < state.LeftPatrolX)
                state.PatrolDirection = HorizontalDirection.Right;
            return;
        }

        if (place.Position.X.Whole >= state.RightPatrolX)
            state.PatrolDirection = HorizontalDirection.Left;
    }

    /// <summary>The 3/256 random attack gate.</summary>
    private void MaybeStartCacatacAttack(RoomEnemyPlace place, CacatacEnemyState state)
    {
        if (!CacatacAttackDefinitions.Attacks.IsTakenBy(_room.Random.Generate(), advancedBy: place.FrameCounter))
            return;

        state.Action = CacatacAction.Stopped;
        SetCacatacAnimation(place, state, place.SpawnSettings<CacatacSpawnSettings>().Mounting == CacatacMounting.Floor ? CacatacSegment.OnFloorAttack : CacatacSegment.OnCeilingAttack);
    }

    /// <summary>Animation cue: restore patrol the way it last patrolled.</summary>
    private static void RestoreCacatacPatrol(CacatacEnemyState state) => state.Action = CacatacAction.Patrolling;

    /// <summary>Animation cue: publish the spike sound.</summary>
    private void PlayCacatacSpikeSound() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.CacatacSpikeSound, priority: SoundRequestPriority.Normal);

    /// <summary>Starts a patrol or attack animation on the next animation tick.</summary>
    internal static void SetCacatacAnimation(RoomEnemyPlace place, CacatacEnemyState state, CacatacSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, CacatacAnimations.StartingWith(segment));
    }

    /// <summary>A Cacatac spike (#1760): it flies by its direction's velocity until it leaves the camera.</summary>
    internal static readonly EnemyProjectileBehavior Spike = new SingleMotionProjectileBehavior(
        context => FlyCacatacSpike(context.Projectile, context.Camera));

    /// <summary>Runs Cacatac's main AI.</summary>
    internal void RunCacatacMainAi(RoomEnemyPlace place)
    {
        RunCacatacMain(place, place.RequireSpeciesState<CacatacEnemyState>());
    }

    /// <summary>Advances Cacatac's typed animation.</summary>
    internal void AdvanceCacatacTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, CacatacAnimations.Segments,
            static (CacatacEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (CacatacEnemyState state, CacatacCue cue) => ApplyCacatacCue(place, state, cue));

    /// <summary>
    /// Spawns a spike projectile from the Cacatac and initializes it.
    /// The source actor supplies the origin's fractions of a pixel and the sheet and colours its spikes draw in.
    /// </summary>
    internal void SpawnCacatacSpike(RoomEnemyPlace source, CacatacSpikeDirection direction)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile();
        if (projectile is null)
            return;

        EnemyProjectileEngine.InitializeEnemyProjectileFromDefinition(
            projectile,
            RoomEnemyProjectileKind.CacatacSpike,
            Spike,
            SpriteGraphics.SpawnedBy(source));
        projectile.InstallAnimation(new CacatacProjectileAnimation(direction));
        projectile.Position = source.Position;
        projectile.Velocity = CacatacSpikes.Velocity(direction);
    }

    /// <summary>
    /// Runs the spike for one update: the spike moves along its velocity, horizontally then vertically, then is culled
    /// once outside the view.
    /// </summary>
    internal static void FlyCacatacSpike(
        EnemyProjectile projectile,
        Position16<RoomPixels> camera)
    {
        projectile.Position = projectile.Position.AdvanceX(projectile.Velocity.X.ToSixteenSixteenDelta());
        projectile.Position = projectile.Position.AdvanceY(projectile.Velocity.Y.ToSixteenSixteenDelta());
        EnemyView.DeleteOutside(projectile, camera);
    }
}
