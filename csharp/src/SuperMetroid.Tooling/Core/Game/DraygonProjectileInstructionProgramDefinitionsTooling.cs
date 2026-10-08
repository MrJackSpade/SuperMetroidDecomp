using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonProjectileInstructionProgramDefinitions))]
internal abstract class DraygonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 38;
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
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        DraygonProjectileInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
