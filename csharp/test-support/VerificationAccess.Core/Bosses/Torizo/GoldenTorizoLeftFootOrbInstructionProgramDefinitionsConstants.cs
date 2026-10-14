
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants
{
    /// <summary>First byte after the left-foot-forward orb list, $AA:CC99.</summary>
    public const ushort End = GoldenTorizoRightOrbInstructionProgramDefinitions.Start;

    /// <summary>Provides the end boundary for Golden Torizo left-foot orb program extraction.</summary>
    extension(GoldenTorizoLeftFootOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoLeftFootOrbInstructionProgramDefinitionsConstants.End;
    }
}
