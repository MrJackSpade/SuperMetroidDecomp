using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="EscapeDachoraInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EscapeDachoraInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_DachoraEscape_GotoY_IfAcidLessThanCE</c>, adjacent code at $B3:EAA8.</summary>
    public const ushort FirstAdjacentCodeRoutine = 0xeaa8;

    /// <summary>Provides the neighboring code boundary for escape Dachora instruction extraction.</summary>
    extension(EscapeDachoraInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EscapeDachoraInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine"/>
        internal static ushort FirstAdjacentCodeRoutine => EscapeDachoraInstructionProgramDefinitionsConstants.FirstAdjacentCodeRoutine;
    }
}
