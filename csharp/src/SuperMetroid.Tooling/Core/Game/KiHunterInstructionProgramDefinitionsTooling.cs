using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="KiHunterInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(KiHunterInstructionProgramDefinitions))]
internal abstract class KiHunterInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of presentation operands extracted from the Ki Hunter instruction program.</summary>
    public static int PresentationWordCount => KiHunterInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the ROM address associated with a presentation operand.</summary>
    /// <param name="index">Zero-based position of the operand in the presentation data.</param>
    /// <returns>The operand's address in the declared program bank.</returns>
    public static ushort PresentationWordAddress(int index) => KiHunterInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the ROM bank containing the compiled Ki Hunter instruction program.</summary>
    static int IDeclaredProgramBank.Bank => KiHunterInstructionProgramDefinitions.Bank;

    /// <summary>Gets the number of mechanics words in the compiled Ki Hunter instruction program.</summary>
    public static int MechanicsWordCount => KiHunterInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Gets a mechanics word from the compiled Ki Hunter instruction program.</summary>
    /// <param name="index">Zero-based position of the mechanics word in the compiled layout.</param>
    /// <returns>The instruction mechanics word at that position.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = KiHunterInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Determines whether an address identifies either byte of a compiled mechanics word.</summary>
    /// <param name="address">Full ROM address to check.</param>
    /// <returns><see langword="true"/> when the address is a mechanics-word byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => KiHunterInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
