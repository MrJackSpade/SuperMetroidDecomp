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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new MetareeEntity(this, slot, common);

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
    /// <summary>Unsigned Y separation divisor used to choose NTSC dive speed.</summary>
    private const int MetareeDiveDivisorNtsc = 24;
    /// <summary>Whole-pixel downward speed added even for zero vertical separation.</summary>
    private const int MetareeMinimumYVelocity = 4;
    /// <summary>Whole-pixel X steering toward Samus, reevaluated every airborne frame.</summary>
    private const int MetareeHorizontalDiveSpeed = 2;
    /// <summary>Ground-impact countdown before the actor deletes itself.</summary>
    private const int MetareeBurrowLifetimeNtsc = 21;
    /// <summary>Burrow countdown value that spawns the particle burst.</summary>
    private const int MetareeParticleFrameNtsc = 8;

    /// <summary>Starts the idle animation directly and initializes the slot's dive, burrow, and animation-change state.</summary>
    internal static void InitializeMetaree(RoomEnemySlot slot)
    {
        var state = new MetareeEnemyState
        {
            BurrowTimer = 0,
            Action = MetareeAction.Idling,
            RequestedAnimation = SkreeMetareeAnimationPhase.Idling,
            InstalledAnimation = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            YVelocity = 0,
        };
        slot.SetSpeciesState(state);

        // The initializer starts the idle animation directly rather than going through the change detector,
        // because both requested and installed indexes intentionally start at zero.
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.MetareeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Dispatches idle, attack preparation, airborne dive, and burrow phases with required Samus and room data.</summary>
    private void RunMetareeMain(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState? samus,
        RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Metaree AI requires the active Samus actor.");

        switch (state.Action)
        {
            case MetareeAction.Idling:
                RunMetareeIdle(slot, state, samus);
                return;

            case MetareeAction.PreparingAttack:
                RunMetareePreparation(slot, state);
                return;

            case MetareeAction.LaunchedAttack:
                if (level is null)
                {
                    throw new InvalidOperationException(
                        "Metaree dive collision requires active room level data.");
                }
                RunMetareeDive(slot, state, samus, level);
                return;

            case MetareeAction.Burrowing:
                RunMetareeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Metaree has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Starts preparation within 72 horizontal pixels and derives dive speed from the wrapped unsigned Y difference.</summary>
    internal static void RunMetareeIdle(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus)
    {
        // The proximity test computes a wrapped signed difference, takes its signed
        // absolute value, and performs an unsigned compare. It notably does *not* check that
        // Samus is below the actor; preserving that retail bug matters to the velocity math.
        int horizontalDistance = Math.Abs(
            CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole));
        if (horizontalDistance >= MetareeHorizontalActivationDistance)
            return;

        // The unsigned divider input is the Y difference wrapped to 0 through 65535. If Samus is
        // above Metaree, this deliberately creates the game's enormous dive velocity
        // instead of sanitizing the old bug into modern signed physics.
        int verticalDifference = CoordinateAxis.Wrap(samus.Position.Y.Whole - slot.Position.Y.Whole);
        state.YVelocity = verticalDifference / MetareeDiveDivisorNtsc + MetareeMinimumYVelocity;
        state.RequestedAnimation = SkreeMetareeAnimationPhase.PreparingAttack;
        StartRequestedMetareeAnimation(slot, state);
        state.Action = MetareeAction.PreparingAttack;
    }

    /// <summary>Consumes the preparation animation's ready flag, starts the dive animation, and publishes the dive sound.</summary>
    internal void RunMetareePreparation(RoomEnemySlot slot, MetareeEnemyState state)
    {
        if (!state.AttackReady)
            return;

        state.AttackReady = false;
        state.RequestedAnimation = SkreeMetareeAnimationPhase.Diving;
        StartRequestedMetareeAnimation(slot, state);
        state.Action = MetareeAction.LaunchedAttack;

        // The dive sound is requested as the attack launches.
        _audio.QueueEnemySound(EnemySoundEffectDefinitions.DiveSound, priority: SoundRequestPriority.Normal);
    }

    /// <summary>Probes solid terrain ahead before whole-pixel descent; ground contact begins burrowing while airborne frames steer toward Samus.</summary>
    private void RunMetareeDive(
        RoomEnemySlot slot,
        MetareeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The game reloads this every airborne frame. It becomes a countdown only after
        // the attachment probe reports the ground, which is why interrupted dives never age.
        state.BurrowTimer = MetareeBurrowLifetimeNtsc;

        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(
                level,
                slot,
                SignedSixteenSixteen.FromWholePixels(state.YVelocity)))
        {
            slot.AnimationClock.Restart();
            state.Action = MetareeAction.Burrowing;
            _audio.QueueEnemySound(EnemySoundEffectDefinitions.BurrowSound, priority: SoundRequestPriority.Normal);
            return;
        }

        // Unlike common movement, Metaree applies whole-pixel speeds directly and leaves
        // both subpositions untouched. Horizontal steering re-evaluates Samus every frame.
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + state.YVelocity);
        bool metareeIsLeftOfSamus =
            CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) < 0;
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (metareeIsLeftOfSamus
                ? MetareeHorizontalDiveSpeed
                : -MetareeHorizontalDiveSpeed));
    }

    /// <summary>Moves down one pixel per burrow frame, emits debris at timer eight, then deletes the actor.</summary>
    private void RunMetareeBurrow(RoomEnemySlot slot, MetareeEnemyState state)
    {
        if (--state.BurrowTimer == 0)
        {
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer == MetareeParticleFrameNtsc)
            _projectiles.SpawnMetalParticleBurst(slot, MetareeParticleDefinitions.Burst);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 1);
    }

    /// <summary>Changes the segment only when the requested animation phase differs from the installed one.</summary>
    internal static void StartRequestedMetareeAnimation(
        RoomEnemySlot slot,
        MetareeEnemyState state)
    {
        SkreeMetareeAnimationPhase requested = state.RequestedAnimation;
        if (requested == state.InstalledAnimation)
            return;

        state.InstalledAnimation = requested;
        state.Playback = EnemyAnimationPlayback.Start(slot, SkreeMetareeAnimations.StartingWith(SkreeMetareeAnimationChoices.MetareeAnimation(requested)));
    }

    /// <summary>Runs Metaree's main AI.</summary>
    internal void RunMetareeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunMetareeMain(slot, slot.RequireSpeciesState<MetareeEnemyState>(), frame.Samus, frame.Level);
    }

    /// <summary>Advances Metaree's typed animation.</summary>
    internal static void AdvanceMetareeTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, SkreeMetareeAnimations.Segments,
            static (MetareeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(slot, cue, () => state.AttackReady = true);
                return null;
            });

    /// <summary>Spawns four metal debris actors in the dying Metaree's colours before common shot death clears them.</summary>
    internal void AddMetareeShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Metaree &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.MetareeShot)
        {
            BeforeGenericDeath = c =>
            {
                _projectiles.SpawnMetalParticleBurst(c.Enemy, MetareeParticleDefinitions.Burst);
                c.Enemy.Shading = PictureShading.Standard;
            },
        });
    }

    /// <summary>After a lethal bomb, spawns debris while the body's graphics state is intact, then clears both indexes on the dead enemy, which keeps its place.</summary>
    internal void AddMetareeBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Metaree &&
                                                       c.SelectedShotAi == EnemyReaction.MetareeShot)
        {
            AfterCommon = c =>
            {
                if (c.Enemy.Health != 0)
                    return;
                _projectiles.SpawnMetalParticleBurst(c.Enemy, MetareeParticleDefinitions.Burst);
                c.Enemy.Shading = PictureShading.Standard;
            },
        });
    }
}
