using SuperMetroid.Core.Game;

/// <summary>Cartridge values of <see cref="MorphBallEyeInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class MorphBallEyeInstructionProgramDefinitionsConstants
{
    /// <summary><c>EyeConstants</c>, adjacent non-instruction data at $A8:9050.</summary>
    public const ushort AdjacentProximityDefinitions = 0x9050;

    /// <summary>Provides the proximity-definition boundary adjacent to Morph Ball Eye instructions.</summary>
    extension(MorphBallEyeInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="MorphBallEyeInstructionProgramDefinitionsConstants.AdjacentProximityDefinitions"/>
        internal static ushort AdjacentProximityDefinitions => MorphBallEyeInstructionProgramDefinitionsConstants.AdjacentProximityDefinitions;
    }
}
