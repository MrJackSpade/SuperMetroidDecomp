using SuperMetroid.Core.Audio;
namespace SuperMetroid.Core.Game;

/// <summary>The Hibashi species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class HibashiBehavior : IEnemySpecies
{
    /// <summary>Locates the hitbox record immediately after each visible geyser record.</summary>
    private readonly EnemySlotTable _slotTable;

    /// <summary>Binds paired Hibashi parts to the room enemy slot table.</summary>
    internal HibashiBehavior(EnemySlotTable slotTable)
    {
        _slotTable = slotTable;
    }

    /// <summary>Executes Hibashi's private animation instructions.</summary>
    private bool TryProcessHibashiInstruction(RoomEnemySlot slot, ushort word, ref ushort cursor)
    {
        if (slot.EnemyDefinitionPointer != EnemyDefinitionId.Hibashi ||
            !Enum.IsDefined((HibashiInstruction)word))
            return false;

        switch ((HibashiInstruction)word)
        {
            case HibashiInstruction.PlaySFX:
                PlayHibashiEruptionSound();
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case HibashiInstruction.ActivityFrame0:
            case HibashiInstruction.ActivityFrame1:
            case HibashiInstruction.ActivityFrame2:
            case HibashiInstruction.ActivityFrame3:
            case HibashiInstruction.ActivityFrame4:
            case HibashiInstruction.ActivityFrame5:
            case HibashiInstruction.ActivityFrame6:
            case HibashiInstruction.ActivityFrame7:
            case HibashiInstruction.ActivityFrame8:
            case HibashiInstruction.ActivityFrame9:
            case HibashiInstruction.ActivityFrameA:
            case HibashiInstruction.ActivityFrameB:
            case HibashiInstruction.ActivityFrameC:
            case HibashiInstruction.ActivityFrameD:
            case HibashiInstruction.ActivityFrameE:
            case HibashiInstruction.ActivityFrameF:
            case HibashiInstruction.ActivityFrame10:
            case HibashiInstruction.ActivityFrame11:
            case HibashiInstruction.ActivityFrame12:
            case HibashiInstruction.ActivityFrame13:
            case HibashiInstruction.ActivityFrame14:
            case HibashiInstruction.ActivityFrame15:
                // These 22 bank-$A6 routines each select the correspondingly indexed
                // Y-offset/radius pair, derived from the executed routine address.
                ApplyHibashiActivityFrame(slot, HibashiInstructionLayout.ActivityFrameIndex((HibashiInstruction)word));
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            case HibashiInstruction.FinishActivity:
                FinishHibashiActivity(slot);
                cursor = unchecked((ushort)(cursor + 2));
                return true;
            default:
                throw new InvalidOperationException(
                    $"Hibashi does not own instruction ${word:X4}.");
        }
    }

    /// <summary>Per-slot geyser timing and shape state cleared at room load.</summary>
    internal readonly HibashiEnemyState?[] _hibashiStates =
        new HibashiEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Hibashi</c> at <c>$A6:8FFC</c>.</summary>
    internal void InitializeHibashi(RoomEnemySlot slot)
    {
        var state = new HibashiEnemyState(slot);
        _hibashiStates[slot.SlotIndex] = state;

        // Every part starts with the one-frame hitbox map. Only part zero replaces it with
        // the instruction-driven eruption list and initializes the state-machine words below.
        slot.CurrentInstruction = HibashiDefinitions.HitboxInstructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        if (state.Part != 0)
            return;

        slot.CurrentInstruction = HibashiDefinitions.GraphicsInstructionList;
        state.Function = HibashiEnemyFunction.Inactive;
        state.SpawnYPosition = slot.Position.Y.Whole;

        // The visible graphics record must never participate through its header radius.
        // Its following part receives the dynamic radius from instruction commands.
        slot.Radius = slot.Radius.WithX(0);
    }

    /// <summary>Ports <c>MainAI_Hibashi</c> at <c>$A6:9023</c>.</summary>
    private void RunHibashiMain(RoomEnemySlot slot, HibashiEnemyState state)
    {
        if (state.Part != 0)
            return;

        switch (state.Function)
        {
            case HibashiEnemyFunction.Inactive:
                RunHibashiInactive(slot, state);
                return;

            case HibashiEnemyFunction.Active:
                RunHibashiActive(slot, state);
                return;

            default:
                throw new InvalidDataException(
                    $"Hibashi function $A6:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports <c>Function_Hibashi_Inactive</c> at <c>$A6:902F</c>.</summary>
    private void RunHibashiInactive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        // DEC/BPL means timer zero deliberately becomes $FFFF and erupts immediately. A
        // reset value N later produces N+1 inactive actor frames because zero is nonnegative.
        state.InactiveTimer = state.InactiveTimer.Decrement().Remaining;
        if (!EnemyMath.IsNegative16(state.InactiveTimer.RawValue))
            return;

        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        state.Function = HibashiEnemyFunction.Active;
        state.FinishedActivityFlag = 0;
        graphics.InstructionTimer = NativeCountdown.Start(1);
        graphics.Timer = NativeCountdown.Start(0);
        graphics.CurrentInstruction = HibashiDefinitions.GraphicsInstructionList;

        // Property $0100 controls rendering only. Property $0400 controls admission to
        // Samus/projectile/grapple collision. The bottom remains visually invisible for the
        // entire eruption while becoming a live collision body here.
        graphics.Properties = graphics.Properties.Without(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.Without(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Ports <c>Function_Hibashi_Active</c> at <c>$A6:9062</c>.</summary>
    private static void RunHibashiActive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        if (state.FinishedActivityFlag == 0)
            return;

        state.InactiveTimer = NativeCountdown.FromRaw(state.InactiveTimerResetValue);
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        state.Function = HibashiEnemyFunction.Inactive;
    }

    /// <summary>Ports instruction <c>$A6:8DAF</c>.</summary>
    private void PlayHibashiEruptionSound()
    {
        LastHibashiSoundEffect = HibashiDefinitions.EruptionSoundEffect;
    }

    /// <summary>
    /// Ports activity instructions $A6:8E13-$8FBD. The 22 commands differ only by their
    /// direct activity index; frame zero additionally restores the fixed eight-pixel X
    /// radius.
    /// </summary>
    internal void ApplyHibashiActivityFrame(RoomEnemySlot graphics, int frameIndex)
    {
        if ((uint)frameIndex >= 22)
            throw new ArgumentOutOfRangeException(nameof(frameIndex));

        LastHibashiActivityFrameIndex = frameIndex;
        HibashiEnemyState state = RequireHibashiState(graphics);
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        HibashiActivityDefinition frame = HibashiDefinitions.ActivityFrame(frameIndex);
        hitbox.Position = hitbox.Position.WithWholeY(unchecked((ushort)(
            state.SpawnYPosition - frame.YOffset)));
        hitbox.Radius = hitbox.Radius.WithY(frame.YRadius);
        if (frameIndex == 0)
            hitbox.Radius = hitbox.Radius.WithX(8);
    }

    /// <summary>Ports finish-activity instruction <c>$A6:8FD1</c>.</summary>
    private void FinishHibashiActivity(RoomEnemySlot graphics)
    {
        HibashiEnemyState state = RequireHibashiState(graphics);
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);

        state.FinishedActivityFlag = 1;
        hitbox.Radius = new(0, 0);
        graphics.Position = graphics.Position.WithWholeY(state.SpawnYPosition);
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>
    /// Resolves the native <c>Enemy[1]</c> operand. Retail populations always pair part zero
    /// with a following part-one record; reject only states that cannot represent that read.
    /// </summary>
    private RoomEnemySlot GetFollowingHibashiPart(RoomEnemySlot graphics)
    {
        if (graphics.SlotIndex >= RoomEnemySystem.MaximumEnemyCount - 1)
            throw new InvalidDataException("Hibashi graphics part has no following enemy slot.");
        return _slotTable._slots[graphics.SlotIndex + 1];
    }

    /// <summary>Rejects geyser AI or instruction dispatch for an uninitialized slot.</summary>
    private HibashiEnemyState RequireHibashiState(RoomEnemySlot slot) =>
        _hibashiStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Hibashi state.");

    /// <summary>Registers initialization of paired graphics and collision records.</summary>
    internal void RegisterHibashiInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Hibashi, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeHibashi(slot);
        });
    }

    /// <summary>Registers inactive-countdown and active-eruption AI dispatch.</summary>
    internal void RegisterHibashiMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Hibashi, slot => slot.EnemyDefinitionPointer == EnemyDefinitionId.Hibashi, (slot, frame, nmiFrameCounter8) =>
        {
            RunHibashiMain(slot, RequireHibashiState(slot));
        });
    }

    /// <summary>Registers sound, 22 shape updates, and finish-activity instructions owned by Hibashi.</summary>
    internal void RegisterHibashiOwnedInstructions(EnemyAiTable table)
    {
        table.AddOwnedInstructions((RoomEnemySlot slot, EnemyInstructionContext context, ushort word, ref ushort cursor, out bool yieldInterpreter) =>
        {
            yieldInterpreter = false;
            return TryProcessHibashiInstruction(slot, word, ref cursor);
        });
    }

    /// <summary>Last library-two Hibashi eruption sound this enemy pass, using the Max6 queue entry; cleared at the next enemy-frame boundary.</summary>
    public ushort? LastHibashiSoundEffect { get; private set; }
    /// <summary>
    /// Zero-based index of the bank-$A6 fire-geyser shape command executed this frame.
    /// This mirrors an instruction-dispatch event rather than inventing persistent enemy
    /// state; <see langword="null"/> means no shape command ran during the current frame.
    /// </summary>
    public int? LastHibashiActivityFrameIndex { get; private set; }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        LastHibashiSoundEffect = null;
        LastHibashiActivityFrameIndex = null;
        Array.Clear(_hibashiStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
        LastHibashiSoundEffect = null;
        LastHibashiActivityFrameIndex = null;
    }

    /// <summary>Queues this frame's published sound effects through the shared enemy audio queue, in the retail queue limits.</summary>
    internal void QueueLegacySounds(EnemyAudioQueue audio)
    {
        audio.QueueLegacySound(LastHibashiSoundEffect, library: SoundEffectLibrary.Library2, maximumQueued: 6);
    }
}
