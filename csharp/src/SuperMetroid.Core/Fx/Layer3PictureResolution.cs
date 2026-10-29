using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the BG3 plane's words show from the room's BG3 word sheet (#1392, #1403, #1466).</summary>
internal static class Layer3PictureResolution
{
    /// <summary>
    /// The picture of each word of the BG3 <paramref name="plane"/> while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as the frames the room effects' animations selected (#1406).
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's words.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures a word for no such values.</exception>
    internal static ImmutableArray<BgTilePicture?> Resolve(this TileWordPictureResolver resolver, Layer3Plane plane, RoomAppearanceView appearance)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != TileWordLayer.Bg3)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no BG3 words.");
        return resolver.ResolveWords(plane, Layer3Plane.Columns * Layer3Plane.Rows, index => plane.Tiles[index], appearance);
    }
}
