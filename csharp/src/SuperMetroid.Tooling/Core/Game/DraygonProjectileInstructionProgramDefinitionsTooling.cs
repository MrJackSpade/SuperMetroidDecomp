using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonProjectileInstructionProgramDefinitions))]
internal abstract class DraygonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts artwork selections in Draygon goop and wall-turret projectile programs.</summary>
    public static int PresentationWordCount => DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a goop or wall-turret spritemap operand for resource validation.</summary>
    public static ushort PresentationWordAddress(int index) => DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>The goop touch, animated goop, shot response, and wall turrets contribute 38 control words.</summary>
    public static int MechanicsWordCount => 38;
    /// <summary>Maps an index across Draygon's goop and wall-turret lists, skipping their visual operands.</summary>
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
    /// <summary>Accepts a bank-$86 address only when its aligned word is recognized by Draygon's compiled control reader.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == EnemyProjectileCodePointers.BankBase &&
        DraygonProjectileInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
