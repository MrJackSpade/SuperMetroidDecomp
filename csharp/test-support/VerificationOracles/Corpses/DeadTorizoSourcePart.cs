
namespace SuperMetroid.TestSupport.VerificationOracles;

/// <summary>One square part of the dead Torizo's source sheet: its top-left tile and its size in tiles.</summary>
/// <param name="Column">The column, in tiles, of the part's top-left tile in the source sheet.</param>
/// <param name="Row">The row, in tiles, of the part's top-left tile in the source sheet.</param>
/// <param name="Extent">The part's width and height, in tiles.</param>
internal readonly record struct DeadTorizoSourcePart(int Column, int Row, int Extent);
