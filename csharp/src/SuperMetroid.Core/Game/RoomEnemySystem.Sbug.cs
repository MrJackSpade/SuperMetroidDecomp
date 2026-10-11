using SuperMetroid.Core.Rooms;

namespace SuperMetroid.Core.Game;

/// <summary>The parameter-two high-byte behaviors accepted by Sbug's activation table.</summary>
public enum SbugActivationBehavior : byte
{
    /// <summary>Selector zero in <c>$A3:A121</c>: continuously moves along the spawn angle without block collision checks.</summary>
    MoveForward = 0,
    /// <summary>Selector one in <c>$A3:A121</c>: alternates the spawn angle minus and plus $20 in sixteen-update legs.</summary>
    ZigZag = 1,
    /// <summary>Selector two in <c>$A3:A121</c>: captures the direction toward Samus at activation and thereafter moves straight, without homing.</summary>
    MoveTowardSamus = 2,
    /// <summary>Selector three in <c>$A3:A121</c>: chooses random-angle segments and returns to proximity waiting on block collision.</summary>
    RandomUntilCollision = 3,
    /// <summary>Selector four in <c>$A3:A121</c>: chooses random segments and reverses when both axis distances from Samus reach 96 pixels during its long countdown.</summary>
    RandomAndReverseWhenFar = 4,
    /// <summary>Selector five in <c>$A3:A121</c>: moves along the spawn angle for 32 updates, then waits for Samus to activate it again.</summary>
    MoveForwardThenWait = 5,
    /// <summary>Selector six in <c>$A3:A121</c>: captures the direction opposite Samus at activation and thereafter moves straight, without homing.</summary>
    MoveAwayFromSamus = 6,
}

/// <summary>The bank-$A3 function pointer stored in a Sbug enemy's variable B.</summary>
public enum SbugEnemyFunction : ushort
{
    /// <summary><c>Function_Sbug_WaitForSamusToGetNear</c> at <c>$A3:A2D7</c>; selects an activation handler only when both axis distances are strictly below the spawn radius.</summary>
    WaitForSamus = 0xa2d7,
    /// <summary><c>Function_Sbug_Activate_0_MoveForwards</c> at <c>$A3:A301</c>; spends one AI invocation selecting continuous forward movement.</summary>
    ActivateMoveForward = 0xa301,
    /// <summary><c>Function_Sbug_Activate_1_MoveForwardsInAZigZag</c> at <c>$A3:A30B</c>; selects alternating side movement without moving on this invocation.</summary>
    ActivateZigZag = 0xa30b,
    /// <summary><c>Function_Sbug_Activate_3_MoveRandomly_WaitForSamusIfHitBlock</c> at <c>$A3:A315</c>; resets shared RNG to $000B and selects the collision-aware random-direction chooser.</summary>
    ActivateRandomUntilCollision = 0xa315,
    /// <summary><c>Function_Sbug_Activate_4_MoveRandomly_ReverseUntil6BlockAway</c> at <c>$A3:A325</c>; sets the $0200 long countdown, resets shared RNG to $000B, and selects its direction chooser.</summary>
    ActivateRandomAndReverseWhenFar = 0xa325,
    /// <summary><c>Function_Sbug_Activate_5_MoveForwards_WaitForSamus</c> at <c>$A3:A33B</c>; initializes the 32-update forward segment without moving yet.</summary>
    ActivateMoveForwardThenWait = 0xa33b,
    /// <summary><c>Function_Sbug_Activate_2_MoveAlong45DegreeAngleTowardsSamus</c> at <c>$A3:A34B</c>; computes and installs a fixed velocity and facing toward Samus without moving yet.</summary>
    ActivateMoveTowardSamus = 0xa34b,
    /// <summary><c>Function_Sbug_Activate_6_MoveAlong45DegreeAngleAwayFromSamus</c> at <c>$A3:A380</c>; computes and installs the opposite velocity and facing without moving yet.</summary>
    ActivateMoveAwayFromSamus = 0xa380,
    /// <summary><c>Function_Sbug_Activated_0_MoveForwards</c> at <c>$A3:A407</c>; adds the precomputed forward magnitudes with signs derived from the spawn angle.</summary>
    MoveForward = 0xa407,
    /// <summary><c>Function_Sbug_Activated_1_MoveForwardsInAZigZag</c> at <c>$A3:A40E</c>; selects side velocity and facing using bit $10 of the enemy frame counter.</summary>
    ZigZag = 0xa40e,
    /// <summary><c>Function_Sbug_Activated_2_MoveAlong45DegreeAngleTowardsSamus</c> at <c>$A3:A440</c>; applies the captured custom velocity without retargeting or block collision checks.</summary>
    MoveTowardSamus = 0xa440,
    /// <summary><c>Function_Sbug_Activated_6_MoveAlong45DegAngleAwayFromSamus</c> at <c>$A3:A447</c>; applies the captured away velocity without retargeting or block collision checks.</summary>
    MoveAwayFromSamus = 0xa447,
    /// <summary><c>Function_Sbug_Activated_5_MoveForwards_WaitForSamus</c> at <c>$A3:A44E</c>; decrements the segment timer before moving and returns to proximity waiting on signed underflow.</summary>
    MoveForwardThenWait = 0xa44e,
    /// <summary><c>Function_Sbug_MoveAlongCustomAngle_WaitForSamusIfHitBlock</c> at <c>$A3:A462</c>; runs a timed, collision-aware segment, waiting on collision or choosing another angle on timer underflow.</summary>
    MoveRandomlyUntilCollision = 0xa462,
    /// <summary><c>Function_Sbug_MoveStraightAlongAngle_ReverseUntil6BlocksAway</c> at <c>$A3:A476</c>; applies custom movement and, while the long countdown is nonnegative, checks segment expiry and the both-axis 96-pixel reversal condition.</summary>
    MoveStraightAndReverseWhenFar = 0xa476,
    /// <summary><c>Function_Sbug_Activated_4_MoveRandomly_ReverseUntil6BlocksAway</c> at <c>$A3:A4B6</c>; perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update reversal-aware segment.</summary>
    ChooseRandomDirectionAndReverseWhenFar = 0xa4b6,
    /// <summary><c>Function_Sbug_Activated_3_MoveRandomly_WaitForSamusIfHitBlock</c> at <c>$A3:A4F0</c>; perturbs the custom angle using shared RNG, installs velocity and facing, and starts a 32-update collision-aware segment.</summary>
    ChooseRandomDirectionUntilCollision = 0xa4f0,
}

