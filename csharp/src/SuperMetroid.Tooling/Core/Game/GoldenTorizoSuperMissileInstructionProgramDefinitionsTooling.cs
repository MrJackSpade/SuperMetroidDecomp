using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoSuperMissileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoSuperMissileInstructionProgramDefinitions))]
internal abstract class GoldenTorizoSuperMissileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of super-missile instruction operands that select presentation data.</summary>
    public static int PresentationWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Maps an indexed presentation operand to its compiled bank-relative address.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Preserves the super-missile instruction bank when audits interpret the operand addresses.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoSuperMissileInstructionProgramDefinitions.Bank;
    /// <summary>Number of control words in the super-missile instruction layout.</summary>
    public static int MechanicsWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Pairs one compiled control word with its address for tooling audits.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether an address is a compiled control byte in this projectile program.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
