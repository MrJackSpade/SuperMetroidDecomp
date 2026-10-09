namespace SuperMetroid.Core.Game;

/// <summary>
/// Compiled engine-control words for Sidehopper and Dessgeega floor/ceiling animation
/// programs. Their forty interleaved spritemap operands select installed artwork;
/// the hop physics, sound, and instruction cadence remain compiled here.
/// </summary>
internal abstract class HopperInstructionProgramDefinitions
{
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideUp</c> at $A3:AA76.</summary>
    internal const ushort SidehopperJumpingFloor = 0xaa76;

    /// <summary><c>InstList_Dessgeega_Hopping_UpsideUp</c> at $A3:AFA5.</summary>
    internal const ushort DessgeegaJumpingFloor = 0xafa5;

    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideUp</c> at $A3:B0C5.</summary>
    internal const ushort LargeSidehopperJumpingFloor = 0xb0c5;

    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideUp</c> at $A3:B237.</summary>
    internal const ushort LargeDessgeegaJumpingFloor = 0xb237;

    /// <summary>$A3:AA7A/AAA0/B0C9/B0EF: Sidehopper airborne sound in library 2.</summary>
    private const ushort JumpSound = 0x005d;
    /// <summary>$A3:AA86/AAAC/B0D5/B0FB: Sidehopper landing sound in library 2.</summary>
    private const ushort LandSound = 0x005e;
    /// <summary>$A3:AA7C and all airborne programs: one-tick pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort AirborneHold = 1;
    /// <summary>$A3:AA88/AA90 and each landed program: two-tick first/third pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort LandingOuterHold = 2;
    /// <summary>$A3:AA8C and each landed program: second pose hold. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort LandingMiddleHold = 5;
    /// <summary>$A3:AA94 and each landed program: last pose hold before ReadyToHop. Reviewed under #1165 as authored animation cadence: the interpreter loads it into the instruction timer and no simulation quantity derives it.</summary>
    private const ushort LandingFinalHold = 3;

    /// <summary>Describes one authored hopper program's starting address, sound prefix, and airborne or landed phase.</summary>
    /// <param name="Start">Native bank-$A3 address at which this program begins.</param>
    /// <param name="Sound">Whether two sound-related words follow the off-screen-processing control word.</param>
    /// <param name="Jumping">Whether the program is airborne and uses the single-pose hop cadence.</param>
    private readonly record struct ProgramDefinition(ushort Start, bool Sound, bool Jumping)
    {
        /// <summary>Number of words before the program's frame poses, including any sound callback operands.</summary>
        internal int PrefixWords => Sound ? 3 : 1;
        /// <summary>Number of timed poses: one airborne pose or four landed poses.</summary>
        internal int PoseCount => Jumping ? 1 : 4;
        /// <summary>Total number of mechanics words, including prefix, poses, and the phase-specific tail.</summary>
        internal int WordCount => PrefixWords + PoseCount + (Jumping ? 1 : 2);
    }

    // Four species/size groups each contain airborne and landed programs for both orientations.
    /// <summary>Resolves one of the sixteen compiled programs from its grouped species, size, phase, and orientation index.</summary>
    /// <param name="index">Zero-based program index across the four species/size groups.</param>
    /// <returns>Definition containing the program start address and its sound and movement-phase traits.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the sixteen compiled programs.</exception>
    private static ProgramDefinition ProgramAt(int index)
    {
        if ((uint)index >= 16) throw new IndexOutOfRangeException();
        int group = index / 4;
        ushort start = group switch
        {
            0 => SidehopperJumpingFloor,
            1 => DessgeegaJumpingFloor,
            2 => LargeSidehopperJumpingFloor,
            _ => LargeDessgeegaJumpingFloor,
        };
        bool sound = (group & 1) == 0;
        bool jumping = (index & 1) == 0;
        int airborneBytes = sound ? 12 : 8;
        int landedBytes = sound ? 26 : 22;
        return new((ushort)(start + (index % 4 / 2) * (airborneBytes + landedBytes) +
            (jumping ? 0 : airborneBytes)), sound, jumping);
    }

