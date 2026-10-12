using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="GoldenTorizoWalkingInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(GoldenTorizoWalkingInstructionProgramDefinitions))]
internal abstract class GoldenTorizoWalkingInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Tracks ten live extended-frame operands in Golden Torizo's walking-left lists.</summary>
    public static int PresentationWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates an editable frame selector among the linked $AA:D20D-D2AC lists.</summary>
    public static ushort PresentationWordAddress(int index) => GoldenTorizoWalkingInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Assigns the linked walking lists to native bank $AA for comparison.</summary>
    static int IDeclaredProgramBank.Bank => GoldenTorizoWalkingInstructionProgramDefinitions.Bank;
    /// <summary>Counts fixed movement, attack, timer, and branch words across the linked lists.</summary>
    public static int MechanicsWordCount => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Provides a bank-relative walking control word together with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = GoldenTorizoWalkingInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Separates compiled walking controls from the ten installed-art selectors by byte address.</summary>
    public static bool IsCompiledMechanicsByte(int address) => GoldenTorizoWalkingInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
