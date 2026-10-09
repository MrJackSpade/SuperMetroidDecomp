using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEyeBeamInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEyeBeamInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEyeBeamInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Number of fixed timing and control operands in the eye-beam instruction program.</summary>
    public static int MechanicsWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Maps a mechanics slot to its instruction address and fixed operand.</summary>
    /// <param name="index">The zero-based mechanics-word index.</param>
    /// <returns>The address and cartridge-defined value of the selected mechanics word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Number of sprite-selector operands supplied as eye-beam presentation data.</summary>
    public static int PresentationWordCount => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the address of one eye-beam sprite selector.</summary>
    /// <param name="index">The zero-based presentation slot.</param>
    /// <returns>The address of the selected presentation word.</returns>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEyeBeamInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Checks whether an address is either byte of a compiled mechanics operand.</summary>
    /// <param name="address">The full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the bank address belongs to a fixed mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEyeBeamInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
