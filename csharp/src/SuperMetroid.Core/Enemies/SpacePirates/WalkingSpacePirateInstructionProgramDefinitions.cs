using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.SpacePirates;

/// <summary>
/// Compiled engine-control words for all walking Space Pirate body programs.
/// Interleaved extended-spritemap operands select installed presentation data.
/// </summary>
internal abstract class WalkingSpacePirateInstructionProgramDefinitions
{
    /// <summary><c>InstList_PirateWalking_Flinch_FacingLeft</c> at $B2:FB4C.</summary>
    internal const ushort FlinchFacingLeft = 0xfb4c;
    /// <summary><c>InstList_PirateWalking_Flinch_FacingRight</c> at $B2:FB58.</summary>
    internal const ushort FlinchFacingRight = 0xfb58;
    /// <summary><c>InstList_PirateWalking_WalkingLeft_0</c> at $B2:FB64.</summary>
    internal const ushort WalkingLeft = 0xfb64;
    /// <summary><c>InstList_PirateWalking_FireLasersLeft</c> at $B2:FB8C.</summary>
    internal const ushort FireLasersLeft = 0xfb8c;
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingLeft</c> at $B2:FBC6.</summary>
    internal const ushort LookingFacingLeft = 0xfbc6;
    /// <summary><c>InstList_PirateWalking_WalkingRight_0</c> at $B2:FBE6.</summary>
    internal const ushort WalkingRight = 0xfbe6;
    /// <summary><c>InstList_PirateWalking_FireLasersRight</c> at $B2:FC0E.</summary>
    internal const ushort FireLasersRight = 0xfc0e;
    /// <summary><c>InstList_PirateWalking_LookingAround_FacingRight</c> at $B2:FC48.</summary>
    internal const ushort LookingFacingRight = 0xfc48;

    /// <summary>Compiled callbacks, timers, holds, and branches across both flinches and facing halves.</summary>
    public static int MechanicsWordCount => 92;
    /// <summary>Separately installed extended-spritemap operands interleaved with the control words.</summary>
    public static int PresentationWordCount => 50;

    /// <summary>Authored sixteen-tick flinch pose before returning to walking (#1165).</summary>
    private const ushort FlinchHold = 16;
    /// <summary>Ten-tick walking pose cadence shared with alternate looking poses (#1165).</summary>
    private const ushort WalkHold = 10;
    /// <summary>Twenty-four-tick aiming pose before or between laser shots (#1165).</summary>
    private const ushort AimHold = 24;
    /// <summary>Eight-tick firing pose also used by the final looking pose (#1165).</summary>
    private const ushort FireHold = 8;
    /// <summary>Thirty-two-tick long looking pose alternating with walking holds (#1165).</summary>
    private const ushort LookHold = 32;
    /// <summary>First and third gun-barrel heights use symmetric +8/-8 Y offsets (#1165).</summary>
    private const short OuterShotOffset = 8;
    /// <summary>Middle laser is placed two pixels below the drawn gun barrel origin (#1165).</summary>
    private const short MiddleShotOffset = 2;

