using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The Skree species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SkreeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyDictionary<EnemyDefinitionId, EnemyEntityCreation> Creations => field ??= EnemyEntityCreations.Each([EnemyDefinitionId.Skree], Create);

    /// <summary>Creates the entity of a newly installed occupant of <paramref name="place"/>, with the setup it has.</summary>
    /// <param name="place">The place, holding the new identity and the entry it is spawned from.</param>
    /// <param name="common">The shared participation the entity composes.</param>
    private EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new SkreeEntity(this, place, common));

    /// <summary>Queues the dive and floor-impact sound requests at their AI transitions.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Spawns the four-piece particle burst on burrow and death.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Starts Skree's generic death after a lethal bomb reaction.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Binds Skree AI to enemy audio, particle, and death owners.</summary>
    internal SkreeBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyDeathsAndDrops deathsAndDrops)
    {
        _deathsAndDrops = deathsAndDrops;
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>Starts a Skree in its idle action and initial shared animation.</summary>
    internal static void InitializeSkree(RoomEnemyPlace place)
    {
        var state = new SkreeEnemyState(SkreeAction.Idling);
        place.SetSpeciesState(state);
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.SkreeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>What <c>RunSkreeMain</c> does for each <see cref="SkreeAction"/> member, each member's own arm.</summary>
    private IdentityTable<SkreeAction, Action<RoomEnemyPlace, SamusState, RoomLevelData, SkreeEnemyState>> RunSkreeMainForSkreeAction => field ??= new(new Dictionary<SkreeAction, Action<RoomEnemyPlace, SamusState, RoomLevelData, SkreeEnemyState>>
    {
        [SkreeAction.Idling] = (place, samus, level, state) =>
        {
            if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) < SkreeMotionDefinitions.WindUpDistance)
            {
                StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreePreparingAttack);
                state.Action = SkreeAction.PreparingAttack;
            }
        },
        [SkreeAction.PreparingAttack] = (place, samus, level, state) => { },
        [SkreeAction.AttackReady] = (place, samus, level, state) =>
        {
            StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreeDiving);
            state.Action = SkreeAction.Diving;
            // The dive screeches on the update the wind-up releases it.
            _audio.QueueEnemySound(EnemySound.DiveScreech, priority: SoundRequestPriority.Normal);
        },
        [SkreeAction.Diving] = (place, samus, level, state) => RunSkreeDive(place, state, samus, level),
        [SkreeAction.Burrowing] = (place, samus, level, state) => RunSkreeBurrow(place, state),
    });

    /// <summary>Winds up near Samus, dives when the wind-up cue is ready, then burrows after floor contact.</summary>
    private void RunSkreeMain(RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        SkreeEnemyState state = place.RequireSpeciesState<SkreeEnemyState>();
        RunSkreeMainForSkreeAction[state.Action](place, samus, level, state);
    }

    /// <summary>Moves downward by <see cref="SkreeMotionDefinitions.DiveStep"/> and toward Samus by <see cref="SkreeMotionDefinitions.DiveDrift"/> until the downward solid-block probe starts burrowing.</summary>
    private void RunSkreeDive(
        RoomEnemyPlace place,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(level, place, SignedSixteenSixteen.FromWholePixels(SkreeMotionDefinitions.DiveStep)))
        {
            place.AnimationClock.Restart();
            state.BurrowTimer = UpdateCountdown.For(SkreeMotionDefinitions.BurrowDuration);
            state.Action = SkreeAction.Burrowing;
            // The floor collision has its own distinct impact/burrow request.
            _audio.QueueEnemySound(EnemySound.Burrow, priority: SoundRequestPriority.Normal);
            return;
        }

        // The probe above ends the dive before a step would take the Skree past the room's bottom row, and each step
        // drifts it a pixel toward Samus, so the dive stays in the room.
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + SkreeMotionDefinitions.DiveStep);
        place.Position = place.Position.WithWholeX(place.Position.X.Whole +
            SkreeMotionDefinitions.DiveDrift.Toward(place.Position.X.Whole >= samus.Position.X.Whole ? HorizontalDirection.Left : HorizontalDirection.Right));
    }

    /// <summary>Advances the burrow (<see cref="SkreeMotionDefinitions.BurrowDuration"/>), spawning debris at <see cref="SkreeMotionDefinitions.DebrisBurstAt"/> and deleting the place at zero.</summary>
    private void RunSkreeBurrow(RoomEnemyPlace place, SkreeEnemyState state)
    {
        if (state.BurrowTimer.StepEnds())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer.UpdatesLeft == SkreeMotionDefinitions.DebrisBurstAt)
            SkreeMetareeParticles.SpawnBurst(_projectiles, place, SkreeMetareeParticleSegment.Skree);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + SkreeMotionDefinitions.BurrowSink);
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    private static void StartSkreeAnimation(
        RoomEnemyPlace place,
        SkreeEnemyState state,
        SkreeMetareeSegment animation) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, SkreeMetareeAnimations.StartingWith(animation));

    /// <summary>Runs Skree's main AI.</summary>
    internal void RunSkreeMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunSkreeMain(place, frame.Samus, frame.Level);
    }

    /// <summary>Advances Skree's typed animation.</summary>
    internal static void AdvanceSkreeTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, SkreeMetareeAnimations.Segments,
            static (SkreeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(place, cue, () => ReadySkreeAttack(state));
                return null;
            });

    /// <summary>
    /// The wind-up's finishing cue (#2450): the dive is ready, and the Skree starts it on its next update.
    /// </summary>
    /// <exception cref="InvalidOperationException">The Skree is not winding up.</exception>
    private static void ReadySkreeAttack(SkreeEnemyState state)
    {
        if (state.Action != SkreeAction.PreparingAttack)
            throw new InvalidOperationException($"A Skree's dive was readied while it was {state.Action}.");
        state.Action = SkreeAction.AttackReady;
    }

    /// <summary>Spawns Skree's four debris actors before generic death processing of a lethal shot.</summary>
    /// <remarks>The Skree's shot reaction adds its four debris actors after the shared normal shot response reports death, before the
    /// common death animation releases the enemy place.</remarks>
    internal EnemyShotReaction SkreeShotReaction => field ??= new()
    {
        // Skree's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        BeforeGenericDeath = c => SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeMetareeParticleSegment.Skree),
    };

    /// <summary>Handles lethal bombs with Skree's particle burst and its ordinary death after the shared hit path.</summary>
    /// <remarks>Skree's bomb reaction spawns its four-piece burst and starts its death itself, after the common bomb damage,
    /// which starts no death for it. A normal bomb gives the ordinary death; only missiles give another.</remarks>
    internal EnemyBombReaction SkreeBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (c.Enemy.Health != 0)
                return;
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeMetareeParticleSegment.Skree);
            _deathsAndDrops.StartGenericEnemyDeath(c.Enemy, EnemyDeathExplosion.Small, c.Samus);
        },
    };
}
