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
    }
}
