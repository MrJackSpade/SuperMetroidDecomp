using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Skree.Artwork;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Enemies.Drawing;

namespace SuperMetroid.Core.Enemies.Skree;

/// <summary>The Skree species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SkreeBehavior : IEnemySpecies
{
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

    /// <summary>Carries out one cue of Skree's animations: the wind-up's attack-ready flag and the dive's off-screen processing.</summary>
    private static SkreeMetareeProgram? ApplySkreeCue(RoomEnemySlot slot, SkreeEnemyState state, SkreeMetareeCue cue)
    {
        switch (cue.Kind)
        {
            case SkreeMetareeCueKind.SetAttackReady:
                state.AttackReady = true;
                break;
            case SkreeMetareeCueKind.ProcessOffScreen:
                slot.Properties = slot.Properties.With(EnemyProperties.ProcessOffScreen);
                break;
            case SkreeMetareeCueKind.StopProcessingOffScreen:
                slot.Properties = slot.Properties.Without(EnemyProperties.ProcessOffScreen);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkreeMetareeCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Per-slot Skree state cleared on each room load.</summary>
    internal readonly SkreeEnemyState?[] _skreeStates =
        new SkreeEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Starts a Skree in its idle function and initial shared instruction list.</summary>
    internal void InitializeSkree(RoomEnemySlot slot)
    {
        var state = new SkreeEnemyState(slot)
        {
            RequestedInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            InstalledInstructionIndex = SkreeMetareeAnimationPhase.Idling,
            AttackReady = false,
            Function = SkreeEnemyFunction.Idling,
        };
        _skreeStates[slot.SlotIndex] = state;
        state.Program = AnimationProgramCursor<SkreeMetareeProgram>.Start(SkreeMetareeProgram.SkreeIdling);
    }

    /// <summary>Winds up near Samus, dives when the instruction callback is ready, then burrows after floor contact.</summary>
    internal void RunSkreeMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Skree AI requires the active Samus actor.");
        SkreeEnemyState state = RequireSkreeState(slot);
        switch (state.Function)
        {
            case SkreeEnemyFunction.Idling:
                if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) < 48)
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
                // `$A3:C70F` queues library-two sound $5B on the exact frame the wind-up
                // instruction releases the dive. Publishing it as a frame event retains
                // native timing without coupling enemy AI to a particular audio backend.
                LastSkreeSoundEffect = EnemySoundEffectDefinitions.DiveSound;
                _audio.QueueEnemySound(SoundEffectLibrary2Sounds.SkreeDive, maximumQueued: 6);
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
                    $"Skree function $A3:{(ushort)state.Function:X4} is not translated.");
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
        state.BurrowTimer = NativeCountdown.Start(21);
        slot.Properties = unchecked((ushort)(slot.Properties | 3));
        if (EnemyTerrain.EnemyHasSolidHighBitAhead(level, slot, 6 << 16, movingDown: true))
        {
            slot.InstructionTimer = NativeCountdown.Start(1);
            slot.Timer = NativeCountdown.Start(0);
            state.Function = SkreeEnemyFunction.Burrowing;
            // The floor collision has its own distinct impact/burrow request at `$A3:C771`.
            LastSkreeSoundEffect = EnemySoundEffectDefinitions.BurrowSound;
            _audio.QueueEnemySound(SoundEffectLibrary2Sounds.SkreeBurrow, maximumQueued: 6);
            return;
        }

        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 6)));
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) >= 0 ? -1 : 1))));
    }

    /// <summary>Advances the 21-frame burrow, spawning debris at eight and deleting the slot at zero.</summary>
    private void RunSkreeBurrow(RoomEnemySlot slot, SkreeEnemyState state)
    {
        state.BurrowTimer = state.BurrowTimer.Decrement().Remaining;
        if (state.BurrowTimer.IsZero)
        {
            slot.PaletteIndex = EnemyPaletteBits.Palette5;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer.RawValue == 8)
            _projectiles.SpawnSkreeParticleBurst(slot);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
    }

    /// <summary>Changes the Skree phase list only when requested and installed phases differ.</summary>
    internal static void InstallRequestedSkreeInstruction(
        RoomEnemySlot slot,
        SkreeEnemyState state)
    {
        if (state.RequestedInstructionIndex == state.InstalledInstructionIndex)
            return;

        state.InstalledInstructionIndex = state.RequestedInstructionIndex;
        state.Program = AnimationProgramCursor<SkreeMetareeProgram>.Start(SkreeMetareeAnimationDefinitions.SkreeProgram(
            state.RequestedInstructionIndex));
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    /// <summary>Rejects Skree AI dispatch for a slot lacking room-load initialization.</summary>
    private SkreeEnemyState RequireSkreeState(RoomEnemySlot slot) =>
        _skreeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skree state.");

    /// <summary>Registers the Skree-specific initializer for matching enemy definitions.</summary>
    internal void RegisterSkreeInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Skree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeSkree(slot);
        });
    }

    /// <summary>Registers Skree's idle, dive, and burrow AI callback.</summary>
    internal void RegisterSkreeMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Skree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree, (slot, frame, nmiFrameCounter8) =>
        {
            RunSkreeMain(slot, frame.Samus, frame.Level);
        });
    }

    /// <summary>Runs Skree's animations as typed programs (#641).</summary>
    internal void RegisterSkreeTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree, (slot, _) =>
        {
            if (!EnemyProgramAnimation.Expire(slot))
                return;
            var state = RequireSkreeState(slot);
            AnimationProgramCursor<SkreeMetareeProgram> cursor = state.Program;
            EnemyProgramAnimation.Run(slot, SkreeMetareeAnimations.Programs, ref cursor, SkreeMetareeFrameDefinitions.Spritemap,
                    (cue, _) => ApplySkreeCue(slot, state, cue));
            state.Program = cursor;
        });


    /// <summary>Last library-two Skree dive ($5B) or burrow-impact ($5C) sound this enemy pass, also queued by its AI through Max6; cleared before the next pass.</summary>
    public ushort? LastSkreeSoundEffect { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastSkreeSoundEffect = null;
        Array.Clear(_skreeStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastSkreeSoundEffect = null;
    }

    // $A3:C7F5 adds Skree's four debris actors after the shared normal shot handler reports death, before the
    // common death animation releases the enemy slot.
    /// <summary>Spawns Skree's four debris actors before generic death processing of a lethal shot.</summary>
    internal void RegisterSkreeShotReactions(EnemyAiTable table)
    {
        table.AddShot(new EnemyShotReaction(enemy => enemy.EnemyDefinitionPointer == EnemyDefinitionId.Skree &&
                                                     EnemyCollision.ShotAi(enemy) == EnemyInteractionCallback.SkreeShot)
        {
            BeforeGenericDeath = c => _projectiles.SpawnSkreeParticleBurst(c.Enemy),
        });
    }

    // Skree's callback owns its four-piece burst and death animation because it called
    // `NormalEnemyShotAiSkipDeathAnim`. A normal bomb selects death variant zero (only missiles select two).
    /// <summary>Handles lethal bombs with Skree's particle burst and death variant zero after the shared hit path.</summary>
    internal void RegisterSkreeBombReactions(EnemyAiTable table)
    {
        table.AddBombCommon(new EnemyBombReaction(c => c.Enemy.EnemyDefinitionPointer == EnemyDefinitionId.Skree &&
                                                       c.SelectedShotAi == EnemyInteractionCallback.SkreeShot)
        {
            RunsGenericDeath = false,
            AfterCommon = c =>
            {
                if (c.Enemy.Health != 0)
                    return;
                _projectiles.SpawnSkreeParticleBurst(c.Enemy);
                _deathsAndDrops.StartGenericEnemyDeath(c.Enemy, deathAnimation: 0);
            },
        });
    }
}
