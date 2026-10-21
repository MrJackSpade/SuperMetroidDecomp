namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusBodyTileDefinition"/> members production does not use.</summary>
internal static class SamusBodyTileDefinitionAccess
{
    /// <summary>Uses the catalog's own pose pointers and frame table for transfer geometry.</summary>
    extension(SamusBodyTileDefinition self)
    {
        /// <summary>Derives transfer layout from one upper or lower tile slot and installed body art.</summary>
        internal SamusBodyTileDefinition WithTransferGeometry(SamusBodyArtworkCatalog body, bool upper, int set, int position) =>
            self.WithTransferGeometry(body, upper, set, position, body.PoseFirstFrames, body.Frames);
    }
}
