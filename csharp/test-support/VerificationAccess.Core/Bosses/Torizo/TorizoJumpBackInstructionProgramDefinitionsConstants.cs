
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="TorizoJumpBackInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoJumpBackInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the right-facing jump-back instruction list at $AA:C188.</summary>
    public const ushort End = 0xc188;

    /// <summary>Provides the end boundary for Torizo jump-back program extraction.</summary>
    extension(TorizoJumpBackInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoJumpBackInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => TorizoJumpBackInstructionProgramDefinitionsConstants.End;
    }
}
