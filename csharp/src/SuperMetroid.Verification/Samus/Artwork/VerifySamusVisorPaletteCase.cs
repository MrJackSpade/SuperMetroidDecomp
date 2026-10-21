namespace SuperMetroid.Verification.Samus.Artwork;

/// <summary>The timer and palette byte offset of one verify samus visor palette case.</summary>
/// <param name="Timer">The timer.</param>
/// <param name="PaletteByteOffset">The palette byte offset.</param>
internal readonly record struct VerifySamusVisorPaletteCase(int Timer, int PaletteByteOffset);
