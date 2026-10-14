
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoAwakeningInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoAwakeningInstructionProgramDefinitionsConstants
{
    /// <summary>First word after the awakening handoff at $AA:CACE.</summary>
    public const ushort End = 0xcace;

    /// <summary>Provides the end boundary for Golden Torizo awakening instruction extraction.</summary>
    extension(GoldenTorizoAwakeningInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoAwakeningInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoAwakeningInstructionProgramDefinitionsConstants.End;
    }
}
