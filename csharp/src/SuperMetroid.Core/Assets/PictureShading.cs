using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a draw's picture is coloured (#1512, #1633): its own colours, with the shades its owner names laid over them in
/// order, then each colour effect applied in order, then the shades laid over the effects, whose colours are written over
/// what the effects show. A shade names the pixels it recolours by their colour key, the colour they hold with their
/// recolour region. The simulation names shades and effects; it never selects a colour for a pixel. A backend colours the
/// picture from <see cref="KeyColours(SpriteImage)"/> alone.
/// </summary>
public sealed record PictureShading
{
    /// <summary>Keeps a draw's shades and effects.</summary>
    /// <param name="shades">The installed shades laid over the standard colours, in order; a later one wins where they overlap.</param>
    /// <param name="effects">The colour effects applied after the shades, in order.</param>
    public PictureShading(ImmutableArray<PictureShade> shades, ImmutableArray<ColourEffect> effects)
        : this(shades, effects, [])
    {
    }

    /// <summary>Keeps a draw's shades, effects and the shades laid over its effects.</summary>
    /// <param name="shades">The installed shades laid over the standard colours, in order; a later one wins where they overlap.</param>
    /// <param name="effects">The colour effects applied after the shades, in order.</param>
    /// <param name="overEffects">The installed shades laid over what the effects show, in order.</param>
    public PictureShading(ImmutableArray<PictureShade> shades, ImmutableArray<ColourEffect> effects, ImmutableArray<PictureShade> overEffects)
    {
        Shades = shades.IsDefault ? [] : shades;
        Effects = effects.IsDefault ? [] : effects;
        OverEffects = overEffects.IsDefault ? [] : overEffects;
    }

    /// <summary>The picture's standard colours, with no shade or effect.</summary>
    public static PictureShading Standard { get; } = new([], []);

    /// <summary>The picture under one shade.</summary>
    public static PictureShading Shaded(PictureShade shade) => new([shade], []);

    /// <summary>The installed shades laid over the standard colours, in order.</summary>
    public ImmutableArray<PictureShade> Shades { get; }

    /// <summary>The colour effects applied after the shades, in order.</summary>
    public ImmutableArray<ColourEffect> Effects { get; }

    /// <summary>The installed shades laid over what the effects show, in order.</summary>
    public ImmutableArray<PictureShade> OverEffects { get; }

    /// <summary>Whether both name the same shades and effects in the same order, so show a picture alike.</summary>
    public bool Equals(PictureShading? other) =>
        other is not null && Shades.SequenceEqual(other.Shades) && Effects.SequenceEqual(other.Effects)
        && OverEffects.SequenceEqual(other.OverEffects);

    /// <summary>Hashes the shades and effects <see cref="Equals(PictureShading)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PictureShade shade in Shades)
            hash.Add(shade);
        foreach (ColourEffect effect in Effects)
            hash.Add(effect);
        foreach (PictureShade shade in OverEffects)
            hash.Add(shade);
        return hash.ToHashCode();
    }

    /// <summary>Whether the picture shows its standard colours.</summary>
    public bool IsStandard => Shades.IsEmpty && Effects.IsEmpty && OverEffects.IsEmpty;

    /// <summary>This shading with <paramref name="shade"/> laid over its shades.</summary>
    public PictureShading With(PictureShade shade) => new(Shades.Add(shade), Effects, OverEffects);

    /// <summary>This shading with <paramref name="effect"/> applied after its effects.</summary>
    public PictureShading With(ColourEffect effect) => new(Shades, Effects.Add(Ensure.NotNull(effect)), OverEffects);

    /// <summary>This shading with <paramref name="shade"/> laid over what its effects show.</summary>
    public PictureShading OverEffect(PictureShade shade) => new(Shades, Effects, OverEffects.Add(shade));

    /// <summary>The colour each of <paramref name="picture"/>'s colour keys (<see cref="SpriteImage.ColourKeys"/>) shows under this shading.</summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    public RgbColor[] KeyColours(SpriteImage picture)
    {
        Ensure.NotNull(picture);
        var shown = new RgbColor[picture.ColourKeys.Length];
        KeyColours(picture, shown);
        return shown;
    }

    /// <summary>
    /// Writes the colour each of <paramref name="picture"/>'s colour keys shows in this shading into <paramref name="shown"/>,
    /// one per key in <see cref="SpriteImage.ColourKeys"/>' order, without allocating, for a backend that packs every
    /// picture's colours each frame. A shade's colour for a key the picture holds no pixel of shows nowhere.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="shown"/> does not hold one colour per key.</exception>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    public void KeyColours(SpriteImage picture, Span<RgbColor> shown)
    {
        Ensure.NotNull(picture);
        ImmutableArray<ColourKey> keys = picture.ColourKeys;
        _ = Ensure.LengthEqual(shown, keys.Length);
        for (int key = 0; key < keys.Length; key++)
            shown[key] = keys[key].Colour;
        foreach (PictureShade shade in Shades)
            Recolour(picture, picture.Shade(shade), shown);
        foreach (ColourEffect effect in Effects)
        {
            for (int key = 0; key < shown.Length; key++)
                shown[key] = effect.Apply(shown[key]);
        }
        foreach (PictureShade shade in OverEffects)
            Recolour(picture, picture.Shade(shade), shown);
    }

    /// <summary>The colour <paramref name="key"/> of <paramref name="picture"/> shows under this shading, such as its backdrop's.</summary>
    /// <exception cref="ArgumentException">The picture holds no such key.</exception>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    public RgbColor Shown(SpriteImage picture, ColourKey key)
    {
        Ensure.NotNull(picture);
        int place = picture.PlaceOf(key) ?? throw new ArgumentException($"Picture {picture.Name} holds no colour key {key}.", nameof(key));
        return KeyColours(picture)[place];
    }

    /// <summary>Writes each colour <paramref name="recolouring"/> shows over the keys of <paramref name="picture"/> it names.</summary>
    private static void Recolour(SpriteImage picture, PictureRecolouring recolouring, Span<RgbColor> shown)
    {
        foreach (KeyedSubstitution substitution in recolouring.Substitutions)
        {
            if (picture.PlaceOf(substitution.Replaced) is { } place)
                shown[place] = substitution.Shown;
        }
    }
}
