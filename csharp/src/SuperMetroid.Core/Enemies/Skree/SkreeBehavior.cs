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
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SkreeEntity(this, slot, common);

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
    internal static void InitializeSkree(RoomEnemySlot slot)
    {
        var state = new SkreeEnemyState
        {
            RequestedAnimation = SkreeMetareeAnimationPhase.Idling,
            InstalledAnimation = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            Action = SkreeAction.Idling,
        };
        slot.SetSpeciesState(state);
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeSegment.SkreeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Winds up near Samus, dives when the wind-up cue is ready, then burrows after floor contact.</summary>
    internal void RunSkreeMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Skree AI requires the active Samus actor.");
        SkreeEnemyState state = slot.RequireSpeciesState<SkreeEnemyState>();
        switch (state.Action)
        {
            case SkreeAction.Idling:
                if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) < 48)
                {
                    state.RequestedAnimation =
                        SkreeMetareeAnimationPhase.PreparingAttack;
                    StartRequestedSkreeAnimation(slot, state);
                    state.Action = SkreeAction.PreparingAttack;
                }
                return;

            case SkreeAction.PreparingAttack:
                if (!state.AttackReady)
                    return;
                state.AttackReady = false;
                state.RequestedAnimation = SkreeMetareeAnimationPhase.Diving;
                StartRequestedSkreeAnimation(slot, state);
                state.Action = SkreeAction.Diving;
                // The dive queues its dive sound on the exact frame the wind-up
                // cue releases the dive. Publishing it as a frame event retains
                // the game's timing without coupling enemy AI to a particular audio backend.
                _audio.QueueEnemySound(EnemySound.SkreeDive, priority: SoundRequestPriority.Normal);
                return;

            case SkreeAction.Diving:
                if (level is null)
                    throw new InvalidOperationException("Skree dive collision requires room level data.");
                RunSkreeDive(slot, state, samus, level);
                return;

            case SkreeAction.Burrowing:
                RunSkreeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Skree has no behaviour for action {state.Action?.ToString() ?? "none"}.");
        }
    }

    /// <summary>Moves six pixels downward and one toward Samus until the downward solid-block probe starts burrowing.</summary>
    private void RunSkreeDive(
        RoomEnemySlot slot,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The Japanese and North American release reloads 21 every airborne frame. It becomes the complete
        // burrow lifetime only on the frame whose six-pixel downward probe collides.
        state.BurrowTimer = 21;
        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(level, slot, SignedSixteenSixteen.FromWholePixels(6)))
        {
            slot.AnimationClock.Restart();
            state.Action = SkreeAction.Burrowing;
            // The floor collision has its own distinct impact/burrow request.
            _audio.QueueEnemySound(EnemySound.SkreeBurrow, priority: SoundRequestPriority.Normal);
            return;
        }

        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 6);
        slot.Position = slot.Position.WithWrappedWholeX(slot.Position.X.Whole +
            (CoordinateAxis.Difference(slot.Position.X.Whole, samus.Position.X.Whole) >= 0 ? -1 : 1));
    }

    /// <summary>Advances the 21-frame burrow, spawning debris at eight and deleting the slot at zero.</summary>
    private void RunSkreeBurrow(RoomEnemySlot slot, SkreeEnemyState state)
    {
        if (--state.BurrowTimer == 0)
        {
            slot.Presence = slot.Presence.With(EnemyPresence.Deleted);
            return;
        }

        if (state.BurrowTimer == 8)
            _projectiles.SpawnMetalParticleBurst(slot, SkreeParticleDefinitions.Burst);
        slot.Position = slot.Position.WithWrappedWholeY(slot.Position.Y.Whole + 1);
    }

    /// <summary>Changes the Skree phase animation only when requested and installed phases differ.</summary>
    internal static void StartRequestedSkreeAnimation(
        RoomEnemySlot slot,
        SkreeEnemyState state)
    {
        if (state.RequestedAnimation == state.InstalledAnimation)
            return;

        state.InstalledAnimation = state.RequestedAnimation;
        state.Playback = EnemyAnimationPlayback.Start(slot, SkreeMetareeAnimations.StartingWith(SkreeMetareeAnimationChoices.SkreeAnimation(
            state.RequestedAnimation)));
    }

    /// <summary>Runs Skree's main AI.</summary>
    internal void RunSkreeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSkreeMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Skree's typed animation.</summary>
    internal static void AdvanceSkreeTypedAnimation(RoomEnemySlot slot, EnemyAnimationCueContext _) =>
        EnemyAnimationPlayback.Advance(slot, SkreeMetareeAnimations.Segments,
            static (SkreeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(slot, cue, () => state.AttackReady = true);
                return null;
            });


    /// <summary>Spawns Skree's four debris actors before generic death processing of a lethal shot.</summary>
    /// <remarks>The Skree's shot reaction adds its four debris actors after the shared normal shot handler reports death, before the
    /// common death animation releases the enemy slot.</remarks>
    internal EnemyShotReaction SkreeShotReaction => field ??= new()
    {
        // Skree's shot reaction runs the common damage without the hit burst.
        ShowsHitBurst = EnemyShotReaction.NeverShowsHitBurst,
        BeforeGenericDeath = c => _projectiles.SpawnMetalParticleBurst(c.Enemy, SkreeParticleDefinitions.Burst),
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
            _projectiles.SpawnMetalParticleBurst(c.Enemy, SkreeParticleDefinitions.Burst);
            _deathsAndDrops.StartGenericEnemyDeath(c.Enemy, deathAnimation: EnemyDeathExplosion.Small);
        },
    };
}
