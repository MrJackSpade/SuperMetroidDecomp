using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RoomSpriteObjectInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RoomSpriteObjectInstructionProgramDefinitions))]
internal abstract class RoomSpriteObjectInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Gets presentation word count.</summary>
    public static int PresentationWordCount => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Presentation word address.</summary>
    public static ushort PresentationWordAddress(int index) => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Gets bank.</summary>
    static int IDeclaredProgramBank.Bank => RoomSpriteObjectInstructionProgramDefinitions.Bank;
    /// <summary>Gets mechanics word count.</summary>
    public static int MechanicsWordCount => RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Mechanics word.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether compiled mechanics byte.</summary>
    public static bool IsCompiledMechanicsByte(int address) => RoomSpriteObjectInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
