
namespace SuperMetroid.Core.Enemies.Beetom;

/// <summary>Cartridge values of <see cref="BeetomInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class BeetomInstructionProgramDefinitionsConstants
{
    /// <summary>The repeating right-crawl frame list at $A8:B6F4.</summary>
    public const ushort CrawlingRightLoop = 0xb6f4;
    /// <summary>The repeating right-drain frame list at $A8:B73A.</summary>
    public const ushort DrainingRightLoop = 0xb73a;
    /// <summary>The terminal left-hop sleep instruction at $A8:B6BE.</summary>
    public const ushort HopLeftSleep = 0xb6be;
    /// <summary>The terminal right-hop sleep instruction at $A8:B71A.</summary>
    public const ushort HopRightSleep = 0xb71a;

    /// <summary>Provides native Beetom instruction loops for crawl, drain, and hop program comparisons.</summary>
    extension(BeetomInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.CrawlingRightLoop"/>
        internal static ushort CrawlingRightLoop => BeetomInstructionProgramDefinitionsConstants.CrawlingRightLoop;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.DrainingRightLoop"/>
        internal static ushort DrainingRightLoop => BeetomInstructionProgramDefinitionsConstants.DrainingRightLoop;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.HopLeftSleep"/>
        internal static ushort HopLeftSleep => BeetomInstructionProgramDefinitionsConstants.HopLeftSleep;
        /// <inheritdoc cref="BeetomInstructionProgramDefinitionsConstants.HopRightSleep"/>
        internal static ushort HopRightSleep => BeetomInstructionProgramDefinitionsConstants.HopRightSleep;
    }
}
