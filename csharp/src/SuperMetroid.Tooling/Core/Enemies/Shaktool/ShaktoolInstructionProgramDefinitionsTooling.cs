using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Shaktool;

/// <summary>Development-tool members of <see cref="ShaktoolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolInstructionProgramDefinitions))]
internal abstract class ShaktoolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Compiled words from the saw-hand attack through the last Shaktool list.</summary>
    public static int MechanicsWordCount => 110;
    /// <summary>Six saw-hand, one arm, and eight head-aim drawing operands.</summary>
    public static int PresentationWordCount => 15;
    /// <summary>Walks Shaktool's contiguous instruction region, skipping presentation operands.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece; address < ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine; address += 2)
        {
            int value = ShaktoolInstructionProgramDefinitions.ProgramWord((ushort)address);
            if (value != ShaktoolInstructionProgramDefinitions.PresentationOperand && index-- == 0) return new((ushort)address, (ushort)value);
        }
        throw new InvalidOperationException("Shaktool mechanics-word index is inconsistent.");
    }
    /// <summary>Locates a saw-hand, arm, or head-aim presentation word.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 6) return (ushort)((index < 3 ? ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece : ShaktoolInstructionProgramDefinitions.SawHandFinalPiece) + (index % 3) * 4 + 2);
        if (index == 6) return ShaktoolInstructionProgramDefinitions.ArmPieceNormal + 2;
        return (ushort)(ShaktoolInstructionProgramDefinitions.HeadAimingLeft + (index - 7) * 8 + 2);
    }
    /// <summary>Classifies bank-$AA instruction bytes outside the fifteen drawing operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000) return false;
        ushort bankAddress = (ushort)(address & 0xfffe);
        return bankAddress >= ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece && bankAddress < ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine && !ShaktoolInstructionProgramDefinitions.IsPresentationWord(bankAddress);
    }
}
