using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Skree species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SkreeBehavior : IEnemySpecies
{
    /// <summary>Stores audio.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Stores projectiles.</summary>
    private readonly EnemyProjectileEngine _projectiles;

    /// <summary>Stores deaths and drops.</summary>
    private readonly EnemyDeathsAndDrops _deathsAndDrops;

    /// <summary>Initializes a new skree behavior instance.</summary>
    internal SkreeBehavior(EnemyAudioQueue audio, EnemyProjectileEngine projectiles, EnemyDeathsAndDrops deathsAndDrops)
    {
        _deathsAndDrops = deathsAndDrops;
        _audio = audio;
        _projectiles = projectiles;
    }

    /// <summary>Executes Skree's private animation instruction.</summary>
    private bool TryProcessSkreeInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Skree ||
            !Enum.IsDefined((SkreeInstruction)word))
            return false;

        switch ((SkreeInstruction)word)
        {
            case SkreeInstruction.SetAttackReadyFlag:
                RequireSkreeState(slot).AttackReady = true;
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Skree does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Stores skree states.</summary>
    internal readonly SkreeEnemyState?[] _skreeStates =
        new SkreeEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>initialize skree.</summary>
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
        slot.CurrentInstruction = OrdinaryEnemyInstructionLists.SkreeInitial;
    }

    /// <summary>run skree main.</summary>
    internal void RunSkreeMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Skree AI requires the active Samus actor.");
        SkreeEnemyState state = RequireSkreeState(slot);
        switch (state.Function)
        {
            case SkreeEnemyFunction.Idling:
                if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) < 0x30)
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
                LastSkreeSoundEffect = 0x005b;
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

    /// <summary>run skree dive.</summary>
    private void RunSkreeDive(
        RoomEnemySlot slot,
        SkreeEnemyState state,
        SamusState samus,
        RoomLevelData level)
    {
        // The USA/Japan build reloads 21 every airborne frame. It becomes the complete
        // burrow lifetime only on the frame whose six-pixel downward probe collides.
        state.BurrowTimer = 21;
        slot.Properties = unchecked((ushort)(slot.Properties | 3));
        if (EnemyTerrain.EnemyHasSolidHighBitAhead(level, slot, 6 << 16, movingDown: true))
        {
            slot.InstructionTimer = 1;
            slot.Timer = 0;
            state.Function = SkreeEnemyFunction.Burrowing;
            // The floor collision has its own distinct impact/burrow request at `$A3:C771`.
            LastSkreeSoundEffect = 0x005c;
            _audio.QueueEnemySound(SoundEffectLibrary2Sounds.SkreeBurrow, maximumQueued: 6);
            return;
        }

        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 6)));
        slot.Position = slot.Position.WithWholeX(unchecked((ushort)(slot.Position.X.Whole +
            (unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)) >= 0 ? -1 : 1))));
    }

    /// <summary>run skree burrow.</summary>
    private void RunSkreeBurrow(RoomEnemySlot slot, SkreeEnemyState state)
    {
        state.BurrowTimer = unchecked((ushort)(state.BurrowTimer - 1));
        if (state.BurrowTimer == 0)
        {
            slot.PaletteIndex = EnemyPaletteBits.Palette5;
            slot.VramTilesIndex = 0;
            slot.Properties = slot.Properties.With(EnemyProperties.Deleted);
            return;
        }

        if (state.BurrowTimer == 8)
            _projectiles.SpawnSkreeParticleBurst(slot);
        slot.Position = slot.Position.WithWholeY(unchecked((ushort)(slot.Position.Y.Whole + 1)));
    }

    /// <summary>install requested skree instruction.</summary>
    internal static void InstallRequestedSkreeInstruction(
        RoomEnemySlot slot,
        SkreeEnemyState state)
    {
        if (state.RequestedInstructionIndex == state.InstalledInstructionIndex)
            return;

        state.InstalledInstructionIndex = state.RequestedInstructionIndex;
        slot.CurrentInstruction = SkreeMetareeAnimationDefinitions.SkreeInstructionList(
            state.RequestedInstructionIndex);
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    /// <summary>require skree state.</summary>
    private SkreeEnemyState RequireSkreeState(RoomEnemySlot slot) =>
        _skreeStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skree state.");

    /// <summary>register skree initialization ai.</summary>
    internal void RegisterSkreeInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Skree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeSkree(slot);
        });
    }

    /// <summary>register skree main ai.</summary>
    internal void RegisterSkreeMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Skree, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Skree, (slot, frame, nmiFrameCounter8) =>
        {
            RunSkreeMain(slot, frame.Samus, frame.Level);
        });
    }

    /// <summary>register skree owned instructions.</summary>
    internal void RegisterSkreeOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessSkreeInstruction(slot, word, ref cursor);
        });
    }

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
    /// <summary>register skree shot reactions.</summary>
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
    /// <summary>register skree bomb reactions.</summary>
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
