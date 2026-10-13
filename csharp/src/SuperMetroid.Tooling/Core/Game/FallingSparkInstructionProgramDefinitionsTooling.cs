using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FallingSparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FallingSparkInstructionProgramDefinitions))]
internal abstract class FallingSparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Bounds the visual operands in the spark's fall and floor-impact programs.</summary>
    public static int PresentationWordCount => FallingSparkInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a visual operand for the native projectile-program audit.</summary>
    public static ushort PresentationWordAddress(int index) => FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Includes falling commands, floor-impact commands, and the terminal delete.</summary>
    public static int MechanicsWordCount => 17;
    /// <summary>Walks the falling program, the hit-floor prelude, and the final delete word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 3 ? (ushort)(FallingSparkInstructionProgramDefinitions.Falling + index * 4)
            : index < 5 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor - 4 + (index - 3) * 2)
            : index < 16 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + (index - 5) * 4) : FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete;
        return new(address, FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks compiled mechanic bytes in the projectile bank by aligned word lookup.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (SnesAddressMath.OffsetInBank(address)) - FallingSparkInstructionProgramDefinitions.Falling;
        return offset >= 0 && FallingSparkInstructionProgramDefinitions.TryRead((ushort)(FallingSparkInstructionProgramDefinitions.Falling + (offset & ~1)), out _);
    }
}
