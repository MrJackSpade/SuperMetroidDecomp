using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoLeftOrbInstructionProgramDefinitions))]
internal abstract class GoldenTorizoLeftOrbInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets the number of presentation words retained as cartridge data.</summary>
    public static int PresentationWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Gets the cartridge address of the presentation word at the given index.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoLeftOrbInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets the cartridge bank containing this instruction program.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoLeftOrbInstructionProgramDefinitions.Bank;
    /// <summary>Gets the number of compiled mechanics words.</summary>
    public static int MechanicsWordCount => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Gets the compiled mechanics word at the given index.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether the address belongs to compiled mechanics data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoLeftOrbInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
