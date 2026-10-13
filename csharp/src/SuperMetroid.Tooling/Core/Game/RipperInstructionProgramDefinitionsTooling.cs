using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RipperInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RipperInstructionProgramDefinitions))]
internal abstract class RipperInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Six Ripper lists with four frame holds and a closing goto pair each.</summary>
    public static int MechanicsWordCount => 36;
    /// <summary>Four drawing operands in each of the six Ripper lists.</summary>
    public static int PresentationWordCount => 24;
    /// <summary>Reconstructs a hold or goto word from its list start and record position.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        ushort start = RipperInstructionProgramDefinitions.ProgramStart(index / 6);
        int record = index % 6;
        int offset = record < 4 ? record * 4 : 16 + 2 * (record - 4);
        ushort value = record < 4 ? RipperInstructionProgramDefinitions.VisualHold(record) :
            record == 4 ? (ushort)CommonEnemyInstruction.Goto : start;
        return new((ushort)(start + offset), value);
    }
    /// <summary>Locates the drawing operand paired with a Ripper frame hold.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount)
            throw new IndexOutOfRangeException();
        return (ushort)(RipperInstructionProgramDefinitions.ProgramStart(index / 4) + 4 * (index % 4) + 2);
    }
    /// <summary>Recognizes either byte of a compiled Ripper word in bank $A2.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa20000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        return RipperInstructionProgramDefinitions.TryRead(bankAddress, out _) || RipperInstructionProgramDefinitions.TryRead(unchecked((ushort)(bankAddress - 1)), out _);
    }
}
