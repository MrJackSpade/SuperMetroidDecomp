using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="GoldenTorizoRightOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoRightOrbInstructionProgramDefinitionsConstants
{
    /// <summary>Instruction boundary at $CCDB after the right-orb attack program.</summary>
    public const ushort End = 0xccdb;

    /// <summary>Provides the end boundary for Golden Torizo right-orb program extraction.</summary>
    extension(GoldenTorizoRightOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoRightOrbInstructionProgramDefinitionsConstants.End"/>
        internal static ushort End => GoldenTorizoRightOrbInstructionProgramDefinitionsConstants.End;
    }
}
