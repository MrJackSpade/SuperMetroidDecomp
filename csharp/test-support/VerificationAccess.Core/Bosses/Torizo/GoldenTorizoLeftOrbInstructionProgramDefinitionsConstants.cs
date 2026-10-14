
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $CB83 after the left-orb attack program.</summary>
    public const ushort End = 0xcb83;

    /// <summary>Provides the end boundary for Golden Torizo left orb program extraction.</summary>
    extension(GoldenTorizoLeftOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftOrbInstructionProgramDefinitionsConstants.End;
    }
}
