namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="LeftBeamStartFromLastShot"/> members production does not use.</summary>
internal static class LeftBeamStartFromLastShotAccess
{
    /// <summary>Exposes <see cref="LeftBeamStartFromLastShot"/> state that only verification reads.</summary>
    extension(LeftBeamStartFromLastShot self)
    {
        /// <summary>What <paramref name="place"/> keeps for its next projectile, 0 through 65535.</summary>
        public int Kept(int place) => self.kept[place];
    }
}
