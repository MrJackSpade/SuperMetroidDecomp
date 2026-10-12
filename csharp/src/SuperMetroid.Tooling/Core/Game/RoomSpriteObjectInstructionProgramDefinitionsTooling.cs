using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RoomSpriteObjectInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RoomSpriteObjectInstructionProgramDefinitions))]
internal abstract class RoomSpriteObjectInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    public static int PresentationWordCount => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
    public static ushort PresentationWordAddress(int index) => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
    static int IDeclaredProgramBank.Bank => RoomSpriteObjectInstructionProgramDefinitions.Bank;
    public static int MechanicsWordCount => RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWordCount;
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    public static bool IsCompiledMechanicsByte(int address) => RoomSpriteObjectInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
