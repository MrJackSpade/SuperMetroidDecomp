using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the BG3 plane's words show from the room's BG3 word sheet (#1392, #1403).</summary>
internal static class Layer3PictureResolution
{
    /// <summary>
    /// The picture of each word of the BG3 <paramref name="plane"/> over what <paramref name="bindings"/> show now at its
    /// characters from <paramref name="characterBase"/>, the HUD character base BG3 shows.
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's words.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures a word over nothing its character shows.</exception>
    internal static ImmutableArray<BgTilePicture?> Resolve(this TileWordPictureResolver resolver, Layer3Plane plane,
        BgArtworkBindings bindings, int characterBase)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != TileWordLayer.Bg3)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no BG3 words.");
        return resolver.ResolveWords(plane, Layer3Plane.Columns * Layer3Plane.Rows, index => plane.Words[index], bindings, characterBase);
    }
}
