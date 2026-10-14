using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Development-tool members of <see cref="TorizoSonicBoomInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(TorizoSonicBoomInstructionProgramDefinitions))]
internal abstract class TorizoSonicBoomInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Limits the operands treated as visual data in the sonic boom program audit.</summary>
    public static int PresentationWordCount => TorizoSonicBoomInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Maps a visual operand to its native address without reading cartridge data.</summary>
    public static ushort PresentationWordAddress(int index) => TorizoSonicBoomInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Supplies the native bank used to validate sonic boom instruction ownership.</summary>
    static int IDeclaredProgramBank.Bank => TorizoSonicBoomInstructionProgramDefinitions.Bank;
    /// <summary>Limits the mechanic words enumerated from the compiled instruction layout.</summary>
    public static int MechanicsWordCount => TorizoSonicBoomInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Exposes the native address and compiled value of one mechanic word to tooling.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = TorizoSonicBoomInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether an address belongs to a compiled mechanic word rather than visual data.</summary>
    public static bool IsCompiledMechanicsByte(int address) => TorizoSonicBoomInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
