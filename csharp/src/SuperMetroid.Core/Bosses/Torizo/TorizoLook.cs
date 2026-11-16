namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One settled colouring of a Torizo's body and belly (#1512); its fades run from one to another.</summary>
/// <param name="Kind">Which colouring.</param>
/// <param name="Band">Golden Torizo's health band for a health colouring (#2242); null otherwise.</param>
public readonly record struct TorizoLook(TorizoLookKind Kind, GoldenTorizoHealthBand? Band = null)
{
    /// <summary>Golden Torizo's body and belly in <paramref name="band"/>'s colours.</summary>
    public static TorizoLook GoldenHealth(GoldenTorizoHealthBand band) => new(TorizoLookKind.GoldenHealth, Ensure.NotNull(band));

    /// <summary>The colouring's name in its shades' names.</summary>
    public string Key => Band is { } band ? $"golden-health-{band.Key}" : Kind.ToString().ToLowerInvariant();
}
