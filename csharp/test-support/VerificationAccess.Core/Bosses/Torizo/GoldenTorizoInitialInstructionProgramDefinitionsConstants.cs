
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="GoldenTorizoInitialInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class GoldenTorizoInitialInstructionProgramDefinitionsConstants
{
    /// <summary>The first bank-$AA sleep instruction at $AA:C9E0.</summary>
    public const ushort Sleep = 0xc9e0;

    /// <summary>Provides the native sleep program that seeds Golden Torizo's initial instruction state.</summary>
    extension(GoldenTorizoInitialInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="GoldenTorizoInitialInstructionProgramDefinitionsConstants.Sleep"/>
        internal static ushort Sleep => GoldenTorizoInitialInstructionProgramDefinitionsConstants.Sleep;
    }
}
