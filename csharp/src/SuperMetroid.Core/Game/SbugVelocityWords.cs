using SuperMetroid.Core.Hardware;
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
    public int SignedFixed => unchecked((int)SnesSixteenSixteen.Compose(Pixel, Subpixel));

    /// <summary>The unchanged 32 bits used by the cartridge's modular position additions.</summary>
    public uint RawFixed => SnesSixteenSixteen.Compose(Pixel, Subpixel);
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
    public byte InitialAngle => SnesLittleEndian.HighByte(_slot.Parameter1);

    /// <summary>Parameter two low byte; both-axis proximity needed for activation.</summary>
    public byte ActivationRadius => unchecked((byte)_slot.Parameter2);

    /// <summary>Parameter two high byte; the activation-table selector.</summary>
    public SbugActivationBehavior ActivationBehavior =>
        ClosedNativeWords.Decode<SbugActivationBehavior>(SnesLittleEndian.HighByte(_slot.Parameter2), "Sbug activation behavior");

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
