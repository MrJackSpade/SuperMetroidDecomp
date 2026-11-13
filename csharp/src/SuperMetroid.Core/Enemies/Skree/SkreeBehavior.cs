using SuperMetroid.Core.Rooms;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The Skree species owner: its per-room state, AI routines and the interactions it registers.</summary>
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

    /// <summary>Starts a Skree in its idle function and initial shared program.</summary>
    internal static void InitializeSkree(RoomEnemySlot slot)
    {
        var state = new SkreeEnemyState
        {
            RequestedInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            InstalledInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            Function = SkreeEnemyFunction.Idling,
        };
        slot.SetSpeciesState(state);
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareeProgram.SkreeIdling), AnimationPlace.StartOfAnimation);
    }

    /// <summary>Winds up near Samus, dives when the instruction callback is ready, then burrows after floor contact.</summary>
    internal void RunSkreeMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Skree AI requires the active Samus actor.");
        SkreeEnemyState state = slot.RequireSpeciesState<SkreeEnemyState>();
        switch (state.Function)
        {
            case SkreeEnemyFunction.Idling:
                if (CoordinateAxis.Distance(slot.Position.X.Whole, samus.Position.X.Whole) < 48)
                {
                    state.RequestedInstructionIndex =
                        SkreeMetareeAnimationPhase.PreparingAttack;
                    InstallRequestedSkreeInstruction(slot, state);
                    state.Function = SkreeEnemyFunction.PreparingAttack;
                }
                return;

            case SkreeEnemyFunction.PreparingAttack:
                if (!state.AttackReady)
                    return;
                state.AttackReady = false;
                state.RequestedInstructionIndex = SkreeMetareeAnimationPhase.Diving;
                InstallRequestedSkreeInstruction(slot, state);
                state.Function = SkreeEnemyFunction.Diving;
                // The dive queues its dive sound on the exact frame the wind-up
                // instruction releases the dive. Publishing it as a frame event retains
                // the game's timing without coupling enemy AI to a particular audio backend.
                _audio.QueueEnemySound(EnemySound.SkreeDive, priority: SoundRequestPriority.Normal);
                return;

            case SkreeEnemyFunction.Diving:
                if (level is null)
                    throw new InvalidOperationException("Skree dive collision requires room level data.");
                RunSkreeDive(slot, state, samus, level);
                return;

            case SkreeEnemyFunction.Burrowing:
                RunSkreeBurrow(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Skree function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Moves six pixels downward and one toward Samus until the downward solid-block probe starts burrowing.</summary>
    private void RunSkreeDive(
        RoomEnemySlot slot,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The USA/Japan build reloads 21 every airborne frame. It becomes the complete
        // burrow lifetime only on the frame whose six-pixel downward probe collides.
        state.BurrowTimer = 21;
        if (EnemyTerrain.EnemyMeetsAttachmentBlockBelow(level, slot, SignedSixteenSixteen.FromWholePixels(6)))
        {
            slot.FrameHold = WrappingCountdown.Start(1);
            slot.Timer = WrappingCountdown.Start(0);
            state.Function = SkreeEnemyFunction.Burrowing;
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

    /// <summary>Changes the Skree phase list only when requested and installed phases differ.</summary>
    internal static void InstallRequestedSkreeInstruction(
        RoomEnemySlot slot,
        SkreeEnemyState state)
    {
        if (state.RequestedInstructionIndex == state.InstalledInstructionIndex)
            return;

        state.InstalledInstructionIndex = state.RequestedInstructionIndex;
        state.Playback = new(SkreeMetareeAnimations.StartingWith(SkreeMetareePrograms.SkreeProgram(
            state.RequestedInstructionIndex)), AnimationPlace.StartOfAnimation);
        slot.FrameHold = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports Skree's main AI.</summary>
    internal void RunSkreeMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunSkreeMain(slot, frame.Samus, frame.Level);
    }

    /// <summary>Advances Skree's typed animation.</summary>
    internal static void AdvanceSkreeTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, SkreeMetareeAnimations.Segments,
            static (SkreeEnemyState state) => state.Playback, static (state, playback) => state.Playback = playback,
            (state, cue) =>
            {
                SkreeMetareeCues.Apply(slot, cue, () => state.AttackReady = true);
                return null;
            });


    /// <summary>Spawns Skree's four debris actors before generic death processing of a lethal shot.</summary>
    /// <remarks>The Skree's shot reaction adds its four debris actors after the shared normal shot handler reports death, before the
    /// common death animation releases the enemy slot.</remarks>
    internal void AddSkreeShotReactions(EnemySharedReactions table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.DefinitionId == EnemyDefinitionId.Skree &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyReaction.SkreeShot)
        {
            BeforeGenericDeath = c => _projectiles.SpawnMetalParticleBurst(c.Enemy, SkreeParticleDefinitions.Burst),
        });
    }

    /// <summary>Handles lethal bombs with Skree's particle burst and death variant zero after the shared hit path.</summary>
    /// <remarks>Skree's callback owns its four-piece burst and death animation because it calls the normal enemy shot
    /// AI that skips the death animation. A normal bomb selects death variant zero (only missiles select two).</remarks>
    internal void AddSkreeBombReactions(EnemySharedReactions table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.DefinitionId == EnemyDefinitionId.Skree &&
                                                       c.SelectedShotAi == EnemyReaction.SkreeShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c =>
            {
                if (c.Enemy.Health != 0)
                    return;
                _projectiles.SpawnMetalParticleBurst(c.Enemy, SkreeParticleDefinitions.Burst);
                _deathsAndDrops.StartGenericEnemyDeath(c.Enemy, deathAnimation: EnemyDeathExplosion.Small);
            },
        });
    }
}
