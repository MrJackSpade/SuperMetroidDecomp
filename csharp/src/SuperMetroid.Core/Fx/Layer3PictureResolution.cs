using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the BG3 plane shows from the room's BG3 picture sheet (#1392, #1403, #1466).</summary>
internal static class Layer3PictureResolution
{
    /// <summary>
    /// The picture of each cell of the BG3 <paramref name="plane"/> while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as the frames the room effects' animations selected (#1406): the cleared
    /// picture everywhere, with the effect's page picture from its first row.
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the plane's pictures for no such values.</exception>
    internal static ImmutableArray<BgTilePicture?> Resolve(this WrittenPictureResolver resolver, Layer3Plane plane, RoomAppearanceView appearance)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != WrittenPictureLayer.Bg3)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no BG3 pictures.");
        var page = new List<WrittenPicture>();
        if (plane.EffectPage is { } type)
            page.Add(new WrittenPicture(Layer3Plane.EffectPagePicture(type), Layer3Plane.EffectPageFirstRow * Layer3Plane.Columns));
        return resolver.ResolvePlaced(plane, Layer3Plane.Columns * Layer3Plane.Rows, Layer3Plane.ClearedPicture, page, appearance);
    }
}
