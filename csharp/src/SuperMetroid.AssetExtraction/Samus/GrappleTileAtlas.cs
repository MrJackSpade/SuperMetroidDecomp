using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>
/// The grapple's characters, which only the import reads (#1636): the grapple sheet is composed from them. Its sprite,
/// flare and swing documents are the installed grapple presentation (<see cref="InstalledGrapplePresentation"/>).
/// </summary>
public sealed class GrappleTileAtlas
{
    /// <summary>
    /// The grapple characters as encoded 4-bpp bytes (#1596): the four endpoint characters, then the horizontal, diagonal
    /// and vertical rope orientations' four characters each.
    /// </summary>
    private readonly byte[] tiles;

    /// <summary>Keeps every grapple character.</summary>
    private GrappleTileAtlas(byte[] tiles) => this.tiles = tiles;

    /// <summary>Compiles the 128-by-8 indexed Grapple page into sixteen row-major SNES 4-bpp characters.</summary>
    /// <param name="png">The caller-owned indexed page stream; pixel indices must fit the 4-bpp domain.</param>
    /// <returns>The immutable tile atlas.</returns>
    public static GrappleTileAtlas Load(Stream png)
    {
        var image = IndexedPng.Read(png, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4));
    }
    /// <summary>Resolves one endpoint character or four-character rope-orientation transfer.</summary>
    /// <param name="transfer">One of the seven grapple transfers (<see cref="GrappleTileDefinitions.Transfers"/>).</param>
    /// <returns>The current 32-byte endpoint or 128-byte segment transfer.</returns>
    private ReadOnlyMemory<byte> Resolve(GrappleTileTransfer transfer) => tiles.AsMemory(transfer.AtlasOffset, transfer.ByteCount);
    /// <summary>
    /// The endpoint character of <paramref name="pointFrame"/> followed by the four rope characters the rope
    /// <paramref name="orientation"/> selects: OBJ characters $20-$24 as the beam's sprites name them.
    /// </summary>
    private SpriteArtwork Tiles(GrappleEndpointFrame pointFrame, GrappleRopeOrientation orientation)
    {
        GrappleTileTransfer point = GrappleTileSelection.PointTransferFor(pointFrame);
        GrappleTileTransfer segments = GrappleTileSelection.SegmentTransferFor(orientation);
        return tileSets.GetOrAdd(new GrappleTileSetKey(point.Asset, segments.Asset), key => new SpriteArtwork(
            new SpriteArtworkId(SpriteArtworkKind.GrappleTiles, new GrappleTilesVariant(key.Point, key.Segments)),
            [.. Resolve(point).Span, .. Resolve(segments).Span]));
    }

    /// <summary>
    /// The standard OBJ page with <see cref="Tiles"/> over characters $20-$24: the beam's sprites name OBJ characters
    /// directly, and one stock rope frame names tile $1FF outside the grapple's own characters.
    /// </summary>
    public SpriteArtwork OnStandardObjects(SpriteArtwork standardObjects, GrappleEndpointFrame pointFrame, GrappleRopeOrientation orientation)
    {
        Ensure.NotNull(standardObjects);
        if (!ReferenceEquals(pagesComposedOn, standardObjects))
        {
            pages.Clear();
            pagesComposedOn = standardObjects;
        }
        SpriteArtwork tiles = Tiles(pointFrame, orientation);
        return pages.GetOrAdd(tiles.Id, id => SpriteArtwork.Overlay(
            id.InKind(SpriteArtworkKind.GrappleTilesOnStandardObjects), standardObjects, tiles,
            GrappleTileDefinitions.FirstCharacter));
    }

    /// <summary>Composed pages by grapple tile selection, for <see cref="pagesComposedOn"/>.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<SpriteArtworkId, SpriteArtwork> pages = new();

    /// <summary>A Grapple tile sheet, distinguished by the endpoint frame and the rope characters it holds.</summary>
    /// <param name="Point">The endpoint frame's characters.</param>
    /// <param name="Segments">The rope's characters.</param>
    private sealed record GrappleTilesVariant(InstalledArtworkId Point, InstalledArtworkId Segments) : SpriteArtworkVariant;

    /// <summary>The standard page <see cref="pages"/> were composed over.</summary>
    private SpriteArtwork? pagesComposedOn;

    /// <summary>Endpoint-plus-rope sheets already built.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<GrappleTileSetKey, SpriteArtwork> tileSets = new();
}
