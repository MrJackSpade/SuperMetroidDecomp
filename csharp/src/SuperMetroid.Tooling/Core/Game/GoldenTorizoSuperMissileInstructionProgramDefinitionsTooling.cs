using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoSuperMissileInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoSuperMissileInstructionProgramDefinitions))]
internal abstract class GoldenTorizoSuperMissileInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoSuperMissileInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoSuperMissileInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoSuperMissileInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
