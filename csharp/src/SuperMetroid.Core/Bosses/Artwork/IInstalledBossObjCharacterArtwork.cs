namespace SuperMetroid.Core.Bosses.Artwork;

/// <summary>The installed boss artwork that boss OBJ character sources name.</summary>
internal interface IInstalledBossObjCharacterArtwork
{
    /// <summary>The installed boss artwork.</summary>
    /// <exception cref="InvalidOperationException">No boss artwork is installed.</exception>
    BossArtworkCatalog BossTiles { get; }
}
