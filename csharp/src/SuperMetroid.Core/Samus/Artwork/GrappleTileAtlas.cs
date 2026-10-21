using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Samus.Artwork;

/// <summary>Indexed Grapple artwork resolved at NMI, without embedding PNG data in pending state.</summary>
public sealed class GrappleTileAtlas : IVramAssetProvider, IInstalledArtworkTransferSource
{
    /// <summary>$9A:8220-829F/8A20-8A9F: eight reviewed drawn electric-stroke masks (64 binary rows)
    /// or independently supplied multicolor pixels. Their spur layout is visual content, not beam collision geometry.</summary>
    internal readonly byte[] independentTiles;
    /// <summary>Whether rope rows can be retained as compact binary ink coverage.</summary>
    private readonly bool singleInk;
    /// <summary>Custom first endpoint tile when it differs from the calculated filled diamond.</summary>
    internal readonly byte[]? firstPoint;
    /// <summary>Custom second endpoint tile when it differs from the calculated hollow diamond.</summary>
    internal readonly byte[]? secondPoint;
    /// <summary>Custom third endpoint tile when it differs from the calculated tipped square.</summary>
    internal readonly byte[]? thirdPoint;
    /// <summary>Custom fourth endpoint tile when it differs from the calculated clipped diamond.</summary>
    internal readonly byte[]? fourthPoint;
    /// <summary>Custom vertical rope tiles when the native reflection of horizontal rows does not match.</summary>
    internal readonly byte[]? verticalSegments;

    /// <summary>Gets the optional installed Grapple endpoint and beam sprite compositions.</summary>
    public GrappleSpriteCatalog? Sprites { get; }

    /// <summary>Gets the optional installed standing and running Grapple flare offsets.</summary>
    public ChargeFlarePlacementCatalog? FlarePlacement { get; }

