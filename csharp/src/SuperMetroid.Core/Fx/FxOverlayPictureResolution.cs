using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the effect overlay shows from the room's effect overlay picture sheet (#1392, #1403, #1466, #1514).</summary>
internal static class FxOverlayPictureResolution
{
    /// <summary>
    /// The page the effect overlay <paramref name="plane"/> shows while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as the frames the room effects' animations selected (#1406): the effect's page
    /// picture, or nothing without a page.
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the page for no such values.</exception>
    internal static FxOverlayPage Resolve(this WrittenPictureResolver resolver, FxOverlayPlane plane, RoomAppearanceView appearance)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != WrittenPictureLayer.FxOverlayAndHud)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no effect overlay pictures.");
        return new FxOverlayPage(plane.EffectPage is { } type
            ? resolver.Sheet.Picture(FxOverlayPlane.EffectPagePicture(type), appearance) : null);
    }
}
