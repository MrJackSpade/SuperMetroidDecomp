using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The Sbug species owner: its per-room state, AI routines and the interactions it registers.</summary>
internal sealed class SbugBehavior : IEnemySpecies
{
    private readonly EnemyRoomBinding _room;
    private readonly EnemyTerrain _terrain;

    internal SbugBehavior(EnemyRoomBinding room, EnemyTerrain terrain)
    {
        _room = room;
        _terrain = terrain;
    }

    private const ushort SbugRandomSeed = 0x000b;
    private const ushort SbugMovementSegmentFrames = 0x0020;
    private const ushort SbugLongRandomLifetime = 0x0200;
    private const int SbugReverseDistance = 0x0060;

    internal readonly SbugEnemyState?[] _sbugStates = new SbugEnemyState?[RoomEnemySystem.MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Sbug</c> at $A3:A14D.</summary>
    internal void InitializeSbug(RoomEnemySlot slot)
    {
        byte behavior = SnesLittleEndian.HighByte(slot.Parameter2);
        if (behavior > (byte)SbugActivationBehavior.MoveAwayFromSamus)
        {
            throw new InvalidDataException(
                $"Sbug activation behavior {behavior} exceeds the seven-entry ROM table.");
        }

        var state = new SbugEnemyState(slot)
        {
            // Enemy extension arrays are cleared with room WRAM before initialization. Spell
            // out the two animation words because SetSbugInstructionList compares them.
            RequestedInstructionList = 0,
            InstalledInstructionList = 0,
            Function = SbugEnemyFunction.WaitForSamus,
        };
        _sbugStates[slot.SlotIndex] = state;

        // $A3:A183 uses the cartridge-derived unsigned quarter-circle magnitudes and lets
        // $A0:B691 apply signs later from the original angle. The side velocities instead use the signed,
        // intentionally imperfect eight-bit multiply routine at $A0:B0B2/$B0C6.
        state.ForwardXVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle,
            state.Speed,
            phase: 0x40);
        state.ForwardYVelocity = CalculateUnsignedSbugMagnitude(
            state.InitialAngle,
            state.Speed,
            phase: 0x80);
        CalculateSignedSbugVelocities(
            unchecked((byte)(state.InitialAngle - 0x20)),
            state.Speed,
            out SbugVelocityWords leftX,
            out SbugVelocityWords leftY);
        state.LeftXVelocity = leftX;
        state.LeftYVelocity = leftY;
        CalculateSignedSbugVelocities(
            unchecked((byte)(state.InitialAngle + 0x20)),
            state.Speed,
            out SbugVelocityWords rightX,
            out SbugVelocityWords rightY);
        state.RightXVelocity = rightX;
        state.RightYVelocity = rightY;

        state.ForwardInstructionIndex = CalculateSbugInstructionIndex(state.InitialAngle);
        state.LeftInstructionIndex = CalculateSbugInstructionIndex(
            unchecked((byte)(state.InitialAngle - 0x20)));
        state.RightInstructionIndex = CalculateSbugInstructionIndex(
            unchecked((byte)(state.InitialAngle + 0x20)));
        SetSbugFacing(slot, state, state.ForwardInstructionIndex);
    }

    /// <summary>Ports <c>MainAI_Sbug</c> and its complete indirect dispatch table.</summary>
    internal void RunSbugMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
    {
        if (samus is null)
            throw new InvalidOperationException("Sbug AI requires the active Samus actor.");

        SbugEnemyState state = RequireSbugState(slot);
        switch (state.Function)
        {
            case SbugEnemyFunction.WaitForSamus:
                RunSbugWait(slot, state, samus);
                return;

            // Native activation functions deliberately consume a frame merely replacing
            // their own pointer. Movement begins on the following enemy-main invocation.
            case SbugEnemyFunction.ActivateMoveForward:
                state.Function = SbugEnemyFunction.MoveForward;
                return;
            case SbugEnemyFunction.ActivateZigZag:
                state.Function = SbugEnemyFunction.ZigZag;
                return;
            case SbugEnemyFunction.ActivateRandomUntilCollision:
                SetSbugRandomSeed();
                state.Function = SbugEnemyFunction.ChooseRandomDirectionUntilCollision;
                return;
            case SbugEnemyFunction.ActivateRandomAndReverseWhenFar:
                state.StraightMovementTimer = NativeCountdown.Start(SbugLongRandomLifetime);
                SetSbugRandomSeed();
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            case SbugEnemyFunction.ActivateMoveForwardThenWait:
                state.MovementTimer = NativeCountdown.Start(SbugMovementSegmentFrames);
                state.Function = SbugEnemyFunction.MoveForwardThenWait;
                return;
            case SbugEnemyFunction.ActivateMoveTowardSamus:
                ActivateSbugTowardOrAway(slot, state, samus, away: false);
                return;
            case SbugEnemyFunction.ActivateMoveAwayFromSamus:
                ActivateSbugTowardOrAway(slot, state, samus, away: true);
                return;

            case SbugEnemyFunction.MoveForward:
                MoveSbugForward(slot, state);
                return;
            case SbugEnemyFunction.ZigZag:
                RunSbugZigZag(slot, state);
                return;
            case SbugEnemyFunction.MoveTowardSamus:
            case SbugEnemyFunction.MoveAwayFromSamus:
                AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
                return;
            case SbugEnemyFunction.MoveForwardThenWait:
                RunSbugForwardThenWait(slot, state);
                return;
            case SbugEnemyFunction.MoveRandomlyUntilCollision:
                RunSbugCollisionAwareSegment(slot, state, level);
                return;
            case SbugEnemyFunction.MoveStraightAndReverseWhenFar:
                RunSbugStraightReverseSegment(slot, state, samus);
                return;
            case SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar:
                ChooseSbugRandomDirection(slot, state, reverseWhenFar: true);
                return;
            case SbugEnemyFunction.ChooseRandomDirectionUntilCollision:
                ChooseSbugRandomDirection(slot, state, reverseWhenFar: false);
                return;
            default:
                throw new InvalidDataException(
                    $"Sbug function $A3:{(ushort)state.Function:X4} is not translated.");
        }
    }

