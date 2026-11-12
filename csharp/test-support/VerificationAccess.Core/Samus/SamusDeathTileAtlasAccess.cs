using SuperMetroid.AssetExtraction.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>Verification access to <see cref="SamusDeathTileAtlas"/> members production does not use.</summary>
internal static class SamusDeathTileAtlasAccess
{
    /// <summary>Exposes <see cref="SamusDeathTileAtlas"/> state that only verification reads.</summary>
    extension(SamusDeathTileAtlas self)
    {
        /// <summary>SHA-256 of all five selected death-explosion character pages.</summary>
        public string ContentIdentity => SelectedPresentationHash.Create(nameof(SamusDeathTileAtlas),
            content => content.Append("death characters", self.planar));
    }
}
