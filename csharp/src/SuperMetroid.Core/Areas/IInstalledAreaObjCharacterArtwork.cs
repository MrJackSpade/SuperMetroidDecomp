namespace SuperMetroid.Core.Areas;

/// <summary>The installed area artwork that area OBJ character sources name.</summary>
internal interface IInstalledAreaObjCharacterArtwork
{
    /// <summary>The installed area artwork.</summary>
    /// <exception cref="InvalidOperationException">No area artwork is installed.</exception>
    AreaArtworkCatalog AreaTiles { get; }
}
