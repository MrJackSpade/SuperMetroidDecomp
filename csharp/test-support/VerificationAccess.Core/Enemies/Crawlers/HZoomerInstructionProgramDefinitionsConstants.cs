
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values of <see cref="HZoomerInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class HZoomerInstructionProgramDefinitionsConstants
{
    /// <summary><c>Instruction_HZoomer_FunctionInY</c> immediately before the programs.</summary>
    public const ushort AdjacentFunctionCode = 0xdfc2;

    /// <summary>Provides the neighboring function-code boundary for horizontal Zoomer program extraction.</summary>
    extension(HZoomerInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="HZoomerInstructionProgramDefinitionsConstants.AdjacentFunctionCode"/>
        internal static ushort AdjacentFunctionCode => HZoomerInstructionProgramDefinitionsConstants.AdjacentFunctionCode;
    }
}
