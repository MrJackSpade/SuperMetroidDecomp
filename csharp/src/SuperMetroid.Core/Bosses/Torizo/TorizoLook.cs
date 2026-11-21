namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One settled colouring of a Torizo's body and belly (#1512); its fades run from one to another.</summary>
/// <param name="Kind">Which colouring.</param>
/// <param name="Band">Golden Torizo's health band for a health colouring (#2242); null otherwise.</param>
public readonly partial record struct TorizoLook(TorizoLookKind Kind, GoldenTorizoHealthBand? Band = null)
{
    /// <summary>Golden Torizo's body and belly in <paramref name="band"/>'s colours.</summary>
    public static TorizoLook GoldenHealth(GoldenTorizoHealthBand band) => new(TorizoLookKind.GoldenHealth, Ensure.NotNull(band));

    /// <summary>
    /// The colouring's name in its shades' names (#2409): its kind's key (<see cref="TorizoLookKindKeys"/>), or for a health
    /// colouring the health colouring's key with its band's after it.
    /// </summary>
    /// <exception cref="InvalidOperationException">The colouring is a health colouring that names no band.</exception>
    [InstalledKey]
    public string Key => Band is { } band ? $"{TorizoLookKindKeys.Installed.Of(TorizoLookKind.GoldenHealth)}-{band.Key}"
        : Kind == TorizoLookKind.GoldenHealth ? throw new InvalidOperationException("A Torizo health colouring names no band.")
        : TorizoLookKindKeys.Installed.Of(Kind);
}
