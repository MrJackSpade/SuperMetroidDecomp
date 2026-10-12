using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DraygonProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DraygonProjectileInstructionProgramDefinitions))]
internal abstract class DraygonProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => DraygonProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => DraygonProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => 38;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
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
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == EnemyProjectileCodePointers.BankBase &&
        DraygonProjectileInstructionProgramDefinitions.TryRead((ushort)(address & ~1), out _);
}