/// <summary>
/// The exact two-word representation used for one signed 16.16 Sbug velocity. Keeping the
/// words separate is intentional: $A3:A52A negates each word independently, preserving a
/// documented cartridge bug that differs from negating the combined 32-bit value.
/// </summary>
/// <param name="Pixel">High word of a pixels-per-AI-update displacement; signed for side/custom velocities, unsigned for forward magnitudes.</param>
/// <param name="Subpixel">Low word of the displacement, in units of 1/65536 pixel.</param>
public readonly record struct SbugVelocityWords(ushort Pixel, ushort Subpixel)
{
    /// <summary>The two native words interpreted as one signed host displacement.</summary>
    public int SignedFixed => unchecked(((short)Pixel << 16) | Subpixel);

    /// <summary>The unchanged 32 bits used by the cartridge's modular position additions.</summary>
    public uint RawFixed => ((uint)Pixel << 16) | Subpixel;
}

/// <summary>
/// Typed debugger view of Sbug's common-slot words and its parallel WRAM extension arrays.
/// The names follow the behavior established by $A3:A14D-$A67C rather than the anonymous
/// var00/var20 labels required by the raw decompilation.
/// </summary>
public sealed class SbugEnemyState
{
    private readonly RoomEnemySlot _slot;

    internal SbugEnemyState(RoomEnemySlot slot) => _slot = slot;

    /// <summary>Variable A; the long lifetime used only by behavior four.</summary>
    public ushort StraightMovementTimer
    {
        get => _slot.VariableA;
        internal set => _slot.VariableA = value;
    }

    /// <summary>Variable B; the current same-bank AI dispatch pointer.</summary>
    public SbugEnemyFunction Function
    {
        get => (SbugEnemyFunction)_slot.VariableB;
        internal set => _slot.VariableB = (ushort)value;
    }

    /// <summary>Variable C; a standard-mathematics byte angle retained in a word.</summary>
    public ushort CustomAngle
    {
        get => _slot.VariableC;
        internal set => _slot.VariableC = value;
    }

    /// <summary>Variable F; the 32-frame segment/wait countdown.</summary>
    public ushort MovementTimer
    {
        get => _slot.VariableF;
        internal set => _slot.VariableF = value;
    }

    /// <summary>Parameter one low byte, multiplied by the ROM trigonometry tables.</summary>
    public byte Speed => unchecked((byte)_slot.Parameter1);

    /// <summary>
    /// Parameter one high byte. Zero points right, $40 up, $80 left, and $C0 down.
    /// </summary>
    public byte InitialAngle => unchecked((byte)(_slot.Parameter1 >> 8));

