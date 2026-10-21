using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Choot.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Choot;

/// <summary>The Choot species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ChootBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Choot];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ChootEntity(this, slot, common);

    /// <summary>Samus must be within 80 horizontal pixels for a waiting Choot to arm its jump timer.</summary>
    private const ushort ChootActivationDistance = 80;

    /// <summary>The words each physical slot keeps across Choot occupants for the current room.</summary>
    private readonly ChootExtensionWords[] _extensions = NewExtensions();

    /// <summary>Fresh, zeroed extension words for every physical slot.</summary>
    private static ChootExtensionWords[] NewExtensions() =>
        [.. Enumerable.Range(0, EnemySlotTable.MaximumEnemyCount).Select(_ => new ChootExtensionWords())];

    /// <summary>Ports <c>InitAI_Choot</c> at <c>$A2:DF76</c>, selecting a falling pattern and calculating ascent speed from native parameters.</summary>
    internal void InitializeChoot(RoomEnemySlot slot)
    {
        var state = new ChootEnemyState(_extensions[slot.SlotIndex]);
        slot.SetSpeciesState(state, state.ClearWithRecord);

        SetChootProgram(slot, state, ChootProgram.Idle);
        state.Function = ChootEnemyFunction.WaitingForSamus;
        state.SpawnPosition = slot.Position.Whole;

        // Parameter one's high byte selects one of five genuine pattern streams. The sixth
        // ROM table word points back into the pointer table and is documented garbage; no
        // retail population selects it, so expose corrupt/high parameters as invalid data.
        ushort patternIndex = SnesLittleEndian.HighByte(slot.Spawn.Population.Parameter1);
        ChootPatternDefinition pattern = ChootPatternDefinitions.ForIndex(patternIndex);
        state.FallingPath = pattern.FallingPath;
        state.FallingPatternYDistance = pattern.FallingPatternYDistance;

        // The SNES multiplier consumes only the loop count's low byte; the authored loop advance is
        // already below 256. Retail loop counts are nonzero.
        ushort jumpHeight = unchecked((ushort)(
            (SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1)) * state.FallingPatternYDistance));
        state.InitialYSpeedTableIndex = CalculateInitialChootYSpeedTableIndex(jumpHeight);
        state.InitialFallingPosition = state.SpawnPosition.WithY(unchecked((ushort)(
            state.SpawnPosition.Y - jumpHeight)));
        state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
    }

    /// <summary>Ports <c>MainAI_Choot</c> and its four wait, prepare, jump, and fall targets at <c>$A2:E02E-$E143</c>.</summary>
    private static void RunChootMain(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        switch (state.Function)
        {
            case ChootEnemyFunction.WaitingForSamus:
                WaitForChootActivation(slot, state, samus);
                return;
            case ChootEnemyFunction.PreparingJump:
                PrepareChootJump(slot, state);
                return;
            case ChootEnemyFunction.Jumping:
                RunChootJump(slot, state);
                return;
            case ChootEnemyFunction.Falling:
                RunChootFall(slot, state);
                return;
            default:
                throw new InvalidDataException(
                    $"Choot function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Uses wrapped signed horizontal distance to arm the parameter-two jump delay when Samus enters range.</summary>
    private static void WaitForChootActivation(
        RoomEnemySlot slot,
        ChootEnemyState state,
        SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Choot proximity AI requires the active Samus actor.");

        // GetSignedYMinusX treats the wrapped word as signed before taking its magnitude.
        // In particular, $8000 remains $8000 after negation; ordinary Math.Abs(short) would
        // throw and a wider subtraction would disagree near the world-coordinate seam.
        ushort distance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        if (EnemyMath.IsNegative16(distance))
            distance = unchecked((ushort)-distance);
        if (unchecked((short)(distance - ChootActivationDistance)) >= 0)
            return;

        state.JumpDelayTimer = WrappingCountdown.FromRaw(checked((ushort)slot.Spawn.Population.Parameter2));
        state.Function = ChootEnemyFunction.PreparingJump;
    }

    /// <summary>Decrements the native delay word until it turns negative, then starts the jumping instruction list.</summary>
    internal static void PrepareChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        CountdownStep timer = state.JumpDelayTimer.Decrement();
        state.JumpDelayTimer = timer.Remaining;
        if (timer.IsNonNegative)
            return;

        SetChootProgram(slot, state, ChootProgram.Jumping);
        state.Function = ChootEnemyFunction.Jumping;
    }

    /// <summary>Applies native quadratic ascent without room collision and snaps whole coordinates to the calculated falling origin at the apex.</summary>
    internal static void RunChootJump(RoomEnemySlot slot, ChootEnemyState state)
    {
        // Choot applies the table's separately stored negative pair, including its fractional
        // negation bug, without consulting room collision during this authored leap.
        int upwardDisplacement = EnemyMath.ReadQuadraticEnemySpeed(state.YSpeedTableIndex, negative: true);
        uint fixedY = slot.Position.Y.RawValue;
        fixedY = unchecked(fixedY + (uint)upwardDisplacement);
        slot.Position = slot.Position.WithY(SnesFixedPosition.FromRaw(unchecked(fixedY)));

        state.YSpeedTableIndex -= 2;
        if (state.YSpeedTableIndex >= 0)
            return;

        // The apex transition snaps only the whole positions to precalculated origins;
        // subpositions deliberately survive until the final spawn reset, just as in WRAM.
        slot.Position = slot.Position.WithWhole(state.InitialFallingPosition);
        state.FallingOrigin = state.InitialFallingPosition;
        state.FallingPatternFrame = 0;
        state.FallingPatternLoopCounter = WrappingCountdown.FromRaw(unchecked((ushort)(
            (SnesLittleEndian.LowByte(slot.Spawn.Population.Parameter1)) - 1)));
        SetChootProgram(slot, state, ChootProgram.Falling);
        state.Function = ChootEnemyFunction.Falling;
    }

    /// <summary>Follows authored path offsets, advancing its origin at terminators and restoring the exact spawn state after the final loop.</summary>
    internal static void RunChootFall(RoomEnemySlot slot, ChootEnemyState state)
    {
        ChootFallingPathPoint point = ChootFallingPathDefinitions.At(
            state.FallingPath ?? throw new InvalidDataException("A falling Choot has no falling pattern."),
            state.FallingPatternFrame);
        if (point.IsTerminator)
        {
            state.FallingOrigin = state.FallingOrigin.WithY(unchecked((ushort)(
                state.FallingOrigin.Y + state.FallingPatternYDistance)));
            state.FallingPatternFrame = 0;
            state.FallingPatternLoopCounter = state.FallingPatternLoopCounter.Decrement().Remaining;
            if (!state.FallingPatternLoopCounter.IsNegative)
                return;

            // The last terminator restores exact spawn coordinates/subpositions, reinstalls
            // idle bytecode, and rearms the same calculated ascent for the next activation.
            state.YSpeedTableIndex = state.InitialYSpeedTableIndex;
            slot.Position = FixedPosition<RoomPixels>.FromWhole(state.SpawnPosition);
            SetChootProgram(slot, state, ChootProgram.Idle);
            state.Function = ChootEnemyFunction.WaitingForSamus;
            return;
        }

        slot.Position = slot.Position.WithWhole(state.FallingOrigin + point.Offset);
        state.FallingPatternFrame++;
    }

    /// <summary>
    /// Ports <c>CalculateChootInitialJumpSpeed</c> at <c>$A2:DFE9</c>, integrating every second quadratic step until
    /// they cover the requested height. The table ends at record 94, so the result is an even index from 2 to 94.
    /// </summary>
    private static int CalculateInitialChootYSpeedTableIndex(ushort jumpHeight)
    {
        int tableIndex = 0;
        uint accumulatedDistance = 0;
        for (int iteration = 0; iteration < 128; iteration++)
        {
            tableIndex += 2;
            accumulatedDistance = unchecked(accumulatedDistance +
                (uint)EnemyMath.ReadQuadraticEnemySpeed(tableIndex, negative: false));
            ushort wholeDistance = SnesFixedPosition.WholeOf(unchecked((int)accumulatedDistance));
            if (unchecked((short)(wholeDistance - jumpHeight)) >= 0)
                return tableIndex;
        }

        throw new InvalidDataException(
            $"Choot jump-height calculation did not reach {jumpHeight} pixels.");
    }

    /// <summary>Installs idle, jumping, or falling bytecode and restarts its instruction timers.</summary>
    private static void SetChootProgram(
        RoomEnemySlot slot,
        ChootEnemyState state,
        ChootProgram program)
    {
        state.Program = AnimationProgramCursor<ChootProgram>.Start(program);
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Advances Choot's typed animation.</summary>
    internal static void AdvanceChootTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _) =>
        EnemyProgramAnimation.Advance(slot, ChootAnimations.Programs, ChootFrameDefinitions.ShownFrames.Of,
            static (ChootEnemyState state) => state.Program, static (state, cursor) => state.Program = cursor,
            (state, cue, _) => ApplyChootCue(slot, cue));

    /// <summary>Carries out one cue of Choot's animation: each switches off-screen processing on or off.</summary>
    private static ChootProgram? ApplyChootCue(RoomEnemySlot slot, ChootCue cue)
    {
        slot.Properties = cue.Kind switch
        {
            ChootCueKind.ProcessOffScreen => slot.Properties.With(EnemyProperties.ProcessOffScreen),
            ChootCueKind.StopProcessingOffScreen => slot.Properties.Without(EnemyProperties.ProcessOffScreen),
            _ => throw new InvalidOperationException($"Undefined {nameof(ChootCueKind)} {cue.Kind}."),
        };
        return null;
    }

    /// <summary>Ports Choot's main AI.</summary>
    internal static void RunChootMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunChootMain(slot, slot.RequireSpeciesState<ChootEnemyState>(), frame.Samus);
    }
}
