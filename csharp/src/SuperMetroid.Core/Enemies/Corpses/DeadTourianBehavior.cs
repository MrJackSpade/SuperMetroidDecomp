using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadTourianBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([.. DeadTourianCorpseSpeciesDefinitions.Species.Keys], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="occupant">The occupant the entity is created for, which carries its identity and the entry it is spawned from.</param>
    /// <param name="place">The place, already holding the occupant.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new DeadTourianEntity(this, occupant, place, common));

    /// <summary>Supplies the room RNG used during decomposition.</summary>
    private readonly EnemyRoomBinding _room;
    /// <summary>Supplies the enemy counter used to pace corpse dust sounds.</summary>
    private readonly EnemyFrameContext _frameContext;
    /// <summary>Spawns the dust each fallen row raises.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Raises each fallen row's dust and its sound request.</summary>
    private readonly CorpseRowDust _rowDust;

    /// <summary>Connects corpse AI to the room's random numbers, frame counter, projectiles and row dust.</summary>
    internal DeadTourianBehavior(EnemyRoomBinding room, EnemyFrameContext frameContext, EnemyProjectileEngine projectiles, CorpseRowDust rowDust)
    {
        _rowDust = rowDust;
        _room = room;
        _frameContext = frameContext;
        _projectiles = projectiles;
    }

    /// <summary>The species whose corpse <paramref name="definitionId"/> places.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="definitionId"/> is not a Tourian corpse.</exception>
    private static DeadTourianCorpseSpecies SpeciesOf(EnemyDefinitionId definitionId) =>
        DeadTourianCorpseSpeciesDefinitions.Species.TryGetValue(definitionId, out DeadTourianCorpseSpecies species)
            ? species
            : throw new ArgumentOutOfRangeException(nameof(definitionId));

    /// <summary>Initializes a Tourian corpse, checking its corpse is one of its species' before registering state and starting its rot.</summary>
    internal static void InitializeDeadTourianCorpse(RoomEnemyPlace place, RoomEnemyOccupant occupant)
    {
        DeadTourianCorpseSpecies species = SpeciesOf(Ensure.NotNull(occupant).DefinitionId);
        DeadTourianCorpseDefinition definition = occupant.SpawnSettings<CorpseSpawnSettings>().Corpse;
        if (definition.Species != species)
        {
            throw new InvalidDataException(
                $"Dead {species} placement names a corpse that is not one of its species'.");
        }
        var state = new DeadTourianCorpseEnemyState(new CorpseRotSchedule(definition.RowCount), CorpsePhase.Waiting);
        place.SetSpeciesState(state);

        place.Shading = TourianCorpseShades.InDrainedColours;
        state.Playback = DeadSidehopperBehavior.StartDeadTourianAnimation(place, definition.InitialAnimation);
    }

    /// <summary>What <see cref="RunDeadTourianCorpseMain"/> does for each <see cref="CorpsePhase"/> member, each member's own arm.</summary>
    private IdentityTable<CorpsePhase, Action<RoomEnemyPlace, SamusState, DeadTourianCorpseEnemyState>> RunDeadTourianCorpseMainForDeadTourianCorpsePhase => field ??= new(new Dictionary<CorpsePhase, Action<RoomEnemyPlace, SamusState, DeadTourianCorpseEnemyState>>
    {
        [CorpsePhase.Waiting] = WaitToRot,
        [CorpsePhase.PreRotDelay] = WaitToRot,
        [CorpsePhase.Rotting] = (place, samus, state) => RunDeadTourianCorpseRotting(place, state),
        [CorpsePhase.Finished] = (place, samus, state) => { },
    });

    /// <summary>Waits for Samus's touch and then the delay before the corpse rots.</summary>
    private static void WaitToRot(RoomEnemyPlace place, SamusState samus, DeadTourianCorpseEnemyState state) =>
        state.Phase = CorpseRotWait.Step(state.Phase, place, samus, state.PreRotDelay);

    /// <summary>Runs the shared corpse behaviour: Samus touching it starts its wait before rotting (<see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/>), then the corpse rots until its last row has fallen.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemyPlace place, SamusState samus)
    {
        DeadTourianCorpseEnemyState state = place.RequireSpeciesState<DeadTourianCorpseEnemyState>();
        RunDeadTourianCorpseMainForDeadTourianCorpsePhase[state.Phase](place, samus, state);
    }

    /// <summary>Touch and shot reactions for these three corpse-only actors are identical.</summary>
    internal static void TriggerDeadTourianCorpseRotting(RoomEnemyPlace place)
    {
        DeadTourianCorpseEnemyState state = place.RequireSpeciesState<DeadTourianCorpseEnemyState>();
        state.Phase = CorpsePhase.Rotting;
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Power-bomb reactions reject actors whose decomposition already made them intangible.</summary>
    private static void ResolveDeadTourianCorpsePowerBomb(RoomEnemyPlace place)
    {
        if (!place.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTourianCorpseRotting(place);
    }

    /// <summary>
    /// Runs one rot update, moving the corpse's own rows as the update moves them (#1539), entering inert AI when it completes.
    /// </summary>
    private void RunDeadTourianCorpseRotting(
        RoomEnemyPlace place,
        DeadTourianCorpseEnemyState state)
    {
        bool stillRotting = state.Rot.Step(
            state.Rows.MoveRow,
            _ => FinishDeadTourianCorpseRow(place));
        if (!stillRotting)
            state.Phase = CorpsePhase.Finished;
    }

    /// <summary>The rows <paramref name="place"/>'s frames show: its corpse's own, as its rot has left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadTourianRows(RoomEnemyPlace place) => place.RequireSpeciesState<DeadTourianCorpseEnemyState>().Rows.Shown;

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(RoomEnemyPlace place) =>
        _rowDust.Spawn(place, _room.Random.Read(), _projectiles, _frameContext.EnemyPassCount);

    /// <summary>Routes bomb hits through the shared shot reaction, which begins decomposition.</summary>
    internal static void ReplaceDeadTourianBombDamage(EnemyBombContext c) =>
        TriggerDeadTourianCorpseRotting(c.Enemy);

    /// <summary>Advances the Tourian corpses' typed animation.</summary>
    internal static void AdvanceDeadTourianTypedAnimation(RoomEnemyPlace place)
    {
        if (!place.AnimationClock.Expire())
            return;
        DeadTourianCorpseEnemyState state = place.RequireSpeciesState<DeadTourianCorpseEnemyState>();
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, DeadTourianAnimations.Segments, ref playback,
            cue => throw new InvalidDataException($"A corpse pose published {cue.Kind}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Tourian corpses' main AI.</summary>
    internal void RunDeadTourianMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunDeadTourianCorpseMain(place, frame.Samus);
    }

    /// <summary>The power-bomb response, which ignores corpses already marked decomposed.</summary>
    internal static EnemyPowerBombReaction DeadTourianPowerBombReaction { get; } = new()
    {
        ReplacesCommon = c => ResolveDeadTourianCorpsePowerBomb(c.Enemy),
    };

    /// <summary>Shot handling that marks the shot's hit and starts decomposition without health damage.</summary>
    /// <remarks>A shot makes the corpse keep updating off screen and lets Samus pass through it, and starts its rot; it
    /// deals no damage and makes no impact.</remarks>
    internal static EnemyShotReaction DeadTourianShotReaction { get; } = new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            TriggerDeadTourianCorpseRotting(c.Enemy);
            return true;
        },
    };

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal static void ReactToDeadTourianTouch(EnemyTouchContext context) =>
        // Touching a dead Zoomer, Ripper or Skree starts its rot just as a shot does; a corpse never deals contact damage.
        TriggerDeadTourianCorpseRotting(context.Place);
}
