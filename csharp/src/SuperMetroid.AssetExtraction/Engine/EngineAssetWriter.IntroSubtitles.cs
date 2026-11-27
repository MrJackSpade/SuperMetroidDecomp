using SuperMetroid.AssetExtraction.Cartridge;
using SuperMetroid.AssetExtraction.Video;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Frontend.Intro;
using SuperMetroid.Core.Frontend.Intro.Artwork;

namespace SuperMetroid.AssetExtraction.Engine;

/// <summary>
/// The opening's Japanese subtitles: the cartridge builds each from its own font of Japanese characters, a page's
/// list of characters at a time, into a strip of the screen under the page. Each is written as one picture of that
/// strip, composed from the same font and lists, read from the ROM.
/// </summary>
public sealed partial class EngineAssetWriter
{
    /// <summary>The compressed font of the subtitles' characters, each a top and a bottom half of eight by eight.</summary>
    private const int IntroSubtitleFont = 0x95D713;

    /// <summary>
    /// The four rows of the cartridge's map of the background the subtitles are on that show them, set up with the
    /// first page; their first row is the screen's row <see cref="IntroSubtitleFirstRow"/>.
    /// </summary>
    private const int IntroSubtitleMap = 0x8BA72B;

    /// <summary>The screen's row, in cells of eight pixels, the subtitles' map begins on, before the background is raised.</summary>
    private const int IntroSubtitleFirstRow = 24;

    /// <summary>How many cells wide the background's map is, and how many of its rows show the subtitles.</summary>
    private const int IntroSubtitleMapColumns = 32, IntroSubtitleMapRows = 4;

    /// <summary>
    /// The first of the background's characters a subtitle is built into, and how many there are: the cartridge's
    /// characters for the background start at the video memory's word <c>$4000</c>, and it sends the subtitles to word
    /// <c>$4180</c>, eight words a character on.
    /// </summary>
    private const int IntroSubtitleFirstCharacter = 0x30, IntroSubtitleCharacters = 0x60;

    /// <summary>
    /// How far on, in bytes, the bottom half of a character is built from its top half: the cartridge's <c>$0300</c>.
    /// </summary>
    private const int IntroSubtitleBottomHalf = 0x300;

    /// <summary>The palette of the background the subtitles are drawn in, as their map names it.</summary>
    private const int IntroSubtitlePalette = 4;

    /// <summary>
    /// The colours one to three of that palette, which each page's subtitle sets as it is built: the opening's palette
    /// at <c>$8C:E3E9</c> from its seventeenth colour on.
    /// </summary>
    private const int IntroSubtitleColours = 0x8CE40B;

    /// <summary>Bytes in one character of eight by eight pixels, four colours each.</summary>
    private const int TwoColourBitsCharacterBytes = 16;

    /// <summary>
    /// Each page's subtitles, in the order of the pages: the lists of characters of the one built as the page begins,
    /// and of the one a second press builds, where the page has it. A list is where on the strip its characters go, how
    /// many there are, and the top and bottom half of each, as the font has them.
    /// </summary>
    private static readonly (IntroNarrationPageId Page, int[] First, int[] Second)[] IntroSubtitleLists =
    [
        (IntroNarrationPageId.ZebesBattle, [0x8BCF75, 0x8BCFBD], []),
        (IntroNarrationPageId.HatchedLarva, [0x8BCFFD, 0x8BD055], [0x8BD085, 0x8BD0E1]),
        (IntroNarrationPageId.DeliveryToCeres, [0x8BD0F9], [0x8BD15D]),
        (IntroNarrationPageId.ScientistsFindings, [0x8BD1B9, 0x8BD215], [0x8BD259, 0x8BD2A5]),
        (IntroNarrationPageId.DistressSignal, [0x8BD2D5], [0x8BD30D]),
        (IntroNarrationPageId.CeresUnderAttack, [0x8BD371], []),
    ];

