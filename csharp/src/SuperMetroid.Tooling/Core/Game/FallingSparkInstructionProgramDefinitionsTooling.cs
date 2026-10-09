using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="FallingSparkInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(FallingSparkInstructionProgramDefinitions))]
internal abstract class FallingSparkInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of live spritemap operands in the falling-spark animation programs.</summary>
    public static int PresentationWordCount => FallingSparkInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the cartridge address of a presentation operand selected by ordinal.</summary>
    /// <param name="index">Zero-based index among the compiled spritemap operands.</param>
    /// <returns>The bank-relative operand address.</returns>
    public static ushort PresentationWordAddress(int index) => FallingSparkInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled instruction words across the falling and floor-impact sequences.</summary>
    public static int MechanicsWordCount => 17;

    /// <summary>Resolves a mechanics ordinal to its instruction address and compiled cartridge value.</summary>
    /// <param name="index">Zero-based index across the fall and floor-impact control words.</param>
    /// <returns>The bank-relative address and decoded instruction word.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the 17 compiled words.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = index < 3 ? (ushort)(FallingSparkInstructionProgramDefinitions.Falling + index * 4)
            : index < 5 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor - 4 + (index - 3) * 2)
            : index < 16 ? (ushort)(FallingSparkInstructionProgramDefinitions.HitFloor + (index - 5) * 4) : FallingSparkInstructionProgramDefinitions.HitFloorTerminalDelete;
        return new(address, FallingSparkInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Classifies bytes in the enemy-projectile bank that belong to a recognized mechanics word.</summary>
    /// <param name="address">Full SNES address of the candidate byte.</param>
    /// <returns><see langword="true"/> when the bank and aligned word are part of the compiled program.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (address & 0xffff) - FallingSparkInstructionProgramDefinitions.Falling;
        return offset >= 0 && FallingSparkInstructionProgramDefinitions.TryRead((ushort)(FallingSparkInstructionProgramDefinitions.Falling + (offset & ~1)), out _);
    }
}
