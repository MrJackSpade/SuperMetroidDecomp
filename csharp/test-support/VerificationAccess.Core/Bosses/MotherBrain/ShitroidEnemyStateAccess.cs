namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="ShitroidEnemyState"/> members production does not use.</summary>
internal static class ShitroidEnemyStateAccess
{
    /// <summary>Exposes <see cref="ShitroidEnemyState"/> state that only verification reads.</summary>
    extension(ShitroidEnemyState self)
    {
        /// <summary>
        /// Sets the membrane's pulse at <paramref name="pace"/> standing on <paramref name="phase"/>, which it moves on from
        /// after <paramref name="updates"/> more updates, as a recording resumes it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
        internal void ResumeMembrane(ShitroidMembranePace pace, ShitroidMembranePulse phase, int updates)
        {
            self.MembranePace = pace;
            self.Membrane = ShitroidMembraneAnimations.StandingOn(pace, phase, updates);
        }
    }
}
