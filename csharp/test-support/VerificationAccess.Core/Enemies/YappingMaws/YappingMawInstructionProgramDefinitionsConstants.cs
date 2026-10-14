
namespace SuperMetroid.Core.Enemies.YappingMaws;

/// <summary>Cartridge values of <see cref="YappingMawInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class YappingMawInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstListPointers_YappingMaw</c>, adjacent selector data at $A8:A097.</summary>
    public const ushort AdjacentAttackSelectorTable = 0xa097;

    /// <summary>Provides the attack selector table adjacent to Yapping Maw instruction programs.</summary>
    extension(YappingMawInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="YappingMawInstructionProgramDefinitionsConstants.AdjacentAttackSelectorTable"/>
        internal static ushort AdjacentAttackSelectorTable => YappingMawInstructionProgramDefinitionsConstants.AdjacentAttackSelectorTable;
    }
}
