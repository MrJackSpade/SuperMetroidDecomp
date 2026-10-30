using System.Collections.Immutable;

namespace SuperMetroid.Core.Rendering;

/// <summary>
/// What a picture layer shows (#1467): installed pictures in drawing order, a later picture covering an earlier one where
/// both show a pixel. Equal to another holding the same pictures at the same places in the same order, so an unchanged
/// screen keeps the backend's packing.
/// </summary>
public sealed class PlacedPictures : IEquatable<PlacedPictures>
{
    /// <summary>Keeps the pictures in drawing order.</summary>
    /// <param name="pictures">The placed pictures, earliest first.</param>
    /// <exception cref="ArgumentException">The pictures are default, or one is null.</exception>
    public PlacedPictures(ImmutableArray<PlacedPicture> pictures)
    {
        if (pictures.IsDefault)
            throw new ArgumentException("A picture layer needs its placements.", nameof(pictures));
        foreach (PlacedPicture placed in pictures)
            _ = Ensure.NotNull(placed.Picture);
        Pictures = pictures;
    }

    /// <summary>A layer showing nothing.</summary>
    public static PlacedPictures None { get; } = new([]);

    /// <summary>The placed pictures, earliest first.</summary>
    public ImmutableArray<PlacedPicture> Pictures { get; }

    /// <inheritdoc />
    public bool Equals(PlacedPictures? other)
    {
        if (other is null || other.Pictures.Length != Pictures.Length)
            return false;
        for (int index = 0; index < Pictures.Length; index++)
        {
            if (!ReferenceEquals(Pictures[index].Picture, other.Pictures[index].Picture) || Pictures[index].At != other.Pictures[index].At)
                return false;
        }
        return true;
    }

    /// <inheritdoc />
    public override bool Equals(object? obj) => Equals(obj as PlacedPictures);

    /// <inheritdoc />
    public override int GetHashCode()
    {
        var hash = new HashCode();
        foreach (PlacedPicture placed in Pictures)
        {
            hash.Add(System.Runtime.CompilerServices.RuntimeHelpers.GetHashCode(placed.Picture));
            hash.Add(placed.At);
        }
        return hash.ToHashCode();
    }
}
