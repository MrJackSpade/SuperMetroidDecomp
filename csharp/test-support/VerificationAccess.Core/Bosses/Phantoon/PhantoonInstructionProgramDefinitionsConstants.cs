
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Cartridge values of <see cref="PhantoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class PhantoonInstructionProgramDefinitionsConstants
{
    /// <summary>First casual-flame timer word following the instruction block at $A7:CCFD.</summary>
    public const ushort AdjacentCasualFlameTimers = 0xccfd;

    /// <summary>Provides adjacent casual flame timer data that bounds Phantoon instruction programs.</summary>
    extension(PhantoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="PhantoonInstructionProgramDefinitionsConstants.AdjacentCasualFlameTimers"/>
        internal static ushort AdjacentCasualFlameTimers => PhantoonInstructionProgramDefinitionsConstants.AdjacentCasualFlameTimers;
    }
}
