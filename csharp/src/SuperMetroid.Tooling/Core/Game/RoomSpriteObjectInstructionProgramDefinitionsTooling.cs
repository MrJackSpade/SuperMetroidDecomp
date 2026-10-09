using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="RoomSpriteObjectInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(RoomSpriteObjectInstructionProgramDefinitions))]
internal abstract class RoomSpriteObjectInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of editable spritemap-pointer words across the catalogued room sprite-object lists.</summary>
    public static int PresentationWordCount => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of an editable spritemap pointer.</summary>
    /// <param name="index">Zero-based ordinal among the catalog's presentation operands.</param>
    /// <returns>Bank-$B4 address of the selected spritemap-pointer word.</returns>
    public static ushort PresentationWordAddress(int index) => RoomSpriteObjectInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets bank $B4, which contains the catalogued room sprite-object instruction lists.</summary>
    static int IDeclaredProgramBank.Bank => RoomSpriteObjectInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled instruction-control and duration words across the room sprite-object lists.</summary>
    public static int MechanicsWordCount => RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Resolves a mechanics ordinal to its native instruction address and operand value.</summary>
    /// <param name="index">Zero-based index among the compiled mechanics words.</param>
    /// <returns>The native address and value of the selected control or duration word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = RoomSpriteObjectInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether a cartridge byte belongs to compiled mechanics rather than a live spritemap pointer.</summary>
    /// <param name="address">Full cartridge address to classify.</param>
    /// <returns><see langword="true"/> when the byte belongs to a compiled control or timing word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => RoomSpriteObjectInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
