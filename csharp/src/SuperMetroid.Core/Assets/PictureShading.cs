using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a draw's picture is coloured (#1512): its standard colours, with the shades its owner names laid over them in
/// order, then each colour effect applied in order. The simulation names shades and effects; it never selects a colour
/// for an ink. A backend colours the picture from <see cref="InkColours"/> alone.
/// </summary>
public sealed record PictureShading
{
    /// <summary>Keeps a draw's shades and effects.</summary>
    /// <param name="shades">The installed shades laid over the standard colours, in order; a later one wins where they overlap.</param>
    /// <param name="effects">The colour effects applied after the shades, in order.</param>
    public PictureShading(ImmutableArray<PictureShade> shades, ImmutableArray<ColourEffect> effects)
    {
        Shades = shades.IsDefault ? [] : shades;
        Effects = effects.IsDefault ? [] : effects;
    }

    /// <summary>The picture's standard colours, with no shade or effect.</summary>
    public static PictureShading Standard { get; } = new([], []);

    /// <summary>The picture under one shade.</summary>
    public static PictureShading Shaded(PictureShade shade) => new([shade], []);

    /// <summary>The installed shades laid over the standard colours, in order.</summary>
    public ImmutableArray<PictureShade> Shades { get; }

    /// <summary>The colour effects applied after the shades, in order.</summary>
    public ImmutableArray<ColourEffect> Effects { get; }

    /// <summary>Whether both name the same shades and effects in the same order, so show a picture alike.</summary>
    public bool Equals(PictureShading? other) =>
        other is not null && Shades.SequenceEqual(other.Shades) && Effects.SequenceEqual(other.Effects);

    /// <summary>Hashes the shades and effects <see cref="Equals(PictureShading)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PictureShade shade in Shades)
            hash.Add(shade);
        foreach (ColourEffect effect in Effects)
            hash.Add(effect);
        return hash.ToHashCode();
    }

    /// <summary>Whether the picture shows its standard colours.</summary>
    public bool IsStandard => Shades.IsEmpty && Effects.IsEmpty;

    /// <summary>This shading with <paramref name="shade"/> laid over its shades.</summary>
    public PictureShading With(PictureShade shade) => new(Shades.Add(shade), Effects);

    /// <summary>This shading with <paramref name="effect"/> applied after its effects.</summary>
    public PictureShading With(ColourEffect effect) => new(Shades, Effects.Add(Ensure.NotNull(effect)));

    /// <summary>The colour each of <paramref name="picture"/>'s inks shows under this shading.</summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    public RgbColor[] InkColours(SpriteImage picture)
    {
        Ensure.NotNull(picture);
        RgbColor[] inks = picture.StandardColours.ToArray();
        foreach (PictureShade shade in Shades)
            picture.Shade(shade).Over(inks);
        foreach (ColourEffect effect in Effects)
        {
            for (int ink = 0; ink < inks.Length; ink++)
                inks[ink] = effect.Apply(inks[ink]);
        }
        return inks;
    }
}
