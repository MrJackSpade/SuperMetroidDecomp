namespace SuperMetroid.Core.Frontend.FileSelect;

/// <summary>How a save slot's field spells its numbers (#1467): two decimal digits each, the time as HH:MM.</summary>
internal static class FileSelectFieldDefinitions
{
    /// <summary>The base the field's digits count in.</summary>
    internal const int DigitBase = 10;
    /// <summary>The largest hours or minutes the field's two digits show; a longer game time shows this.</summary>
    internal const int MaximumTimePart = 99;
    /// <summary>The glyphs the time's colon is shown right of its first hour digit: after the two hour digits.</summary>
    internal const int ColonColumn = 2;
    /// <summary>Pixels across one digit or the colon: each of a field's glyphs is shown this far right of the one before.</summary>
    internal const int GlyphWidthPixels = 8;
}
