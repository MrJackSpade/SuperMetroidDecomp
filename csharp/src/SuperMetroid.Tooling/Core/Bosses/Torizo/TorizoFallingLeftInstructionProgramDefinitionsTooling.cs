using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="TorizoFallingLeftInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoFallingLeftInstructionProgramDefinitions))]
internal abstract class TorizoFallingLeftInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Locates the single falling-left visual operand that selects frame $AA:B014.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoFallingLeftInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Constrains the shared falling-left list to native bank $AA.</summary>
    static int IDeclaredProgramBank.Bank => TorizoFallingLeftInstructionProgramDefinitions.Bank;
    /// <summary>Counts timing and branch controls spanning the fall and landing entries.</summary>
    public static int MechanicsWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Exposes the compiled control value and cartridge address at one layout ordinal.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoFallingLeftInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>The one installed extended-frame selector is separate from falling mechanics.</summary>
    public static int PresentationWordCount => TorizoFallingLeftInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Recognizes bytes in the fall or landing controls without claiming the visual operand.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoFallingLeftInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
