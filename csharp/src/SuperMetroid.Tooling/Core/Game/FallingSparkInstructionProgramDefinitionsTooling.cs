using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FallingSparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FallingSparkInstructionProgramDefinitions))]
internal abstract class FallingSparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => FallingSparkInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 17;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 3 ? (ushort)(FallingSparkInstructionProgramDefinitions.Falling + index * 4)
            : index < 5 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor - 4 + (index - 3) * 2)
            : index < 16 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + (index - 5) * 4) : FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete;
        return new(address, FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - FallingSparkInstructionProgramDefinitions.Falling;
        return offset >= 0 && FallingSparkInstructionProgramDefinitions.TryRead((ushort)(FallingSparkInstructionProgramDefinitions.Falling + (offset & ~1)), out _);
    }
}
