using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.FallingSparks;

namespace SuperMetroid.Core.Enemies.Sparks;

/// <summary>The Spark species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SparkBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <summary>The pass-through shot reaction for this species' private shot reaction.</summary>
    internal static EnemyShotReaction PassThroughShot { get; } =
        new()
        {
            AfterSuperMissileQuake = ProjectilePassThrough.Shot,
        };

    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Spark], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new SparkEntity(this, place, common));

    /// <summary>Owner of the falling-spark projectile emitted by one spark action.</summary>
    private FallingSparksBehavior FallingSparks { get; }

    /// <summary>Binds the spark species to its falling-spark producer.</summary>
    internal SparkBehavior(FallingSparksBehavior fallingSparksBehavior) => FallingSparks = fallingSparksBehavior;

    /// <summary>What <c>ApplySparkCue</c> does for each <see cref="SparkCueKind"/> member, each member's own arm.</summary>
    private static IdentityTable<SparkCueKind, Action<RoomEnemyPlace>> ApplySparkCueForSparkCueKind => field ??= new(new Dictionary<SparkCueKind, Action<RoomEnemyPlace>>
    {
        [SparkCueKind.BecomeIntangible] = static place =>
        {
            // Flicker-out: the spark stops touching Samus and passes her beams and the grapple.
            place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
        },
        [SparkCueKind.BecomeTangible] = static place =>
        {
            // Flicker-on runs this before the first visible activation frame, so collision and art become live
            // together.
            place.Contact = place.Contact.Without(EnemyContact.IgnoresSamus);
        },
    });

    /// <summary>Carries out one cue of Spark's animations.</summary>
    private static void ApplySparkCue(RoomEnemyPlace place, SparkCue cue)
    {
        ApplySparkCueForSparkCueKind[cue.Kind](place);
    }

    /// <summary>Initializes the Spark.</summary>
    internal static void InitializeSpark(RoomEnemyPlace place)
    {
        SparkMovementDefinition definition = place.SpawnSettings<SparkSpawnSettings>().Start;
        var state = new SparkEnemyState(definition.Action,
            new(SparkAnimations.StartingWith(definition.Animation), AnimationPlace.StartOfAnimation));
        place.SetSpeciesState(state);

        SetSparkActionTimer(place, state, additionalTime: 0);

        place.AnimationClock.StepOnNextUpdate();
        // Spark keeps its population properties: the setup's extra property change before Phantoon's
        // defeat is exempt under the emulation scope limits (owner decision on #1318).
    }

    /// <summary>What <c>RunSparkMain</c> does for each <see cref="SparkAction"/> member, each member's own arm.</summary>
    private IdentityTable<SparkAction, Action<RoomEnemyPlace, SparkEnemyState>> RunSparkMainForSparkAction => field ??= new(new Dictionary<SparkAction, Action<RoomEnemyPlace, SparkEnemyState>>
    {
        [SparkAction.AlwaysActive] = (place, state) => { },
        [SparkAction.IntermittentInactive] = (place, state) =>
        {
            if (!DecrementSparkTimerAndTestExpired(state))
                return;

            state.Action = SparkAction.IntermittentActive;
            StartSparkAnimation(place, state, SparkSegment.FlickerOn);
            SetSparkActionTimer(place, state, additionalTime: 0);
        },
        [SparkAction.IntermittentActive] = (place, state) =>
        {
            if (!DecrementSparkTimerAndTestExpired(state))
                return;

            state.Action = SparkAction.IntermittentInactive;
            StartSparkAnimation(place, state, SparkSegment.FlickerOut);
            SetSparkActionTimer(place, state, additionalTime: SparkTimingDefinitions.ExtraTimeOut);
        },
        [SparkAction.EmitFallingSparks] = (place, state) =>
        {
            if (!DecrementSparkTimerAndTestExpired(state))
                return;

            FallingSparks.SpawnFallingSpark(place);
            SetSparkActionTimer(place, state, additionalTime: 0);
        },
    });

    /// <summary>Runs the main step.</summary>
    private void RunSparkMain(RoomEnemyPlace place, SparkEnemyState state)
    {
        RunSparkMainForSparkAction[state.Action](place, state);
    }

    /// <summary>Counts one update of the Spark's wait: whether it ends on this update, which every action follows with a new wait.</summary>
    /// <exception cref="InvalidOperationException">The wait is already over: a counting Spark's base time is zero.</exception>
    private static bool DecrementSparkTimerAndTestExpired(SparkEnemyState state) => state.ActionTimer.Count();

    /// <summary>
    /// Sets the Spark's action timer: its placement's wait between actions (#2450) and <paramref name="additionalTime"/>.
    /// </summary>
    private static void SetSparkActionTimer(RoomEnemyPlace place, SparkEnemyState state, int additionalTime)
    {
        state.ActionTimer = UpdateCountdown.For(place.SpawnSettings<SparkSpawnSettings>().BaseActionTime + additionalTime);
    }

    /// <summary>Selects an animation and makes its first step due on the next pass.</summary>
    private static void StartSparkAnimation(RoomEnemyPlace place, SparkEnemyState state, SparkSegment segment)
    {
        state.Playback = new(SparkAnimations.StartingWith(segment), AnimationPlace.StartOfAnimation);
        place.AnimationClock.StepOnNextUpdate();
    }

    /// <summary>Runs Spark's main AI.</summary>
    internal void RunSparkMainAi(RoomEnemyPlace place)
    {
        RunSparkMain(place, place.RequireSpeciesState<SparkEnemyState>());
    }

    /// <summary>Advances Spark's typed animation.</summary>
    internal static void AdvanceSparkTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        SparkEnemyState state = place.RequireSpeciesState<SparkEnemyState>();
        AnimationPlayback<SparkSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, SparkAnimations.Segments, ref playback,
            cue =>
            {
                ApplySparkCue(place, cue);
                return null;
            });
        state.Playback = playback;
    }
}
