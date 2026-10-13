using SuperMetroid.Core.Hardware;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MamaTurtleInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(MamaTurtleInstructionProgramDefinitions))]
internal abstract class MamaTurtleInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe
{
    /// <summary>Uses the production program's declared spritemap operand count for presentation auditing.</summary>
    public static int PresentationWordCount => MamaTurtleInstructionProgramDefinitions.PresentationWordCount;
    /// <summary>Locates the indexed spritemap operand through the production instruction catalog.</summary>
    public static ushort PresentationWordAddress(int index) => MamaTurtleInstructionProgramDefinitions.PresentationWordAddress(index);
    /// <summary>Number of compiled control words between the baby-crawl and adjacent movement regions.</summary>
    public static int MechanicsWordCount => 117;
    /// <summary>Walks compiled turtle control words in address order, rejecting an incomplete layout.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        if ((uint)index >= MechanicsWordCount) throw new IndexOutOfRangeException();
        for (int address = MamaTurtleInstructionProgramDefinitions.BabyCrawlingLeft; address < MamaTurtleInstructionProgramDefinitions.AdjacentMovementDefinitions; address += 2)
            if (MamaTurtleInstructionProgramDefinitions.TryControl((ushort)address, out ushort value) && index-- == 0)
                return new((ushort)address, value);
        throw new InvalidDataException("Turtle control layout is incomplete.");
    }
    /// <summary>Recognizes bytes belonging to compiled turtle control words in bank $A2.</summary>
    public static bool IsCompiledMechanicsByte(int address) =>
        (SnesAddressMath.BankBase(address)) == 0xa20000 && MamaTurtleInstructionProgramDefinitions.TryControl((ushort)(address & 0xfffe), out _);
}
