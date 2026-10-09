using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonProjectileInstructionProgramDefinitions))]
internal abstract class DraygonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of presentation selector operands in the compiled Draygon projectile lists.</summary>
    public static int PresentationWordCount => DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank address of one compiled projectile sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of fixed timing and control operands in the compiled projectile lists.</summary>
    public static int MechanicsWordCount => 38;

    /// <summary>Maps a flattened mechanics slot to its instruction address and fixed operand.</summary>
    /// <param name="index">The zero-based slot across the compiled Draygon projectile programs.</param>
    /// <returns>The address and cartridge-defined value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        int address = index switch
        {
            0 => DraygonProjectileInstructionProgramDefinitions.GoopTouch,
            < 7 => DraygonProjectileInstructionProgramDefinitions.Goop + (index - 1) * 4,
            < 10 => DraygonProjectileInstructionProgramDefinitions.Goop + 24 + (index - 7) * 2,
            < 12 => DraygonProjectileInstructionProgramDefinitions.GoopShot + (index - 10) * 4,
            < 16 => DraygonProjectileInstructionProgramDefinitions.GoopShot + 8 + (index - 12) * 2,
            < 33 => DraygonProjectileInstructionProgramDefinitions.WallTurretBloom + (index - 16) * 4,
            < 36 => DraygonProjectileInstructionProgramDefinitions.WallTurretFlight + (index - 33) * 4,
            _ => DraygonProjectileInstructionProgramDefinitions.WallTurretFlight + 12 + (index - 36) * 2,
        };
        return new((ushort)address, DraygonProjectileInstructionProgramDefinitions.ReadMechanicsWord((ushort)address));
    }
    /// <summary>Checks whether an address points to either byte of a compiled mechanics word.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank and aligned word belong to the compiled mechanics catalog.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        DraygonProjectileInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
