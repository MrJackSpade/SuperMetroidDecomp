using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Animation;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Enemies.Skree;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Enemies.Metaree;

/// <summary>The Metaree species owner: its per-room state, behaviour and the interactions it registers.</summary>
[RoomLifetime]
internal sealed class MetareeBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Metaree];

    /// <inheritdoc/>
    public EnemyOccupant Create(RoomEnemyPlace place, EnemyCommonParticipation common) => EnemyOccupant.Of(new MetareeEntity(this, place, common));

    /// <summary>Creates the debris burst during burrow and death reactions.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Receives the species' sound requests, each queued as the species makes it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Uses the room's projectile engine for Metaree's four-piece metal debris.</summary>
    internal MetareeBehavior(EnemyProjectileEngine projectiles, EnemyAudioQueue audio)
    {
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>Starts the idle animation directly and initializes the place's dive, burrow, and animation-change state.</summary>
    internal static void InitializeMetaree(RoomEnemyPlace place)
    {
        var state = new MetareeEnemyState(MetareeAction.Idling);
        place.SetSpeciesState(state);

        // The idle's first frame advances on the update the spawn's frame time runs out.
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.MetareeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Runs the idle, attack preparation, airborne dive or burrow phase with required Samus and room data.</summary>
    private void RunMetareeMain(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        switch (state.Action)
        {
            case MetareeAction.Idling:
                RunMetareeIdle(place, state, samus);
                return;

            case MetareeAction.PreparingAttack:
                return;

            case MetareeAction.AttackReady:
                LaunchMetareeAttack(place, state);
                return;

            case MetareeAction.LaunchedAttack:
                RunMetareeDive(place, state, samus, level);
                return;

            case MetareeAction.Burrowing:
                RunMetareeBurrow(place, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Metaree has no behaviour for action {state.Action}.");
        }
    }

    /// <summary>Starts preparation within its activation distance across and takes the dive's speed from Samus's height (<see cref="MetareeDiveSpeed"/>).</summary>
    private static void RunMetareeIdle(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SamusState samus)
    {
        // The Metaree dives once Samus is within its reach either side, whether or not she is below it; its dive
        // speed depends on her distance either way.
        int horizontalDistance = Math.Abs(place.Position.X.Whole - samus.Position.X.Whole);
        if (horizontalDistance >= MetareeMotionDefinitions.ActivationDistance)
            return;

        state.YVelocity = MetareeDiveSpeed.Of(place.Position.Y.Whole, samus.Position.Y.Whole);
        StartMetareeAnimation(place, state, SkreeMetareeSegment.MetareePreparingAttack);
        state.Action = MetareeAction.PreparingAttack;
    }

    /// <summary>Launches the attack its preparation readied: starts the dive animation and publishes the dive sound.</summary>
    private void LaunchMetareeAttack(RoomEnemyPlace place, MetareeEnemyState state)
    {
        StartMetareeAnimation(place, state, SkreeMetareeSegment.MetareeDiving);
        state.Action = MetareeAction.LaunchedAttack;

        // The dive sound is requested as the attack launches.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DiveSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Dives at its whole-pixel speed while steering toward Samus, until the block below stops it and it starts burrowing.</summary>
    private void RunMetareeDive(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(
                level,
                place,
                SignedSixteenSixteen.FromWholePixels(state.YVelocity)))
        {
            place.AnimationClock.Restart();
            state.BurrowTimer = UpdateCountdown.For(MetareeMotionDefinitions.BurrowLifetime);
            state.Action = MetareeAction.Burrowing;
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BurrowSound, priority: SoundRequestPriority.Normal);
            return;
        }

        // The dive moves Metaree by whole pixels, so where it lies within a pixel never changes. Even the fastest dive,
        // when Samus lies above it, stays in the room: the probe above ends the dive before a step would take the Metaree
        // past the room's bottom row. It steers towards Samus anew every update.
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + state.YVelocity);
        bool metareeIsLeftOfSamus = place.Position.X.Whole < samus.Position.X.Whole;
        place.Position = place.Position.WithWholeX(place.Position.X.Whole +
            MetareeMotionDefinitions.DiveSteer.Toward(metareeIsLeftOfSamus ? HorizontalDirection.Right : HorizontalDirection.Left));
    }

    /// <summary>Moves down by <see cref="MetareeMotionDefinitions.BurrowSpeed"/> each burrow frame, emits debris at <see cref="MetareeMotionDefinitions.BurstCountdown"/>, then deletes the actor.</summary>
    private void RunMetareeBurrow(RoomEnemyPlace place, MetareeEnemyState state)
    {
        if (state.BurrowTimer.StepEnds())
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer.UpdatesLeft == MetareeMotionDefinitions.BurstCountdown)
            SkreeMetareeParticles.SpawnBurst(_projectiles, place, SkreeMetareeParticleSegment.Metaree);
        place.Position = place.Position.WithWholeY(place.Position.Y.Whole + MetareeMotionDefinitions.BurrowSpeed);
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one playing, which keeps playing (#2145).</summary>
    private static void StartMetareeAnimation(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SkreeMetareeSegment animation) =>
        state.Playback = EnemyAnimationPlayback.StartUnlessPlaying(place, state.Playback, SkreeMetareeAnimations.StartingWith(animation));

    /// <summary>Runs Metaree's main AI.</summary>
    internal void RunMetareeMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMetareeMain(place, place.RequireSpeciesState<MetareeEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Metaree's typed animation.</summary>
    internal static void AdvanceMetareeTypedAnimation(RoomEnemyPlace place) =>
        EnemyAnimationPlayback.Advance(place, SkreeMetareeAnimations.Segments,
            static (MetareeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(place, cue, () => ReadyMetareeAttack(state));
                return null;
            });

    /// <summary>
    /// The launch crouch's finishing cue (#2439): the attack is ready, and the Metaree launches it on its next update.
    /// </summary>
    /// <exception cref="InvalidOperationException">The Metaree is not preparing an attack.</exception>
    private static void ReadyMetareeAttack(MetareeEnemyState state)
    {
        if (state.Action != MetareeAction.PreparingAttack)
            throw new InvalidOperationException($"A Metaree's attack was readied while it was {state.Action}.");
        state.Action = MetareeAction.AttackReady;
    }

    /// <summary>Spawns four metal debris actors in the dying Metaree's colours before common shot death clears them.</summary>
    internal EnemyShotReaction MetareeShotReaction => field ??= new()
    {
        BeforeGenericDeath = c =>
        {
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeMetareeParticleSegment.Metaree);
            c.Enemy.Shading = PictureShading.Standard;
        },
    };

    /// <summary>After a lethal bomb, spawns the debris in the dying Metaree's colours, then shows the dead enemy, which keeps its place, in its standard shading.</summary>
    internal EnemyBombReaction MetareeBombReaction => field ??= new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.Health != 0)
                return;
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, SkreeMetareeParticleSegment.Metaree);
            c.Enemy.Shading = PictureShading.Standard;
        },
    };
}
