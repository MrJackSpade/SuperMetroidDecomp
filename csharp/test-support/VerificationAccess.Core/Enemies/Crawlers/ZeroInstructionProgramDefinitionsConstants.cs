
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values of <see cref="ZeroInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ZeroInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused alternate upside-right program at $A3:982B.</summary>
    public const ushort UnusedAlternateUpsideRight = 0x982b;

    /// <summary>Provides Zero's unused alternate upside-right program for adjacent instruction coverage.</summary>
    extension(ZeroInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ZeroInstructionProgramDefinitionsConstants.UnusedAlternateUpsideRight"/>
        internal static ushort UnusedAlternateUpsideRight => ZeroInstructionProgramDefinitionsConstants.UnusedAlternateUpsideRight;
    }
}
