using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CeresBabyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CeresBabyInstructionProgramDefinitions))]
internal abstract class CeresBabyInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of compiled non-presentation instruction words exposed to tooling.</summary>
    public static int MechanicsWordCount => CeresBabyInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets one compiled mechanics word in the catalog's native address order.</summary>
    /// <param name="index">Zero-based mechanics-word ordinal.</param>
    /// <returns>The word's address and value.</returns>
    /// <exception cref="ArgumentOutOfRangeException">The ordinal is outside the compiled mechanics-word sequence.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => CeresBabyInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Bank containing the Ceres Baby instruction program, exposed for catalog discovery.</summary>
    static int IDeclaredProgramBank.Bank => CeresBabyInstructionProgramDefinitions.Bank;

    /// <summary>Tests whether a full address names either byte of a compiled mechanics word in the Ceres Baby bank.</summary>
    /// <param name="address">Full banked address to classify.</param>
    /// <returns><see langword="true"/> when the address is in bank $A6 and matches a mechanics-word byte.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != 0xa60000)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < CeresBabyInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            ushort wordAddress = CeresBabyInstructionProgramDefinitions.MechanicsWord(index).Address;
            if (bankAddress == wordAddress || bankAddress == unchecked((ushort)(wordAddress + 1)))
                return true;
        }
        return false;
    }
}
