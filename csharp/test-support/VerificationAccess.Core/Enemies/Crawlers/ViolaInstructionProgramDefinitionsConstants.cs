
namespace SuperMetroid.Core.Enemies.Crawlers;

/// <summary>Cartridge values of <see cref="ViolaInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class ViolaInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused X-flipped Viola program at $A3:B62B.</summary>
    public const ushort UnusedXFlipped = 0xb62b;

    /// <summary>Provides Viola's unused X-flipped program for neighboring instruction coverage checks.</summary>
    extension(ViolaInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="ViolaInstructionProgramDefinitionsConstants.UnusedXFlipped"/>
        internal static ushort UnusedXFlipped => ViolaInstructionProgramDefinitionsConstants.UnusedXFlipped;
    }
}
