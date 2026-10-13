using SuperMetroid.Core.Hardware;
using SuperMetroid.Core.Assets;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="PhantoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(PhantoonInstructionProgramDefinitions))]
internal abstract class PhantoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Timing, callback, and branch words across Phantoon's body, eye, tentacle, and mouth lists.</summary>
    public static int MechanicsWordCount => PhantoonInstructionProgramDefinitions.MechanicsWordCount;
    /// <summary>Reads a compiled word from one of Phantoon's four enemy records.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index) => PhantoonInstructionProgramDefinitions.MechanicsWord(index);
    /// <summary>Pose operands resolved through installed Phantoon display data.</summary>
    public static int PresentationWordCount => PhantoonInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates a Phantoon pose operand without reading installed art.</summary>
    public static ushort PresentationWordAddress(int index) => PhantoonInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Matches either byte of a compiled Phantoon word in bank $A7.</summary>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((SnesAddressMath.BankBase(address)) != 0xa70000)
            return false;

        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < PhantoonInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = PhantoonInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress ||
                bankAddress == unchecked((ushort)(wordAddress + 1)))
            {
                return true;
            }
        }
        return false;
    }
}
