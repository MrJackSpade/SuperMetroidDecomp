namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusArmCannonPlacement"/> members production does not use.</summary>
internal static class SamusArmCannonPlacementAccess
{
    /// <summary>Inspects a pose's installed cover displacements.</summary>
    extension(SamusArmCannonPlacement self)
    {
        /// <summary>How many animation frames have an installed displacement.</summary>
        internal int FrameCount => self.Offsets.Count;
    }
}
