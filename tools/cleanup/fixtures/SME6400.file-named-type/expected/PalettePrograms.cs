namespace Fixture;

/// <summary>Every colour program.</summary>
public static class PalettePrograms
{
    /// <summary>The programs.</summary>
    public static IReadOnlyList<PaletteProgram> All { get; } =
    [
        new("heat", [new("Samus in heat")]),
    ];
}
