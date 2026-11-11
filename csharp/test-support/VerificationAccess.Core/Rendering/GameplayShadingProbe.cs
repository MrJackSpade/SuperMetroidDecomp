using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// Reads what a gameplay colour owner lays over the gameplay pictures (#1512): the shading its
/// <see cref="IGameplayShadingSource.ShadeGameplay"/> gives each group, built over a one-pixel room picture whose one
/// colour, its backdrop, stays black under every shade the room can be given.
/// </summary>
internal static class GameplayShadingProbe
{
    /// <summary>The shading <paramref name="source"/> gives <paramref name="pictures"/> this frame.</summary>
    /// <param name="source">The colour owner.</param>
    /// <param name="pictures">The group of pictures to read.</param>
    /// <param name="roomShades">Every shade the owner can lay over the room's pictures.</param>
    internal static PictureShading ShadingOf(IGameplayShadingSource source, GameplayPictures pictures, IEnumerable<PictureShade> roomShades)
    {
        ArgumentNullException.ThrowIfNull(source);
        ArgumentNullException.ThrowIfNull(roomShades);
        var frame = new GameplayShadingBuilder();
        source.ShadeGameplay(frame);
        var colours = new PictureColours([RgbColor.Black])
        {
            Backdrop = 0,
            Shades = roomShades.Distinct().ToImmutableDictionary(shade => shade, _ => new ShadeInks([], [])),
        };
        return frame.Build(SpriteImageFixtures.Copied("Room", new(1, 1), [default], [SpriteImageInk.Transparent], colours))
            .Of(pictures);
    }
}
