
namespace SuperMetroid.Core.Enemies.MaridiaSnails;

/// <summary>Cartridge values of <see cref="MaridiaLargeSnailInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MaridiaLargeSnailInstructionProgramDefinitionsConstants
{
    /// <summary>The selector table immediately following Oum's programs, at $A2:CB77.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xcb77;

    /// <summary>Provides the adjacent mechanics boundary for Maridia large snail program decoding.</summary>
    extension(MaridiaLargeSnailInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MaridiaLargeSnailInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => MaridiaLargeSnailInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}
