using SuperMetroid.Core.Samus;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Skultera.Artwork;
using SuperMetroid.Core.Rooms.LevelData;

namespace SuperMetroid.Core.Enemies.Skultera;

/// <summary>The Skultera species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SkulteraBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Skultera];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new SkulteraEntity(this, slot, common);

    /// <summary>Room collision helper used by Skultera's horizontal and vertical swim steps.</summary>
    private readonly EnemyTerrain _terrain;

    /// <summary>Binds Skultera movement to the room enemy terrain rules.</summary>
    internal SkulteraBehavior(EnemyTerrain terrain)
    {
        _terrain = terrain;
    }

    /// <summary>Carries out one cue of Skultera's animations.</summary>
    private static SkulteraProgram? ApplySkulteraCue(RoomEnemySlot slot, SkulteraEnemyState state, SkulteraCue cue)
    {
        switch (cue.Kind)
        {
            case SkulteraCueKind.DrawAboveScenery:
                // The right-facing swim promotes the fish above foreground scenery only after its first
                // instruction tick, exactly like the ROM.
                slot.Layer = 6;
                break;
            case SkulteraCueKind.DrawBelowScenery:
                // The left-facing swim draws below the matching scenery layer.
                slot.Layer = 2;
                break;
            case SkulteraCueKind.FinishTurn:
                // Turns sleep immediately after publishing this flag. Main AI consumes it on the following frame
                // and starts the steady swim.
                state.TurnFinished = true;
                break;
            default:
                throw new InvalidOperationException($"Undefined {nameof(SkulteraCueKind)} {cue.Kind}.");
        }
        return null;
    }

    /// <summary>Per-slot radius used to turn the sine phase into vertical bob distance.</summary>
    internal readonly ushort[] _skulteraRadii = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot instruction flag consumed by main AI on the frame after a turn animation sleeps.</summary>
    internal readonly ushort[] _skulteraTurnFinishedFlags = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Per-slot angle step, negated when a completed turn reverses the swim wave.</summary>
    internal readonly ushort[] _skulteraAngleDeltas = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Previous sine Y offset used to move by the difference between adjacent phases.</summary>
    internal readonly ushort[] _skulteraPreviousYOffsets = new ushort[RoomEnemySystem.MaximumEnemyCount];
    /// <summary>Current sine Y offset retained after successful movement or vertical collision.</summary>
    internal readonly ushort[] _skulteraCurrentYOffsets = new ushort[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Skultera</c> at <c>$A3:90B5</c>.</summary>
    internal void InitializeSkultera(RoomEnemySlot slot)
    {
        var state = new SkulteraEnemyState(
            slot,
            _skulteraRadii,
            _skulteraTurnFinishedFlags,
            _skulteraAngleDeltas,
            _skulteraPreviousYOffsets,
            _skulteraCurrentYOffsets);
        slot.SetSpeciesState(state);

        // Parameter one's high byte selects initial facing: zero means right, any nonzero
        // value means left. Its low byte is multiplied by eight because each common speed
        // magnitude stores adjacent {right whole, right fraction, left whole, left fraction}
        // words. Keep the byte offset visible; treating it as an array index is a common
        // source of subtly wrong half-pixel speeds in ports of this AI.
        bool startsLeft = (SnesLittleEndian.HighByteInPlace(slot.Parameter1)) != 0;
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(startsLeft
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight);
        state.Function = startsLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;

        ushort speedTableOffset = unchecked((ushort)((SnesLittleEndian.LowByte(slot.Parameter1)) * 8));
        (state.RightVelocity, state.RightSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(speedTableOffset);
        (state.LeftVelocity, state.LeftSubvelocity) =
            EnemyMath.ReadLinearEnemySpeed(unchecked((ushort)(speedTableOffset + 4)));

        // Parameter two packs radius in its low byte and unsigned initial angle delta in
        // its high byte. The delta becomes a signed word only when a turn negates it.
        state.Radius = unchecked((ushort)(SnesLittleEndian.LowByte(slot.Parameter2)));
        state.AngleDelta = (short)SnesLittleEndian.HighByte(slot.Parameter2);
        state.Angle = 0;
        state.TurnFinished = false;
        state.CurrentYOffset = 0;
        state.PreviousYOffset = unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
    }

    /// <summary>Ports <c>MainAI_Skultera</c> at <c>$A3:912B</c>.</summary>
    private void RunSkulteraMain(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData? level)
    {
        if (level is null)
            throw new InvalidOperationException("Skultera AI requires active room collision data.");

        switch (state.Function)
        {
            case SkulteraEnemyFunction.SwimmingLeft:
                SwimSkultera(slot, state, level, movingLeft: true);
                return;
            case SkulteraEnemyFunction.SwimmingRight:
                SwimSkultera(slot, state, level, movingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningRight:
                FinishSkulteraTurn(slot, state, nowMovingLeft: false);
                return;
            case SkulteraEnemyFunction.TurningLeft:
                FinishSkulteraTurn(slot, state, nowMovingLeft: true);
                return;
            default:
                throw new InvalidDataException(
                    $"Skultera function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    /// <summary>Ports the mirrored swimming functions at <c>$A3:9132/$91AB</c>.</summary>
    private void SwimSkultera(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        RoomLevelData level,
        bool movingLeft)
    {
        int horizontalDisplacement = movingLeft
            ? state.LeftDisplacement
            : state.RightDisplacement;
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                horizontalDisplacement))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
            // Horizontal collision jumps directly to the native merge label. Consequently
            // CurrentYOffset remains the preceding frame's word and is copied below again.
            state.PreviousYOffset = state.CurrentYOffset;
            return;
        }

        state.CurrentYOffset = unchecked((short)EnemyMath.ReadEightBitNegativeSineProduct(
            state.Angle,
            state.Radius));
        short yDifference = unchecked((short)(
            state.CurrentYOffset - state.PreviousYOffset));
        if (_terrain.MoveEnemyVertically(level, slot, SnesSixteenSixteen.FromWhole(yDifference)))
        {
            BeginSkulteraTurn(slot, state, currentlyMovingLeft: movingLeft);
        }
        else
        {
            // Only unobstructed movement advances the low-byte phase. The AND is not
            // cosmetic: native AI discards any high word produced by signed delta wrapping.
            state.Angle = unchecked((ushort)(unchecked((byte)(state.Angle + state.AngleDelta))));
        }

        // Both successful movement and vertical collision retain the newly calculated
        // offset. This prevents a failed vertical step from being retried after the turn.
        state.PreviousYOffset = state.CurrentYOffset;
    }

    /// <summary>Switches swimming AI to its opposite-facing turn animation after a terrain collision.</summary>
    internal static void BeginSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool currentlyMovingLeft)
    {
        state.Function = currentlyMovingLeft
            ? SkulteraEnemyFunction.TurningRight
            : SkulteraEnemyFunction.TurningLeft;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(currentlyMovingLeft
            ? SkulteraProgram.TurningRight
            : SkulteraProgram.TurningLeft);
    }

    /// <summary>Ports the mirrored turn completion functions at <c>$A3:9224/$9256</c>.</summary>
    private static void FinishSkulteraTurn(
        RoomEnemySlot slot,
        SkulteraEnemyState state,
        bool nowMovingLeft)
    {
        if (!state.TurnFinished)
            return;

        state.TurnFinished = false;
        state.Function = nowMovingLeft
            ? SkulteraEnemyFunction.SwimmingLeft
            : SkulteraEnemyFunction.SwimmingRight;
        state.AngleDelta = unchecked((short)-state.AngleDelta);
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
        state.Program = AnimationProgramCursor<SkulteraProgram>.Start(nowMovingLeft
            ? SkulteraProgram.SwimmingLeft
            : SkulteraProgram.SwimmingRight);
    }

    /// <summary>Gets the initialized state for a Skultera slot, rejecting calls before its setup.</summary>
    internal static SkulteraEnemyState RequireSkulteraState(RoomEnemySlot slot) =>
        slot.SpeciesState<SkulteraEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Skultera state.");

    /// <summary>Ports Skultera's initialization AI.</summary>
    internal void RunSkulteraInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeSkultera(slot);
    }

    /// <summary>Ports Skultera's main AI.</summary>
    internal void RunSkulteraMainAi(RoomEnemySlot slot, IEnemyAiFrame frame, byte _)
    {
        RunSkulteraMain(slot, RequireSkulteraState(slot), frame.Level);
    }

    /// <summary>Advances Skultera's typed animation.</summary>
    internal static void AdvanceSkulteraTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        var state = RequireSkulteraState(slot);
        AnimationProgramCursor<SkulteraProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, SkulteraAnimations.Programs, ref cursor, SkulteraFrameDefinitions.Spritemap,
                (cue, _) => ApplySkulteraCue(slot, state, cue));
        state.Program = cursor;
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_skulteraRadii);
        Array.Clear(_skulteraTurnFinishedFlags);
        Array.Clear(_skulteraAngleDeltas);
        Array.Clear(_skulteraPreviousYOffsets);
        Array.Clear(_skulteraCurrentYOffsets);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