    /// <summary>Parameter two low byte; both-axis proximity needed for activation.</summary>
    public byte ActivationRadius => unchecked((byte)_slot.Parameter2);

    /// <summary>Parameter two high byte; the activation-table selector.</summary>
    public SbugActivationBehavior ActivationBehavior =>
        ClosedNativeWords.Decode<SbugActivationBehavior>(unchecked((byte)(_slot.Parameter2 >> 8)), "Sbug activation behavior");

    /// <summary>Spawn-computed unsigned 16.16 horizontal magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardXVelocity { get; internal set; }
    /// <summary>Spawn-computed unsigned 16.16 vertical magnitude in pixels per AI update; the spawn angle supplies its sign during forward movement.</summary>
    public SbugVelocityWords ForwardYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle - $20</c>, used by the native left zigzag leg.</summary>
    public SbugVelocityWords LeftYVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 horizontal displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightXVelocity { get; internal set; }
    /// <summary>Spawn-computed signed 16.16 vertical displacement for angle <c>InitialAngle + $20</c>, used by the native right zigzag leg.</summary>
    public SbugVelocityWords RightYVelocity { get; internal set; }
    /// <summary>Current signed 16.16 horizontal displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomXVelocity { get; internal set; }
    /// <summary>Current signed 16.16 vertical displacement in pixels per AI update, computed from the custom angle and independently word-negated on reversal.</summary>
    public SbugVelocityWords CustomYVelocity { get; internal set; }

    /// <summary>Even byte offsets into the eight-entry instruction-pointer table.</summary>
    public ushort ForwardInstructionIndex { get; internal set; }
    /// <summary>Spawn-computed even byte offset $00-$0E into <c>$A3:A111</c> for angle <c>InitialAngle - $20</c>; used by the left zigzag leg.</summary>
    public ushort LeftInstructionIndex { get; internal set; }
    /// <summary>Spawn-computed even byte offset $00-$0E into <c>$A3:A111</c> for angle <c>InitialAngle + $20</c>; used by the right zigzag leg.</summary>
    public ushort RightInstructionIndex { get; internal set; }
    /// <summary>Custom-direction byte offset into <c>$A3:A111</c>; normally even $00-$0E, but reversal preserves the native <c>(index + 4) &amp; $0007</c> mask.</summary>
    public ushort CustomInstructionIndex { get; internal set; }

    /// <summary>The list most recently requested by direction-selection code.</summary>
    public ushort RequestedInstructionList { get; internal set; }

    /// <summary>The list already installed in the common enemy instruction interpreter.</summary>
    public ushort InstalledInstructionList { get; internal set; }
}

/// <summary>
/// Literal translation of Sbug/Sbug2 enemy AI $A3:A14D-$A67C. The source labels call this
/// creature “roach”; the player-facing/debug name Sbug is used here consistently.
/// </summary>
public sealed partial class RoomEnemySystem
{

    private const ushort SbugRandomSeed = 0x000b;
    private const ushort SbugMovementSegmentFrames = 0x0020;
    private const ushort SbugLongRandomLifetime = 0x0200;
    private const int SbugReverseDistance = 0x0060;

    private readonly SbugEnemyState?[] _sbugStates = new SbugEnemyState?[MaximumEnemyCount];

