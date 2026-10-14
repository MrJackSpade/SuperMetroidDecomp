using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="FirefleaInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class FirefleaInstructionProgramDefinitionsConstants
{
    /// <summary>The adjacent unused Fireflea data block at $A3:8D03.</summary>
    public const ushort AdjacentUnusedData = 0x8d03;

    /// <summary>Provides adjacent unused data that bounds Fireflea instruction program decoding.</summary>
    extension(FirefleaInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="FirefleaInstructionProgramDefinitionsConstants.AdjacentUnusedData"/>
        internal static ushort AdjacentUnusedData => FirefleaInstructionProgramDefinitionsConstants.AdjacentUnusedData;
    }
}
