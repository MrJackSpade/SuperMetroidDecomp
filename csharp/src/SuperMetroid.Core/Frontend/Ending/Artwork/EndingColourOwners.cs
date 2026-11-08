using System.Collections.Frozen;
using System.Collections.Immutable;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.PaletteFx;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Which ending actors a shade the ending's colours show is authored for (#1544), as
/// <see cref="EndingColourOwnerDefinitions"/> lists them: the screen names only an actor's own shades over its pictures,
/// and the import pre-colours each actor's pictures in those alone.
/// </summary>
internal static class EndingColourOwners
{
    /// <summary>The actors each shade family is authored for, by the family's name.</summary>
    private static readonly FrozenDictionary<string, FrozenSet<EndingSpriteColouring>> ByEffect = Listed();

    /// <summary>The colouring each actor colouring's drawn shade names.</summary>
    private static readonly FrozenDictionary<PictureShade, EndingSpriteColouring> Colourings =
        Enum.GetValues<EndingSpriteColouring>().ToFrozenDictionary(EndingShades.Drawn);

    /// <summary>Whether the pictures of the actors drawn in <paramref name="colouring"/> show <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The shade is of no colour effect the ending shows.</exception>
    internal static bool Shows(EndingSpriteColouring colouring, PictureShade shade) =>
        ByEffect.TryGetValue(shade.Effect, out FrozenSet<EndingSpriteColouring>? owners)
            ? owners.Contains(colouring)
            : throw new InvalidDataException($"Shade {shade.Key} is of no colour effect the ending shows.");

    /// <summary>The colouring whose drawn shade is <paramref name="drawn"/>.</summary>
    /// <exception cref="InvalidDataException">The shade is no ending actor colouring's drawn shade.</exception>
    internal static EndingSpriteColouring ColouringOf(PictureShade drawn) =>
        Colourings.TryGetValue(drawn, out EndingSpriteColouring colouring)
            ? colouring
            : throw new InvalidDataException($"Shade {drawn.Key} is no ending actor colouring's drawn shade.");

    /// <summary>Every shade family the ending shows, with the actors it is authored for.</summary>
    /// <exception cref="InvalidDataException">A load, fade or flash the ending makes lists no actors.</exception>
    private static FrozenDictionary<string, FrozenSet<EndingSpriteColouring>> Listed()
    {
        var listed = new Dictionary<string, FrozenSet<EndingSpriteColouring>>(StringComparer.Ordinal);
        foreach (EndingColourLoad load in Enum.GetValues<EndingColourLoad>())
            Add(EndingShades.Loaded(load), Required(EndingColourOwnerDefinitions.Loads, load));
        foreach (EndingFade fade in Enum.GetValues<EndingFade>())
            Add(EndingShades.Faded(fade, 0), Required(EndingColourOwnerDefinitions.Fades, fade));
        foreach (EndingFlash flash in Enum.GetValues<EndingFlash>())
            Add(EndingShades.Flashed(flash), Required(EndingColourOwnerDefinitions.Flashes, flash));
        Add(EndingShades.LogoCrossfade(0), EndingColourOwnerDefinitions.LogoCrossfade);
        foreach (KeyValuePair<PaletteFxColours, ImmutableArray<EndingSpriteColouring>> group in EndingColourOwnerDefinitions.PaletteFx)
            Add(PaletteFxShades.Of(group.Key, new PaletteFxFrame(0)), group.Value);
        return listed.ToFrozenDictionary(StringComparer.Ordinal);

        // Lists the actors `owners` the family of `shade` is authored for.
        void Add(PictureShade shade, ImmutableArray<EndingSpriteColouring> owners) => listed.Add(shade.Effect, owners.ToFrozenSet());
    }

    /// <summary>The actors <paramref name="listing"/> lists for <paramref name="effect"/>.</summary>
    /// <exception cref="InvalidDataException">The listing lists no actors for it.</exception>
    private static ImmutableArray<EndingSpriteColouring> Required<TEffect>(FrozenDictionary<TEffect, ImmutableArray<EndingSpriteColouring>> listing,
        TEffect effect)
        where TEffect : struct, Enum =>
        listing.TryGetValue(effect, out ImmutableArray<EndingSpriteColouring> owners)
            ? owners
            : throw new InvalidDataException($"The ending's colour effect {effect} lists no actors it is authored for.");
}
