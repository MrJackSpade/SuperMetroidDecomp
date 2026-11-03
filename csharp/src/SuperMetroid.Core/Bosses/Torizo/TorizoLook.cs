namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One settled colouring of a Torizo's body and belly (#1512); its fades run from one to another.</summary>
/// <param name="Kind">Which colouring.</param>
/// <param name="Band">Golden Torizo's health band, zero through seven, for a health colouring; zero otherwise.</param>
public readonly record struct TorizoLook(TorizoLookKind Kind, int Band = 0)
{
    /// <summary>The colouring's name in its shades' names.</summary>
    public string Key => Kind == TorizoLookKind.GoldenHealth ? $"golden-health-{Band}" : Kind.ToString().ToLowerInvariant();
}
