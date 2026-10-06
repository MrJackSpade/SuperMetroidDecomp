namespace SuperMetroid.Core.Game;

/// <summary>One compiled mechanics-owned word at its native bank-$A3 address.</summary>
internal readonly record struct HopperInstructionMechanicsWord(
    ushort Address,
    ushort Value);

/// <summary>
/// Compiled engine-control words for Sidehopper and Dessgeega floor/ceiling animation
/// programs. Their forty interleaved spritemap operands select installed artwork;
/// the hop physics, sound, and instruction cadence remain compiled here.
/// </summary>
internal static class HopperInstructionProgramDefinitions
{
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideUp</c> at $A3:AA76.</summary>
    internal const ushort SidehopperJumpingFloor = 0xaa76;
    /// <summary><c>InstList_Sidehopper_Landed_UpsideUp</c> at $A3:AA82.</summary>
    internal const ushort SidehopperLandedFloor = 0xaa82;
    /// <summary><c>InstList_Sidehopper_Hopping_UpsideDown</c> at $A3:AA9C.</summary>
    internal const ushort SidehopperJumpingCeiling = 0xaa9c;
    /// <summary><c>InstList_Sidehopper_Landed_UpsideDown</c> at $A3:AAA8.</summary>
    internal const ushort SidehopperLandedCeiling = 0xaaa8;

    /// <summary><c>InstList_Dessgeega_Hopping_UpsideUp</c> at $A3:AFA5.</summary>
    internal const ushort DessgeegaJumpingFloor = 0xafa5;
    /// <summary><c>InstList_Dessgeega_Landed_UpsideUp</c> at $A3:AFAD.</summary>
    internal const ushort DessgeegaLandedFloor = 0xafad;
    /// <summary><c>InstList_Dessgeega_Hopping_UpsideDown</c> at $A3:AFC3.</summary>
    internal const ushort DessgeegaJumpingCeiling = 0xafc3;
    /// <summary><c>InstList_Dessgeega_Landed_UpsideDown</c> at $A3:AFCB.</summary>
    internal const ushort DessgeegaLandedCeiling = 0xafcb;

    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideUp</c> at $A3:B0C5.</summary>
    internal const ushort LargeSidehopperJumpingFloor = 0xb0c5;
    /// <summary><c>InstList_SidehopperLarge_Landed_UpsideUp</c> at $A3:B0D1.</summary>
    internal const ushort LargeSidehopperLandedFloor = 0xb0d1;
    /// <summary><c>InstList_SidehopperLarge_Hopping_UpsideDown</c> at $A3:B0EB.</summary>
    internal const ushort LargeSidehopperJumpingCeiling = 0xb0eb;
    /// <summary><c>InstList_SidehopperLarge_Landed_UpsideDown</c> at $A3:B0F7.</summary>
    internal const ushort LargeSidehopperLandedCeiling = 0xb0f7;

    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideUp</c> at $A3:B237.</summary>
    internal const ushort LargeDessgeegaJumpingFloor = 0xb237;
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideUp</c> at $A3:B23F.</summary>
    internal const ushort LargeDessgeegaLandedFloor = 0xb23f;
    /// <summary><c>InstList_DessgeegaLarge_Hopping_UpsideDown</c> at $A3:B255.</summary>
    internal const ushort LargeDessgeegaJumpingCeiling = 0xb255;
    /// <summary><c>InstList_DessgeegaLarge_Landed_UpsideDown</c> at $A3:B25D.</summary>
    internal const ushort LargeDessgeegaLandedCeiling = 0xb25d;

    /// <summary>The final hopper physics-table word immediately before the first program.</summary>
    internal const ushort LastAdjacentPhysicsWord = 0xaa74;

    /// <summary>$A3:AA7A/AAA0/B0C9/B0EF: Sidehopper airborne sound in library 2.</summary>
    private const ushort JumpSound = 0x005d;
    /// <summary>$A3:AA86/AAAC/B0D5/B0FB: Sidehopper landing sound in library 2.</summary>
    private const ushort LandSound = 0x005e;
    /// <summary>$A3:AA7C and all airborne programs: independent one-tick pose hold, pending disposition.</summary>
    private const ushort UnresolvedAirborneHold = 1;
    /// <summary>$A3:AA88/AA90 and each landed program: independent two-tick first/third pose hold, pending disposition.</summary>
    private const ushort UnresolvedLandingOuterHold = 2;
    /// <summary>$A3:AA8C and each landed program: independent second pose hold, pending disposition.</summary>
    private const ushort UnresolvedLandingMiddleHold = 5;
    /// <summary>$A3:AA94 and each landed program: independent last pose hold before ReadyToHop, pending disposition.</summary>
    private const ushort UnresolvedLandingFinalHold = 3;

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

    internal static int MechanicsWordCount => 96;
    internal static int PresentationWordCount => 40;

    internal static HopperInstructionMechanicsWord MechanicsWord(int index)
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

    private static HopperInstructionMechanicsWord Word(ProgramDefinition program, int index)
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
            ushort hold = program.Jumping ? UnresolvedAirborneHold : pose switch
            {
                0 or 2 => UnresolvedLandingOuterHold,
                1 => UnresolvedLandingMiddleHold,
                _ => UnresolvedLandingFinalHold,
            };
            return new((ushort)(poseStart + pose * 4), hold);
        }
        int tail = pose - program.PoseCount;
        return new((ushort)(poseStart + program.PoseCount * 4 + tail * 2),
            !program.Jumping && tail == 0 ? EnemyInstructionCodePointers.Instruction_Hopper_ReadyToHop
                : CommonEnemyInstructionCodes.Sleep);
    }

    internal static ushort PresentationWordAddress(int index)
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

    internal static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa30000) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1))) return true;
        }
        return false;
    }
}
