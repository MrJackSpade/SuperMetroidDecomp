using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleProjectileInstructionProgramDefinitions))]
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>One installed spritemap selection accompanies each of twelve three-tick poses.</summary>
    public static int PresentationWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the visual operand after a Puromi body-loop duration in bank $86.</summary>
    public static ushort PresentationWordAddress(int index) => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Counts twelve pose holds plus the terminal goto opcode and target.</summary>
    public static int MechanicsWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount + 2;
    /// <summary>Reads a timed pose or goto control from its exact bank-relative position.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + (index < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Claims only duration and terminal goto bytes in the $86:BB5E Puromi loop.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - NuclearWaffleProjectileInstructionProgramDefinitions.Initial;
        return (uint)offset < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
