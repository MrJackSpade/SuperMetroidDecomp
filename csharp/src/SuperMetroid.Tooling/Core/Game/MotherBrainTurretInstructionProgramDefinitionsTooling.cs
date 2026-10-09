using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainTurretInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTurretInstructionProgramDefinitions))]
internal abstract class MotherBrainTurretInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of spritemap operands embedded in the turret and bullet instruction lists.</summary>
    public static int PresentationWordCount => MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of a presentation operand by its compiled table position.</summary>
    /// <param name="index">Zero-based position in the presentation-word table.</param>
    /// <returns>Address containing the selected spritemap operand.</returns>
    public static ushort PresentationWordAddress(int index) => MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of compiled mechanics address/value pairs for turret poses and projectile behavior.</summary>
    public static int MechanicsWordCount => 49;

    /// <summary>Returns one address/value pair from the turret and bullet mechanics tables.</summary>
    /// <param name="index">Zero-based position in the combined mechanics-word table.</param>
    /// <returns>The instruction address and the operand used by that instruction.</returns>
    /// <exception cref="IndexOutOfRangeException">The index is outside the compiled mechanics table.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 16)
            return PoseWord(MotherBrainTurretInstructionProgramDefinitions.TurretLeft, index);
        if (index == 16)
            return new(MotherBrainTurretInstructionProgramDefinitions.BulletSelector, EnemyProjectileCodePointers.Instruction_EnemyProjectile_MotherBrainsTurretBullets_GotoY);
        if (index < 25)
            return new((ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletSelector + 2 + 2 * (index - 17)), (ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletLeft + 6 * (index - 17)));
        if (index < 41)
            return PoseWord(MotherBrainTurretInstructionProgramDefinitions.BulletLeft, index - 25);
        return index switch
        {
            41 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot, EnemyProjectileCodePointers.Instruction_EnemyProjectile_UsePalette0),
            42 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 2, EnemyProjectileCodePointers.Instruction_EnemyProjectile_ClearPreInstruction),
            48 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 24, EnemyProjectileCodePointers.Instruction_EnemyProjectile_Delete),
            _ => new((ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 4 + 4 * (index - 43)), index == 47 ? (ushort)32 : (ushort)8),
        };
    }
    /// <summary>Checks whether a byte address overlaps either byte of a compiled enemy-projectile mechanics word.</summary>
    /// <param name="address">Full cartridge address to inspect.</param>
    /// <returns><see langword="true"/> when the address belongs to the enemy-projectile bank and a compiled word.</returns>
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        (MotherBrainTurretInstructionProgramDefinitions.TryRead(unchecked((ushort)address), out _) ||
         MotherBrainTurretInstructionProgramDefinitions.TryRead(unchecked((ushort)(address - 1)), out _));
    /// <summary>Builds one instruction operand from the two-word pose records used by turrets and their bullets.</summary>
    /// <param name="first">Address of the first instruction word in the pose sequence.</param>
    /// <param name="index">Word position; each pose contributes a duration word followed by its sleep operation.</param>
    /// <returns>The address/value pair for the selected pose word.</returns>
    internal static InstructionMechanicsWord PoseWord(ushort first, int index) =>
        new((ushort)(first + 6 * (index / 2) + 4 * (index % 2)),
            index % 2 == 0 ? (ushort)1 : EnemyProjectileCodePointers.Instruction_EnemyProjectile_Sleep);
}
