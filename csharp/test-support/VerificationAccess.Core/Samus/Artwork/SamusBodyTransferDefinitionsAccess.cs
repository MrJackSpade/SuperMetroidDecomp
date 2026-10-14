namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Verification access to <see cref="SamusBodyTransferDefinitions"/> members production does not use.</summary>
internal static class SamusBodyTransferDefinitionsAccess
{
    /// <summary>Checks canonical transfer size using in-group OAM and the separate pixel payload.</summary>
    extension(SamusBodyTransferDefinitions)
    {
        /// <summary>
        /// Canonical in-group OAM establishes the first-row footprint; the remainder
        /// of the separately owned packed pixel payload is the second transfer.
        /// Physical cross-group selectors remain valid but do not redefine a glyph's
        /// canonical composition from an unrelated pose's OAM.
        /// </summary>
        internal static bool TryFirstSize(SamusBodyArtworkCatalog body, bool upper, int set, int position,
            int payloadBytes, out ushort firstSize)
        {
            if (SamusBodyTransferDefinitions.TrySelectedPacking(upper, set, position, payloadBytes, out firstSize))
                return true;
            return SamusBodyTransferDefinitions.TryFirstSize(body, new SamusBodyTileSlot(upper, set, position), payloadBytes, body.PosePointers, body.Frames, out firstSize);
        }
    }
}
