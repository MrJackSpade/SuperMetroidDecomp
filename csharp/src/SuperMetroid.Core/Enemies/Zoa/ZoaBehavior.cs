using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Enemies.Zoa.Artwork;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Zoa;

/// <summary>The Zoa species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class ZoaBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Zoa];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new ZoaEntity(slot, common);

    /// <summary>Advances Zoa's typed animation (#641) where the interpreter would run its native list.</summary>
    internal static void AdvanceZoaAnimation(RoomEnemySlot slot)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        ZoaEnemyState state = slot.RequireSpeciesState<ZoaEnemyState>();
        AnimationProgramCursor<ZoaProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, ZoaAnimations.Programs, ref cursor, ZoaFrameDefinitions.ShownFrames.Of,
            (cue, _) => ApplyZoaCue(state, cue));
        state.Program = cursor;
    }

    /// <summary>Carries out one cue of Zoa's animation: each selects the horizontal shot speed.</summary>
    private static ZoaProgram? ApplyZoaCue(ZoaEnemyState state, ZoaCue cue)
    {
        state.XSpeedTableIndex = cue.Kind switch
        {
            ZoaCueKind.SetSpeedIndex4 => 4,
            ZoaCueKind.SetSpeedIndex8 => 8,
            ZoaCueKind.SetSpeedIndex12 => 12,
            _ => throw new InvalidOperationException($"Undefined {nameof(ZoaCueKind)} {cue.Kind}."),
        };
        return null;
    }

    /// <summary>Samus must be within 128 horizontal pixels before a hidden Zoa rises.</summary>
    private const int ZoaActivationColumnDistance = 128;
    /// <summary>NTSC upward travel of half a pixel per gameplay frame.</summary>
    private const int ZoaRisingSubpixelSpeed = 32768;

    /// <summary>Ports <c>InitAI_Zoa</c> at $A3:B44A, saving the population coordinate and hiding the idle actor.</summary>
    internal static void InitializeZoa(RoomEnemySlot slot)
    {
        var state = new ZoaEnemyState
        {
            Function = ZoaEnemyFunction.WaitForSamus,
            InstructionListTableIndex = ZoaAnimationSelector.None,
            PreviousInstructionListTableIndex = ZoaAnimationSelector.None,
            XSpeedTableIndex = 0,
            SpawnPosition = slot.Position.Whole,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);

        // The initializer installs list zero directly while also setting “previous” to
        // zero. Consequently the later change detector is deliberately a no-op at rest.
        state.Program = AnimationProgramCursor<ZoaProgram>.Start(ZoaAnimationDefinitions.Program(ZoaAnimationSelector.None));
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
    }

    /// <summary>Ports <c>MainAI_Zoa</c>, dispatching proximity wait, rise, or horizontal shot from the room state.</summary>
    private static void RunZoaMain(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState? samus,
        Position16<RoomPixels> camera)
    {
        if (samus is null)
            throw new InvalidOperationException("Zoa AI requires the active Samus actor.");

        switch (state.Function)
        {
            case ZoaEnemyFunction.WaitForSamus:
                RunZoaWait(slot, state, samus);
                return;
            case ZoaEnemyFunction.Rising:
                RunZoaRising(slot, state, samus);
                return;
            case ZoaEnemyFunction.Shooting:
                RunZoaShooting(slot, state, camera);
                return;
            default:
                throw new InvalidDataException(
                    $"Zoa function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Wakes when Samus enters the 128-pixel column and selects a rising pose from the wrapped horizontal direction.</summary>
    internal static void RunZoaWait(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        ushort signedDistance = unchecked((ushort)(samus.Position.X.Whole - slot.Position.X.Whole));
        ushort absoluteDistance = unchecked((short)signedDistance) < 0
            ? unchecked((ushort)-signedDistance)
            : signedDistance;
        if (absoluteDistance >= ZoaActivationColumnDistance)
            return;

        // Odd indexes are rising lists: one faces left and three faces right. The sign test
        // uses the same wrapped Samus-minus-enemy word as the common bank-$A0 helper.
        state.InstructionListTableIndex = ZoaAnimationSelector.Rising |
            (unchecked((short)signedDistance) < 0
                ? ZoaAnimationSelector.None
                : ZoaAnimationSelector.FacingRight);
        SetZoaProgram(slot, state);
        state.Function = ZoaEnemyFunction.Rising;
    }

    /// <summary>Reveals Zoa and rises until it reaches Samus's height, then starts its shooting list and clears loop scratch.</summary>
    internal static void RunZoaRising(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        SamusState samus)
    {
        slot.Properties = slot.Properties.Without(EnemyProperties.Invisible);
        if (unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole)) < 0)
        {
            // NTSC subtracts exactly 0.5 px/frame. The PAL regional constant is 0.625;
            // this project targets the user's Japan/USA retail ROM and reads NTSC data.
            slot.Position = slot.Position.AdvanceY(SnesSignedSixteenSixteen.FromRaw(-ZoaRisingSubpixelSpeed));
            return;
        }

        state.InstructionListTableIndex &= ZoaAnimationSelector.FacingRight;
        SetZoaProgram(slot, state);
        state.Function = ZoaEnemyFunction.Shooting;
    }

    /// <summary>Travels in the selected facing direction and resets whole-pixel position when its center exits the camera square.</summary>
    internal static void RunZoaShooting(
        RoomEnemySlot slot,
        ZoaEnemyState state,
        Position16<RoomPixels> camera)
    {
        int unsignedDisplacement = ZoaSpeedDefinitions.Displacement(state.XSpeedTableIndex);

        // Shooting list zero travels left by subtraction; list two travels right by
        // addition. The instruction bytecode changes speed at 64-, 8-, and 48-frame marks.
        int xDisplacement = state.InstructionListTableIndex == ZoaAnimationSelector.None
            ? -unsignedDisplacement
            : unsignedDisplacement;
        slot.Position = slot.Position.AdvanceX(SnesSignedSixteenSixteen.FromRaw(xDisplacement));
        if (EnemyProcessingWindow.EnemyCenterIsOnScreen(slot, camera))
        {
            SetZoaProgram(slot, state);
            return;
        }

        // Once its center leaves the inclusive 256x256 camera square, Zoa teleports to the
        // saved population coordinate and becomes invisible until the next proximity wake.
        // `$A3:B51E-$B527` stores only the whole words: the subpixel fractions left by the
        // rise and the shot carry into the next flight.
        slot.Properties = slot.Properties.With(EnemyProperties.Invisible);
        slot.Position = slot.Position.WithWhole(state.SpawnPosition);
        state.InstructionListTableIndex = ZoaAnimationSelector.None;
        SetZoaProgram(slot, state);
        state.Function = ZoaEnemyFunction.WaitForSamus;
    }

    /// <summary>Starts the selected animation, restarting its timers, only when it differs from the previous one.</summary>
    internal static void SetZoaProgram(RoomEnemySlot slot, ZoaEnemyState state)
    {
        if (state.InstructionListTableIndex == state.PreviousInstructionListTableIndex)
            return;
        state.PreviousInstructionListTableIndex = state.InstructionListTableIndex;
        state.Program = AnimationProgramCursor<ZoaProgram>.Start(ZoaAnimationDefinitions.Program(state.InstructionListTableIndex));
        slot.InstructionTimer = WrappingCountdown.Start(1);
        slot.Timer = WrappingCountdown.Start(0);
    }

    /// <summary>Ports Zoa's main AI.</summary>
    internal static void RunZoaMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunZoaMain(slot, slot.RequireSpeciesState<ZoaEnemyState>(), frame.Samus, frame.Camera);
    }
}
