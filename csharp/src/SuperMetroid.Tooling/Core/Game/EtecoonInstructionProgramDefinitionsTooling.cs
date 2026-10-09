using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="EtecoonInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(EtecoonInstructionProgramDefinitions))]
internal abstract class EtecoonInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of spritemap operands in Etecoon's compiled instruction programs.</summary>
    public static int PresentationWordCount => EtecoonInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Gets the native address containing a presentation operand at the requested catalog position.</summary>
    /// <param name="index">Zero-based position in Etecoon's presentation-word table.</param>
    /// <returns>Address of the selected spritemap operand.</returns>
    public static ushort PresentationWordAddress(int index) => EtecoonInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Bank containing the instruction programs declared by the wrapped catalog.</summary>
    static int IDeclaredProgramBank.Bank => EtecoonInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled mechanics address/value pairs in Etecoon's instruction programs.</summary>
    public static int MechanicsWordCount => EtecoonInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Returns one mechanics address/value pair from the wrapped catalog's ordered table.</summary>
    /// <param name="index">Zero-based position in the mechanics-word table.</param>
    /// <returns>The instruction address and operand value at that position.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = EtecoonInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Reports whether a byte address belongs to a compiled mechanics operand in Etecoon's instruction lists.</summary>
    /// <param name="address">Full cartridge address to check.</param>
    /// <returns><see langword="true"/> when the address is part of a compiled mechanics word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => EtecoonInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
