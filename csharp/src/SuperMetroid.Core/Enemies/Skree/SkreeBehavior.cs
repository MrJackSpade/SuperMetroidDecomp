using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The Skree species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class SkreeBehavior : IEnemySpecies, IEnemySpawnFactory<EnemySpawnSettings>
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Skree];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyOccupant occupant, EnemySpawnSettings settings, RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Running(new SkreeEntity(this, occupant, place, common));

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
    /// <returns>The state the enemy holds from its setup.</returns>
    internal static SkreeEnemyState InitializeSkree()
    {
        var state = new SkreeEnemyState(SkreeAction.Idling);
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.SkreeIdling), AnimationPlace.StartOfAnimation);
        return state;
    }

    /// <summary>Winds up near Samus, dives when the wind-up cue is ready, then burrows after floor contact.</summary>
    private void RunSkreeMain(RoomEnemyOccupant occupant, RoomEnemyPlace place, SkreeEnemyState state, SamusState samus, RoomLevelData level)
    {
        switch (state.Action)
        {
            case SkreeAction.Idling:
                if (Math.Abs(place.Position.X.Whole - samus.Position.X.Whole) < SkreeMotionDefinitions.WindUpDistance)
                {
                    StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreePreparingAttack);
                    state.Action = SkreeAction.PreparingAttack;
                }
                return;

            case SkreeAction.PreparingAttack:
                return;

            case SkreeAction.AttackReady:
                StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreeDiving);
                state.Action = SkreeAction.Diving;
                // The dive screeches on the update the wind-up releases it.
                _audio.QueueEnemySound(EnemySound.DiveScreech, priority: SoundRequestPriority.Normal);
                return;

            case SkreeAction.Diving:
                RunSkreeDive(place, state, samus, level);
                return;

            case SkreeAction.Burrowing:
                RunSkreeBurrow(occupant, place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Skree has no behaviour for action {state.Action}.");
        }
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
    private void RunSkreeBurrow(RoomEnemyOccupant occupant, RoomEnemyPlace place, SkreeEnemyState state)
    {
        if (state.BurrowTimer.StepEnds())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer.UpdatesLeft == SkreeMotionDefinitions.DebrisBurstAt)
            SkreeMetareeParticles.SpawnBurst(_projectiles, occupant, place, SkreeMetareeParticleSegment.Skree);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + SkreeMotionDefinitions.BurrowSink);
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    private static void StartSkreeAnimation(
        RoomEnemyPlace place,
        SkreeEnemyState state,
        SkreeMetareeSegment animation) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, SkreeMetareeAnimations.StartingWith(animation));

    /// <summary>Runs Skree's main AI.</summary>
    internal void RunSkreeMainAi(RoomEnemyOccupant occupant, RoomEnemyPlace place, SkreeEnemyState skreeEnemyState, IEnemyAiFrame frame)
    {
        RunSkreeMain(occupant, place, skreeEnemyState, frame.Samus, frame.Level);
    }

    /// <summary>Advances Skree's typed animation.</summary>
    internal static void AdvanceSkreeTypedAnimation(RoomEnemyPlace place, SkreeEnemyState skreeEnemyState) =>
        EnemyAnimationPlayback.Advance(place, skreeEnemyState, SkreeMetareeAnimations.Segments,
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
    /// <param name="skree">The Skree the reaction is built for, whose sheet the objects it spawns draw from.</param>
    internal EnemyShotReaction SkreeShotReaction(RoomEnemyOccupant skree) => new()
    {
        // Skree's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        BeforeGenericDeath = c => SkreeMetareeParticles.SpawnBurst(_projectiles, skree, c.Enemy, SkreeMetareeParticleSegment.Skree),
    };

    /// <summary>Handles lethal bombs with Skree's particle burst and its ordinary death after the shared hit path.</summary>
    /// <remarks>Skree's bomb reaction spawns its four-piece burst and starts its death itself, after the common bomb damage,
    /// which starts no death for it. A normal bomb gives the ordinary death; only missiles give another.</remarks>
    /// <param name="skree">The Skree the reaction is built for, whose sheet the objects it spawns draw from.</param>
    internal EnemyBombReaction SkreeBombReaction(RoomEnemyOccupant skree) => new()
    {
        RunsGenericDeath = false,
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (c.Enemy.Health != 0)
                return;
            SkreeMetareeParticles.SpawnBurst(_projectiles, skree, c.Enemy, SkreeMetareeParticleSegment.Skree);
            _deathsAndDrops.StartGenericEnemyDeath(c.Occupant, EnemyDeathExplosion.Small, c.Samus);
        },
    };
}
