using SuperMetroid.Tooling;
using SuperMetroid.Core.Programs;

namespace SuperMetroid.Core.Rooms.SpriteObjects;

/// <summary>Development-tool members of <see cref="RoomSpriteObjectInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RoomSpriteObjectInstructionProgramDefinitions))]
internal abstract class RoomSpriteObjectInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Counts presentation operands in room sprite-object instruction programs.</summary>
    public static int PresentationWordCount => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Returns the bank address of an indexed sprite-object presentation word.</summary>
    public static ushort PresentationWordAddress(int index) => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Identifies the native bank of room sprite-object programs.</summary>
    static int IDeclaredProgramBank.Bank => RoomSpriteObjectInstructionProgramDefinitions.Bank;
    /// <summary>Counts sprite-object instruction words translated into compiled mechanics.</summary>
    public static int MechanicsWordCount => RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Returns a compiled sprite-object mechanics word with its native address.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Checks whether a sprite-object program byte is owned by compiled mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => RoomSpriteObjectInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
