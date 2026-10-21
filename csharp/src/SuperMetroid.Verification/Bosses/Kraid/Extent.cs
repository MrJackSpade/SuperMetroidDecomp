using SuperMetroid.Core.Bosses.Kraid;

namespace SuperMetroid.Verification.Bosses.Kraid;

/// <summary>The source, start and end of one extent.</summary>
/// <param name="Source">The source.</param>
/// <param name="Start">The start.</param>
/// <param name="End">The end.</param>
internal readonly record struct Extent(KraidPaletteSource Source, int Start, int End);
