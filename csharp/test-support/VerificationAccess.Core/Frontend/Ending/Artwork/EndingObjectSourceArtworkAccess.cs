using SuperMetroid.AssetExtraction.Rooms;
using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Frontend.Ending.Artwork;

/// <summary>Verification access to <see cref="EndingObjectSourceArtwork"/> members production does not use.</summary>
internal static class EndingObjectSourceArtworkAccess
{
    /// <summary>Exposes <see cref="EndingObjectSourceArtwork"/> state that only verification reads.</summary>
    extension(EndingObjectSourceArtwork self)
    {
        /// <summary>The four explosion maps in their installed fragments' order.</summary>
        public IReadOnlyList<RoomBackgroundTilemapAtlas> Fragments => self.fragments;
        /// <summary>
        /// Identity of every selected ending/credits map and the compositions its sheets are composed from; the characters are
        /// import data the installation never holds (#1636).
        /// </summary>
        public string ContentIdentity => SelectedPresentationHash.Create("EndingObjectArtworkCatalog-v2", content =>
        {
            content.AppendTiles("waiting-map", self.WaitingTilemap.Tiles.Span);
            content.AppendTiles("post-credits-fragment-b", self.PostCreditsFragmentB.Tiles.Span);
            content.AppendTiles("logo-map", self.PostShotLogoMap.Tiles.Span);
            content.Append("fragment-count", self.Fragments.Count);
            foreach (RoomBackgroundTilemapAtlas fragment in self.Fragments)
                content.AppendTiles("fragment", fragment.Tiles.Span);
            content.Append("cloud-sprites", Convert.FromHexString(self.CloudSprites.ContentIdentity));
            content.Append("explosion-sprites", Convert.FromHexString(self.ExplosionSprites.ContentIdentity));
            content.Append("completion-text-sprites", Convert.FromHexString(self.CompletionTextSprites.ContentIdentity));
            content.Append("reward-sprites", Convert.FromHexString(self.RewardSprites.ContentIdentity));
            content.Append("logo-sprites", Convert.FromHexString(self.LogoSprites.ContentIdentity));
        });
    }
}
