namespace Fixture;

/// <summary>One colour program.</summary>
/// <param name="Key">The key its colours are stored under.</param>
/// <param name="Runs">The runs it gives colours to.</param>
public sealed record PaletteProgram(string Key, IReadOnlyList<PaletteRun> Runs);
