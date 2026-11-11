using SuperMetroid.Core.Rooms.Scrolling;

namespace SuperMetroid.AssetExtraction.Rooms;

/// <summary>Verification's view of which page each row of a scrolling sky shows.</summary>
internal static class SkyTilemapAccess
{
    /// <summary>The rows of a scrolling sky.</summary>
    extension(SkyTilemap sky)
    {
        /// <summary>The sky page <paramref name="row"/>'s chunk shows in the sky's chunk table.</summary>
        internal int RowPage(int row) => ScrollingSkyChunks.PageOf(sky.Variant, row / SkyTilemap.RowsPerChunk);
    }
}