    /// <summary>
    /// Writes each page's subtitles as pictures of the strip that shows them, and returns where the strip lies on the
    /// screen and, for each page, the picture shown as it begins and the one a second press shows, if it has one.
    /// </summary>
    /// <param name="folder">The opening's folder of pictures.</param>
    /// <param name="raise">How far the opening's background of words is raised on the screen.</param>
    /// <exception cref="InvalidDataException">The cartridge's subtitles are not laid out as this reads them.</exception>
    private (int X, int Y, Dictionary<IntroNarrationPageId, (string First, string? Second)> Pages) WriteIntroSubtitles(string folder, int raise)
    {
        byte[] rom;
        using (Stream source = File.OpenRead(_installation.RomPath))
            rom = SupportedCartridge.Read(source);
        var cartridge = new CartridgeImportAddressSpace(rom);
        byte[] font = RomDataReader.Decompress(cartridge, IntroSubtitleFont);

        // Colour none of a background shows what lies behind it.
        var colours = new RgbaPixel[4];
        for (int colour = 1; colour < colours.Length; colour++)
            colours[colour] = RgbColor.FromWord(RomDataReader.ReadWordFixedBank(cartridge, IntroSubtitleColours + 2 * (colour - 1))).ToRgbaPixel();

        // Where on the strip each of the subtitles' characters is shown.
        var cells = new Dictionary<int, (int Column, int Row)>();
        for (int cell = 0; cell < IntroSubtitleMapColumns * IntroSubtitleMapRows; cell++)
        {
            int entry = RomDataReader.ReadWordFixedBank(cartridge, IntroSubtitleMap + 2 * cell);
            int character = entry & 0x3FF;
            if (character is < IntroSubtitleFirstCharacter or >= IntroSubtitleFirstCharacter + IntroSubtitleCharacters)
                continue;
            if ((entry >> 10 & 7) != IntroSubtitlePalette || (entry & 0xC000) != 0 || !cells.TryAdd(character - IntroSubtitleFirstCharacter, (cell % IntroSubtitleMapColumns, cell / IntroSubtitleMapColumns)))
                throw new InvalidDataException($"The opening's map of its subtitles shows character ${character:X} in a way this does not read.");
        }

        if (cells.Count != IntroSubtitleCharacters)
            throw new InvalidDataException($"The opening's map of its subtitles shows {cells.Count} of their {IntroSubtitleCharacters} characters.");
        int left = cells.Values.Min(cell => cell.Column), top = cells.Values.Min(cell => cell.Row);
        int width = (cells.Values.Max(cell => cell.Column) - left + 1) * 8, height = (cells.Values.Max(cell => cell.Row) - top + 1) * 8;

        string Picture(string name, int[] lists)
        {
            // The characters the subtitle is built into, blank until a list fills them.
            var built = new byte[IntroSubtitleCharacters * TwoColourBitsCharacterBytes];
            foreach (int list in lists)
            {
                int at = RomDataReader.ReadWordFixedBank(cartridge, list), count = RomDataReader.ReadWordFixedBank(cartridge, list + 2);
                if (at % TwoColourBitsCharacterBytes != 0 || at + count * TwoColourBitsCharacterBytes > IntroSubtitleBottomHalf)
                    throw new InvalidDataException($"The opening's list of subtitle characters at ${list:X6} does not fit its strip.");
                for (int letter = 0; letter < count; letter++)
                {
                    int to = at + letter * TwoColourBitsCharacterBytes;
                    foreach ((int from, int into) in new[] { (RomDataReader.ReadWordFixedBank(cartridge, list + 4 + 4 * letter), to), (RomDataReader.ReadWordFixedBank(cartridge, list + 6 + 4 * letter), to + IntroSubtitleBottomHalf) })
                    {
                        if (from + TwoColourBitsCharacterBytes > font.Length)
                            throw new InvalidDataException($"The opening's list of subtitle characters at ${list:X6} names a character beyond its font.");
                        font.AsSpan(from, TwoColourBitsCharacterBytes).CopyTo(built.AsSpan(into));
                    }
                }
            }

            var pixels = new RgbaPixel[width * height];
            foreach ((int character, (int column, int row)) in cells)
            {
                for (int y = 0; y < 8; y++)
                {
                    byte low = built[character * TwoColourBitsCharacterBytes + 2 * y], high = built[character * TwoColourBitsCharacterBytes + 2 * y + 1];
                    for (int x = 0; x < 8; x++)
                    {
                        int colour = (low >> (7 - x) & 1) | (high >> (7 - x) & 1) << 1;
                        pixels[((row - top) * 8 + y) * width + (column - left) * 8 + x] = colours[colour];
                    }
                }
            }

            string path = $"{folder}/{name}.png";
            _tree.WritePicture(path, width, height, pixels);
            return path;
        }

        var pages = new Dictionary<IntroNarrationPageId, (string, string?)>();
        foreach ((IntroNarrationPageId page, int[] first, int[] second) in IntroSubtitleLists)
        {
            if ((second.Length > 0) != (IntroNarrationPageDefinitions.Pages[page].Closing == IntroNarrationPageClosing.AwaitsSecondSubtitle))
                throw new InvalidDataException($"The opening's page {page} waits for a second press where it has no second subtitle, or has one it does not wait for.");
            string name = "subtitle_" + Named(page);
            pages[page] = (Picture(name, first), second.Length > 0 ? Picture(name + "_second", second) : null);
        }

        if (pages.Count != Enum.GetValues<IntroNarrationPageId>().Length)
            throw new InvalidDataException("A page of the opening has no subtitle.");
        return (left * 8, (IntroSubtitleFirstRow + top) * 8 - raise, pages);
    }
}
