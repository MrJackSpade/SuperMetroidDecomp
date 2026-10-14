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
    internal const int TransparentCell = 0x0f;

    /// <summary>$52: the highest large-font cell (Z's lower half).</summary>
    internal const int LastCell = 0x52;

    /// <summary>The selected large-font cells, including V's $2D/$3E and Y's $41/$17 halves.</summary>
    internal static readonly MenuAtlasCellRange[] Cells =
    [
        new(0x0a, 0x0f), new(0x11), new(0x17), new(0x1a, 0x1f), new(0x21, 0x27), new(0x2b, 0x2d),
        new(0x2f, 0x31), new(0x33, 0x3b), new(0x3e, 0x42), new(0x50), new(0x52),
    ];

    /// <summary>Each letter's upper and lower half cells; D and O share their halves, and C/G, D/P/Q/R and E/F share an upper half.</summary>
    internal static readonly IReadOnlyDictionary<char, MenuLargeFontLetter> Letters = new Dictionary<char, MenuLargeFontLetter>
    {
        ['A'] = new(0x0a, 0x1a),
        ['B'] = new(0x0b, 0x1b),
        ['C'] = new(0x0c, 0x1c),
        ['D'] = new(0x0d, 0x1d),
        ['E'] = new(0x0e, 0x1e),
        ['F'] = new(0x0e, 0x1f),
        ['G'] = new(0x0c, 0x30),
        ['H'] = new(0x21, 0x31),
        ['J'] = new(0x23, 0x33),
        ['K'] = new(0x24, 0x34),
        ['L'] = new(0x25, 0x35),
        ['M'] = new(0x26, 0x36),
        ['N'] = new(0x27, 0x37),
        ['O'] = new(0x0d, 0x1d),
        ['P'] = new(0x0d, 0x38),
        ['Q'] = new(0x0d, 0x39),
        ['R'] = new(0x0d, 0x3a),
        ['S'] = new(0x2b, 0x3b),
        ['T'] = new(0x2c, 0x11),
        ['V'] = new(0x2d, 0x3e),
        ['W'] = new(0x2f, 0x3f),
        ['X'] = new(0x40, 0x50),
        ['Y'] = new(0x41, 0x17),
        ['Z'] = new(0x42, 0x52),
    };

    /// <summary>Each lower-half cell's upper-half cell.</summary>
    internal static readonly IReadOnlyDictionary<int, int> UpperHalves = Letters.Values
        .DistinctBy(letter => letter.Lower)
        .ToDictionary(letter => letter.Lower, letter => letter.Upper);
}
