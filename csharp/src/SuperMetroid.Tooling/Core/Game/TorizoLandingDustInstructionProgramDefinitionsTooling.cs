using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="TorizoLandingDustInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoLandingDustInstructionProgramDefinitions))]
internal abstract class TorizoLandingDustInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of spritemap operands used by the two landing-dust programs.</summary>
    public static int PresentationWordCount => TorizoLandingDustInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the bank-local address of a landing-dust spritemap operand.</summary>
    /// <param name="index">Zero-based position among the compiled presentation operands.</param>
    /// <returns>The instruction word address selecting that operand's frame art.</returns>
    public static ushort PresentationWordAddress(int index) => TorizoLandingDustInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets the native bank containing the right- and left-foot landing-dust instruction lists.</summary>
    static int IDeclaredProgramBank.Bank => TorizoLandingDustInstructionProgramDefinitions.Bank;

    /// <summary>Gets the number of instruction words compiled as landing-dust mechanics data.</summary>
    public static int MechanicsWordCount => TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns a compiled mechanics word's address and value in layout order.</summary>
    /// <param name="index">Zero-based index into the mechanics words.</param>
    /// <returns>The address/value pair used by tooling audits.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoLandingDustInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Tests whether a full SNES address belongs to compiled landing-dust mechanics data.</summary>
    /// <param name="address">Full SNES address to classify.</param>
    /// <returns><see langword="true"/> when the address is occupied by a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => TorizoLandingDustInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
