namespace SuperMetroid.Core.Bosses.MotherBrain;

/// <summary>Verification access to <see cref="MotherBrainTurretAnimation"/> members production does not use.</summary>
internal static class MotherBrainTurretAnimationAccess
{
    /// <summary>Exposes <see cref="MotherBrainTurretAnimation"/> state that only verification reads.</summary>
    extension(MotherBrainTurretAnimation self)
    {
        /// <summary>The program of the segment played.</summary>
        internal MotherBrainTurretSegment Program => self.PlayingSegment.Animation;
    }
}
