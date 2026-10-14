using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Development-tool members of <see cref="DeadTourianCorpseInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(DeadTourianCorpseInstructionProgramDefinitions))]
internal abstract class DeadTourianCorpseInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Counts the compiled mechanics words in dead Tourian corpse programs.</summary>
    public static int MechanicsWordCount => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Returns a corpse-program mechanics word and its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => DeadTourianCorpseInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Locates the presentation word two bytes after an indexed corpse program's start.</summary>
    internal static ushort PresentationWordAddress(int index) => (ushort)(DeadTourianCorpseInstructionProgramDefinitions.Program(index)+2);
    /// <summary>Checks both bytes of corpse mechanics words within bank $A9.</summary>
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
