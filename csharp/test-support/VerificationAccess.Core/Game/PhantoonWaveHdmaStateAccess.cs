using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="PhantoonWaveHdmaState"/> members production does not use.</summary>
internal static class PhantoonWaveHdmaStateAccess
{
    /// <summary>Reads the active portion of Phantoon's circular wave-scroll table.</summary>
    extension(PhantoonWaveHdmaState self)
    {
        /// <summary>Only the initialized prefix of the room's wave-scroll cycle.</summary>
        internal ReadOnlySpan<ushort> ScrollCycle => self._cycle.AsSpan(0, self._cycleLength);
    }
}
