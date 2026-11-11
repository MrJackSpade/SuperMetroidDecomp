using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rendering;

/// <summary>An installed picture placed on a pictured plane (#1467, #1555).</summary>
/// <param name="Picture">
/// The picture, pre-coloured under the palette its screen shows it in, as its part drawn behind and its part drawn in front
/// (#1634), drawn whole at its plane's place: a plane's pass draws its in-front part over its behind part.
/// </param>
/// <param name="Corner">The plane pixel the picture's upper-left corner sits on, which may lie left of or above the plane's origin.</param>
/// <param name="Shading">
/// The shades and colour effects its screen shows it in (#1512); <see cref="PictureShading.Standard"/> where the screen
/// shows no effect on it.
/// </param>
public readonly record struct PlacedPlanePicture(LayeredPicture Picture, Offset16<PlanePixels> Corner, PictureShading Shading)
{
    /// <summary>
    /// <paramref name="picture"/> placed with the point its frames name as their origin on plane pixel
    /// <paramref name="origin"/>, shown in <paramref name="shading"/>: a picture whose origin lies inside it reaches left of
    /// or above that pixel.
    /// </summary>
    /// <exception cref="InvalidDataException">The picture shows nothing, or its parts name different origins.</exception>
    public static PlacedPlanePicture WithOriginAt(LayeredPicture picture, Position16<PlanePixels> origin, PictureShading shading)
    {
        Ensure.NotNull(picture);
        Ensure.NotNull(shading);
        Offset16<ImagePixels> from = OriginOf(picture);
        return new(picture, Offset16<PlanePixels>.Of(origin.X - from.X, origin.Y - from.Y), shading);
    }

    /// <summary>The point <paramref name="picture"/>'s parts name as their origin, from their top-left pixel.</summary>
    /// <exception cref="InvalidDataException">The picture shows nothing, or its parts name different origins.</exception>
    private static Offset16<ImagePixels> OriginOf(LayeredPicture picture) => (picture.Behind, picture.InFront) switch
    {
        (null, null) => throw new InvalidDataException("A picture that shows nothing has no origin to place."),
        ({ } behind, null) => behind.Origin,
        (null, { } inFront) => inFront.Origin,
        ({ } behind, { } inFront) => behind.Origin == inFront.Origin ? behind.Origin
            : throw new InvalidDataException($"A picture's behind part names origin {behind.Origin} and its in-front part {inFront.Origin}."),
    };
}
