using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed Grapple artwork resolved at NMI, without embedding PNG data in pending state.</summary>
public sealed class GrappleTileAtlas : IVramAssetProvider, IInstalledArtworkTransferSource
{
    private readonly byte[] tiles;
    public GrappleSpriteCatalog? Sprites { get; }
    public ChargeFlarePlacementCatalog? FlarePlacement { get; }
    public GrappleSwingFrameCatalog? SwingFrames { get; }
    private GrappleTileAtlas(byte[] tiles, GrappleSpriteCatalog? sprites, ChargeFlarePlacementCatalog? flarePlacement, GrappleSwingFrameCatalog? swingFrames)
    { this.tiles = tiles; Sprites = sprites; FlarePlacement = flarePlacement; SwingFrames = swingFrames; }
    public static GrappleTileAtlas Load(Stream png, GrappleSpriteCatalog? sprites = null, ChargeFlarePlacementCatalog? flarePlacement = null, GrappleSwingFrameCatalog? swingFrames = null)
    {
        var image = IndexedPng.Read(png, GrappleTileDefinitions.Width, GrappleTileDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4), sprites, flarePlacement, swingFrames);
    }
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        var transfer = GrappleTileDefinitions.TransferFor(asset);
        return tiles.AsMemory(transfer.AtlasOffset, transfer.ByteCount);
    }
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
    public void QueuePoint(VramWriteQueue queue, ushort frame)
    {
        var asset = GrappleTileDefinitions.PointAssetFor(frame);
        queue.EnqueueAsset(asset, checked((ushort)Resolve(asset).Length), GrappleTileDefinitions.PointDestination);
    }
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
    }
}
