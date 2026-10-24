using SuperMetroid.AssetExtraction.Video;
namespace SuperMetroid.Core.Hardware;

/// <summary>The floor-map fills verification's native memory images replay (#1404); the game builds its floor cells itself.</summary>
internal static class Mode7VramAccess
{
    /// <summary>Fills a floor map in a native memory image.</summary>
    extension(Mode7Vram self)
    {
        /// <summary>Writes <paramref name="count"/> copies of <paramref name="tileNumber"/> starting at map cell <paramref name="cell"/>.</summary>
        public void FillMap(byte tileNumber, int count, int cell = 0)
        {
            var tiles = new byte[count];
            tiles.AsSpan().Fill(tileNumber);
            self.LoadMap(tiles, cell);
        }
    }
}
