using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="CrocomireInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(CrocomireInstructionProgramDefinitions))]
internal abstract class CrocomireInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the bank containing the Crocomire instruction programs exposed to tooling.</summary>
    static int IDeclaredProgramBank.Bank => CrocomireInstructionProgramDefinitions.Bank;

    /// <summary>Gets the number of address/value mechanics words compiled for Crocomire's instruction programs.</summary>
    public static int MechanicsWordCount => CrocomireInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Gets the number of live presentation-operand slots interleaved with those mechanics words.</summary>
    public static int PresentationWordCount => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotCount;

    /// <summary>Returns the compiled mechanics address/value pair at the requested ordinal.</summary>
    /// <param name="index">Zero-based index in the compiled mechanics-word sequence.</param>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = CrocomireInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }

    /// <summary>Returns the native address of a presentation operand slot.</summary>
    /// <param name="index">Zero-based presentation-slot index in the compiled programs.</param>
    public static ushort PresentationWordAddress(int index) => CrocomireInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);

    /// <summary>Checks whether an address is part of a compiled mechanics word for Crocomire.</summary>
    /// <param name="address">The bank-relative address to test.</param>
    /// <returns><see langword="true"/> when the address names a compiled mechanics byte; otherwise, <see langword="false"/>.</returns>
    public static bool IsCompiledMechanicsByte(int address) => CrocomireInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
