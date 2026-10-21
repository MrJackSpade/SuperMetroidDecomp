using SuperMetroid.Core.Progression;


namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Verification access to <see cref="SamusProjectileTypeWord"/> members production does not use.</summary>
internal static class SamusProjectileTypeWordAccess
{
    /// <summary>Tests the live-marker bit in a packed Samus projectile type word.</summary>
    extension(SamusProjectileTypeWord self)
    {
        /// <summary>Tests the projectile word's native live marker.</summary>
        internal bool IsLive => (self.Raw & SamusProjectileTypeWord.LiveMarker) != 0;
    }
}
