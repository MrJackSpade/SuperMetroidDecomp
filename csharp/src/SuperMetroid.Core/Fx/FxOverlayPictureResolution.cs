using SuperMetroid.Core.Fx.Artwork;
using SuperMetroid.Core.Rooms.Artwork;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the effect overlay shows from the room's effect overlay picture sheet (#1392, #1403, #1466, #1514, #1706).</summary>
internal static class FxOverlayPictureResolution
{

    /// <summary>
    /// The surface strip and body liquid effect <paramref name="type"/> shows while the room's appearance inputs take the
    /// values <paramref name="appearance"/> reads (#2505).
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the artwork for no such values.</exception>
    internal static LiquidArtwork Liquid(this WrittenPictureResolver resolver, RoomFxType type, RoomAppearanceView appearance) =>
        RoomEffectPictures.Liquid(OverlaySheet(resolver), type, appearance);

    /// <summary>
    /// The repeating picture atmosphere effect <paramref name="type"/> shows while the room's appearance inputs take the
    /// values <paramref name="appearance"/> reads (#2505).
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the artwork for no such values.</exception>
    internal static AtmosphereArtwork Atmosphere(this WrittenPictureResolver resolver, RoomFxType type, RoomAppearanceView appearance) =>
        RoomEffectPictures.Atmosphere(OverlaySheet(resolver), type, appearance);

    /// <summary>The resolver's sheet, which pictures the effect overlay.</summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    internal static WrittenPictureSheet OverlaySheet(WrittenPictureResolver resolver)
    {
        Ensure.NotNull(resolver);
        return resolver.Sheet.Layer == WrittenPictureLayer.FxOverlay ? resolver.Sheet
            : throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no effect overlay pictures.");
    }
}
