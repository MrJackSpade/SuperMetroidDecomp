using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FallingSparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FallingSparkInstructionProgramDefinitions))]
internal abstract class FallingSparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => FallingSparkInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 17;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 3 ? (ushort)(FallingSparkInstructionProgramDefinitions.Falling + index * 4)
            : index < 5 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor - 4 + (index - 3) * 2)
            : index < 16 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + (index - 5) * 4) : FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete;
        return new(address, FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (SnesAddressMath.OffsetInBank(address)) - FallingSparkInstructionProgramDefinitions.Falling;
        return offset >= 0 && FallingSparkInstructionProgramDefinitions.TryRead((ushort)(FallingSparkInstructionProgramDefinitions.Falling + (offset & ~1)), out _);
    }
}
