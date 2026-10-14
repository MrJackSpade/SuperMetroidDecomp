
namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Verification access to <see cref="CrocomireDeathState"/> members production does not use.</summary>
internal static class CrocomireDeathStateAccess
{
    /// <summary>Reads Crocomire's per-column erase heights during the melt sequence.</summary>
    extension(CrocomireDeathState self)
    {
        /// <summary>Resultant per-column erase heights, exposed read-only to the debugger.</summary>
        internal IReadOnlyList<byte> MeltingColumnHeights => self._meltingColumnHeights;
    }
}
