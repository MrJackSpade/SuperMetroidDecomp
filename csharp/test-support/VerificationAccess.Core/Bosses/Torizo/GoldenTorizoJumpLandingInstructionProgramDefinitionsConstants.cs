
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoJumpLandingInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $CD D7 after Golden Torizo's jump-landing program.</summary>
    public const ushort End = 0xcdd7;

    /// <summary>Provides the end boundary for Golden Torizo jump-landing program extraction.</summary>
    extension(GoldenTorizoJumpLandingInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoJumpLandingInstructionProgramDefinitionsConstants.End;
    }
}
