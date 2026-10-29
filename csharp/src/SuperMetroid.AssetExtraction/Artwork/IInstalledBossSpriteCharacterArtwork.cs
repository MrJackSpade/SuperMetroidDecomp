using SuperMetroid.Core.Bosses.Artwork;

namespace SuperMetroid.AssetExtraction.Artwork;

/// <summary>The installed boss artwork that boss sprite character sources name.</summary>
internal interface IInstalledBossSpriteCharacterArtwork
{
    /// <summary>The installed boss artwork.</summary>
    /// <exception cref="InvalidOperationException">No boss artwork is installed.</exception>
    BossArtworkCatalog BossTiles { get; }
}
