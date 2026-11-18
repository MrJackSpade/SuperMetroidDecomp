namespace SuperMetroid.Core.Samus.Projectiles;

/// <summary>Development-tool instance members of <see cref="BombSpreadFlight"/>.</summary>
internal static class BombSpreadFlightToolingExtensions
{
    /// <summary>Exposes the spread bomb's horizontal velocity in the launch table's packed form to tools and verification.</summary>
    extension(BombSpreadFlight self)
    {
        /// <summary>
        /// The spread bomb's horizontal velocity in the launch table's packed form, 0 through 65535, for traces that print
        /// it: the speed's whole pixels with the leftward bit above its 256ths. A bomb with no horizontal speed packs as
        /// rightward.
        /// </summary>
        public int PackedHorizontalVelocity => BombSpreadLaunchWords.Pack(self.HorizontalVelocity);
    }
}
