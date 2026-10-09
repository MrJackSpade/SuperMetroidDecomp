using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="WallSpacePirateInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(WallSpacePirateInstructionProgramDefinitions))]
internal abstract class WallSpacePirateInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Number of editable extended-spritemap pointer words in the wall Space Pirate programs.</summary>
    public static int PresentationWordCount => WallSpacePirateInstructionProgramDefinitions.PresentationWordCount;

    /// <summary>Returns the native address of an editable extended-spritemap pointer.</summary>
    /// <param name="index">Zero-based ordinal among the catalog's presentation operands.</param>
    /// <returns>Bank-$B2 address of the selected pointer word.</returns>
    public static ushort PresentationWordAddress(int index) => WallSpacePirateInstructionProgramDefinitions.PresentationWordAddress(index);

    /// <summary>Gets bank $B2, which contains the wall Space Pirate instruction programs.</summary>
    static int IDeclaredProgramBank.Bank => WallSpacePirateInstructionProgramDefinitions.Bank;

    /// <summary>Number of compiled instruction-control and timing words across the wall-pirate programs.</summary>
    public static int MechanicsWordCount => WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWordCount;

    /// <summary>Resolves a mechanics ordinal to its native instruction address and operand value.</summary>
    /// <param name="index">Zero-based index among the compiled mechanics words.</param>
    /// <returns>The native address and value of the selected control or timing word.</returns>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = WallSpacePirateInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Tests whether a byte belongs to compiled mechanics rather than a live spritemap pointer.</summary>
    /// <param name="address">Full cartridge address to classify.</param>
    /// <returns><see langword="true"/> when the byte is part of a compiled control or timing word.</returns>
    public static bool IsCompiledMechanicsByte(int address) => WallSpacePirateInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