    /// <summary>Gets the optional installed angle-to-displayed-frame mapping for Samus's swing pose.</summary>
    public GrappleSwingFrameCatalog? SwingFrames { get; }
    /// <summary>Retains only independently supplied tile differences and associated optional visual catalogs.</summary>
    private GrappleTileAtlas(byte[] tiles, GrappleSpriteCatalog? sprites, ChargeFlarePlacementCatalog? flarePlacement, GrappleSwingFrameCatalog? swingFrames)
    {
        Sprites = sprites; FlarePlacement = flarePlacement; SwingFrames = swingFrames;
        byte[]? coverage = GrappleBeamTilePatterns.InkCoverage(tiles.AsSpan(128, 256));
        singleInk = coverage is not null;
        independentTiles = coverage ?? tiles.AsSpan(128, 256).ToArray();
        if (!tiles.AsSpan(0, 32).SequenceEqual(GrappleBeamTilePatterns.Point(GrapplePointFrame.FilledDiamond)))
            firstPoint = tiles.AsSpan(0, 32).ToArray();
        if (!tiles.AsSpan(32, 32).SequenceEqual(GrappleBeamTilePatterns.Point(GrapplePointFrame.HollowDiamond)))
            secondPoint = tiles.AsSpan(32, 32).ToArray();
        if (!tiles.AsSpan(64, 32).SequenceEqual(GrappleBeamTilePatterns.Point(GrapplePointFrame.TippedSquare)))
            thirdPoint = tiles.AsSpan(64, 32).ToArray();
        if (!tiles.AsSpan(96, 32).SequenceEqual(GrappleBeamTilePatterns.Point(GrapplePointFrame.ClippedDiamond)))
            fourthPoint = tiles.AsSpan(96, 32).ToArray();
        if (!tiles.AsSpan(384, 128).SequenceEqual(GrappleBeamTilePatterns.VerticalSegments(tiles.AsSpan(128, 128))))
            verticalSegments = tiles.AsSpan(384, 128).ToArray();
    }
    /// <summary>Compiles the 128-by-8 indexed Grapple PNG into sixteen row-major SNES 4-bpp characters.</summary>
    /// <param name="png">The caller-owned indexed PNG stream; pixel indices must fit the 4-bpp domain.</param>
    /// <param name="sprites">Optional installed Grapple sprite compositions.</param>
    /// <param name="flarePlacement">Optional installed Grapple flare placement catalog.</param>
    /// <param name="swingFrames">Optional installed swing-frame selection catalog.</param>
    /// <returns>The immutable tile atlas and its associated optional visual assets.</returns>
    public static GrappleTileAtlas Load(Stream png, GrappleSpriteCatalog? sprites = null, ChargeFlarePlacementCatalog? flarePlacement = null, GrappleSwingFrameCatalog? swingFrames = null)
    {
        var image = IndexedPng.Read(png, new(GrappleTileDefinitions.Width, GrappleTileDefinitions.Height));
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Size, 4), sprites, flarePlacement, swingFrames);
    }
    /// <summary>Resolves one endpoint character or four-character rope-orientation transfer.</summary>
    /// <param name="asset">One of the seven Grapple-owned VRAM asset identities.</param>
    /// <returns>The current 32-byte endpoint or 128-byte segment transfer.</returns>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        var transfer = GrappleTileDefinitions.TransferFor(asset);
        return asset switch
        {
            VramAssetId.GrapplePointFirstTiles => firstPoint ?? GrappleBeamTilePatterns.Point(GrapplePointFrame.FilledDiamond),
            VramAssetId.GrapplePointSecondTiles => secondPoint ?? GrappleBeamTilePatterns.Point(GrapplePointFrame.HollowDiamond),
            VramAssetId.GrapplePointThirdTiles => thirdPoint ?? GrappleBeamTilePatterns.Point(GrapplePointFrame.TippedSquare),
            VramAssetId.GrapplePointFourthTiles => fourthPoint ?? GrappleBeamTilePatterns.Point(GrapplePointFrame.ClippedDiamond),
            VramAssetId.GrappleVerticalSegmentTiles => verticalSegments ??
                GrappleBeamTilePatterns.VerticalSegments(SegmentBytes(0).Span),
            VramAssetId.GrappleHorizontalSegmentTiles or VramAssetId.GrappleDiagonalSegmentTiles =>
                SegmentBytes(transfer.AtlasOffset - 128),
            VramAssetId.None or
                VramAssetId.StandardHudTiles or
                VramAssetId.EscapeTimerFirstTiles or
                VramAssetId.EscapeTimerSecondTiles or
                VramAssetId.BeamPowerTiles or
                VramAssetId.BeamWaveTiles or
                VramAssetId.BeamIceTiles or
                VramAssetId.BeamIceWaveTiles or
                VramAssetId.BeamSpazerTiles or
                VramAssetId.BeamSpazerWaveTiles or
                VramAssetId.BeamSpazerIceTiles or
                VramAssetId.BeamSpazerIceWaveTiles or
                VramAssetId.BeamPlasmaTiles or
                VramAssetId.BeamPlasmaWaveTiles or
                VramAssetId.BeamPlasmaIceTiles or
                VramAssetId.BeamPlasmaIceWaveTiles or
                VramAssetId.ProjectileIceWaveTrailTiles or
                VramAssetId.ProjectileMissileTrailTiles or
                VramAssetId.KraidBg3RestoreQuarter0 or
                VramAssetId.KraidBg3RestoreQuarter1 or
                VramAssetId.KraidBg3RestoreQuarter2 or
                VramAssetId.KraidBg3RestoreQuarter3 or
                VramAssetId.GunshipLiftoffFirstTiles or
                VramAssetId.GunshipLiftoffSecondTiles or
                VramAssetId.GunshipLiftoffThirdTiles or
                VramAssetId.GunshipLiftoffFourthTiles or
                VramAssetId.GunshipLiftoffFifthTiles or
                VramAssetId.BeamChainsawTiles or
                VramAssetId.BeamSpacetimeTiles or
                VramAssetId.StandardObjectTiles or
                VramAssetId.HudTopRowTilemap => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
            _ => throw new InvalidDataException($"Not a Grapple tile asset: {asset}."),
        };
    }
    /// <summary>Expands compact rope ink rows or slices the independently colored segment bytes.</summary>
    private ReadOnlyMemory<byte> SegmentBytes(int offset) => singleInk
        ? GrappleBeamTilePatterns.EncodeInk(independentTiles.AsSpan(offset / 4, 32))
        : independentTiles.AsMemory(offset, 128);
    /// <summary>Resolves native endpoint/segment queue records without requiring an eager queue rewrite.</summary>
    public bool TryResolve(int sourceAddress, int byteCount, out ReadOnlyMemory<byte> data)
    {
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            if (transfer.SourceAddress == sourceAddress && transfer.ByteCount == byteCount)
            {
                data = Resolve(transfer.Asset);
                return true;
            }
        data = default;
        return false;
    }
    /// <summary>
    /// The endpoint character for animation <paramref name="pointFrame"/> followed by the four rope characters the
    /// grapple <paramref name="angle"/> selects: OBJ characters $20-$24 as the beam's sprites name them.
    /// </summary>
    public SpriteArtwork Tiles(int pointFrame, SnesAngle angle)
    {
        VramAssetId point = GrappleTileDefinitions.PointAssetFor(pointFrame);
        VramAssetId segments = GrappleTileDefinitions.SegmentAssetFor(angle);
        return tileSets.GetOrAdd(new GrappleTileSetKey(point, segments), key => new SpriteArtwork(
            new SpriteArtworkId(SpriteArtworkKind.GrappleTiles, SnesLittleEndian.Word(checked((byte)key.Segments), checked((byte)key.Point))),
            [.. Resolve(key.Point).Span, .. Resolve(key.Segments).Span]));
    }

    /// <summary>
    /// The standard OBJ page with <see cref="Tiles"/> over characters $20-$24: the beam's sprites name OBJ characters
    /// directly, and one stock rope frame names tile $1FF outside the grapple's own characters.
    /// </summary>
    public SpriteArtwork OnStandardObjects(SpriteArtwork standardObjects, int pointFrame, SnesAngle angle)
    {
        Ensure.NotNull(standardObjects);
        if (!ReferenceEquals(pagesComposedOn, standardObjects))
        {
            pages.Clear();
            pagesComposedOn = standardObjects;
        }
        SpriteArtwork tiles = Tiles(pointFrame, angle);
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
    /// <summary>Restored legacy transfers keep their order and destination but resolve current artwork at NMI.</summary>
    [PpuLayoutBoundary("codec: rebinds restored native queued-DMA records to installed Grapple artwork")]
    public void RebindPendingWrites(VramWriteQueue queue)
    {
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            queue.RebindBusSource(transfer.SourceAddress, checked((ushort)Resolve(transfer.Asset).Length), transfer.Asset);
        // That legacy second frame is the restored animation ordinal one: the second endpoint tiles.
        queue.RebindBusSource(GrappleTileDefinitions.LegacyAlternateEndpointSource,
            checked((ushort)Resolve(VramAssetId.GrapplePointSecondTiles).Length), VramAssetId.GrapplePointSecondTiles);
    }
}
