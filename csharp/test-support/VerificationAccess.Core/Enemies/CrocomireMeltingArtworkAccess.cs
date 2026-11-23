using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Enemies;

/// <summary>Verification access to <see cref="CrocomireMeltingArtwork"/> members production does not use.</summary>
internal static class CrocomireMeltingArtworkAccess
{
    /// <summary>Exposes <see cref="CrocomireMeltingArtwork"/> state that only verification reads.</summary>
    extension(CrocomireMeltingArtwork self)
    {
        /// <summary>Canonical selected presentation data; no derived field is added to debugger states.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create("enemy-crocomire-melt-v2", content =>
            {
                content.Append("first", self.flesh.Characters.Span);
                content.Append("second", self.bareSkull.Characters.Span);
                content.AppendTiles("first-map", self.fleshTilemap);
                content.AppendTiles("second-map", self.bareSkullTilemap);
            });
    }
}
