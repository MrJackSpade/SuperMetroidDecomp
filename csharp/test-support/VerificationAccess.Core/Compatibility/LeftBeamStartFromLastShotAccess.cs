namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="LeftBeamStartFromLastShot"/> members production does not use.</summary>
internal static class LeftBeamStartFromLastShotAccess
{
    /// <summary>Exposes <see cref="LeftBeamStartFromLastShot"/> state that only verification reads.</summary>
    extension(LeftBeamStartFromLastShot self)
    {
        /// <summary>The horizontal fraction of a pixel <paramref name="place"/> keeps for its next projectile.</summary>
        public PixelFraction Kept(int place) => self.kept[place];
    }
}
