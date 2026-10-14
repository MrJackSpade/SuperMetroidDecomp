
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Cartridge values of <see cref="DachoraInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class DachoraInstructionProgramDefinitionsConstants
{
    /// <summary>The retail-unused left-facing charge program at $A7:F3F1.</summary>
    public const ushort UnusedChargeLeft = 0xf3f1;

    /// <summary>Provides Dachora's unused left-charge instruction pointer for adjacent program coverage checks.</summary>
    extension(DachoraInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="DachoraInstructionProgramDefinitionsConstants.UnusedChargeLeft"/>
        internal static ushort UnusedChargeLeft => DachoraInstructionProgramDefinitionsConstants.UnusedChargeLeft;
    }
}