    /// <summary>Reconstructs one native control address/value pair by ordinal across both facing programs.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: false);
    }

    /// <summary>Locates a visual operand by native address while leaving its artwork separately installed.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return Select(index, visual: true).Address;
    }

    /// <summary>
    /// $B2:FB4C-FC67 contains two12-byte flinches followed by two facing halves.
    /// Each half has a40-byte walk loop,58-byte three-shot attack and32-byte look/turn list.
    /// A timed pose occupies four bytes; functions, callbacks and jump operands two each.
    /// </summary>
    private static InstructionMechanicsWord Select(int index, bool visual)
    {
        var layout = new Layout(index, visual, FlinchFacingLeft);
        for (int facing = 0; facing < 2; facing++)
        {
            layout.InstallFunction(WalkingSpacePirateFunction.NoOperation);
            layout.Pose(FlinchHold);
            layout.Goto(facing == 0 ? WalkingLeft : WalkingRight);
        }
        for (int facing = 0; facing < 2; facing++)
        {
            bool right = facing != 0;
            ushort walking = right ? WalkingRight : WalkingLeft;
            layout.InstallFunction(right ? WalkingSpacePirateFunction.WalkingRight : WalkingSpacePirateFunction.WalkingLeft);
            for (int pose = 0; pose < 8; pose++) layout.Pose(WalkHold);
            layout.Goto((ushort)(walking + 4));

            layout.InstallFunction(WalkingSpacePirateFunction.AnimationOwnedNoOperation);
            layout.Pose(AimHold);
            for (int pose = 0; pose < 3; pose++) layout.Pose(FireHold);
            for (int shot = 0; shot < 3; shot++)
            {
                layout.Word(right ? (ushort)SpacePirateInstruction.PirateWalking_FireLaserRightWithYOffsetInY
                    : (ushort)SpacePirateInstruction.PirateWalking_FireLaserLeftWithYOffsetInY);
                layout.Word(unchecked((ushort)(shot == 0 ? OuterShotOffset : shot == 1 ? MiddleShotOffset : -OuterShotOffset)));
                layout.Pose(shot == 1 ? AimHold : FireHold);
            }
            for (int pose = 0; pose < 3; pose++) layout.Pose(FireHold);
            layout.Word((ushort)SpacePirateInstruction.PirateWalking_ChooseAMovement);

            layout.InstallFunction(WalkingSpacePirateFunction.AnimationOwnedNoOperation);
            for (int pose = 0; pose < 5; pose++) layout.Pose((pose & 1) == 0 ? LookHold : WalkHold);
            layout.Pose(FireHold);
            layout.Goto(right ? WalkingLeft : WalkingRight);
        }
        return layout.Result;
    }

    /// <summary>Walks variable-width poses and commands while selecting a mechanics word or visual operand.</summary>
    private ref struct Layout(int requested, bool visual, ushort start)
    {
        /// <summary>Current bank-$B2 address in the emitted instruction stream.</summary>
        private ushort cursor = start;
        /// <summary>Number of visited control words.</summary>
        private int mechanics;
        /// <summary>Number of visited spritemap operands.</summary>
        private int presentation;
        /// <summary>Selected address/value pair, with placeholder value for visual operands.</summary>
        internal InstructionMechanicsWord Result { get; private set; }

        /// <summary>Emits a native control word and captures the requested mechanics ordinal.</summary>
        internal void Word(ushort value)
        {
            if (!visual && mechanics == requested) Result = new(cursor, value);
            mechanics++;
            cursor += 2;
        }
        /// <summary>Emits a timed pose followed by a separately owned visual operand.</summary>
        internal void Pose(ushort duration)
        {
            Word(duration);
            if (visual && presentation == requested) Result = new(cursor, 0);
            presentation++;
            cursor += 2;
        }
        /// <summary>Emits the function-change callback and its bank-$B2 AI pointer.</summary>
        internal void InstallFunction(WalkingSpacePirateFunction function)
        {
            Word((ushort)SpacePirateInstruction.PirateWalking_FunctionInY);
            Word((ushort)function);
        }
        /// <summary>Emits a native branch opcode and its destination list pointer.</summary>
        internal void Goto(ushort target)
        {
            Word((ushort)CommonEnemyInstruction.Goto);
            Word(target);
        }
    }

    /// <summary>Finds a compiled bank-$B2 control word by address and rejects presentation-only or unknown words.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        int low = 0, high = MechanicsWordCount - 1;
        while (low <= high)
        {
            int middle = low + ((high - low) >> 1);
            var word = MechanicsWord(middle);
            if (word.Address == address) return word.Value;
            if (word.Address < address) low = middle + 1;
            else high = middle - 1;
        }
        throw new InvalidDataException($"Walking Space Pirate instruction mechanics pointer $B2:{address:X4} is not compiled.");
    }
}
