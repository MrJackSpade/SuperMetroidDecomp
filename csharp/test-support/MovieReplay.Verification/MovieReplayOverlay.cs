using SuperMetroid.Core.Assets;

namespace SuperMetroid.TestSupport.MovieReplay.Verification;

/// <summary>
/// The caption band a recorded replay shows under each frame: two lines of 5x7 text in a black band below the picture,
/// so it never covers the game. It is drawn at the frame's own resolution, and the video scales it with the picture.
/// </summary>
internal static class MovieReplayOverlay
{
    /// <summary>The band's height in frame pixels: two seven-pixel lines with room above, between and below them.</summary>
    internal const int Height = 22;

    private const int GlyphWidth = 5;
    private const int GlyphHeight = 7;
    private const int Advance = GlyphWidth + 1;
    private const int Margin = 2;
    private static readonly RgbaPixel Ink = new(255, 255, 255);
    private static readonly RgbaPixel Paper = new(0, 0, 0);

    /// <summary>Each glyph's seven rows, five columns each, the leftmost column first; '#' is ink.</summary>
    private static readonly Dictionary<char, string[]> Glyphs = new()
    {
        [' '] = [".....", ".....", ".....", ".....", ".....", ".....", "....."],
        ['0'] = [".###.", "#...#", "#..##", "#.#.#", "##..#", "#...#", ".###."],
        ['1'] = ["..#..", ".##..", "..#..", "..#..", "..#..", "..#..", ".###."],
        ['2'] = [".###.", "#...#", "....#", "...#.", "..#..", ".#...", "#####"],
        ['3'] = ["#####", "...#.", "..#..", "...#.", "....#", "#...#", ".###."],
        ['4'] = ["...#.", "..##.", ".#.#.", "#..#.", "#####", "...#.", "...#."],
        ['5'] = ["#####", "#....", "####.", "....#", "....#", "#...#", ".###."],
        ['6'] = ["..##.", ".#...", "#....", "####.", "#...#", "#...#", ".###."],
        ['7'] = ["#####", "....#", "...#.", "..#..", ".#...", ".#...", ".#..."],
        ['8'] = [".###.", "#...#", "#...#", ".###.", "#...#", "#...#", ".###."],
        ['9'] = [".###.", "#...#", "#...#", ".####", "....#", "...#.", ".##.."],
        ['A'] = [".###.", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
        ['B'] = ["####.", "#...#", "#...#", "####.", "#...#", "#...#", "####."],
        ['C'] = [".###.", "#...#", "#....", "#....", "#....", "#...#", ".###."],
        ['D'] = ["###..", "#..#.", "#...#", "#...#", "#...#", "#..#.", "###.."],
        ['E'] = ["#####", "#....", "#....", "####.", "#....", "#....", "#####"],
        ['F'] = ["#####", "#....", "#....", "####.", "#....", "#....", "#...."],
        ['G'] = [".###.", "#...#", "#....", "#.###", "#...#", "#...#", ".####"],
        ['H'] = ["#...#", "#...#", "#...#", "#####", "#...#", "#...#", "#...#"],
        ['I'] = [".###.", "..#..", "..#..", "..#..", "..#..", "..#..", ".###."],
        ['J'] = ["..###", "...#.", "...#.", "...#.", "...#.", "#..#.", ".##.."],
        ['K'] = ["#...#", "#..#.", "#.#..", "##...", "#.#..", "#..#.", "#...#"],
        ['L'] = ["#....", "#....", "#....", "#....", "#....", "#....", "#####"],
        ['M'] = ["#...#", "##.##", "#.#.#", "#.#.#", "#...#", "#...#", "#...#"],
        ['N'] = ["#...#", "#...#", "##..#", "#.#.#", "#..##", "#...#", "#...#"],
        ['O'] = [".###.", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
        ['P'] = ["####.", "#...#", "#...#", "####.", "#....", "#....", "#...."],
        ['Q'] = [".###.", "#...#", "#...#", "#...#", "#.#.#", "#..#.", ".##.#"],
        ['R'] = ["####.", "#...#", "#...#", "####.", "#.#..", "#..#.", "#...#"],
        ['S'] = [".####", "#....", "#....", ".###.", "....#", "....#", "####."],
        ['T'] = ["#####", "..#..", "..#..", "..#..", "..#..", "..#..", "..#.."],
        ['U'] = ["#...#", "#...#", "#...#", "#...#", "#...#", "#...#", ".###."],
        ['V'] = ["#...#", "#...#", "#...#", "#...#", "#...#", ".#.#.", "..#.."],
        ['W'] = ["#...#", "#...#", "#...#", "#.#.#", "#.#.#", "#.#.#", ".#.#."],
        ['X'] = ["#...#", "#...#", ".#.#.", "..#..", ".#.#.", "#...#", "#...#"],
        ['Y'] = ["#...#", "#...#", ".#.#.", "..#..", "..#..", "..#..", "..#.."],
        ['Z'] = ["#####", "....#", "...#.", "..#..", ".#...", "#....", "#####"],
        [':'] = [".....", "..#..", "..#..", ".....", "..#..", "..#..", "....."],
        ['-'] = [".....", ".....", ".....", "#####", ".....", ".....", "....."],
        ['/'] = ["....#", "....#", "...#.", "..#..", ".#...", "#....", "#...."],
        ['.'] = [".....", ".....", ".....", ".....", ".....", ".##..", ".##.."],
        ['('] = ["...#.", "..#..", ".#...", ".#...", ".#...", "..#..", "...#."],
        [')'] = [".#...", "..#..", "...#.", "...#.", "...#.", "..#..", ".#..."],
    };

    /// <summary>
    /// The frame <paramref name="picture"/> with the band appended below it, showing <paramref name="first"/> on the band's
    /// first line and <paramref name="second"/> on its second, each cut at the band's width.
    /// </summary>
    /// <param name="picture">The game's frame, row by row.</param>
    /// <param name="width">The frame's width in pixels.</param>
    /// <param name="height">The frame's height in pixels.</param>
    /// <param name="first">The first line.</param>
    /// <param name="second">The second line.</param>
    internal static RgbaPixel[] Captioned(RgbaPixel[] picture, int width, int height, string first, string second)
    {
        var captioned = new RgbaPixel[width * (height + Height)];
        picture.AsSpan(0, width * height).CopyTo(captioned);
        captioned.AsSpan(width * height).Fill(Paper);
        Write(captioned, width, height + Margin, first);
        Write(captioned, width, height + Margin + GlyphHeight + Margin * 2, second);
        return captioned;
    }

    /// <summary>The characters a line can hold in a band <paramref name="width"/> pixels wide.</summary>
    internal static int Columns(int width) => (width - Margin * 2 + 1) / Advance;

    private static void Write(RgbaPixel[] frame, int width, int top, string text)
    {
        int columns = Columns(width);
        string shown = text.ToUpperInvariant();
        for (int index = 0; index < shown.Length && index < columns; index++)
        {
            string[] rows = Glyphs.TryGetValue(shown[index], out string[]? glyph)
                ? glyph
                : throw new InvalidOperationException($"The caption font has no glyph for '{shown[index]}' in \"{text}\".");
            int left = Margin + index * Advance;
            for (int y = 0; y < GlyphHeight; y++)
            {
                for (int x = 0; x < GlyphWidth; x++)
                {
                    if (rows[y][x] == '#')
                        frame[(top + y) * width + left + x] = Ink;
                }
            }
        }
    }
}
