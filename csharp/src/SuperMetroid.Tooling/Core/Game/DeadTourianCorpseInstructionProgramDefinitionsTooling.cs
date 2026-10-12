using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="DeadTourianCorpseInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTourianCorpseInstructionProgramDefinitions))]
internal abstract class DeadTourianCorpseInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Presentation word address.</summary>
    internal static ushort PresentationWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+2);
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa90000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
