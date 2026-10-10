using static SuperMetroid.Core.Game.InstructionItem;
using SuperMetroid.Tooling;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="SporeSpawnInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
[ToolingFor(typeof(SporeSpawnInstructionProgramDefinitions))]
internal abstract class SporeSpawnInstructionProgramDefinitionsTooling : IInstructionProgramCatalog, IPresentationOperandCatalog, ICompiledMechanicsByteProbe, IDeclaredProgramBank
{
    /// <summary>Bank identifier used by tooling to associate these compiled program words with their cartridge source bank.</summary>
    static int IDeclaredProgramBank.Bank => SporeSpawnInstructionProgramDefinitions.Bank;
    /// <summary>Number of simulation-owned instruction words compiled from Spore Spawn's five native programs.</summary>
    public static int MechanicsWordCount => SporeSpawnInstructionProgramDefinitions.Layout.MechanicsWordCount;
    /// <summary>Number of interleaved presentation words, compiled separately for installed play.</summary>
    public static int PresentationWordCount => SporeSpawnInstructionProgramDefinitions.Layout.PresentationSlotCount;
    /// <summary>Returns one mechanics definition for cartridge-equivalence verification.</summary>
    public static InstructionMechanicsWord MechanicsWord(int index)
    {
        (ushort address, ushort value) = SporeSpawnInstructionProgramDefinitions.Layout.MechanicsWord(index);
        return new(address, value);
    }
    /// <summary>Returns one live spritemap-word address for boundary verification.</summary>
    public static ushort PresentationWordAddress(int index) => SporeSpawnInstructionProgramDefinitions.Layout.PresentationSlotAddress(index);
    /// <summary>True when an absolute address names a byte owned by compiled mechanics.</summary>
    public static bool IsCompiledMechanicsByte(int address) => SporeSpawnInstructionProgramDefinitions.Layout.IsCompiledMechanicsByte(address);
}
