namespace SuperMetroid.Core.Frontend.Menus.Artwork;

/// <summary>
/// The menu atlas cells that hold the large menu font: each letter is an upper and a lower half cell, and cell $0F is
/// the transparent cell the font shares. The lower half's shadow continues the upper half's ink across the cell
/// boundary. Most lower halves sit one atlas row (16 cells) below their upper half; P's bottom is $38 below D's shared
/// top $0D, and T's stem is $11.
/// </summary>
internal static class MenuLargeFontCellDefinitions
{
    /// <summary>$0F: the transparent cell inside the large-font run; it carries no ink.</summary>
    internal const int TransparentCell = 15;

    /// <summary>$52: the highest large-font cell (Z's lower half).</summary>
    internal const int LastCell = 82;

    /// <summary>The selected large-font cells, including V's $2D/$3E and Y's $41/$17 halves.</summary>
    internal static readonly MenuAtlasCellRange[] Cells =
    [
        new(10, 15), new(17), new(23), new(26, 31), new(33, 39), new(43, 45),
        new(47, 49), new(51, 59), new(62, 66), new(80), new(82),
    ];

    /// <summary>Each letter's upper and lower half cells; D and O share their halves, and C/G, D/P/Q/R and E/F share an upper half.</summary>
    internal static readonly IReadOnlyDictionary<char, MenuLargeFontLetter> Letters = new Dictionary<char, MenuLargeFontLetter>
    {
        ['A'] = new(10, 26),
        ['B'] = new(11, 27),
        ['C'] = new(12, 28),
        ['D'] = new(13, 29),
        ['E'] = new(14, 30),
        ['F'] = new(14, 31),
        ['G'] = new(12, 48),
        ['H'] = new(33, 49),
        ['J'] = new(35, 51),
        ['K'] = new(36, 52),
        ['L'] = new(37, 53),
        ['M'] = new(38, 54),
        ['N'] = new(39, 55),
        ['O'] = new(13, 29),
        ['P'] = new(13, 56),
        ['Q'] = new(13, 57),
        ['R'] = new(13, 58),
        ['S'] = new(43, 59),
        ['T'] = new(44, 17),
        ['V'] = new(45, 62),
        ['W'] = new(47, 63),
        ['X'] = new(64, 80),
        ['Y'] = new(65, 23),
        ['Z'] = new(66, 82),
    };

    /// <summary>Each lower-half cell's upper-half cell.</summary>
    internal static readonly IReadOnlyDictionary<int, int> UpperHalves = Letters.Values
        .DistinctBy(letter => letter.Lower)
        .ToDictionary(letter => letter.Lower, letter => letter.Upper);
}
