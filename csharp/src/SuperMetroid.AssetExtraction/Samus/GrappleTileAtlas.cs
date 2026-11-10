using SuperMetroid.AssetExtraction.Artwork;
using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.Core;
using SuperMetroid.Core.Samus.Artwork;

namespace SuperMetroid.AssetExtraction.Samus;

/// <summary>The installed grapple characters, with the grapple sprite, flare and swing catalogs; the grapple sheet is composed from them.</summary>
public sealed class GrappleTileAtlas : IInstalledArtworkContent
{
    /// <summary>
    /// The installed grapple characters as encoded 4-bpp bytes, as installed (#1596): the four endpoint characters, then
    /// the horizontal, diagonal and vertical rope orientations' four characters each.
    /// </summary>
    private readonly byte[] tiles;

    /// <summary>Gets the optional installed Grapple endpoint and beam sprite compositions.</summary>
    public InstalledGrappleSprites? Sprites { get; }

    /// <summary>Gets the optional installed standing and running Grapple flare offsets.</summary>
    public ChargeFlarePlacements? FlarePlacement { get; }

    /// <summary>Gets the optional installed angle-to-displayed-frame mapping for Samus's swing pose.</summary>
    public InstalledGrappleSwingFrames? SwingFrames { get; }
    /// <summary>Keeps every installed grapple character and the associated visual catalogs.</summary>
    private GrappleTileAtlas(byte[] tiles, InstalledGrappleSprites? sprites, ChargeFlarePlacements? flarePlacement, InstalledGrappleSwingFrames? swingFrames)
    {
        Sprites = sprites; FlarePlacement = flarePlacement; SwingFrames = swingFrames;
        this.tiles = tiles;
    }
    /// <summary>Compiles the 128-by-8 indexed Grapple PNG into sixteen row-major SNES 4-bpp characters.</summary>
    /// <param name="png">The caller-owned indexed PNG stream; pixel indices must fit the 4-bpp domain.</param>
    /// <param name="sprites">Optional installed Grapple sprite compositions.</param>
    /// <param name="flarePlacement">Optional installed Grapple flare placement catalog.</param>
    /// <param name="swingFrames">Optional installed swing-frame selection catalog.</param>
    /// <returns>The immutable tile atlas and its associated optional visual assets.</returns>
    public static GrappleTileAtlas Load(Stream png, InstalledGrappleSprites? sprites = null, ChargeFlarePlacements? flarePlacement = null, InstalledGrappleSwingFrames? swingFrames = null)
    {
        var image = IndexedPng.Read(png, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4), sprites, flarePlacement, swingFrames);
    }
    /// <summary>Resolves one endpoint character or four-character rope-orientation transfer.</summary>
    /// <param name="asset">One of the seven Grapple-owned VRAM asset identities.</param>
    /// <returns>The current 32-byte endpoint or 128-byte segment transfer.</returns>
    public ReadOnlyMemory<byte> Resolve(InstalledArtworkId asset)
    {
        // The transfer names only Grapple-owned assets; any other asset is rejected there.
        GrappleTileTransfer transfer = GrappleTileSelection.TransferFor(asset);
        return tiles.AsMemory(transfer.AtlasOffset, transfer.ByteCount);
    }
    /// <summary>
    /// The endpoint character for animation <paramref name="pointFrame"/> followed by the four rope characters the
    /// rope <paramref name="orientation"/> selects: OBJ characters $20-$24 as the beam's sprites name them.
    /// </summary>
    public SpriteArtwork Tiles(int pointFrame, GrappleRopeOrientation orientation)
    {
        InstalledArtworkId point = GrappleTileSelection.PointAssetFor(pointFrame);
        InstalledArtworkId segments = GrappleTileSelection.SegmentAssetFor(orientation);
        return tileSets.GetOrAdd(new GrappleTileSetKey(point, segments), key => new SpriteArtwork(
            new SpriteArtworkId(SpriteArtworkKind.GrappleTiles, LittleEndianBytes.Word(checked((byte)key.Segments), checked((byte)key.Point))),
            [.. Resolve(key.Point).Span, .. Resolve(key.Segments).Span]));
    }

    /// <summary>
    /// The standard OBJ page with <see cref="Tiles"/> over characters $20-$24: the beam's sprites name OBJ characters
    /// directly, and one stock rope frame names tile $1FF outside the grapple's own characters.
    /// </summary>
    public SpriteArtwork OnStandardObjects(SpriteArtwork standardObjects, int pointFrame, GrappleRopeOrientation orientation)
    {
        Ensure.NotNull(standardObjects);
        if (!ReferenceEquals(pagesComposedOn, standardObjects))
        {
            pages.Clear();
            pagesComposedOn = standardObjects;
        }
        SpriteArtwork tiles = Tiles(pointFrame, orientation);
        return pages.GetOrAdd(tiles.Id.Variant, variant => SpriteArtwork.Overlay(
            new SpriteArtworkId(SpriteArtworkKind.GrappleTilesOnStandardObjects, variant), standardObjects, tiles,
            GrappleTileDefinitions.FirstCharacter));
    }

    /// <summary>Composed pages by grapple tile selection, for <see cref="pagesComposedOn"/>.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<int, SpriteArtwork> pages = new();

    /// <summary>The standard page <see cref="pages"/> were composed over.</summary>
    private SpriteArtwork? pagesComposedOn;

    /// <summary>Endpoint-plus-rope sheets already built.</summary>
    private readonly System.Collections.Concurrent.ConcurrentDictionary<GrappleTileSetKey, SpriteArtwork> tileSets = new();
}
