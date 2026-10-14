using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.SporeSpawn;

/// <summary>Verification access to <see cref="SporeSpawnEnemyState"/> members production does not use.</summary>
internal static class SporeSpawnEnemyStateAccess
{
    /// <summary>Reads Spore Spawn's target CGRAM buffer separately from the colors currently displayed.</summary>
    extension(SporeSpawnEnemyState self)
    {
        /// <summary>
        /// The global target-palette words written by $A5:EA2A/$E91C. The runtime currently
        /// presents completed room fades directly in CGRAM, but retaining this buffer makes the
        /// cartridge's separate target/current ownership inspectable and testable.
        /// </summary>
        internal ReadOnlyMemory<Bgr555> TargetPalette => self._targetPalette;
    }
}
