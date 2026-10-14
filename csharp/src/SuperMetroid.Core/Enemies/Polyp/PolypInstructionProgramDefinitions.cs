using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Polyp;

/// <summary>
/// Compiled one-pose sleep program for Polyp; its visual selector uses installed artwork.
/// </summary>
internal abstract class PolypInstructionProgramDefinitions
{
    /// <summary><c>InstList_Polyp</c> at $A2:B51A.</summary>
    internal const ushort Stationary = 0xb51a;

    /// <summary>The compiled <c>Spritemap_Polyp</c> operand at $A2:B51C.</summary>
    internal const ushort PresentationWord = 0xb51c;

    /// <summary><c>Spritemap_Polyp</c> at $A2:B5FB, selected by the one stationary pose at $B51C.</summary>
    internal const ushort StationaryFrame = 0xb5fb;

    /// <summary>Resolves only the stationary pose's compiled visual operand to its installed frame.</summary>
    internal static ushort FrameAt(ushort operandAddress) =>
        operandAddress == PresentationWord ? StationaryFrame
            : throw new InvalidDataException("Polyp visual operand is outside its stationary program.");

    /// <summary>The stationary program has a one-tick hold followed by a sleep opcode.</summary>
    public static int MechanicsWordCount => 2;
    /// <summary>Exposes the hold or sleep word at its native bank-$A2 address for audits.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new ArgumentOutOfRangeException(nameof(index));
        return new((ushort)(Stationary + index * 4),
            index == 0 ? (ushort)1 : (ushort)CommonEnemyInstruction.Sleep);
    }

    /// <summary>Reads a compiled hold or sleep word while rejecting the intervening visual operand.</summary>
    internal static ushort ReadMechanicsWord(ushort address)
    {
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            if (MechanicsWord(index).Address == address)
                return MechanicsWord(index).Value;
        }
        throw new InvalidDataException(
            $"Polyp instruction mechanics pointer $A2:{address:X4} is not compiled.");
    }
}
