namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>A recording's resume of a <see cref="BabyMetroidEnemyState"/>'s membrane through the membrane's own player.</summary>
internal static class BabyMetroidEnemyStateAccess
{
    /// <summary>Resumes a recorded Baby Metroid's membrane pulse.</summary>
    extension(BabyMetroidEnemyState self)
    {
        /// <summary>
        /// Shows the membrane's pulse at <paramref name="pace"/> standing on <paramref name="phase"/>, which it moves on from
        /// after <paramref name="updates"/> more updates, as a recording resumes it: the membrane's player shows that place of
        /// the pace's pulse, as <see cref="BabyMetroidMembraneAnimations.StandingOn"/> starts one.
        /// </summary>
        /// <exception cref="ArgumentOutOfRangeException"><paramref name="updates"/> is below one.</exception>
        internal void ResumeMembrane(BabyMetroidMembranePace pace, BabyMetroidMembranePulse phase, int updates)
        {
            global::SuperMetroid.Core.Animation.Animation<BabyMetroidMembranePace> pulse = BabyMetroidMembraneAnimations.At(pace);
            self.Membrane.ShowAt(pulse, BabyMetroidMembraneAnimations.Segments.PlaceOfFrame(pulse, pace, phase), updates);
        }
    }
}
