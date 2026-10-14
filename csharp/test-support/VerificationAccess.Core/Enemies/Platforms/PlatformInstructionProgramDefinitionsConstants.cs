
namespace SuperMetroid.Core.Enemies.Platforms;

/// <summary>Cartridge values of <see cref="PlatformInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PlatformInstructionProgramDefinitionsConstants
{
    /// <summary>The first callback implementation immediately after the programs.</summary>
    public const ushort FirstAdjacentCallback = 0x9c6b;

    /// <summary>Provides the first neighboring callback that bounds platform instruction extraction.</summary>
    extension(PlatformInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PlatformInstructionProgramDefinitionsConstants.FirstAdjacentCallback"/>
        internal static ushort FirstAdjacentCallback => PlatformInstructionProgramDefinitionsConstants.FirstAdjacentCallback;
    }
}
