
namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>Cartridge values of <see cref="TorizoChozoOrbInstructionProgramDefinitions"/> that only verification reads.</summary>
internal static class TorizoChozoOrbInstructionProgramDefinitionsConstants
{
    /// <summary><c>InstList_EnemyProjectile_TorizoChozoOrbs_Right</c> at $86:AB1D.</summary>
    public const ushort MovingRight = 0xab1d;

    /// <summary>Provides the Chozo orb right-moving program for Torizo instruction comparisons.</summary>
    extension(TorizoChozoOrbInstructionProgramDefinitions)
    {
        /// <inheritdoc cref="TorizoChozoOrbInstructionProgramDefinitionsConstants.MovingRight"/>
        internal static ushort MovingRight => TorizoChozoOrbInstructionProgramDefinitionsConstants.MovingRight;
    }
}
