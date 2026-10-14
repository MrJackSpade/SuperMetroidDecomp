
namespace SuperMetroid.Core.Enemies.Corpses;

/// <summary>Cartridge values of <see cref="DeadTorizoInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadTorizoInstructionProgramDefinitionsConstants
{
    /// <summary><c>Spritemaps_CorpseTorizo</c> begins after the program at $A9:D6E2.</summary>
    public const ushort FirstAdjacentPresentationData = 0xd6e2;

    /// <summary>Provides the adjacent presentation boundary used when checking dead Torizo program extraction.</summary>
    extension(DeadTorizoInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadTorizoInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData"/>
        internal static ushort FirstAdjacentPresentationData => DeadTorizoInstructionProgramDefinitionsConstants.FirstAdjacentPresentationData;
    }
}
