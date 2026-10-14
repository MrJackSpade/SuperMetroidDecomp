using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Enemies.Projectiles;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="BombTorizoDroolInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(BombTorizoDroolInstructionProgramDefinitions))]
internal abstract class BombTorizoDroolInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts the seven visual operand words excluded from the Bomb Torizo drool mechanics stream.</summary>
    public static int PresentationWordCount => BombTorizoDroolInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an indexed visual operand in the drool instruction stream.</summary>
    public static ushort PresentationWordAddress(int index) => BombTorizoDroolInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Counts the control words between drool stream start and floor impact, excluding visuals.</summary>
    public static int MechanicsWordCount => 19;
    /// <summary>Walks the bounded drool stream to return an indexed control word and its native address.</summary>
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
    /// <summary>Accepts control-word bytes within the drool stream while excluding seven presentation words.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase) return false;
        int offset = unchecked((ushort)address) - BombTorizoDroolInstructionProgramDefinitions.StreamStart;
        return (uint)offset < (ushort)BombTorizoDroolProgram.FloorImpact + 16 - BombTorizoDroolInstructionProgramDefinitions.StreamStart &&
            !BombTorizoDroolInstructionProgramDefinitions.IsPresentationWord((ushort)(BombTorizoDroolInstructionProgramDefinitions.StreamStart + (offset & ~1)));
    }
}
