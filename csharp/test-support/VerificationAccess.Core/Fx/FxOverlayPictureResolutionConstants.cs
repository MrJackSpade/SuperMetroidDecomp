using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>Cartridge values and helpers of <see cref="FxOverlayPictureResolution"/> that only verification reads.</summary>
internal static class FxOverlayPictureResolutionConstants
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
        Ensure.NotNull(plane);
        return new FxOverlayArtwork(plane.Effect is { } type
            ? RoomEffectPicturesConstants.Artwork(FxOverlayPictureResolution.OverlaySheet(resolver), type, appearance) : null);
    }
}
