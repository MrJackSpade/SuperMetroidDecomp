
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values of <see cref="SciserInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class SciserInstructionProgramDefinitionsConstants
{
    /// <summary>The final elevator return opcode immediately before Sciser's palette.</summary>
    public const ushort AdjacentPreviousCode = 0x95eb;

    /// <summary>Provides the preceding code boundary for Sciser instruction program extraction.</summary>
    extension(SciserInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="SciserInstructionProgramDefinitionsConstants.AdjacentPreviousCode"/>
        internal static ushort AdjacentPreviousCode => SciserInstructionProgramDefinitionsConstants.AdjacentPreviousCode;
    }
}
