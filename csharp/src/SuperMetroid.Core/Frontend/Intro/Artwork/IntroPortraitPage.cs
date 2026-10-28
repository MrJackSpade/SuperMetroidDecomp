using SuperMetroid.Core.Assets;
using static SuperMetroid.Core.Frontend.Intro.Artwork.IntroPortraitTilemapDefinitions;

namespace SuperMetroid.Core.Frontend.Intro.Artwork;

/// <summary>
/// Draws the intro's Samus-head BG2 page (<see cref="IntroPortraitTilemapDefinitions"/>): the silhouette's atlas cells and the
/// hand-placed shoulder, fin and chin fragments.
/// </summary>
internal static class IntroPortraitPage
{
    /// <summary>Builds the centered portrait BG page from atlas occupancy and its hand-placed edge fragments.</summary>
    internal static BackgroundTile[] Compile()
    {
        const int columns = IntroCinematicArtworkFormat.TileColumns;
        int left = (columns - AtlasSide) / 2;
        var output = new BackgroundTile[IntroCinematicArtworkFormat.BackgroundPageByteCount / sizeof(ushort)];
        Array.Fill(output, new BackgroundTile(BlankTile, Palette, highPriority: false));
        void Put(int x, int y, int tile) => output[y * columns + x] = new BackgroundTile(tile, Palette, highPriority: false);
        void Run(int x, int y, int tile, int count)
        {
            for (int column = 0; column < count; column++) Put(x + column, y, tile + column);
        }
        for (int row = 0; row < AtlasSide; row++)
        for (int column = 0; column < AtlasSide; column++)
            if ((SilhouetteRows[row] & (1 << column)) != 0)
                Put(left + column, Top + row, AtlasOrigin + row * AtlasSide + column);

        // Left shoulder fragments reuse free atlas cells around the main head drawing.
        Run(4, 12, AtlasOrigin + 11, 2);
        Put(4, 13, AtlasOrigin + 13);
        for (int row = 0; row < 2; row++) Run(5, 13 + row, AtlasOrigin + 26 + row * AtlasSide, 3);
        Run(5, 15, AtlasOrigin, 3);
        Run(6, 16, AtlasOrigin + 17, 2);
        Put(7, 17, AtlasOrigin + 49);
        Put(7, 18, AtlasOrigin + 3);

        // The near-right fin follows one atlas column, while the far-right tip
        // stitches two vertically packed two-cell strips side by side.
        for (int row = 0; row < 3; row++) Put(24, 11 + row, AtlasOrigin + 141 + row * AtlasSide);
        for (int column = 0; column < 2; column++)
        for (int row = 0; row < 2; row++)
            Put(26 + column, 13 + row, AtlasOrigin + 191 + (column * 2 + row) * AtlasSide);

        // Seven chin cells occupy two source rows because the atlas keeps other
        // head fragments in the adjacent cells; displayed runs remain contiguous.
        Run(13, 21, AtlasOrigin + 89, 4);
        Run(17, 21, AtlasOrigin + 105, 3);
        return output;
    }
}
