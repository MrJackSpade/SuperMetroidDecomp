using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Puyo;

/// <summary>Development-tool members of <see cref="PuyoInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PuyoInstructionProgramDefinitions))]
internal abstract class PuyoInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Puyo's three grounded loops and five directional poses contribute 28 control words.</summary>
    public static int MechanicsWordCount => 28;
    /// <summary>The grounded and directional Puyo programs retain 17 visual operands.</summary>
    public static int PresentationWordCount => 17;
    /// <summary>Maps an index across grounded loops and directional poses, skipping their spritemap words.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int pointer;
        if (index < 18)
        {
            int local = index % 6;
            pointer = PuyoInstructionProgramDefinitions.GroundedFast + index / 6 * 20 + (local < 5 ? local * 4 : 18);
        }
        else pointer = PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 18) / 2 * 6 + (index % 2) * 4;
        return new((ushort)pointer, PuyoInstructionProgramDefinitions.ReadMechanicsWord((ushort)pointer));
    }
    /// <summary>Locates a grounded-loop or directional-pose visual operand in bank $A2.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        return (ushort)(index < 12 ? PuyoInstructionProgramDefinitions.GroundedFast + index / 4 * 20 + index % 4 * 4 + 2 :
            PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4 + (index - 12) * 6 + 2);
    }
    /// <summary>Matches Puyo timing and control bytes in bank $A2 while excluding interleaved spritemaps.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000) return false;
        int offset = (ushort)address - PuyoInstructionProgramDefinitions.GroundedFast;
        if ((uint)offset < 60) return offset % 20 >= 16 || offset % 20 % 4 < 2;
        offset = (ushort)address - PuyoInstructionProgramDefinitions.RightFrame0LeftFrame4;
        return (uint)offset < 30 && offset % 6 is not (2 or 3);
    }
}
