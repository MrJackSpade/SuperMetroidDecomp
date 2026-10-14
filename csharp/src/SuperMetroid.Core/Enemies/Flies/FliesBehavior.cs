using SuperMetroid.Core.Fx;
using SuperMetroid.Core.Enemies.Flies.Artwork;
using SuperMetroid.Core.Enemies.Drawing;
using SuperMetroid.Core.Samus;

namespace SuperMetroid.Core.Enemies.Flies;

/// <summary>The Flies species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class FliesBehavior : IEnemySpecies
{
    /// <summary>Provides the shared room RNG whose advance is part of each fly AI update.</summary>
    private readonly EnemyRoomBinding _room;

    /// <summary>Attaches fly AI to the room's enemy and random-state services.</summary>
    internal FliesBehavior(EnemyRoomBinding room)
    {
        _room = room;
    }

    /// <summary>Maximum horizontal Samus distance, in pixels, that triggers an attack from circular flight.</summary>
    private const int FlyAttackHorizontalRange = 0x70;
    /// <summary>Per-slot motion state for active Mellow, Mella, and Menu enemies.</summary>
    internal readonly FlyEnemyState?[] _flyStates = new FlyEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Starts a newly loaded fly at angle zero with its flight instruction and clockwise motion.</summary>
    internal void InitializeFly(RoomEnemySlot slot)
    {
        var state = new FlyEnemyState(slot)
        {
            Angle = 0,
            Function = FlyEnemyFunction.ClockwiseCircle,
        };
        _flyStates[slot.SlotIndex] = state;
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
                    $"Fly function $A2:{(ushort)state.Function:X4} is not translated.");
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
        state.Angle = unchecked((ushort)((state.Angle + (clockwise ? 0x20 : -0x20)) & 0x01ff));
        if (state.Angle == 0)
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
        state.TargetYPosition = samus.Position.Y.Whole;
        state.Function = FlyEnemyFunction.AttackSamus;
    }

    /// <summary>Continues the dive until it crosses Samus's captured height, then reverses vertical speed into retreat.</summary>
    private static void RunFlyAttack(RoomEnemySlot slot, FlyEnemyState state)
    {
        MoveFlyAccordingToVelocities(slot, state);
        state.RetreatTimer = NativeCountdown.FromRaw(unchecked((ushort)(state.RetreatTimer.RawValue + 1)));

        bool crossedTarget = state.Velocity.Y.SignedRawValue < 0
            ? slot.Position.Y.Whole < state.TargetYPosition
            : slot.Position.Y.Whole >= state.TargetYPosition;
        if (!crossedTarget)
            return;

        state.Velocity = state.Velocity.ReversedY();
        state.Function = FlyEnemyFunction.Retreat;
    }

    /// <summary>Applies the sine and phase-offset cosine table deltas for circular flight.</summary>
    private static void MoveFlyAccordingToAngle(RoomEnemySlot slot, FlyEnemyState state)
    {
        int tableIndex = state.Angle >> 1;
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
    private FlyEnemyState RequireFlyState(RoomEnemySlot slot) =>
        _flyStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized fly state.");

    /// <summary>Runs the shared flight loop as a typed program (#641).</summary>
    internal void RegisterFliesTypedAnimations(EnemyAiTable table) =>
        table.AddTypedAnimation(slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu,
            (slot, _) =>
            {
                if (!EnemyProgramAnimation.Expire(slot))
                    return;
                FlyEnemyState state = RequireFlyState(slot);
                AnimationProgramCursor<FlyProgram> cursor = state.Program;
                EnemyProgramAnimation.Run(slot, FlyAnimations.Programs, ref cursor, FlyFrameDefinitions.Spritemap);
                state.Program = cursor;
            });

    /// <summary>Registers shared initialization for Mellow, Mella, and Menu definitions.</summary>
    internal void RegisterFliesInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Mellow_Mella_Menu, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeFly(slot);
        });
    }

    /// <summary>Registers their shared per-frame movement and attack dispatcher.</summary>
    internal void RegisterFliesMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Mellow_Mella_Menu, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Mellow or EnemyDefinitionId.Mella or EnemyDefinitionId.Menu, (slot, frame, nmiFrameCounter8) =>
        {
            RunFlyMain(slot, frame.Samus);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_flyStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
