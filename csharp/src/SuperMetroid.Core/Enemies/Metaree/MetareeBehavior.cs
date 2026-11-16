using SuperMetroid.Core.Compatibility;
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
    public IEnemyEntity Create(RoomEnemyPlace place, EnemyCommonParticipation common) => new MetareeEntity(this, place, common);

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

    /// <summary>Maximum strict horizontal separation for starting a dive, regardless of Samus's vertical side.</summary>
    private const int MetareeHorizontalActivationDistance = 72;
    /// <summary>Whole-pixel X steering toward Samus, reevaluated every airborne frame.</summary>
    private const int MetareeHorizontalDiveSpeed = 2;
    /// <summary>Ground-impact countdown before the actor deletes itself.</summary>
    private const int MetareeBurrowLifetime = 21;
    /// <summary>Burrow countdown value that spawns the particle burst.</summary>
    private const int MetareeParticleFrame = 8;

    /// <summary>Starts the idle animation directly and initializes the place's dive, burrow, and animation-change state.</summary>
    internal static void InitializeMetaree(RoomEnemyPlace place)
    {
        var state = new MetareeEnemyState(MetareeAction.Idling)
        {
            BurrowTimer = 0,
            InstalledAnimation = SkreeMetareeSegment.MetareeIdling,
            AttackReady = false,
            YVelocity = 0,
        };
        place.SetSpeciesState(state);

        // The initializer starts the idle animation directly rather than through StartMetareeAnimation, which would
        // leave the idle animation it records as installed unstarted.
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
                RunMetareePreparation(place, state);
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

    /// <summary>Starts preparation within 72 horizontal pixels and takes the dive's speed from Samus's height (<see cref="MetareeDiveSpeed"/>).</summary>
    internal static void RunMetareeIdle(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SamusState samus)
    {
        // The proximity test computes a wrapped signed difference, takes its signed
        // absolute value, and compares it as a distance that is never negative. It notably does *not* check that
        // Samus is below the actor; preserving that retail bug matters to the velocity math.
        int horizontalDistance = Math.Abs(
            CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole));
        if (horizontalDistance >= MetareeHorizontalActivationDistance)
            return;

        state.YVelocity = MetareeDiveSpeed.Of(place.Position.Y.Whole, samus.Position.Y.Whole);
        StartMetareeAnimation(place, state, SkreeMetareeSegment.MetareePreparingAttack);
        state.Action = MetareeAction.PreparingAttack;
    }

    /// <summary>Consumes the preparation animation's ready flag, starts the dive animation, and publishes the dive sound.</summary>
    internal void RunMetareePreparation(RoomEnemyPlace place, MetareeEnemyState state)
    {
        if (!state.AttackReady)
            return;

        state.AttackReady = false;
        StartMetareeAnimation(place, state, SkreeMetareeSegment.MetareeDiving);
        state.Action = MetareeAction.LaunchedAttack;

        // The dive sound is requested as the attack launches.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DiveSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Probes solid terrain ahead before whole-pixel descent; ground contact begins burrowing while airborne frames steer toward Samus.</summary>
    private void RunMetareeDive(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The game reloads this every airborne frame. It becomes a countdown only after
        // the attachment probe reports the ground, which is why interrupted dives never age.
        state.BurrowTimer = MetareeBurrowLifetime;

        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(
                level,
                place,
                SignedSixteenSixteen.FromWholePixels(state.YVelocity)))
        {
            place.AnimationClock.Restart();
            state.Action = MetareeAction.Burrowing;
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BurrowSound, priority: SoundRequestPriority.Normal);
            return;
        }

        // Unlike common movement, Metaree applies whole-pixel speeds directly and leaves
        // both subpositions untouched. Horizontal steering re-evaluates Samus every frame.
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + state.YVelocity);
        bool metareeIsLeftOfSamus =
            CoordinateAxis.Difference(place.Position.X.Whole, samus.Position.X.Whole) < 0;
        place.Position = place.Position.WithWrappedWholeX(place.Position.X.Whole +
            (metareeIsLeftOfSamus
                ? MetareeHorizontalDiveSpeed
                : -MetareeHorizontalDiveSpeed));
    }

    /// <summary>Moves down one pixel per burrow frame, emits debris at timer eight, then deletes the actor.</summary>
    private void RunMetareeBurrow(RoomEnemyPlace place, MetareeEnemyState state)
    {
        if (--state.BurrowTimer == 0)
        {
            place.Presence = place.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer == MetareeParticleFrame)
            SkreeMetareeParticles.SpawnBurst(_projectiles, place, MetareeParticleDefinitions.Burst);
        place.Position = place.Position.WithWrappedWholeY(place.Position.Y.Whole + 1);
    }

    /// <summary>Starts <paramref name="animation"/> unless it is the one already installed, which keeps playing.</summary>
    internal static void StartMetareeAnimation(
        RoomEnemyPlace place,
        MetareeEnemyState state,
        SkreeMetareeSegment animation)
    {
        if (animation == state.InstalledAnimation)
            return;

        state.InstalledAnimation = animation;
        state.Playback = EnemyAnimationPlayback.Start(place, SkreeMetareeAnimations.StartingWith(animation));
    }

    /// <summary>Runs Metaree's main AI.</summary>
    internal void RunMetareeMainAi(RoomEnemyPlace place, IEnemyAiFrame frame)
    {
        RunMetareeMain(place, place.RequireSpeciesState<MetareeEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Metaree's typed animation.</summary>
    internal static void AdvanceMetareeTypedAnimation(RoomEnemyPlace place, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(place, SkreeMetareeAnimations.Segments,
            static (MetareeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(place, cue, () => state.AttackReady = true);
                return null;
            });

    /// <summary>Spawns four metal debris actors in the dying Metaree's colours before common shot death clears them.</summary>
    internal EnemyShotReaction MetareeShotReaction => field ??= new()
    {
        BeforeGenericDeath = c =>
        {
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, MetareeParticleDefinitions.Burst);
            c.Enemy.Shading = PictureShading.Standard;
        },
    };

    /// <summary>After a lethal bomb, spawns debris while the body's graphics state is intact, then clears both indexes on the dead enemy, which keeps its place.</summary>
    internal EnemyBombReaction MetareeBombReaction => field ??= new()
    {
        AfterCommon = c =>
        {
            if (c.Enemy.Health != 0)
                return;
            SkreeMetareeParticles.SpawnBurst(_projectiles, c.Enemy, MetareeParticleDefinitions.Burst);
            c.Enemy.Shading = PictureShading.Standard;
        },
    };
}
