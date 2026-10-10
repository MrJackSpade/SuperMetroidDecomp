using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="BombTorizoDroolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoDroolInstructionProgramDefinitions))]
internal abstract class BombTorizoDroolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    public static int PresentationWordCount => BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index);
    public static int MechanicsWordCount => 19;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        // Every two-byte word in the bounded program is control except the seven visuals.
        for (ushort address = BombTorizoDroolInstructionProgramDefinitions.StreamStart; address <= (ushort)BombTorizoDroolProgram.FloorImpact + 14; address += 2)
        {
            if (BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord(address)) continue;
            if (index-- == 0) return new(address, BombTorizoDroolInstructionProgramDefinitions.ReadMechanicsWord(address));
        }
        throw new InvalidOperationException("Drool program word count does not match its layout.");
    }
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = unchecked((ushort)address) - BombTorizoDroolInstructionProgramDefinitions.StreamStart;
        return (uint)offset < (ushort)BombTorizoDroolProgram.FloorImpact + 16 - BombTorizoDroolInstructionProgramDefinitions.StreamStart &&
            !BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)(BombTorizoDroolInstructionProgramDefinitions.StreamStart + (offset & ~1)));
    }
}
