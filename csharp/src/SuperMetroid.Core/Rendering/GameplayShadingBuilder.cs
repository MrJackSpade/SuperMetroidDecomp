using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gathers a gameplay frame's shading as it is published (#1512): each owner of a colour effect lays the shades its
/// typed state names and the effects it applies over the groups of pictures it colours, in the order the publication
/// asks them. Later shades win where they recolour the same inks, as the later of two colour writes did.
/// </summary>
public sealed class GameplayShadingBuilder
{
    /// <summary>Each group's shading so far.</summary>
    private readonly Dictionary<GameplayPictures, PictureShading> shadings =
        Enum.GetValues<GameplayPictures>().ToDictionary(pictures => pictures, _ => PictureShading.Standard);

    /// <summary>The effects every shaded sprite draw shows so far.</summary>
    private readonly List<ColourEffect> sprites = [];

    /// <summary>Lays <paramref name="shade"/> over <paramref name="pictures"/>' shades.</summary>
    public void Shade(GameplayPictures pictures, PictureShade shade) => shadings[Defined(pictures)] = shadings[pictures].With(shade);

    /// <summary>Applies <paramref name="effect"/> to <paramref name="pictures"/> after their effects.</summary>
    public void Affect(GameplayPictures pictures, ColourEffect effect) =>
        shadings[Defined(pictures)] = shadings[pictures].With(Ensure.NotNull(effect));

    /// <summary>Lays <paramref name="shading"/>'s shades over <paramref name="pictures"/>' and applies its effects after theirs.</summary>
    public void Show(GameplayPictures pictures, PictureShading shading)
    {
        Ensure.NotNull(shading);
        foreach (PictureShade shade in shading.Shades)
            Shade(pictures, shade);
        foreach (ColourEffect effect in shading.Effects)
            Affect(pictures, effect);
    }

    /// <summary>Applies <paramref name="effect"/> to every sprite draw that names its shading, after its own effects.</summary>
    public void AffectSprites(ColourEffect effect) => sprites.Add(Ensure.NotNull(effect));

    /// <summary>
    /// The frame's shading, its backdrop the colour ink zero of <paramref name="roomColours"/>, the room's tileset picture,
    /// shows under the room's shading: the tileset's backdrop colour, with what the room's effects make of it.
    /// </summary>
    /// <param name="roomColours">A picture of the room's tileset, whose ink zero is the backdrop.</param>
    public GameplayShading Build(SpriteImage roomColours)
    {
        Ensure.NotNull(roomColours);
        RgbColor backdrop = shadings[GameplayPictures.Room].InkColours(roomColours)[0];
        return new GameplayShading(backdrop, shadings.ToImmutableDictionary(), [.. sprites]);
    }

    /// <summary>Checks that <paramref name="pictures"/> is a group of gameplay pictures.</summary>
    private static GameplayPictures Defined(GameplayPictures pictures) => Enum.IsDefined(pictures)
        ? pictures
        : throw new ArgumentOutOfRangeException(nameof(pictures), pictures, "Undefined group of gameplay pictures.");
}
