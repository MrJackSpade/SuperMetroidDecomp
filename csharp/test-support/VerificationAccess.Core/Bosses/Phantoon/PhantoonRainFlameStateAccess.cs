namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonRainFlameState"/> members production does not use.</summary>
internal static class PhantoonRainFlameStateAccess
{
    /// <summary>Reads how long a rain flame still hangs.</summary>
    extension(PhantoonRainFlameState self)
    {
        /// <summary>The updates the hanging flame still hangs, the one it falls on included (#2469).</summary>
        /// <exception cref="InvalidOperationException">The flame has fallen.</exception>
        internal int HangUpdates => self.Falls
            ? throw new InvalidOperationException("A rain flame that has fallen no longer hangs.")
            : self.HangWait.UpdatesLeft;
    }
}
