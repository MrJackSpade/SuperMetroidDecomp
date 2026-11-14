using System.Collections.Frozen;
using SuperMetroid.Core.Assets;
using SuperMetroid.Core.Fx.ColourEffects;

namespace SuperMetroid.Core.Frontend.Ending.Artwork;

/// <summary>
/// Which ending actors and scenery a shade the ending's colours show is authored for (#1544), as
/// <see cref="EndingColourOwnerDefinitions"/> lists them: the screen names only an actor's or a background picture's or
/// floor's own shades over its pictures, and the import pre-colours each in those alone.
/// </summary>
internal static class EndingColourOwners
{
    /// <summary>What each shade family is authored for, by the family's name.</summary>
    private static readonly FrozenDictionary<string, ShadeOwners> ByEffect = Listed();

    /// <summary>The colouring each actor colouring's drawn shade names.</summary>
    private static readonly FrozenDictionary<PictureShade, EndingSpriteColouring> Colourings =
        Enum.GetValues<EndingSpriteColouring>().ToFrozenDictionary(EndingShades.Drawn);

    /// <summary>Whether the pictures of the actors drawn in <paramref name="colouring"/> show <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The shade is of no colour effect the ending shows.</exception>
    internal static bool Shows(EndingSpriteColouring colouring, PictureShade shade) => OwnersOf(shade).Actors.Contains(colouring);

    /// <summary>Whether <paramref name="scenery"/> shows <paramref name="shade"/>.</summary>
    /// <exception cref="InvalidDataException">The shade is of no colour effect the ending shows.</exception>
    internal static bool Shows(EndingScenery scenery, PictureShade shade) => OwnersOf(shade).Scenery.Contains(scenery);

    /// <summary>The colouring whose drawn shade is <paramref name="drawn"/>.</summary>
    /// <exception cref="InvalidDataException">The shade is no ending actor colouring's drawn shade.</exception>
    internal static EndingSpriteColouring ColouringOf(PictureShade drawn) =>
        Colourings.TryGetValue(drawn, out EndingSpriteColouring colouring)
            ? colouring
            : throw new InvalidDataException($"Shade {drawn.Key} is no ending actor colouring's drawn shade.");

    /// <summary>What the family of <paramref name="shade"/> is authored for.</summary>
    /// <exception cref="InvalidDataException">The shade is of no colour effect the ending shows.</exception>
    private static ShadeOwners OwnersOf(PictureShade shade) => ByEffect.TryGetValue(shade.Effect, out ShadeOwners? owners)
        ? owners
        : throw new InvalidDataException($"Shade {shade.Key} is of no colour effect the ending shows.");

    /// <summary>Every shade family the ending shows, with what it is authored for.</summary>
    /// <exception cref="InvalidDataException">A load, fade or flash the ending makes lists no owners.</exception>
    private static FrozenDictionary<string, ShadeOwners> Listed()
    {
        var listed = new Dictionary<string, ShadeOwners>(StringComparer.Ordinal);
        foreach (EndingColourLoad load in Enum.GetValues<EndingColourLoad>())
            Add(EndingShades.Loaded(load), Required(EndingColourOwnerDefinitions.Loads, load));
        foreach (EndingFade fade in Enum.GetValues<EndingFade>())
            Add(EndingShades.Faded(fade, 0), Required(EndingColourOwnerDefinitions.Fades, fade));
        foreach (EndingFlash flash in Enum.GetValues<EndingFlash>())
            Add(EndingShades.Flashed(flash), Required(EndingColourOwnerDefinitions.Flashes, flash));
        Add(EndingShades.LogoCrossfade(0), EndingColourOwnerDefinitions.LogoCrossfade);
        // A colour group's shades form one family per animation that shows in it (#1598).
        ShownColourEffectFrame[] shownFrames = ColourEffectShownFrames.ShownBy(Enum.GetValues<ColourEffectId>());
        foreach (KeyValuePair<ColourEffectColours, EndingEffectOwners> group in EndingColourOwnerDefinitions.ColourEffects)
        {
            ColourEffectAnimation[] animations = [.. shownFrames.Where(shown => shown.Colours == group.Key)
                .Select(shown => shown.Frame.Animation).Distinct()];
            if (animations.Length == 0)
                throw new InvalidDataException($"The ending lists what {group.Key} is authored for, which no colour effect shows.");
            foreach (ColourEffectAnimation animation in animations)
                Add(ColourEffectShades.Of(group.Key, new ColourEffectFrame(animation, 0)), group.Value);
        }
        return listed.ToFrozenDictionary(StringComparer.Ordinal);

        // Lists what the family of `shade` is authored for.
        void Add(PictureShade shade, EndingEffectOwners owners) =>
            listed.Add(shade.Effect, new ShadeOwners(owners.Actors.ToFrozenSet(), owners.Scenery.ToFrozenSet()));
    }

    /// <summary>What <paramref name="listing"/> lists for <paramref name="effect"/>.</summary>
    /// <exception cref="InvalidDataException">The listing lists no owners for it.</exception>
    private static EndingEffectOwners Required<TEffect>(FrozenDictionary<TEffect, EndingEffectOwners> listing, TEffect effect)
        where TEffect : struct, Enum =>
        listing.TryGetValue(effect, out EndingEffectOwners? owners)
            ? owners
            : throw new InvalidDataException($"The ending's colour effect {effect} lists nothing it is authored for.");

    /// <summary>The actors and scenery one shade family is authored for, as sets.</summary>
    /// <param name="Actors">The colourings of the actors that show it.</param>
    /// <param name="Scenery">The background pictures and floors that show it.</param>
    private sealed record ShadeOwners(FrozenSet<EndingSpriteColouring> Actors, FrozenSet<EndingScenery> Scenery);
}
