namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusArmCannonPlacement"/> members production does not use.</summary>
internal static class SamusArmCannonPlacementAccess
{
    /// <summary>Inspects a pose's installed cover displacements.</summary>
    extension(SamusArmCannonPlacement self)
    {
        /// <summary>The animation frames with an installed displacement, from frame zero.</summary>
        internal int FrameCount => self.offsets.Length;
    }
}
