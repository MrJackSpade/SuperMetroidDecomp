using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Gathers a gameplay frame's shading as it is published (#1512): each owner of a colour effect lays the shades its
/// typed state names and the effects it applies over the groups of pictures it colours, in the order the publication
/// asks them, and a door or the death sequence fades the whole display. Later shades win where they recolour the same
/// inks, as the later of two colour writes did.
/// </summary>
public sealed class GameplayShadingBuilder
{
    /// <summary>Each group's shading so far.</summary>
    private readonly Dictionary<GameplayPictures, PictureShading> shadings =
        Enum.GetValues<GameplayPictures>().ToDictionary(pictures => pictures, _ => PictureShading.Standard);

    /// <summary>The fade of the whole display, or null while none runs.</summary>
    private GameplayDisplayFade? fade;

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

    /// <summary>Fades the whole display with <paramref name="displayFade"/>: one fade runs at a time.</summary>
    /// <exception cref="InvalidOperationException">Another fade of the display already runs.</exception>
    public void Fade(GameplayDisplayFade displayFade)
    {
        Ensure.NotNull(displayFade);
        fade = fade is null ? displayFade
            : throw new InvalidOperationException("A gameplay frame shows one fade of its display at a time.");
    }

    /// <summary>
    /// The frame's shading, its backdrop the colour ink zero of <paramref name="roomColours"/>, the room's tileset picture,
    /// shows under the room's shading and the display's fade: the tileset's backdrop colour, with what the room's effects
    /// make of it.
    /// </summary>
    /// <param name="roomColours">A picture of the room's tileset, whose ink zero is the backdrop.</param>
    public GameplayShading Build(SpriteImage roomColours)
    {
        Ensure.NotNull(roomColours);
        var built = new GameplayShading(RgbColor.Black, shadings.ToImmutableDictionary(), fade);
        return built.WithBackdrop(built.Of(GameplayPictures.Room).InkColours(roomColours)[0]);
    }

    /// <summary>Checks that <paramref name="pictures"/> is a group of gameplay pictures.</summary>
    private static GameplayPictures Defined(GameplayPictures pictures) => Enum.IsDefined(pictures)
        ? pictures
        : throw new ArgumentOutOfRangeException(nameof(pictures), pictures, "Undefined group of gameplay pictures.");
}
