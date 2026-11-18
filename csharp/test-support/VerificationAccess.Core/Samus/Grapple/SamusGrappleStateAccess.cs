using SuperMetroid.Core.Samus.Projectiles;

namespace SuperMetroid.Core.Samus.Grapple;

/// <summary>Verification access to <see cref="SamusGrappleState"/> members production does not use.</summary>
internal static class SamusGrappleStateAccess
{
    /// <summary>Exposes <see cref="SamusGrappleState"/> state that only verification reads.</summary>
    extension(SamusGrappleState self)
    {
        /// <summary>The installed flare origins, or none before they are installed; never changes physical hand offsets.</summary>
        public SuperMetroid.Core.Samus.Artwork.ChargeFlarePlacements FlarePlacement => self.Artwork.GrappleFlarePlacement;
        /// <summary>The installed swing pictures, or none before they are installed; never a collision/body offset source.</summary>
        public SuperMetroid.Core.Samus.Artwork.InstalledGrappleSwingFrames SwingFrames => self.Artwork.GrappleSwingFrames;

        /// <summary>
        /// The grapple flare's picture as the original numbers the main flare's pictures (#2396): sixteen opening pictures,
        /// then the loop's fourteen.
        /// </summary>
        internal int FlareFrame
        {
            get => self.Flare.NativePicture ?? throw new InvalidOperationException("The grapple's flare shows no picture.");
            set => self.Flare.ShowNative(value, self.Flare.NativeWait);
        }

        /// <summary>The grapple flare's wait as the original keeps it: the updates its picture still shows less one.</summary>
        internal WrappingCountdown FlareAnimationTimer
        {
            get => self.Flare.NativeWait;
            set => self.Flare.ShowNative(self.FlareFrame, value);
        }
    }
}
