namespace SuperMetroid.Core.Compatibility;

/// <summary>Verification access to <see cref="WrappingCountdown"/> members production does not use.</summary>
internal static class WrappingCountdownAccess2
{
    /// <summary>Exposes <see cref="WrappingCountdown"/> state that only verification reads.</summary>
    extension(WrappingCountdown self)
    {
        /// <summary>Whether at least <paramref name="frames"/> frames are left; a countdown run past zero counts as a large count.</summary>
        /// <param name="frames">The frames compared, 0 through 65535.</param>
        public bool IsAtLeast(int frames) => self.RemainingFrames >= frames;
    }
}
