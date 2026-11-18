using System.Collections.Frozen;

namespace SuperMetroid.Core.Bosses.Torizo;

/// <summary>One settled colouring of a Torizo's body and belly (#1512); its fades run from one to another.</summary>
/// <param name="Kind">Which colouring.</param>
/// <param name="Band">Golden Torizo's health band for a health colouring (#2242); null otherwise.</param>
public readonly record struct TorizoLook(TorizoLookKind Kind, GoldenTorizoHealthBand? Band = null)
{
    /// <summary>
    /// Each colouring's name in its installed shades' names (#2409), written out so renaming a colouring's identity never
    /// changes a name on disk. A health colouring is named by its band instead.
    /// </summary>
    private static readonly FrozenDictionary<TorizoLookKind, string> Keys =
        new Dictionary<TorizoLookKind, string>
        {
            [TorizoLookKind.BombStatue] = "bombstatue",
            [TorizoLookKind.GoldenInitial] = "goldeninitial",
            [TorizoLookKind.Normal] = "normal",
            [TorizoLookKind.Golden] = "golden",
            [TorizoLookKind.White] = "white",
            [TorizoLookKind.Black] = "black",
        }.ToFrozenDictionary();

    /// <summary>Golden Torizo's body and belly in <paramref name="band"/>'s colours.</summary>
    public static TorizoLook GoldenHealth(GoldenTorizoHealthBand band) => new(TorizoLookKind.GoldenHealth, Ensure.NotNull(band));

    /// <summary>The colouring's name in its shades' names.</summary>
    /// <exception cref="ArgumentOutOfRangeException">The colouring is a health colouring that names no band.</exception>
    public string Key => Band is { } band ? $"golden-health-{band.Key}" : Ensure.Listed(Keys, Kind);
}
