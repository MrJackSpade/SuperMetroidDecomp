using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="DeadSidehopperInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DeadSidehopperInstructionProgramDefinitionsConstants
{
    /// <summary>The first following program, <c>InstList_CorpseZoomer_Param1_0</c>, at $A9:ECF5.</summary>
    public const ushort FirstAdjacentProgram = 0xecf5;

    /// <summary>Provides the first neighboring program address that bounds dead Sidehopper instruction data.</summary>
    extension(DeadSidehopperInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DeadSidehopperInstructionProgramDefinitionsConstants.FirstAdjacentProgram"/>
        internal static ushort FirstAdjacentProgram => DeadSidehopperInstructionProgramDefinitionsConstants.FirstAdjacentProgram;
    }
}
