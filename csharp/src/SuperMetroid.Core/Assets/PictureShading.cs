using System.Collections.Frozen;
using System.Collections.Immutable;
using System.Runtime.CompilerServices;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// How a draw's picture is coloured (#1512, #1633): its own colours, with the shades its owner names laid over them in
/// order, then the fades between shadings it shows (#2215), then each colour effect applied in order, then the shades laid over the effects, whose colours are written over
/// what the effects show. A shade names the pixels it recolours by their colour key, the colour they hold with their
/// recolour region. The simulation names shades and effects; it never selects a colour for a pixel or blends one. A backend
/// colours the picture from <see cref="KeyColours(SpriteImage, IColourBlend)"/> alone, blending its fades as it blends them
/// (#2563). Each colour key's colour under a shading is worked out once per picture and blend, keyed by the colour key
/// (#2565).
/// </summary>
public sealed record PictureShading
{
    /// <summary>
    /// Each colour key's colour under this shading, by blend and picture, worked out once for each picture it colours with
    /// each blend: derived from the shading, the picture and the backend's blend, not game state, and made again when absent.
    /// </summary>
    [NonSerialized]
    private ConditionalWeakTable<IColourBlend, ConditionalWeakTable<SpriteImage, FrozenDictionary<ColourKey, RgbColor>>>? coloured;

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
        : this(shades, [], effects, overEffects)
    {
    }

    /// <summary>Keeps a draw's shades, the fades shown over them, its effects and the shades laid over its effects.</summary>
    /// <param name="shades">The installed shades laid over the standard colours, in order; a later one wins where they overlap.</param>
    /// <param name="fades">The fades shown over the shades, in order (#2215).</param>
    /// <param name="effects">The colour effects applied after the fades, in order.</param>
    /// <param name="overEffects">The installed shades laid over what the effects show, in order.</param>
    public PictureShading(ImmutableArray<PictureShade> shades, ImmutableArray<PictureShadeFade> fades, ImmutableArray<ColourEffect> effects,
        ImmutableArray<PictureShade> overEffects)
    {
        Shades = shades.IsDefault ? [] : shades;
        Fades = fades.IsDefault ? [] : fades;
        Effects = effects.IsDefault ? [] : effects;
        OverEffects = overEffects.IsDefault ? [] : overEffects;
    }

    /// <summary>The picture's standard colours, with no shade or effect.</summary>
    public static PictureShading Standard { get; } = new([], []);

    /// <summary>The picture under one shade.</summary>
    public static PictureShading Shaded(PictureShade shade) => new([shade], []);

    /// <summary>The installed shades laid over the standard colours, in order.</summary>
    public ImmutableArray<PictureShade> Shades { get; }

    /// <summary>The fades shown over the shades, in order (#2215).</summary>
    private ImmutableArray<PictureShadeFade> Fades { get; }

    /// <summary>The colour effects applied after the fades, in order.</summary>
    public ImmutableArray<ColourEffect> Effects { get; }

    /// <summary>The installed shades laid over what the effects show, in order.</summary>
    private ImmutableArray<PictureShade> OverEffects { get; }

    /// <summary>Whether both name the same shades and effects in the same order, so show a picture alike.</summary>
    public bool Equals(PictureShading? other) =>
        other is not null && Shades.SequenceEqual(other.Shades) && Fades.SequenceEqual(other.Fades) && Effects.SequenceEqual(other.Effects)
        && OverEffects.SequenceEqual(other.OverEffects);

