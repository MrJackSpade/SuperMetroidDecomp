
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values of <see cref="SharedCrawlerInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SharedCrawlerInstructionProgramDefinitionsConstants
{
    /// <summary>The first word of the adjacent initial-list pointer table at $A3:E2CC.</summary>
    public const ushort AdjacentInitialSelectorTable = 0xe2cc;

    /// <summary>Provides the neighboring selector table that bounds shared crawler instruction programs.</summary>
    extension(SharedCrawlerInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SharedCrawlerInstructionProgramDefinitionsConstants.AdjacentInitialSelectorTable"/>
        internal static ushort AdjacentInitialSelectorTable => SharedCrawlerInstructionProgramDefinitionsConstants.AdjacentInitialSelectorTable;
    }
}
