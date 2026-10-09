using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoEggInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoEggInstructionProgramDefinitions))]
internal abstract class GoldenTorizoEggInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Gets the number of compiled mechanics words across the Golden Torizo egg instruction programs.</summary>
    public static int MechanicsWordCount => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount;

    /// <summary>Gets a compiled mechanics instruction word by its zero-based position in the combined egg programs.</summary>
    /// <param name="index">Zero-based position in the mechanics-word sequence.</param>
    /// <returns>The native address and value of the selected mechanics instruction.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the compiled mechanics-word range.</exception>
    public static InstructionMechanicsWord MechanicsWord(int index) => GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);

    /// <summary>Gets the number of extracted spritemap operand words in the Golden Torizo egg programs.</summary>
    public static int PresentationWordCount => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the bank-local address of an extracted spritemap operand by its zero-based position.</summary>
    /// <param name="index">Zero-based position in the presentation-word sequence.</param>
    /// <returns>The bank-local address of the selected operand.</returns>
    /// <exception cref="IndexOutOfRangeException"><paramref name="index"/> is outside the presentation-word range.</exception>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoEggInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Determines whether an address identifies a byte in compiled egg mechanics, including reused Torizo-orb wall-impact mechanics.</summary>
    /// <param name="address">24-bit SNES address to check.</param>
    /// <returns><see langword="true"/> when the address is a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address)
    {
        if (TorizoChozoOrbInstructionProgramDefinitionsTooling.IsCompiledMechanicsByte(address) &&
            unchecked((ushort)address) is >= TorizoChozoOrbInstructionProgramDefinitions.WallImpact and <= 0xab40)
        {
            return true;
        }
        if ((address & 0xff0000) != EnemyProjectileCodePointers.BankBase) return false;
        ushort bankAddress = unchecked((ushort)address);
        for (int index = 0; index < GoldenTorizoEggInstructionProgramDefinitions.MechanicsWordCount; index++)
        {
            var word = GoldenTorizoEggInstructionProgramDefinitions.MechanicsWord(index);
            if (bankAddress == word.Address || bankAddress == unchecked((ushort)(word.Address + 1)))
                return true;
        }
        return false;
    }
}
