using SuperMetroid.Core.Rooms.LevelData;
using SuperMetroid.Core.Input;
using SuperMetroid.Core.Programs;
using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The Flies species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FliesBehavior : IEnemySpecies, IEnemyEntityFactory
{
    /// <inheritdoc/>
    public IReadOnlyList<EnemyDefinitionId> Definitions { get; } = [EnemyDefinitionId.Mella, EnemyDefinitionId.Mellow, EnemyDefinitionId.Menu];

    /// <inheritdoc/>
    public IEnemyEntity Create(RoomEnemySlot slot, EnemyCommonParticipation common) => new FliesEntity(this, slot, common);

    /// <summary>Provides the shared room RNG whose advance is part of each fly AI update.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Attaches fly AI to the room's enemy and random-state services.</summary>
    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Maximum horizontal Samus distance, in pixels, that triggers an attack from circular flight.</summary>
    private const int FlyAttackHorizontalRange = 112;

    /// <summary>Starts a newly loaded fly at angle zero with its flight instruction and clockwise motion.</summary>
    internal static void InitializeFly(RoomEnemySlot slot)
    {
        var state = new FlyEnemyState
        {
            Angle = SnesAngle.Zero,
            Function = FlyEnemyFunction.ClockwiseCircle,
        };
        slot.SetSpeciesState(state, state.ClearWithRecord);
        state.Program = AnimationProgramCursor<FlyProgram>.Start(FlyProgram.Flight);
        slot.SpritemapPointer = CommonEnemyEmptyExtendedFrameDefinitions.EmptySpritemap;
        slot.InstructionTimer = NativeCountdown.Start(1);
    }

    /// <summary>Advances room RNG and dispatches a fly's circle, attack, or retreat phase.</summary>
    private void RunFlyMain(RoomEnemySlot slot, SamusState? samus)
    {
        if (samus is null)
            throw new InvalidOperationException("Fly AI requires the active Samus actor.");

        // $A2:B11F advances the global RNG even though this family does not consume the
        // returned word. The side effect matters when several random enemies share a room.
        _room.Random.Generate();
        FlyEnemyState state = RequireFlyState(slot);
        switch (state.Function)
        {
            case FlyEnemyFunction.ClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: true);
                return;
            case FlyEnemyFunction.AntiClockwiseCircle:
                RunFlyCircle(slot, state, samus, clockwise: false);
                return;
            case FlyEnemyFunction.AttackSamus:
                RunFlyAttack(slot, state);
                return;
            case FlyEnemyFunction.Retreat:
                MoveFlyAccordingToVelocities(slot, state);
                state.RetreatTimer = state.RetreatTimer.Decrement().Remaining;
                if (state.RetreatTimer.IsNegative)
                {
                    state.RetreatTimer = NativeCountdown.Start(24);
                    state.Function = FlyEnemyFunction.ClockwiseCircle;
                }
                return;
            default:
                throw new InvalidDataException(
                    $"Fly function {state.Function?.ToString() ?? "none"} is not translated.");
        }
    }

    /// <summary>Rotates a fly around its flight path, reversing direction each revolution or diving when Samus enters range.</summary>
    private static void RunFlyCircle(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus,
        bool clockwise)
    {
        if (!state.RetreatTimer.IsZero)
        {
            state.RetreatTimer = state.RetreatTimer.Decrement().Remaining;
        }
        else if (Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole))) <
            FlyAttackHorizontalRange)
        {
            SetFlyToAttackSamus(slot, state, samus);
            return;
        }

        MoveFlyAccordingToAngle(slot, state);
        state.AdvanceCircle(clockwise);
        if (state.Angle == SnesAngle.Zero)
        {
            state.Function = clockwise
                ? FlyEnemyFunction.AntiClockwiseCircle
                : FlyEnemyFunction.ClockwiseCircle;
        }
    }

    /// <summary>Aims an asymmetric horizontal and vertical dive toward Samus's current position.</summary>
    private static void SetFlyToAttackSamus(
        RoomEnemySlot slot,
        FlyEnemyState state,
        SamusState samus)
    {
        byte angle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
        state.Velocity = new(
            unchecked((short)(ReadSignedSineCosine(angle + 64) * 2)),
            unchecked((short)(ReadSignedSineCosine(angle) * 4)));
        state.TargetHeight = new Position16<RoomPixels>(0, samus.Position.Y.Whole);
        state.Function = FlyEnemyFunction.AttackSamus;
    }

    /// <summary>Continues the dive until it crosses Samus's captured height, then reverses vertical speed into retreat.</summary>
    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.RetreatTimer.RawValue + 1)));

        bool crossedTarget = state.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole < state.TargetHeight.Y
            : slot.Position.Y.Whole >= state.TargetHeight.Y;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.Function = FlyEnemyFunction.Retreat;
    }

    /// <summary>Applies the sine and phase-offset cosine table deltas for circular flight.</summary>
    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        int tableIndex = state.Angle.TableIndex;
        slot.Position = slot.Position.AdvanceX(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex + 64))).ToSixteenSixteenDelta());
        slot.Position = slot.Position.AdvanceY(new SnesSignedEightEight(unchecked((ushort)ReadSignedSineCosine(tableIndex))).ToSixteenSixteenDelta());
    }

    /// <summary>Applies the fixed-point dive or retreat velocity to the enemy position.</summary>
    private static void MoveFlyAccordingToVelocities(RoomEnemySlot slot, FlyEnemyState state)
    {
        slot.Position = slot.Position.Advance(state.Velocity.ToSixteenSixteen());
    }

    /// <summary>Reads the cartridge's negative-cosine table, including the phase-offset range used for horizontal motion.</summary>
    internal static short ReadSignedSineCosine(int index)
    {
        // The backing table begins with negative cosine at index zero, sine at index 64,
        // and continues far enough for every byte angle plus the cosine phase offset.
        return EnemyTrigonometryTables.SignedNegativeCosineWord(index);
    }

    /// <summary>Requires the motion state installed for a live fly slot before its AI runs.</summary>
    private static FlyEnemyState RequireFlyState(RoomEnemySlot slot) =>
        slot.SpeciesState<FlyEnemyState>() ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized fly state.");

    /// <summary>Advances Flies's typed animation.</summary>
    internal static void AdvanceFliesTypedAnimation(RoomEnemySlot slot, EnemyInstructionContext _)
    {
        if (!EnemyProgramAnimation.Expire(slot))
            return;
        FlyEnemyState state = RequireFlyState(slot);
        AnimationProgramCursor<FlyProgram> cursor = state.Program;
        EnemyProgramAnimation.Run(slot, FlyAnimations.Programs, ref cursor, FlyFrameDefinitions.Spritemap);
        state.Program = cursor;
    }

    /// <summary>Ports Flies's initialization AI.</summary>
    internal static void RunFliesInitializationAi(RoomEnemySlot slot, RoomLevelData? _, SamusState? _1, SnesButton _2, ushort _3)
    {
        InitializeFly(slot);
    }

    /// <summary>Ports Flies's main AI.</summary>
    internal void RunFliesMainAi(RoomEnemySlot slot, IEnemyAiFrame frame)
    {
        RunFlyMain(slot, frame.Samus);
    }
}
