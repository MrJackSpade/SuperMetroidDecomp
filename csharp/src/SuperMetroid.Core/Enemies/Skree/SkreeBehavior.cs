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
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Skree];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new SkreeEntity(this, place, common);

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
        var state = new SkreeEnemyState(SkreeAction.Idling)
        {
            AttackReady = false,
        };
        place.SetSpeciesState(state);
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.SkreeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Winds up near Samus, dives when the wind-up cue is ready, then burrows after floor contact.</summary>
    internal void RunSkreeMain(RoomEnemyPlace place, SamusState samus, RoomLevelData level)
    {
        SkreeEnemyState state = place.RequireSpeciesState<SkreeEnemyState>();
        switch (state.Action)
        {
            case SkreeAction.Idling:
                if (CoordinateAxis.Distance(place.Position.X.Whole, samus.Position.X.Whole) < SkreeMotionDefinitions.WindUpDistance)
                {
                    StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreePreparingAttack);
                    state.Action = SkreeAction.PreparingAttack;
                }
                return;

            case SkreeAction.PreparingAttack:
                if (!state.AttackReady)
                    return;
                state.AttackReady = false;
                StartSkreeAnimation(place, state, SkreeMetareeSegment.SkreeDiving);
                state.Action = SkreeAction.Diving;
                // The dive queues its dive sound on the exact frame the wind-up
                // cue releases the dive. Publishing it as a frame event retains
                // the game's timing without coupling enemy AI to a particular audio backend.
                _audio.QueueEnemySound(EnemySound.SkreeDive, priority: SoundRequestPriority.Normal);
                return;

            case SkreeAction.Diving:
                RunSkreeDive(place, state, samus, level);
                return;

            case SkreeAction.Burrowing:
                RunSkreeBurrow(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Skree has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Moves six pixels downward and one toward Samus until the downward solid-block probe starts burrowing.</summary>
    private void RunSkreeDive(
        RoomEnemyPlace place,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The Japanese and North American release reloads the burrow duration every airborne frame. It becomes the
        // complete burrow lifetime only on the frame whose downward probe of one dive step collides.
        state.BurrowTimer = SkreeMotionDefinitions.BurrowDuration;
        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(level, place, SignedSixteenSixteen.FromWholePixels(SkreeMotionDefinitions.DiveStep)))
        {
            place.AnimationClock.Restart();
            state.Action = SkreeAction.Burrowing;
            // The floor collision has its own distinct impact/burrow request.
            _audio.QueueEnemySound(EnemySound.SkreeBurrow, priority: SoundRequestPriority.Normal);
            return;
        }

        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + SkreeMotionDefinitions.DiveStep);
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole +
            (CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) >= 0 ? -1 : 1));
    }

    /// <summary>Advances the 21-frame burrow, spawning debris at eight and deleting the place at zero.</summary>
    private void RunSkreeBurrow(RoomEnemyPlace place, SkreeEnemyState state)
    {
        if (--state.BurrowTimer == 0)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer == SkreeMotionDefinitions.DebrisBurstAt)
            SkreeMetareeParticles.SpawnBurst(_projectiles, place, SkreeParticleDefinitions.Burst);
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + 1);
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    internal static void StartSkreeAnimation(
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
    internal static void AdvanceSkreeTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, SkreeMetareeAnimations.Segments,
            static (SkreeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(place, cue, () => state.AttackReady = true);
                return null;
            });


    /// <summary>Spawns Skree's four debris actors before generic death processing of a lethal shot.</summary>
    /// <remarks>The Skree's shot reaction adds its four debris actors after the shared normal shot response reports death, before the
    /// common death animation releases the enemy place.</remarks>
    internal EnemyShotReaction SkreeShotReaction => field ??= new()
    {
        // Skree's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        BeforeGenericDeath = c => SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeParticleDefinitions.Burst),
    };

    /// <summary>Handles lethal bombs with Skree's particle burst and its ordinary death after the shared hit path.</summary>
    /// <remarks>Skree's shot reaction owns its four-piece burst and death animation because it calls the normal enemy shot
    /// AI that skips the death animation. A normal bomb gives the ordinary death; only missiles give another.</remarks>
    internal EnemyBombReaction SkreeBombReaction => field ??= new()
    {
        RunsGenericDeath = false,
        ShowsHitBurst = EnemyBombReaction.NeverShowsHitBurst,
        AfterCommon = c =>
        {
            if (c.Enemy.Health != 0)
                return;
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeParticleDefinitions.Burst);
            _deathsAndDrops.StartGenericEnemyDeath(c.Enemy, deathAnimation: EnemyDeathExplosion.Small);
        },
    };
}
