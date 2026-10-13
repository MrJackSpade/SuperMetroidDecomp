using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="NoobTubeProjectileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(NoobTubeProjectileInstructionProgramDefinitions))]
internal abstract class NoobTubeProjectileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>The tube crack, flying glass, and released-air lists expose their compiled callbacks and timing words.</summary>
    public static int MechanicsWordCount => NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Exposes an indexed control word for crack, shard, or bubble playback.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Counts the interleaved spritemap selections for the tube crack, glass shards, and bubbles.</summary>
    public static int PresentationWordCount => NoobTubeProjectileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a tube projectile's presentation operand for resource checks.</summary>
    public static ushort PresentationWordAddress(int index) => NoobTubeProjectileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of compiled bank-$86 tube projectile control while excluding spritemap operands.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != EnemyProjectileCodePointers.BankBase)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < NoobTubeProjectileInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = NoobTubeProjectileInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }

        return false;
    }
}
