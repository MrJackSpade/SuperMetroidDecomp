using SuperMetroid.Core.Game;

/// <summary>Verification access to <see cref="SamusDrainedState"/> members production does not use.</summary>
internal static class SamusDrainedStateAccess
{
    /// <summary>Checks whether the drained-Samus sequence has spawned its persistent Hyper Beam palette FX.</summary>
    extension(SamusDrainedState self)
    {
        /// <summary>
        /// True after controller function three writes equipped beams <c>$1009</c>, writes
        /// hyper-beam flag <c>$8000</c>, and spawns the persistent bank-$8D palette object.
        /// </summary>
        internal bool HyperBeamPaletteFxRequested => self.HyperBeamPaletteFx.IsActive;
    }
}
