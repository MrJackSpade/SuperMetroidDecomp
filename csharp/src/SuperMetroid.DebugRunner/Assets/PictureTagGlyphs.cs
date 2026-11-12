namespace SuperMetroid.DebugRunner.Assets;

/// <summary>
/// The bitmap font picture previews draw tags in (#1639): each character a tag uses, as five rows of three pixels, a
/// <c>1</c> marking a lit pixel.
/// </summary>
internal static class PictureTagGlyphs
{
    /// <summary>The width of every glyph, in font pixels.</summary>
    internal const int Width = 3;

    /// <summary>The height of every glyph, in font pixels.</summary>
    internal const int Height = 5;

    /// <summary>Each character's rows, top to bottom.</summary>
    internal static readonly IReadOnlyDictionary<char, string[]> Rows = new Dictionary<char, string[]>
    {
        ['P'] = ["111", "101", "111", "100", "100"],
        ['-'] = ["000", "000", "111", "000", "000"],
        ['0'] = ["111", "101", "101", "101", "111"],
        ['1'] = ["010", "110", "010", "010", "111"],
        ['2'] = ["111", "001", "111", "100", "111"],
        ['3'] = ["111", "001", "111", "001", "111"],
        ['4'] = ["101", "101", "111", "001", "001"],
        ['5'] = ["111", "100", "111", "001", "111"],
        ['6'] = ["111", "100", "111", "101", "111"],
        ['7'] = ["111", "001", "001", "001", "001"],
        ['8'] = ["111", "101", "111", "101", "111"],
        ['9'] = ["111", "101", "111", "001", "111"],
    };
}
