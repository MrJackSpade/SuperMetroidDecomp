using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A turning floor's picture (#1403, #1467, #1649, #2104): the floor's pre-coloured image, which the import composes once
/// as the whole floor, and the pictures of the floor's named states drawn over it, in order. A published affine transform
/// turns and scales it onto the screen. The floor is its image: its extent is the image's, and a floor that repeats beyond
/// its edges repeats with that period. Each picture drawn over it is placed by its frame's origin: drawn at the floor's
/// top-left corner, so its own top-left lands at the floor point the origin names, within the floor.
/// </summary>
public sealed class AffineFloor : IEquatable<AffineFloor>
{
    /// <summary>Holds a floor's image and the pictures drawn over it.</summary>
    /// <param name="image">The whole floor, placed at the floor's top-left corner.</param>
    /// <param name="drawnOver">The pictures of the floor's states drawn over it, in drawing order; each lies on the floor.</param>
    /// <exception cref="InvalidDataException">
    /// The image is not placed at the floor's top-left corner, or a picture leaves the floor.
    /// </exception>
    public AffineFloor(SpriteSheetFrame image, ImmutableArray<SpriteSheetFrame> drawnOver = default)
    {
        Image = Ensure.NotNull(image);
        if (image.Origin.X != 0 || image.Origin.Y != 0)
            throw new InvalidDataException(
                $"A floor's image is the whole floor, placed at its top-left corner, not at ({-image.Origin.X}, {-image.Origin.Y}).");
        DrawnOver = drawnOver.IsDefault ? [] : drawnOver;
        foreach (SpriteSheetFrame picture in DrawnOver)
            RequireOnFloor(picture);
        Pictures = [.. new[] { image }.Concat(DrawnOver).Distinct()];
    }

    /// <summary>The floor's image: the whole floor.</summary>
    public SpriteSheetFrame Image { get; }

    /// <summary>The floor's width and height: its image's.</summary>
    public Extent16<ImagePixels> Extent => Image.Size;

    /// <summary>The pictures of the floor's states drawn over the image, in drawing order.</summary>
    public ImmutableArray<SpriteSheetFrame> DrawnOver { get; }

    /// <summary>The image and every picture drawn over it, each once, the image first.</summary>
    public ImmutableArray<SpriteSheetFrame> Pictures { get; }

    /// <summary>
    /// The shading every picture of the floor shows (#1512), or null for a gameplay room's floor, which shows the room's
    /// shading: a frontend screen's floor names its shading here, <see cref="PictureShading.Standard"/> where the screen
    /// shows no effect on it, and a backend refuses a frontend floor that names none.
    /// </summary>
    public PictureShading? Shading { get; init; }

    /// <summary>This floor's pictures shown under <paramref name="shading"/>.</summary>
    public AffineFloor Shaded(PictureShading shading) => new(Image, DrawnOver) { Shading = Ensure.NotNull(shading) };

    /// <summary>Whether <paramref name="other"/> lays the same pictures, whatever each shows them in.</summary>
    public bool SamePictures(AffineFloor? other) =>
        other is not null && ReferenceEquals(Image, other.Image) && DrawnOver.AsSpan().SequenceEqual(other.DrawnOver.AsSpan(), ReferenceEqualityComparer.Instance);

    /// <inheritdoc />
    public bool Equals(AffineFloor? other) => other is { } floor && SamePictures(floor) && Equals(Shading, floor.Shading);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AffineFloor);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Image, DrawnOver.Length);

    /// <summary>Requires <paramref name="picture"/>, placed by its origin, to lie on the floor's image.</summary>
    /// <exception cref="InvalidDataException">The picture leaves the floor.</exception>
    private void RequireOnFloor(SpriteSheetFrame picture)
    {
        if (-picture.Origin.X < 0 || -picture.Origin.Y < 0 || -picture.Origin.X + picture.Size.X > Extent.X ||
            -picture.Origin.Y + picture.Size.Y > Extent.Y)
            throw new InvalidDataException(
                $"A {picture.Size.X}x{picture.Size.Y} picture placed at ({-picture.Origin.X}, {-picture.Origin.Y}) leaves the {Extent.X}x{Extent.Y} floor.");
    }
}
