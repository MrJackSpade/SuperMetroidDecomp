using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed Grapple artwork resolved at NMI, without embedding PNG data in pending state.</summary>
public sealed class GrappleTileAtlas : IVramAssetProvider, IInstalledArtworkTransferSource
{
    /// <summary>$9A:8220-829F/8A20-8A9F: eight reviewed drawn electric-stroke masks (64 binary rows)
    /// or independently supplied multicolor pixels. Their spur layout is visual content, not beam collision geometry.</summary>
    private readonly byte[] independentTiles;
    private readonly bool singleInk;
    private readonly byte[]? firstPoint;
    private readonly byte[]? secondPoint;
    private readonly byte[]? thirdPoint;
    private readonly byte[]? fourthPoint;
    private readonly byte[]? verticalSegments;

    /// <summary>Gets the optional installed Grapple endpoint and beam sprite compositions.</summary>
    public GrappleSpriteCatalog? Sprites { get; }

    /// <summary>Gets the optional installed standing and running Grapple flare offsets.</summary>
    public ChargeFlarePlacementCatalog? FlarePlacement { get; }

    /// <summary>Gets the optional installed angle-to-displayed-frame mapping for Samus's swing pose.</summary>
    public GrappleSwingFrameCatalog? SwingFrames { get; }
    private GrappleTileAtlas(byte[] tiles, GrappleSpriteCatalog? sprites, ChargeFlarePlacementCatalog? flarePlacement, GrappleSwingFrameCatalog? swingFrames)
    {
        Sprites = sprites; FlarePlacement = flarePlacement; SwingFrames = swingFrames;
        byte[]? coverage = GrappleBeamTilePatterns.InkCoverage(tiles.AsSpan(128, 256));
        singleInk = coverage is not null;
        independentTiles = coverage ?? tiles.AsSpan(128, 256).ToArray();
        if (!tiles.AsSpan(0, 32).SequenceEqual(GrappleBeamTilePatterns.Point(0)))
            firstPoint = tiles.AsSpan(0, 32).ToArray();
        if (!tiles.AsSpan(32, 32).SequenceEqual(GrappleBeamTilePatterns.Point(1)))
            secondPoint = tiles.AsSpan(32, 32).ToArray();
        if (!tiles.AsSpan(64, 32).SequenceEqual(GrappleBeamTilePatterns.Point(2)))
            thirdPoint = tiles.AsSpan(64, 32).ToArray();
        if (!tiles.AsSpan(96, 32).SequenceEqual(GrappleBeamTilePatterns.Point(3)))
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
        var image = IndexedPng.Read(png, GrappleTileDefinitions.Width, GrappleTileDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4), sprites, flarePlacement, swingFrames);
    }
    /// <summary>Resolves one endpoint character or four-character rope-orientation transfer.</summary>
    /// <param name="asset">One of the seven Grapple-owned VRAM asset identities.</param>
    /// <returns>The current 32-byte endpoint or 128-byte segment transfer.</returns>
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        var transfer = GrappleTileDefinitions.TransferFor(asset);
        return asset switch
        {
            VramAssetId.GrapplePointFirstTiles => firstPoint ?? GrappleBeamTilePatterns.Point(0),
            VramAssetId.GrapplePointSecondTiles => secondPoint ?? GrappleBeamTilePatterns.Point(1),
            VramAssetId.GrapplePointThirdTiles => thirdPoint ?? GrappleBeamTilePatterns.Point(2),
            VramAssetId.GrapplePointFourthTiles => fourthPoint ?? GrappleBeamTilePatterns.Point(3),
            VramAssetId.GrappleVerticalSegmentTiles => verticalSegments ??
                GrappleBeamTilePatterns.VerticalSegments(SegmentBytes(0).Span),
            _ => SegmentBytes(transfer.AtlasOffset - 128),
        };
    }
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
    /// <summary>Queues the selected endpoint-animation character for VRAM word <c>$6200</c>.</summary>
    /// <param name="queue">The deferred VRAM write queue.</param>
    /// <param name="frame">Endpoint animation ordinal from zero through three.</param>
    public void QueuePoint(VramWriteQueue queue, ushort frame)
    {
        var asset = GrappleTileDefinitions.PointAssetFor(frame);
        queue.EnqueueAsset(asset, checked((ushort)Resolve(asset).Length), GrappleTileDefinitions.PointDestination);
    }
    /// <summary>Queues the four horizontal, diagonal, or vertical rope characters selected by the native angle fold.</summary>
    /// <param name="queue">The deferred VRAM write queue.</param>
    /// <param name="angle">Unsigned native Grapple angle whose 64 sectors select the rope orientation.</param>
    public void QueueSegments(VramWriteQueue queue, ushort angle)
    {
        var asset = GrappleTileDefinitions.SegmentAssetFor(angle);
        queue.EnqueueAsset(asset, checked((ushort)Resolve(asset).Length), GrappleTileDefinitions.SegmentDestination);
    }
    /// <summary>Restored legacy transfers keep their order and destination but resolve current artwork at NMI.</summary>
    public void RebindPendingWrites(VramWriteQueue queue)
    {
        foreach (var transfer in GrappleTileDefinitions.Transfers)
            queue.RebindBusSource(transfer.SourceAddress, checked((ushort)Resolve(transfer.Asset).Length), transfer.Asset);
        // That legacy second frame is the restored animation ordinal one: the second endpoint tiles.
        queue.RebindBusSource(GrappleTileDefinitions.LegacyAlternateEndpointSource,
            checked((ushort)Resolve(VramAssetId.GrapplePointSecondTiles).Length), VramAssetId.GrapplePointSecondTiles);
    }
}
