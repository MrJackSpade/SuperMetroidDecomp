using System.Collections.Immutable;
using SuperMetroid.Core.Assets;

namespace SuperMetroid.Core.Rooms.Backgrounds;

/// <summary>
/// A turning floor's picture (#1403, #1467): the whole floor's pre-coloured image, which the import composes once, and
/// the pictures of the floor's named states drawn over it, in order. A published affine transform turns and scales it
/// onto the screen. Each picture drawn over the floor is placed by its frame's origin: drawn at the floor's top-left
/// corner, so its own top-left lands at the floor point its origin names.
/// </summary>
public sealed class AffineFloor : IEquatable<AffineFloor>
{
    /// <summary>Pixels along each side of the floor.</summary>
    public const int SidePixels = 1024;

    /// <summary>Holds a floor's image and the pictures drawn over it.</summary>
    /// <param name="image">The whole floor, <see cref="SidePixels"/> square, coloured from the full palette.</param>
    /// <param name="drawnOver">The pictures of the floor's states drawn over it, in drawing order; each lies on the floor.</param>
    /// <exception cref="InvalidDataException">The image is not the whole floor or not coloured from the full palette, or a picture leaves the floor.</exception>
    public AffineFloor(SpriteSheetFrame image, ImmutableArray<SpriteSheetFrame> drawnOver = default)
    {
        Image = Ensure.NotNull(image);
        if (image.Size.X != SidePixels || image.Size.Y != SidePixels)
            throw new InvalidDataException($"A floor image is {image.Size.X}x{image.Size.Y}, not {SidePixels} square.");
        DrawnOver = drawnOver.IsDefault ? [] : drawnOver;
        foreach (SpriteSheetFrame picture in DrawnOver)
        {
            if (-picture.Origin.X < 0 || -picture.Origin.Y < 0 || -picture.Origin.X + picture.Size.X > SidePixels ||
                -picture.Origin.Y + picture.Size.Y > SidePixels)
                throw new InvalidDataException($"A {picture.Size.X}x{picture.Size.Y} picture placed at ({-picture.Origin.X}, {-picture.Origin.Y}) leaves the floor.");
        }
        Pictures = [.. new[] { image }.Concat(DrawnOver).Distinct()];
        foreach (SpriteSheetFrame picture in Pictures)
        {
            if (picture.Sheet.Colours != SpriteImageColours.FullPalette)
                throw new InvalidDataException("A floor's pictures are coloured from the full palette.");
        }
    }

    /// <summary>The whole floor's image.</summary>
    public SpriteSheetFrame Image { get; }

    /// <summary>The pictures of the floor's states drawn over the image, in drawing order.</summary>
    public ImmutableArray<SpriteSheetFrame> DrawnOver { get; }

    /// <summary>The image and every picture drawn over it, each once, the image first.</summary>
    public ImmutableArray<SpriteSheetFrame> Pictures { get; }

    /// <inheritdoc />
    public bool Equals(AffineFloor? other) =>
        other is not null && ReferenceEquals(Image, other.Image) && DrawnOver.AsSpan().SequenceEqual(other.DrawnOver.AsSpan(), ReferenceEqualityComparer.Instance);

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as AffineFloor);

    /// <inheritdoc />
    public override int GetHashCode() => HashCode.Combine(Image, DrawnOver.Length);
}
