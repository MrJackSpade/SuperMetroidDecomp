using System.Collections.Immutable;
using SuperMetroid.Core.Rooms.Artwork;
using SuperMetroid.Core.Rooms.Backgrounds;

namespace SuperMetroid.Core.Fx;

/// <summary>Resolves what the effect overlay plane shows from the room's effect overlay picture sheet (#1392, #1403, #1466).</summary>
internal static class FxOverlayPictureResolution
{
    /// <summary>
    /// The picture of each cell of the effect overlay <paramref name="plane"/> while the room's appearance inputs take the values
    /// <paramref name="appearance"/> reads, as the frames the room effects' animations selected (#1406): the cleared
    /// picture everywhere, with the effect's page picture from its first row.
    /// </summary>
    /// <exception cref="InvalidOperationException">The resolver's sheet pictures another layer's pictures.</exception>
    /// <exception cref="InvalidDataException">The sheet pictures the plane's pictures for no such values.</exception>
    internal static ImmutableArray<BgTilePicture?> Resolve(this WrittenPictureResolver resolver, FxOverlayPlane plane, RoomAppearanceView appearance)
    {
        Ensure.NotNull(resolver);
        Ensure.NotNull(plane);
        if (resolver.Sheet.Layer != WrittenPictureLayer.FxOverlayAndHud)
            throw new InvalidOperationException($"{resolver.Sheet.Name} pictures no effect overlay pictures.");
        var page = new List<WrittenPicture>();
        if (plane.EffectPage is { } type)
            page.Add(new WrittenPicture(FxOverlayPlane.EffectPagePicture(type), new Position16<TilemapCells>(0, FxOverlayPlane.EffectPageFirstRow)));
        return resolver.ResolvePlaced(plane, new Extent16<TilemapCells>(FxOverlayPlane.Columns, FxOverlayPlane.Rows), FxOverlayPlane.ClearedPicture, page, appearance);
    }
}
