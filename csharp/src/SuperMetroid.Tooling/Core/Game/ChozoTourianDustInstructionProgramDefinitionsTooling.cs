using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="ChozoTourianDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(ChozoTourianDustInstructionProgramDefinitions))]
internal abstract class ChozoTourianDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of compiled timing and control operands in the dust instruction programs.</summary>
    public static int MechanicsWordCount => ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets the address and fixed operand for a mechanics slot.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The compiled instruction address and its cartridge-defined value.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite-selector operands represented by presentation data.</summary>
    public static int PresentationWordCount => ChozoTourianDustInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank address of one presentation sprite selector.</summary>
    /// <param name="index">The zero-based presentation-word index.</param>
    /// <returns>The address containing the selected presentation operand.</returns>
    public static ushort PresentationWordAddress(int index) => ChozoTourianDustInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether a bank address is either byte of a compiled mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address belongs to a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase)
            return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < ChozoTourianDustInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            InstructionMechanicsWord word = ChozoTourianDustInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address ||
                bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
