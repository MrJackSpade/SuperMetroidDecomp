namespace SuperMetroid.Verification.Areas.Tourian;

/// <summary>The cursor, code and width of one lookup tourian artwork instruction.</summary>
/// <param name="Cursor">The cursor.</param>
/// <param name="Code">The code.</param>
/// <param name="Width">The width.</param>
internal readonly record struct LookupTourianArtworkInstruction(ushort Cursor, ushort Code, int Width);
