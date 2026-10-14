using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="ShitroidEnemyState"/> members production does not use.</summary>
internal static class ShitroidEnemyStateAccess
{
    /// <summary>Reads the separate 256-color target palette used by the Baby's Shitroid phase.</summary>
    extension(ShitroidEnemyState self)
    {
        /// <summary>The independent 256-color target palette written during initialization.</summary>
        internal ReadOnlyMemory<Bgr555> TargetPalette => self._targetPalette;
    }
}