    /// <summary>$A3:AAC2-AAE1 maps size/species, orientation and movement phase to its executable program.</summary>
    internal static ushort InstructionList(ushort variant, bool upsideDown, bool jumping)
    {
        int group = variant switch
        {
            0 => 0, // Small Sidehopper.
            1 => 2, // Large Sidehopper.
            2 => 3, // Large Dessgeega.
            3 => 1, // Small Dessgeega.
            _ => throw new InvalidDataException($"Hopper animation variant {variant} exceeds four authored records."),
        };
        return ProgramAt(group * 4 + (upsideDown ? 2 : 0) + (jumping ? 0 : 1)).Start;
    }

    /// <summary>Total number of instruction, callback, timing, and flow-control words across all hopper programs.</summary>
    public static int MechanicsWordCount => 96;
    /// <summary>Total number of interleaved spritemap operands selected from installed presentation artwork.</summary>
    public static int PresentationWordCount => 40;

    /// <summary>Returns one indexed mechanics word from the concatenated hopper program streams.</summary>
    /// <param name="index">Zero-based index in the 96-word mechanics stream.</param>
    /// <returns>The native address and value of the selected mechanics word.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics stream.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int programIndex = 0; programIndex < 16; programIndex++)
        {
            var program = ProgramAt(programIndex);
            if (index < program.WordCount) return Word(program, index);
            index -= program.WordCount;
        }
        throw new IndexOutOfRangeException();
    }

    /// <summary>Builds one mechanics entry from a program's sound prefix, pose timing, or control-flow tail.</summary>
    /// <param name="program">Program traits used to select the word's address and value.</param>
    /// <param name="index">Zero-based word offset within the program.</param>
    /// <returns>The compiled address/value pair at that offset.</returns>
    private static InstructionMechanicsWord Word(ProgramDefinition program, int index)
    {
        if (index == 0)
            return new(program.Start, program.Jumping ? CommonEnemyInstructionCodes.EnableOffScreenProcessing
                : CommonEnemyInstructionCodes.DisableOffScreenProcessing);
        if (program.Sound && index < program.PrefixWords)
            return new((ushort)(program.Start + index * 2), index == 1
                ? EnemyInstructionCodePointers.Instruction_Sidehopper_QueueSoundInY_Lib2_Max3
                : program.Jumping ? JumpSound : LandSound);
        int pose = index - program.PrefixWords;
        int poseStart = program.Start + program.PrefixWords * 2;
        if (pose < program.PoseCount)
        {
            ushort hold = program.Jumping ? AirborneHold : pose switch
            {
                0 or 2 => LandingOuterHold,
                1 => LandingMiddleHold,
                _ => LandingFinalHold,
            };
            return new((ushort)(poseStart + pose * 4), hold);
        }
        int tail = pose - program.PoseCount;
        return new((ushort)(poseStart + program.PoseCount * 4 + tail * 2),
            !program.Jumping && tail == 0 ? EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop
                : CommonEnemyInstructionCodes.Sleep);
    }

    /// <summary>Returns the native address of one of the interleaved spritemap operands across hopper programs.</summary>
    /// <param name="index">Zero-based index among the forty presentation words.</param>
    /// <returns>Bank-$A3 address containing the selected spritemap operand.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the presentation word sequence.</exception>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int orientation = index / 5;
        int pose = index % 5;
        var program = ProgramAt(orientation * 2 + (pose == 0 ? 0 : 1));
        return (ushort)(program.Start + program.PrefixWords * 2 + (pose == 0 ? 0 : pose - 1) * 4 + 2);
    }

    /// <summary>Determines whether an address is one of the compiled spritemap operand locations.</summary>
    /// <param name="address">Bank-$A3 instruction address to classify.</param>
    /// <returns>True when the address selects a presentation word rather than mechanics.</returns>
    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }

    /// <summary>Resolves a native mechanics address to its compiled instruction, timing, sound, or flow-control word.</summary>
    /// <param name="address">Bank-$A3 address to read from the compiled program set.</param>
    /// <returns>The mechanics word stored at that address.</returns>
    /// <exception cref="InvalidDataException">No compiled mechanics word has the requested address.</exception>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            var word = MechanicsWord(index);
            if (word.Address == address) return word.Value;
        }
        throw new InvalidDataException($"Hopper instruction mechanics pointer $A3:{address:X4} is not compiled.");
    }
}
