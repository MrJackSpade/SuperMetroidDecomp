using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ShaktoolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ShaktoolInstructionProgramDefinitions))]
internal abstract class ShaktoolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 110;
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => 15;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
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
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index)
    {
        if ((uint)index >= PresentationWordCount) throw new IndexOutOfRangeException();
        if (index < 6) return (ushort)((index < 3 ? ShaktoolInstructionProgramDefinitions.SawHandPrimaryPiece : ShaktoolInstructionProgramDefinitions.SawHandFinalPiece) + (index % 3) * 4 + 2);
        if (index == 6) return ShaktoolInstructionProgramDefinitions.ArmPieceNormal + 2;
        return (ushort)(ShaktoolInstructionProgramDefinitions.HeadAimingLeft + (index - 7) * 8 + 2);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xaa0000) return false;
        ushort bankAddress = (ushort)(address & 0xfffe);
        return bankAddress >= ShaktoolInstructionProgramDefinitions.SawHandAttackPrimaryPiece && bankAddress < ShaktoolInstructionProgramDefinitions.FirstAdjacentCodeRoutine && !ShaktoolInstructionProgramDefinitions.IsPresentationWord(bankAddress);
    }
}
