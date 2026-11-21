using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Samus;
using SuperMetroid.AssetExtraction.CartridgeTables;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>
/// Reads <see cref="GrappleTileAtlas"/>'s characters as the game shows them: on the page it composes over the standard
/// objects for an endpoint frame and a rope orientation (<see cref="GrappleTileAtlas.OnStandardObjects"/>).
/// </summary>
internal static class GrappleTileAtlasAccess
{
    /// <summary>The standard object page the checks compose over: every character blank, 512 of them.</summary>
    private static SpriteArtwork BlankStandardObjects { get; } =
        new(new SpriteArtworkId(SpriteArtworkKind.StandardObjects), new byte[512 * SpriteArtwork.CharacterByteCount]);

    /// <summary>Reads the grapple characters a composed page shows.</summary>
    extension(GrappleTileAtlas self)
    {
        /// <summary>
        /// The bytes <paramref name="transfer"/> shows on the page the game composes: an endpoint frame's character at the
        /// grapple's first character, or a rope orientation's four after it, for the frame or orientation that selects it.
        /// </summary>
        /// <exception cref="ArgumentException">No endpoint frame or rope orientation selects the transfer.</exception>
        public ReadOnlyMemory<byte> Shown(GrappleTileTransfer transfer)
        {
            int first = GrappleTileDefinitions.FirstCharacter * SpriteArtwork.CharacterByteCount;
            foreach (GrappleEndpointFrame frame in Enum.GetValues<GrappleEndpointFrame>())
            {
                if (GrappleTileSelection.PointTransferFor(frame) == transfer)
                    return self.OnStandardObjects(BlankStandardObjects, frame, GrappleRopeOrientation.Horizontal).Characters.Slice(first, transfer.ByteCount).ToArray();
            }
            foreach (GrappleRopeOrientation orientation in Enum.GetValues<GrappleRopeOrientation>())
            {
                if (GrappleTileSelection.SegmentTransferFor(orientation) == transfer)
                    return self.OnStandardObjects(BlankStandardObjects, GrappleEndpointFrame.Point, orientation).Characters.Slice(first + SpriteArtwork.CharacterByteCount, transfer.ByteCount).ToArray();
            }
            throw new ArgumentException($"No endpoint frame or rope orientation selects grapple transfer {transfer}.", nameof(transfer));
        }

        /// <summary>
        /// The grapple's five characters on the page the game composes for <paramref name="frame"/> and
        /// <paramref name="orientation"/>: the endpoint frame's character, then the rope's four.
        /// </summary>
        public ReadOnlyMemory<byte> ShownTiles(GrappleEndpointFrame frame, GrappleRopeOrientation orientation) =>
            self.OnStandardObjects(BlankStandardObjects, frame, orientation).Characters.Slice(
                GrappleTileDefinitions.FirstCharacter * SpriteArtwork.CharacterByteCount, 5 * SpriteArtwork.CharacterByteCount).ToArray();

        /// <summary>Resolves native endpoint/segment queue records as the page the game composes shows them.</summary>
        public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
        {
            foreach (var transfer in GrappleTileDefinitions.Transfers)
                if (transfer.SourceAddress == sourceAddress && transfer.ByteCount == byteCount)
                {
                    data = self.Shown(transfer);
                    return true;
                }
            data = default;
            return false;
        }
    }
}
