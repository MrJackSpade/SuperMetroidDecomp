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

    private readonly record struct ProgramDefinition(ushort Start, bool Sound, bool Jumping)
    {
        internal int PrefixWords => Sound ? 3 : 1;
        internal int PoseCount => Jumping ? 1 : 4;
        internal int WordCount => PrefixWords + PoseCount + (Jumping ? 1 : 2);
    }

    // Four species/size groups each contain airborne and landed programs for both orientations.
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

    public static int MechanicsWordCount => 96;
    public static int PresentationWordCount => 40;

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

    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        int orientation = index / 5;
        int pose = index % 5;
        var program = ProgramAt(orientation * 2 + (pose == 0 ? 0 : 1));
        return (ushort)(program.Start + program.PrefixWords * 2 + (pose == 0 ? 0 : pose - 1) * 4 + 2);
    }

    internal static bool IsPresentationWord(ushort address)
    {
        for (int index = 0; index < PresentationWordCount; index++)
            if (PresentationWordAddress(index) == address) return true;
        return false;
    }

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
