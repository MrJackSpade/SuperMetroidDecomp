namespace SuperMetroid.Core.Bosses.Crocomire;

/// <summary>Reads what Crocomire's colours show from the body and death colourings his own updates draw from.</summary>
internal static class CrocomireColoursAccess
{
    /// <summary>Reads his body's and his death's colourings.</summary>
    extension(CrocomireColours self)
    {
        /// <summary>Whether his body shows the hurt flash.</summary>
        internal bool HurtFlash => self.Body == CrocomireBodyColours.HurtFlash;

        /// <summary>Whether his body shows its fight colours rather than his room's own.</summary>
        internal bool ShowsFightBody => self.Body != CrocomireBodyColours.Room;

        /// <summary>Whether his death has shown the skeleton's arm.</summary>
        internal bool ShowsSkeletonArm => self.DeathColours == CrocomireDeathColours.WallSpikesAndSkeletonArm;

        /// <summary>Whether his death has shown the wall's spikes.</summary>
        internal bool ShowsWallSpikes => self.DeathColours != CrocomireDeathColours.None;
    }
}
