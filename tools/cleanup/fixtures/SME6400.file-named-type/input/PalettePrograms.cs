namespace Fixture;

/// <summary>One colour program.</summary>
/// <param name="Key">The key its colours are stored under.</param>
/// <param name="Runs">The runs it gives colours to.</param>
public sealed record PaletteProgram(string Key, IReadOnlyList<PaletteRun> Runs);

/// <summary>One run of a program.</summary>
/// <param name="Owner">How a failure names the run.</param>
public sealed record PaletteRun(string Owner);

/// <summary>Every colour program.</summary>
public static class PalettePrograms
{
    /// <summary>The programs.</summary>
    public static IReadOnlyList<PaletteProgram> All { get; } =
    [
        new("heat", [new("Samus in heat")]),
    ];
}