    /// <summary>Ports <c>InitAI_Sbug</c> at $A3:A14D.</summary>
    private void InitializeSbug(RoomEnemySlot slot)
    {
        byte behavior = unchecked((byte)(slot.Parameter2 >> 8));
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
    private void RunSbugMain(RoomEnemySlot slot, SamusState? samus, RoomLevelData? level)
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
                state.StraightMovementTimer = SbugLongRandomLifetime;
                SetSbugRandomSeed();
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            case SbugEnemyFunction.ActivateMoveForwardThenWait:
                state.MovementTimer = SbugMovementSegmentFrames;
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
        int xDistance = Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition)));
        int yDistance = Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition)));
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
        byte cartridgeAngle = CalculateCartridgeAngle(
            unchecked((short)(samus.XPosition - slot.XPosition)),
            unchecked((short)(samus.YPosition - slot.YPosition)));
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
        state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
        if ((short)state.MovementTimer < 0)
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
        state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
        if ((short)state.MovementTimer < 0)
        {
            state.Function = SbugEnemyFunction.ChooseRandomDirectionUntilCollision;
            return;
        }
        if (level is null)
            throw new InvalidOperationException("Collision-aware Sbug movement requires room level data.");

        // $A3:A648 stops at the first colliding axis. The common movers also perform the
        // cartridge's wall alignment, so a collision frame must not receive a second raw
        // position adjustment here.
        if (MoveEnemyHorizontallyIgnoringNonSquareSlopes(
                level,
                slot,
                state.CustomXVelocity.SignedFixed))
        {
            state.Function = SbugEnemyFunction.WaitForSamus;
            return;
        }
        if (MoveEnemyVertically(level, slot, state.CustomYVelocity.SignedFixed))
            state.Function = SbugEnemyFunction.WaitForSamus;
    }

    private static void RunSbugStraightReverseSegment(
        RoomEnemySlot slot,
        SbugEnemyState state,
        SamusState samus)
    {
        state.StraightMovementTimer = unchecked((ushort)(state.StraightMovementTimer - 1));
        if ((short)state.StraightMovementTimer >= 0)
        {
            state.MovementTimer = unchecked((ushort)(state.MovementTimer - 1));
            if ((short)state.MovementTimer < 0)
            {
                state.Function = SbugEnemyFunction.ChooseRandomDirectionAndReverseWhenFar;
                return;
            }

            // The native branch reverses only when both coordinate differences are at
            // least six blocks. This is a rectangular test, not Euclidean distance.
            int xDistance = Math.Abs(unchecked((short)(slot.XPosition - samus.XPosition)));
            int yDistance = Math.Abs(unchecked((short)(slot.YPosition - samus.YPosition)));
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
        ushort randomLow = unchecked((byte)_nextRandom!());
        ushort angleDelta = unchecked((byte)(randomLow - 0x40));
        state.CustomAngle = unchecked((ushort)(state.CustomAngle + angleDelta));
        RecalculateSbugCustomDirection(slot, state);
        state.MovementTimer = SbugMovementSegmentFrames;
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
        (slot.XPosition, slot.XSubposition) = AddRawSbugVelocity(
            slot.XPosition,
            slot.XSubposition,
            state.ForwardXVelocity.RawFixed,
            subtractX);
        (slot.YPosition, slot.YSubposition) = AddRawSbugVelocity(
            slot.YPosition,
            slot.YSubposition,
            state.ForwardYVelocity.RawFixed,
            subtractY);
    }

    private static void AddSbugVelocity(
        RoomEnemySlot slot,
        SbugVelocityWords xVelocity,
        SbugVelocityWords yVelocity)
    {
        (slot.XPosition, slot.XSubposition) = AddRawSbugVelocity(
            slot.XPosition,
            slot.XSubposition,
            xVelocity.RawFixed,
            subtract: false);
        (slot.YPosition, slot.YSubposition) = AddRawSbugVelocity(
            slot.YPosition,
            slot.YSubposition,
            yVelocity.RawFixed,
            subtract: false);
    }

    private static (ushort Position, ushort Subposition) AddRawSbugVelocity(
        ushort position,
        ushort subposition,
        uint velocity,
        bool subtract)
    {
        uint fixedPosition = ((uint)position << 16) | subposition;
        fixedPosition = subtract
            ? unchecked(fixedPosition - velocity)
            : unchecked(fixedPosition + velocity);
        return (
            unchecked((ushort)(fixedPosition >> 16)),
            unchecked((ushort)fixedPosition));
    }

    private static SbugVelocityWords CalculateUnsignedSbugMagnitude(
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
            unchecked((ushort)(product >> 16)),
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

    private static SbugVelocityWords CalculateSignedSbugComponent(byte angle, byte speed, int phase)
    {
        // $A0:B0DA multiplies two bytes, swaps the product's bytes into 16.16 words, then
        // (when negative) negates those words independently. Recreate the operations rather
        // than using floating point or Math.Sin, both of which erase its rounding bug.
        byte tableAngle = unchecked((byte)(angle + phase));
        int sineMagnitude = EnemyTrigonometryTables.EightBitHalfWave(tableAngle & 0x7f);
        ushort product = unchecked((ushort)(sineMagnitude * speed));
        ushort pixel = unchecked((ushort)(product >> 8));
        ushort subpixel = unchecked((ushort)(product << 8));
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
        slot.InstructionTimer = 1;
        slot.Timer = 0;
    }

    private void SetSbugRandomSeed()
    {
        if (_setRandomNumber is null)
        {
            throw new InvalidOperationException(
                "Sbug random behaviors require the shared RNG seed-write callback.");
        }
        _setRandomNumber(SbugRandomSeed);
    }

    private SbugEnemyState RequireSbugState(RoomEnemySlot slot) =>
        _sbugStates[slot.SlotIndex] ?? throw new InvalidOperationException(
            $"Enemy slot {slot.SlotIndex} has no initialized Sbug state.");
}
