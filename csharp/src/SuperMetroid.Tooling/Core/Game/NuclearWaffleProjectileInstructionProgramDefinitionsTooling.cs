using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NuclearWaffleProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NuclearWaffleProjectileInstructionProgramDefinitions))]
internal abstract class NuclearWaffleProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of sprite-selector operands supplied for the nuclear waffle projectile frames.</summary>
    public static int PresentationWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of one projectile-frame sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => NuclearWaffleProjectileInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Number of frame-duration and trailing control operands in the projectile program.</summary>
    public static int MechanicsWordCount => NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount + 2;

    /// <summary>Maps a mechanics slot to its instruction address and fixed operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and cartridge-defined value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        ushort address = (ushort)(NuclearWaffleProjectileInstructionProgramDefinitions.Initial + (index < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount ? index * 4 : NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + (index - NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount) * 2));
        return new(address, NuclearWaffleProjectileInstructionProgramDefinitions.ReadMechanicsWord(address));
    }
    /// <summary>Checks whether an address identifies a byte of a compiled duration or control operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address is part of a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = (ushort)address - NuclearWaffleProjectileInstructionProgramDefinitions.Initial;
        return (uint)offset < NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 + 4 && (offset >= NuclearWaffleProjectileInstructionProgramDefinitions.FrameCount * 4 || offset % 4 < 2);
    }
}
