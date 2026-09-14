using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Assets;

/// <summary>Indexed Grapple artwork resolved at NMI, without embedding PNG data in pending state.</summary>
public sealed class GrappleTileAtlas : IVramAssetProvider
{
    private readonly byte[] tiles;
    public GrappleSpriteCatalog? Sprites { get; }
    private GrappleTileAtlas(byte[] tiles, GrappleSpriteCatalog? sprites) { this.tiles = tiles; Sprites = sprites; }
    public static GrappleTileAtlas Load(Stream png, GrappleSpriteCatalog? sprites = null)
    {
        var image = IndexedPng.Read(png, GrappleTileDefinitions.Width, GrappleTileDefinitions.Height);
        return new(SnesPlanarTileEncoder.Encode(image.Pixels, image.Width, image.Height, 4), sprites);
    }
    public ReadOnlyMemory<byte> Resolve(VramAssetId asset)
    {
        var transfer = GrappleTileDefinitions.TransferFor(asset);
        return tiles.AsMemory(transfer.AtlasOffset, transfer.ByteCount);
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
