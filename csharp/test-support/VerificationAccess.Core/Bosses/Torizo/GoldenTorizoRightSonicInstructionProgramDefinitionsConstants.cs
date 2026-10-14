
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoRightSonicInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightSonicInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $CDAF after the right sonic-boom program.</summary>
    public const ushort End = 0xcdaf;

    /// <summary>Provides the end boundary for Golden Torizo right-sonic program extraction.</summary>
    extension(GoldenTorizoRightSonicInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightSonicInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightSonicInstructionProgramDefinitionsConstants.End;
    }
}
