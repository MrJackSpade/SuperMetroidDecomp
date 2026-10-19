
namespace SuperMetroid.Core.Bosses.Phantoon;

/// <summary>Verification access to <see cref="PhantoonWaveHdmaState"/> members production does not use.</summary>
internal static class PhantoonWaveHdmaStateAccess
{
    /// <summary>Reads the cycle of horizontal scrolls Phantoon's wave now holds (#1353).</summary>
    extension(PhantoonWaveHdmaState self)
    {
        /// <summary>One cycle of the wave's per-line scrolls, built from its current model.</summary>
        internal ushort[] ScrollCycle => (self.CurrentWave ?? throw new InvalidOperationException("Phantoon's wave has no setup yet.")).Cycle();
    }
}
