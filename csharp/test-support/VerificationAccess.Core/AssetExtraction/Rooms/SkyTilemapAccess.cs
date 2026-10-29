using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification's view of which rows of a scrolling sky show a page.</summary>
internal static class SkyTilemapAccess
{
    /// <summary>The rows of a scrolling sky.</summary>
    extension(SkyTilemap sky)
    {
        /// <summary>Whether <paramref name="row"/>'s chunk selects a sky page in the sky's chunk table.</summary>
        internal bool RowShown(int row) => ScrollingSkyChunks.TryGetPage(sky.Variant, row / SkyTilemap.RowsPerChunk, out _);
    }
}
