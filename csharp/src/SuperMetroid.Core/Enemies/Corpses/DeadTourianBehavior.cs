using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed partial class DeadTourianBehavior : IEnemySpecies, IEnemySpawnFactory<CorpseSpawnSettings>
{

    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DeadTourianCorpseSpeciesDefinitions.Species.Keys];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, CorpseSpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new DeadTourianEntity(this, occupant, place, common, settings));

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
    internal static DeadTourianCorpseSpecies SpeciesOf(EnemyDefinitionId definitionId) =>
        DeadTourianCorpseSpeciesDefinitions.Species.TryGetValue(definitionId, out DeadTourianCorpseSpecies species)
            ? species
            : throw new ArgumentOutOfRangeException(nameof(definitionId));

    /// <summary>Initializes a Tourian corpse, checking its corpse is one of its species' before registering state and starting its rot.</summary>
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static DeadTourianCorpseEnemyState InitializeDeadTourianCorpse(RoomEnemyPlace place, RoomEnemyOccupant occupant, CorpseSpawnSettings corpseSpawnSettings)
    {
        DeadTourianCorpseSpecies species = SpeciesOf(Ensure.NotNull(occupant).DefinitionId);
        DeadTourianCorpseDefinition definition = corpseSpawnSettings.Corpse;
        if (definition.Species != species)
        {
            throw new InvalidDataException(
                $"Dead {species} placement names a corpse that is not one of its species'.");
        }
        var state = new DeadTourianCorpseEnemyState(new CorpseRotSchedule(definition.RowCount), CorpsePhase.Waiting);

        place.Shading = TourianCorpseShades.InDrainedColours;
        state.Playback = DeadSidehopperBehavior.StartDeadTourianAnimation(place, definition.InitialAnimation);
        return state;
    }

    /// <summary>What <see cref="RunDeadTourianCorpseMain"/> does for each <see cref="CorpsePhase"/> member, each member's own arm.</summary>
    [field: NonSerialized]
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
    private void RunDeadTourianCorpseMain(RoomEnemyPlace place, DeadTourianCorpseEnemyState state, SamusState samus)
    {
        RunDeadTourianCorpseMainForDeadTourianCorpsePhase[state.Phase](place, samus, state);
    }

    /// <summary>Touch and shot reactions for these three corpse-only actors are identical.</summary>
    internal static void TriggerDeadTourianCorpseRotting(RoomEnemyPlace place, DeadTourianCorpseEnemyState state)
    {
        state.Phase = CorpsePhase.Rotting;
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Power-bomb reactions reject actors whose decomposition already made them intangible.</summary>
    internal static void ResolveDeadTourianCorpsePowerBomb(RoomEnemyPlace place, DeadTourianCorpseEnemyState deadTourianCorpseEnemyState)
    {
        if (!place.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTourianCorpseRotting(place, deadTourianCorpseEnemyState);
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
    internal static CorpseRotRows ShownDeadTourianRows(DeadTourianCorpseEnemyState deadTourianCorpseEnemyState) => deadTourianCorpseEnemyState.Rows.Shown;

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(RoomEnemyPlace place) =>
        _rowDust.Spawn(place, _room.Random.Read(), _projectiles, _frameContext.EnemyPassCount);

    /// <summary>Routes bomb hits through the shared shot reaction, which begins decomposition.</summary>
    internal static void ReplaceDeadTourianBombDamage(EnemyBombContext c, DeadTourianCorpseEnemyState deadTourianCorpseEnemyState) =>
        TriggerDeadTourianCorpseRotting(c.Enemy, deadTourianCorpseEnemyState);

    /// <summary>Advances the Tourian corpses' typed animation.</summary>
    internal static void AdvanceDeadTourianTypedAnimation(RoomEnemyPlace place, DeadTourianCorpseEnemyState state)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, DeadTourianAnimations.Segments, ref playback,
            cue => throw new InvalidDataException($"A corpse pose published {cue.Kind}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Tourian corpses' main AI.</summary>
    internal void RunDeadTourianMainAi(RoomEnemyPlace place, DeadTourianCorpseEnemyState deadTourianCorpseEnemyState, IEnemyAiFrame frame)
    {
        RunDeadTourianCorpseMain(place, deadTourianCorpseEnemyState, frame.Samus);
    }

    /// <summary>The power-bomb response, which ignores corpses already marked decomposed.</summary>
    /// <param name="startsRot">The corpse's own method starting its rot.</param>
    internal static EnemyPowerBombReaction DeadTourianPowerBombReaction(EnemyPowerBombStage startsRot) => new()
    {
        ReplacesCommon = startsRot,
    };

    /// <summary>Shot handling that marks the shot's hit and starts decomposition without health damage.</summary>
    /// <remarks>A shot makes the corpse keep updating off screen and lets Samus pass through it, and starts its rot; it
    /// deals no damage and makes no impact.</remarks>
    /// <param name="marksShotAndRots">The corpse's own method marking the shot and starting its rot.</param>
    internal static EnemyShotReaction DeadTourianShotReaction(EnemyShotStage marksShotAndRots) => new()
    {
        BeforeSuperMissileQuake = marksShotAndRots,
    };

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal static void ReactToDeadTourianTouch(EnemyTouchContext context, DeadTourianCorpseEnemyState deadTourianCorpseEnemyState) =>
        // Touching a dead Zoomer, Ripper or Skree starts its rot just as a shot does; a corpse never deals contact damage.
        TriggerDeadTourianCorpseRotting(context.Place, deadTourianCorpseEnemyState);
}
