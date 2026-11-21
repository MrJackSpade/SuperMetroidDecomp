using System.Globalization;

namespace Fixture;

/// <summary>Names the palettes.</summary>
public static class PaletteNames
{
    /// <summary>Each phase with its name.</summary>
    private static readonly PaletteName[] Names = [new(0, "Idle"), new(1, "Cry")];

    /// <summary>The names in order.</summary>
    public static IReadOnlyList<string> InOrder { get; } = [.. Names.Select(entry => entry.Name)];

    /// <summary>A phase's number as text.</summary>
    /// <param name="phase">The phase.</param>
    /// <returns>Its number.</returns>
    public static string Number(int phase) => phase.ToString(CultureInfo.InvariantCulture);
}
