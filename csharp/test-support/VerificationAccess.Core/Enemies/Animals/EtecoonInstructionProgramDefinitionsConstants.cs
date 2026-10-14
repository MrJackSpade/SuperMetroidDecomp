
namespace SuperMetroid.Core.Enemies.Animals;

/// <summary>Cartridge values of <see cref="EtecoonInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class EtecoonInstructionProgramDefinitionsConstants
{
    /// <summary>The first Etecoon movement constant after the programs, at $A7:E900.</summary>
    public const ushort FirstAdjacentMechanicsData = 0xe900;

    /// <summary>Provides the mechanics-data boundary adjacent to Etecoon instruction programs.</summary>
    extension(EtecoonInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="EtecoonInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData"/>
        internal static ushort FirstAdjacentMechanicsData => EtecoonInstructionProgramDefinitionsConstants.FirstAdjacentMechanicsData;
    }
}