    /// <summary>Hashes the shades and effects <see cref="Equals(PictureShading)"/> compares.</summary>
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PictureShade shade in Shades)
            hash.Add(shade);
        foreach (PictureShadeFade fade in Fades)
            hash.Add(fade);
        foreach (ColourEffect effect in Effects)
            hash.Add(effect);
        foreach (PictureShade shade in OverEffects)
            hash.Add(shade);
        return hash.ToHashCode();
    }

    /// <summary>Whether the picture shows its standard colours.</summary>
    public bool IsStandard => Shades.IsEmpty && Fades.IsEmpty && Effects.IsEmpty && OverEffects.IsEmpty;

    /// <summary>This shading with <paramref name="shade"/> laid over its shades.</summary>
    public PictureShading With(PictureShade shade) => new(Shades.Add(shade), Fades, Effects, OverEffects);

    /// <summary>This shading with <paramref name="fade"/> shown after its fades (#2215).</summary>
    public PictureShading With(PictureShadeFade fade) => new(Shades, Fades.Add(Ensure.NotNull(fade)), Effects, OverEffects);

    /// <summary>This shading with <paramref name="effect"/> applied after its effects.</summary>
    public PictureShading With(ColourEffect effect) => new(Shades, Fades, Effects.Add(Ensure.NotNull(effect)), OverEffects);

    /// <summary>This shading with <paramref name="shade"/> laid over what its effects show.</summary>
    public PictureShading OverEffect(PictureShade shade) => new(Shades, Fades, Effects, OverEffects.Add(shade));

    /// <summary>The colour each of <paramref name="picture"/>'s colour keys (<see cref="SpriteImage.ColourKeys"/>) shows under this shading.</summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    /// <param name="picture">The picture coloured.</param>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    public RgbColor[] KeyColours(SpriteImage picture, IColourBlend blend)
    {
        FrozenDictionary<ColourKey, RgbColor> colours = ColoursOf(picture, blend);
        return [.. picture.ColourKeys.Select(key => colours[key])];
    }

    /// <summary>
    /// Each of <paramref name="picture"/>'s colour keys' colour under this shading, by key, worked out once per picture and
    /// blend.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    /// <param name="picture">The picture coloured.</param>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    public FrozenDictionary<ColourKey, RgbColor> ColoursOf(SpriteImage picture, IColourBlend blend)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(blend);
        return (coloured ??= new()).GetValue(blend, _ => new()).GetValue(picture, shaded => Colour(shaded, blend));
    }

    /// <summary>Works out each of <paramref name="picture"/>'s colour keys' colour under this shading, from the key's own.</summary>
    private FrozenDictionary<ColourKey, RgbColor> Colour(SpriteImage picture, IColourBlend blend)
    {
        Dictionary<ColourKey, RgbColor> shown = picture.ColourKeys.ToDictionary(key => key, key => key.Colour);
        ApplyOver(picture, shown, blend);
        return shown.ToFrozenDictionary();
    }

    /// <summary>
    /// Writes the colour each of <paramref name="picture"/>'s colour keys shows in this shading into <paramref name="shown"/>,
    /// one per key in <see cref="SpriteImage.ColourKeys"/>' order, without allocating, for a backend that packs every
    /// picture's colours each frame. A shade's colour for a key the picture holds no pixel of shows nowhere.
    /// </summary>
    /// <exception cref="ArgumentException"><paramref name="shown"/> does not hold one colour per key.</exception>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    /// <param name="picture">The picture coloured.</param>
    /// <param name="shown">One colour per key, written.</param>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    public void KeyColours(SpriteImage picture, Span<RgbColor> shown, IColourBlend blend)
    {
        FrozenDictionary<ColourKey, RgbColor> colours = ColoursOf(picture, blend);
        ImmutableArray<ColourKey> keys = picture.ColourKeys;
        _ = Ensure.LengthEqual(shown, keys.Length);
        // The colours are listed in the order of the picture's keys; each is written at its key's own place.
        int place = 0;
        foreach (ColourKey key in keys)
            shown[place++] = colours[key];
    }

    /// <summary>
    /// Lays this shading over <paramref name="shown"/>, the colour each of <paramref name="picture"/>'s colour keys shows so
    /// far, by key: its shades, then its fades, its effects and the shades over its effects.
    /// </summary>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    internal void ApplyOver(SpriteImage picture, Dictionary<ColourKey, RgbColor> shown, IColourBlend blend)
    {
        Ensure.NotNull(blend);
        foreach (PictureShade shade in Shades)
            Recolour(picture.Shade(shade), shown);
        foreach (PictureShadeFade fade in Fades)
            fade.ApplyOver(picture, shown, blend);
        foreach (ColourEffect effect in Effects)
        {
            foreach (ColourKey key in picture.ColourKeys)
                shown[key] = effect.Apply(shown[key], blend);
        }
        foreach (PictureShade shade in OverEffects)
            Recolour(picture.Shade(shade), shown);
    }

    /// <summary>The colour <paramref name="key"/> of <paramref name="picture"/> shows under this shading, such as its backdrop's.</summary>
    /// <exception cref="ArgumentException">The picture holds no such key.</exception>
    /// <exception cref="InvalidDataException">The installation did not pre-colour the picture in a shade this shading names.</exception>
    /// <param name="picture">The picture coloured.</param>
    /// <param name="key">The key whose colour is shown.</param>
    /// <param name="blend">The backend's blending of a fade's colours.</param>
    public RgbColor Shown(SpriteImage picture, ColourKey key, IColourBlend blend) =>
        ColoursOf(picture, blend).TryGetValue(key, out RgbColor shown)
            ? shown
            : throw new ArgumentException($"Picture {picture.Name} holds no colour key {key}.", nameof(key));

    /// <summary>Writes each colour <paramref name="recolouring"/> shows over the keys it names that the picture holds.</summary>
    private static void Recolour(PictureRecolouring recolouring, Dictionary<ColourKey, RgbColor> shown)
    {
        foreach (KeyedSubstitution substitution in recolouring.Substitutions)
        {
            if (shown.ContainsKey(substitution.Replaced))
                shown[substitution.Replaced] = substitution.Shown;
        }
    }
}
