using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Corpses.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>The dead Tourian corpse species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class DeadTourianBehavior : IEnemySpecies, IEnemySettingsFactory<CorpseSpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [.. DeadTourianCorpseSpeciesDefinitions.Species.Keys];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, RoomEnemyPlace place, EnemyCommonParticipation common, EnemySetup setup, CorpseSpawnSettings settings) => EnemyOccupant.Of(new DeadTourianEntity(this, occupant, place, common, settings));

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

    /// <summary>Recognizes the three Tourian corpse enemy definitions handled by this owner.</summary>
    internal static bool IsDeadTourianCorpseDefinition(EnemyDefinitionId definitionId) =>
        DeadTourianCorpseSpeciesDefinitions.Species.ContainsKey(definitionId);

    /// <summary>The species whose corpse <paramref name="definitionId"/> places.</summary>
    /// <exception cref="ArgumentOutOfRangeException"><paramref name="definitionId"/> is not a Tourian corpse.</exception>
    internal static DeadTourianCorpseSpecies SpeciesOf(EnemyDefinitionId definitionId) =>
        DeadTourianCorpseSpeciesDefinitions.Species.TryGetValue(definitionId, out DeadTourianCorpseSpecies species)
            ? species
            : throw new ArgumentOutOfRangeException(nameof(definitionId));

    /// <summary>Initializes a Tourian corpse, checking its corpse is one of its species' before registering state and starting its rot.</summary>
    internal static DeadTourianCorpseEnemyState InitializeDeadTourianCorpse(RoomEnemyPlace place, CorpseSpawnSettings settings)
    {
        DeadTourianCorpseSpecies species = SpeciesOf(place.DefinitionId);
        DeadTourianCorpseDefinition definition = settings.Corpse;
        if (definition.Species != species)
        {
            throw new InvalidDataException(
                $"Dead {species} placement names a corpse that is not one of its species'.");
        }
        var state = new DeadTourianCorpseEnemyState(new CorpseRotSchedule(definition.RowCount), DeadTourianCorpsePhase.Waiting);

        place.Shading = TourianCorpseShades.InDrainedColours;
        state.Playback = DeadSidehopperBehavior.StartDeadTourianAnimation(place, definition.InitialAnimation);
        return state;
    }

    /// <summary>Runs the shared corpse behaviour: Samus touching it starts its wait before rotting (<see cref="CorpseRotScheduleDefinitions.PreRotDelayUpdates"/>), then the corpse rots until its last row has fallen.</summary>
    private void RunDeadTourianCorpseMain(RoomEnemyPlace place, DeadTourianCorpseEnemyState state, SamusState samus)
    {
        switch (state.Phase)
        {
            case DeadTourianCorpsePhase.Waiting:
                if (samus.Kinematics.DidCollideWithSolidEnemy(place) == true)
                    state.Phase = DeadTourianCorpsePhase.PreRotDelay;
                return;
            case DeadTourianCorpsePhase.PreRotDelay:
                if (state.PreRotDelay.Step())
                {
                    state.Phase = DeadTourianCorpsePhase.Rotting;
                    place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
                }
                return;
            case DeadTourianCorpsePhase.Rotting:
                RunDeadTourianCorpseRotting(place, state);
                return;
            case DeadTourianCorpsePhase.Finished:
                return;
            default:
                throw new InvalidDataException(
                    $"Dead Tourian enemy in place {place.Ordinal} has no behaviour for phase {state.Phase}.");
        }
    }

    /// <summary>Touch and shot reactions for these three corpse-only actors are identical.</summary>
    internal static void TriggerDeadTourianCorpseRotting(RoomEnemyPlace place, DeadTourianCorpseEnemyState state)
    {
        state.Phase = DeadTourianCorpsePhase.Rotting;
        place.Processing = place.Processing.With(EnemyProcessing.OffScreen);
        place.Contact = place.Contact.With(EnemyContact.IgnoresSamus);
    }

    /// <summary>Power-bomb reactions reject actors whose decomposition already made them intangible.</summary>
    private static void ResolveDeadTourianCorpsePowerBomb(RoomEnemyPlace place, DeadTourianCorpseEnemyState state)
    {
        if (!place.Contact.HasAny(EnemyContact.IgnoresSamus))
            TriggerDeadTourianCorpseRotting(place, state);
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
            state.Phase = DeadTourianCorpsePhase.Finished;
    }

    /// <summary>The rows <paramref name="place"/>'s frames show: its corpse's own, as its rot has left them (#1539).</summary>
    internal static CorpseRotRows ShownDeadTourianRows(DeadTourianCorpseEnemyState state) => state.Rows.Shown;

    /// <summary>Spawns randomized dust for a completed rot row and gates its sound on the enemy counter.</summary>
    private void FinishDeadTourianCorpseRow(RoomEnemyPlace place) =>
        _rowDust.Spawn(place, _room.Random.Read(), _projectiles, _frameContext.EnemyPassCount);

    /// <summary>Routes bomb hits through the shared shot reaction, which begins decomposition.</summary>
    internal static void ReplaceDeadTourianBombDamage(EnemyBombContext c, DeadTourianCorpseEnemyState state) =>
        TriggerDeadTourianCorpseRotting(c.Enemy, state);

    /// <summary>Advances the Tourian corpses' typed animation.</summary>
    internal static void AdvanceDeadTourianTypedAnimation(RoomEnemyPlace place, DeadTourianCorpseEnemyState state, EnemyAnimationCueContext _)
    {
        if (!place.AnimationClock.Expire())
            return;
        AnimationPlayback<DeadTourianSegment> playback = state.Playback;
        EnemyAnimationPlayback.Play(place, DeadTourianAnimations.Segments, ref playback,
            cue => throw new InvalidDataException($"A corpse pose published {cue.Kind}."));
        state.Playback = playback;
    }

    /// <summary>Runs the Tourian corpses' main AI.</summary>
    internal void RunDeadTourianMainAi(RoomEnemyPlace place, DeadTourianCorpseEnemyState state, IEnemyAiFrame frame)
    {
        RunDeadTourianCorpseMain(place, state, frame.Samus);
    }

    /// <summary>The power-bomb response, which ignores corpses already marked decomposed.</summary>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal static EnemyPowerBombReaction DeadTourianPowerBombReactionOf(DeadTourianCorpseEnemyState state) => new()
    {
        ReplacesCommon = c => ResolveDeadTourianCorpsePowerBomb(c.Enemy, state),
    };

    /// <summary>Shot handling that marks the shot's hit and starts decomposition without health damage.</summary>
    /// <remarks>A shot makes the corpse keep updating off screen and lets Samus pass through it, and starts its rot; it
    /// deals no damage and makes no impact.</remarks>
    /// <param name="state">The enemy's state, which the reaction acts on, after a hit has killed it too.</param>
    internal static EnemyShotReaction DeadTourianShotReactionOf(DeadTourianCorpseEnemyState state) => new()
    {
        BeforeSuperMissileQuake = (in EnemyShotContext c) =>
        {
            c.Projectiles.EnemyImpacts.TouchEnemyWithoutImpact(c.Shot, c.MarksCollision);
            TriggerDeadTourianCorpseRotting(c.Enemy, state);
            return true;
        },
    };

    /// <summary>Starts decomposition on physical contact without applying ordinary enemy contact damage.</summary>
    internal static void ReactToDeadTourianTouch(EnemyTouchContext context, DeadTourianCorpseEnemyState state) =>
        // Touching a dead Zoomer, Ripper or Skree starts its rot just as a shot does; a corpse never deals contact damage.
        TriggerDeadTourianCorpseRotting(context.Place, state);
}
