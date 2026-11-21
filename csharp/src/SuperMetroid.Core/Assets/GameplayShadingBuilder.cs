using System.Collections.Immutable;

namespace SuperMetroid.Core.Assets;

/// <summary>
/// Gathers a gameplay frame's shading as it is published (#1512): each owner of a colour effect lays the shades its
/// typed state names and the effects it applies over the groups of pictures it colours, in the order the publication
/// asks them, and a door or the death sequence fades the whole display. Later shades win where they recolour the same
/// inks.
/// </summary>
public sealed class GameplayShadingBuilder
{
    /// <summary>Each group's shading so far.</summary>
    private readonly Dictionary<GameplayPictures, PictureShading> shadings =
        Enum.GetValues<GameplayPictures>().ToDictionary(pictures => pictures, _ => PictureShading.Standard);

    /// <summary>The fade of the whole display, or null while none runs.</summary>
    private GameplayDisplayFade? fade;

    /// <summary>Lays <paramref name="shade"/> over <paramref name="pictures"/>' shades.</summary>
    public void Shade(GameplayPictures pictures, PictureShade shade) => shadings[pictures] = shadings[pictures].With(shade);

    /// <summary>Shows <paramref name="fade"/> over <paramref name="pictures"/>' shades, after their fades (#2215).</summary>
    public void Shade(GameplayPictures pictures, PictureShadeFade fade) =>
        shadings[pictures] = shadings[pictures].With(Ensure.NotNull(fade));

    /// <summary>Applies <paramref name="effect"/> to <paramref name="pictures"/> after their effects.</summary>
    public void Affect(GameplayPictures pictures, ColourEffect effect) =>
        shadings[pictures] = shadings[pictures].With(Ensure.NotNull(effect));

    /// <summary>Fades the whole display with <paramref name="displayFade"/>: one fade runs at a time.</summary>
    /// <exception cref="InvalidOperationException">Another fade of the display already runs.</exception>
    public void Fade(GameplayDisplayFade displayFade)
    {
        Ensure.NotNull(displayFade);
        fade = fade is null ? displayFade
            : throw new InvalidOperationException("A gameplay frame shows one fade of its display at a time.");
    }

    /// <summary>
    /// The frame's shading, its backdrop the colour <paramref name="roomColours"/>, the room's tileset picture, names as its
    /// scene's backdrop (#1544), shown under the room's shading and the display's fade: the tileset's backdrop colour, with
    /// what the room's effects make of it.
    /// </summary>
    /// <param name="roomColours">A picture of the room's tileset.</param>
    /// <param name="backdrop">The backdrop colour the picture names for its scene, which its room sheet requires (#2554).</param>
    public GameplayShading Build(SpriteImage roomColours, ColourKey backdrop)
    {
        Ensure.NotNull(roomColours);
        var built = new GameplayShading(RgbColor.Black, shadings.ToImmutableDictionary(), fade);
        return built.WithBackdrop(new ShownColour.OfPicture(roomColours, backdrop, built.Of(GameplayPictures.Room)));
    }

}
