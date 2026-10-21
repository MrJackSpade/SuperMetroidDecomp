namespace SuperMetroid.Verification.Frontend.Ending;

/// <summary>The glyph and address of one large record.</summary>
/// <param name="Glyph">The glyph.</param>
/// <param name="Address">The address.</param>
internal readonly record struct LargeRecord(char Glyph, int Address);
