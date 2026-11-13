using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the effect overlay shows from the room's effect overlay picture sheet (#1392, #1403, #1466, #1514, #1706).</summary>
internal static class FxOverlayPictureResolution
{
    /// <summary>
    /// The artwork the effect overlay <paramref name="plane"/> shows while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as the frames the room effects' animations selected (#1406): the effect's
    /// liquid or atmosphere artwork, or nothing for an effect that shows none.
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the artwork for no such values.</exception>
    internal static FxOverlayArtwork Resolve(this WrittenPictureResolver resolver, FxOverlayPlane plane, RoomAppearanceView appearance)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != WrittenPictureLayer.FxOverlay)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no effect overlay pictures.");
        return new FxOverlayArtwork(plane.Effect is { } type
            ? RoomEffectPictures.Artwork(resolver.Sheet, type, appearance) : null);
    }
}
