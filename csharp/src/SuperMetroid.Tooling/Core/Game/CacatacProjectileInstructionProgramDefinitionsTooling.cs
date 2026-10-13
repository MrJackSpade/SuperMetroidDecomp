using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CacatacProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CacatacProjectileInstructionProgramDefinitions))]
internal abstract class CacatacProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Each of ten Cacatac spike directions selects a separate projectile spritemap.</summary>
    public static int PresentationWordCount => CacatacProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the interleaved visual operand for an upward or downward spike trajectory.</summary>
    public static ushort PresentationWordAddress(int index) => CacatacProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Each six-byte spike list contributes a one-tick pose and terminal sleep word.</summary>
    public static int MechanicsWordCount => 2 * CacatacProjectileInstructionProgramDefinitions.ProgramCount;
    /// <summary>Maps a spike direction to its one-tick hold or sleep instruction, skipping its visual operand.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        bool sleep = (index & 1) != 0;
        return new((ushort)(CacatacProjectileInstructionProgramDefinitions.LeftFacingUp + 6 * (index / 2) + (sleep ? 4 : 0)),
            sleep ? (ushort)EnemyProjectileInstruction.Sleep : (ushort)1);
    }
    /// <summary>Recognizes bank-$86 Cacatac duration and sleep bytes without claiming spritemap operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0x860000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < MechanicsWordCount; index++)
        {
            ushort wordAddress = MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
