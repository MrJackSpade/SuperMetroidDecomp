
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values of <see cref="DeadTourianCorpseInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadTourianCorpseInstructionProgramDefinitionsConstants
{
    /// <summary>The first dead-monster spritemap after the programs, at $A9:ED25.</summary>
    public const ushort FirstAdjacentPresentationData = 0xed25;

    /// <summary>Provides the adjacent presentation boundary used when checking dead Tourian corpse programs.</summary>
    extension(DeadTourianCorpseInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadTourianCorpseInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData"/>
        internal static ushort FirstAdjacentPresentationData => DeadTourianCorpseInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData;
    }
}
