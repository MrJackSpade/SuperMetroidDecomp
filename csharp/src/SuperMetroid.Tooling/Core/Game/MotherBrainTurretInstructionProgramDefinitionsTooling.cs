using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainTurretInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MotherBrainTurretInstructionProgramDefinitions))]
internal abstract class MotherBrainTurretInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Drawing operands in Mother Brain's turret and bullet pose lists.</summary>
    public static int PresentationWordCount => MotherBrainTurretInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a turret or bullet pose operand by catalog index.</summary>
    public static ushort PresentationWordAddress(int index) => MotherBrainTurretInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Compiled words spanning turret poses, bullet selection, and bullet hit behavior.</summary>
    public static int MechanicsWordCount => 49;
    /// <summary>Maps a mechanics index into the turret, selector, bullet pose, or touch/shot program.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount)
            throw new IndexOutOfRangeException();
        if (index < 16)
            return PoseWord(MotherBrainTurretInstructionProgramDefinitions.TurretLeft, index);
        if (index == 16)
            return new(MotherBrainTurretInstructionProgramDefinitions.BulletSelector, (ushort)EnemyProjectileInstruction.MotherBrainsTurretBullets_GotoY);
        if (index < 25)
            return new((ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletSelector + 2 + 2 * (index - 17)), (ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletLeft + 6 * (index - 17)));
        if (index < 41)
            return PoseWord(MotherBrainTurretInstructionProgramDefinitions.BulletLeft, index - 25);
        return index switch
        {
            41 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot, (ushort)EnemyProjectileInstruction.UsePalette0),
            42 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 2, (ushort)EnemyProjectileInstruction.ClearPreInstruction),
            48 => new(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 24, (ushort)EnemyProjectileInstruction.Delete),
            _ => new((ushort)(MotherBrainTurretInstructionProgramDefinitions.BulletTouchOrShot + 4 + 4 * (index - 43)), index == 47 ? (ushort)32 : (ushort)8),
        };
    }
    /// <summary>Checks both possible word starts for a bank-$86 projectile byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == EnemyProjectileCodePointers.BankBase &&
        (MotherBrainTurretInstructionProgramDefinitions.TryRead(unchecked((ushort)address), out _) ||
         MotherBrainTurretInstructionProgramDefinitions.TryRead(unchecked((ushort)(address - 1)), out _));
    /// <summary>Expands a turret or bullet pose into its one-frame duration and sleep command.</summary>
    internal static InstructionMechanicsWord PoseWord(ushort first, int index) =>
        new((ushort)(first + 6 * (index / 2) + 4 * (index % 2)),
            index % 2 == 0 ? (ushort)1 : (ushort)EnemyProjectileInstruction.Sleep);
}
