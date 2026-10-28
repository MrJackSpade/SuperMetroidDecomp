namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusBodyTileTransfer"/> members production does not use.</summary>
internal static class SamusBodyTileTransferAccess
{
    /// <summary>Uses the catalog's own pose pointers and frame table for transfer geometry.</summary>
    extension(SamusBodyTileTransfer self)
    {
        /// <summary>Derives transfer layout from one upper or lower tile slot and installed body art.</summary>
        internal SamusBodyTileTransfer WithTransferGeometry(InstalledSamusBodyArtwork body, bool upper, int set, int position) =>
            self.WithTransferGeometry(body, upper, set, position, body.PoseFirstFrames, body.Frames);
    }
}
