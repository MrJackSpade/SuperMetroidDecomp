namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="BabyMetroidEnemyState"/> members production does not use.</summary>
internal static class BabyMetroidEnemyStateAccess
{
    /// <summary>Exposes <see cref="BabyMetroidEnemyState"/> state that only verification reads.</summary>
    extension(BabyMetroidEnemyState self)
    {
        /// <summary>
        /// Sets the membrane's pulse at <paramref name="pace"/> standing on <paramref name="phase"/>, which it moves on from
        /// after <paramref name="updates"/> more updates, as a recording resumes it.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
        internal void ResumeMembrane(BabyMetroidMembranePace pace, BabyMetroidMembranePulse phase, int updates)
        {
            global::SuperMetroid.Core.PrivateState.Set(self, "MembranePace", (global::SuperMetroid.Core.Bosses.MotherBrain.BabyMetroidMembranePace)(pace));
            self.Membrane = BabyMetroidMembraneAnimations.StandingOn(pace, phase, updates);
        }
    }
}
