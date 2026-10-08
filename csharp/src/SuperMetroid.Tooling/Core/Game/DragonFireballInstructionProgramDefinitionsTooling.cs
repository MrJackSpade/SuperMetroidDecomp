using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DragonFireballInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DragonFireballInstructionProgramDefinitions))]
internal abstract class DragonFireballInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => DragonFireballInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => DragonFireballInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 16;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        int step = index % 4;
        ushort address = (ushort)(DragonFireballInstructionProgramDefinitions.RisingLeft + index / 4 * 12 + (step < 3 ? step * 4 : 10));
        return new(address, DragonFireballInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    public static bool IsCompiledMechanicsByte(int address) =>
        (address & 0xff0000) == EnemyProjectileCodePointers.BankBase &&
        (DragonFireballInstructionProgramDefinitions.TryRead((ushort)address, out _) || DragonFireballInstructionProgramDefinitions.TryRead((ushort)address - 1, out _));
}
