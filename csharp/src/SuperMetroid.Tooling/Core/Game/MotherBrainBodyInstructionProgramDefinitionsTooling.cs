using static SuperMetroid.Core.Game.InstructionItem;

namespace SuperMetroid.Core.Game;

/// <summary>Development-tool members of <see cref="MotherBrainBodyInstructionProgramDefinitions"/>; never linked by player hosts.</summary>
internal static class MotherBrainBodyInstructionProgramDefinitionsTooling
{
    /// <summary>All compiled mechanics words in address order, for cartridge-equivalence tests.</summary>
    internal static IReadOnlyList<MotherBrainBodyInstructionMechanicsWord> AllWords =>
        Enumerable.Range(0, MotherBrainBodyInstructionProgramDefinitions.Layout.MechanicsWordCount).Select(index =>
        {
            (ushort address, ushort value) = MotherBrainBodyInstructionProgramDefinitions.Layout.MechanicsWord(index);
            return new MotherBrainBodyInstructionMechanicsWord(address, value);
        }).ToArray();
}
