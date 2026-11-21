using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Artwork;

namespace SuperMetroid.Core.Enemies.Cacatac;

/// <summary>The Cacatac species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class CacatacBehavior : IEnemySpecies, IEnemySpawnFactory<CacatacSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Cacatac];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, CacatacSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new CacatacEntity(this, occupant, place, common, settings));

    /// <summary>Supplies shared room RNG for the spike attack gate (<see cref="CacatacAttackDefinitions.Attacks"/>).</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Spawns spikes at the Cacatac, drawn in its colours.</summary>
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

    /// <summary>What <see cref="ApplyCacatacCue"/> does for each <see cref="CacatacCueKind"/> member, each member's own arm.</summary>
    private IdentityTable<CacatacCueKind, Action<CacatacEnemyState>> ApplyCacatacCueForCacatacCueKind => field ??= new(new Dictionary<CacatacCueKind, Action<CacatacEnemyState>>
    {
        [CacatacCueKind.RestorePatrol] = state => RestoreCacatacPatrol(state),
        [CacatacCueKind.PlaySpikeSound] = state => PlayCacatacSpikeSound(),
    });

    /// <summary>Carries out one cue of Cacatac's animations.</summary>
    private void ApplyCacatacCue(RoomEnemyOccupant occupant, RoomEnemyPlace place, CacatacEnemyState state, CacatacCue cue)
    {
        cue.React(ApplyCacatacCueReactions, new ApplyCacatacCueContext(occupant, place, state, this));
    }

    /// <summary>What <see cref="ApplyCacatacCue"/> does on each <see cref="CacatacCue"/>, built once (#2553).</summary>
    private static CacatacCueReactions<ApplyCacatacCueContext> ApplyCacatacCueReactions { get; } = new(
        SpawnSpike: static (spike, context) => context.Owner.SpawnCacatacSpike(context.Occupant, context.Place, spike.Direction),
        Signal: static (signal, context) =>
        {
            context.Owner.ApplyCacatacCueForCacatacCueKind[signal.Kind](context.State);
        });

    /// <summary>What the cases read besides the case (#2553), handed to them so they are built once.</summary>
    /// <param name="Occupant">The occupant whose cue is carried out.</param>
    /// <param name="Place">The place the cases read.</param>
    /// <param name="State">The state the cases read.</param>
    /// <param name="Owner">The <see cref="CacatacBehavior"/> reacting.</param>
    private readonly record struct ApplyCacatacCueContext(RoomEnemyOccupant Occupant, RoomEnemyPlace Place, CacatacEnemyState State, CacatacBehavior Owner);

    /// <summary>Initializes the Cacatac.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static CacatacEnemyState InitializeCacatac(CacatacSpawnSettings settings, RoomEnemyPlace place)
    {
        bool onFloor = settings.Mounting == CacatacMounting.Floor;
        place.Frame = EnemyFrameId.PointAtOrigin;

        int spawnX = place.Position.X.Whole;
        // A Cacatac placed to stop at first stands still, then patrols its direction from its first idle step.
        var state = new CacatacEnemyState(settings.Start)
        {
            PatrolDirection = settings.Direction,
            RightPatrolX = spawnX + settings.TravelDistance,
            LeftPatrolX = spawnX - settings.TravelDistance,
        };
        SetCacatacAnimation(place, state, onFloor ? CacatacSegment.OnFloorIdle : CacatacSegment.OnCeilingIdle);
        return state;
    }

    /// <summary>What <see cref="RunCacatacMain"/> does for each <see cref="CacatacAction"/> member, each member's own arm.</summary>
    private IdentityTable<CacatacAction, Action<CacatacSpawnSettings, RoomEnemyPlace, CacatacEnemyState>> RunCacatacMainForCacatacAction => field ??= new(new Dictionary<CacatacAction, Action<CacatacSpawnSettings, RoomEnemyPlace, CacatacEnemyState>>
    {
        [CacatacAction.Patrolling] = (settings, place, state) =>
        {
            MoveCacatac(settings, place, state);
            MaybeStartCacatacAttack(settings, place, state);
        },
        [CacatacAction.Stopped] = (_, place, state) => { },
    });

    /// <summary>Runs a Cacatac for one update: its patrol, or it stays stopped.</summary>
    private void RunCacatacMain(CacatacSpawnSettings cacatacSpawnSettings, RoomEnemyPlace place, CacatacEnemyState state)
    {
        RunCacatacMainForCacatacAction[state.Action](cacatacSpawnSettings, place, state);
    }

    /// <summary>
    /// Moves the Cacatac along its patrol by its speed, then turns it once it passes the left limit or reaches or passes
    /// the right one.
    /// </summary>
    private static void MoveCacatac(CacatacSpawnSettings cacatacSpawnSettings, RoomEnemyPlace place, CacatacEnemyState state)
    {
        bool right = state.PatrolDirection == HorizontalDirection.Right;
        SignedSixteenSixteen speed = cacatacSpawnSettings.Speed;
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

    /// <summary>The random attack gate (<see cref="CacatacAttackDefinitions.Attacks"/>).</summary>
    private void MaybeStartCacatacAttack(CacatacSpawnSettings cacatacSpawnSettings, RoomEnemyPlace place, CacatacEnemyState state)
    {
        if (!CacatacAttackDefinitions.Attacks.IsTakenBy(_room.Random.Generate(), advancedBy: place.FrameCounter))
            return;

        state.Action = CacatacAction.Stopped;
        SetCacatacAnimation(place, state, cacatacSpawnSettings.Mounting == CacatacMounting.Floor ? CacatacSegment.OnFloorAttack : CacatacSegment.OnCeilingAttack);
    }

    /// <summary>Animation cue: restore patrol the way it last patrolled.</summary>
    private static void RestoreCacatacPatrol(CacatacEnemyState state) => state.Action = CacatacAction.Patrolling;

    /// <summary>Animation cue: publish the spike sound.</summary>
    private void PlayCacatacSpikeSound() =>
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.CacatacSpikeSound, priority: SoundRequestPriority.Normal);

    /// <summary>Starts a patrol or attack animation on the next animation tick.</summary>
    private static void SetCacatacAnimation(RoomEnemyPlace place, CacatacEnemyState state, CacatacSegment segment)
    {
        state.Playback = EnemyAnimationPlayback.Start(place, CacatacAnimations.StartingWith(segment));
    }

    /// <summary>A Cacatac spike (#1760): it flies by its direction's velocity until it leaves the camera.</summary>
    private static readonly EnemyProjectileBehavior Spike = new SingleMotionProjectileBehavior(
        context => FlyCacatacSpike(context.Projectile, context.Camera));

    /// <summary>Runs Cacatac's main AI.</summary>
    internal void RunCacatacMainAi(CacatacSpawnSettings cacatacSpawnSettings, RoomEnemyPlace place, CacatacEnemyState cacatacEnemyState)
    {
        RunCacatacMain(cacatacSpawnSettings, place, cacatacEnemyState);
    }

    /// <summary>Advances Cacatac's typed animation.</summary>
    internal void AdvanceCacatacTypedAnimation(RoomEnemyOccupant occupant, RoomEnemyPlace place, CacatacEnemyState cacatacEnemyState) =>
        EnemyAnimationPlayback.AdvanceApplyingCues(place, cacatacEnemyState, CacatacAnimations.Segments,
            static (CacatacEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (CacatacEnemyState state, CacatacCue cue) => ApplyCacatacCue(occupant, place, state, cue));

    /// <summary>
    /// Spawns a spike projectile from the Cacatac and initializes it.
    /// The source actor supplies the origin's fractions of a pixel and the sheet and colours its spikes draw in.
    /// </summary>
    private void SpawnCacatacSpike(RoomEnemyOccupant occupant, RoomEnemyPlace source, CacatacSpikeDirection direction)
    {
        EnemyProjectile? projectile = _projectiles.AllocateEnemyProjectile(RoomEnemyProjectileKind.CacatacSpike, Spike, SpriteGraphics.SpawnedBy(occupant, source.Shading), new CacatacProjectileAnimation(direction));
        if (projectile is null)
            return;

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
