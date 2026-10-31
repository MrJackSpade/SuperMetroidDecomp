using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Hibashi.Artwork;
namespace SuperMetroid.Core.Enemies.Hibashi;

/// <summary>The Hibashi species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class HibashiBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Hibashi];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new HibashiEntity(this, slot, common);

    /// <summary>Locates the hitbox record immediately after each visible geyser record.</summary>
    private readonly EnemySlotTable _slotTable;
    /// <summary>Receives each eruption's sound in call order, as the cartridge queues it.</summary>
    private readonly EnemyAudioQueue _audio;

    /// <summary>Binds paired Hibashi parts to the room enemy slot table and the enemy audio queue.</summary>
    internal HibashiBehavior(EnemySlotTable slotTable, EnemyAudioQueue audio)
    {
        _slotTable = slotTable;
        _audio = audio;
    }

    /// <summary>Carries out one cue of the Hibashi's eruption: its sound, an eruption frame's hitbox, or finishing.</summary>
    private HibashiProgram? ApplyHibashiCue(RoomEnemySlot slot, HibashiCue cue)
    {
        switch (cue.Kind)
        {
            case HibashiCueKind.PlayEruptionSound:
                PlayHibashiEruptionSound();
                break;
            case HibashiCueKind.ActivityFrame:
                // Each of the 22 bank-$A6 routines selects the correspondingly indexed Y-offset/radius pair.
                ApplyHibashiActivityFrame(slot, cue.Value);
                break;
            case HibashiCueKind.FinishActivity:
                FinishHibashiActivity(slot);
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(HibashiCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Ports <c>InitAI_Hibashi</c> at <c>$A6:8FFC</c>.</summary>
    internal static void InitializeHibashi(RoomEnemySlot slot)
    {
        var state = new HibashiEnemyState
        {
            InactiveTimerReset = WrappingCountdown.Start(slot.SpawnSettings<HibashiSpawnSettings>().InactiveTime),
            IsCollisionPart = slot.SpawnSettings<HibashiSpawnSettings>().IsCollisionPart,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // Every part starts with the one-frame hitbox map. Only part zero replaces it with
        // the eruption animation and initializes the state-machine words below.
        state.Program = AnimationProgramCursor<HibashiProgram>.Start(HibashiProgram.Hitbox);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
        if (state.IsCollisionPart)
            return;

        state.Program = AnimationProgramCursor<HibashiProgram>.Start(HibashiProgram.Graphics);
        state.Function = HibashiEnemyFunction.Inactive;
        state.SpawnHeight = new Position16<RoomPixels>(0, slot.Position.Y.Whole);

        // The visible graphics record must never participate through its header radius.
        // Its following part receives the dynamic radius from instruction commands.
        slot.Radius = slot.Radius.WithX(0);
    }

    /// <summary>Ports <c>MainAI_Hibashi</c> at <c>$A6:9023</c>.</summary>
    private void RunHibashiMain(RoomEnemySlot slot, HibashiEnemyState state)
    {
        if (state.IsCollisionPart)
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
                    $"Hibashi function {state.Function?.ToString() ?? "none"} is not translated.");
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
        state.ActivityFinished = false;
        graphics.InstructionTimer = WrappingCountdown.Start(1);
        graphics.Timer = WrappingCountdown.Start(0);
        state.Program = AnimationProgramCursor<HibashiProgram>.Start(HibashiProgram.Graphics);

        // Property $0100 controls rendering only. Property $0400 controls admission to
        // Samus/projectile/grapple collision. The bottom remains visually invisible for the
        // entire eruption while becoming a live collision body here.
        graphics.Properties = graphics.Properties.Without(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.Without(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>Ports <c>Function_Hibashi_Active</c> at <c>$A6:9062</c>.</summary>
    private static void RunHibashiActive(RoomEnemySlot graphics, HibashiEnemyState state)
    {
        if (!state.ActivityFinished)
            return;

        state.InactiveTimer = state.InactiveTimerReset;
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        state.Function = HibashiEnemyFunction.Inactive;
    }

    /// <summary>Ports instruction <c>$A6:8DAF</c>.</summary>
    private void PlayHibashiEruptionSound()
    {
        // Each geyser queues its own sound; two erupting in one frame queue two.
        _audio.QueueEnemySound(HibashiDefinitions.EruptionSoundEffect, maximumQueued: SoundQueueLimit.Max6);
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

        HibashiEnemyState state = graphics.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);
        HibashiActivityDefinition frame = HibashiEruption.ActivityFrame(frameIndex);
        hitbox.Position = hitbox.Position.WithWholeY(unchecked((ushort)(
            state.SpawnHeight.Y - frame.YOffset)));
        hitbox.Radius = hitbox.Radius.WithY(checked((ushort)frame.YRadius));
        if (frameIndex == 0)
            hitbox.Radius = hitbox.Radius.WithX(8);
    }

    /// <summary>Ports finish-activity instruction <c>$A6:8FD1</c>.</summary>
    private void FinishHibashiActivity(RoomEnemySlot graphics)
    {
        HibashiEnemyState state = graphics.RequireSpeciesState<HibashiEnemyState>();
        RoomEnemySlot hitbox = GetFollowingHibashiPart(graphics);

        state.ActivityFinished = true;
        hitbox.Radius = new(0, 0);
        graphics.Position = graphics.Position.WithWholeY(state.SpawnHeight.Y);
        graphics.Properties = graphics.Properties.With(EnemyProperties.Invisible);
        hitbox.Properties = hitbox.Properties.With(EnemyProperties.IgnoreSamusCollision);
    }

    /// <summary>
    /// Resolves the native <c>Enemy[1]</c> operand. Retail populations always pair part zero
    /// with a following part-one record; reject only states that cannot represent that read.
    /// </summary>
    private RoomEnemySlot GetFollowingHibashiPart(RoomEnemySlot graphics)
    {
        if (graphics.SlotIndex >= EnemySlotOrder.Capacity - 1)
            throw new InvalidDataException("Hibashi graphics part has no following enemy slot.");
        return _slotTable.Slots[graphics.SlotIndex + 1];
    }

    /// <summary>Ports Hibashi's main AI.</summary>
    internal void RunHibashiMainAi(RoomEnemySlot slot, IEnemyAiFrame _)
    {
        RunHibashiMain(slot, slot.RequireSpeciesState<HibashiEnemyState>());
    }

    /// <summary>Advances Hibashi's typed animation.</summary>
    internal void AdvanceHibashiTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, HibashiAnimations.Programs, HibashiFrameDefinitions.ShownFrames.Of,
            static (HibashiEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyHibashiCue(slot, cue));
}
