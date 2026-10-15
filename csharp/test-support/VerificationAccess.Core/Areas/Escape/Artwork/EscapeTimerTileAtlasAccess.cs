using SuperMetroid.Core.Hardware;

namespace SuperMetroid.Core.Areas.Escape.Artwork;

/// <summary>Verification access to <see cref="EscapeTimerTileAtlas"/> members production does not use.</summary>
internal static class EscapeTimerTileAtlasAccess
{
    /// <summary>Checks and queues the two native escape-timer DMA transfers.</summary>
    extension(EscapeTimerTileAtlas self)
    {
        /// <summary>Queues both native records in their original order and at their original destinations.</summary>
        [PpuLayoutBoundary("codec: escape timer queued DMA records at their native VRAM words")]
        internal void QueueTo(VramWriteQueue queue)
        {
            Ensure.NotNull(queue);
            if (((byte[])(self.Transfer())).Length != EscapeTimerTileAtlasFormat.TotalByteCount)
                throw new InvalidDataException("Escape timer artwork no longer matches its native transfer pages.");
            queue.EnqueueAsset(VramAssetId.EscapeTimerFirstTiles,
                EscapeTimerTileAtlasFormat.FirstByteCount, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.FirstDestinationWord));
            queue.EnqueueAsset(VramAssetId.EscapeTimerSecondTiles,
                EscapeTimerTileAtlasFormat.SecondByteCount, VramDmaDestination.FromNative(EscapeTimerTileAtlasFormat.SecondDestinationWord));
        }
    }
}