    private static void RunSbugWait(RoomEnemySlot slot, SbugEnemyState state, SamusState samus)
    {
        int threshold = state.ActivationRadius;
        int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
        int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
        if (xDistance >= threshold || yDistance >= threshold)
            return;

        state.Function = SbugMovementDefinitions.ActivationFunction(state.ActivationBehavior);
    }

    private static void ActivateSbugTowardOrAway(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus,
        bool away)
    {
        // CalculateAngleOfSamusFromEnemy uses zero-up/clockwise units. Subtracting it from
        // $40 converts to Sbug's zero-right/counter-clockwise mathematical convention;
        // adding $80 selects the exact opposite direction for behavior six.
        byte cartridgeAngle = EnemyMath.CalculateCartridgeAngle(
            new(unchecked((short)(samus.Position.X.Whole - slot.Position.X.Whole)), unchecked((short)(samus.Position.Y.Whole - slot.Position.Y.Whole))));
        state.CustomAngle = unchecked((byte)(0x40 - cartridgeAngle + (away ? 0x80 : 0)));
        RecalculateSbugCustomDirection(slot, state);
        state.Function = away
            ? SbugEnemyFunction.MoveAwayFromSamus
            : SbugEnemyFunction.MoveTowardSamus;
    }

    private static void RunSbugZigZag(RoomEnemySlot slot, SbugEnemyState state)
    {
        // The global enemy frame counter divides motion into alternating 16-frame diagonal
        // legs. It is sampled before the scheduler increments it for the current frame.
        if ((slot.FrameCounter & 0x0010) == 0)
        {
            SetSbugFacing(slot, state, state.LeftInstructionIndex);
            AddSbugVelocity(slot, state.LeftXVelocity, state.LeftYVelocity);
        }
        else
        {
            SetSbugFacing(slot, state, state.RightInstructionIndex);
            AddSbugVelocity(slot, state.RightXVelocity, state.RightYVelocity);
        }
    }

    private static void RunSbugForwardThenWait(RoomEnemySlot slot, SbugEnemyState state)
    {
        state.MovementTimer = state.MovementTimer.Decrement().Remaining;
        if (state.MovementTimer.IsNegative)
        {
            state.Function = SbugEnemyFunction.WaitForSamus;
            return;
        }

        MoveSbugForward(slot, state);
    }

    private void RunSbugCollisionAwareSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        RoomLevelData? level)
    {
        state.MovementTimer = state.MovementTimer.Decrement().Remaining;
        if (state.MovementTimer.IsNegative)
        {
            state.Function = SbugEnemyFunction.ChooseRandomDirectionUntilCollision;
            return;
        }
        if (level is null)
            throw new InvalidOperationException("Collision-aware Sbug movement requires room level data.");

        // $A3:A648 stops at the first colliding axis. The common movers also perform the
        // cartridge's wall alignment, so a collision frame must not receive a second raw
        // position adjustment here.
        if (_terrain.MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.CustomXVelocity.SignedFixed))
        {
            state.Function = SbugEnemyFunction.WaitForSamus;
            return;
        }
        if (_terrain.MoveEnemyVertically(level, slot, state.CustomYVelocity.SignedFixed))
            state.Function = SbugEnemyFunction.WaitForSamus;
    }

    private static void RunSbugStraightReverseSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus)
    {
        state.StraightMovementTimer = state.StraightMovementTimer.Decrement().Remaining;
        if (!state.StraightMovementTimer.IsNegative)
        {
            state.MovementTimer = state.MovementTimer.Decrement().Remaining;
            if (state.MovementTimer.IsNegative)
            {
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            }

            // The native branch reverses only when both coordinate differences are at
            // least six blocks. This is a rectangular test, not Euclidean distance.
            int xDistance = Math.Abs(unchecked((short)(slot.Position.X.Whole - samus.Position.X.Whole)));
            int yDistance = Math.Abs(unchecked((short)(slot.Position.Y.Whole - samus.Position.Y.Whole)));
            if (xDistance >= SbugReverseDistance && yDistance >= SbugReverseDistance)
                ReverseSbugCustomDirection(slot, state);
        }

        AddSbugVelocity(slot, state.CustomXVelocity, state.CustomYVelocity);
    }

    private void ChooseSbugRandomDirection(
        RoomEnemySlot slot,
        SbugEnemyState state,
        bool reverseWhenFar)
    {
        // AND #$00FF / SBC #$0040 / AND #$00FF creates an unsigned byte delta. Adding
        // that to the full word is intentional; all later trigonometry masks the low byte.
        ushort randomLow = unchecked((byte)_room.Random.Generate());
        ushort angleDelta = unchecked((byte)(randomLow - 0x40));
        state.CustomAngle = unchecked((ushort)(state.CustomAngle + angleDelta));
        RecalculateSbugCustomDirection(slot, state);
        state.MovementTimer = NativeCountdown.Start(SbugMovementSegmentFrames);
        state.Function = reverseWhenFar
            ? SbugEnemyFunction.MoveStraightAndReverseWhenFar
            : SbugEnemyFunction.MoveRandomlyUntilCollision;
    }

    private static void RecalculateSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        CalculateSignedSbugVelocities(
            unchecked((byte)state.CustomAngle),
            state.Speed,
            out SbugVelocityWords customX,
            out SbugVelocityWords customY);
        state.CustomXVelocity = customX;
        state.CustomYVelocity = customY;
        state.CustomInstructionIndex = CalculateSbugInstructionIndex(
            unchecked((byte)state.CustomAngle));
        SetSbugFacing(slot, state, state.CustomInstructionIndex);
    }

    private static void ReverseSbugCustomDirection(RoomEnemySlot slot, SbugEnemyState state)
    {
        // The original negates pixel and subpixel words separately. For a nonzero low word
        // that is off by exactly 1.0 compared with a proper 32-bit two's complement; this
        // visible drift is part of retail behavior and must not be “fixed.”
        state.CustomXVelocity = NegateSbugWordsIndependently(state.CustomXVelocity);
        state.CustomYVelocity = NegateSbugWordsIndependently(state.CustomYVelocity);

        // $A3:A565 masks with $0007 rather than the expected $000E. Preserve that oddity:
        // normal even indexes remain even, but directions in the upper half fold down.
        state.CustomInstructionIndex = unchecked((ushort)(
            (state.CustomInstructionIndex + 4) & 0x0007));
        SetSbugFacing(slot, state, state.CustomInstructionIndex);
    }

    private static SbugVelocityWords NegateSbugWordsIndependently(SbugVelocityWords value) =>
        new(
            unchecked((ushort)-value.Pixel),
            unchecked((ushort)-value.Subpixel));

    private static void MoveSbugForward(RoomEnemySlot slot, SbugEnemyState state)
    {
        // MoveEnemyAccordingToAngleAndXYSpeeds applies signs to the unsigned magnitudes by
        // testing the original mathematical angle independently for X and Y.
        bool subtractX = ((state.InitialAngle + 0x40) & 0x80) != 0;
        bool subtractY = ((state.InitialAngle + 0x80) & 0x80) != 0;
        slot.Position = slot.Position.WithX(AddRawSbugVelocity(
            slot.Position.X,
            state.ForwardXVelocity.RawFixed,
            subtractX));
        slot.Position = slot.Position.WithY(AddRawSbugVelocity(
            slot.Position.Y,
            state.ForwardYVelocity.RawFixed,
            subtractY));
    }

    private static void AddSbugVelocity(
        RoomEnemySlot slot,
        SbugVelocityWords xVelocity,
        SbugVelocityWords yVelocity)
    {
        slot.Position = slot.Position.WithX(AddRawSbugVelocity(
            slot.Position.X,
            xVelocity.RawFixed,
            subtract: false));
        slot.Position = slot.Position.WithY(AddRawSbugVelocity(
            slot.Position.Y,
            yVelocity.RawFixed,
            subtract: false));
    }

    private static SnesFixedPosition AddRawSbugVelocity(
        SnesFixedPosition position,
        uint velocity,
        bool subtract)
    {
        uint fixedPosition = position.RawValue;
        fixedPosition = subtract
            ? unchecked(fixedPosition - velocity)
            : unchecked(fixedPosition + velocity);
        return SnesFixedPosition.FromRaw(fixedPosition);
    }

    internal static SbugVelocityWords CalculateUnsignedSbugMagnitude(
        byte angle,
        byte speed,
        int phase)
    {
        // ConvertAngleToXy at $A0:B643 indexes a 128-word unsigned quarter-circle table,
        // then performs a complete 16x16->32 multiplication. Population speed is a byte,
        // but retaining uint arithmetic documents the actual width of the result.
        int tableIndex = (angle + phase) & 0x7f;
        uint product = (uint)EnemyTrigonometryTables.UnsignedHalfWave(tableIndex) * speed;
        return new SbugVelocityWords(
            SnesFixedPosition.WholeOf(unchecked((int)product)),
            unchecked((ushort)product));
    }

    private static void CalculateSignedSbugVelocities(
        byte angle,
        byte speed,
        out SbugVelocityWords xVelocity,
        out SbugVelocityWords yVelocity)
    {
        xVelocity = CalculateSignedSbugComponent(angle, speed, phase: 0x40);
        yVelocity = CalculateSignedSbugComponent(angle, speed, phase: 0x80);
    }

    internal static SbugVelocityWords CalculateSignedSbugComponent(byte angle, byte speed, int phase)
    {
        // $A0:B0DA multiplies two bytes, swaps the product's bytes into 16.16 words, then
        // (when negative) negates those words independently. Recreate the operations rather
        // than using floating point or Math.Sin, both of which erase its rounding bug.
        byte tableAngle = unchecked((byte)(angle + phase));
        int sineMagnitude = EnemyTrigonometryTables.EightBitHalfWave(tableAngle & 0x7f);
        ushort product = unchecked((ushort)(sineMagnitude * speed));
        ushort pixel = unchecked((ushort)(SnesUnsignedEightEight.Whole(product)));
        ushort subpixel = unchecked((ushort)SnesSixteenSixteen.FromEightEight(product));
        if ((tableAngle & 0x80) != 0)
        {
            pixel = unchecked((ushort)-pixel);
            subpixel = unchecked((ushort)-subpixel);
        }
        return new SbugVelocityWords(pixel, subpixel);
    }

    private static ushort CalculateSbugInstructionIndex(byte angle) =>
        unchecked((ushort)(2 * (unchecked((byte)(angle - 0x30)) >> 5)));

    private static void SetSbugFacing(
        RoomEnemySlot slot,
        SbugEnemyState state,
        ushort instructionIndex)
    {
        // The index is a byte offset, not an element number. ReverseSbugCustomDirection's
        // retail mask can fold it to $0000-$0007, so do not normalize it to even values.
        // Native code indexes a word table with [index >> 1]. Multiplying that element
        // index back into a byte address clears bit zero. This only matters after the
        // cartridge's odd `(index + 4) & 7` reversal mask; the compiled selector must
        // choose the same word rather than treating an odd index as a distinct direction.
        state.RequestedInstructionList =
            SbugMovementDefinitions.FacingInstructionList(instructionIndex);
        if (state.RequestedInstructionList == state.InstalledInstructionList)
            return;

        state.InstalledInstructionList = state.RequestedInstructionList;
        slot.CurrentInstruction = state.RequestedInstructionList;
        slot.InstructionTimer = NativeCountdown.Start(1);
        slot.Timer = NativeCountdown.Start(0);
    }

    private void SetSbugRandomSeed()
    {
        _room.Random.Seed(SbugRandomSeed);
    }

    private SbugEnemyState RequireSbugState(RoomEnemySlot slot) =>
        _sbugStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Sbug state.");

    internal void RegisterSbugInitializationAi(EnemyAiTable table)
    {

        table.RegisterInitialization(EnemyAiRoutine.InitAI_Sbug, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2, (slot, level, samus, controllerInput, cameraX) =>
        {
            InitializeSbug(slot);
        });
    }

    internal void RegisterSbugMainAi(EnemyAiTable table)
    {

        table.RegisterMain(EnemyAiRoutine.MainAI_Sbug, slot => slot.EnemyDefinitionPointer is EnemyDefinitionId.Sbug or EnemyDefinitionId.Sbug2, (slot, frame, nmiFrameCounter8) =>
        {
            RunSbugMain(slot, frame.Samus, frame.Level);
        });
    }

    /// <inheritdoc/>
    public void ResetForRoomLoad(IRoomEnemyLoad request)
    {
        Array.Clear(_sbugStates);
    }

    /// <inheritdoc/>
    public void BeginEnemyFrame(RoomEnemyFrame frame)
    {
    }
}
